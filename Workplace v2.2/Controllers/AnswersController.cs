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
    [CustomAuthorize(Roles = "Candidate,Administrator,Supervisor")]
    public class AnswersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly string error = "~/Views/Shared/Error.cshtml";

        [CustomAuthorize(Roles = "Candidate")]
        public ActionResult Index()
        {
            try
            {
                var user = db.Users.Find(User.Identity.GetUserId());

                Session.RemoveAll();
                Interview interview = db.Interviews.Where(i => i.CandidateId == user.Id).FirstOrDefault();

                if (interview.HasBegun == false)
                {
                    return RedirectToAction("StartInterview", "Interviews");
                }
                if (interview.IsDone == false)
                {
                    throw new Exception();
                }
                List<Answer> answers = db.Answers.Where(a => a.CandidateId == user.Id).ToList();
                return View(answers);
            }
            catch (NullReferenceException)
            {
                ViewBag.exceptionMessage = "The path you've accessed wasn't correct.";
            }
            catch (Exception)
            {
                ViewBag.exceptionMessage = "You can't access this page.";
            }
            return View(error);
        }

        [CustomAuthorize(Roles = "Administrator,Supervisor")]
        public ActionResult Details(int idi)
        {
            try
            {
                Interview interview = db.Interviews.Find(idi);
                var UserId = User.Identity.GetUserId();
                if ((interview.Job.Department.AdminId != UserId && !User.IsInRole("Administrator")) || interview.IsDone == false)
                {
                    throw new Exception();
                }
                if (interview.IsGraded == true)
                {
                    TempData["danger"] = "The Interview has already been graded.";
                    return RedirectToAction("Index");
                }
                List<Answer> answers = db.Answers.Where(a => a.CandidateId == interview.CandidateId).ToList();
                foreach (var ans in answers)
                {
                    ans.Question = db.Questions.Find(ans.QuestionId);
                }
                InterviewViewModel interviewModel = new InterviewViewModel
                {
                    InterviewId = idi,
                    Answer1 = answers[0],
                    Answer2 = answers[1],
                    Answer3 = answers[2],
                    Answer4 = answers[3],
                    Answer5 = answers[4],
                    Answer6 = answers[5],
                    Answer7 = answers[6],
                    Answer8 = answers[7],
                    Answer9 = answers[8],
                    Answer10 = answers[9]
                };

                return View(interviewModel);
            }
            catch (NullReferenceException)
            {
                ViewBag.exceptionMessage = "The interview doesn't exist.";
            }
            catch (Exception)
            {
                ViewBag.exceptionMessage = "You can't access this page.";
            }
            return View(error);
        }

        [CustomAuthorize(Roles = "Administrator,Supervisor")]
        [HttpPost]
        public ActionResult SubmitScore(InterviewViewModel interviewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Interview interview = db.Interviews.Find(interviewModel.InterviewId);
                    List<Answer> answers = new List<Answer>();
                    answers.Add(interviewModel.Answer1);
                    answers.Add(interviewModel.Answer2);
                    answers.Add(interviewModel.Answer3);
                    answers.Add(interviewModel.Answer4);
                    answers.Add(interviewModel.Answer5);
                    answers.Add(interviewModel.Answer6);
                    answers.Add(interviewModel.Answer7);
                    answers.Add(interviewModel.Answer8);
                    answers.Add(interviewModel.Answer9);
                    answers.Add(interviewModel.Answer10);

                    float interviewScore = 0;
                    foreach (var ans in answers)
                    {
                        Answer answerToGrade = db.Answers.Find(ans.AnswerId);
                        if (answerToGrade != null)
                        {
                            answerToGrade.Score = ans.Score;
                        }
                        interviewScore += ans.Score;

                        db.SaveChanges();
                    }

                    interview.IsGraded = true;
                    interview.Score = interviewScore;

                    db.SaveChanges();
                    
                    TempData["success"] = "The interview has been successfully graded.";

                    var candidate = db.Users.Find(interview.CandidateId);
                    string message;
                    if (interview.Score >= interview.Job.MinimumScore)
                    {
                        message = "Hi, " + candidate.UserName + "! \n We're happy to announce you that you've been selected for the next stage of the interview for " + interview.Job.Title + " job. Someone will contact you soon.";

                    }
                    else
                    {
                        var callbackUrl = Url.Action("Index", "Home", routeValues: null, protocol: Request.Url.Scheme);
                        message = "Hi, " + candidate.UserName + "! \n We're sorry to announce that you haven't been selected for the next stage of the interview for " + interview.Job.Title + " job. You can apply for other jobs on our website.\n" + callbackUrl;

                        candidate.IsActive = false;
                        db.SaveChanges();
                    }
                    new AccountController().SendMail(candidate.Email, "Preliminary Interview Result", message);
                    return Redirect(Url.RouteUrl(new { controller = "News", action = "Index" }) + "#interviews");
                    //return RedirectToAction("Index", "News");
                }
                catch (NullReferenceException e)
                {
                    ViewBag.exceptionMessage = e.Message;
                }
                catch (Exception)
                {
                    ViewBag.exceptionMessage = "You can't access this page.";
                }
                return View(error);
            }
            return View("~/Views/Answers/Details.cshtml", interviewModel);
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
