using System.Web.Http;

namespace LargeGraphLayout
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{moduleId}",
                defaults: new { moduleId = RouteParameter.Optional }
            );
        }
    }
}
