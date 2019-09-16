using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Workplace_v2._2.Models
{
    public class Education
    {
        [Key]
        public int EducationId { get; set; }
       
        public string Institution {get;set;}

        public int StudyId { get; set; }
        public virtual Study Study { get; set; }

        public int ExperienceId { get; set; }
        public virtual Experience Experience { get; set; }
    }

    public class EducationViewModel
    {
        [Required]
        [Display(Name = "Institution Name")]
        public string InstitutionName { get; set; }
        public Study Study { get; set; }
        public string newDegree { get; set; }
        public string newSpecialty { get; set; }
        
        public IEnumerable<string> GetAllDegrees()
        {
            var selectList = new List<string>();

            selectList.Add("HighSchool");
            selectList.Add("Associate's");
            selectList.Add("Bachelor's");
            selectList.Add("Master's");
            selectList.Add("Doctorate");
            selectList.Add("Professional");
            selectList.Add("Specialist");

            return selectList;
        }

        public IEnumerable<SelectListItem> GetAllStudies()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var selectList = new SelectList(db.Studies, "StudyId", "Specialty");
            return selectList;
        }
    }
}