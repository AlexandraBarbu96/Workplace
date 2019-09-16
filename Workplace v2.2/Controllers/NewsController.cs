using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Workplace_v2._2.Attributes;
using Workplace_v2._2.Models;

namespace Workplace_v2._2.Controllers
{
    [CustomAuthorize(Roles = "Administrator,Supervisor")]
    public class NewsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly string error = "~/Views/Shared/Error.cshtml";

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
            NewsViewModel news = GetAdministratorModel();

            return View(news);
        }

        [NonAction]
        [HttpGet]
        public NewsViewModel GetAdministratorModel()
        {
            NewsViewModel news = new NewsViewModel();

            List<Question> allRequests = db.Questions.Where(q => q.Approved == false).ToList();
            if (User.IsInRole("Supervisor"))
            {
                List<Question> requests = new List<Question>();
                int departmentId = db.Users.Find(User.Identity.GetUserId()).AdminDepartmentId;
                List<int> jobIds = db.Jobs.Where(j => j.DepartmentId == departmentId).Select(j => j.JobId).ToList();
                foreach (int jobId in jobIds)
                {
                    requests.AddRange(allRequests.Where(q => q.JobId == jobId));
                }
                allRequests = requests;
            }
            news.Requests = allRequests;

            List<Interview> interviews = db.Interviews.Where(i => i.Candidate.IsActive && i.IsDone && !i.IsGraded).ToList();
            if (User.IsInRole("Supervisor"))
            {
                int departmentId = db.Users.Find(User.Identity.GetUserId()).AdminDepartmentId;
                interviews = interviews.Where(i => i.Job.DepartmentId == departmentId).ToList();
            }
            news.Interviews = interviews;
            /*
            var candidatesToDelete = db.Users.Where(u => u.CandidateInterviewId == 0).ToList();
            if (candidatesToDelete.Count() > 0)
            {
                foreach(var candidate in candidatesToDelete)
                {
                    if(candidate.IsInRole("Candidate"))
                        new UsersController().Delete(candidate.Id, Url.Action("GetAdministratorModel","News"));
                }
            }
            */
            List<ApplicationUser> allCandidates = db.Users.Where(c => c.IsActive == false && c.CandidateInterview.IsGraded == false).ToList();
            if (User.IsInRole("Supervisor"))
            {
                List<ApplicationUser> candidates = new List<ApplicationUser>();
                int departmentId = db.Users.Find(User.Identity.GetUserId()).AdminDepartmentId;
                List<int> jobIds = db.Jobs.Where(j => j.DepartmentId == departmentId).Select(j => j.JobId).ToList();
                foreach (int jobId in jobIds)
                {
                    candidates.AddRange(allCandidates.Where(c => c.CandidateInterview.JobId == jobId));
                }
                allCandidates = candidates;
            }
            news.Candidates = allCandidates;

            List<Experience> experiences = new List<Experience>();
            foreach (var candidate in allCandidates)
            {
                Experience experience = db.Experiences.Where(e => e.CandidateId == candidate.Id).First();
                experiences.Add(experience);
            }
            news.Experiences = experiences;

            return news;
        }
        
        [HttpPut]
        [ValidateAntiForgeryToken]
        public ActionResult Candidates(int cn)
        {
            try
            {
                NewsViewModel news = GetAdministratorModel();
                ApplicationUser candidateToActivate = db.Users.Find(news.Candidates[cn].Id);
                candidateToActivate.IsActive = true;
                
                db.SaveChanges();

                var callbackUrl = Url.Action("Index", "Home", routeValues: null, protocol: Request.Url.Scheme);
                new AccountController().SendMail(candidateToActivate.Email, "Workplace Interview Update", "Congratulations! You have been selected for the preliminary interview for " + candidateToActivate.CandidateInterview.Job.Title + " job. You can begin right away!" + callbackUrl);

                TempData["success"] = "The candidate has been successfully activated. ";
                return Redirect(Url.RouteUrl(new { controller = "News", action = "Index" }) + "#candidates");
                //return RedirectToAction("Index");
            }
            
            catch (Exception e)
            {
                ViewBag.exceptionMessage = e.Message;
                return View(error);
            }
        }
        
        [HttpPut]
        [ValidateAntiForgeryToken]
        public ActionResult Requests(int qn)
        {
            try
            {
                NewsViewModel news = GetAdministratorModel();
                Question questionToApprove = db.Questions.Find(news.Requests[qn].QuestionId);
                questionToApprove.Approved = true;

                db.SaveChanges();

                var message = "";
                var count = db.Questions.Where(q => q.JobId == questionToApprove.JobId && q.Approved == true).ToList().Count();
                if (db.Questions.Where(q => q.JobId == questionToApprove.JobId && q.Approved == true).ToList().Count() == 10)
                {
                    var job = db.Jobs.Find(questionToApprove.JobId);
                    job.IsAvailable = true;
                    db.SaveChanges();

                    message = "The job is now available for candidates.";
                }

                TempData["success"] = "The request has been successfully approved. " + message;
                return Redirect(Url.RouteUrl(new { controller = "News", action = "Index" }) + "#requests");
                //return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                ViewBag.exceptionMessage = e.Message;
                return View(error);
            }
        }
    }
}