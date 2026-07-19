using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Data.Models
{
    public class Course
    {
        public int Id { get; set; } 

        [Required]
        [StringLength(150)]
        public string Title { get; set; } 

        [Required]
        public string Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; }

        public bool IsPublished { get; set; } 
       public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>(); 
    }
}
