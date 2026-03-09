using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRM393_Travel_Planner_BE.DTOs.Trip;
using PRM393_Travel_Planner_BE.Services.Interfaces;

namespace PRM393_Travel_Planner_BE.Controllers
{
    [ApiController]
    [Route("api/trips")]
    [Authorize]
    [Produces("application/json")]
    public class TripsController(ITripService tripService) : ControllerBase
    {
        // GET /api/trips
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TripDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyTrips()
        {
            var result = await tripService.GetMyTripsAsync(UserId);
            return Ok(result);
        }

        // GET /api/trips/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(TripDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTripDetail(Guid id)
        {
            var result = await tripService.GetTripDetailAsync(id, UserId);
            return Ok(result);
        }

        // POST /api/trips
        [HttpPost]
        [ProducesResponseType(typeof(TripDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTrip([FromBody] CreateTripRequest request)
        {
            var result = await tripService.CreateTripAsync(UserId, request);
            return CreatedAtAction(nameof(GetTripDetail), new { id = result.Id }, result);
        }

        // PUT /api/trips/{id}
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(TripDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTrip(Guid id, [FromBody] UpdateTripRequest request)
        {
            var result = await tripService.UpdateTripAsync(id, UserId, request);
            return Ok(result);
        }

        // DELETE /api/trips/{id}
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTrip(Guid id)
        {
            await tripService.DeleteTripAsync(id, UserId);
            return NoContent();
        }

        private Guid UserId => Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)
         ?? User.FindFirstValue("sub")!);
    }
}
