using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Api.Data;
using Api.Handler;
using Api.Model;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Car API", Version = "v2" });
    c.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Bitte geben Sie Ihren API-Key ein. Dieser wird als 'X-API-Key' Header mitgesendet.",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "X-API-Key",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(optionsBuilder =>
    optionsBuilder.UseSqlite("Data Source=mydatabase.db"));

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = ApiKeyAuthenticationHandler.SchemeName;
        options.DefaultChallengeScheme = ApiKeyAuthenticationHandler.SchemeName;
    })
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationHandler.SchemeName, null);

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "Car API v2");
    options.RoutePrefix = string.Empty;
});

app.MapGet("/api/cars", async (AppDbContext db, int page = 1, int pageSize = 10) =>
{
    var totalCount = await db.Cars.CountAsync();
    var cars = await db.Cars
        .OrderBy(c => c.Brand)
        .ThenBy(c => c.Model)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return Results.Ok(new
    {
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize,
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
        Items = cars
    });
});

app.MapGet("/api/cars/{id}", async (string id, AppDbContext db) =>
{
    var result = await db.Cars.FindAsync(id);
    return result is not null ? Results.Ok(result) : Results.NotFound();
});

app.MapPost("/api/cars", async (Car car, AppDbContext db) =>
{
    if (car.Id == "") car.Id = car.Brand+"-"+car.Model+"-"+car.ConstructionYear;
    db.Cars.Add(car);
    await db.SaveChangesAsync();
    return Results.Created($"/api/cars/{car.Id}", car);
}).RequireAuthorization();

app.MapPut("/api/cars/{id}", async (string id, Car inputCar, AppDbContext db) =>
{
    var car = await db.Cars.FindAsync(id);
    if (car is null) return Results.NotFound();
    car.Model = inputCar.Model;
    car.Brand = inputCar.Brand;
    car.HorsePower = inputCar.HorsePower;
    car.Doors = inputCar.Doors;
    car.Fuel = inputCar.Fuel;
    car.Colors = inputCar.Colors;
    car.ConstructionYear = inputCar.ConstructionYear;
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

app.MapDelete("/api/cars/{id}", async (string id, AppDbContext db) =>
{
    var car = await db.Cars.FindAsync(id);
    if (car is null) return Results.NotFound();
    db.Cars.Remove(car);
    await db.SaveChangesAsync();
    return Results.Ok();
}).RequireAuthorization();

app.Run();

public partial class Program { }
