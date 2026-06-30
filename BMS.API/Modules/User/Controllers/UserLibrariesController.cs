using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BMS.API.Modules.Shared.Data;
using BMS.API.Modules.Shared.Models;
using System.Linq;
using System;
using System.Security.Claims;

namespace BMS.API.Modules.User.Controllers
{
    [ApiController]
    [Route("api/user/libraries")]
    public class UserLibrariesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserLibrariesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var libraries = await _context.Libraries
                .Include(l => l.Areas)
                    .ThenInclude(a => a.Seats)
                .Include(l => l.Shifts)
                .Include(l => l.Plans)
                .Include(l => l.Reviews)
                .Where(l => l.IsVerified && l.IsPublished)
                .ToListAsync();

            var today = DateTime.UtcNow.Date;
            var libraryIds = libraries.Select(l => l.Id).ToList();
            
            var activeBookings = await _context.Bookings
                .Where(b => libraryIds.Contains(b.LibraryId) && 
                            b.StartDate <= today && 
                            b.EndDate >= today &&
                            b.Status != BookingStatus.Cancelled &&
                            b.Status != BookingStatus.Expired)
                .ToListAsync();
            
            var bookingsByLibrary = activeBookings.GroupBy(b => b.LibraryId).ToDictionary(g => g.Key, g => g.ToList());

            var dtos = libraries.Select(l => 
            {
                var libBookings = bookingsByLibrary.ContainsKey(l.Id) ? bookingsByLibrary[l.Id] : new System.Collections.Generic.List<Booking>();
                var totalSeats = l.Areas.Sum(a => a.Seats.Count(s => !s.IsInactive));
                var occupiedSeats = libBookings.Select(b => b.SeatId).Distinct().Count();
                
                var totalReviews = l.Reviews.Count;
                var averageRating = totalReviews > 0 ? l.Reviews.Average(r => r.Rating) : 0.0;
                
                return MapToFrontendDto(l, totalSeats, occupiedSeats, averageRating, totalReviews);
            }).ToList();

