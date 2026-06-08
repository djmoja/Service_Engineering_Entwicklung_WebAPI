using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Model;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("database"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/cars", async (AppDbContext db) =>
    await db.Cars.ToListAsync());

app.MapGet("/api/cars/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var result = await db.Cars.FindAsync(id);
    return result is not null ? Results.Ok(result) : Results.NotFound();
});

app.MapPost("/api/cars", async (Car car, AppDbContext db) =>
{
    if (car.Id == Guid.Empty)
        car.Id = Guid.NewGuid();

    db.Cars.Add(car);
    await db.SaveChangesAsync();
    return Results.Created($"/api/cars/{car.Id}", car);
});

app.MapPut("/api/cars/{id:guid}", async (Guid id, Car inputCar, AppDbContext db) =>
{
    var car = await db.Cars.FindAsync(id);
    if (car is null) return Results.NotFound();

    car.Model = inputCar.Model;
    car.Brand = inputCar.Brand;
    car.HorsePower = inputCar.HorsePower;
    car.Doors = inputCar.Doors;
    car.Fuel = inputCar.Fuel;
    car.Colors = inputCar.Colors;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/api/cars/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var car = await db.Cars.FindAsync(id);
    if (car is null) return Results.NotFound();

    db.Cars.Remove(car);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.Run();