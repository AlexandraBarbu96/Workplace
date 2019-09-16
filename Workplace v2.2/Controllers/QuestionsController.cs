using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Workplace_v2._2.Attributes;
using Workplace_v2._2.Models;

namespace Workplace_v2._2.Controllers
{
    [CustomAuthorize(Roles = "Administrator,Supervisor,Employee")]
    public class QuestionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly string error = "~/Views/Shared/Error.cshtml";

        public ActionResult Index(int id)
        {
            try
            {
                if (TempData.ContainsKey("success"))
                {
                    ViewBag.message = TempData["success"].ToString();
                }
                if (TempData.ContainsKey("danger"))
                {
                    ViewBag.message = TempData["danger"].ToString();
                }
                var job = db.Jobs.Find(id);
                List<Question> questions = db.Questions.Where(q => q.JobId == id && q.Approved == true).ToList();
                ViewBag.Job = job;
                return View(questions);
            }
            catch (NullReferenceException)
            {
                ViewBag.exceptionMessage = "The job doesn't exist.";
            }
            catch (Exception)
            {
                ViewBag.exceptionMessage = "You can't access this page.";
            }
            return View(error);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "QuestionId,Text,JobId,CreatorId")] Question question)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (User.IsInRole("Employee"))
                    {
                        question.Approved = false;
                        question.Job = db.Jobs.Find(question.JobId);
                        db.Questions.Add(question);
                        db.SaveChanges();
                        TempData["success"] = "The question has been successfully submited to be reviewed. ";
                        return RedirectToAction("Details", "Departments", new { id = question.Job.DepartmentId });
                    }
                    else
                    {
                        question.Approved = true;
                        db.Questions.Add(question);
                        db.SaveChanges();

                        var message = "";
                        var count = db.Questions.Where(q => q.JobId == question.JobId && q.Approved).Count();
                        if (count == 10)
                        {
                            var job = db.Jobs.Find(question.JobId);
                            job.IsAvailable = true;
                            db.SaveChanges();

                            message = "The job is now available for candidates.";
                        }
                        //else if( count > 10 )
                        {
                            //message = ", but only the first 10 questions will be used for the interviews.";
                        }

                        TempData["success"] = "The question has been successfully added. " + message;
                        return RedirectToAction("Index", "Questions", new { id = question.JobId });
                    }
                }
                catch (Exception)
                {
                    ViewBag.exceptionMessage = "You can't access this page.";
                }
                return View(error);
            }
            return View(question);
        }
        
        public ActionResult Edit(int id)
        {
            try
            {
                var question = db.Questions.Find(id);
                ApplicationUser editor = db.Users.Find(User.Identity.GetUserId());
                if ((User.IsInRole("Employee") || User.IsInRole("Supervisor")) && editor.Id != question.CreatorId)
                {
                    throw new Exception();
                    //return RedirectToAction("Index", "Questions", new { id = question.JobId });
                }

                return View(question);
            }
            catch (NullReferenceException)
            {
                ViewBag.exceptionMessage = "The question doesn't exist.";
            }
            catch (Exception)
            {
                ViewBag.exceptionMessage = "You can't access this page.";
            }
            return View(error);
        }

        // POST: Questions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPut]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "QuestionId,Text,JobId,CreatorId,Approved")] Question question)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(question).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["success"] = "The question has been successfully updated.";
                    return RedirectToAction("Index", "Questions", new { id = question.JobId });
                }
                catch (Exception)
                {
                    ViewBag.exceptionMessage = "You can't access this page.";
                }
                return View(error);
            }
            return View(question);
        }

        // POST: Questions/Delete/5
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteQuestion(int id,string url)
        {
            try
            {
                var question = db.Questions.Find(id);
                var jobId = question.JobId;
                ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
                if ((!user.IsInRole("Administrator") && user.Id != question.CreatorId) && (User.IsInRole("Supervisor") && user.AdminDepartmentId != db.Jobs.Find(jobId).DepartmentId))
                {
                    throw new Exception();
                }
                if (db.Interviews.Where(i => i.JobId == jobId).Any() && question.Approved)// && db.Questions.Where(q => q.JobId == question.JobId && q.Approved).Count() == 10)
                {
                    TempData["danger"] = "You can't change the questions while there are on-going interviews.";
                }
                else
                {
                    db.Questions.Remove(question);
                    db.SaveChanges();

                    var message = "";
                    if (db.Questions.Where(q => q.JobId == jobId && q.Approved).Count() == 9)
                    {
                        var job = db.Jobs.Find(jobId);
                        job.IsAvailable = false;
                        db.SaveChanges();

                        message = "Candidates can't apply for this job anymore.";
                    }

                    TempData["success"] = "The question has been successfully deleted. " + message;
                }
                
                return Redirect(url);
            }
            catch (ArgumentException)
            {
                ViewBag.exceptionMessage = "The path you have accessed does not exist.";
            }
            catch (NullReferenceException)
            {
                ViewBag.exceptionMessage = "The question doesn't exist.";
            }
            catch (Exception)
            {
                ViewBag.exceptionMessage = "You can't access this page.";
            }
            return View(error);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
