using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Workplace_v2._2.Models
{
    public class Skill
    {
        [Key]
        public int SkillId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<JobSkill> JobSkills { get; set; }
        public virtual ICollection<CandidateSkill> CandidateSkills { get; set; }
    }

    public class ApplicationsSkillModelEqualityComparer : IEqualityComparer<Skill>
    {
        public bool Equals(Skill x, Skill y)
        {
            return x.SkillId == y.SkillId;
        }

        public int GetHashCode(Skill obj)
        {
            int hashCode = (obj.SkillId != 0 ? obj.SkillId.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ obj.SkillId.GetHashCode();
            return hashCode;
        }
    }

    public class SkillViewModel
    {
        public IEnumerable<Skill> addedSkills { get; set; }
        public IEnumerable<Skill> skillsToAdd { get; set; }
        public IEnumerable<Skill> newSkills { get; set; }
        public int newSkillId { get; set; }
    }
}