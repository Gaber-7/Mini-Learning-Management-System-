using AutoMapper;
using Microsoft.AspNetCore.Identity;
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
    public class AdminUserService : IAdminUserService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AdminUserService(AppDbContext context, IMapper mapper, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
        }

        // ==================== STUDENTS ====================

        public async Task<IEnumerable<StudentListItemDto>> GetAllStudentsAsync()
        {
            var students = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .OrderByDescending(s => s.Id)
                .ToListAsync();

            return _mapper.Map<IEnumerable<StudentListItemDto>>(students);
        }

        public async Task<StudentListItemDto?> GetStudentByIdAsync(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            return student == null ? null : _mapper.Map<StudentListItemDto>(student);
        }

        public async Task<StudentListItemDto> CreateStudentAsync(CreateStudentDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username) ||
                await _context.Students.AnyAsync(s => s.Email == dto.Email))
            {
                throw new InvalidOperationException("Username or Email is already taken.");
            }

            var user = new User
            {
                Username = dto.Username,
                Role = "Student"
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var student = new Student
            {
                Id = user.Id,
                FullName = dto.FullName,
                Email = dto.Email
            };

            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();

            return (await GetStudentByIdAsync(student.Id))!;
        }

        public async Task<bool> UpdateStudentAsync(int studentId, UpdateStudentDto dto)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return false;

            student.FullName = dto.FullName;
            student.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                student.User.PasswordHash = _passwordHasher.HashPassword(student.User, dto.Password);
                _context.Users.Update(student.User);
            }

            _context.Students.Update(student);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteStudentAsync(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.Enrollments)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return false;

            // Remove lesson progresses and enrollments first
            var enrollmentIds = student.Enrollments.Select(e => e.Id).ToList();
            var progresses = await _context.LessonProgresses.Where(lp => enrollmentIds.Contains(lp.EnrollmentId)).ToListAsync();
            _context.LessonProgresses.RemoveRange(progresses);
            _context.Enrollments.RemoveRange(student.Enrollments);

            // Remove submissions and attempts
            var submissions = await _context.AssignmentSubmissions.Where(a => a.StudentId == studentId).ToListAsync();
            _context.AssignmentSubmissions.RemoveRange(submissions);

            var attempts = await _context.QuizAttempts.Where(qa => qa.StudentId == studentId).ToListAsync();
            _context.QuizAttempts.RemoveRange(attempts);

            var reviews = await _context.CourseReviews.Where(cr => cr.StudentId == studentId).ToListAsync();
            _context.CourseReviews.RemoveRange(reviews);

            _context.Students.Remove(student);
            if (student.User != null)
            {
                _context.Users.Remove(student.User);
            }

            return await _context.SaveChangesAsync() > 0;
        }

        // ==================== INSTRUCTORS ====================

        public async Task<IEnumerable<InstructorListItemDto>> GetAllInstructorsAsync()
        {
            var instructors = await _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Courses)
                .OrderByDescending(i => i.Id)
                .ToListAsync();

            var list = _mapper.Map<List<InstructorListItemDto>>(instructors);

            foreach (var inst in list)
            {
                var courseIds = instructors.First(i => i.Id == inst.Id).Courses.Select(c => c.Id).ToList();
                inst.TotalStudentsCount = await _context.Enrollments.CountAsync(e => courseIds.Contains(e.CourseId));
            }

            return list;
        }

        public async Task<InstructorListItemDto?> GetInstructorByIdAsync(int instructorId)
        {
            var instructor = await _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Courses)
                .FirstOrDefaultAsync(i => i.Id == instructorId);

            if (instructor == null) return null;

            var dto = _mapper.Map<InstructorListItemDto>(instructor);
            var courseIds = instructor.Courses.Select(c => c.Id).ToList();
            dto.TotalStudentsCount = await _context.Enrollments.CountAsync(e => courseIds.Contains(e.CourseId));

            return dto;
        }

        public async Task<InstructorListItemDto> CreateInstructorAsync(CreateInstructorDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username) ||
                await _context.Instructors.AnyAsync(i => i.Email == dto.Email))
            {
                throw new InvalidOperationException("Username or Email is already taken.");
            }

            var user = new User
            {
                Username = dto.Username,
                Role = "Instructor"
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var instructor = new Instructor
            {
                Id = user.Id,
                FullName = dto.FullName,
                Email = dto.Email,
                Headline = dto.Headline,
                Bio = dto.Bio,
                ProfilePictureUrl = dto.ProfilePictureUrl,
                WebsiteUrl = dto.WebsiteUrl,
                LinkedInUrl = dto.LinkedInUrl,
                GitHubUrl = dto.GitHubUrl,
                YouTubeUrl = dto.YouTubeUrl
            };

            await _context.Instructors.AddAsync(instructor);
            await _context.SaveChangesAsync();

            return (await GetInstructorByIdAsync(instructor.Id))!;
        }

        public async Task<bool> UpdateInstructorAsync(int instructorId, UpdateInstructorAdminDto dto)
        {
            var instructor = await _context.Instructors
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.Id == instructorId);

            if (instructor == null) return false;

            instructor.FullName = dto.FullName;
            instructor.Email = dto.Email;
            instructor.Headline = dto.Headline;
            instructor.Bio = dto.Bio;
            instructor.ProfilePictureUrl = dto.ProfilePictureUrl;
            instructor.WebsiteUrl = dto.WebsiteUrl;
            instructor.LinkedInUrl = dto.LinkedInUrl;
            instructor.GitHubUrl = dto.GitHubUrl;
            instructor.YouTubeUrl = dto.YouTubeUrl;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                instructor.User.PasswordHash = _passwordHasher.HashPassword(instructor.User, dto.Password);
                _context.Users.Update(instructor.User);
            }

            _context.Instructors.Update(instructor);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteInstructorAsync(int instructorId)
        {
            var instructor = await _context.Instructors
                .Include(i => i.Courses)
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.Id == instructorId);

            if (instructor == null) return false;

            // Reassign or keep course instructor reference null
            foreach (var course in instructor.Courses)
            {
                course.InstructorId = null;
            }

            _context.Instructors.Remove(instructor);
            if (instructor.User != null)
            {
                _context.Users.Remove(instructor.User);
            }

            return await _context.SaveChangesAsync() > 0;
        }

        // ==================== REVIEWS ====================

        public async Task<IEnumerable<AdminReviewItemDto>> GetAllReviewsAsync()
        {
            var reviews = await _context.CourseReviews
                .Include(r => r.Student)
                .Include(r => r.Course)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reviews.Select(r => new AdminReviewItemDto
            {
                Id = r.Id,
                CourseId = r.CourseId,
                CourseTitle = r.Course != null ? r.Course.Title : "Unknown Course",
                StudentId = r.StudentId,
                StudentName = r.Student != null ? r.Student.FullName : "Student",
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                IsApproved = r.IsApproved
            });
        }

        public async Task<bool> ToggleReviewApprovalAsync(int reviewId)
        {
            var review = await _context.CourseReviews.FindAsync(reviewId);
            if (review == null) return false;

            review.IsApproved = !review.IsApproved;
            _context.CourseReviews.Update(review);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteReviewAsync(int reviewId)
        {
            var review = await _context.CourseReviews.FindAsync(reviewId);
            if (review == null) return false;

            _context.CourseReviews.Remove(review);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
