using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAlpha.Data.Models
{
    public class Instructor
    {
        [Key, ForeignKey("User")]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Headline { get; set; }

        public string? Bio { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public string? WebsiteUrl { get; set; }

        public string? LinkedInUrl { get; set; }

        public string? GitHubUrl { get; set; }

        public string? YouTubeUrl { get; set; }

        public virtual User User { get; set; } = null!;

        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
