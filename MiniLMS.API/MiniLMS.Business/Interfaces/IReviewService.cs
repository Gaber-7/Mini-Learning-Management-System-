using MiniLMS.Business.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Business.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<CourseReviewDto>> GetCourseReviewsAsync(int courseId);
        Task<CourseRatingSummaryDto> GetCourseRatingSummaryAsync(int courseId);
        Task<CourseReviewDto> AddOrUpdateReviewAsync(int courseId, int studentId, CreateCourseReviewDto dto);
        Task<bool> DeleteReviewAsync(int reviewId, int userId, string userRole);
    }
}
