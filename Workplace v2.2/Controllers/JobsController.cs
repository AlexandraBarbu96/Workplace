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
    [HandleError]
    [CustomAuthorize(Roles = "Administrator,Supervisor")]
    public class JobsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly string error = "~/Views/Shared/Error.cshtml";
        
        public ActionResult Create(int? id)
        {
            try
            {
                if (TempData.ContainsKey("danger"))
                {
                    ViewBag.message = TempData["danger"].ToString();
                }
                var user = db.Users.Find(User.Identity.GetUserId());
                if (id == null && User.IsInRole("Administrator"))
                {
                    throw new ArgumentNullException();
                }
                else if (id == null)
                {
                    id = user.AdminDepartmentId;
                }
                else
                {
                    if (id != user.AdminDepartmentId && user.AdminDepartmentId != 0)
                        throw new Exception();
                }
                JobViewModel job = new JobViewModel();
                if (Session["jobModel" + Session.SessionID] != null)
                {
                    job = (JobViewModel)Session["jobModel" + Session.SessionID];
                }
                else
                {
                    job.DepartmentId = (int)id;
                    job.isEdit = false;
                    job.YearsOfExperience = "none";
                    job.MinimumScore = 0;

                    job.addedSkills = new List<Skill>();
                    job.skillsToAdd = db.Skills.ToList();
                    job.newSkills = new List<Skill>();

                    job.addedStudies = new List<Study>();
                    job.studiesToAdd = db.Studies.ToList();
                    job.newStudies = new List<Study>();

                    Session["jobModel" + Session.SessionID] = job;
                }

                return View(job);
            }
            catch (ArgumentNullException)
            {
                ViewBag.exceptionMessage = "The path you've accessed wasn't correct.";
            }
            catch (NullReferenceException)
            {
                ViewBag.exceptionMessage = "The department doesn't exist.";
            }
            catch (Exception)
            {
                ViewBag.exceptionMessage = "You can't access this page.";
            }
            return View(error);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateStep2([Bind(Include = "JobId,DepartmentId,Title,Description,MinimumScore,YearsOfExperience")] JobViewModel result)
        {
            if (!string.IsNullOrEmpty(result.Description))
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        if (Session["jobModel" + Session.SessionID] == null)
                        {
                            throw new Exception();
                        }
                        JobViewModel jobModel = (JobViewModel)Session["jobModel" + Session.SessionID];
                        jobModel.Title = result.Title;
                        jobModel.Description = result.Description;
                        jobModel.MinimumScore = result.MinimumScore;
                        jobModel.YearsOfExperience = result.YearsOfExperience;
                        Session["jobModel" + Session.SessionID] = jobModel;
                        
                        return RedirectToAction("AddJobSkills");
                    }
                    catch (Exception)
                    {
                        return View(error);
                    }
                }
                TempData["danger"] = "You can't access this page.";
            }
            else
            {
                ModelState.AddModelError("Description", "The job description is required.");
            }
            return View("~/Views/Jobs/Create.cshtml",result);
        }

        public ActionResult AddJobSkills()
        {
            try
            {
                if (Session["jobModel" + Session.SessionID] == null)
                {
                    throw new Exception();
                }
                if(Session["IsEmptySkill" + Session.SessionID] != null)
                {
                    ModelState.AddModelError("Name", "Enter a skill name.");
                    Session.Remove("IsEmptySkill" + Session.SessionID);
                }
                if (TempData.ContainsKey("danger"))
                {
                    ViewBag.message = TempData["danger"].ToString();
                }

                return View();
            }
            catch(Exception)
            {
                return View(error);
            }
        }

        public ActionResult AddSkill(int ids)
        {
            try
            {
                if (ids == 0 || Session["jobModel" + Session.SessionID] == null)
                {
                    throw new Exception();
                }
                JobViewModel jobModel = (JobViewModel)Session["jobModel" + Session.SessionID];
                Skill skill = db.Skills.Find(ids);
                if (jobModel.addedSkills.Find(s => s.SkillId == skill.SkillId) == null)
                {
                    jobModel.addedSkills.Add(skill);
                }

                for (int i = 0; i < jobModel.skillsToAdd.Count(); ++i)
                {
                    if (jobModel.skillsToAdd[i].SkillId == skill.SkillId)
                        jobModel.skillsToAdd.RemoveAt(i);
                }

                Session["jobModel" + Session.SessionID] = jobModel;
                return RedirectToAction("AddJobSkills");
            }
            catch (NullReferenceException)
            {
                ViewBag.exceptionMessage = "The skill doesn't exist.";
            }
            catch (Exception)
            {
                ViewBag.exceptionMessage = "You can't access this page.";
            }

            return View(error);
        }

        public ActionResult RemoveSkill(int ids)
        {
            try
            {
                if (ids == 0 || Session["jobModel" + Session.SessionID] == null)
                {
                    throw new Exception();
                }
                JobViewModel jobModel = (JobViewModel)Session["jobModel" + Session.SessionID];
                Skill skill = db.Skills.Find(ids);
                for (int i = 0; i < jobModel.addedSkills.Count(); ++i)
                {
                    if (jobModel.addedSkills[i].SkillId == skill.SkillId)
                    {
                        jobModel.addedSkills.RemoveAt(i);
                        jobModel.skillsToAdd.Add(skill);
                    } 
                }

                Session["jobModel" + Session.SessionID] = jobModel;
                return RedirectToAction("AddJobSkills");
            }
            catch (NullReferenceException)
            {
                ViewBag.exceptionMessage = "The skill doesn't exist.";
            }
            catch (Exception)
            {
                ViewBag.exceptionMessage = "You can't access this page.";
            }

            return View(error);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNewSkill([Bind(Include = "Name")]Skill newSkill)
        {
            if (!string.IsNullOrEmpty(newSkill.Name))
            {
                try
                {
                    if (Session["jobModel" + Session.SessionID] == null)
                    {
                        throw new Exception();
                    }
                    JobViewModel jobModel = (JobViewModel)Session["jobModel" + Session.SessionID];
                    jobModel.newSkills.Add(newSkill);
                    Session["jobModel" + Session.SessionID] = jobModel;
                }
                catch(Exception)
                {
                    return View(error);
                }
            }
            else
            {
                Session["IsEmptySkill" + Session.SessionID] = true;
            }
            return RedirectToAction("AddJobSkills");
        }

        public ActionResult RemoveNewSkill(int? ns)
        {
            try
            {
                if (ns == null || Session["jobModel" + Session.SessionID] == null)
                {
                    throw new Exception();
                }
                JobViewModel jobModel = (JobViewModel)Session["jobModel" + Session.SessionID];
                jobModel.newSkills.RemoveAt((int)ns);
                Session["jobModel" + Session.SessionID] = jobModel;

                return RedirectToAction("AddJobSkills");
            }
            catch (Exception)
            {
                return View(error);
            }
        }

        public ActionResult AddJobStudies()
        {
            try
            {
                if (Session["jobModel" + Session.SessionID] == null)
                {
                    throw new Exception();
                }
                if (Session["IsEmptyDegree" + Session.SessionID] != null)
                {
                    ModelState.AddModelError("Degree", "Select a degree.");
                    Session.Remove("IsEmptyDegree" + Session.SessionID);
                }
                if (Session["IsEmptySpecialty" + Session.SessionID] != null)
                {
                    ModelState.AddModelError("Specialty", "Enter a specialty.");
                    Session.Remove("IsEmptySpecialty" + Session.SessionID);
                }
                if (TempData.ContainsKey("danger"))
                {
                    ViewBag.message = TempData["danger"].ToString();
                }
                List<string> degrees = db.Studies.Select(s => s.Degree).Distinct().ToList();
                List<Study> studies = new List<Study>();
                foreach (var d in degrees)
                {
                    Study item = db.Studies.Where(s => s.Degree == d).First();
                    studies.Add(item);
                }
                ViewBag.Degrees = new SelectList(studies, "StudyId", "Degree");

                return View();
            }
            catch (Exception)
            {
                return View(error);
            }
        }

        public ActionResult AddStudy(int ids)
        {
            try
            {
                if (ids == 0 || Session["jobModel" + Session.SessionID] == null)
                {
                    throw new Exception();
                }
                JobViewModel jobModel = (JobViewModel)Session["jobModel" + Session.SessionID];
                Study study = db.Studies.Find(ids);
                if (jobModel.addedStudies.Find(s => s.StudyId == study.StudyId) == null)
                {
                    jobModel.addedStudies.Add(study);
                }

                for (int i = 0; i < jobModel.studiesToAdd.Count(); ++i)
                {
                    if (jobModel.studiesToAdd[i].StudyId == study.StudyId)
                        jobModel.studiesToAdd.RemoveAt(i);
                }

                Session["jobModel" + Session.SessionID] = jobModel;
                return RedirectToAction("AddJobStudies");
            }
            catch (NullReferenceException)
            {
                ViewBag.exceptionMessage = "The study doesn't exist.";
            }
            catch (Exception)
            {
                ViewBag.exceptionMessage = "You can't access this page.";
            }

            return View(error);
        }

        public ActionResult RemoveStudy(int ids)
        {
            try
            {
                if (ids == 0 || Session["jobModel" + Session.SessionID] == null)
                {
                    throw new Exception();
                }
                JobViewModel jobModel = (JobViewModel)Session["jobModel" + Session.SessionID];
                Study study = db.Studies.Find(ids);
                for (int i = 0; i < jobModel.addedStudies.Count(); ++i)
                {
                    if (jobModel.addedStudies[i].StudyId == study.StudyId)
                    {
                        jobModel.addedStudies.RemoveAt(i);
                        jobModel.studiesToAdd.Add(study);
                    }
                        
                }

                Session["jobModel" + Session.SessionID] = jobModel;
                return RedirectToAction("AddJobStudies");
            }
            catch (NullReferenceException)
            {
                ViewBag.exceptionMessage = "The skill doesn't exist.";
            }
            catch (Exception)
            {
                ViewBag.exceptionMessage = "You can't access this page.";
            }

            return View(error);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNewStudy(Study newStudy)
        {
            if (newStudy.Degree != null && newStudy.Specialty != null)
            {
                try
                {
                    if (Session["jobModel" + Session.SessionID] == null)
                    {
                        throw new Exception();
                    }
                    JobViewModel jobModel = (JobViewModel)Session["jobModel" + Session.SessionID];
                    string degree = db.Studies.Find(int.Parse(newStudy.Degree)).Degree;
                    bool found = false;
                    foreach (var ns in jobModel.newStudies)
                    {
                        if (ns.Degree == degree && ns.Specialty == newStudy.Specialty)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        newStudy.Degree = degree;
                        jobModel.newStudies.Add(newStudy);
                        Session["jobModel" + Session.SessionID] = jobModel;
                    }
                    else
                    {
                        TempData["danger"] = "You can't add the same specialty twice.";
                    }
                }
                catch (Exception)
                {
                    return View(error);
                }
            }
            else
            {
                if(newStudy.Degree == null)
                    Session["IsEmptyDegree" + Session.SessionID] = true;
                if(newStudy.Specialty == null)
                    Session["IsEmptySpecialty" + Session.SessionID] = true;
            }
            return RedirectToAction("AddJobStudies");
        }

        public ActionResult RemoveNewStudy(int? ns)
        {
            try
            {
                if (ns == null || Session["jobModel" + Session.SessionID] == null)
                {
                    throw new Exception();
                }
                JobViewModel jobModel = (JobViewModel)Session["jobModel" + Session.SessionID];
                jobModel.newStudies.RemoveAt((int)ns);
                Session["jobModel" + Session.SessionID] = jobModel;

                return RedirectToAction("AddJobStudies");
            }
            catch (Exception)
            {
                return View(error);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FinishAddingJob()
        {
            try
            {
                if(Session["jobModel" + Session.SessionID] == null)
                {
                    throw new Exception();
                }
                Job job = new Job();
                JobViewModel jobModel = (JobViewModel)Session["jobModel" + Session.SessionID];
                if(jobModel.isEdit)
                {
                    job = db.Jobs.Find(jobModel.JobId);
                    job.Title = jobModel.Title;
                    job.Description = jobModel.Description;
                    job.DepartmentId = jobModel.DepartmentId;
                    job.MinimumScore = jobModel.MinimumScore;
                    job.YearsOfExperience = jobModel.YearsOfExperience;

                    foreach (var jobSkill in db.JobSkills.Where(ds => ds.JobId == job.JobId))
                    {
                        db.JobSkills.Remove(jobSkill);
                    }
                    foreach (var jobStudy in db.JobStudies.Where(ds => ds.JobId == job.JobId))
                    {
                        db.JobStudies.Remove(jobStudy);
                    }
                }
                else
                {
                    job.Title = jobModel.Title;
                    job.Description = jobModel.Description;
                    job.DepartmentId = jobModel.DepartmentId;
                    job.MinimumScore = jobModel.MinimumScore;
                    job.YearsOfExperience = jobModel.YearsOfExperience;
                    job.IsAvailable = false;
                    db.Jobs.Add(job);
                }

                db.SaveChanges();
                
                List<Skill> addedSkills = new List<Skill>();
                List<Study> addedStudies = new List<Study>();
                foreach (var ns in jobModel.newSkills)
                {
                    db.Skills.Add(ns);
                    db.SaveChanges();

                    addedSkills.Add(ns);
                }
                foreach (var ns in jobModel.newStudies)
                {
                    db.Studies.Add(ns);
                    db.SaveChanges();

                    addedStudies.Add(ns);
                }
                
                addedSkills.AddRange(jobModel.addedSkills);
                addedStudies.AddRange(jobModel.addedStudies);
                foreach (Skill s in addedSkills)
                {
                    JobSkill ds = new JobSkill();
                    ds.JobId = job.JobId;
                    ds.SkillId = s.SkillId;
                    db.JobSkills.Add(ds);
                }
                foreach (Study s in addedStudies)
                {
                    JobStudy ds = new JobStudy
                    {
                        JobId = job.JobId,
                        StudyId = s.StudyId
                    };
                    db.JobStudies.Add(ds);
                }

                db.SaveChanges();
                
                if(jobModel.isEdit)
                {
                    TempData["success"] = "The job has been successfully edited.";
                }
                else
                {
                    TempData["success"] = "The job has been successfully added.";
                }
                return RedirectToAction("Details", "Departments", new { id = job.DepartmentId });
            }
            catch (Exception)
            {
                return View(error);
            }
        }
        
        public ActionResult Edit(int id)
        {
            try
            {
                if (TempData.ContainsKey("danger"))
                {
                    ViewBag.message = TempData["danger"].ToString();
                }
                Job job = db.Jobs.Find(id);
                if (db.Users.Where(c => c.CandidateInterview.JobId == id && c.IsActive).ToList().Count() != 0)
                {
                    TempData["danger"] = "You can't edit the job while there are active candidates. Try de-activating the job first.";
                    return RedirectToAction("Details", new { id });
                }
                JobViewModel jobModel = new JobViewModel(); 
                if (Session["jobModel" + Session.SessionID]  != null)
                {
                    jobModel = (JobViewModel)Session["jobModel" + Session.SessionID];
                }
                
                if (job.Department.AdminId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
                {
                    jobModel.isEdit = true;
                    jobModel.JobId = id;
                    jobModel.DepartmentId = job.DepartmentId;
                    if(jobModel.Title == null && jobModel.Description == null)
                    {
                        jobModel.Title = job.Title;
                        jobModel.Description = job.Description;
                        jobModel.MinimumScore = job.MinimumScore;
                        jobModel.YearsOfExperience = job.YearsOfExperience;
                    }
                    ///PENTRU SKILLS
                    if (jobModel.addedSkills == null)
                    {
                        jobModel.addedSkills = new List<Skill>();
                        foreach (var ds in db.JobSkills.Where(d => d.JobId == job.JobId).ToList())
                        {
                            jobModel.addedSkills.Add(db.Skills.Find(ds.SkillId));
                        }
                    }

                    if (jobModel.skillsToAdd == null)
                    {
                        jobModel.skillsToAdd = db.Skills.ToList().Except(jobModel.addedSkills, new ApplicationsSkillModelEqualityComparer()).ToList();
                    }

                    if (jobModel.newSkills == null)
                    {
                        jobModel.newSkills = new List<Skill>();
                    }

                    ///PENTRU STUDII
                    if (jobModel.addedStudies == null)
                    {
                        jobModel.addedStudies = new List<Study>();
                        foreach (var ds in db.JobStudies.Where(d => d.JobId == job.JobId).ToList())
                        {
                            jobModel.addedStudies.Add(db.Studies.Find(ds.StudyId));
                        }
                    }

                    if (jobModel.studiesToAdd == null)
                    {
                        jobModel.studiesToAdd = db.Studies.ToList().Except(jobModel.addedStudies, new ApplicationsStudyModelEqualityComparer()).ToList();
                    }

                    if (jobModel.newStudies == null)
                    {
                        jobModel.newStudies = new List<Study>();
                    }

                    Session["jobModel" + Session.SessionID] = jobModel;
                    return View("~/Views/Jobs/Create.cshtml", jobModel);
                }
            }
            
            catch(NullReferenceException)
            {
                ViewBag.exceptionMessage = "The job doesn't exist.";
            }
            catch (Exception)
            {
                ViewBag.exceptionMessage = "You can't access this page.";
            }
            return View(error);
        }
        
        [HttpPut]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeAvailability(int id,string url)
        {
            try
            {
                if(string.IsNullOrEmpty(url))
                {
                    throw new ArgumentException();
                }
                Job job = db.Jobs.Find(id);
                if (job.Questions.Count() < 10)
                {
                    throw new Exception();
                }
                job.IsAvailable = !job.IsAvailable;

                if(job.IsAvailable)
                { 
                    foreach(var candidate in db.Users.Where(c => c.CandidateInterview.IsGraded && c.CandidateInterview.Score >= c.CandidateInterview.Job.MinimumScore).ToList())
                    {
                        candidate.IsActive = true;
                    }
                }
                else
                {
                    var callbackUrl = Url.Action("Index", "Home", routeValues: null, protocol: Request.Url.Scheme);
                    foreach (var candidate in db.Users.Where(c => c.CandidateInterview.JobId == id && c.IsActive).ToList())
                    {
                        candidate.IsActive = false;

                        if (!candidate.CandidateInterview.HasBegun)
                        {
                            var message = "Hi, " + candidate.UserName + "! \n We're sorry to announce that the " + candidate.CandidateInterview.Job.Title + " job you've applied to is no longer available. You can apply for other jobs on our website.\n" + callbackUrl;
                            new AccountController().SendMail(candidate.Email, "Workplace Interview Update", message);
                        }
                    }
                }
                
                db.SaveChanges();

                if(job.IsAvailable)
                    TempData["success"] = "The " + job.Title + " job is now available.";
                else
                    TempData["success"] = "The " + job.Title + " job is now unavailable.";
                return Redirect(url);
            }
            catch (ArgumentException)
            {
                ViewBag.exceptionMessage = "The path you have accessed does not exist.";
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
        
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id,string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    throw new ArgumentException();
                }
                Job job = db.Jobs.Find(id);
                var departmentId = job.DepartmentId;
                if (db.Users.Where(c => c.CandidateInterview.JobId == id && c.IsActive).ToList().Count() != 0)
                {
                    TempData["danger"] = "You can't delete the job while there are active candidates. Try de-activating the job first.";
                }
                else
                {
                    var jobCandidates = db.Users.Where(c => c.CandidateInterview.JobId == id).ToList();
                    var pendingJobCandidates = jobCandidates.Where(c => !c.CandidateInterview.HasBegun).ToList();
                    var jobTitle = job.Title;
                    if (job.Department.AdminId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
                    {
                        db.Jobs.Remove(job);
                        db.SaveChanges();
                    }
                    else
                    {
                        throw new Exception();
                    }
                    var callbackUrl = Url.Action("Index", "Home", routeValues: null, protocol: Request.Url.Scheme);
                    foreach (var candidate in pendingJobCandidates)
                    {
                        var message = "Hi, " + candidate.UserName + "! \n We're sorry to announce that the " + jobTitle + " job you've applied to is no longer available. You can apply for other jobs on our website.\n" + callbackUrl;
                        new AccountController().SendMail(candidate.Email, "Workplace Interview Update", message);
                    }
                    
                    foreach (var candidate in jobCandidates)
                    {
                        new UsersController().Delete(candidate.Id, Url.Action("Delete", "Jobs", new { id }));
                    }
                    TempData["success"] = "The job has been successfully deleted.";
                }

                return Redirect(url);
            }
            catch (ArgumentException)
            {
                ViewBag.exceptionMessage = "The path you have accessed does not exist.";
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
