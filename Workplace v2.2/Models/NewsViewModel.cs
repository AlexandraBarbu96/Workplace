using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Workplace_v2._2.Models
{
    public class NewsViewModel
    {
        public List<Question> Requests { get; set; }

        public List<Interview> Interviews { get; set; }

        public List<ApplicationUser> Candidates { get; set; }
        public List<Experience> Experiences { get; set; }
    }
}