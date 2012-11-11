using System.Xml.XPath;

namespace VersionOne.Web.Plugins.Api
{
    public interface ITranslateApiInputToAssetXml
    {
        bool CanTranslate(string contentType);
        XPathDocument Execute(string input);
    }
}
