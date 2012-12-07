using System.IO;
using System.Xml.XPath;
using FluentJson;
using NUnit.Framework;
using Newtonsoft.Json.Linq;

namespace VersionOne.Web.Plugins.Tests.Api
{
    [TestFixture]
    public class TranslateAssetXmlOutputToJsonTests
    {
        private const string AssetExample =
            @"<Asset href='/versionone.web/rest-1.v1/Data/Member/20' id='Member:20'>
  <Attribute name='DefaultRole.Name'>Role.Name'System Admin</Attribute>
  <Attribute name='SecurityScope.Name'/>
  <Attribute name='Ideas'/>
  <Attribute name='AssetState'>64</Attribute>
  <Attribute name='SendConversationEmails'>true</Attribute>
  <Relation name='SecurityScope'/>
  <Attribute name='Username'>admin</Attribute>
  <Attribute name='Followers.Name'/>
  <Attribute name='Description'/>
  <Attribute name='Email'>admin@company.com</Attribute>
  <Relation name='DefaultRole'>
    <Asset href='/versionone.web/rest-1.v1/Data/Role/1' idref='Role:1'/>
  </Relation>
</Asset>
";

        [Test]
        public void translates_members_with_array_notation()
        {
            var expected =
                @"[
  {
    ""DefaultRole.Name"": ""Role.Name'System Admin"",
    ""SecurityScope.Name"": """",
    ""Ideas"": """",
    ""AssetState"": ""64"",
    ""SendConversationEmails"": ""true"",
    ""Username"": ""admin"",
    ""Followers.Name"": """",
    ""Description"": """",
    ""Email"": ""admin@company.com""
  },
  {
    ""SecurityScope"": [],
    ""DefaultRole"": [
      {
        ""href"": ""/versionone.web/rest-1.v1/Data/Role/1"",
        ""idref"": ""Role:1""
      }
    ]
  },
  {
    ""href"": ""/versionone.web/rest-1.v1/Data/Member/20"",
    ""id"": ""Member:20""
  }
]";
            var actual = ConvertWithArrayNotation.Translate(AssetExample);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void translates_members_with_object_notation()
        {
            var expected =
@"{
  ""Attributes"": {
    ""DefaultRole.Name"": ""Role.Name'System Admin"",
    ""SecurityScope.Name"": """",
    ""Ideas"": """",
    ""AssetState"": ""64"",
    ""SendConversationEmails"": ""true"",
    ""Username"": ""admin"",
    ""Followers.Name"": """",
    ""Description"": """",
    ""Email"": ""admin@company.com""
  },
  ""Relations"": {
    ""SecurityScope"": [],
    ""DefaultRole"": [
      {
        ""href"": ""/versionone.web/rest-1.v1/Data/Role/1"",
        ""idref"": ""Role:1""
      }
    ]
  },
  ""Asset"": {
    ""href"": ""/versionone.web/rest-1.v1/Data/Member/20"",
    ""id"": ""Member:20""
  }
}";
            var actual = ConvertWithObjectNotation.Translate(AssetExample);

            Assert.AreEqual(expected, actual);
        }
    }

    public static class ConvertWithArrayNotation
    {
        public static string Translate(string data)
        {
            using (var reader = new StringReader(data))
            {
                var doc = new XPathDocument(reader);
                var nav = doc.CreateNavigator();

                var container = new JArray();
                var attributes = new JObject();
                var relations = new JObject();
                var identity = new JObject();

                AddAttributeValues(nav, attributes, container);
                AddRelationships(nav, relations, container);
                AddIdentity(nav, identity, container);

                return container.ToString();
            }
        }

        private static void AddAttributeValues(XPathNavigator nav, JObject attributes, JArray container)
        {
            var attributeNodes = nav.Select("//Attribute");
            while (attributeNodes.MoveNext())
            {
                var nodeNav = attributeNodes.Current;
                var attrName = nodeNav.GetAttribute("name", string.Empty);
                var attrValue = nodeNav.Value;
                attributes.Add(attrName, attrValue);
            }
            container.Add(attributes);
        }

        private static void AddRelationships(XPathNavigator nav, JObject relations, JArray container)
        {
            var relationNodes = nav.Select("//Relation");
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
                relations.Add(relationName, relatedAssets);
            }
            container.Add(relations);
        }

        private static void AddIdentity(XPathNavigator nav, JObject identity, JArray container)
        {
            var assetNode = nav.SelectSingleNode("//Asset");
            var href = assetNode.GetAttribute("href", string.Empty);
            var id = assetNode.GetAttribute("id", string.Empty);
            identity.Add("href", new JValue(href));
            identity.Add("id", new JValue(id));
            container.Add(identity);
        }

    }

    public static class ConvertWithObjectNotation
    {
        public static string Translate(string data)
        {
            using (var reader = new StringReader(data))
            {
                var doc = new XPathDocument(reader);
                var nav = doc.CreateNavigator();

                var container = new JObject();
                var attributes = new JObject();
                var relations = new JObject();
                var identity = new JObject();

                AddAttributeValues(nav, attributes, container);
                AddRelationships(nav, relations, container);
                AddIdentity(nav, identity, container);

                return container.ToString();
            }
        }

        private static void AddAttributeValues(XPathNavigator nav, JObject attributes, JObject container)
        {
            var attributeNodes = nav.Select("//Attribute");
            while (attributeNodes.MoveNext())
            {
                var nodeNav = attributeNodes.Current;
                var attrName = nodeNav.GetAttribute("name", string.Empty);
                var attrValue = nodeNav.Value;
                attributes.Add(attrName, attrValue);
            }
            container.Add("Attributes", attributes);
        }

        private static void AddRelationships(XPathNavigator nav, JObject relations, JObject container)
        {
            var relationNodes = nav.Select("//Relation");
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
                relations.Add(relationName, relatedAssets);
            }
            container.Add("Relations", relations);
        }

        private static void AddIdentity(XPathNavigator nav, JObject identity, JObject container)
        {
            var assetNode = nav.SelectSingleNode("//Asset");
            var href = assetNode.GetAttribute("href", string.Empty);
            var id = assetNode.GetAttribute("id", string.Empty);
            identity.Add("href", new JValue(href));
            identity.Add("id", new JValue(id));
            container.Add("Asset", identity);
        }
    }
}
