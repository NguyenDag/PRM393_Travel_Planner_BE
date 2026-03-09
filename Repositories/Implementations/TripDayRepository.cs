using System;
using Microsoft.EntityFrameworkCore;
using PRM393_Travel_Planner_BE.Models;
using PRM393_Travel_Planner_BE.Repositories.Interfaces;

namespace PRM393_Travel_Planner_BE.Repositories.Implementations
{
    public class TripDayRepository(Prm393TravelPlannerContext db) : ITripDayRepository
    {
        public Task<IEnumerable<TripDay>> GetByTripIdAsync(Guid tripId)
            => Task.FromResult<IEnumerable<TripDay>>(
                db.TripDays
                  .Where(d => d.TripId == tripId)
                  .OrderBy(d => d.DayNumber)
                  .AsEnumerable());

        public Task<TripDay?> GetByIdAsync(Guid id)
            => db.TripDays.FirstOrDefaultAsync(d => d.Id == id);

        public Task<TripDay?> GetWithActivitiesAsync(Guid id)
            => db.TripDays
                 .Include(d => d.TripActivities.OrderBy(a => a.SortOrder))
                 .FirstOrDefaultAsync(d => d.Id == id);

        public async Task AddAsync(TripDay day)
        {
            db.TripDays.Add(day);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(TripDay day)
        {
            db.TripDays.Update(day);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(TripDay day)
        {
            db.TripDays.Remove(day);
            await db.SaveChangesAsync();
        }

        public Task<bool> BelongsToTripAsync(Guid dayId, Guid tripId)
            => db.TripDays.AnyAsync(d => d.Id == dayId && d.TripId == tripId);
    }
}
