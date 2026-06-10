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

# 3. Implementierung der API (ohne Sicherheit)

## Abhängigkeiten

Es wird mit DotNet9.0 gearbeitet.
Abhängigkeiten/Pakete wurden via NuGet Paketmanager installiert.
Diese Pakete sind:

### Swashbuckle.AspNetCore V. 6.5.0
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

# 4. Implementierung mit Sicherheit

## ApiKeyAuthenticationHandler.cs

```
namespace Api.Handler;

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
private readonly IConfiguration _configuration;
public const string SchemeName = "ApiKey";

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-API-Key", out var extractedApiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("API-Key fehlt im Header."));
        }

        var apiKey = _configuration["ApiSettings:Key"];
        if (!apiKey.Equals(extractedApiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("Ungültiger API-Key."));
        }

        var claims = new[] { new Claim(ClaimTypes.Name, "ApiUser") };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
```

## Programm.cs
```
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = ApiKeyAuthenticationHandler.SchemeName;
    options.DefaultChallengeScheme = ApiKeyAuthenticationHandler.SchemeName;
})
.AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
    ApiKeyAuthenticationHandler.SchemeName, null);

builder.Services.AddAuthorization();
...
app.UseAuthentication();
app.UseAuthorization();
...
app.MapPost(...).RequireAuthorization();
app.MapPut(...).RequireAuthorization();
app.MapDelete(...).RequireAuthorization();
...
// Autho. Button in SwaggerUI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "Car API", 
        Version = "v1" 
    });
    
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
```

## appsettings.json
```
{
  "ApiSettings": {
    "Key": "f47ac10b-58cc-4372-a567-0e02b2c3d479"
  }
}
```

# 5. Tests

- aktuell nur Klicktests in der SwaggerUI


