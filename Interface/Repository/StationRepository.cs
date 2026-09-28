using Microsoft.Data.SqlClient;
using System.Data;
using Infrastructure.Data;
using Domain.Models;
using Application.Interface;

namespace Infrastructure.Repository;
    public class StationRepository : IStationRepository
    {
        private readonly IDbConnectionFactory _factory;
        public StationRepository(IDbConnectionFactory factory) => _factory = factory;

        public async Task<IEnumerable<Station>> SearchAsync(string term)
        {
            var list = new List<Station>();
            using var conn = _factory.CreateConnection();
            using var cmd = new SqlCommand("usp_Station_Search", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Term", term);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                list.Add(Map(reader));
            return list;
        }

        public async Task<Station?> GetByIdAsync(int id)
        {
            using var conn = _factory.CreateConnection();
            using var cmd = new SqlCommand("usp_Station_GetById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@StationId", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        private static Station Map(SqlDataReader r) => new()
        {
            StationId = r.GetInt32(r.GetOrdinal("StationId")),
            Code = r.GetString(r.GetOrdinal("Code")),
            Name = r.GetString(r.GetOrdinal("Name")),
            City = r.GetString(r.GetOrdinal("City"))
        };
    }