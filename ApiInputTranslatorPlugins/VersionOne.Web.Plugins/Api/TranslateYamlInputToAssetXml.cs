using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml.XPath;
using YamlDotNet.RepresentationModel;

namespace VersionOne.Web.Plugins.Api
{
    [Export(typeof(ITranslateApiInputToAssetXml))]
    public class TranslateYamlInputToAssetXml : ITranslateApiInputToAssetXml
    {
        private readonly XmlAssetBuilder _builder = new XmlAssetBuilder();
           
        public XPathDocument Execute(string input)
        {
            var yamlDocument = new StringReader(input);

            var yaml = new YamlStream();
            yaml.Load(yamlDocument);

            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

            foreach (var entry in mapping.Children)
            {
                var name = entry.Key.ToString();

                if (entry.Value is YamlMappingNode)
                {
                    AddRelations(name, entry);
                }
                else if (entry.Value is YamlSequenceNode)
                {
                    AddAttributesWithExplicitActions(entry, name);
                }
                else if (entry.Value is YamlScalarNode)
                {
                    var value = entry.Value;
                    _builder.AddAssetAttributeFromScalar(name, value);
                }
            }

            return _builder.GetAssetXml();
        }

        private void AddRelations(string name, KeyValuePair<YamlNode, YamlNode> entry)
        {
            if (name.Equals("_links", StringComparison.OrdinalIgnoreCase))
            {
                var relationList = new RelationList();
                var mappingNode = (entry.Value as YamlMappingNode);
                foreach (var link in mappingNode)
                {
                    var relation = new Relation(link.Key.ToString());
                    var rels = GetRelationItems(link);

                    foreach (var item in rels)
                    {
                        var relationItems = item as YamlMappingNode;
                        var relationAttributes = new List<Attribute>();
                        foreach (var relItem in relationItems.Children)
                        {
                            var attr = new Attribute(relItem.Key.ToString(), relItem.Value);
                            relationAttributes.Add(attr);
                        }
                        relation.Add(relationAttributes);
                    }
                    relationList.Add(relation);
                }
                _builder.AddRelationsFromRelationList(relationList);
            }
        }

        private void AddAttributesWithExplicitActions(KeyValuePair<YamlNode, YamlNode> entry, string name)
        {
            var sequence = (entry.Value as YamlSequenceNode);
            var array = sequence.Children.Cast<object>().ToArray();

            var act = array[0].ToString();
            if (new[] { "set", "add" }.Any(a => a.Equals(act, StringComparison.OrdinalIgnoreCase)))
            {
                var value = array[1];
                var attr = new Attribute(name, value, act);
                _builder.AddAttributeFromArray(attr);
            }
            else if (act.Equals("remove", StringComparison.OrdinalIgnoreCase))
            {
                var attr = Attribute.CreateForRemove(name);
                _builder.AddAttributeFromArray(attr);
            }
        }

        private static IEnumerable<object> GetRelationItems(KeyValuePair<YamlNode, YamlNode> link)
        {
            var rels = new List<object>();

            if (link.Value is YamlMappingNode)
            {
                var value = (link.Value as YamlMappingNode);
                rels = value.Children.Cast<object>().ToList();
            }
            else if (link.Value is YamlSequenceNode)
            {
                var value = (link.Value as YamlSequenceNode);
                rels = value.Children.Cast<object>().ToList();
            }
            return rels;
        }

        private readonly string[] _contentTypes = new[]
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
                return _contentTypes.Any(c => c.Equals(contentType, StringComparison.OrdinalIgnoreCase));
            }

            return false;
        }

    }
}