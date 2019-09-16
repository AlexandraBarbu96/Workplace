using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Workplace_v2._2.Models
{
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }
        [Required]
        public string Text { get; set; }
        public bool Approved { get; set; }

        public int JobId { get; set; }
        public virtual Job Job { get; set; }
        
        public string CreatorId { get; set; }
        public virtual ApplicationUser Creator { get; set; }

        public virtual ICollection<Answer> Answers { get; set; }
    }

    public class ApplicationsQuestionModelEqualityComparer : IEqualityComparer<Question>
    {
        public bool Equals(Question x, Question y)
        {
            return x.QuestionId == y.QuestionId;
        }

        public int GetHashCode(Question obj)
        {
            int hashCode = (obj.QuestionId != 0 ? obj.QuestionId.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ obj.QuestionId.GetHashCode();
            return hashCode;
        }
    }
}