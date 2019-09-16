using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Workplace_v2._2.Controllers
{
    public class ErrorsController : Controller
    {
        // GET: Errors
        public ActionResult PageNotFound()
        {
            ViewBag.exceptionMessage = "The path you've accessed wasn't correct.";
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}