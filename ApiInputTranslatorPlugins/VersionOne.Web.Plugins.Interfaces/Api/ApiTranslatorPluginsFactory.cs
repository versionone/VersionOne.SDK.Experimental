using System.Collections.Generic;
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

        public static TTranslatorInterfaceType
            GetPluginForAcceptTypes<TTranslatorInterfaceType>(IEnumerable<string> acceptTypes)
            where TTranslatorInterfaceType : class, IContentTypeHandler
        {
            var path = HttpContext.Current.Server.MapPath("bin\\Plugins");
            var plugins = new PartsList<TTranslatorInterfaceType>(path);

            foreach (var plugin in plugins.Items)
            {
                foreach (var acceptType in acceptTypes)
                {
                    if (plugin.CanHandle(acceptType))
                    {
                        return plugin;
                    }
                }
            }
            return null;
        }
    }
}
