using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Workplace_v2._2.Attributes;
using Workplace_v2._2.Models;

namespace Workplace_v2._2.Controllers
{
    [CustomAuthorize(Roles = "Candidate")]
    public class InterviewsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly string error = "~/Views/Shared/Error.cshtml";
        
        public ActionResult StartInterview()
        {
            try
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                Interview interview = db.Interviews.Where(i => i.CandidateId == user.Id).FirstOrDefault();
                if (interview.IsDone == true || interview.HasBegun == true)
                {
                    return RedirectToAction("Index", "Answers");
                }
                interview.HasBegun = true;
                db.SaveChanges();

                List<Answer> answers = new List<Answer>();
                List<Question> allQuestions = db.Questions.Where(q => q.JobId == interview.JobId && q.Approved).ToList();
                int qcount = 1;
                Random Randomizer = new Random();
                while (allQuestions.Count > 0 && qcount <= 10)
                {
                    int idx = Randomizer.Next(0, allQuestions.Count);
                    Answer answer = new Answer
                    {
                        Candidate = user,
                        CandidateId = user.Id,
                        Question = allQuestions[idx],
                        QuestionId = allQuestions[idx].QuestionId,
                        Text = "",
                        Score = 0
                    };
                    answers.Add(answer);

                    allQuestions.RemoveAt(idx);
                    ++qcount;
                }
                InterviewViewModel interviewModel = new InterviewViewModel
                {
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
                Session["TimeExpire" + Session.SessionID] = DateTime.UtcNow.AddSeconds(5400);

                return View(interviewModel);
            }
            catch (NullReferenceException)
            {
                ViewBag.exceptionMessage = "The interview doesn't exist";
            }
            catch (Exception)
            {
                ViewBag.exceptionMessage = "You can't access this page.";
            }
            return View(error);
        }
        
        [HttpPost]
        public ActionResult SubmitInterview(InterviewViewModel interviewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = db.Users.Find(User.Identity.GetUserId());
                    Interview interview = db.Interviews.Where(i => i.CandidateId == user.Id).FirstOrDefault();
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

                    foreach (var ans in answers)
                    {
                        Answer newAnswer = new Answer()
                        {
                            Candidate = user,
                            CandidateId = user.Id,
                            Question = db.Questions.Find(ans.QuestionId),
                            QuestionId = ans.QuestionId,
                            Text = ans.Text,
                            Score = 0
                        };
                        db.Answers.Add(newAnswer);

                        db.SaveChanges();
                    }

                    interview.IsDone = true;
                    db.SaveChanges();

                    Session.Remove("TimeExpire" + Session.SessionID);
                    TempData["success"] = "Your answers have been successfully submited.";

                    return RedirectToAction("Index", "Home");
                }
                catch (NullReferenceException)
                {
                    ViewBag.exceptionMessage = ViewBag.exceptionMessage = "The interview doesn't exist";
                }
                catch (Exception)
                {
                    ViewBag.exceptionMessage = "You can't access this page.";
                }
                return View(error);
            }
            return View("~/Views/Interviews/StartInterview.cshtml", interviewModel);
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
