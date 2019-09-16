using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Workplace_v2._2.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }

        public IEnumerable<SelectListItem> AllRoles { get; set; }
        public IEnumerable<SelectListItem> AllDepartments { get; set; }

        public int CandidateInterviewId { get; set; }
        public virtual Interview CandidateInterview { get; set; }

        public int AdminDepartmentId { get; set; }
        public virtual Department AdminDepartment { get; set; }

        public int EmployeeDepartmentId { get; set; }
        public virtual Department EmployeeDepartment { get; set; }

        public virtual ICollection<Answer> CandidateAnswers { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        public IEnumerable<SelectListItem> GetAllRoles()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var selectList = new List<SelectListItem>();
            foreach (var role in db.Roles)
            {
                //if(role.Name != "Administrator")
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }

        public IEnumerable<SelectListItem> GetAllDepartments()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var selectList = new List<SelectListItem>();
            foreach (var dep in db.Departments)
            {
                selectList.Add(new SelectListItem
                {
                    Value = dep.DepartmentId.ToString(),
                    Text = dep.Name.ToString()
                });
            }
            return selectList;
        }

        public bool IsInRole(string role)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            if (db.Roles.Find(this.Roles.FirstOrDefault().RoleId).Name == role)
                return true;
            return false;
        }

    }


    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("Workplace", throwIfV1Schema: false)
        {
            //Database.SetInitializer<ApplicationDbContext>(null);
        }

        public DbSet<Department> Departments { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Interview> Interviews { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<PastJob> PastJobs { get; set; }
        public DbSet<Experience> Experiences { get; set; }
        public DbSet<Education> Educations { get; set; }
        public DbSet<Study> Studies { get; set; }
        public DbSet<JobStudy> JobStudies { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<JobSkill> JobSkills { get; set; }
        public DbSet<CandidateSkill> CandidateSkills { get; set; }

        public IEnumerable ApplicationUsers { get; internal set; }
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }

    public class CandidateViewModel
    {
        public RegisterViewModel Candidate { get; set; }
        public int JobId { get; set; }

        public string newSkillName { get; set; }
        public List<Skill> addedSkills { get; set; }
        public List<Skill> skillsToAdd { get; set; }
        public List<Skill> newSkills { get; set; }
        
        public EducationViewModel educationModel { get; set; }
        public List<Education> userEducation { get; set; }
        public List<Education> newEducationStudies { get; set; }

        public PastJob pastJob { get; set; }
        public List<PastJob> addedPastJobs { get; set; }
    }
}