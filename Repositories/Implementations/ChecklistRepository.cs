using System;
using Microsoft.EntityFrameworkCore;
using PRM393_Travel_Planner_BE.Models;
using PRM393_Travel_Planner_BE.Repositories.Interfaces;

namespace PRM393_Travel_Planner_BE.Repositories.Implementations
{
    public class ChecklistRepository(Prm393TravelPlannerContext db) : IChecklistRepository
    {
        public Task<IEnumerable<Checklist>> GetByTripIdAsync(Guid tripId)
            => Task.FromResult<IEnumerable<Checklist>>(
                db.Checklists
                  .Include(c => c.ChecklistItems.OrderBy(i => i.SortOrder))
                  .Where(c => c.TripId == tripId)
                  .OrderBy(c => c.SortOrder)
                  .AsEnumerable());

        public Task<Checklist?> GetByIdAsync(Guid id)
            => db.Checklists.FirstOrDefaultAsync(c => c.Id == id);

        public Task<Checklist?> GetWithItemsAsync(Guid id)
            => db.Checklists
                 .Include(c => c.ChecklistItems.OrderBy(i => i.SortOrder))
                 .FirstOrDefaultAsync(c => c.Id == id);

        public async Task AddAsync(Checklist checklist)
        {
            db.Checklists.Add(checklist);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Checklist checklist)
        {
            db.Checklists.Update(checklist);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Checklist checklist)
        {
            db.Checklists.Remove(checklist);
            await db.SaveChangesAsync();
        }

        public Task<bool> BelongsToTripAsync(Guid checklistId, Guid tripId)
            => db.Checklists.AnyAsync(c => c.Id == checklistId && c.TripId == tripId);
    }
}
