using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAlpha.Business.DTOs
{
    public class UpdateInstructorProfileDto
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Headline { get; set; }

        public string? Bio { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public string? WebsiteUrl { get; set; }

        public string? LinkedInUrl { get; set; }

        public string? GitHubUrl { get; set; }

        public string? YouTubeUrl { get; set; }
    }
}
