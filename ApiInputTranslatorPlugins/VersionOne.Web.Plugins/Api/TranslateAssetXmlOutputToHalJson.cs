using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml.XPath;
using Newtonsoft.Json.Linq;

namespace VersionOne.Web.Plugins.Api
{
    [Export(typeof(ITranslateAssetXmlOutputToContentType))]
    public class TranslateAssetXmlOutputToHalJson : ITranslateAssetXmlOutputToContentType
    {
        private static readonly string[] ContentTypes = new[]
        {
            "hal+json",
            "text/hal+json",
            "application/hal+json"
        };

        protected string[] GetContentTypes()
        {
            return ContentTypes;
        }

        public bool CanTranslate(string contentType)
        {
            if (!string.IsNullOrWhiteSpace(contentType))
            {
                contentType = contentType.Trim();
                return GetContentTypes().Any(c => c.Equals(contentType, StringComparison.OrdinalIgnoreCase));
            }

            return false;
        }

        public string Execute(string input)
        {
            using (var reader = new StringReader(input))
            {
                var doc = new XPathDocument(reader);
                var nav = doc.CreateNavigator();

                var rootObject = new JObject();

                AddAttributes(nav, rootObject);
                AddRelationships(nav, rootObject);

                return rootObject.ToString();
            }
        }

        private static void AddAttributes(XPathNavigator nav, JObject rootObject)
        {
            var attributeNodes = nav.Select("//Attribute");
            while (attributeNodes.MoveNext())
            {
                var nodeNav = attributeNodes.Current;
                var attrName = nodeNav.GetAttribute("name", string.Empty);
                var attrValue = nodeNav.Value;
                rootObject.Add(attrName, attrValue);
            }
        }

        private static void AddRelationships(XPathNavigator nav, JObject rootObject)
        {
            var relationNodes = nav.Select("//Relation");
            var relations = new JObject();

            // Add the identity relation
            var assetNode = nav.SelectSingleNode("//Asset");
            var href = assetNode.GetAttribute("href", string.Empty);
            var id = assetNode.GetAttribute("id", string.Empty);
            var self = new JObject { { "href", new JValue(href) }, { "id", new JValue(id) } };
            relations.Add("self", self);

            while (relationNodes.MoveNext())
            {
                var nodeNav = relationNodes.Current;
                var relationName = nodeNav.GetAttribute("name", string.Empty);
                var relatedAssets = new JArray();
                var assets = nodeNav.SelectDescendants("Asset", string.Empty, false);
                while (assets.MoveNext())
                {
                    var assetNav = assets.Current;
                    var asset = new JObject();
                    var hrefVal = assetNav.GetAttribute("href", string.Empty);
                    var idref = assetNav.GetAttribute("idref", string.Empty);
                    asset.Add("href", hrefVal);
                    asset.Add("idref", idref);
                    relatedAssets.Add(asset);
                }
                AddRelationItems(relatedAssets, relations, relationName);
            }

            rootObject.Add("_links", relations);
        }

        private static void AddRelationItems(JArray relatedAssets, JObject relations, string relationName)
        {
            if (relatedAssets.Count > 1)
            {
                relations.Add(relationName, relatedAssets);
            }
            else if (relatedAssets.Count == 1)
            {
                relations.Add(relationName, relatedAssets[0]);
            }
            else
            {
                relations.Add(relationName, new JObject());
            }
        }
    }
}
