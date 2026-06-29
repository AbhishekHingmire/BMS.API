using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BMS.API.Modules.Shared.Data;
using System;
using System.Linq;

namespace BMS.API.Modules.Shared.Controllers
{
    [ApiController]
    [Route("api/locations")]
    public class LocationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LocationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("cities")]
        public async Task<IActionResult> GetCities()
        {
            var cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
            return Ok(cities);
        }

        [HttpGet("cities/{cityId}/localities")]
        public async Task<IActionResult> GetLocalities(Guid cityId)
        {
            var localities = await _context.Localities
                .Where(l => l.CityId == cityId)
                .OrderBy(l => l.Name)
                .ToListAsync();
            return Ok(localities);
        }

        [HttpGet("localities")]
        public async Task<IActionResult> GetAllLocalities()
        {
            var localities = await _context.Localities.OrderBy(l => l.Name).ToListAsync();
            return Ok(localities);
        }
    }
}
