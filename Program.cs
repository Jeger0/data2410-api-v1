using Microsoft.Data.SqlClient;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Ensure Students table exists on startup.
var connectionString = app.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    try
    {
        using var conn = new SqlConnection(connectionString);
        conn.Open();
        using var cmd = new SqlCommand("""
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Students')
            CREATE TABLE Students (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                Name NVARCHAR(100) NOT NULL,
                Course NVARCHAR(100) NOT NULL,
                Marks INT NOT NULL,
                Grade NVARCHAR(2) NULL
            )
            """, conn);
        cmd.ExecuteNonQuery();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Failed to initialize database on startup.");
    }
}

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { status = "running", api = "/api/Students" }));

app.MapGet("/health", (IConfiguration config) =>
{
    var cs = config.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(cs))
    {
        return Results.Ok(new { database = "No connection string found", hint = "Add DefaultConnection in App Service > Environment variables > Connection strings" });
    }

    try
    {
        using var conn = new SqlConnection(cs);
        conn.Open();
        return Results.Ok(new { database = "Connected", server = conn.DataSource });
    }
    catch (Exception ex)
    {
        return Results.Ok(new { database = "Failed", error = ex.Message });
    }
});

app.MapControllers();

app.Run();
