using System.Text.Json;
record User(string Login, string Password);
record LoginRequest(string Login, string Password);
var builder = WebApplication.CreateBuilder(args);
// CORS для работы с внешними клиентами
builder.Services.AddCors(options =>
{
options.AddDefaultPolicy(policy =>
{
policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
});
});
var app = builder.Build();
app.UseCors();
// Простое хранилище пользователей (логин -> пароль)
var usersJson = Environment.GetEnvironmentVariable("ALLOWED_USERS");
var users = JsonSerializer.Deserialize<List<User>>(usersJson ?? "[]") ?? new List<User>();

// Эндпоинт для авторизации
app.MapPost("/api/auth/login", (LoginRequest req) =>
{
    var user = users.FirstOrDefault(u =>
        u.Login == req.Login && u.Password == req.Password);

    if (user is null)
    {
        return Results.Json(new
        {
            success = false,
            error = "Логин или пароль неверны"
        });
    }

    var auth = new
    {
        success = true,
        token = Guid.NewGuid().ToString("N"),
        user = user.Login,
        expires = DateTime.UtcNow.AddHours(12)
    };

    return Results.Json(auth);
});
// Главная страница (для проверки работы)
app.MapGet("/", () => Results.Text("BIMFunction Auth Server v1.0 - Running ✓"));
// Health check для автопинга (чтобы сервер не засыпал)
app.MapGet("/health", () => Results.Ok(new { status = "alive", timestamp = DateTime.UtcNow }));
app.Run();
record LoginRequest(string Login, string Password);
