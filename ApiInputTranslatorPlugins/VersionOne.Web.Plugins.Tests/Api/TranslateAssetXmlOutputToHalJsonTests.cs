using NUnit.Framework;
using VersionOne.Web.Plugins.Api;

namespace VersionOne.Web.Plugins.Tests.Api
{
    [TestFixture]
    public class TranslateAssetXmlOutputToHalJsonTests
    {
        private readonly TranslateAssetXmlOutputToHalJson _subject = new TranslateAssetXmlOutputToHalJson();

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
        public void translates_members_with_object_notation()
        {
            const string expected =
@"{
  ""DefaultRole.Name"": ""Role.Name'System Admin"",
  ""SecurityScope.Name"": """",
  ""Ideas"": """",
  ""AssetState"": ""64"",
  ""SendConversationEmails"": ""true"",
  ""Username"": ""admin"",
  ""Followers.Name"": """",
  ""Description"": """",
  ""Email"": ""admin@company.com"",
  ""_links"": {
    ""self"": {
      ""href"": ""/versionone.web/rest-1.v1/Data/Member/20"",
      ""id"": ""Member:20""
    },
    ""SecurityScope"": {},
    ""DefaultRole"": {
      ""href"": ""/versionone.web/rest-1.v1/Data/Role/1"",
      ""idref"": ""Role:1""
    }
  }
}";
            var actual = _subject.Execute(AssetExample);

            Assert.AreEqual(expected, actual);
        }
    }
}
