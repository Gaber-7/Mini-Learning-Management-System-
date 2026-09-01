using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAlpha.Data.Models
{
    public class Section
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        public int OrderIndex { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

        public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}
