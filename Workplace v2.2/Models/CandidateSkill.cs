using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Workplace_v2._2.Models
{
    public class CandidateSkill
    {
        [Key]
        public int CandidateSkillId { get; set; }

        public int ExperienceId { get; set; }
        public virtual Experience Experience { get; set; }

        public int SkillId { get; set; }
        public virtual Skill Skill { get; set; }
    }
}