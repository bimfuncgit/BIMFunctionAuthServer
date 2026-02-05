using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors();

// Настройки JSON для нечувствительности к регистру
var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

// Users from env
var usersJson = Environment.GetEnvironmentVariable("ALLOWED_USERS");
// Добавляем jsonOptions здесь!
var users = JsonSerializer.Deserialize<List<User>>(usersJson ?? "[]", jsonOptions) ?? new List<User>();

app.MapPost("/api/auth/login", (LoginRequest req) =>
{
    // Minimal API автоматически использует нечувствительный регистр для LoginRequest,
    // но сравнение в FirstOrDefault должно быть надежным.
    var user = users.FirstOrDefault(u =>
        string.Equals(u.Login, req.Login, StringComparison.OrdinalIgnoreCase) && 
        u.Password == req.Password);

    if (user is null)
    {
        return Results.Json(new
        {
            success = false,
            token   = string.Empty,
            user    = string.Empty,
            expires = DateTime.UtcNow,
            error   = "Логин или пароль неверны"
        });
    }

    var auth = new
    {
        success = true,
        token   = Guid.NewGuid().ToString("N"),
        user    = user.Login,
        expires = DateTime.UtcNow.AddDays(30),
        error   = string.Empty
    };

    return Results.Json(auth);
});

app.MapGet("/", () =>
    Results.Text("BIMFunction Auth Server v1.0 - Running ✓"));

app.MapGet("/health", () =>
    Results.Ok(new { status = "alive", timestamp = DateTime.UtcNow }));

app.Run();

record User(string Login, string Password);
record LoginRequest(string Login, string Password);
