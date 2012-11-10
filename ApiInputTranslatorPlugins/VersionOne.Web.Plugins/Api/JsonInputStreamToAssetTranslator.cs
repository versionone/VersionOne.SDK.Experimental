using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VersionOne.Web.Plugins.Api
{
    [Export(typeof(IApiInputStreamTranslator))]
    public class JsonInputStreamToAssetXmlTranslator : IApiInputStreamTranslator
    {
        private string _inputData;
        private NameValueCollection _queryString;

        [ImportingConstructor]
        public JsonInputStreamToAssetXmlTranslator()
        {
        }

        public JsonInputStreamToAssetXmlTranslator(string inputData, NameValueCollection queryString)
        {
           Initialize(inputData, queryString);
        }

        public XPathDocument Execute()
        {
            XPathDocument doc = null;

            if (IsInputStreamJson())
            {
                var jsonObject = (JObject)JsonConvert.DeserializeObject(_inputData);
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

            return doc;
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

        private bool IsInputStreamJson()
        {
            var format = _queryString["fmt"] ?? _queryString["format"];
            return format != null && format.Equals("json", StringComparison.OrdinalIgnoreCase);
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
                attribute = CreateAssetAttributeForUpdateOrAdd(new string[] { name, act, value });
            }
            else if (act.Equals("remove", StringComparison.OrdinalIgnoreCase))
            {
                attribute = CreateAssetAttributeForRemove(new string[] { name, act });
            }
            
            buffer.Append(attribute);
        }
        
        public void Initialize(string inputData, NameValueCollection queryString)
        {
            _inputData = inputData;
            _queryString = queryString;
        }
    }
}