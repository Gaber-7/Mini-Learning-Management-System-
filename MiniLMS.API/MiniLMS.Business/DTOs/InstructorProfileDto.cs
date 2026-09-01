using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAlpha.Business.DTOs
{
    public class InstructorProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Headline { get; set; }
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? GitHubUrl { get; set; }
        public string? YouTubeUrl { get; set; }
        public int TotalCourses { get; set; }
        public int TotalStudents { get; set; }
        public List<CourseDto> Courses { get; set; } = new();
    }
}
