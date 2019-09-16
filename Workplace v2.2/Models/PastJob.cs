using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Workplace_v2._2.Models
{
    public class PastJob
    {
        [Key]
        public int PastJobId { get; set; }

        public string Company { get; set; }
        public string Position { get; set; }

        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        public int ExperienceId { get; set; }
        public virtual Experience Experience { get; set; }

    }
}