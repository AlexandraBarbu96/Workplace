using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Workplace_v2._2.Models
{
    public class JobSkill
    {
        [Key]
        public int JobSkillId { get; set; }

        public int JobId { get; set; }
        public virtual Job Job { get; set; }

        public int SkillId { get; set; }
        public virtual Skill Skill { get; set; }
    }
}