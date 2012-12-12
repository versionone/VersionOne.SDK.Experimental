namespace VersionOne.Web.Plugins.Api
{
    public interface ITranslateApiInputToAssetXml : IContentTypeHandler
    {
        string Execute(string input);
    }
}
