using System.Collections.Specialized;
using System.Xml.XPath;

namespace VersionOne.Web.Plugins.Api
{
    public interface IApiInputStreamTranslator
    {
        XPathDocument Execute();
        void Initialize(string inputData, NameValueCollection queryString);
    }
}
