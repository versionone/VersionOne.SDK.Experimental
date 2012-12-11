namespace VersionOne.Web.Plugins.Api
{
    public interface ITranslateAssetXmlOutputToContentType : IContentTypeHandler
    {
        string Execute(string input);
    }
}
