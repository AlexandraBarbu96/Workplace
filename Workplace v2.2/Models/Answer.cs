using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Workplace_v2._2.Models
{
    public class Answer
    {
        [Key]
        public int AnswerId { get; set; }

        public string CandidateId { get; set; }
        public virtual ApplicationUser Candidate { get; set; }

        public int QuestionId { get; set; }
        public virtual Question Question { get; set; }

        [AllowHtml]
        public string Text { get; set; }

        [Range(0, 10, ErrorMessage = "Value must be between {1} and {2}.")]
        public int Score { get; set; }
    }
}