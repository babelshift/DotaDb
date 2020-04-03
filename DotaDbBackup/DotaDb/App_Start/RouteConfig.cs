using System.Web.Mvc;
using System.Web.Routing;

namespace DotaDb
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "HeroSpecificsWithName",
                url: "heroes/{id}/{heroName}",
                defaults: new { controller = "Heroes", action = "Hero", heroName = UrlParameter.Optional }
            );

            //routes.MapRoute(
            //    name: "HeroSpecifics",
            //    url: "heroes/{id}",
            //    defaults: new { controller = "Heroes", action = "Hero" }
            //);

            routes.MapRoute(
                name: "HeroItemBuildsWithName",
                url: "heroes/{id}/{heroName}/build",
                defaults: new { controller = "Heroes", action = "Build", heroName = UrlParameter.Optional }
            );

            //routes.MapRoute(
            //    name: "HeroItemBuilds",
            //    url: "heroes/{id}/{action}",
            //    defaults: new { controller = "Heroes" }
            //);

            //routes.MapRoute(
            //    name: "CosmeticItems",
            //    url: "items/cosmetics/{prefab}",
            //    defaults: new { controller = "Items", Action = "Cosmetics", prefab = UrlParameter.Optional }
            //);

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}