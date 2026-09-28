using Microsoft.Data.SqlClient;
using System.Data;
using Infrastructure.Data;
using Domain.Models;
using Application.Interface;

namespace Infrastructure.Repository;

public class TrainRepository(IDbConnectionFactory factory) : ITrainRepository
{
    public async Task<IEnumerable<TrainSearchResult>> SearchAsync(
    int fromStationId, int toStationId, DateOnly journeyDate)
    {
        var trains = new List<TrainSearchResult>();
        var byId = new Dictionary<int, TrainSearchResult>();

        using var conn = factory.CreateConnection();
        using var cmd = new SqlCommand("usp_Train_SearchWithAvailability", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.Add("@FromStationId", SqlDbType.Int).Value = fromStationId;
        cmd.Parameters.Add("@ToStationId", SqlDbType.Int).Value = toStationId;
        cmd.Parameters.Add("@JourneyDate", SqlDbType.Date).Value = journeyDate.ToDateTime(TimeOnly.MinValue);

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var trainId = reader.GetInt32(reader.GetOrdinal("TrainId"));
            if (!byId.TryGetValue(trainId, out var train))
            {
                train = new TrainSearchResult
                {
                    TrainId = trainId,
                    TrainNumber = reader.GetString(reader.GetOrdinal("TrainNumber")),
                    TrainName = reader.GetString(reader.GetOrdinal("TrainName")),
                    TrainType = reader.GetString(reader.GetOrdinal("TrainType")),
                    DepartureTime = reader.GetTimeSpan(reader.GetOrdinal("DepartureTime")),
                    ArrivalTime = reader.GetTimeSpan(reader.GetOrdinal("ArrivalTime")),
                    DaysTaken = reader.GetInt32(reader.GetOrdinal("DaysTaken")),
                    DistanceKm = reader.GetInt32(reader.GetOrdinal("DistanceKm"))
                };
                byId[trainId] = train;
                trains.Add(train);
            }

            train.Classes.Add(new ClassAvailability
            {
                ClassId = reader.GetInt32(reader.GetOrdinal("ClassId")),
                ClassCode = reader.GetString(reader.GetOrdinal("ClassCode")),
                ClassName = reader.GetString(reader.GetOrdinal("ClassName")),
                Fare = reader.GetDecimal(reader.GetOrdinal("Fare")),
                AvailableSeats = reader.GetInt32(reader.GetOrdinal("AvailableSeats"))
            });
        }
        return trains;
    }
}