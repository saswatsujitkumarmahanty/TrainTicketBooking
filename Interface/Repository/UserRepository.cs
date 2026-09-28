using System.Data;
using Microsoft.Data.SqlClient;
using Infrastructure.Data;
using Domain.Models;
using Application.Interface;

namespace Infrastructure.Repository;

public class UserRepository(IDbConnectionFactory factory) : IUserRepository
{
    public async Task<int> CreateAsync(User user)
    {
        using var conn = factory.CreateConnection();
        using var cmd = new SqlCommand("usp_User_Register", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = user.FullName;
        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = user.Email;
        cmd.Parameters.Add("@Phone", SqlDbType.VarChar, 15).Value = user.Phone;
        cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 300).Value = user.PasswordHash;

        try
        {
            await conn.OpenAsync();
            return (int)(await cmd.ExecuteScalarAsync())!;
        }
        // 50001 = our duplicate check; 2627/2601 = unique index hit by two simultaneous sign-ups
        catch (SqlException ex) when (ex.Number is 50001 or 2627 or 2601)
        {
            throw new BusinessRuleException("This email is already registered.", 50001);
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var conn = factory.CreateConnection();
        using var cmd = new SqlCommand("usp_User_GetByEmail", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = email;

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        return new User
        {
            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
            FullName = reader.GetString(reader.GetOrdinal("FullName")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            Phone = reader.GetString(reader.GetOrdinal("Phone")),
            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
            Role = reader.GetString(reader.GetOrdinal("Role"))
        };
    }
}