using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRM393_Travel_Planner_BE.DTOs.Trip;
using PRM393_Travel_Planner_BE.DTOs.TripDay;
using PRM393_Travel_Planner_BE.Services.Interfaces;

namespace PRM393_Travel_Planner_BE.Controllers
{
    [ApiController]
    [Route("api/trips/{tripId:guid}/days")]
    [Authorize]
    [Produces("application/json")]
    public class TripDaysController(ITripDayService dayService) : ControllerBase
    {
        // GET /api/trips/{tripId}/days/{dayId}
        [HttpGet("{dayId:guid}")]
        [ProducesResponseType(typeof(TripDayDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDay(Guid tripId, Guid dayId)
        {
            var result = await dayService.GetTripDayAsync(tripId, dayId, UserId);
            return Ok(result);
        }

        // POST /api/trips/{tripId}/days
        [HttpPost]
        [ProducesResponseType(typeof(TripDayDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateDay(Guid tripId, [FromBody] CreateTripDayRequest request)
        {
            var result = await dayService.CreateTripDayAsync(tripId, UserId, request);
            return CreatedAtAction(nameof(GetDay), new { tripId, dayId = result.Id }, result);
        }

        // PUT /api/trips/{tripId}/days/{dayId}
        [HttpPut("{dayId:guid}")]
        [ProducesResponseType(typeof(TripDayDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateDay(Guid tripId, Guid dayId, [FromBody] UpdateTripDayRequest request)
        {
            var result = await dayService.UpdateTripDayAsync(tripId, dayId, UserId, request);
            return Ok(result);
        }

        // DELETE /api/trips/{tripId}/days/{dayId}
        [HttpDelete("{dayId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteDay(Guid tripId, Guid dayId)
        {
            await dayService.DeleteTripDayAsync(tripId, dayId, UserId);
            return NoContent();
        }

        private Guid UserId => Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)
         ?? User.FindFirstValue("sub")!);
    }
}
