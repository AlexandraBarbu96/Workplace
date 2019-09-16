using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Workplace_v2._2.Attributes;
using Workplace_v2._2.Models;

namespace Workplace_v2._2.Controllers
{
    [CustomAuthorize]
    public class AccountController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly string error = "~/Views/Shared/Error.cshtml";
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    {
                        var user = db.Users.Where(u => u.UserName == model.UserName).First();
                        if (!user.IsActive)
                        {
                            if (user.IsInRole("Candidate"))
                            {
                                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                                TempData["success"] = "Your account has not been checked yet. You will be notified via email when it will happen.";
                                if(user.CandidateInterview.IsGraded)
                                {
                                    TempData["success"] = "You can't access this account anymore.";
                                }
                                return RedirectToAction("Index", "Home");
                            }
                            else if(user.IsInRole("Employee") || user.IsInRole("Supervisor"))
                            {
                                user.IsActive = true;
                                db.SaveChanges();
                                return RedirectToAction("ChangePassword","Manage",routeValues:null);
                            }
                        }
                        else
                        {
                            if (user.IsInRole("Candidate") && user.CandidateInterview.IsGraded && user.CandidateInterview.Score >= user.CandidateInterview.Job.MinimumScore)
                            {
                                ViewBag.message = "You have been selected for the next stage of the interview for " + user.CandidateInterview.Job.Title + " job. Someone will contact you soon.";
                            }
                        }
                        return RedirectToLocal(returnUrl);
                    }
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(model);
                default:
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }
       
        [AllowAnonymous]
        public ActionResult Register()
        {
            try
            {
                RegisterViewModel user = new RegisterViewModel();
                ExperienceViewModel experienceModel = new ExperienceViewModel();

                if (Session["experienceModel" + Session.SessionID] == null)
                {
                    if (Session["jobId" + Session.SessionID] == null)
                    {
                        throw new Exception();
                    }
                    if (db.Jobs.Find((int)Session["jobId" + Session.SessionID]) != null)
                    {
                        if(db.Jobs.Find((int)Session["jobId" + Session.SessionID]).IsAvailable)
                        {
                            experienceModel.JobId = (int)Session["jobId" + Session.SessionID];
                            Session.Remove("jobId" + Session.SessionID);
                            Session["experienceModel" + Session.SessionID] = experienceModel;
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    return View(user);
                }
                else
                {
                    experienceModel = (ExperienceViewModel)Session["experienceModel" + Session.SessionID];
                    user = experienceModel.Candidate;
                    user.Password = "";
                    user.ConfirmPassword = "";
                    return View(user);
                }
            }
            catch(Exception)
            {
                return View(error);
            }
        }

        [NonAction]
        public bool IsValidUsername(string usernameToCheck)
        {
            return !db.Users.Any(u => u.UserName == usernameToCheck);
        }

        [NonAction]
        public bool IsValidEmail(string emailToCheck)
        {
            return !db.Users.Any(u => u.Email == emailToCheck);
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid && IsValidUsername(model.UserName) && IsValidEmail(model.Email))
            {
                var user = new ApplicationUser { FirstName = model.FirstName, LastName = model.LastName, UserName = model.UserName, Email = model.Email, PhoneNumber = model.PhoneNumber, IsActive = false };
                var result = UserManager.Create(user, model.Password);
                if (result.Succeeded)
                {
                    UserManager.Delete(user);
                    //await _signInManager.SignInAsync(user, isPersistent: false);
                    //_logger.LogInformation(3, "User created a new account with password.");
                    //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    //await UserManager.AddToRoleAsync(user.Id, "Candidate");                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                                                                                               // Send an email with this link
                                                                                               // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                                                                                               // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                                                                                               // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
                                                                                               
                    ExperienceViewModel experienceModel = (ExperienceViewModel)Session["experienceModel" + Session.SessionID];
                    experienceModel.Candidate = model;
                    experienceModel.addedSkills = new List<Skill>();
                    experienceModel.skillsToAdd = db.Skills.ToList();
                    experienceModel.newSkills = new List<Skill>();
                    experienceModel.userEducation = new List<Education>();
                    experienceModel.newEducationStudies = new List<Education>();
                    experienceModel.addedPastJobs = new List<PastJob>();
                    Session["experienceModel" + Session.SessionID] = experienceModel;
                    return RedirectToAction("AddUserSkills");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            if (!IsValidUsername(model.UserName))
            {
                ModelState.AddModelError("UserName", "Name " + model.UserName + " is already taken.");
            }
            if (!IsValidEmail(model.Email))
            {
                ModelState.AddModelError("Email", "Email " + model.Email + " is already taken.");
            }
            //ModelState.AddModelError("", "The Email or Username is already taken.");
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult AddUserSkills()
        {
            try
            {
                if (Session["experienceModel" + Session.SessionID] == null)
                {
                    throw new Exception();
                }
                if (Session["IsEmptySkill" + Session.SessionID] != null)
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
            catch (Exception)
            {
                return View(error);
            }
        }

        [AllowAnonymous]
        public ActionResult AddSkill(int ids)
        {
            try
            {
                if (ids == 0 || Session["experienceModel" + Session.SessionID] == null)
                {
                    throw new Exception();
                }
                ExperienceViewModel experienceModel = (ExperienceViewModel)Session["experienceModel" + Session.SessionID];
                Skill skill = new Skill();
                skill = db.Skills.Find(ids);
                if (experienceModel.addedSkills.Find(s => s.SkillId == skill.SkillId) == null)
                {
                    experienceModel.addedSkills.Add(skill);
                }
                for (int i = 0; i < experienceModel.skillsToAdd.Count(); ++i)
                {
                    if (experienceModel.skillsToAdd[i].SkillId == skill.SkillId)
                        experienceModel.skillsToAdd.RemoveAt(i);
                }
                Session["experienceModel" + Session.SessionID] = experienceModel;

                return RedirectToAction("AddUserSkills");
                //return View("~/Views/Account/AddUserSkills.cshtml");
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

        [AllowAnonymous]
        public ActionResult RemoveSkill(int ids)
        {
            try
            {
                if (ids == 0 || Session["experienceModel" + Session.SessionID] == null)
                {
                    throw new Exception();
                }
                ExperienceViewModel experienceModel = (ExperienceViewModel)Session["experienceModel" + Session.SessionID];
                Skill skill = new Skill();
            
                skill = db.Skills.Find(ids);
                for (int i = 0; i < experienceModel.addedSkills.Count(); ++i)
                {
                    if (experienceModel.addedSkills[i].SkillId == skill.SkillId)
                        experienceModel.addedSkills.RemoveAt(i);
                }
                experienceModel.skillsToAdd.Add(skill);
                Session["experienceModel" + Session.SessionID] = experienceModel;

                return RedirectToAction("AddUserSkills");
                //return View("~/Views/Account/AddUserSkills.cshtml");
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

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNewSkill([Bind(Include = "Name")]Skill newSkill)
        {
            if (!string.IsNullOrEmpty(newSkill.Name))
            {
                if (Session["experienceModel" + Session.SessionID] == null)
                {
                    throw new Exception();
                }
                try
                {
                    ExperienceViewModel experienceModel = (ExperienceViewModel)Session["experienceModel" + Session.SessionID];
                    experienceModel.newSkills.Add(newSkill);
                    Session["experienceModel" + Session.SessionID] = experienceModel;
                }
                catch (Exception)
                {
                    return View(error);
                }
            }
            else
            {
                Session["IsEmptySkill" + Session.SessionID] = true;
            }

            return RedirectToAction("AddUserSkills");
        }

        [AllowAnonymous]
        public ActionResult RemoveNewSkill(int? ns)
        {
            try
            {
                if (ns == null || Session["experienceModel" + Session.SessionID] == null)
                {
                    throw new Exception();
                }
                ExperienceViewModel experienceModel = (ExperienceViewModel)Session["experienceModel" + Session.SessionID];
            
                experienceModel.newSkills.RemoveAt((int)ns);
                Session["experienceModel" + Session.SessionID] = experienceModel;

                return RedirectToAction("AddUserSkills");
            }
            catch (Exception)
            {
                return View(error);
            }
        }

        [AllowAnonymous]
        public ActionResult AddUserEducation()
        {
            try
            {
                if (Session["experienceModel" + Session.SessionID] == null)
                {
                    throw new Exception();
                }
                if (Session["IsEmptyInstitution" + Session.SessionID] != null)
                {
                    ModelState.AddModelError("InstitutionName", "The Institution Name is required.");
                    Session.Remove("IsEmptyInstitution" + Session.SessionID);
                }
                if (Session["IsEmptyStudy" + Session.SessionID] != null)
                {
                    ModelState.AddModelError("Study", "Select a study.");
                    Session.Remove("IsEmptyStudy" + Session.SessionID);
                }
                if (Session["IsEmptyDegree" + Session.SessionID] != null)
                {
                    ModelState.AddModelError("newDegree", "Select a degree.");
                    Session.Remove("IsEmptyDegree" + Session.SessionID);
                }
                if (Session["IsEmptySpecialty" + Session.SessionID] != null)
                {
                    ModelState.AddModelError("newSpecialty", "Enter a specialty.");
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
                ViewBag.Studies = new SelectList(db.Studies, "StudyId", "Specialty");
                EducationViewModel educationModel = new EducationViewModel();
                if (Session["educationModel" + Session.SessionID] != null)
                {
                    educationModel = (EducationViewModel)Session["educationModel" + Session.SessionID];
                    if(educationModel.newDegree != null)
                    ViewBag.selectedDegree = educationModel.newDegree;
                    if(educationModel.Study != null)
                    ViewBag.selectedStudy = educationModel.Study.StudyId;
                    Session.Remove("educationModel" + Session.SessionID);
                }
                return View(educationModel);
            }
            catch (Exception)
            {
                return View(error);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultipleButton(Name = "action", Argument = "Add Study")]
        public ActionResult AddUserEducation([Bind(Include = "InstitutionName,Study")]EducationViewModel ev)
        {
            var goback = false;
            var selectedStudy = HttpContext.Request.Params.Get("selectedStudy");
            EducationViewModel educationModel = new EducationViewModel();
            if (selectedStudy == "" || selectedStudy == null)
            {
                Session["IsEmptyStudy" + Session.SessionID] = true;
                goback = true;
            }
            else
            {
                educationModel.Study = db.Studies.Find(int.Parse(selectedStudy));
            }
            if(string.IsNullOrEmpty(ev.InstitutionName))
            {
                Session["isEmptyInstitution" + Session.SessionID] = true;
                goback = true;
            }
            else
            {
                educationModel.InstitutionName = ev.InstitutionName;
            }
            if (goback)
            {
                Session["educationModel" + Session.SessionID] = educationModel;
                return RedirectToAction("AddUserEducation");
            }
            try
            {
                if (Session["experienceModel" + Session.SessionID] == null)
                {
                    throw new Exception();
                }
                ExperienceViewModel experienceModel = (ExperienceViewModel)Session["experienceModel" + Session.SessionID];

                var studyToAdd = db.Studies.Find(int.Parse(selectedStudy));
                bool found = false;
                foreach (var ud in experienceModel.userEducation)
                {
                    if (ud.Institution == ev.InstitutionName && ud.StudyId == studyToAdd.StudyId)
                    {
                        found = true;
                        break;
                    }

                }
                if (!found)
                {
                    Education e = new Education
                    {
                        Institution = ev.InstitutionName,
                        Study = studyToAdd,
                        StudyId = studyToAdd.StudyId
                    };

                    experienceModel.userEducation.Add(e);
                    Session["experienceModel" + Session.SessionID] = experienceModel;
                }
                else
                {
                    TempData["danger"] = "You can't add the same specialty twice.";
                }
                return RedirectToAction("AddUserEducation");
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

        [AllowAnonymous]
        public ActionResult RemoveEducation(int ns)
        {
            try
            {
                if (ns < 0 || Session["experienceModel" + Session.SessionID] == null)
                {
                    throw new Exception();
                }
                ExperienceViewModel experienceModel = (ExperienceViewModel)Session["experienceModel" + Session.SessionID];
                experienceModel.userEducation.RemoveAt(ns);
                Session["experienceModel" + Session.SessionID] = experienceModel;

                return RedirectToAction("AddUserEducation");
            }
            catch (ArgumentOutOfRangeException)
            {
                ViewBag.exceptionMessage = "The study doesn't exist.";
            }
            catch (Exception)
            {
                ViewBag.exceptionMessage = "You can't access this page.";
            }

            return View(error);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultipleButton(Name = "action", Argument = "Add New Study")]
        public ActionResult AddNewStudy([Bind (Include = "InstitutionName,newDegree,newSpecialty")]EducationViewModel ev)
        {
            var goback = false;
            var newDegree = HttpContext.Request.Params.Get("newDegree");
            EducationViewModel educationModel = new EducationViewModel();
            if (newDegree == "" || newDegree == null)
            {
                Session["IsEmptyDegree" + Session.SessionID] = true;
                goback = true;
            }
            else
            {
                educationModel.newDegree = newDegree;
            }
            if (string.IsNullOrEmpty(ev.newSpecialty))
            {
                Session["isEmptySpecialty" + Session.SessionID] = true;
                goback = true;
            }
            else
            {
                educationModel.newSpecialty = ev.newSpecialty;
            }
            if (string.IsNullOrEmpty(ev.InstitutionName))
            {
                Session["isEmptyInstitution" + Session.SessionID] = true;
                goback = true;
            }
            else
            {
                educationModel.InstitutionName = ev.InstitutionName;
            }
            if (goback)
            {
                Session["educationModel" + Session.SessionID] = educationModel;
                return RedirectToAction("AddUserEducation");
            }
            try
            {
                if (Session["experienceModel" + Session.SessionID] == null)
                {
                    throw new Exception();
                }
                ExperienceViewModel experienceModel = (ExperienceViewModel)Session["experienceModel" + Session.SessionID];

                string degree = db.Studies.Find(int.Parse(ev.newDegree)).Degree;
                bool found = false;
                foreach (var nes in experienceModel.newEducationStudies)
                {
                    if (nes.Institution == ev.InstitutionName && nes.Study.Degree == degree && nes.Study.Specialty == ev.newSpecialty)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    Education e = new Education
                    {
                        Institution = ev.InstitutionName,
                        Study = new Study()
                    };
                    e.Study.Degree = degree;
                    e.Study.Specialty = ev.newSpecialty;

                    experienceModel.newEducationStudies.Add(e);
                    Session["experienceModel" + Session.SessionID] = experienceModel;
                }
                else
                {
                    TempData["danger"] = "You can't add the same specialty twice.";
                }
                return RedirectToAction("AddUserEducation");
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

        [AllowAnonymous]
        public ActionResult RemoveNewEducation(int ns)
        {
            try
            {
                if (ns < 0 || Session["experienceModel" + Session.SessionID] == null)
                {
                    throw new Exception();
                }
                ExperienceViewModel experienceModel = (ExperienceViewModel)Session["experienceModel" + Session.SessionID];
                
                experienceModel.newEducationStudies.RemoveAt(ns);
                Session["experienceModel" + Session.SessionID] = experienceModel;

                return RedirectToAction("AddUserEducation");
            }
            catch (ArgumentOutOfRangeException)
            {
                ViewBag.exceptionMessage = "The study doesn't exist.";
            }
            catch (Exception)
            {
                ViewBag.exceptionMessage = "You can't access this page.";
            }

            return View(error);
        }

        [AllowAnonymous]
        public ActionResult AddUserJobs()
        {
            try
            {
                if (Session["experienceModel" + Session.SessionID] == null)
                {
                    throw new Exception();
                }
                if (Session["IsEmptyCompany" + Session.SessionID] != null)
                {
                    ModelState.AddModelError("Company", "The company name is required.");
                    Session.Remove("IsEmptyCompany" + Session.SessionID);
                }
                if (Session["IsEmptyPosition" + Session.SessionID] != null)
                {
                    ModelState.AddModelError("Position", "The position title is required.");
                    Session.Remove("IsEmptyPosition" + Session.SessionID);
                }
                if (Session["IsEmptyStartDate" + Session.SessionID] != null)
                {
                    ModelState.AddModelError("StartDate", "The start date is required.");
                    Session.Remove("IsEmptyStartDate" + Session.SessionID);
                }
                if (Session["IsEmptyEndDate" + Session.SessionID] != null)
                {
                    ModelState.AddModelError("EndDate", "The end date is required.");
                    Session.Remove("IsEmptyEndDate" + Session.SessionID);
                }
                if (TempData.ContainsKey("danger"))
                {
                    ViewBag.message = TempData["danger"].ToString();
                }

                PastJob pastJob = new PastJob();
                pastJob.StartDate = DateTime.Now;
                pastJob.EndDate = DateTime.Now;
                if (Session["pastJob" + Session.SessionID] != null)
                {
                    pastJob = (PastJob)Session["pastJob" + Session.SessionID];
                    Session.Remove("pastJob" + Session.SessionID);
                }
                return View(pastJob);
            }
            catch (Exception)
            {
                return View(error);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUserJobs(PastJob newJob)
        {
            try
            {
                var goback = false;
                PastJob pastJob = new PastJob();
                if (string.IsNullOrEmpty(newJob.Company))
                {
                    Session["isEmptyCompany" + Session.SessionID] = true;
                    goback = true;
                }
                else
                {
                    pastJob.Company = newJob.Company;
                }
                if (string.IsNullOrEmpty(newJob.Position))
                {
                    Session["isEmptyPosition" + Session.SessionID] = true;
                    goback = true;
                }
                else
                {
                    pastJob.Position = newJob.Position;
                }
                string startdate = HttpContext.Request.Params.Get("StartDate").Substring(0, 10);
                newJob.StartDate = DateTime.ParseExact(startdate, "mm/dd/yyyy", CultureInfo.InvariantCulture);
                if (newJob.StartDate.CompareTo(new DateTime()) == 0)
                {
                    Session["isEmptyStartDate" + Session.SessionID] = true;
                    goback = true;
                }
                else
                {
                    pastJob.StartDate = newJob.StartDate.Date;
                }
                string enddate = HttpContext.Request.Params.Get("EndDate").Substring(0,10);
                newJob.EndDate = DateTime.ParseExact(enddate, "mm/dd/yyyy", CultureInfo.InvariantCulture);
                if (newJob.EndDate.CompareTo(new DateTime()) == 0)
                {
                    Session["isEmptyEndDate" + Session.SessionID] = true;
                    goback = true;
                }
                else
                {
                    pastJob.EndDate = newJob.EndDate.Date;
                }
                if (goback)
                {
                    Session["pastJob" + Session.SessionID] = pastJob;
                    return RedirectToAction("AddUserJobs");
                }
            
                if (Session["experienceModel" + Session.SessionID] == null)
                {
                    throw new Exception();
                }
                ExperienceViewModel experienceModel = (ExperienceViewModel)Session["experienceModel" + Session.SessionID];

                if (newJob.StartDate < newJob.EndDate)
                {
                    bool found = false;
                    foreach (var aj in experienceModel.addedPastJobs)
                    {
                        if (newJob.Company == aj.Company && newJob.Position == aj.Position && newJob.StartDate == aj.StartDate && newJob.EndDate == aj.EndDate)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        newJob.StartDate = newJob.StartDate;
                        newJob.EndDate = newJob.EndDate;
                        experienceModel.addedPastJobs.Add(newJob);

                        Session["experienceModel" + Session.SessionID] = experienceModel;
                    }
                    else
                    {
                        TempData["danger"] = "You can't add the same job twice.";
                    }
                }
                else
                {
                    TempData["danger"] = "The start date can't come after the end date.";
                }
                return RedirectToAction("AddUserJobs");
            }
            catch (Exception)
            {
                return View(error);
            }
        }
        [AllowAnonymous]
        public ActionResult RemoveJob(int nj)
        {
            try
            {
                if (nj < 0 || Session["experienceModel" + Session.SessionID] == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                ExperienceViewModel experienceModel = (ExperienceViewModel)Session["experienceModel" + Session.SessionID];

                experienceModel.addedPastJobs.RemoveAt(nj);
                Session["experienceModel" + Session.SessionID] = experienceModel;
                return RedirectToAction("AddUserJobs");
            }
            catch (ArgumentOutOfRangeException)
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
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterConfirmed()
        {
            try
            { 
                ExperienceViewModel experienceModel = (ExperienceViewModel)Session["experienceModel" + Session.SessionID];
                RegisterViewModel model = experienceModel.Candidate;
                var user = new ApplicationUser { FirstName = model.FirstName, LastName = model.LastName, UserName = model.UserName, Email = model.Email, PhoneNumber = model.PhoneNumber, IsActive = false };
                var result = UserManager.Create(user, model.Password);
                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user.Id, "Candidate");
                    //await _signInManager.SignInAsync(user, isPersistent: false);
                    //_logger.LogInformation(3, "User created a new account with password.");
                    //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    //await UserManager.AddToRoleAsync(user.Id, "Candidate");                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
                    
                    experienceModel.userId = user.Id;
                    Session["experienceModel" + Session.SessionID] = experienceModel;
                    return RedirectToAction("Experience");
                    //return View("~/Views/Account/AddUserSkills.cshtml");
                }
                AddErrors(result);
            }
            catch
            {
                return View(error);
            }
            return RedirectToAction("AddUserJobs");
        }

        [AllowAnonymous]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Experience()
        {
            try
            {
                ExperienceViewModel experienceModel = (ExperienceViewModel)Session["experienceModel" + Session.SessionID];
                var user = db.Users.Find(experienceModel.userId);
                Interview interview = new Interview()
                {
                    Candidate = user,
                    CandidateId = user.Id,
                    HasBegun = false,
                    IsDone = false,
                    IsGraded = false,
                    Job = db.Jobs.Find(experienceModel.JobId),
                    JobId = experienceModel.JobId,
                    Score = 0
                };
                db.Interviews.Add(interview);
                db.SaveChanges();

                user.CandidateInterviewId = interview.InterviewId;

                Experience exp = new Experience
                {
                    CandidateId = user.Id,
                    Candidate = user
                };
                db.Experiences.Add(exp);
                db.SaveChanges();

                foreach (var ns in experienceModel.newSkills)
                {
                    db.Skills.Add(ns);
                    db.SaveChanges();

                    experienceModel.addedSkills.Add(ns);
                }

                foreach (Skill s in experienceModel.addedSkills)
                {
                    CandidateSkill es = new CandidateSkill
                    {
                        ExperienceId = exp.ExperienceId, 
                        SkillId = s.SkillId
                    };
                    db.CandidateSkills.Add(es);
                }
                db.SaveChanges();
                
                for (int i = 0; i < experienceModel.newEducationStudies.Count(); ++i)
                {
                    Study newStudy = new Study
                    {
                        Degree = experienceModel.newEducationStudies[i].Study.Degree,
                        Specialty = experienceModel.newEducationStudies[i].Study.Specialty
                    };
                    db.Studies.Add(newStudy);

                    experienceModel.newEducationStudies[i].StudyId = newStudy.StudyId;
                }

                experienceModel.userEducation.AddRange(experienceModel.newEducationStudies);
                foreach (var ue in experienceModel.userEducation)
                {
                    Education ed = new Education
                    {
                        ExperienceId = exp.ExperienceId,
                        Institution = ue.Institution,
                        StudyId = ue.StudyId
                    };
                    db.Educations.Add(ed);
                }
                db.SaveChanges();

                foreach (var aj in experienceModel.addedPastJobs)
                {
                    PastJob j = new PastJob
                    {
                        ExperienceId = exp.ExperienceId,
                        Company = aj.Company,
                        Position = aj.Position,
                        StartDate = aj.StartDate,
                        EndDate = aj.EndDate
                    };
                    db.PastJobs.Add(j);
                }
                db.SaveChanges();
            }
            catch (Exception e)
            {
                ViewBag.exceptionMessage = e.Message;
                return View(error);
            }
            Session.Remove("experienceModel" + Session.SessionID);
            TempData["success"] = "Thank you for your subscription. Your profile will be checked soon and we will set you up for an interview.";
            return RedirectToAction("Index", "Home");
        }
        
        [NonAction]
        public void SendMail(string user_email, string subject, string message)
        {
            var senderEmail = new MailAddress("workplace.interview@gmail.com", "Workplace Administrator");
            var receiverEmail = new MailAddress(user_email, "Receiver");
            var password = "testingcheats";
            var sub = subject; //"Workplace Interview";
            var body = message; //"Click <a href=\"" + callbackUrl + "\">here</a> to login." + email_message;
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderEmail.Address, password)
            };
            using (var mess = new MailMessage(senderEmail, receiverEmail)
            {
                Subject = sub,
                Body = body
            })
            {
                smtp.Send(mess);
            }
        }
       
        [CustomAuthorize(Roles = "Administrator")]
        public async Task<ActionResult> Create()
        {
            RegisterViewModel user = new RegisterViewModel();
            try
            {
                user.AllDepartments = user.GetAllDepartments();
                if (!user.AllDepartments.Any())
                {
                    TempData["danger"] = "Try adding a department first.";
                    return RedirectToAction("Create","Departments");
                }
                var isValid = false;
                do
                {
                    user.Password = Membership.GeneratePassword(9, 1);
                    isValid = (await UserManager.PasswordValidator.ValidateAsync(user.Password)).Succeeded;
                }
                while (!isValid);
                user.ConfirmPassword = user.Password;
                //user.UserName = user.Password;

                Session["departments" + Session.SessionID] = new SelectList(db.Departments, "DepartmentId", "Name");
                Session["departmentFilter" + Session.SessionID] = db.Departments.First().Name;
                var users = db.Users.ToList().Except(db.Users.Where(c => c.EmployeeDepartmentId == 0 && c.AdminDepartmentId == 0 && !c.IsActive)).ToList();
                var filter = HttpContext.Request.Params.Get("departmentsFilter");
                if (filter != null)
                {
                    Session["departmentFilter" + Session.SessionID] = db.Departments.Find(int.Parse(filter)).DepartmentId;
                    users = users.Where(u => u.EmployeeDepartmentId == int.Parse(filter) || u.AdminDepartmentId == int.Parse(filter) || (u.CandidateInterviewId != 0 && u.CandidateInterview.Job.DepartmentId == int.Parse(filter))).ToList();
                }
                Session["users" + Session.SessionID] = users;
            }
            catch (Exception)
            {
                return View(error);
            }
            Session["userModel" + Session.SessionID] = user;
            if (Session["url" + Session.SessionID] != null)
            {
                var url = (string)Session["url" + Session.SessionID];
                Session.Remove("url" + Session.SessionID);
                return Redirect(Url.RouteUrl(new { controller = "Users", action = "Index" }) + "#" + url);
            }
            return RedirectToAction("Index", "Users");
            //return View(user);
        }

        
        [HttpPost]
        [CustomAuthorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RegisterViewModel model)
        {
            var goback = false;
            if (string.IsNullOrEmpty(model.FirstName))
            {
                Session["IsEmptyFirstName" + Session.SessionID] = true;
                goback = true;
            }
            if (string.IsNullOrEmpty(model.LastName))
            {
                Session["IsEmptyLastName" + Session.SessionID] = true;
                goback = true;
            }
            if (string.IsNullOrEmpty(model.UserName))
            {
                Session["IsEmptyUserName" + Session.SessionID] = true;
                goback = true;
            }
            else if (!IsValidUsername(model.UserName))
            {
                Session["IsValidUserName" + Session.SessionID] = "The " + model.UserName + " username is already taken.";
                goback = true;
            }
            if (string.IsNullOrEmpty(model.Email))
            {
                Session["IsEmptyEmail" + Session.SessionID] = true;
                goback = true;
            }
            else if (!IsValidEmail(model.Email))
            {
                Session["IsValidEmail" + Session.SessionID] = "The " + model.Email + " email address is already taken.";
                goback = true;
            }
            if (ModelState.IsValid && !goback)
            {
                string email_message = "Your Username is " + model.UserName + " and your password is " + model.Password + ".";
                int newDepartmentId = int.Parse(HttpContext.Request.Params.Get("newDepartmentId"));
                var user = new ApplicationUser { FirstName = model.FirstName, LastName = model.LastName, UserName = model.UserName, Email = model.Email, PhoneNumber = model.PhoneNumber, EmployeeDepartmentId = newDepartmentId, IsActive = false };

                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user.Id, "Employee");

                    var callbackUrl = Url.Action("Login", "Account", "", protocol: Request.Url.Scheme);
                    SendMail(user.Email, "Welcome to Workplace!", "Click <a href=\"" + callbackUrl + "\">here</a> to login." + email_message);

                    await db.SaveChangesAsync();

                    TempData["success"] = "The user has been successfully added.";
                    Session["url" + Session.SessionID] = "employees";
                    return RedirectToAction("Create", "Account");
                }
                AddErrors(result);
            }
            // If we got this far, something failed, redisplay form
            //model.AllDepartments = model.GetAllDepartments();
            Session["oldUserModel" + Session.SessionID] = model;
            Session["url" + Session.SessionID] = "add-employee";
            return RedirectToAction("Create", "Account");
            //return View(model);
        }
        
        [NonAction]
        public bool IsValidDepartmentName(string departmentName)
        {
            return !db.Departments.Any(d => d.Name == departmentName);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateSupervisor(DepartmentViewModel model)
        {
            var goback = false;
            if (!IsValidUsername(model.NewSupervisor.UserName))
            {
                Session["isValidUserName" + Session.SessionID] = "The " + model.NewSupervisor.UserName + " username is already taken.";
                goback = true;
            }
            if (!IsValidEmail(model.NewSupervisor.Email))
            {
                Session["isValidEmail" + Session.SessionID] = "The " + model.NewSupervisor.Email + " email address is already taken.";
                goback = true;
            }
            if(model.EditedDepartmentId != 0)
            {
                if (!new DepartmentsController().IsValidDepartmentName(model.Name, db.Departments.Find(model.EditedDepartmentId).Name))
                {
                    //Session["tabToShow" + Session.SessionID] = model.EditedDepartmentId;
                    Session["IsValidDepartmentName" + Session.SessionID] = "The " + model.Name + " department already exists.";
                    goback = true;
                }
            }
            else if (!IsValidDepartmentName(model.Name))
            {
                Session["isValidDepartmentName" + Session.SessionID] = "The " + model.Name + " department name is already taken.";
                goback = true;
            }
            if (goback)
            {
                Session["oldDepartmentModel" + Session.SessionID] = model;
                Session["url" + Session.SessionID] = "add-department";
                if(model.EditedDepartmentId != 0)
                {
                    Session["url" + Session.SessionID] = "tabs-min-" + model.EditedDepartmentId;
                }
                return RedirectToAction("Create", "Departments");
            }
            try
            {
                string email_message = "Your Username is " + model.NewSupervisor.UserName + " and your password is " + model.NewSupervisor.Password + ".";

                var user = new ApplicationUser { FirstName = model.NewSupervisor.FirstName, LastName = model.NewSupervisor.LastName, UserName = model.NewSupervisor.UserName, Email = model.NewSupervisor.Email, PhoneNumber = model.NewSupervisor.PhoneNumber, IsActive = false };
                var result = UserManager.Create(user, model.NewSupervisor.Password);

                if (result.Succeeded)
                {
                    UserManager.AddToRole(user.Id, "Supervisor");

                    var callbackUrl = Url.Action("Login", "Account", "", protocol: Request.Url.Scheme);
                    new AccountController().SendMail(user.Email, "Welcome to Workplace!", "Click <a href=\"" + callbackUrl + "\">here</a> to login." + email_message);

                    await db.SaveChangesAsync();

                    if (model.EditedDepartmentId != 0)
                    {
                        return RedirectToAction("EditWithNewSupervisor", "Departments", new { departmentId = model.EditedDepartmentId, departmentName = model.Name, adminId = user.Id });
                    }
                    else
                    {
                        return RedirectToAction("CreateWithNewSupervisor", "Departments", new { departmentName = model.Name, adminId = user.Id });
                    }
                    
                }
                
                AddErrors(result);
                return RedirectToAction("Create","Departments");
            }
            catch
            {
                return RedirectToAction("Create", "Departments");
            }
        }


        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}