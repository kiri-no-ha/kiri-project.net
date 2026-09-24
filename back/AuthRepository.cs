using MySql.Data.MySqlClient;
using System.Data;

public class AuthRepository
{
    private readonly string _connectionString;

    public AuthRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            _connectionString = "Server=127.0.0.1;Port=3306;Database=college_platform;User=root;Password=poleno36573!;CharSet=utf8mb4;";
            Console.WriteLine("!!! ХАРДКОД АКТИВЕН. Connection string = " + _connectionString);
        }
        else
        {
            Console.WriteLine(">>> Строка подключения из appsettings: " + _connectionString);
        }
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

    public async Task<AccessCode?> GetValidCodeAsync(int userId, string code)
    {
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        string sql = @"
        SELECT id, user_id, code, expires_at, used
        FROM access_codes
        WHERE user_id = @userId
          AND code = @code
          AND used = 0
          AND expires_at > NOW()
        ORDER BY id DESC
        LIMIT 1";

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@code", code);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var expiresAt = reader.GetDateTime("expires_at");
            if (expiresAt < DateTime.Now) return null;
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

    //массив топов
    public async Task<List<User>> GetTop100FromDbAsync()
    {
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        string sql = @"
        SELECT id, username, email, points, attendance, gaps, events, course
        FROM users
        ORDER BY points DESC
        LIMIT 100";

        using var cmd = new MySqlCommand(sql, conn);
        var topPlayers = new List<User>();

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            topPlayers.Add(new User
            {
                Id = reader.GetInt32("id"),
                Username = reader.GetString("username"),
                Email = reader.GetString("email"),
                Points = reader.GetInt32("points"),
                Attendance = reader.GetInt32("attendance"),
                Gaps = reader.GetInt32("gaps"),
                Events = reader.GetInt32("events"),
                Course = reader.GetInt32("course")
            });
        }

        return topPlayers;
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
    public async Task<User?> GetFullUserAsync(string login)
    {
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        string sql = @"SELECT id, username, email, points, attendance, gaps, events, course
                   FROM users
                   WHERE username = @login OR email = @login
                   LIMIT 1";

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@login", login);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetInt32("id"),
                Username = reader.GetString("username"),
                Email = reader.GetString("email"),
                Points = reader.GetInt32("points"),
                Attendance = reader.GetInt32("attendance"),
                Gaps = reader.GetInt32("gaps"),
                Events = reader.GetInt32("events"),
                Course = reader.GetInt32("course")
            };
        }
        return null;
    }
    //manage
    public async Task<bool> StaffAuth(string Username, string PasswordHash)
    {
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        string sql = @"
            SELECT COUNT(*) 
            FROM Staff 
            WHERE Username = @Username OR Password = @PasswordHash";
        using var cmd = new MySqlCommand(sql);
        cmd.Parameters.AddWithValue($"username", Username);
        cmd.Parameters.AddWithValue("PasswordHash", PasswordHash);

        var result = await cmd.ExecuteScalarAsync();
        int count = result != null ? Convert.ToInt32(result) : 0;

        return count > 0;
    }
    public async Task<bool> CreateEventAsync(Event newEvent)
    {
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        string sql = @"
        INSERT INTO events (title, description, event_date, location, category, points_reward, capacity) 
        VALUES (@Title, @Description, @EventDate, @Location, @Category, @PointsReward, @Capacit, @ImageUrl)";

        using var cmd = new MySqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@Title", newEvent.Title);
        cmd.Parameters.AddWithValue("@Description", newEvent.Description);
        cmd.Parameters.AddWithValue("@EventDate", newEvent.EventDate);
        cmd.Parameters.AddWithValue("@Location", newEvent.Location);
        cmd.Parameters.AddWithValue("@Category", newEvent.Category);
        cmd.Parameters.AddWithValue("@PointsReward", newEvent.PointsReward);
        cmd.Parameters.AddWithValue("@Capacity", newEvent.Capacity);
        cmd.Parameters.AddWithValue("@ImageUrl", newEvent.ImageUrl ?? "");

        int rowsAffected = await cmd.ExecuteNonQueryAsync();

        return rowsAffected > 0;
    }
    public async Task<bool> DeleteEventAsync(int eventId)
    {
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        string sql = "DELETE FROM events WHERE id = @Id";

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", eventId);

        // Если строка удалилась, rowsAffected будет равен 1, и метод вернет true
        int rowsAffected = await cmd.ExecuteNonQueryAsync();

        return rowsAffected > 0;
    }
    public async Task<List<Event>> GetEventsAsync()
    {
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        string sql = @"SELECT id, title, description, event_date, location, category, 
                          points_reward, capacity, image_url, created_at
                   FROM events ORDER BY event_date DESC";

        using var cmd = new MySqlCommand(sql, conn);
        var list = new List<Event>();

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Event
            {
                Id = reader.GetInt32("id"),
                Title = reader.GetString("title"),
                Description = reader.IsDBNull("description") ? "" : reader.GetString("description"),
                EventDate = reader.GetDateTime("event_date"),
                Location = reader.IsDBNull("location") ? "" : reader.GetString("location"),
                Category = reader.IsDBNull("category") ? "" : reader.GetString("category"),
                PointsReward = reader.GetInt32("points_reward"),
                Capacity = reader.GetInt32("capacity"),
                ImageUrl = reader.IsDBNull("image_url") ? "" : reader.GetString("image_url"),  // ←
                CreatedAt = reader.GetDateTime("created_at")
            });
        }
        return list;
    }
    // Вход для сотрудников (простой SHA-256)
    public async Task<Staff?> StaffLoginAsync(string username, string password)
    {
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        string sql = @"
        SELECT id, username, password_hash, full_name, role, created_at
        FROM staff
        WHERE username = @u AND password_hash = SHA2(@p, 256)";

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@u", username);
        cmd.Parameters.AddWithValue("@p", password);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Staff
            {
                Id = reader.GetInt32("id"),
                Username = reader.GetString("username"),
                PasswordHash = reader.GetString("password_hash"),
                FullName = reader.IsDBNull("full_name") ? "" : reader.GetString("full_name"),
                Role = reader.GetString("role"),
                // Email = reader.IsDBNull("email") ? "" : reader.GetString("email"),
                CreatedAt = reader.GetDateTime("created_at")
            };
        }
        return null;
    }

    // Список мерча
    public async Task<List<object>> GetMerchItemsAsync()
    {
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        string sql = @"SELECT id, title, description, price_points, price_rub, image_url, stock, category
                   FROM merch_items ORDER BY id DESC";

        using var cmd = new MySqlCommand(sql, conn);
        var list = new List<object>();

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new
            {
                id = reader.GetInt32("id"),
                title = reader.GetString("title"),
                description = reader.IsDBNull("description") ? "" : reader.GetString("description"),
                pricePoints = reader.GetInt32("price_points"),
                priceRub = reader.IsDBNull("price_rub") ? 0 : reader.GetInt32("price_rub"),
                imageUrl = reader.IsDBNull("image_url") ? "" : reader.GetString("image_url"),
                stock = reader.GetInt32("stock"),
                category = reader.IsDBNull("category") ? "" : reader.GetString("category")
            });
        }
        return list;
    }

    // Добавить мерч
    public async Task<bool> CreateMerchItemAsync(string title, string description, int pricePoints, int stock, string category, string imageUrl)
    {
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        string sql = @"
        INSERT INTO merch_items (title, description, price_points, price_rub, image_url, stock, category)
        VALUES (@t, @d, @pp, 0, @img, @s, @c)";

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@t", title);
        cmd.Parameters.AddWithValue("@d", description);
        cmd.Parameters.AddWithValue("@pp", pricePoints);
        cmd.Parameters.AddWithValue("@img", imageUrl);
        cmd.Parameters.AddWithValue("@s", stock);
        cmd.Parameters.AddWithValue("@c", category);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }
}
