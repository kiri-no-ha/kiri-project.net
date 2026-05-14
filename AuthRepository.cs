using MySql.Data.MySqlClient;
using System.Data;

public class AuthRepository
{
    private readonly string _connectionString;

    public AuthRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Default");
    }

    // Найти пользователя по email или username
    public async Task<User?> GetUserByLoginAsync(string login)
    {
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        string sql = "SELECT id, username, email FROM users WHERE email = @login OR username = @login";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@login", login);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetInt32("id"),
                Username = reader.GetString("username"),
                Email = reader.GetString("email")
            };
        }
        return null;
    }

    // Сохранить код доступа
    public async Task<bool> SaveCodeAsync(int userId, string code, DateTime expiresAt)
    {
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        string sql = "INSERT INTO access_codes (user_id, code, expires_at, used) VALUES (@userId, @code, @expiresAt, FALSE)";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@code", code);
        cmd.Parameters.AddWithValue("@expiresAt", expiresAt);
        int affected = await cmd.ExecuteNonQueryAsync();
        return affected == 1;
    }

    // Получить валидный (неиспользованный, не истёкший) код
    public async Task<AccessCode?> GetValidCodeAsync(int userId, string code)
    {
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        string sql = @"
            SELECT id, user_id, code, expires_at, used
            FROM access_codes
            WHERE user_id = @userId AND code = @code AND used = FALSE AND expires_at > NOW()
            LIMIT 1";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@code", code);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new AccessCode
            {
                Id = reader.GetInt32("id"),
                UserId = reader.GetInt32("user_id"),
                Code = reader.GetString("code"),
                ExpiresAt = reader.GetDateTime("expires_at"),
                Used = reader.GetBoolean("used")
            };
        }
        return null;
    }

    // Пометить код как использованный
    public async Task MarkCodeAsUsedAsync(int codeId)
    {
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        string sql = "UPDATE access_codes SET used = TRUE WHERE id = @id";
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", codeId);
        await cmd.ExecuteNonQueryAsync();
    }
    public async Task<bool> UserExistsAsync(string username, string email)
    {
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        string sql = @"
        SELECT COUNT(*)
        FROM users
        WHERE username = @username OR email = @email";

        using var cmd = new MySqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@username", username);
        cmd.Parameters.AddWithValue("@email", email);

        var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());

        return count > 0;
    }
    public async Task<bool> CreateUserAsync(string username, string email)
    {
        using var conn = new MySqlConnection(_connectionString);

        await conn.OpenAsync();

        string sql = @"
        INSERT INTO users (username, email)
        VALUES (@username, @email)";

        using var cmd = new MySqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@username", username);
        cmd.Parameters.AddWithValue("@email", email);

        int affected = await cmd.ExecuteNonQueryAsync();

        return affected == 1;
    }
}