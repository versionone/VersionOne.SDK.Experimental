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
    public class TranslateJsonInputToAssetXml : ITranslateApiInputToAssetXml
    {
        public XPathDocument Execute(string input)
        {
            try
            {
                var jsonObject = (JObject)JsonConvert.DeserializeObject(input);
                var buffer = new StringBuilder();

                foreach (var item in jsonObject.Root)
                {
                    if (new [] {JTokenType.String, JTokenType.Boolean}.Any(x => x.Equals(item.First.Type)))
                    {
                        AddAssetAttributeFromJValueScalar(item, buffer);
                    }
                    else if (item.First.Type == JTokenType.Array)
                    {
                        AddAttributeFromJArray(item, buffer);
                    }
                }

                return CreateUpdateAssetXmlFragment(buffer);
            }
            catch
            {
                var jsonObject = (JArray)JsonConvert.DeserializeObject(input);
                var buffer = new StringBuilder();

                var props = jsonObject.Root[0];
                foreach (var item in props)
                {
                    if (new[] { JTokenType.String, JTokenType.Boolean }.Any(x => x.Equals(item.First.Type)))
                    {
                        AddAssetAttributeFromJValueScalar(item, buffer);
                    }
                    else if (item.First.Type == JTokenType.Array)
                    {
                        AddAttributeFromJArray(item, buffer);
                    }
                }
                
                if (jsonObject.Count() > 1)
                {
                    var relations = jsonObject.Root[1];
                    foreach (var relation in relations)
                    {
                        JProperty relationship = (JProperty)relation.First;
                        var name = relationship.Name;
                        var value = relationship.Value;
                        AddRelation(buffer, name, value);
                    }
                }

                return CreateUpdateAssetXmlFragment(buffer);
            }
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

        private static string CreateAssetAttributeForUpdateOrAdd(IList<string> attributeDef)
        {
            const string xmlAttribute = /* one space */ " <Attribute name='{0}' act='{1}'>{2}</Attribute>\n";
            var attribute = string.Format(xmlAttribute, attributeDef[0], attributeDef[1], attributeDef[2]);
            return attribute;
        }

        private static string CreateAssetAttributeForRemove(IList<string> attributeDef)
        {
            const string xmlAttribute = /* one space */ " <Attribute name='{0}' act='{1}' />\n";
            var attribute = string.Format(xmlAttribute, attributeDef[0], attributeDef[1]);
            return attribute;
        }

        private static void AddAssetAttributeFromJValueScalar(JToken item, StringBuilder buffer)
        {
            var property = item as JProperty;
            var name = property.Name;
            var value = property.Value.ToString();
            buffer.Append(CreateAssetAttributeForUpdateOrAdd(new[] { name, "set", value }));
        }

        private static void AddAttributeFromJArray(JToken item, StringBuilder buffer)
        {
            var name = (item as JProperty).Name;
            var array = item.First as JArray;

            string attribute = string.Empty;

            var act = array[0].Value<string>();
            if (new[] { "set", "add" }.Any(a => a.Equals(act, StringComparison.OrdinalIgnoreCase)))
            {
                var value = array[1].Value<string>();
                attribute = CreateAssetAttributeForUpdateOrAdd(new[] { name, act, value });
            }
            else if (act.Equals("remove", StringComparison.OrdinalIgnoreCase))
            {
                attribute = CreateAssetAttributeForRemove(new[] { name, act });
            }

            buffer.Append(attribute);
        }

        private static readonly string[] ContentTypes = new[]
            {
                "json",
                "text/json",
                "application/json"
            };

        public bool CanHandle(string contentType)
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