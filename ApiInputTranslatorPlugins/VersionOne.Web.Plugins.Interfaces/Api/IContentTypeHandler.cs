namespace VersionOne.Web.Plugins.Api
{
    public interface IContentTypeHandler
    {
        bool CanHandle(string contentType);
    }
}