using AutoMapper;
using MiniLMS.Business.DTOs;
using MiniLMS.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Business.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ==================== Courses & Curriculum ====================
            CreateMap<Course, CourseDto>()
                .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Instructor != null ? src.Instructor.FullName : null))
                .ReverseMap();

            CreateMap<CreateCourseDto, Course>();

            CreateMap<Section, SectionDto>().ReverseMap();
            CreateMap<CreateSectionDto, Section>();

            CreateMap<Lesson, LessonDto>().ReverseMap();
            CreateMap<CreateLessonDto, Lesson>();

            // ==================== Enrollments & Progress ====================
            CreateMap<Enrollment, EnrollmentDto>()
                .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.Course.Title));

            CreateMap<LessonProgress, LessonProgressDto>()
                .ForMember(dest => dest.LessonTitle, opt => opt.MapFrom(src => src.Lesson.Title))
                .ForMember(dest => dest.LessonType, opt => opt.MapFrom(src => src.Lesson.LessonType))
                .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src => src.Lesson.DurationMinutes))
                .ForMember(dest => dest.IsFreePreview, opt => opt.MapFrom(src => src.Lesson.IsFreePreview))
                .ForMember(dest => dest.OrderIndex, opt => opt.MapFrom(src => src.Lesson.OrderIndex));

            // ==================== Users, Students & Instructors ====================
            CreateMap<Student, StudentListItemDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.TotalEnrollments, opt => opt.MapFrom(src => src.Enrollments.Count))
                .ForMember(dest => dest.CompletedCourses, opt => opt.MapFrom(src => src.Enrollments.Count(e => e.Status == "Completed")))
                .ForMember(dest => dest.InProgressCourses, opt => opt.MapFrom(src => src.Enrollments.Count(e => e.Status == "InProgress")))
                .ForMember(dest => dest.Enrollments, opt => opt.MapFrom(src => src.Enrollments));

            CreateMap<Instructor, InstructorProfileDto>()
                .ForMember(dest => dest.TotalCourses, opt => opt.MapFrom(src => src.Courses.Count))
                .ForMember(dest => dest.Courses, opt => opt.MapFrom(src => src.Courses.Where(c => c.IsPublished)));

            CreateMap<Instructor, InstructorListItemDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.TotalCourses, opt => opt.MapFrom(src => src.Courses.Count))
                .ForMember(dest => dest.PublishedCoursesCount, opt => opt.MapFrom(src => src.Courses.Count(c => c.IsPublished)));

            CreateMap<UpdateInstructorProfileDto, Instructor>();

            // ==================== Quizzes ====================
            CreateMap<Quiz, QuizDto>()
                .ForMember(dest => dest.TotalQuestions, opt => opt.MapFrom(src => src.Questions.Count))
                .ForMember(dest => dest.TotalPoints, opt => opt.MapFrom(src => src.Questions.Sum(q => q.Points)))
                .ReverseMap();

            CreateMap<CreateQuizDto, Quiz>();

            CreateMap<QuizQuestion, QuizQuestionDto>().ReverseMap();
            CreateMap<CreateQuizQuestionDto, QuizQuestion>();

            CreateMap<QuizOption, QuizOptionDto>().ReverseMap();
            CreateMap<CreateQuizOptionDto, QuizOption>();

            CreateMap<QuizAttempt, QuizResultDto>()
                .ForMember(dest => dest.AttemptId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.QuestionsResults, opt => opt.MapFrom(src => src.Answers));

            CreateMap<QuizAnswer, QuizQuestionResultDto>()
                .ForMember(dest => dest.QuestionText, opt => opt.MapFrom(src => src.Question.QuestionText))
                .ForMember(dest => dest.Explanation, opt => opt.MapFrom(src => src.Question.Explanation))
                .ForMember(dest => dest.CorrectOptionId, opt => opt.MapFrom(src => src.Question.Options.FirstOrDefault(o => o.IsCorrect) != null ? src.Question.Options.FirstOrDefault(o => o.IsCorrect)!.Id : (int?)null));

            // ==================== Assignments ====================
            CreateMap<Assignment, AssignmentDto>()
                .ForMember(dest => dest.TotalSubmissionsCount, opt => opt.MapFrom(src => src.Submissions.Count))
                .ReverseMap();

            CreateMap<CreateAssignmentDto, Assignment>();

            CreateMap<AssignmentSubmission, AssignmentSubmissionDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.FullName))
                .ForMember(dest => dest.StudentEmail, opt => opt.MapFrom(src => src.Student.Email));

            // ==================== Q&A ====================
            CreateMap<LessonQuestion, LessonQuestionDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.FullName))
                .ForMember(dest => dest.LessonTitle, opt => opt.MapFrom(src => src.Lesson.Title))
                .ForMember(dest => dest.RepliesCount, opt => opt.MapFrom(src => src.Replies.Count));

            CreateMap<CreateLessonQuestionDto, LessonQuestion>();

            CreateMap<LessonReply, LessonReplyDto>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User.Instructor != null ? src.User.Instructor.FullName : (src.User.Student != null ? src.User.Student.FullName : src.User.Username)))
                .ForMember(dest => dest.AuthorRole, opt => opt.MapFrom(src => src.User.Role));

            CreateMap<CreateLessonReplyDto, LessonReply>();

            // ==================== Reviews ====================
            CreateMap<CourseReview, CourseReviewDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.FullName));

            CreateMap<CreateCourseReviewDto, CourseReview>();
        }
    }
}
