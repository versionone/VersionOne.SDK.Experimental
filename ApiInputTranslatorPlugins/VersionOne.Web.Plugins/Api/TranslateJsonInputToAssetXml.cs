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
            var jsonObject = (JObject)JsonConvert.DeserializeObject(input);
            var buffer = new StringBuilder();

            foreach (var item in jsonObject.Root)
            {
                if (item.First.Type == JTokenType.String)
                {
                    AddAssetAttributeFromJValueString(item, buffer);
                }
                else if (item.First.Type == JTokenType.Array)
                {
                    AddAttributeFromJArray(item, buffer);
                }
            }

            return CreateUpdateAssetXmlFragment(buffer);
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

        private static void AddAssetAttributeFromJValueString(JToken item, StringBuilder buffer)
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
            if (new []{"set", "add"}.Any(a => a.Equals(act, StringComparison.OrdinalIgnoreCase)))
            {
                var value = array[1].Value<string>();
                attribute = CreateAssetAttributeForUpdateOrAdd(new [] { name, act, value });
            }
            else if (act.Equals("remove", StringComparison.OrdinalIgnoreCase))
            {
                attribute = CreateAssetAttributeForRemove(new [] { name, act });
            }
            
            buffer.Append(attribute);
        }

        private static readonly string[] ContentTypes = new []
            {
                "json",
                "text/json",
                "application/json"
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