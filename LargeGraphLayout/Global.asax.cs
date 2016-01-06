using System.Web.Http;
using LargeGraphLayout.App_Start;

namespace LargeGraphLayout
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            Data.Initialize();
        }
    }
}