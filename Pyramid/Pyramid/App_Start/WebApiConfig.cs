using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Pyramid
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "AspireApi",
                routeTemplate: "api/{controller}/{action}"
            );
        }
    }
}