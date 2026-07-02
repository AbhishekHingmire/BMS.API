using System;
using System.Threading.Tasks;
using BMS.API.Modules.Owner.DTOs;
using BMS.API.Modules.Owner.Models;
using BMS.API.Modules.Shared.Data;
using BMS.API.Modules.Shared.Security;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace BMS.API.Modules.Owner.Services
{
    public interface IOwnerAuthService
    {
        Task<AuthResponseDto> RegisterAsync(OwnerRegisterRequestDto request);
        Task<AuthResponseDto> LoginAsync(OwnerLoginRequestDto request);
    }

    public class OwnerAuthService : IOwnerAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtTokenGenerator _jwtTokenGenerator;

        public OwnerAuthService(ApplicationDbContext context, JwtTokenGenerator jwtTokenGenerator)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<AuthResponseDto> LoginAsync(OwnerLoginRequestDto request)
        {
            // Login via phone number
            var user = await _context.OwnerUsers.FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);
            if (user == null)
            {
                return new AuthResponseDto { Message = "Invalid credentials." };
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return new AuthResponseDto { Message = "Invalid credentials." };
            }

            var token = _jwtTokenGenerator.GenerateOwnerToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Message = "Login successful.",
                User = new OwnerUserDto
                {
                    Id = user.Id.ToString(),
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role.ToString()
                }
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(OwnerRegisterRequestDto request)
        {
            // Check phone uniqueness
            if (await _context.OwnerUsers.AnyAsync(u => u.PhoneNumber == request.PhoneNumber))
            {
                return new AuthResponseDto { Message = "This phone number is already registered." };
            }

            // Check email uniqueness
            if (await _context.OwnerUsers.AnyAsync(u => u.Email == request.Email))
            {
                return new AuthResponseDto { Message = "Email is already registered." };
            }

            var user = new OwnerUser
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Role = request.Role,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                AvatarUrl = "",
                BankName = "",
                AccountNumber = "",
                IfscCode = "",
                UpiId = ""
            };

            _context.OwnerUsers.Add(user);
            await _context.SaveChangesAsync();

            var token = _jwtTokenGenerator.GenerateOwnerToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Message = "Registration successful.",
                User = new OwnerUserDto
                {
                    Id = user.Id.ToString(),
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role.ToString()
                }
            };
        }
    }
}
