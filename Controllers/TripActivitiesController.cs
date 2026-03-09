using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRM393_Travel_Planner_BE.DTOs.Trip;
using PRM393_Travel_Planner_BE.DTOs.TripActivity;
using PRM393_Travel_Planner_BE.Services.Interfaces;

namespace PRM393_Travel_Planner_BE.Controllers
{
    [ApiController]
    [Route("api/trips/{tripId:guid}/days/{dayId:guid}/activities")]
    [Authorize]
    [Produces("application/json")]
    public class TripActivitiesController(ITripActivityService activityService) : ControllerBase
    {
        // GET /api/trips/{tripId}/days/{dayId}/activities/{activityId}
        [HttpGet("{activityId:guid}")]
        [ProducesResponseType(typeof(TripActivityDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActivity(Guid tripId, Guid dayId, Guid activityId)
        {
            var result = await activityService.GetActivityAsync(tripId, dayId, activityId, UserId);
            return Ok(result);
        }

        // POST /api/trips/{tripId}/days/{dayId}/activities
        [HttpPost]
        [ProducesResponseType(typeof(TripActivityDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateActivity(
            Guid tripId, Guid dayId, [FromBody] CreateTripActivityRequest request)
        {
            var result = await activityService.CreateActivityAsync(tripId, dayId, UserId, request);
            return CreatedAtAction(nameof(GetActivity),
                new { tripId, dayId, activityId = result.Id }, result);
        }

        // PUT /api/trips/{tripId}/days/{dayId}/activities/{activityId}
        [HttpPut("{activityId:guid}")]
        [ProducesResponseType(typeof(TripActivityDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateActivity(
            Guid tripId, Guid dayId, Guid activityId, [FromBody] UpdateTripActivityRequest request)
        {
            var result = await activityService.UpdateActivityAsync(tripId, dayId, activityId, UserId, request);
            return Ok(result);
        }

        // DELETE /api/trips/{tripId}/days/{dayId}/activities/{activityId}
        [HttpDelete("{activityId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteActivity(Guid tripId, Guid dayId, Guid activityId)
        {
            await activityService.DeleteActivityAsync(tripId, dayId, activityId, UserId);
            return NoContent();
        }

        // PATCH /api/trips/{tripId}/days/{dayId}/activities/reorder
        [HttpPatch("reorder")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Reorder(
            Guid tripId, Guid dayId, [FromBody] ReorderActivitiesRequest request)
        {
            await activityService.ReorderActivitiesAsync(tripId, dayId, UserId, request);
            return NoContent();
        }

        private Guid UserId => Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)
         ?? User.FindFirstValue("sub")!);
    }
}
