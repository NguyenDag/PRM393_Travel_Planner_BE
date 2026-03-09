using System.Net.NetworkInformation;
using PRM393_Travel_Planner_BE.Commons.Enums;
using PRM393_Travel_Planner_BE.DTOs.Trip;
using PRM393_Travel_Planner_BE.Models;
using PRM393_Travel_Planner_BE.Repositories.Interfaces;
using PRM393_Travel_Planner_BE.Services.Interfaces;

namespace PRM393_Travel_Planner_BE.Services.Implementations
{
    public class TripService(ITripRepository tripRepo) : ITripService
    {
        public async Task<IEnumerable<TripDto>> GetMyTripsAsync(Guid userId)
        {
            var trips = await tripRepo.GetByUserIdAsync(userId);
            return trips.Select(MapToDto);
        }

        public async Task<TripDetailDto> GetTripDetailAsync(Guid tripId, Guid userId)
        {
            var trip = await tripRepo.GetDetailAsync(tripId)
                ?? throw new KeyNotFoundException("Chuyến đi không tồn tại.");

            if (trip.UserId != userId)
                throw new UnauthorizedAccessException("Bạn không có quyền xem chuyến đi này.");

            return MapToDetailDto(trip);
        }

        public async Task<TripDto> CreateTripAsync(Guid userId, CreateTripRequest req)
        {
            if (req.DateTo < req.DateFrom)
                throw new InvalidOperationException("Ngày kết thúc phải sau ngày bắt đầu.");

            var totalDays = req.DateTo.DayNumber - req.DateFrom.DayNumber + 1;

            var trip = new Trip
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = req.Title,
                Location = req.Location,
                Country = req.Country,
                DateFrom = req.DateFrom,
                DateTo = req.DateTo,
                Status = TripStatus.Preparing.ToDbString(),
                CoverImageUrl = req.CoverImageUrl,
                TotalDays = totalDays,
                SourceAiSuggestionId = req.SourceAiSuggestionId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            // Auto-generate TripDays theo số ngày
            for (int i = 0; i < totalDays; i++)
            {
                trip.TripDays.Add(new TripDay
                {
                    Id = Guid.NewGuid(),
                    TripId = trip.Id,
                    DayNumber = i + 1,
                    Date = req.DateFrom.AddDays(i),
                });
            }

            await tripRepo.AddAsync(trip);
            return MapToDto(trip);
        }

        public async Task<TripDto> UpdateTripAsync(Guid tripId, Guid userId, UpdateTripRequest req)
        {
            var trip = await tripRepo.GetByIdAsync(tripId)
                ?? throw new KeyNotFoundException("Chuyến đi không tồn tại.");

            if (trip.UserId != userId)
                throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa chuyến đi này.");

            if (req.Title is not null) trip.Title = req.Title;
            if (req.Location is not null) trip.Location = req.Location;
            if (req.Country is not null) trip.Country = req.Country;
            if (req.CoverImageUrl is not null) trip.CoverImageUrl = req.CoverImageUrl;
            if (req.Status.HasValue) trip.Status = req.Status.Value.ToDbString();

            if (req.DateFrom.HasValue) trip.DateFrom = req.DateFrom.Value;
            if (req.DateTo.HasValue) trip.DateTo = req.DateTo.Value;

            if (trip.DateTo < trip.DateFrom)
                throw new InvalidOperationException("Ngày kết thúc phải sau ngày bắt đầu.");

            trip.TotalDays = (trip.DateTo!.Value.ToDateTime(TimeOnly.MinValue)
                    - trip.DateFrom!.Value.ToDateTime(TimeOnly.MinValue)).Days + 1;

            await tripRepo.UpdateAsync(trip);
            return MapToDto(trip);
        }

        public async Task DeleteTripAsync(Guid tripId, Guid userId)
        {
            var trip = await tripRepo.GetByIdAsync(tripId)
                ?? throw new KeyNotFoundException("Chuyến đi không tồn tại.");

            if (trip.UserId != userId)
                throw new UnauthorizedAccessException("Bạn không có quyền xóa chuyến đi này.");

            await tripRepo.DeleteAsync(trip);
        }

        // ── Mappers ───────────────────────────────────────────────────────────────

        private static TripDto MapToDto(Trip t) => new(
            t.Id, t.Title, t.Location, t.Country,
            t.DateFrom, t.DateTo, t.Status,
            t.CoverImageUrl, t.TotalDays, t.CreatedAt, t.UpdatedAt);

        private static TripDetailDto MapToDetailDto(Trip t) => new(
            t.Id, t.Title, t.Location, t.Country,
            t.DateFrom, t.DateTo, t.Status,
            t.CoverImageUrl, t.TotalDays, t.CreatedAt, t.UpdatedAt,
            t.TripDays.Select(d => new TripDayDto(
                d.Id, d.DayNumber, d.Date, d.Title,
                d.TripActivities.Select(MapActivityDto))));

        internal static TripActivityDto MapActivityDto(TripActivity a) => new(
            a.Id, a.Time.HasValue ? a.Time.Value.ToString("HH:mm") : string.Empty, a.Title,
            a.LocationName, a.Latitude, a.Longitude,
            a.Note, a.SortOrder, a.CreatedAt);
    }
}
