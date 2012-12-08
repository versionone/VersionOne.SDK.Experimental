using System.Xml.XPath;

namespace VersionOne.Web.Plugins.Api
{
    public interface ITranslateAssetXmlOutputToContentType
    {
        bool CanTranslate(string contentType);
        string Execute(string input);
    }
}
