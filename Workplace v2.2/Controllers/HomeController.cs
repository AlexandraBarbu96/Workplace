using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Workplace_v2._2.Models;

namespace Workplace_v2._2.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            if (TempData.ContainsKey("success"))
            {
                ViewBag.message = TempData["success"].ToString();
            }
            if (TempData.ContainsKey("danger"))
            {
                ViewBag.message = TempData["danger"].ToString();
            }
            Session.RemoveAll();
            ViewBag.Departments = new SelectList(db.Departments, "DepartmentId", "Name");
            ViewBag.YearsOfExperience = new JobViewModel().GetAllYears();
            ViewBag.departmentFilter = "---Select---"; //db.Departments.First().Name;
            ViewBag.experienceFilter = "none";
            var jobs = db.Jobs.Where(j => j.IsAvailable).ToList();
            var filter = HttpContext.Request.Params.Get("departmentsFilter");
            if (filter != null)
            {
                ViewBag.departmentFilter = db.Departments.Find(int.Parse(filter)).DepartmentId;
                jobs = jobs.Where(j => j.DepartmentId == int.Parse(filter)).ToList();
            }
            filter = HttpContext.Request.Params.Get("experienceFilter");
            if (filter != null)
            {
                ViewBag.experienceFilter = filter;
                jobs = jobs.Where(j => j.YearsOfExperience == filter).ToList();
            }
            return View(jobs);
        }

        public ActionResult Register(int id)
        {
            Session["jobId" + Session.SessionID] = id;
            return RedirectToAction("Register", "Account");
        }
    }
}