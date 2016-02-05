using DotaDb.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace DotaDb.Controllers
{
    public class BaseController : Controller
    {
        #region Properties

        /// <summary>
        /// Allows inherited controllers to properly log any events.
        /// </summary>
        protected LogHelper Log { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor establishes a log to use
        /// </summary>
        public BaseController()
        {
            Log = new LogHelper(GetType().FullName);
        }

        #endregion Constructors
        
        /// <summary>
        /// Global OnException filter which is used by MVC when an exception occurs in a controller. Only works in production mode.
        /// Does nothing if the exception is already handled. If the exception is unhandled, the status code is set to 500 and returned.
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnException(ExceptionContext filterContext)
        {
#if !DEBUG
            if (filterContext.ExceptionHandled)
            {
                return;
            }

            filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            filterContext.Result = GetActionResultOnException(filterContext);

            Log.Error(filterContext.Exception.Message, filterContext.Exception.StackTrace);

            filterContext.ExceptionHandled = true;
#endif
        }

        /// <summary>
        /// Gets the appropriate result to send back on an exception. If the request is an AJAX request, JSON is returned. Otherwise, the standard Error view is displayed.
        /// </summary>
        /// <param name="filterContext"></param>
        /// <returns></returns>
        private ActionResult GetActionResultOnException(ExceptionContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                JsonResult jsonResult = Json(new { success = false, message = "Unhandled exception." });
                jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                return jsonResult;
            }
            else
            {
                return new ViewResult() { ViewName = "~/Views/Shared/Error.cshtml" };
            }
        }
    }
}