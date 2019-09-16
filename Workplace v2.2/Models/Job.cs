using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Workplace_v2._2.Models
{
    public class Job
    {
        [Key]
        public int JobId { get; set; }
        [Required]
        public string Title { get; set; }
        [AllowHtml]
        public string Description { get; set; }
        [Required]
        public string YearsOfExperience { get; set; }
        public bool IsAvailable { get; set; }
        [Range(0, 100, ErrorMessage = "Value must be between {1} and {2}.")]
        public int MinimumScore { get; set; }

        public int DepartmentId { get; set; }
        public virtual Department Department { get; set; }

        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<JobStudy> JobStudies { get; set; }
        public virtual ICollection<JobSkill> JobSkills { get; set; }
        public virtual ICollection<Interview> Interviews { get; set; }
    }

    public class JobViewModel
    {
        public int JobId { get; set; }
        [Required]
        [Display(Name = "Job Title")]
        public string Title { get; set; }
        [AllowHtml]
        public string Description { get; set; }
        [Required]
        public string YearsOfExperience { get; set; }
        [Range(0, 100, ErrorMessage = "Value must be between {1} and {2}.")]
        public int MinimumScore { get; set; }
        [Required]
        public int DepartmentId { get; set; }

        public List<Skill> addedSkills { get; set; }
        public List<Skill> skillsToAdd { get; set; }
        public List<Skill> newSkills { get; set; }
        public string newSkillName { get; set; }

        public List<Study> addedStudies { get; set; }
        public List<Study> studiesToAdd { get; set; }
        public List<Study> newStudies { get; set; }
        public string newStudyDegree { get; set; }
        public string newStudySpecialty { get; set; }

        public bool isEdit { get; set; }

        public IEnumerable<string> GetAllYears()
        {
            var selectList = new List<string>();
            
            selectList.Add("none");
            selectList.Add("1-2 year(s) (Junior)");
            selectList.Add("3-5 years");
            selectList.Add("5 years or more (Senior)");
            return selectList;
        }
    }
}