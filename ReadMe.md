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
- SQL Version? (PSQL, SQLite)

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



