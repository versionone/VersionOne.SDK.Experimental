using NUnit.Framework;
using VersionOne.Web.Plugins.Api;

namespace VersionOne.Web.Plugins.Tests.Api
{
    [TestFixture]
    public class TranslateJsonInputToAssetXmlTests
    {

        private TranslateJsonInputToAssetXml _subject;

        [TestCase("json", true)]
        [TestCase("    json", true)]
        [TestCase("text/json   ", true)]
        [TestCase("  application/json ", true)]
        [TestCase("", false)]
        [TestCase(null, false)]
        [TestCase("text/xml", false)]
        public void CanProcess_supports_correct_content_types(string contentType, bool expected)
        {
            _subject = new TranslateJsonInputToAssetXml();

            Assert.AreEqual(expected, _subject.CanTranslate(contentType), "Content-Type:" + contentType);
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

            _subject = new TranslateJsonInputToAssetXml();

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

            _subject = new TranslateJsonInputToAssetXml();

            var actual = _subject.Execute(input).CreateNavigator().OuterXml;

            Assert.AreEqual(expected, actual);
        }
    }
}