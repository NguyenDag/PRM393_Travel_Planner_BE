using System;
using Microsoft.EntityFrameworkCore;
using PRM393_Travel_Planner_BE.Models;
using PRM393_Travel_Planner_BE.Repositories.Interfaces;

namespace PRM393_Travel_Planner_BE.Repositories.Implementations
{
    public class ChecklistItemRepository(Prm393TravelPlannerContext db) : IChecklistItemRepository
    {
        public Task<ChecklistItem?> GetByIdAsync(Guid id)
            => db.ChecklistItems.FirstOrDefaultAsync(i => i.Id == id);

        public async Task AddAsync(ChecklistItem item)
        {
            db.ChecklistItems.Add(item);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(ChecklistItem item)
        {
            db.ChecklistItems.Update(item);
            await db.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<ChecklistItem> items)
        {
            db.ChecklistItems.UpdateRange(items);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(ChecklistItem item)
        {
            db.ChecklistItems.Remove(item);
            await db.SaveChangesAsync();
        }

        public Task<bool> BelongsToChecklistAsync(Guid itemId, Guid checklistId)
            => db.ChecklistItems.AnyAsync(i => i.Id == itemId && i.ChecklistId == checklistId);
    }
}
