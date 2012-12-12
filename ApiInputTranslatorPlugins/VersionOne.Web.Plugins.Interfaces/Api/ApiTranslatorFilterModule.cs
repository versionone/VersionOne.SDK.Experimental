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
            context.BeginRequest += context_BeginRequest;
        }

        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpContext.Current.Request.Filter =
                new ApiInputTranslatorFilter(HttpContext.Current.Request.Filter);

            HttpContext.Current.Response.Filter =
                new ApiOutputTranslatorFilter(HttpContext.Current.Response.Filter);
        }
    }
}