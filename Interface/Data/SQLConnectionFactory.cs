using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data;



public class SQLConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SQLConnectionFactory(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("RailwayDb")
            ?? throw new InvalidOperationException("Missing RailwayDb connection string");
    }

    public SqlConnection CreateConnection() => new SqlConnection(_connectionString);
}