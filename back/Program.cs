using MailKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.Data;
using MimeKit;
using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы
builder.Services.AddHostedService<LeaderboardCacheService>();
builder.Services.AddSingleton<AuthRepository>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();

// --- Endpoints ---
app.MapPost("/request-code", async (RequestCodeRequest request, AuthRepository repo, EmailService email) =>
{
    try
    {
        Console.WriteLine($">>> /request-code: login={request.Login}");

        var user = await repo.GetUserByLoginAsync(request.Login);
        Console.WriteLine($">>> user: {(user == null ? "NULL" : $"id={user.Id}, email={user.Email}")}");
        if (user == null) return Results.NotFound("User not found");

        var code = Random.Shared.Next(100000, 999999).ToString();
        Console.WriteLine($">>> code: {code}");

        var saved = await repo.SaveCodeAsync(user.Id, code, DateTime.Now.AddMinutes(5));
        Console.WriteLine($">>> saved: {saved}");
        if (!saved) return Results.StatusCode(500);

        await email.SendCodeAsync(user.Email, code);
        Console.WriteLine($">>> email done");
        return Results.Ok("Code sent");
    }
    catch (Exception ex)
    {
        Console.WriteLine(">>> /request-code CRASH: " + ex);
        return Results.StatusCode(500);
    }
});

app.MapPost("/verify-code", async (VerifyCodeRequest request, AuthRepository repo) =>
{
    var user = await repo.GetUserByLoginAsync(request.Login);
    if (user == null) return Results.NotFound("User not found");

    var validCode = await repo.GetValidCodeAsync(user.Id, request.Code);
    if (validCode == null) return Results.BadRequest("Invalid or expired code");

    await repo.MarkCodeAsUsedAsync(validCode.Id);
    return Results.Ok("Access granted");
});
app.MapPost("/register", async (RegisterRequest request, AuthRepository repo) =>
{
    if (string.IsNullOrWhiteSpace(request.Username) ||
        string.IsNullOrWhiteSpace(request.Email))
    {
        return Results.BadRequest("Username and email required");
    }

    var exists = await repo.UserExistsAsync(request.Username, request.Email);

    if (exists)
    {
        return Results.Conflict("User already exists");
    }

    var created = await repo.CreateUserAsync(request.Username, request.Email);

    if (!created)
    {
        return Results.StatusCode(500);
    }

    return Results.Ok("User created");
});
app.MapGet("/leaderboard", () =>
{
    return Results.Ok(LeaderboardCacheService.TopPlayers);
});
app.MapPost("/staff-login", async (StaffLoginRequest req, AuthRepository repo) =>
{
    var staff = await repo.StaffLoginAsync(req.Username, req.Password);
    if (staff == null) return Results.Unauthorized();
    return Results.Ok(new { staff.Username, staff.FullName, staff.Role });
});

app.MapGet("/merch", async (AuthRepository repo) =>
    Results.Ok(await repo.GetMerchItemsAsync()));

app.MapPost("/merch", async (MerchRequest req, AuthRepository repo) =>
{
    var ok = await repo.CreateMerchItemAsync(
        req.Title, req.Description, req.PricePoints,
        req.Stock, req.Category, req.ImageUrl);
    return ok ? Results.Ok(new { ok = true }) : Results.StatusCode(500);
});
app.MapGet("/events", async (AuthRepository repo) =>
    Results.Ok(await repo.GetEventsAsync()));

app.MapPost("/events", async (Event e, AuthRepository repo) =>
{
    var ok = await repo.CreateEventAsync(e);
    return ok ? Results.Ok(new { ok = true }) : Results.StatusCode(500);
});

app.MapDelete("/events/{id}", async (int id, AuthRepository repo) =>
{
    var ok = await repo.DeleteEventAsync(id);
    return ok ? Results.Ok(new { ok = true }) : Results.NotFound();
});
app.MapGet("/profile/{login}", async (string login, AuthRepository repo) =>
{
    var user = await repo.GetFullUserAsync(login);
    return user == null ? Results.NotFound() : Results.Ok(user);
});
app.Run();


// --- DTOs ---
public record RequestCodeRequest(string Login);
public record VerifyCodeRequest(string Login, string Code);
public record RegisterRequest(string Username, string Email);
public record StaffLoginRequest(string Username, string Password);
public record MerchRequest(string Title, string Description, int PricePoints, int Stock, string Category, string ImageUrl);