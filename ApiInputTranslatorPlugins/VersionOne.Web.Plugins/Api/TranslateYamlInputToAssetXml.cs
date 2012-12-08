using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using YamlDotNet.RepresentationModel;

namespace VersionOne.Web.Plugins.Api
{
    [Export(typeof(ITranslateApiInputToAssetXml))]
    public class TranslateYamlInputToAssetXml : ITranslateApiInputToAssetXml
    {
        public XPathDocument Execute(string input)
        {
            var buffer = new StringBuilder();
            var yamlDocument = new StringReader(input);

            var yaml = new YamlStream();
            yaml.Load(yamlDocument);

            // Examine the stream
            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

            foreach (var entry in mapping.Children)
            {
                var name = entry.Key.ToString();

                if (entry.Value is YamlMappingNode)
                {
                    if (name.Equals("_links", StringComparison.OrdinalIgnoreCase))
                    {
                        AddRelationsFromLinks((entry.Value as YamlMappingNode), buffer);
                    }
                }
                else if (entry.Value is YamlSequenceNode)
                {
                    var array = (entry.Value as YamlSequenceNode);
                    var items = array.Children.Cast<object>().ToArray();
                    AddAttributeFromArray(name, items, buffer);
                }
                else if (entry.Value is YamlScalarNode)
                {
                    var value = entry.Value;
                    AddAssetAttributeFromScalar(name, value, buffer);
                }
            }

            return CreateUpdateAssetXmlFragment(buffer);
        }

        private static void AddRelationsFromLinks(YamlMappingNode links, StringBuilder buffer)
        {
            foreach (var link in links.Children)
            {
                if (link.Value is YamlSequenceNode)
                {
                    var refItems = new List<string>();
                    var items = (link.Value as YamlSequenceNode).Children;
                    foreach (var item in items)
                    {
                        var relation = item as YamlMappingNode;

                        CreateRelationItemsFromToken(relation, refItems);
                    }
                    var key = link.Key.ToString();
                    AddRelation(buffer, key, refItems.ToArray());
                }
                else if (link.Value is YamlMappingNode)
                {
                    var items = new List<string>();
                    var relation = link.Value as YamlMappingNode;
                    CreateRelationItemsFromToken(relation, items);
                    AddRelation(buffer, link.Key.ToString(), items.ToArray());
                }
            }
        }

        private static void CreateRelationItemsFromToken(YamlMappingNode relation,
            List<string> items)
        {
            var idref = string.Empty;
            foreach (var item in relation.Children)
            {
                //if (prop.Name.Equals("href", StringComparison.OrdinalIgnoreCase))
                if (item.Key.ToString().Equals("idref", StringComparison.OrdinalIgnoreCase))
                {
                    idref = item.Value.ToString();
                    items.Add(idref);
                }
            }
        }

        private static void AddRelation(StringBuilder buffer, object relationName, string[] relationValues)
        {
            const string relationTemplate =
@" <Relation name=""{0}"" act=""set"">
  {1}
 </Relation>
";
            const string relationItem =
@"
  <Asset idref=""{0}"" />
";
            var buff = new StringBuilder();
            foreach (var item in relationValues)
            {
                buff.Append(string.Format(relationItem, item));
            }
            var relation = string.Format(relationTemplate, relationName, buff.ToString());

            buffer.Append(relation);
        }

        private static void AddAssetAttributeFromScalar(string name, object value, StringBuilder buffer)
        {
            buffer.Append(CreateAssetAttributeForUpdateOrAdd(new[] { name, "set", value.ToString() }));
        }

        private static void AddAttributeFromArray(string name, object[] array, StringBuilder buffer)
        {
            var attribute = string.Empty;
            var act = array[0].ToString();

            if (new[] { "set", "add" }.Any(a => a.Equals(act, StringComparison.OrdinalIgnoreCase)))
            {
                var value = array[1];
                attribute = CreateAssetAttributeForUpdateOrAdd(new[] { name, act, value });
            }
            else if (act.Equals("remove", StringComparison.OrdinalIgnoreCase))
            {
                attribute = CreateAssetAttributeForRemove(new[] { name, act });
            }
            buffer.Append(attribute);
        }

        private static XPathDocument CreateUpdateAssetXmlFragment(StringBuilder buffer)
        {
            const string xmlAsset = "<Asset>\n{0}</Asset>";

            var xml = string.Format(xmlAsset, buffer);

            using (var stringReader = new StringReader(xml))
            {
                var doc = new XPathDocument(stringReader);
                return doc;
            }
        }

        private static string CreateAssetAttributeForUpdateOrAdd(IList<object> attributeDef)
        {
            const string xmlAttribute = /* one space */ " <Attribute name='{0}' act='{1}'>{2}</Attribute>\n";
            var attribute = string.Format(xmlAttribute, attributeDef[0], attributeDef[1], attributeDef[2]);
            return attribute;
        }

        private static string CreateAssetAttributeForRemove(IList<object> attributeDef)
        {
            const string xmlAttribute = /* one space */ " <Attribute name='{0}' act='{1}' />\n";
            var attribute = string.Format(xmlAttribute, attributeDef[0], attributeDef[1]);
            return attribute;
        }

        private static readonly string[] ContentTypes = new[]
            {
                "yaml",
                "application/yaml",
                "text/yaml"
            };

        public bool CanTranslate(string contentType)
        {
            if (!string.IsNullOrWhiteSpace(contentType))
            {
                contentType = contentType.Trim();
                return ContentTypes.Any(c => c.Equals(contentType, StringComparison.OrdinalIgnoreCase));
            }

            return false;
        }
    }
}