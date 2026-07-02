using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BMS.API.Modules.Shared.Data;
using BMS.API.Modules.User.Models;
using BMS.API.Modules.User.DTOs;
using BMS.API.Modules.Shared.Security;
using Microsoft.AspNetCore.Identity;

namespace BMS.API.Modules.User.Services
{
    public class UserAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly PasswordHasher<EndUser> _passwordHasher;

        public UserAuthService(ApplicationDbContext context, JwtTokenGenerator jwtTokenGenerator)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
            _passwordHasher = new PasswordHasher<EndUser>();
        }

        public async Task<UserAuthResponseDto> RegisterAsync(UserRegisterDto dto)
        {
            // Check phone uniqueness (phone is now the primary identifier)
            if (await _context.EndUsers.AnyAsync(u => u.PhoneNumber == dto.PhoneNumber))
            {
                throw new Exception("This phone number is already registered.");
            }

            var user = new EndUser
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Email = null, // Email is no longer collected
                PhoneNumber = dto.PhoneNumber,
                City = dto.City,
                Locality = dto.Locality,
                Occupation = dto.Occupation,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            _context.EndUsers.Add(user);
            await _context.SaveChangesAsync();

            var token = _jwtTokenGenerator.GenerateUserToken(user);

            return new UserAuthResponseDto
            {
                Token = token,
                UserId = user.Id.ToString(),
                Name = user.Name
            };
        }

        public async Task<UserAuthResponseDto> LoginAsync(UserLoginDto dto)
        {
            // Login via phone number
            var user = await _context.EndUsers.FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber);
            if (user == null)
            {
                throw new Exception("Invalid credentials.");
            }
            if (!user.IsActive)
            {
                throw new Exception("Your account has been deactivated by an administrator.");
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                throw new Exception("Invalid credentials.");
            }

            var token = _jwtTokenGenerator.GenerateUserToken(user);

            return new UserAuthResponseDto
            {
                Token = token,
                UserId = user.Id.ToString(),
                Name = user.Name
            };
        }
    }
}
