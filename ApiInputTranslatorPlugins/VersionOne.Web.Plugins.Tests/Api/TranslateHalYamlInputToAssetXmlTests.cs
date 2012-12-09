using NUnit.Framework;
using VersionOne.Web.Plugins.Api;

namespace VersionOne.Web.Plugins.Tests.Api
{
    [TestFixture]
    public class TranslateHalYamlInputToAssetXmlTests
    {
        private TranslateHalYamlInputToAssetXml _subject;

        [TestCase("json", false)]
        [TestCase("    json", false)]
        [TestCase("text/json   ", false)]
        [TestCase("  application/json ", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        [TestCase("text/xml", false)]
        [TestCase("yaml", true)]
        [TestCase("text/yaml", true)]
        [TestCase("application/yaml", true)]
        [TestCase("     application/yaml", true)]
        [TestCase("application/yaml     ", true)]
        public void CanProcess_supports_correct_content_types(string contentType, bool expected)
        {
            _subject = new TranslateHalYamlInputToAssetXml();

            Assert.AreEqual(expected, _subject.CanTranslate(contentType), "Content-Type:" + contentType);
        }

        [Test]
        public void single_attribute_update_creates_set_as_default_action()
        {
            const string input =
@"
Name : Josh
";

            const string expected =
@"<Asset>
  <Attribute name=""Name"" act=""set"">Josh</Attribute>
</Asset>";

            _subject = new TranslateHalYamlInputToAssetXml();

            var actual = _subject.Execute(input).CreateNavigator().OuterXml;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void multiple_attribute_update_has_correct_actions()
        {
            const string input =
@"
Name:   Josh
Phone:  [set, 555]
Address: [remove]
Info:   [add, new value]
";
            const string expected =
@"<Asset>
  <Attribute name=""Name"" act=""set"">Josh</Attribute>
  <Attribute name=""Phone"" act=""set"">555</Attribute>
  <Attribute name=""Address"" act=""remove"" />
  <Attribute name=""Info"" act=""add"">new value</Attribute>
</Asset>";

            _subject = new TranslateHalYamlInputToAssetXml();

            var actual = _subject.Execute(input).CreateNavigator().OuterXml;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void create_asset_with_multiple_attributes_and_single_relation()
        {
            const string input =
@"
_links:
    Asset:
        - idref: Story:1082
        - idref: Story:9090
    Scope:
        - idref: Scope:0
Name:   Commit
URL:    http://jgough/apiservice/commits.html?id=1
OnMenu: true
RemoveProp: [remove]
AddToProp:  [add, Added Value]
";

const string expected =
@"<Asset>
  <Relation name=""Asset"" act=""set"">
    <Asset idref=""Story:1082"" />
    <Asset idref=""Story:9090"" />
  </Relation>
  <Relation name=""Scope"" act=""set"">
    <Asset idref=""Scope:0"" />
  </Relation>
  <Attribute name=""Name"" act=""set"">Commit</Attribute>
  <Attribute name=""URL"" act=""set"">http://jgough/apiservice/commits.html?id=1</Attribute>
  <Attribute name=""OnMenu"" act=""set"">true</Attribute>
  <Attribute name=""RemoveProp"" act=""remove"" />
  <Attribute name=""AddToProp"" act=""add"">Added Value</Attribute>
</Asset>";
            _subject = new TranslateHalYamlInputToAssetXml();

            var actual = _subject.Execute(input).CreateNavigator().OuterXml;

            Assert.AreEqual(expected, actual);
        }
    }
}