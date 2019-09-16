using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Workplace_v2._2.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }
        [Required]
        public string Name { get; set; }

        public string AdminId { get; set; }
        public virtual ApplicationUser Admin { get; set; }

        public virtual ICollection<Job> Jobs { get; set; }
        public virtual ICollection<ApplicationUser> Employees { get; set; }
    }

    public class DepartmentViewModel
    {
        [Required]
        public string Name { get; set; }

        public int EditedDepartmentId { get; set; }
        public string AdminId { get; set; }
        public List<SelectListItem> Employees { get; set; }

        public RegisterViewModel NewSupervisor { get; set; }

        public List<SelectListItem> GetAllEmployees()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var selectList = new List<SelectListItem>();
            foreach (var user in db.Users.ToList())
            {
                if (!user.IsInRole("Supervisor") && !user.IsInRole("Candidate"))
                {
                    selectList.Add(new SelectListItem
                    {
                        Value = user.Id,
                        Text = user.UserName
                    });
                }
            }
            return selectList;
        }
    }
}