            // Custom location-based sorting
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdString, out Guid userId))
                {
                    var user = await _context.EndUsers.FindAsync(userId);
                    if (user != null && (!string.IsNullOrEmpty(user.City) || !string.IsNullOrEmpty(user.Locality)))
                    {
                        var userCity = user.City?.ToLower() ?? "";
                        var userLocality = user.Locality?.ToLower() ?? "";

                        dtos = dtos.OrderBy(d => 
                        {
                            var dto = d as dynamic;
                            string libCity = dto.city?.ToLower() ?? "";
                            string libArea = dto.area?.ToLower() ?? "";

                            // Priority 1: Match City and Locality
                            if (libCity == userCity && libArea == userLocality) return 1;
                            
                            // Priority 2: Match City only
                            if (libCity == userCity) return 2;
                            
                            // Priority 3: All others
                            return 3;
                        }).ToList();
                    }
                }
            }

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var library = await _context.Libraries
                .Include(l => l.Areas)
                    .ThenInclude(a => a.Seats)
                .Include(l => l.Shifts)
                .Include(l => l.Plans)
                .Include(l => l.Reviews)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (library == null)
            {
                return NotFound();
            }

            var today = DateTime.UtcNow.Date;
            var activeBookings = await _context.Bookings
                .Where(b => b.LibraryId == id && 
                            b.StartDate <= today && 
                            b.EndDate >= today &&
                            b.Status != BookingStatus.Cancelled &&
                            b.Status != BookingStatus.Expired)
                .ToListAsync();
                
            var totalSeats = library.Areas.Sum(a => a.Seats.Count(s => !s.IsInactive));
            var occupiedSeats = activeBookings.Select(b => b.SeatId).Distinct().Count();

            var totalReviews = library.Reviews.Count;
            var averageRating = totalReviews > 0 ? library.Reviews.Average(r => r.Rating) : 0.0;

            return Ok(MapToFrontendDto(library, totalSeats, occupiedSeats, averageRating, totalReviews));
        }

        [HttpGet("{id}/reviews")]
        public async Task<IActionResult> GetLibraryReviews(Guid id)
        {
            var reviews = await (from r in _context.Reviews
                                 join u in _context.EndUsers on r.UserId equals u.Id into userGroup
                                 from u in userGroup.DefaultIfEmpty()
                                 where r.LibraryId == id
                                 orderby r.CreatedAt descending
                                 select new
                                 {
                                     id = r.Id,
                                     userId = r.UserId,
                                     userName = u != null ? u.Name : "Unknown User",
                                     rating = r.Rating,
                                     comment = r.Comment,
                                     createdAt = r.CreatedAt.ToString("o")
                                 })
                                 .ToListAsync();
            return Ok(reviews);
        }

        public class SubmitReviewRequest
        {
            public Guid UserId { get; set; }
            public int Rating { get; set; }
            public string Comment { get; set; }
        }

        [HttpPost("{id}/reviews")]
        public async Task<IActionResult> SubmitReview(Guid id, [FromBody] SubmitReviewRequest request)
        {
            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest(new { message = "Rating must be between 1 and 5." });

            var libraryExists = await _context.Libraries.AnyAsync(l => l.Id == id);
            if (!libraryExists) return NotFound(new { message = "Library not found." });

            // Check if user already reviewed
            var existingReview = await _context.Reviews.FirstOrDefaultAsync(r => r.LibraryId == id && r.UserId == request.UserId);
            if (existingReview != null)
            {
                return BadRequest(new { message = "You have already reviewed this library." });
            }

            var review = new BMS.API.Modules.Shared.Models.Review
            {
                Id = Guid.NewGuid(),
                LibraryId = id,
                UserId = request.UserId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Review submitted successfully.", review });
        }

        [HttpPut("{id}/reviews/{reviewId}")]
        public async Task<IActionResult> UpdateReview(Guid id, Guid reviewId, [FromBody] SubmitReviewRequest request)
        {
            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest(new { message = "Rating must be between 1 and 5." });

            var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId && r.LibraryId == id);
            if (review == null) return NotFound(new { message = "Review not found." });

            if (review.UserId != request.UserId)
                return Forbid();

            review.Rating = request.Rating;
            review.Comment = request.Comment;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Review updated successfully.", review });
        }

        [HttpDelete("{id}/reviews/{reviewId}")]
        public async Task<IActionResult> DeleteReview(Guid id, Guid reviewId, [FromQuery] Guid userId)
        {
            var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId && r.LibraryId == id);
            if (review == null) return NotFound(new { message = "Review not found." });

            if (review.UserId != userId)
                return Forbid();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Review deleted successfully." });
        }

        [HttpGet("{id}/bookings")]
        public async Task<IActionResult> GetLibraryBookings(Guid id)
        {
            var bookings = await _context.Bookings
                .Where(b => b.LibraryId == id && b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.Expired)
                .Select(b => new 
                {
                    seatId = b.SeatId,
                    startDate = b.StartDate.ToString("yyyy-MM-dd"),
                    endDate = b.EndDate.ToString("yyyy-MM-dd"),
                    status = b.Status.ToString().ToLower()
                })
                .ToListAsync();
            return Ok(bookings);
        }

        [HttpGet("~/api/user/reviews")]
        public async Task<IActionResult> GetUserReviews()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var reviews = await _context.Reviews
                .Include(r => r.Library)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    id = r.Id,
                    libraryId = r.LibraryId,
                    libraryName = r.Library.Name,
                    rating = r.Rating,
                    comment = r.Comment,
                    createdAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(reviews);
        }

        private object MapToFrontendDto(BMS.API.Modules.Shared.Models.Library l, int totalSeats = 0, int occupiedSeats = 0, double averageRating = 0.0, int totalReviews = 0)
        {
            return new
            {
                id = l.Id,
                ownerId = l.OwnerId,
                name = l.Name,
                description = l.Description,
                address = l.Address,
                area = l.AreaName,
                city = l.City,
                photos = l.Photos,
                amenities = string.IsNullOrEmpty(l.AmenitiesString) ? new string[0] : l.AmenitiesString.Split(','),
                verified = l.IsVerified,
                published = l.IsPublished,
                cancellationPolicy = l.CancellationPolicy,
                shifts = l.Shifts.Select(s => new {
                    id = s.Id,
                    name = s.Name,
                    start = s.StartTime.ToString(@"hh\:mm"),
                    end = s.EndTime.ToString(@"hh\:mm")
                }),
                plans = l.Plans.Select(p => new {
                    id = p.Id,
                    duration = p.Duration.ToString().ToLower(),
                    shiftId = p.ShiftId,
                    basePrice = p.BasePrice,
                    discountPercent = p.DiscountPercent,
                    discountFlat = p.DiscountFlat,
                    name = p.Name,
                    enabled = p.IsEnabled,
                    daysOfWeek = string.IsNullOrEmpty(p.DaysOfWeekString) ? new int[0] : p.DaysOfWeekString.Split(',').Select(int.Parse)
                }),
                areas = l.Areas.Select(a => new {
                    id = a.Id,
                    name = a.Name,
                    tags = string.IsNullOrEmpty(a.TagsString) ? new string[0] : a.TagsString.Split(','),
                    priceModifier = a.PriceModifierType.HasValue ? new { type = a.PriceModifierType.Value.ToString().ToLower(), value = a.PriceModifierValue } : null,
                    floorPlan = string.IsNullOrEmpty(a.FloorPlanJson) ? null : System.Text.Json.JsonSerializer.Deserialize<object>(a.FloorPlanJson),
                    seats = a.Seats.Select(s => new {
                        id = s.Id,
                        number = s.Number,
                        genderRestriction = s.GenderRestriction == 0 ? null : s.GenderRestriction.ToString().ToLower(),
                        priceOverride = s.PriceOverride,
                        inactive = s.IsInactive
                    })
                }),
                totalSeats = totalSeats,
                occupiedSeats = occupiedSeats,
                averageRating = averageRating,
                totalReviews = totalReviews,
                faqJson = l.FaqJson
            };
        }
    }
}
