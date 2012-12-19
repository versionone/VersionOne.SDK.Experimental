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
            "haljson",
            "hal+json",
            "text/hal+json",
            "application/hal+json"
        };

        protected string[] GetContentTypes()
        {
            return ContentTypes;
        }

        public bool CanHandle(string contentType)
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

                var assetList = AddAssets(nav);
                if (assetList.Count > 0)
                {
                    return assetList.ToString();
                } 
                else 
                {
                    var rootObject = new JObject();
                    var relations = new JObject();

                    AddAttributes(nav, "//Attribute", rootObject);
                    AddIdentityRelation(nav, "//Asset", relations);
                    AddRelationships(nav, "//Relation", rootObject, relations);

                    return rootObject.ToString();
                }
            }
        }

        // TODO: add tests
        private static JArray AddAssets(XPathNavigator nav)
        {
            var assetList = new JArray();

            var assetsNodes = nav.Select("//Assets");
            var hasRootNode = assetsNodes.MoveNext();
            if (!hasRootNode)
            {
                return assetList;
            }
            var nodeNav = assetsNodes.Current;
            var assetNodes = nodeNav.SelectDescendants("Asset", string.Empty, false);
            while(assetNodes.MoveNext())
            {
                var assetNode = assetNodes.Current;
                var asset = new JObject();
                var assetRelations = new JObject();
                AddIdentityRelation(assetNode, ".", assetRelations);
                AddAttributes(assetNode, "./Attribute", asset);
                AddRelationships(assetNode, "./Relation", asset, assetRelations);
                assetList.Add(asset);
            }
            
            return assetList;
        }

        private static void AddAttributes(XPathNavigator nav, string selectPath, JObject propertyContainer)
        {
            var attributeNodes = nav.Select(selectPath);
            while (attributeNodes.MoveNext())
            {
                var nodeNav = attributeNodes.Current;
                var attrName = nodeNav.GetAttribute("name", string.Empty);
                var attrValue = nodeNav.Value;
                propertyContainer.Add(attrName, attrValue);
            }
        }

        private static void AddIdentityRelation(XPathNavigator nav, string selectPath, JObject relations)
        {
            // Add the identity relation
            var assetNode = nav.SelectSingleNode(selectPath);
            var href = assetNode.GetAttribute("href", string.Empty);
            var id = assetNode.GetAttribute("id", string.Empty);
            var self = new JObject { { "href", new JValue(href) }, { "id", new JValue(id) } };
            relations.Add("self", self);
        }

        private static void AddRelationships(XPathNavigator nav, string selectPath, JObject propertyContainer, JObject relations)
        {
            var relationNodes = nav.Select(selectPath);

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

            propertyContainer.Add("_links", relations);
        }

        private static void AddRelationItems(JArray relatedAssets, JObject relations, string relationName)
        {
            if (relatedAssets.Count > 0)
            {
                relations.Add(relationName, relatedAssets);
            }
            else
            {
                relations.Add(relationName, new JArray());
            }
        }
    }
}
