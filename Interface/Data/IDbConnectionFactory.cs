using Microsoft.Data.SqlClient;

namespace Infrastructure.Data
{
    public interface IDbConnectionFactory
    {
        SqlConnection CreateConnection();
    }
}