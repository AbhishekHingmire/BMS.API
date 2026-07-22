using System;
using System.Linq;
using System.Threading.Tasks;
using BMS.API.Modules.Shared.Data;
using BMS.API.Modules.Shared.DTOs;
using BMS.API.Modules.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace BMS.API.Modules.Shared.Services
{
    /// <summary>
    /// Shared "create or reuse a receipt share token" logic used by both the owner-side and
    /// student-side share-receipt endpoints, so token lifetime/reuse rules never drift between
    /// the two callers.
    /// </summary>
    public static class ReceiptShareHelper
    {
        private static readonly TimeSpan TokenLifetime = TimeSpan.FromDays(30);

        public static async Task<ShareReceiptResponseDto> CreateOrReuseShareTokenAsync(Guid bookingId, ApplicationDbContext context)
        {
            var existing = await context.ReceiptShareTokens
                .Where(t => t.BookingId == bookingId && t.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();
            if (existing != null)
            {
                return new ShareReceiptResponseDto { Token = existing.Token, ExpiresAt = existing.ExpiresAt };
            }

            var record = new ReceiptShareToken
            {
                Id = Guid.NewGuid(),
                BookingId = bookingId,
                Token = Guid.NewGuid().ToString("N"),
                ExpiresAt = DateTime.UtcNow.Add(TokenLifetime),
                CreatedAt = DateTime.UtcNow
            };

            context.ReceiptShareTokens.Add(record);
            await context.SaveChangesAsync();

            return new ShareReceiptResponseDto { Token = record.Token, ExpiresAt = record.ExpiresAt };
        }
    }
}
