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
    private static readonly ServiceProvider ServiceProvider = BuildServices();

    private static ServiceProvider BuildServices()
    {
        var services = new ServiceCollection();

        services.AddDbContext<EPDDbContext>(options =>
            options.UseSqlite("Data Source=epd.db"));

        services.RegisterEpdRepositories();
        services.RegisterBusinessServices();

        return services.BuildServiceProvider();
    }

    static void Main(string[] args)
    {
        ExecuteWithScope<DatabaseInitializer>(db => db.EnsureCreated());
        ShowMenu();

        ServiceProvider.Dispose();
    }

    public static void ShowMenu()
    {
        bool doorgaan = true;

        while (doorgaan)
        {
            Console.Clear();

            if (File.Exists("logo.txt"))
            {
                foreach (var line in File.ReadAllLines("logo.txt"))
                    Console.WriteLine(line);
            }

            Console.WriteLine();
            Console.WriteLine("1 - Patient toevoegen");
            Console.WriteLine("2 - Patienten verwijderen");
            Console.WriteLine("3 - Arts toevoegen");
            Console.WriteLine("4 - Arts verwijderen");
            Console.WriteLine("5 - Afspraak toevoegen");
            Console.WriteLine("6 - Afspraken inzien");
            Console.WriteLine("7 - Sluiten");
            Console.WriteLine("8 - Reset db");

            if (int.TryParse(Console.ReadLine(), out int option))
            {
                switch (option)
                {
                    case 1:
                        AddPatient();
                        break;
                    case 2:
                        DeletePatient();
                        break;
                    case 3:
                        AddPhysician();
                        break;
                    case 4:
                        DeletePhysician();
                        break;
                    case 5:
                        AddAppointment();
                        break;
                    case 6:
                        ShowAppointment();
                        break;
                    case 7:
                        doorgaan = false;
                        break;
                    case 8:
                        ExecuteWithScope<DatabaseInitializer>(db => db.Reset());
                        ShowMessage("Database is opnieuw aangemaakt.");
                        break;
                }
            }
        }
    }

    private static void AddPatient()
    {
        Console.Clear();
        Console.WriteLine("Patient toevoegen");

        var patient = new Patient
        {
            Name = ReadRequiredText("Naam: "),
            Address = ReadRequiredText("Adres: "),
            PhoneNumber = ReadRequiredText("Telefoonnummer: "),
            Email = ReadRequiredText("E-mail: "),
            DateOfBirth = ReadDate("Geboortedatum (bijv. 25-04-1990): ").Date
        };

        ExecuteWithScope<PatientService>(service =>
            service.AddAsync(patient).GetAwaiter().GetResult());

        ShowMessage("Patient toegevoegd.");
    }

    private static void DeletePatient()
    {
        Console.Clear();
        Console.WriteLine("Patient verwijderen");

        var patients = ExecuteWithScope<PatientService, IReadOnlyList<Patient>>(service =>
            service.GetAllAsync().GetAwaiter().GetResult());

        if (patients.Count == 0)
        {
            ShowMessage("Er zijn geen patienten om te verwijderen.");
            return;
        }

        PrintPatients(patients);

        int id = ReadInt("Kies het ID van de patient: ");

        ExecuteWithScope<PatientService>(service =>
            service.DeleteAsync(id).GetAwaiter().GetResult());

        ShowMessage("Patient verwijderd.");
    }

    private static void AddPhysician()
    {
        Console.Clear();
        Console.WriteLine("Arts toevoegen");

        var doctor = new Doctor
        {
            Name = ReadRequiredText("Naam: "),
            Address = ReadRequiredText("Adres: ")
        };

        ExecuteWithScope<DoctorService>(service =>
            service.AddAsync(doctor).GetAwaiter().GetResult());

        ShowMessage("Arts toegevoegd.");
    }

    private static void DeletePhysician()
    {
        Console.Clear();
        Console.WriteLine("Arts verwijderen");

        var doctors = ExecuteWithScope<DoctorService, IReadOnlyList<Doctor>>(service =>
            service.GetAllAsync().GetAwaiter().GetResult());

        if (doctors.Count == 0)
        {
            ShowMessage("Er zijn geen artsen om te verwijderen.");
            return;
        }

        PrintDoctors(doctors);

        int id = ReadInt("Kies het ID van de arts: ");

        ExecuteWithScope<DoctorService>(service =>
            service.DeleteAsync(id).GetAwaiter().GetResult());

        ShowMessage("Arts verwijderd.");
    }

    private static void AddAppointment()
    {
        Console.Clear();
        Console.WriteLine("Afspraak toevoegen");

        var patients = ExecuteWithScope<PatientService, IReadOnlyList<Patient>>(service =>
            service.GetAllAsync().GetAwaiter().GetResult());

        var doctors = ExecuteWithScope<DoctorService, IReadOnlyList<Doctor>>(service =>
            service.GetAllAsync().GetAwaiter().GetResult());

        if (patients.Count == 0)
        {
            ShowMessage("Voeg eerst minimaal een patient toe.");
            return;
        }

        if (doctors.Count == 0)
        {
            ShowMessage("Voeg eerst minimaal een arts toe.");
            return;
        }

        PrintPatients(patients);
        int patientId = ReadInt("Kies het ID van de patient: ");

        PrintDoctors(doctors);
        int doctorId = ReadInt("Kies het ID van de arts: ");

        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            Date = ReadDate("Datum en tijd (bijv. 25-04-2026 14:30): ")
        };

        try
        {
            ExecuteWithScope<AppointmentService>(service =>
                service.AddAsync(appointment).GetAwaiter().GetResult());

            ShowMessage("Afspraak toegevoegd.");
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                Console.WriteLine(error.ErrorMessage);
            }

            WaitForUser();
        }
        catch (Exception ex)
        {
            ShowMessage($"Er ging iets mis: {ex.Message}");
        }
    }

    private static void ShowAppointment()
    {
        Console.Clear();
        Console.WriteLine("Afspraken");

        var appointments = ExecuteWithScope<AppointmentService, IReadOnlyList<AppointmentOverview>>(service =>
            service.GetAllAsync().GetAwaiter().GetResult());

        if (appointments.Count == 0)
        {
            ShowMessage("Er zijn nog geen afspraken.");
            return;
        }

        foreach (var appointment in appointments)
        {
            Console.WriteLine(
                $"{appointment.Id} - {appointment.Date:dd-MM-yyyy HH:mm} - Patient: {appointment.PatientName} - Arts: {appointment.DoctorName}");
        }

        WaitForUser();
    }

    private static void PrintPatients(IEnumerable<Patient> patients)
    {
        Console.WriteLine("Patienten:");

        foreach (var patient in patients)
            Console.WriteLine($"{patient.Id} - {patient.Name}");

        Console.WriteLine();
    }

    private static void PrintDoctors(IEnumerable<Doctor> doctors)
    {
        Console.WriteLine("Artsen:");

        foreach (var doctor in doctors)
            Console.WriteLine($"{doctor.Id} - {doctor.Name}");

        Console.WriteLine();
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

            if (int.TryParse(Console.ReadLine(), out int value) && value > 0)
                return value;

            Console.WriteLine("Voer een geldig positief nummer in.");
        }
    }

    private static DateTime ReadDate(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);

            if (DateTime.TryParse(
                    Console.ReadLine(),
                    new CultureInfo("nl-NL"),
                    DateTimeStyles.AllowWhiteSpaces,
                    out DateTime date))
            {
                return date;
            }

            Console.WriteLine("Voer een geldige datum in, bijvoorbeeld 25-04-2026 14:30.");
        }
    }

    private static T ExecuteWithScope<TService, T>(Func<TService, T> action)
        where TService : notnull
    {
        using var scope = ServiceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        return action(service);
    }

    private static void ExecuteWithScope<TService>(Action<TService> action)
        where TService : notnull
    {
        using var scope = ServiceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        action(service);
    }

    private static void ShowMessage(string message)
    {
        Console.WriteLine(message);
        WaitForUser();
    }

    private static void WaitForUser()
    {
        Console.WriteLine();
        Console.WriteLine("Druk op Enter om verder te gaan.");
        Console.ReadLine();
    }
}