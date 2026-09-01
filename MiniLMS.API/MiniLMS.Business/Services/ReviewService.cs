using AutoMapper;
using Microsoft.EntityFrameworkCore;
using GenAlpha.Business.DTOs;
using GenAlpha.Business.Interfaces;
using GenAlpha.Data.Data;
using GenAlpha.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAlpha.Business.Services
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ReviewService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CourseReviewDto>> GetCourseReviewsAsync(int courseId)
        {
            var reviews = await _context.CourseReviews
                .Include(r => r.Student)
                .Where(r => r.CourseId == courseId && r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CourseReviewDto>>(reviews);
        }

        public async Task<CourseRatingSummaryDto> GetCourseRatingSummaryAsync(int courseId)
        {
            var reviews = await _context.CourseReviews
                .Where(r => r.CourseId == courseId && r.IsApproved)
                .ToListAsync();

            var total = reviews.Count;
            var avg = total > 0 ? (decimal)reviews.Average(r => r.Rating) : 0;

            return new CourseRatingSummaryDto
            {
                AverageRating = Math.Round(avg, 2),
                TotalReviews = total,
                FiveStarCount = reviews.Count(r => r.Rating == 5),
                FourStarCount = reviews.Count(r => r.Rating == 4),
                ThreeStarCount = reviews.Count(r => r.Rating == 3),
                TwoStarCount = reviews.Count(r => r.Rating == 2),
                OneStarCount = reviews.Count(r => r.Rating == 1)
            };
        }

        public async Task<CourseReviewDto> AddOrUpdateReviewAsync(int courseId, int studentId, CreateCourseReviewDto dto)
        {
            var existingReview = await _context.CourseReviews
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.CourseId == courseId && r.StudentId == studentId);

            if (existingReview != null)
            {
                existingReview.Rating = dto.Rating;
                existingReview.Comment = dto.Comment;
                existingReview.CreatedAt = DateTime.UtcNow;
                _context.CourseReviews.Update(existingReview);
            }
            else
            {
                existingReview = _mapper.Map<CourseReview>(dto);
                existingReview.CourseId = courseId;
                existingReview.StudentId = studentId;
                existingReview.CreatedAt = DateTime.UtcNow;
                existingReview.IsApproved = true;

                await _context.CourseReviews.AddAsync(existingReview);
            }

            await _context.SaveChangesAsync();

            await UpdateCourseRatingStatsAsync(courseId);

            var student = await _context.Students.FindAsync(studentId);
            existingReview.Student = student!;

            return _mapper.Map<CourseReviewDto>(existingReview);
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, int userId, string userRole)
        {
            var review = await _context.CourseReviews.FindAsync(reviewId);
            if (review == null) return false;

            if (userRole != "Admin" && review.StudentId != userId)
            {
                return false;
            }

            var courseId = review.CourseId;
            _context.CourseReviews.Remove(review);
            await _context.SaveChangesAsync();

            await UpdateCourseRatingStatsAsync(courseId);
            return true;
        }

        private async Task UpdateCourseRatingStatsAsync(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course != null)
            {
                var reviews = await _context.CourseReviews.Where(r => r.CourseId == courseId && r.IsApproved).ToListAsync();
                course.ReviewsCount = reviews.Count;
                course.AverageRating = reviews.Any() ? Math.Round((decimal)reviews.Average(r => r.Rating), 2) : 0;
                _context.Courses.Update(course);
                await _context.SaveChangesAsync();
            }
        }
    }
}
