using System;
using System.IO;
using System.Xml.XPath;
using FluentJson;
using NUnit.Framework;

namespace VersionOne.Web.Plugins.Tests.Api
{
    [TestFixture]
    public class AssetXmlToJsonTranslatorTests
    {
        [Test]
        public void translates_member()
        {
            const string input = 
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

            var expected = @"{""Asset"":{""href"":""/versionone.web/rest-1.v1/Data/Member/20"",""id"":""Member:20""},""Data"":{""DefaultRole.Name"":""Role.Name'System Admin"",""SecurityScope.Name"":"""",""Ideas"":"""",""AssetState"":""64"",""SendConversationEmails"":""true"",""Username"":""admin"",""Followers.Name"":"""",""Description"":"""",""Email"":""admin@company.com""}}";
            
            var actual = ConvertAssetXmlToJsonString(input);

            Assert.AreEqual(expected, actual);
        }

        private static string ConvertAssetXmlToJsonString(string data)
        {
            using (var reader = new StringReader(data))
            {
                var doc = new XPathDocument(reader);
                var nav = doc.CreateNavigator();

                var json = JsonObject.Create();

                json.AddProperty("Asset", p =>
                                              {
                                                  var assetNode = nav.SelectSingleNode("//Asset");
                                                  var href = assetNode.GetAttribute("href", string.Empty);
                                                  var idref = assetNode.GetAttribute("id", string.Empty);
                                                  p
                                                      .AddProperty("href", href)
                                                      .AddProperty("id", idref);
                                              }
                    )
                    .AddProperty("Data", p =>
                                             {
                                                 var attributes = nav.Select("//Attribute");
                                                 while (attributes.MoveNext())
                                                 {
                                                     var nodeNav = attributes.Current;
                                                     var attrName = nodeNav.GetAttribute("name", string.Empty);
                                                     var attrValue = nodeNav.Value;
                                                     p
                                                         .AddProperty(attrName, attrValue);
                                                 }
                                             }
                    );

                var jsonString = json.ToJson();

                return jsonString;
            }
        }
    }
}
