using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.Data;
using MimeKit;
using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы
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
    var user = await repo.GetUserByLoginAsync(request.Login);
    if (user == null) return Results.NotFound("User not found");

    var code = Random.Shared.Next(100000, 999999).ToString();
    var saved = await repo.SaveCodeAsync(user.Id, code, DateTime.UtcNow.AddMinutes(5));
    if (!saved) return Results.StatusCode(500);

    await email.SendCodeAsync(user.Email, code);
    return Results.Ok("Code sent");
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

app.Run();

// --- DTOs ---
public record RequestCodeRequest(string Login);
public record VerifyCodeRequest(string Login, string Code);
public record RegisterRequest(string Username, string Email);