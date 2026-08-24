using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MiniLMS.Business.DTOs;
using MiniLMS.Business.Interfaces;
using MiniLMS.Data.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Business.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public AdminUserService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<StudentListItemDto>> GetAllStudentsAsync()
        {
            var students = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .OrderBy(s => s.FullName)
                .ToListAsync();

            return _mapper.Map<IEnumerable<StudentListItemDto>>(students);
        }

        public async Task<IEnumerable<InstructorListItemDto>> GetAllInstructorsAsync()
        {
            var instructors = await _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Courses)
                .OrderBy(i => i.FullName)
                .ToListAsync();

            var list = _mapper.Map<List<InstructorListItemDto>>(instructors);

            // Populate total students taught for each instructor
            foreach (var inst in list)
            {
                var courseIds = instructors.First(i => i.Id == inst.Id).Courses.Select(c => c.Id).ToList();
                inst.TotalStudentsCount = await _context.Enrollments.CountAsync(e => courseIds.Contains(e.CourseId));
            }

            return list;
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
    }
}
