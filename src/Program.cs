using System.Data;
using Dapper;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// Vars de entorno
var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
var connString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? "Server=mysql;Port=3306;Database=appdb;User ID=appuser;Password=apppass;";

// Logging básico
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

app.MapGet("/", () => new {
    message = "TP2 Docker - Minimal API (MySQL)",
    environment = env,
    db = "MySQL",
    endpoints = new [] { "/health", "/todos", "/seed" }
});

app.MapGet("/health", () => "ok");

// Crea tabla y datos de ejemplo si no existen
app.MapPost("/seed", async () =>
{
    await using var conn = new MySqlConnection(connString);
    await conn.ExecuteAsync(@"
        CREATE TABLE IF NOT EXISTS todos(
            id INT AUTO_INCREMENT PRIMARY KEY,
            title TEXT NOT NULL
        );
        INSERT INTO todos(title) VALUES ('primer todo'), ('segundo todo');
    ");
    return Results.Ok(new { inserted = true });
});

app.MapGet("/todos", async () =>
{
    await using var conn = new MySqlConnection(connString);
    var todos = await conn.QueryAsync<Todo>("SELECT id, title FROM todos ORDER BY id;");
    return Results.Ok(todos);
});

app.MapPost("/todos", async (TodoCreate req) =>
{
    await using var conn = new MySqlConnection(connString);
    var id = await conn.ExecuteScalarAsync<long>(
        "INSERT INTO todos(title) VALUES (@title); SELECT LAST_INSERT_ID();", new { req.title });
    return Results.Created($"/todos/{id}", new { id, req.title });
});

app.Run();

record Todo(int id, string title);
record TodoCreate(string title);
