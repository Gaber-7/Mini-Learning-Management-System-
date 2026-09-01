using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAlpha.Business.DTOs
{
    public class CourseReviewDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int Rating { get; set; } = 5;
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsApproved { get; set; }
    }

    public class CreateCourseReviewDto
    {
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; } = 5;

        [StringLength(1000)]
        public string? Comment { get; set; }
    }

    public class CourseRatingSummaryDto
    {
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }
    }
}
