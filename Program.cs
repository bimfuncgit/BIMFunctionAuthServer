using System.Text.Json;
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
var users = new Dictionary<string, string>
{
{ "user@company.com", "secret123" },
{ "admin@bimfunction.com", "admin456" },
{ "test@test.com", "test" }
};
// Эндпоинт для авторизации
app.MapPost("/api/auth/login", (LoginRequest req) =>
{
// Проверяем, что логин и пароль не пустые
if (string.IsNullOrWhiteSpace(req.Login) || string.IsNullOrWhiteSpace(req.Password))
{
return Results.Json(new { success = false, error = "Login and password required" }, statusCode: 400);
}
// Проверяем логин и пароль
if (users.TryGetValue(req.Login, out var password) && password == req.Password)
{
    return Results.Json(new
    {
        success = true,
        token = Guid.NewGuid().ToString(),
        user = req.Login,
        expires = DateTime.UtcNow.AddMonths(1).ToString("O")
    });
}

return Results.Json(new { success = false, error = "Invalid login or password" }, statusCode: 401);

});
// Главная страница (для проверки работы)
app.MapGet("/", () => Results.Text("BIMFunction Auth Server v1.0 - Running ✓"));
// Health check для автопинга (чтобы сервер не засыпал)
app.MapGet("/health", () => Results.Ok(new { status = "alive", timestamp = DateTime.UtcNow }));
app.Run();
record LoginRequest(string Login, string Password);
