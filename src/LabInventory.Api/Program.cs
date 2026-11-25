using LabInventory.Api.Data;
using LabInventory.Api.Endpoints;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// Validación nativa de Minimal APIs (.NET 10): aplica los atributos [Range], [MaxLength], etc.
// de los parámetros y devuelve 400 con ProblemDetails antes de entrar al handler.
builder.Services.AddValidation();

// Enums como texto en JSON ("Reagent", "Pending") en vez de números.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

// TimeProvider inyectado en vez de DateTime.Now: el "hoy" se puede fijar en los tests.
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Inventory")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    db.Database.EnsureCreated();
    SeedData.Seed(db, scope.ServiceProvider.GetRequiredService<TimeProvider>());
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapProductEndpoints();
app.MapLotEndpoints();
app.MapRestockOrderEndpoints();

app.Run();

// Expone Program a WebApplicationFactory en los tests de integración.
public partial class Program;
