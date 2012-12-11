using System.Web;
using VersionOne.Web.Plugins.Composition;

namespace VersionOne.Web.Plugins.Api
{
    public static class ApiTranslatorPluginsFactory
    {
        public static TTranslatorInterfaceType
            GetPluginForContentType<TTranslatorInterfaceType>(string contentType)
            where TTranslatorInterfaceType : class, IContentTypeHandler
        {
            var path = HttpContext.Current.Server.MapPath("bin\\Plugins");
            var plugins = new PartsList<TTranslatorInterfaceType>(path);

            foreach (var plugin in plugins.Items)
            {
                if (plugin.CanHandle(contentType))
                {
                    return plugin;
                }
            }
            return null;
        }


    }
}
