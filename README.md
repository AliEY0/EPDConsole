# EPD Console

## Overzicht

Dit project is een console-applicatie voor een huisartsenpraktijk. Met de applicatie kunnen patienten, artsen en afspraken worden beheerd. De data wordt opgeslagen met Entity Framework Core in een SQLite-database.

## Builden en starten

Build de solution vanuit de root van het project:

```powershell
dotnet build .\Chipsoft.Assignments.EPDConsole\Chipsoft.Assignments.EPDConsole.csproj
```

Start de applicatie met:

```powershell
dotnet run --project .\Chipsoft.Assignments.EPDConsole\Chipsoft.Assignments.EPDConsole.csproj
```

Voer de tests uit met:

```powershell
dotnet test .\Chipsoft.Assignments.Tests\Chipsoft.Assignments.Tests.csproj
```

## Clean Architecture

De solution is opgesplitst in lagen zodat verantwoordelijkheden gescheiden blijven:

- `Core.Domain` bevat de domeinmodellen
- `Application.Contracts` bevat interfaces en contracten
- `Application.Business` bevat businesslogica en validatie
- `Infrastructure.DataAccess` bevat Entity Framework en repository-implementaties
- `EPDConsole` bevat de console-interface

Hierdoor blijft de presentatie-laag los van de businesslogica en blijft de businesslaag los van de concrete database-implementatie.

## SOLID

De code is opgezet met de SOLID-principes als uitgangspunt:

- `Single Responsibility`: `Program.cs` behandelt alleen console-interactie, services bevatten businesslogica, repositories verzorgen data-access.
- `Open/Closed`: nieuwe regels of functionaliteit kunnen worden toegevoegd via extra services, validators of repository-methodes zonder de hele structuur te wijzigen.
- `Liskov Substitution`: de businesslaag werkt met interfaces, waardoor implementaties en test doubles onderling uitwisselbaar zijn.
- `Interface Segregation`: de gebruikte interfaces zijn klein en doelgericht, zoals `IPatientRepository`, `IDoctorRepository` en `IAppointmentRepository`.
- `Dependency Inversion`: de businesslaag hangt af van abstraheringen en niet direct van Entity Framework of SQLite.

## Validatie met FluentValidation

Validatie is ondergebracht in aparte validators:

- `PatientValidator`
- `DoctorValidator`
- `AppointmentValidator`

Daardoor staan validatieregels centraal op één plek en niet verspreid door de applicatie. Dit maakt de code overzichtelijker en beter testbaar. Een voorbeeld hiervan is de regel dat afspraken niet in het verleden mogen liggen.

## Dependency Injection

De applicatie gebruikt `Microsoft.Extensions.DependencyInjection` om afhankelijkheden te registreren en op te lossen.

Via Dependency Injection worden onder andere geregistreerd:

- repositories
- business services
- validators
- database-initialisatie

Daardoor hoeven objecten niet handmatig in `Program.cs` te worden opgebouwd en blijft de oplossing flexibeler en netter gestructureerd.

## Testen

De tests richten zich op de businesslaag, zoals:

- geldige en ongeldige invoer
- verwijderen met gekoppelde afspraken
- aanmaken van geldige en ongeldige afspraken

De tests in `Chipsoft.Assignments.Tests` zijn AI-generated en daarna opgenomen in het project als extra verificatie van de businessregels.
