using NUnit.Framework;
using VersionOne.Web.Plugins.Api;

namespace VersionOne.Web.Plugins.Tests.Api
{
    [TestFixture]
    public class TranslateHalJsonInputToAssetXmlTests
    {
        private TranslateHalJsonHalInputToAssetXml _subject;

        [TestCase("json", false)]
        [TestCase("    json", false)]
        [TestCase("text/json   ", false)]
        [TestCase("  application/json ", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        [TestCase("text/xml", false)]
        [TestCase("hal+json", true)]
        [TestCase("text/hal+json", true)]
        [TestCase("application/hal+json", true)]
        [TestCase("     application/hal+json", true)]
        [TestCase("application/hal+json     ", true)]
        public void CanProcess_supports_correct_content_types(string contentType, bool expected)
        {
            _subject = new TranslateHalJsonHalInputToAssetXml();

            Assert.AreEqual(expected, _subject.CanHandle(contentType), "Content-Type:" + contentType);
        }

        [Test]
        public void single_attribute_update_creates_set_as_default_action()
        {
            const string input =
@"
{ ""Name"" : ""Josh"" }
";

            const string expected =
@"<Asset>
  <Attribute name=""Name"" act=""set"">Josh</Attribute>
</Asset>";

            _subject = new TranslateHalJsonHalInputToAssetXml();

            var actual = _subject.Execute(input).CreateNavigator().OuterXml;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void multiple_attribute_update_has_correct_actions()
        {
            const string input =
@"
{ ""Name"" : ""Josh"", ""Phone"" : [""set"", ""555""], ""Address"": [""remove""], ""Info"" : [""add"", ""newvalue""] }
";

            const string expected =
@"<Asset>
  <Attribute name=""Name"" act=""set"">Josh</Attribute>
  <Attribute name=""Phone"" act=""set"">555</Attribute>
  <Attribute name=""Address"" act=""remove"" />
  <Attribute name=""Info"" act=""add"">newvalue</Attribute>
</Asset>";

            _subject = new TranslateHalJsonHalInputToAssetXml();

            var actual = _subject.Execute(input).CreateNavigator().OuterXml;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void create_asset_with_multiple_attributes_and_single_relation()
        {
            const string input =
@"
{
    Name:""Commit"",
    URL:""http://jgough/apiservice/commits.html?id=1"",
    OnMenu:true,
    RemoveProp:[""remove""],
    AddToProp:[""add"",""addedValue""],
    _links: {
        ""Asset"":
            [
                {""idref"":""Story:1082""},
                {""idref"":""Story:9090""}
            ],
        ""Scope"" : { ""idref"" : ""Scope:0"" }        
    }
}
";

const string expected =
@"<Asset>
  <Attribute name=""Name"" act=""set"">Commit</Attribute>
  <Attribute name=""URL"" act=""set"">http://jgough/apiservice/commits.html?id=1</Attribute>
  <Attribute name=""OnMenu"" act=""set"">True</Attribute>
  <Attribute name=""RemoveProp"" act=""remove"" />
  <Attribute name=""AddToProp"" act=""add"">addedValue</Attribute>
  <Relation name=""Asset"" act=""set"">
    <Asset idref=""Story:1082"" />
    <Asset idref=""Story:9090"" />
  </Relation>
  <Relation name=""Scope"" act=""set"">
    <Asset idref=""Scope:0"" />
  </Relation>
</Asset>";
            _subject = new TranslateHalJsonHalInputToAssetXml();

            var actual = _subject.Execute(input).CreateNavigator().OuterXml;

            Assert.AreEqual(expected, actual);
        }
    }
}