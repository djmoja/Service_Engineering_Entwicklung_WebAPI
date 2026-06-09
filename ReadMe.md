# 1. Dokumentation der Entwicklungsumgebung

## Auswahl

Die Wahl fiel auf die C# Entwicklungsumgebung Rider,
da sie durch voran gegangene Uni Projekte bereits auf
auf dem Computer war und unser Team bereits Erfahrung in
der Entwicklung mit Rider hat.

## Benötigte Werkzeuge

- .Net 9 SDK
- Rider
- Swagger
- SQLite

## Installation

1. IDE von https://www.jetbrains.com/de-de/rider/herunterladen herunterladen
2. Heruntergeladene Datei ausführen

## Durchführung
1. neues Projekt (WebApi) anlegen
2. Pakete installieren (SwaggerUi, OpenApi, EF)
3. Model anlegen
4. AppDbContext anlegen
5. Endpunkte anlegen
6. Datenbank erzeugen
7. Daten einspielen

# 2. Analyse

### Welche Daten soll der WebService verwalten?
Autos mit:
- Model
- Marke
- PS Zahl
- Anzahl Türen
- Kraftstoff (Benzin, Elektro, Diesel)
- verfügbare Farben

### Welche Operationen werden benötigt?
- C(reate) = Autos können erstellt werden 
- R(ead) = Autos können zurückgegeben werden
- U(pdate) = Autos lassen sich bearbeiten
- D(elese) = Autos lassen sich löschen

### Wie funktioniert die/der Api benutzung/aufruf?
- Curl an die Endpunkte
- Postman
- SwaggerUi

### Besonderheiten
- (noch) keine Authentifizierung
- HTTPS
- keine Paginierung
- kein Caching

# 3. Design

## Endpunkte

### Create
- Post ("/api/cars")

### Update
- Put ("/api/cars/{id:guid}")

Braucht einen .json Body mit den entsprechenden Attributen des Autos.
> [!WARNING] 
> Funktioniert nur, wenn die ID vergeben ist.

### Read
#### a) für ein einzelnes Auto
- "/api/cars/{id:guid}"

> [!WARNING]
> Funktioniert nur, wenn die ID vergeben ist.

#### b) für alle Autos
"/api/cars"

### Delete
- Delete ("/api/cars/{id:guid}")

> [!WARNING]
> Funktioniert nur, wenn die ID vergeben ist.

# 3. Implementierung

## Abhängigkeiten

Es wird mit DotNet9.0 gearbeitet.
Abhängigkeiten/Pakete wurden via NuGet Paketmanager installiert.
Diese Pakete sind:

### Microsoft.EntityFramworkCore.InMemory V. 9.0.16 
Speichert Daten (nur) im Ram.
Wird noch auf SQLite umgebaut.

### Swashbuckle.AspNetCore V. 10.2.1
Zur automatischen Generierung einer Swagger / OpenAPI-Dokumentation

Zukünftig:
### Microsoft.EntityFrameworkCore.Sqlite
Zur Einbindung einer SQLite DB.

### Microsoft.EntityFrameworkCore.Tools
Hilfreiche Werkzeuge zum Erstellen von Datenbankschemen mithilfe von Migrationen.


## Model

```
namespace Api.Model;

public class Car
{
    public Guid Id { get; set; }
    public string Model { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string HorsePower { get; set; } = string.Empty;
    public int Doors { get; set; }
    public string Fuel { get; set; } = string.Empty;
    public List<string> Colors { get; set; } = new();
}
```

## Program.cs

```
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
```

## AppDbContext

```
using Microsoft.EntityFrameworkCore;
using Api.Model;   
namespace Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Car> Cars { get; set; }
}
```

# 4. Tests

- aktuell nur Klicktests in der SwaggerUI


