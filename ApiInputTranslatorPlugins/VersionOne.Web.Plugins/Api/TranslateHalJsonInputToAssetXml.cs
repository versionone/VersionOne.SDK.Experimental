using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VersionOne.Web.Plugins.Api
{
    [Export(typeof(ITranslateApiInputToAssetXml))]
    public class TranslateHalJsonInputToAssetXml : ITranslateApiInputToAssetXml
    {
        public XPathDocument Execute(string input)
        {
            var jsonObject = (JObject)JsonConvert.DeserializeObject(input);
            var buffer = new StringBuilder();

            foreach (var item in jsonObject.Properties())
            {
                var name = item.Name;

                if (new[] { JTokenType.Object, JTokenType.Array }.Any(x => x.Equals(item.Value.Type)))
                {
                    if (item.Value.Type == JTokenType.Object 
                        && name.Equals("_links", StringComparison.OrdinalIgnoreCase))
                    {
                        var obj = (item.Value as JObject);
                        AddRelationsFromLinks(obj, buffer);
                    }
                    else
                    {
                        var array = (item.Value as JArray);
                        AddAttributeFromArray(name, array, buffer);
                    }
                }
                else
                {
                    AddAssetAttributeFromJValueScalar(item, buffer);
                }
            }

            return CreateUpdateAssetXmlFragment(buffer);
        }

        private static void AddRelationsFromLinks(JObject links, StringBuilder buffer)
        {
            foreach (var link in links.Properties())
            {
                if (link.Value.Type == JTokenType.Array)
                {
                    var array = link.Value<JArray>();
                    foreach (var item in array)
                    {
                        var relation = item.Value<JObject>();
                        CreateRelationFromToken(link.Name, relation, buffer);
                    }
                }
                else if (link.Value.Type == JTokenType.Object)
                {
                    var relation = (link.Value as JObject);
                    CreateRelationFromToken(link.Name, relation, buffer);
                }
            }
        }

        private static void CreateRelationFromToken(string name, JObject relation, StringBuilder buffer)
        {
            //var href = string.Empty;
            var idref = string.Empty;
            foreach (var prop in relation.Properties())
            {
                //if (prop.Name.Equals("href", StringComparison.OrdinalIgnoreCase))
                //{
                //    href = prop.Value<string>();
                //}
                if (prop.Name.Equals("idref", StringComparison.OrdinalIgnoreCase))
                {
                    idref = prop.Value.ToString();
                }
            }

            AddRelation(buffer, name, idref);
        }

        private static void AddRelation(StringBuilder buffer, object relationName, object relationValue)
        {
            const string relationTemplate =
@" <Relation name=""{0}"" act=""set"">
  <Asset idref=""{1}"" />
 </Relation>
";
            var relation = string.Format(relationTemplate, relationName, relationValue);

            buffer.Append(relation);
        }

        private static void AddAssetAttributeFromJValueScalar(JProperty item, StringBuilder buffer)
        {
            var name = item.Name;
            var value = item.Value.ToString();
            buffer.Append(CreateAssetAttributeForUpdateOrAdd(new[] { name, "set", value }));
        }

        private static void AddAttributeFromArray(string name, JArray array, StringBuilder buffer)
        {
            var attribute = string.Empty;
            var act = array[0].Value<string>();

            if (new[] { "set", "add" }.Any(a => a.Equals(act, StringComparison.OrdinalIgnoreCase)))
            {
                var value = array[1].Value<object>();
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
                "hal+json",
                "text/hal+json",
                "application/hal+json"
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