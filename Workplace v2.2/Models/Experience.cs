using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Workplace_v2._2.Models
{
    public class Experience
    {
        [Key]
        public int ExperienceId { get; set; }

        public virtual ICollection<Education> Educations { get; set; }
        public virtual ICollection<PastJob> PastJobs { get; set; }
        public virtual ICollection<CandidateSkill> CandidateSkills { get; set; }

        [Required]
        public string CandidateId { get; set; }
        [Required]
        public virtual ApplicationUser Candidate { get; set; }
    }

    public class ExperienceViewModel
    {
        public string userId { get; set; }
        public RegisterViewModel Candidate { get; set; }
        public int JobId { get; set; }

        public List<Skill> addedSkills { get; set; }
        public List<Skill> skillsToAdd { get; set; }
        public List<Skill> newSkills { get; set; }

        public List<Education> userEducation { get; set; }
        public List<Education> newEducationStudies { get; set; }

        public List<PastJob> addedPastJobs { get; set; }
    }
}