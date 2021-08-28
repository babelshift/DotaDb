using System.Web.Mvc;

namespace DotaDb.Controllers
{
    public class MapController : BaseController
    {
        [OutputCache(CacheProfile = "Default")]
        public ActionResult Index()
        {
            return View();
        }
    }
}