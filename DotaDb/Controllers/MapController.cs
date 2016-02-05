using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DotaDb.Controllers
{
    public class MapController : BaseController
    {
        [OutputCache(Duration = 86400, Location = System.Web.UI.OutputCacheLocation.Server)]
        public ActionResult Index()
        {
            return View();
        }
    }
}