using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRM393_Travel_Planner_BE.DTOs.Checklist;
using PRM393_Travel_Planner_BE.Services.Interfaces;

namespace PRM393_Travel_Planner_BE.Controllers
{
    [ApiController]
    [Route("api/trips/{tripId:guid}/checklists")]
    [Authorize]
    [Produces("application/json")]
    public class ChecklistsController(IChecklistService checklistService) : ControllerBase
    {
        // GET /api/trips/{tripId}/checklists
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ChecklistDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetChecklists(Guid tripId)
        {
            var result = await checklistService.GetChecklistsAsync(tripId, UserId);
            return Ok(result);
        }

        // GET /api/trips/{tripId}/checklists/{checklistId}
        [HttpGet("{checklistId:guid}")]
        [ProducesResponseType(typeof(ChecklistDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetChecklist(Guid tripId, Guid checklistId)
        {
            var result = await checklistService.GetChecklistAsync(tripId, checklistId, UserId);
            return Ok(result);
        }

        // POST /api/trips/{tripId}/checklists
        [HttpPost]
        [ProducesResponseType(typeof(ChecklistDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateChecklist(Guid tripId, [FromBody] CreateChecklistRequest request)
        {
            var result = await checklistService.CreateChecklistAsync(tripId, UserId, request);
            return CreatedAtAction(nameof(GetChecklist),
                new { tripId, checklistId = result.Id }, result);
        }

        // PUT /api/trips/{tripId}/checklists/{checklistId}
        [HttpPut("{checklistId:guid}")]
        [ProducesResponseType(typeof(ChecklistDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateChecklist(
            Guid tripId, Guid checklistId, [FromBody] UpdateChecklistRequest request)
        {
            var result = await checklistService.UpdateChecklistAsync(tripId, checklistId, UserId, request);
            return Ok(result);
        }

        // DELETE /api/trips/{tripId}/checklists/{checklistId}
        [HttpDelete("{checklistId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteChecklist(Guid tripId, Guid checklistId)
        {
            await checklistService.DeleteChecklistAsync(tripId, checklistId, UserId);
            return NoContent();
        }

        // ── Items ─────────────────────────────────────────────────────────────────

        // POST /api/trips/{tripId}/checklists/{checklistId}/items
        [HttpPost("{checklistId:guid}/items")]
        [ProducesResponseType(typeof(ChecklistItemDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> AddItem(
            Guid tripId, Guid checklistId, [FromBody] CreateChecklistItemRequest request)
        {
            var result = await checklistService.AddItemAsync(tripId, checklistId, UserId, request);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        // PUT /api/trips/{tripId}/checklists/{checklistId}/items/{itemId}
        [HttpPut("{checklistId:guid}/items/{itemId:guid}")]
        [ProducesResponseType(typeof(ChecklistItemDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateItem(
            Guid tripId, Guid checklistId, Guid itemId, [FromBody] UpdateChecklistItemRequest request)
        {
            var result = await checklistService.UpdateItemAsync(tripId, checklistId, itemId, UserId, request);
            return Ok(result);
        }

        // DELETE /api/trips/{tripId}/checklists/{checklistId}/items/{itemId}
        [HttpDelete("{checklistId:guid}/items/{itemId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteItem(Guid tripId, Guid checklistId, Guid itemId)
        {
            await checklistService.DeleteItemAsync(tripId, checklistId, itemId, UserId);
            return NoContent();
        }

        // PATCH /api/trips/{tripId}/checklists/{checklistId}/items/bulk-toggle
        [HttpPatch("{checklistId:guid}/items/bulk-toggle")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> BulkToggle(
            Guid tripId, Guid checklistId, [FromBody] BulkToggleRequest request)
        {
            await checklistService.BulkToggleAsync(tripId, checklistId, UserId, request);
            return NoContent();
        }

        private Guid UserId => Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)
         ?? User.FindFirstValue("sub")!);
    }
}
