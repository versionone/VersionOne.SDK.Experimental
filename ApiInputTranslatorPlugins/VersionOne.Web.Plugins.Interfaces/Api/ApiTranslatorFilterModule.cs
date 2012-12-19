using System;
using System.Web;

namespace VersionOne.Web.Plugins.Api
{
    public class ApiTranslatorFilterModule : IHttpModule
    {
        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.PreRequestHandlerExecute += (sender, args) =>
            {
                HttpContext.Current.Request.Filter =
                    new ApiInputTranslatorFilter(HttpContext.Current.Request.Filter);

                HttpContext.Current.Response.Filter =
                    new ApiOutputTranslatorFilter(
                        HttpContext.Current.Response.Filter);
            };
        }
    }
}