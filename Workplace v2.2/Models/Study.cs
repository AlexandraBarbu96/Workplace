using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Workplace_v2._2.Models
{

    public class Study
    {
        public int StudyId { get; set; }

        public string Degree { get; set; }
        public string Specialty { get; set; }

        public virtual ICollection<Education> Educations { get; set; }

        public virtual ICollection<JobStudy> JobStudies { get; set; }
    }

    public class ApplicationsStudyModelEqualityComparer : IEqualityComparer<Study>
    {
        public bool Equals(Study x, Study y)
        {
            return x.StudyId == y.StudyId;
        }

        public int GetHashCode(Study obj)
        {
            int hashCode = (obj.StudyId != 0 ? obj.StudyId.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ obj.StudyId.GetHashCode();
            return hashCode;
        }
    }

    public class StudyViewModel
    {
        public List<Study> addedStudies { get; set; }
        public List<Study> studiesToAdd { get; set; }
        public List<Study> newStudies { get; set; }
    }
}