using Chipsoft.Assignments.Application.Business.Configuration;
using Chipsoft.Assignments.Application.Business.Models;
using Chipsoft.Assignments.Application.Business.Services;
using Chipsoft.Assignments.Core.Domain;
using Chipsoft.Assignments.Infrastructure.DataAccess.Configuration;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace Chipsoft.Assignments.EPDConsole;

public class Program
{
    private static readonly CultureInfo[] SupportedCultures =
    [
        new("nl-NL"),
        CultureInfo.InvariantCulture
    ];

    private static readonly ServiceProvider ServiceProvider = BuildServices();

    public static async Task Main(string[] args)
    {
        ExecuteWithScope<DatabaseInitializer>(databaseInitializer => databaseInitializer.EnsureCreated());

        while (await ShowMenuAsync())
        {
        }
    }

    private static ServiceProvider BuildServices()
    {
        var services = new ServiceCollection();

        services.AddDbContext<EPDDbContext>(options => options.UseSqlite("Data Source=epd.db"));
        services.RegisterEpdRepositories();
        services.RegisterBusinessServices();

        return services.BuildServiceProvider();
    }

    private static async Task<bool> ShowMenuAsync()
    {
        Console.Clear();
        PrintLogo();

        Console.WriteLine();
        Console.WriteLine("1 - Patient toevoegen");
        Console.WriteLine("2 - Patienten verwijderen");
        Console.WriteLine("3 - Arts toevoegen");
        Console.WriteLine("4 - Arts verwijderen");
        Console.WriteLine("5 - Afspraak toevoegen");
        Console.WriteLine("6 - Afspraken inzien");
        Console.WriteLine("7 - Sluiten");
        Console.WriteLine("8 - Reset db");

        if (!int.TryParse(Console.ReadLine(), out var option))
            return true;

        switch (option)
        {
            case 1:
                await ExecuteCommandAsync(AddPatientAsync);
                return true;
            case 2:
                await ExecuteCommandAsync(DeletePatientAsync);
                return true;
            case 3:
                await ExecuteCommandAsync(AddDoctorAsync);
                return true;
            case 4:
                await ExecuteCommandAsync(DeleteDoctorAsync);
                return true;
            case 5:
                await ExecuteCommandAsync(AddAppointmentAsync);
                return true;
            case 6:
                await ExecuteCommandAsync(ShowAppointmentsAsync);
                return true;
            case 7:
                return false;
            case 8:
                ExecuteCommand(ResetDatabase);
                return true;
            default:
                return true;
        }
    }

    private static async Task AddPatientAsync()
    {
        ShowHeader("Patient toevoegen");

        var patient = new Patient
        {
            Name = ReadRequiredText("Naam: "),
            Address = ReadRequiredText("Adres: "),
            PhoneNumber = ReadRequiredText("Telefoonnummer: "),
            Email = ReadRequiredText("E-mail: "),
            DateOfBirth = ReadDate("Geboortedatum (bijv. 26-04-2026): ").Date
        };

        await ExecuteWithScopeAsync<PatientService>(patientService => patientService.AddAsync(patient));
        ShowMessage("Patient toegevoegd.");
    }

    private static async Task DeletePatientAsync()
    {
        ShowHeader("Patient verwijderen");

        var patients = await ExecuteWithScopeAsync<PatientService, IReadOnlyList<Patient>>(patientService => patientService.GetAllAsync());
        if (!EnsureItemsAvailable(patients, "Er zijn geen patienten om te verwijderen."))
            return;

        PrintOptions("Patienten", patients, patient => patient.Id, patient => patient.Name);

        var patient = SelectById(patients, "Kies het ID van de patient: ", currentPatient => currentPatient.Id);
        if (patient is null)
            throw new InvalidOperationException("Patient niet gevonden.");

        await ExecuteWithScopeAsync<PatientService>(patientService => patientService.DeleteAsync(patient.Id));
        ShowMessage("Patient verwijderd.");
    }

    private static async Task AddDoctorAsync()
    {
        ShowHeader("Arts toevoegen");

        var doctor = new Doctor
        {
            Name = ReadRequiredText("Naam: "),
            Address = ReadRequiredText("Adres: ")
        };

        await ExecuteWithScopeAsync<DoctorService>(doctorService => doctorService.AddAsync(doctor));
        ShowMessage("Arts toegevoegd.");
    }

    private static async Task DeleteDoctorAsync()
    {
        ShowHeader("Arts verwijderen");

        var doctors = await ExecuteWithScopeAsync<DoctorService, IReadOnlyList<Doctor>>(doctorService => doctorService.GetAllAsync());
        if (!EnsureItemsAvailable(doctors, "Er zijn geen artsen om te verwijderen."))
            return;

        PrintOptions("Artsen", doctors, doctor => doctor.Id, doctor => doctor.Name);

        var doctor = SelectById(doctors, "Kies het ID van de arts: ", currentDoctor => currentDoctor.Id);
        if (doctor is null)
            throw new InvalidOperationException("Arts niet gevonden.");

        await ExecuteWithScopeAsync<DoctorService>(doctorService => doctorService.DeleteAsync(doctor.Id));
        ShowMessage("Arts verwijderd.");
    }

