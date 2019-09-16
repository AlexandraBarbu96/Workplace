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
    [CustomAuthorize(Roles = "Administrator")]
    public class UsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly string error = "~/Views/Shared/Error.cshtml";

        // GET: Users
        public ActionResult Index()
        {
            try
            {
                if (TempData.ContainsKey("success"))
                {
                    ViewBag.message = TempData["success"].ToString();
                }
                if (Session["isEmptyFirstName" + Session.SessionID] != null)
                {
                    ModelState.AddModelError("FirstName", "The FirstName field is required.");
                    Session.Remove("isEmptyFirstName" + Session.SessionID);
                }
                if (Session["isEmptyLastName" + Session.SessionID] != null)
                {
                    ModelState.AddModelError("LastName", "The LastName field is required.");
                    Session.Remove("isEmptyLastName" + Session.SessionID);
                }
                if (Session["isEmptyUserName" + Session.SessionID] != null)
                {
                    ModelState.AddModelError("UserName", "The UserName field is required.");
                    Session.Remove("isEmptyUserName" + Session.SessionID);
                }
                else if (Session["isValidUserName" + Session.SessionID] != null)
                {
                    var message = (string)Session["isValidUserName" + Session.SessionID];
                    ModelState.AddModelError("UserName", (string)Session["isValidUserName" + Session.SessionID]);
                    Session.Remove("IsValidUserName" + Session.SessionID);
                }
                if (Session["isEmptyEmail" + Session.SessionID] != null)
                {
                    ModelState.AddModelError("Email", "The Email field is required.");
                    Session.Remove("isEmptyEmail" + Session.SessionID);
                }
                else if (Session["isValidEmail" + Session.SessionID] != null)
                {
                    var message = (string)Session["isValidEmail" + Session.SessionID];
                    ModelState.AddModelError("Email", (string)Session["isValidEmail" + Session.SessionID]);
                    Session.Remove("IsValidEmail" + Session.SessionID);
                }
                ViewBag.Departments = Session["departments" + Session.SessionID];
                ViewBag.departmentFilter = Session["departmentFilter" + Session.SessionID];
                var users = Session["users" + Session.SessionID];
                Session.Remove("departments" + Session.SessionID);
                Session.Remove("departmentFilter" + Session.SessionID);
                Session.Remove("users" + Session.SessionID);

                return View(users);
            }
            catch (Exception)
            {
                ViewBag.exceptionMessage = "You can't access this page.";
            }
            return View(error);
        }

        // GET: Users/Details/5
        public ActionResult Details(string id)
        {
            try
            {
                if(id == null)
                {
                    throw new Exception();
                }
                ApplicationUser applicationUser = db.Users.Find(id);
                if(applicationUser.IsInRole("Candidate"))
                {
                    Experience experience = db.Experiences.Where(e => e.CandidateId == id).FirstOrDefault();
                    ExperienceViewModel experienceModel = new ExperienceViewModel()
                    {
                        userId = id,
                        JobId = applicationUser.CandidateInterview.JobId,

                        addedSkills = db.CandidateSkills.Where(cs => cs.ExperienceId == experience.ExperienceId).Select(cs => cs.Skill).ToList(),
                        userEducation = db.Educations.Where(e => e.ExperienceId == experience.ExperienceId).ToList(),
                        addedPastJobs = db.PastJobs.Where(pj => pj.ExperienceId == experience.ExperienceId).ToList()
                    };
                    return View("~/Views/Users/CandidateDetails.cshtml",experienceModel);
                }
                return View(applicationUser);
            }
            catch (NullReferenceException)
            {
                ViewBag.exceptionMessage = "The user doesn't exist.";
            }
            catch (Exception)
            {
                ViewBag.exceptionMessage = "You can't access this page.";
            }
            return View(error);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return RedirectToAction("Create", "Account");
        }

        [NonAction]
        public bool IsValidUsername(string newName, string oldName)
        {
            return !(db.Users.Any(d => d.UserName == newName) && newName != oldName);
        }

        [NonAction]
        public bool IsValidEmail(string newEmail, string oldEmail)
        {
            return !(db.Users.Any(d => d.Email == newEmail) && newEmail != oldEmail);
        }

        [HttpPut]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ApplicationUser newData)
        {
            var goback = false;
            if (string.IsNullOrEmpty(newData.FirstName))
            {
                Session["IsEmptyFirstName" + Session.SessionID] = true;
                goback = true;
            }
            if (string.IsNullOrEmpty(newData.LastName))
            {
                Session["IsEmptyLastName" + Session.SessionID] = true;
                goback = true;
            }
            if (string.IsNullOrEmpty(newData.UserName))
            {
                Session["IsEmptyUserName" + Session.SessionID] = true;
                goback = true;
            }
            else if (!IsValidUsername(newData.UserName, db.Users.Find(newData.Id).UserName))
            {
                Session["IsValidUserName" + Session.SessionID] = "The " + newData.UserName + " username is already taken.";
                goback = true;
            }
            if (string.IsNullOrEmpty(newData.Email))
            {
                Session["IsEmptyEmail" + Session.SessionID] = true;
                goback = true;
            }
            else if (!IsValidEmail(newData.Email, db.Users.Find(newData.Id).Email))
            {
                Session["IsValidEmail" + Session.SessionID] = "The " + newData.Email + " email address is already taken.";
                goback = true;
            }
            if (ModelState.IsValid && !goback)
            {
                try
                {
                    ApplicationUser user = db.Users.Find(newData.Id);
                    if (TryUpdateModel(user))
                    {
                        user.FirstName = newData.FirstName;
                        user.LastName = newData.LastName;
                        user.UserName = newData.UserName;
                        user.Email = newData.Email;
                        user.PhoneNumber = newData.PhoneNumber;

                        if (user.IsInRole("Employee"))
                        {
                            var newDepartmentId = int.Parse(HttpContext.Request.Params.Get("newDepartmentId"));
                            if (user.EmployeeDepartmentId != newDepartmentId)
                            {
                                user.EmployeeDepartmentId = newDepartmentId;
                            }
                        }
                        db.SaveChanges();
                    }
                    TempData["success"] = "The user has been successfully updated.";
                    if (user.IsInRole("Supervisor"))
                    {
                        Session["url" + Session.SessionID] = "supervisors";
                    }
                    else
                    {
                        Session["url" + Session.SessionID] = "employees";
                    }
                    return RedirectToAction("Create","Account");
                }
                catch (NullReferenceException)
                {
                    ViewBag.exceptionMessage = "The user doesn't exist.";
                }
                catch (Exception)
                {
                    ViewBag.exceptionMessage = "You can't access this page.";
                }
            }
            //newData.AllDepartments = newData.GetAllDepartments();
            
            Session["url" + Session.SessionID] = "tabs-min-" + newData.Id;
            return RedirectToAction("Create", "Account");
            //return View(newData);
        }

        // POST: Users/Delete/5
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id,string url)
        {
            try
            {
                ApplicationUser userToBeDeleted = db.Users.Find(id);
                var urlRoute = "";
                if (userToBeDeleted.IsInRole("Administrator") || (userToBeDeleted.IsInRole("Supervisor") && userToBeDeleted.AdminDepartment != null))
                {
                    throw new Exception();
                }
                else if (userToBeDeleted.IsInRole("Candidate"))
                {
                    urlRoute = "#candidates";
                    Interview interview = db.Interviews.FirstOrDefault(i => i.CandidateId == id);
                    if (interview != null)
                    {
                        foreach(var ans in db.Answers.Where(a => a.CandidateId == userToBeDeleted.Id))
                        {
                            db.Answers.Remove(ans);
                        }

                        var message = "";
                        var callbackUrl = Url.Action("Index", "Home", routeValues: null, protocol: Request.Url.Scheme);
                        if (!userToBeDeleted.IsActive && !interview.HasBegun && interview.Job != null)
                        { 
                            message = "We're sorry to inform you that you haven't been selected for the preliminary interview for " + userToBeDeleted.CandidateInterview.Job.Title + " job. ";
                            new AccountController().SendMail(userToBeDeleted.Email, "Workplace Interview Update", message + "You can apply for other jobs on our website:" + callbackUrl);
                        }
                        if (userToBeDeleted.IsActive)
                        {
                            if (!interview.HasBegun)
                            {
                                message = "We're sorry to inform you that your preliminary interview for " + userToBeDeleted.CandidateInterview.Job.Title + " job has been canceled. ";
                            }
                            if (interview.IsDone || interview.IsGraded)
                            {
                                message = "We're sorry to inform you that you haven't been selected for the next stage of the interview for " + userToBeDeleted.CandidateInterview.Job.Title + " job. ";
                            }
                            new AccountController().SendMail(userToBeDeleted.Email, "Workplace Interview Update", message + "You can apply for other jobs on our website:" + callbackUrl);
                        }
                    }
                    db.Users.Remove(userToBeDeleted);
                    if (interview != null)
                    {
                        db.Interviews.Remove(interview);
                    }
                    db.SaveChanges();

                    if (!TempData.ContainsKey("success"))
                    {
                        TempData["success"] = "The user has been successfully deleted.";
                    }
                    return Redirect(url);
                }
                if (userToBeDeleted.IsInRole("Employee") || userToBeDeleted.IsInRole("Supervisor"))
                {
                    if (userToBeDeleted.IsInRole("Employee"))
                    {
                        urlRoute = "#employees";
                    }
                    List<Question> questions = db.Questions.Where(q => q.CreatorId == id).ToList();
                    if (questions.Any())
                    {
                        Department department = new Department();
                        if (userToBeDeleted.AdminDepartmentId != 0)
                        {
                            department = db.Departments.Find(userToBeDeleted.AdminDepartmentId);
                        }
                        else
                        {
                            department = db.Departments.Find(userToBeDeleted.EmployeeDepartmentId);
                        }

                        if (department != null)
                        {
                            foreach (var q in questions)
                            {
                                if (q.Approved && userToBeDeleted.IsInRole("Employee"))
                                {
                                    q.CreatorId = department.AdminId;
                                    q.Creator = department.Admin;
                                }
                                else
                                {
                                    db.Questions.Remove(q);
                                }
                            }
                        }
                        else
                        {
                            db.Questions.RemoveRange(questions);
                        }
                        db.SaveChanges();
                    }
                    db.Users.Remove(userToBeDeleted);
                    db.SaveChanges();
                }  

                if (!TempData.ContainsKey("success"))
                {
                    TempData["success"] = "The user has been successfully deleted.";
                }
                return Redirect(url + urlRoute);
            }
            catch (ArgumentException)
            {
                ViewBag.exceptionMessage = "The path you have accessed does not exist.";
            }
            catch (NullReferenceException)
            {
                ViewBag.exceptionMessage =  "The user doesn't exist";
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