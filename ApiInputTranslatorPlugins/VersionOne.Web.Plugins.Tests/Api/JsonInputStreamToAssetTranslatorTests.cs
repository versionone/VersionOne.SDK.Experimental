using System.Collections.Specialized;
using NUnit.Framework;
using VersionOne.Web.Plugins.Api;

namespace VersionOne.Web.Plugins.Tests.Api
{
    [TestFixture]
    public class JsonInputStreamToAssetTranslatorTests
    {
        private JsonInputStreamToAssetXmlTranslator _subject;

        private readonly NameValueCollection _queryString = new NameValueCollection();

        [TestFixtureSetUp]
        public void Setup()
        {
            _queryString.Add("format", "json");    
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

            _subject = new JsonInputStreamToAssetXmlTranslator(input, _queryString);

            var actual = _subject.Execute().CreateNavigator().OuterXml;

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

            _subject = new JsonInputStreamToAssetXmlTranslator(input, _queryString);

            var actual = _subject.Execute().CreateNavigator().OuterXml;

            Assert.AreEqual(expected, actual);
        }
    }
}
