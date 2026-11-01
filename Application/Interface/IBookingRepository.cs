using Application.Dto;
using Domain.Models;

namespace Application.Interface;

public interface IBookingRepository
{
    Task<BookingCreatedResult> CreateAsync(int userId, CreateBookingRequest request);
    Task<RoundTripResult> CreateRoundTripAsync(int userId, RoundTripRequest request);
    Task<BookingDetails?> GetByPnrAsync(string pnr, int userId);
    Task<IEnumerable<BookingSummary>> GetByUserAsync(int userId);
    Task CancelAsync(string pnr, int userId);
}