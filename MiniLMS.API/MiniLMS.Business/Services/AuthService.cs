using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MiniLMS.Business.DTOs;
using MiniLMS.Business.Interfaces;
using MiniLMS.Data.Data;
using MiniLMS.Data.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<User>(); // استخدام الهاشر الافتراضي لتشفير كلمات المرور 
        }

        public async Task<AuthResponseDto?> RegisterStudentAsync(RegisterDto registerDto)
        {
            // التحقق من عدم تكرار اسم المستخدم
            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == registerDto.Username.ToLower()))
                return null;

            // 1. إنشاء حساب المستخدم (User) وحفظه
            var user = new User
            {
                Username = registerDto.Username,
               Role = "Student" // التسجيل من الشاشة مخصص للطلاب فقط [cite: 25, 27]
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, registerDto.Password); // التشفير [cite: 29]

            _context.Users.Add(user);
            await _context.SaveChangesAsync(); // نحفظ أولاً لكي يتولد الـ Id للمستخدم

            var student = new Student
            {
                Id = user.Id, // One-to-One علاقة
                FullName = registerDto.FullName,
                Email = registerDto.Email
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            // توليد التوكن بعد التسجيل المباشر لتسجيل دخول تلقائي
            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Role = user.Role
            };
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            // البحث عن المستخدم باسم المستخدم
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == loginDto.Username.ToLower());
            if (user == null)
                return null;

            // التحقق من صحة كلمة المرور المشفّرة
            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);
            if (verificationResult == PasswordVerificationResult.Failed)
                return null;

            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Role = user.Role
            };
        }

       // دالة توليد الـ JWT Token 
        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role) // تضمين الصلاحيات [cite: 32]
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpireMinutes"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
