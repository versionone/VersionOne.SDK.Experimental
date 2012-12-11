using System.Xml.XPath;

namespace VersionOne.Web.Plugins.Api
{
    public interface ITranslateApiInputToAssetXml : IContentTypeHandler
    {
        XPathDocument Execute(string input);
    }
}
