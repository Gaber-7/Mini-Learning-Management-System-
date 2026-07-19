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
            CreateMap<Course, CourseDto>().ReverseMap();
            CreateMap<CreateCourseDto, Course>();
            CreateMap<Lesson, LessonDto>().ReverseMap();
            CreateMap<CreateLessonDto, Lesson>();

            // إعداد ربط الاشتراكات وتضمين اسم الكورس تلقائياً
            CreateMap<Enrollment, EnrollmentDto>()
                .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.Course.Title));
        }
    }
}
