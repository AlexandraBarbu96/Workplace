using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Workplace_v2._2.Models
{
    public class JobStudy
    {
        [Key]
        public int JobStudyId { get; set; }

        public int JobId { get; set; }
        public virtual Job Job { get; set; }

        public int StudyId { get; set; }
        public virtual Study Study { get; set; }
    }
}