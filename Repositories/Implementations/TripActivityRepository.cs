using System;
using Microsoft.EntityFrameworkCore;
using PRM393_Travel_Planner_BE.Models;
using PRM393_Travel_Planner_BE.Repositories.Interfaces;

namespace PRM393_Travel_Planner_BE.Repositories.Implementations
{
    public class TripActivityRepository(Prm393TravelPlannerContext db) : ITripActivityRepository
    {
        public Task<IEnumerable<TripActivity>> GetByDayIdAsync(Guid tripDayId)
            => Task.FromResult<IEnumerable<TripActivity>>(
                db.TripActivities
                  .Where(a => a.TripDayId == tripDayId)
                  .OrderBy(a => a.SortOrder)
                  .AsEnumerable());

        public Task<TripActivity?> GetByIdAsync(Guid id)
            => db.TripActivities.FirstOrDefaultAsync(a => a.Id == id);

        public async Task AddAsync(TripActivity activity)
        {
            db.TripActivities.Add(activity);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(TripActivity activity)
        {
            db.TripActivities.Update(activity);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(TripActivity activity)
        {
            db.TripActivities.Remove(activity);
            await db.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<TripActivity> activities)
        {
            db.TripActivities.UpdateRange(activities);
            await db.SaveChangesAsync();
        }

        public Task<bool> BelongsToDayAsync(Guid activityId, Guid tripDayId)
            => db.TripActivities.AnyAsync(a => a.Id == activityId && a.TripDayId == tripDayId);
    }
}
