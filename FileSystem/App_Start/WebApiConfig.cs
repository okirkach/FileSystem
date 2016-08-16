using System.Web.Http;

namespace FileSystem {
    public static class WebApiConfig {
        public static void Register(HttpConfiguration config) {
            // Маршруты веб-API
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{action}");
        }
    }
}