    private static async Task AddAppointmentAsync()
    {
        ShowHeader("Afspraak toevoegen");

        var patients = await ExecuteWithScopeAsync<PatientService, IReadOnlyList<Patient>>(patientService => patientService.GetAllAsync());
        if (!EnsureItemsAvailable(patients, "Voeg eerst minimaal een patient toe."))
            return;

        var doctors = await ExecuteWithScopeAsync<DoctorService, IReadOnlyList<Doctor>>(doctorService => doctorService.GetAllAsync());
        if (!EnsureItemsAvailable(doctors, "Voeg eerst minimaal een arts toe."))
            return;

        PrintOptions("Patienten", patients, patient => patient.Id, patient => patient.Name);
        PrintOptions("Artsen", doctors, doctor => doctor.Id, doctor => doctor.Name);

        var patient = SelectById(patients, "Kies het ID van de patient: ", currentPatient => currentPatient.Id);
        var doctor = SelectById(doctors, "Kies het ID van de arts: ", currentDoctor => currentDoctor.Id);

        if (patient is null || doctor is null)
            throw new InvalidOperationException("Ongeldige patient of arts geselecteerd.");

        var appointment = new Appointment
        {
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            Date = ReadDate("Datum en tijd (bijv. 27-04-2026 14:30): ")
        };

        await ExecuteWithScopeAsync<AppointmentService>(appointmentService => appointmentService.AddAsync(appointment));
        ShowMessage("Afspraak toegevoegd.");
    }

    private static async Task ShowAppointmentsAsync()
    {
        ShowHeader("Afspraken");

        var appointments = await ExecuteWithScopeAsync<AppointmentService, IReadOnlyList<AppointmentOverview>>(appointmentService => appointmentService.GetAllAsync());
        if (!EnsureItemsAvailable(appointments, "Er zijn nog geen afspraken."))
            return;

        foreach (var appointment in appointments)
        {
            Console.WriteLine(
                $"{appointment.Id} - {appointment.Date:dd-MM-yyyy HH:mm} - Patient: {appointment.PatientName} - Arts: {appointment.DoctorName}");
        }

        WaitForUser();
    }

    private static void ResetDatabase()
    {
        ExecuteWithScope<DatabaseInitializer>(databaseInitializer => databaseInitializer.Reset());
        ShowMessage("Database is opnieuw aangemaakt.");
    }

    private static async Task ExecuteCommandAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (ValidationException validationException)
        {
            ShowValidationErrors(validationException);
        }
        catch (DbUpdateException)
        {
            ShowMessage("Er ging iets mis bij het opslaan in de database.");
        }
        catch (InvalidOperationException invalidOperationException)
        {
            ShowMessage(invalidOperationException.Message);
        }
    }

    private static void ExecuteCommand(Action action)
    {
        try
        {
            action();
        }
        catch (DbUpdateException)
        {
            ShowMessage("Er ging iets mis bij het opslaan in de database.");
        }
        catch (InvalidOperationException invalidOperationException)
        {
            ShowMessage(invalidOperationException.Message);
        }
    }
    private static void ExecuteWithScope<TService>(Action<TService> action)
        where TService : notnull
    {
        using var scope = ServiceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        action(service);
    }

    private static async Task<T> ExecuteWithScopeAsync<TService, T>(Func<TService, Task<T>> action)
        where TService : notnull
    {
        using var scope = ServiceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        return await action(service);
    }

    private static async Task ExecuteWithScopeAsync<TService>(Func<TService, Task> action)
        where TService : notnull
    {
        using var scope = ServiceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        await action(service);
    }

    private static void PrintLogo()
    {
        if (!File.Exists("logo.txt"))
            return;

        foreach (var line in File.ReadAllLines("logo.txt"))
            Console.WriteLine(line);
    }

    private static void ShowHeader(string title)
    {
        Console.Clear();
        Console.WriteLine(title);
        Console.WriteLine();
    }

    private static void PrintOptions<T>(
        string title,
        IEnumerable<T> items,
        Func<T, int> getId,
        Func<T, string> displayText)
    {
        Console.WriteLine(title + ":");

        foreach (var item in items)
            Console.WriteLine($"{getId(item)} - {displayText(item)}");

        Console.WriteLine();
    }

    private static T? SelectById<T>(IEnumerable<T> items, string prompt, Func<T, int> getId)
        where T : class
    {
        var id = ReadInt(prompt);
        return items.FirstOrDefault(item => getId(item) == id);
    }

    private static bool EnsureItemsAvailable<T>(IReadOnlyCollection<T> items, string message)
    {
        if (items.Count > 0)
            return true;

        ShowMessage(message);
        return false;
    }

    private static void ShowValidationErrors(ValidationException validationException)
    {
        foreach (var error in validationException.Errors)
            Console.WriteLine(error.ErrorMessage);

        WaitForUser();
    }

    private static void ShowMessage(string message)
    {
        Console.WriteLine(message);
        WaitForUser();
    }

    private static DateTime ReadDate(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();

            foreach (var culture in SupportedCultures)
            {
                if (DateTime.TryParse(input, culture, DateTimeStyles.AllowWhiteSpaces, out var result))
                    return result;
            }

            Console.WriteLine("Voer een geldige datum in, bijvoorbeeld 25-04-2026 14:30.");
        }
    }

    private static string ReadRequiredText(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var value = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();

            Console.WriteLine("Waarde is verplicht.");
        }
    }

    private static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);

            if (int.TryParse(Console.ReadLine(), out var value) && value > 0)
                return value;

            Console.WriteLine("Voer een geldig positief nummer in.");
        }
    }

    private static void WaitForUser()
    {
        Console.WriteLine();
        Console.WriteLine("Druk op Enter om verder te gaan.");
        Console.ReadLine();
    }
}
