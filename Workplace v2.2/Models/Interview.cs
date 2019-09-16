using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Workplace_v2._2.Models
{
    public class Interview
    {
        [Key]
        public int InterviewId { get; set; }

        public bool IsDone { get; set; }
        public bool IsGraded { get; set; }
        public bool HasBegun { get; set; }
        public float Score { get; set; }

        public int JobId { get; set; }
        public virtual Job Job { get; set; }

        [Required]
        public string CandidateId { get; set; }
        [Required]
        public virtual ApplicationUser Candidate { get; set; }
    }

    public class InterviewViewModel
    {
        public int InterviewId { get; set; }

        public Answer Answer1 { get; set; }
        public Answer Answer2 { get; set; }
        public Answer Answer3 { get; set; }
        public Answer Answer4 { get; set; }
        public Answer Answer5 { get; set; }
        public Answer Answer6 { get; set; }
        public Answer Answer7 { get; set; }
        public Answer Answer8 { get; set; }
        public Answer Answer9 { get; set; }
        public Answer Answer10 { get; set; }
    }
}