using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Business.DTOs
{
    public class StudentListItemDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TotalEnrollments { get; set; }
        public int CompletedCourses { get; set; }
        public int InProgressCourses { get; set; }
        public List<EnrollmentDto> Enrollments { get; set; } = new();
    }

    public class InstructorListItemDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
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
        public int PublishedCoursesCount { get; set; }
        public int TotalStudentsCount { get; set; }
    }
}
