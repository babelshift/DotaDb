using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DotaDb.Controllers
{
    public class HeroesController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult Hero()
        {
            return View();
        }
    }
}