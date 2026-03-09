using System;
using Microsoft.EntityFrameworkCore;
using PRM393_Travel_Planner_BE.Models;
using PRM393_Travel_Planner_BE.Repositories.Interfaces;

namespace PRM393_Travel_Planner_BE.Repositories.Implementations
{
    public class TripRepository(Prm393TravelPlannerContext db) : ITripRepository
    {
        public Task<IEnumerable<Trip>> GetByUserIdAsync(Guid userId)
            => Task.FromResult<IEnumerable<Trip>>(
                db.Trips
                  .Where(t => t.UserId == userId)
                  .OrderByDescending(t => t.CreatedAt)
                  .AsEnumerable());

        public Task<Trip?> GetByIdAsync(Guid id)
            => db.Trips.FirstOrDefaultAsync(t => t.Id == id);

        public Task<Trip?> GetDetailAsync(Guid id)
            => db.Trips
                 .Include(t => t.TripDays.OrderBy(d => d.DayNumber))
                 .ThenInclude(d => d.TripActivities.OrderBy(a => a.SortOrder))
                 .FirstOrDefaultAsync(t => t.Id == id);

        public async Task AddAsync(Trip trip)
        {
            db.Trips.Add(trip);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Trip trip)
        {
            trip.UpdatedAt = DateTime.UtcNow;
            db.Trips.Update(trip);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Trip trip)
        {
            db.Trips.Remove(trip);
            await db.SaveChangesAsync();
        }

        public Task<bool> BelongsToUserAsync(Guid tripId, Guid userId)
            => db.Trips.AnyAsync(t => t.Id == tripId && t.UserId == userId);
    }
}
