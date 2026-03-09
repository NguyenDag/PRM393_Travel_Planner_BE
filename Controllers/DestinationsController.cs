using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRM393_Travel_Planner_BE.Models;

namespace PRM393_Travel_Planner_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DestinationsController : ControllerBase
    {
        private readonly Prm393TravelPlannerContext _context;

        public DestinationsController(Prm393TravelPlannerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDestinations()
        {
            var data = await _context.Destinations.ToListAsync();
            return Ok(data);
        }
    }
}
