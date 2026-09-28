using Application.Dto;
using Application.Interface;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure.Repository;

public class BookingRepository(IDbConnectionFactory factory) : IBookingRepository
{
    public async Task<BookingCreatedResult> CreateAsync(int userId, CreateBookingRequest request)
    {
        var passengers = new DataTable();
        passengers.Columns.Add("Name", typeof(string));
        passengers.Columns.Add("Age", typeof(int));
        passengers.Columns.Add("Gender", typeof(string));
        foreach (var p in request.Passengers)
            passengers.Rows.Add(p.Name.Trim(), p.Age, p.Gender.ToUpperInvariant());

        using var conn = factory.CreateConnection();
        using var cmd = new SqlCommand("usp_Booking_Create", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        cmd.Parameters.Add("@TrainId", SqlDbType.Int).Value = request.TrainId;
        cmd.Parameters.Add("@ClassId", SqlDbType.Int).Value = request.ClassId;
        cmd.Parameters.Add("@FromStationId", SqlDbType.Int).Value = request.FromStationId;
        cmd.Parameters.Add("@ToStationId", SqlDbType.Int).Value = request.ToStationId;
        cmd.Parameters.Add("@JourneyDate", SqlDbType.Date).Value = request.JourneyDate.ToDateTime(TimeOnly.MinValue);
        var tvp = cmd.Parameters.Add("@Passengers", SqlDbType.Structured);
        tvp.TypeName = "dbo.PassengerTableType";
        tvp.Value = passengers;

        try
        {
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();
            var wlOrdinal = reader.GetOrdinal("WaitlistNumber");
            return new BookingCreatedResult
            {
                BookingId = reader.GetInt32(reader.GetOrdinal("BookingId")),
                Pnr = reader.GetString(reader.GetOrdinal("Pnr")).Trim(),
                TotalFare = reader.GetDecimal(reader.GetOrdinal("TotalFare")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                WaitlistNumber = reader.IsDBNull(wlOrdinal) ? null : reader.GetInt32(wlOrdinal)
            };
        }
        catch (SqlException ex) when (ex.Number >= 50000)
        {
            throw new BusinessRuleException(ex.Message, ex.Number);
        }
    }

    public async Task<BookingDetails?> GetByPnrAsync(string pnr, int userId)
    {
        using var conn = factory.CreateConnection();
        using var cmd = new SqlCommand("usp_Booking_GetByPnr", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.Add("@Pnr", SqlDbType.Char, 10).Value = pnr;
        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;
        var booking = ReadSummary<BookingDetails>(reader);

        await reader.NextResultAsync();
        while (await reader.ReadAsync())
        {
            string? Str(string col) => reader.IsDBNull(reader.GetOrdinal(col)) ? null : reader.GetString(reader.GetOrdinal(col));
            int? seatOrdinal = reader.GetOrdinal("SeatNumber");

            booking.Passengers.Add(new PassengerDto
            {
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Age = reader.GetInt32(reader.GetOrdinal("Age")),
                Gender = reader.GetString(reader.GetOrdinal("Gender")).Trim(),
                CoachCode = Str("CoachCode"),
                SeatNumber = reader.IsDBNull(seatOrdinal.Value) ? null : reader.GetInt32(seatOrdinal.Value),
                BerthType = Str("BerthType")
            });
        }
        return booking;
    }

    public async Task<IEnumerable<BookingSummary>> GetByUserAsync(int userId)
    {
        var list = new List<BookingSummary>();

        using var conn = factory.CreateConnection();
        using var cmd = new SqlCommand("usp_Booking_GetByUser", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(ReadSummary<BookingSummary>(reader));
        return list;
    }

    public async Task CancelAsync(string pnr, int userId)
    {
        using var conn = factory.CreateConnection();
        using var cmd = new SqlCommand("usp_Booking_Cancel", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.Add("@Pnr", SqlDbType.Char, 10).Value = pnr;
        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

        try
        {
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
        catch (SqlException ex) when (ex.Number >= 50000)
        {
            throw new BusinessRuleException(ex.Message, ex.Number);
        }
    }

    private static T ReadSummary<T>(SqlDataReader r) where T : BookingSummary, new()
    {
        var wlOrdinal = r.GetOrdinal("WaitlistNumber");
        return new T
        {
            BookingId = r.GetInt32(r.GetOrdinal("BookingId")),
            Pnr = r.GetString(r.GetOrdinal("Pnr")).Trim(),
            Status = r.GetString(r.GetOrdinal("Status")),
            WaitlistNumber = r.IsDBNull(wlOrdinal) ? null : r.GetInt32(wlOrdinal),
            JourneyDate = DateOnly.FromDateTime(r.GetDateTime(r.GetOrdinal("JourneyDate"))),
            TotalFare = r.GetDecimal(r.GetOrdinal("TotalFare")),
            BookedAt = r.GetDateTime(r.GetOrdinal("BookedAt")),
            TrainNumber = r.GetString(r.GetOrdinal("TrainNumber")),
            TrainName = r.GetString(r.GetOrdinal("TrainName")),
            ClassCode = r.GetString(r.GetOrdinal("ClassCode")),
            ClassName = r.GetString(r.GetOrdinal("ClassName")),
            FromCode = r.GetString(r.GetOrdinal("FromCode")),
            FromName = r.GetString(r.GetOrdinal("FromName")),
            ToCode = r.GetString(r.GetOrdinal("ToCode")),
            ToName = r.GetString(r.GetOrdinal("ToName"))
        };
    }
}
