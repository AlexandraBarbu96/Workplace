using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Workplace_v2._2.Attributes;
using Workplace_v2._2.Models;

namespace Workplace_v2._2.Controllers
{
    [CustomAuthorize(Roles = "Administrator,Employee,Supervisor")]
    public class DepartmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly string error = "~/Views/Shared/Error.cshtml";

        private ApplicationUserManager _userManager;
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

        [CustomAuthorize(Roles = "Administrator")]
        public ActionResult Index()
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

                if (Session["departmentModel" + Session.SessionID] == null)
                {
                    throw new Exception();
                }
                if (Session["isValidUserName" + Session.SessionID] != null)
                {
                    ModelState.AddModelError("NewSupervisor.UserName", (string)Session["isValidUserName" + Session.SessionID]);
                    Session.Remove("IsValidUserName" + Session.SessionID);
                }
                if (Session["isValidEmail" + Session.SessionID] != null)
                {
                    ModelState.AddModelError("NewSupervisor.Email", (string)Session["isValidEmail" + Session.SessionID]);
                    Session.Remove("IsValidEmail" + Session.SessionID);
                }
                if (Session["isValidDepartmentName" + Session.SessionID] != null)
                {
                    ModelState.AddModelError("Name", (string)Session["isValidDepartmentName" + Session.SessionID]);
                    Session.Remove("isValidDepartmentName" + Session.SessionID);
                }
                if (Session["IsEmptySkill" + Session.SessionID] != null)
                {
                    ModelState.AddModelError("Name", "Enter a name.");
                    Session.Remove("IsEmptySkill" + Session.SessionID);
                }
                if (Session["isValidSkill" + Session.SessionID] != null)
                {
                    ModelState.AddModelError("Name", (string)Session["isValidSkill" + Session.SessionID]);
                    Session.Remove("isValidSkill" + Session.SessionID);
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
                if (Session["isValidSpecialty" + Session.SessionID] != null)
                {
                    ModelState.AddModelError("Degree", (string)Session["isValidSpecialty" + Session.SessionID]);
                    Session.Remove("isValidSpecialty" + Session.SessionID);
                }
                return View(db.Departments.ToList());
            }
            catch
            {
                return View(error);
            }
        }

        public ActionResult Details(int? id)
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
                var user = db.Users.Find(User.Identity.GetUserId());
                if (id == null && User.IsInRole("Administrator"))
                {
                    throw new ArgumentNullException();
                }
                else if (id == null)
                {
                    id = user.AdminDepartmentId;
                }
                Department department = db.Departments.Find(id);
                if (!User.IsInRole("Administrator"))
                {
                    if (department.AdminId != user.Id && department.DepartmentId != user.EmployeeDepartmentId)
                        throw new Exception();
                }
                
                Session.RemoveAll();
                return View(department);
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

        [CustomAuthorize(Roles = "Administrator")]
        public async System.Threading.Tasks.Task<ActionResult> Create()
        {
            if (TempData.ContainsKey("danger"))
            {
                ViewBag.message = TempData["danger"].ToString();
            }
            
            DepartmentViewModel department = new DepartmentViewModel();
            department.EditedDepartmentId = 0;
            department.Employees = department.GetAllEmployees();

            RegisterViewModel newSupervisor = new RegisterViewModel();
            var isValid = false;
            do
            {
                newSupervisor.Password = Membership.GeneratePassword(9, 1);
                isValid = (await UserManager.PasswordValidator.ValidateAsync(newSupervisor.Password)).Succeeded;
            }
            while (!isValid);
            newSupervisor.ConfirmPassword = newSupervisor.Password;
            //newSupervisor.UserName = newSupervisor.Password;
            department.NewSupervisor = newSupervisor;

            //Session.RemoveAll();
            Session["departmentModel" + Session.SessionID] = department;
            if(Session["url" + Session.SessionID] != null)
            {
                string url = (string)Session["url" + Session.SessionID];
                Session.Remove("url" + Session.SessionID);
                return Redirect(Url.RouteUrl(new { controller = "Departments", action = "Index" }) + "#" + url);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,AdminId")]DepartmentViewModel department)
        {
            if (ModelState.IsValid && new AccountController().IsValidDepartmentName(department.Name))
            {
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                try
                {
                    Department departmentToAdd = new Department();
                    departmentToAdd.Name = department.Name;
                    departmentToAdd.AdminId = department.AdminId;

                    db.Departments.Add(departmentToAdd);
                    db.SaveChanges();

                    ApplicationUser admin = db.Users.Find(department.AdminId);
                    if (admin.IsInRole("Employee"))
                    {
                        UserManager.RemoveFromRole(admin.Id, "Employee");
                        UserManager.AddToRole(admin.Id, "Supervisor");

                        admin.AdminDepartmentId = departmentToAdd.DepartmentId;
                        db.SaveChanges();
                    }

                    Session.RemoveAll();
                    TempData["success"] = "The department has been successfully added.";
                    return RedirectToAction("Create");
                }
                catch (Exception e)
                {
                    TempData["danger"] = e.Message;
                    return RedirectToAction("Create");
                }
            }
            if(!new AccountController().IsValidDepartmentName(department.Name))
            {
                Session["isValidDepartmentName" + Session.SessionID] = "The " + department.Name + " department name is already taken.";
                department.NewSupervisor = new RegisterViewModel();
                department.Employees = department.GetAllEmployees();
                Session["oldDepartmentModel" + Session.SessionID] = department;
                Session["url" + Session.SessionID] = "add-department";
            }
            return RedirectToAction("Create");
        }
        
        public ActionResult CreateWithNewSupervisor(string departmentName, string adminId)
        {
            try
            {
                if (departmentName == null || adminId == null)
                {
                    throw new ArgumentNullException();
                }
                Department departmentToAdd = new Department();
                ApplicationUser admin = db.Users.Find(adminId);
                if (!admin.IsInRole("Supervisor"))
                {
                    throw new Exception();
                }
                departmentToAdd.Name = departmentName;
                departmentToAdd.AdminId = adminId;

                db.Departments.Add(departmentToAdd);
                db.SaveChanges();
                
                admin.AdminDepartmentId = departmentToAdd.DepartmentId;
                db.SaveChanges();

                TempData["success"] = "The department has been successfully added.";
                return RedirectToAction("Create");
            }
            catch (ArgumentNullException)
            {
                ViewBag.exceptionMessage = "The path you've accessed wasn't correct.";
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

        [NonAction]
        public bool IsValidDepartmentName(string newName, string oldName)
        {
            return !(db.Departments.Any(d => d.Name == newName) && newName != oldName);
        }

        [HttpPut]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EditedDepartmentId,Name,AdminId")]DepartmentViewModel department)
        {
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new Exception();
                }
                if (!IsValidDepartmentName(department.Name, db.Departments.Find(department.EditedDepartmentId).Name))
                {
                    Session["IsValidDepartmentName" + Session.SessionID] = "The " + department.Name + " department already exists.";
                    Session["url" + Session.SessionID] = "tabs-min-" + department.EditedDepartmentId;
                    return RedirectToAction("Create");
                }
                Department departmentToUpdate = db.Departments.Find(department.EditedDepartmentId);
                var oldAdminId = departmentToUpdate.AdminId;
                departmentToUpdate.Name = department.Name;
                departmentToUpdate.AdminId = department.AdminId;

                db.SaveChanges();

                if (oldAdminId != department.AdminId && !db.Users.Find(oldAdminId).IsInRole("Administrator"))
                {
                    var user = db.Users.Find(oldAdminId);
                    UserManager.RemoveFromRole(user.Id, "Supervisor");
                    UserManager.AddToRole(user.Id, "Employee");
                    user.AdminDepartmentId = 0;
                    user.EmployeeDepartmentId = departmentToUpdate.DepartmentId;

                    ApplicationUser admin = db.Users.Find(department.AdminId);
                    if (admin.IsInRole("Employee"))
                    {
                        UserManager.RemoveFromRole(admin.Id, "Employee");
                        UserManager.AddToRole(admin.Id, "Supervisor");

                        admin.AdminDepartmentId = departmentToUpdate.DepartmentId;
                    }
                }
                db.SaveChanges();

                TempData["success"] = "The department has been successfully updated.";
                return RedirectToAction("Create");
            }
            catch (NullReferenceException)
            {
                ViewBag.exceptionMessage = "The department doesn't exist.";
            }
            catch(Exception)
            {
                ViewBag.exceptionMessage = "You can't access this page.";
            }
            return View(error);
        }
        
        public ActionResult EditWithNewSupervisor(int departmentId, string departmentName, string adminId)
        {
            try
            {
                if (departmentId == 0 || departmentName == null || adminId == null)
                {
                    throw new ArgumentNullException();
                }
                Department departmentToUpdate = db.Departments.Find(departmentId);
                departmentToUpdate.Name = departmentName;
                var oldAdminId = departmentToUpdate.AdminId;
                departmentToUpdate.AdminId = adminId;
                
                db.SaveChanges();
                if (!db.Users.Find(oldAdminId).IsInRole("Administrator"))
                {
                    var user = db.Users.Find(oldAdminId);
                    UserManager.RemoveFromRole(user.Id, "Supervisor");
                    UserManager.AddToRole(user.Id, "Employee");
                    user.AdminDepartmentId = 0;
                    user.EmployeeDepartmentId = departmentToUpdate.DepartmentId;
                }

                ApplicationUser admin = db.Users.Find(adminId);
                admin.AdminDepartmentId = departmentToUpdate.DepartmentId;

                db.SaveChanges();
                TempData["success"] = "The department has been successfully updated.";
                return RedirectToAction("Create");
            }
            catch (ArgumentNullException)
            {
                ViewBag.exceptionMessage = "The path you've accessed wasn't correct.";
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

        [NonAction]
        public bool IsValidSkill(string skillName)
        {
            return !db.Skills.Any(s => s.Name == skillName);
        }

        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddSkill([Bind(Include = "Name")]Skill newSkill)
        {
            if (string.IsNullOrEmpty(newSkill.Name))
            {
                Session["IsEmptySkill" + Session.SessionID] = true;
            }
            else if (!IsValidSkill(newSkill.Name))
            {
                Session["IsValidSkill" + Session.SessionID] = "The '" + newSkill.Name + "' skill already exists.";
            }
            else
            {
                try
                {
                    var found = false;
                    foreach (var skill in db.Skills.ToList())
                    {
                        if (skill.Name == newSkill.Name)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        db.Skills.Add(newSkill);
                        db.SaveChanges();

                        TempData["success"] = "The skill has been successfully added.";
                    }
                    else
                    {
                        TempData["danger"] = "You can't add the same skill twice.";
                    }
                }
                catch (Exception)
                {
                    return View(error);
                }
            }
            Session["skillModel" + Session.SessionID] = newSkill;
            Session["url" + Session.SessionID] = "skills";
            return RedirectToAction("Create");
        }

        [CustomAuthorize(Roles = "Administrator")]
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveSkill(int id)
        {
            try
            {
                var skillToBeDeleted = db.Skills.Find(id);
                db.Skills.Remove(skillToBeDeleted);
                db.SaveChanges();

                TempData["success"] = "The skill has been successfully deleted.";
                Session["url" + Session.SessionID] = "skills";
                return RedirectToAction("Create");
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

        [NonAction]
        public bool IsValidSpecialty(Study study)
        {
            return !db.Studies.Any(s => s.Degree == study.Degree && s.Specialty == study.Specialty);
        }

        [CustomAuthorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddStudy([Bind(Include = "Degree,Specialty")]Study newStudy)
        {
            var goback = false;
            if (string.IsNullOrEmpty(newStudy.Degree))
            {
                Session["IsEmptyDegree" + Session.SessionID] = true;
                goback = true;
            }
            if (string.IsNullOrEmpty(newStudy.Specialty))
            {
                Session["IsEmptySpecialty" + Session.SessionID] = true;
                goback = true;
            }
            else if(!IsValidSpecialty(newStudy))
            {
                Session["isValidSpecialty" + Session.SessionID] = "The '" + newStudy.Specialty + "' study already exists.";
                goback = true;
            }
            if (!goback)
            {
                try
                {
                    var found = false;
                    foreach (var study in db.Studies.ToList())
                    {
                        if (study.Degree == newStudy.Degree && study.Specialty == newStudy.Specialty)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        db.Studies.Add(newStudy);
                        db.SaveChanges();

                        TempData["success"] = "The study has been successfully added.";
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
            Session["studyModel" + Session.SessionID] = newStudy;
            Session["url" + Session.SessionID] = "studies";
            return RedirectToAction("Create");
        }

        [CustomAuthorize(Roles = "Administrator")]
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveStudy(int id)
        {
            try
            {
                var studyToBeDeleted = db.Studies.Find(id);
                db.Studies.Remove(studyToBeDeleted);
                db.SaveChanges();

                TempData["success"] = "The study has been successfully deleted.";
                Session["url" + Session.SessionID] = "studies";
                return RedirectToAction("Create");
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

        [CustomAuthorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        [HttpDelete]
        public ActionResult Delete(int id)
        {
            try
            {
                Department department = db.Departments.Find(id);
                var canDelete = true;
                foreach (var job in db.Jobs.Where(j => j.DepartmentId == id).ToList())
                {
                    var activeCandidates = db.Users.Where(c => c.CandidateInterview.Job.DepartmentId == id && c.IsActive); 
                    if (activeCandidates.Count() != 0)
                    {
                        TempData["danger"] = "You can't delete the department while there are active candidates for some jobs. Try de-activating them first.";
                        canDelete = false;
                    }
                }
                if (canDelete)
                {
                    foreach (var employee in db.Users.Where(e => e.EmployeeDepartmentId == id).ToList())
                    {
                        new UsersController().Delete(employee.Id, Url.Action("Delete", "Departments", new { id }));
                    }
                    foreach (var job in db.Jobs.Where(j => j.DepartmentId == id).ToList())
                    {
                        var jobCandidates = db.Users.Where(c => c.CandidateInterview.JobId == job.JobId).ToList();

                        db.Jobs.Remove(job);
                        db.SaveChanges();

                        foreach (var candidate in jobCandidates)
                        {
                            new UsersController().Delete(candidate.Id, Url.Action("Delete", "Jobs", new { id }));
                        }
                    }
                    var adminId = department.AdminId;
                    
                    db.Departments.Remove(department);
                    db.SaveChanges();

                    if (!db.Users.Find(adminId).IsInRole("Administrator"))
                        new UsersController().Delete(adminId, Url.Action("Delete", "Departments", new { id }));
                    TempData["success"] = "The department has been successfully deleted.";
                }
                
                return RedirectToAction("Index");
            }
            catch (NullReferenceException)
            {
                ViewBag.exceptionMessage = "The department doesn't exist.";
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
