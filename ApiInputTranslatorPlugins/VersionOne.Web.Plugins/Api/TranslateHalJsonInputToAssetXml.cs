using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.XPath;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VersionOne.Web.Plugins.Api
{
    [Export(typeof(ITranslateApiInputToAssetXml))]
    public class TranslateHalJsonInputToAssetXml : 
        BaseTranslateApiInputToAssetXml, ITranslateApiInputToAssetXml
    {
        public XPathDocument Execute(string input)
        {
            var jsonObject = (JObject) JsonConvert.DeserializeObject(input);

            foreach (var prop in jsonObject.Properties())
            {
                var name = prop.Name;

                if (prop.Value.Type == JTokenType.Object)
                {
                    AddRelations(name, prop.Value);
                }
                else if (prop.Value.Type == JTokenType.Array)
                {
                    AddAttributesWithExplicitActions(name, prop.Value);
                }
                else if (prop.Type == JTokenType.Property)
                {
                    AddAttributeFromScalar(name, prop);
                }
            }

            return GetAssetXml();
        }

        protected override IEnumerable GetObjectItems(object obj)
        {
            var links = obj as JObject;
            return links.Properties();
        }

        protected override string GetKey(object item)
        {
            var link = item as JProperty;
            return link.Name;
        }

        protected override IEnumerable GetRelationItemEnumerable(object obj)
        {
            return obj as JObject;
        }

        protected override Attribute CreateAttributeFromRelationItem(object obj)
        {
            var prop = obj as JProperty;
            return new Attribute(prop.Name, prop.Value);
        }

        protected override IEnumerable<object> GetRelationItems(object linkObj)
        {
            var link = linkObj as JProperty;
            var relationItems = new List<object>();

            if (link.Value.Type == JTokenType.Array)
            {
                var array = link.Value as JArray;
                foreach (var item in array)
                {
                    var props = item as JObject;
                    relationItems.Add(props);
                }
            }
            else if (link.Value.Type == JTokenType.Object)
            {
                var relationItem = (link.Value as JObject);
                relationItems.Add(relationItem);
            }

            return relationItems;
        }

        protected override object[] GetArrayFromObject(object obj)
        {
            var array = obj as JArray;
            return array.Children().Cast<object>().ToArray();
        }

        protected override void AddAttributeFromScalar(string name, object scalar)
        {
            var prop = scalar as JProperty;
            _builder.AddAssetAttributeFromScalar(name, prop.Value);
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