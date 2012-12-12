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
    public class TranslateHalJsonHalInputToAssetXml :
        BaseTranslateApiHalInputToAssetXml, ITranslateApiInputToAssetXml
    {
        public override string Execute(string input)
        {
            var jsonObject = (JObject)JsonConvert.DeserializeObject(input);

            foreach (var prop in jsonObject.Properties())
            {
                var name = prop.Name;

                if (prop.Value.Type == JTokenType.Object)
                {
                    AddRelationsFromLinks(name, prop.Value);
                }
                else if (prop.Value.Type == JTokenType.Array)
                {
                    AddAttributesWithExplicitActionFromArray(name, prop.Value);
                }
                else if (prop.Type == JTokenType.Property)
                {
                    AddAttributeFromScalarProperty(name, prop);
                }
            }

            return GetAssetXml();
        }

        protected override IEnumerable GetLinkGroupsFromRootProperty(object rootObject)
        {
            var links = rootObject as JObject;
            return links.Properties();
        }

        protected override string GetLinkGroupKeyFromProperty(object property)
        {
            var link = property as JProperty;
            return link.Name;
        }

        protected override Attribute CreateAttributeFromRelationItem(object obj)
        {
            var prop = obj as JProperty;
            return new Attribute(prop.Name, prop.Value);
        }

        protected override IEnumerable<IEnumerable> GetLinkGroupRelations(object linkGroup)
        {
            var link = linkGroup as JProperty;
            var relationItems = new List<JObject>();

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

        protected override void AddAttributeFromScalarProperty(string name, object scalar)
        {
            var prop = scalar as JProperty;
            Builder.AddAssetAttributeFromScalar(name, prop.Value);
        }

        private static readonly string[] _contentTypes = new[]
            {
                "hal+json",
                "text/hal+json",
                "application/hal+json"
            };

        protected override string[] GetContentTypes()
        {
            return _contentTypes;
        }
    }
}