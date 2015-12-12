using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DotaDb.Controllers
{
    public class MatchesController : Controller
    {
        public async Task<ActionResult> Live(long id)
        {
            return View();
        }
    }
}