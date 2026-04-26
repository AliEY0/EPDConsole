using Chipsoft.Assignments.Application.Business.Services;
using Chipsoft.Assignments.Application.Business.Validators;
using Chipsoft.Assignments.Core.Domain;
using FluentValidation;

namespace Chipsoft.Assignments.Tests;

public class AppointmentServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsAppointmentsSortedAndMapped()
    {
        var appointmentRepository = new InMemoryAppointmentRepository();
        var patientRepository = new InMemoryPatientRepository();
        var doctorRepository = new InMemoryDoctorRepository();

        patientRepository.Items.AddRange(
        [
            CreatePatient(1, "Anna"),
            CreatePatient(2, "Bram")
        ]);
        doctorRepository.Items.AddRange(
        [
            CreateDoctor(1, "Dr. Vos"),
            CreateDoctor(2, "Dr. Noor")
        ]);
        appointmentRepository.Items.AddRange(
        [
            new Appointment { Id = 2, PatientId = 2, DoctorId = 1, Date = DateTime.Now.AddDays(2) },
            new Appointment { Id = 1, PatientId = 1, DoctorId = 2, Date = DateTime.Now.AddDays(1) }
        ]);

        var service = CreateService(appointmentRepository, patientRepository, doctorRepository);

        var appointments = await service.GetAllAsync();

        Assert.Collection(
            appointments,
            appointment =>
            {
                Assert.Equal(1, appointment.Id);
                Assert.Equal("Anna", appointment.PatientName);
                Assert.Equal("Dr. Noor", appointment.DoctorName);
            },
            appointment =>
            {
                Assert.Equal(2, appointment.Id);
                Assert.Equal("Bram", appointment.PatientName);
                Assert.Equal("Dr. Vos", appointment.DoctorName);
            });
    }

    [Fact]
    public async Task GetAllAsync_UsesFallbackNames_WhenReferencesAreMissing()
    {
        var appointmentRepository = new InMemoryAppointmentRepository();
        var patientRepository = new InMemoryPatientRepository();
        var doctorRepository = new InMemoryDoctorRepository();
        appointmentRepository.Items.Add(new Appointment
        {
            Id = 1,
            PatientId = 99,
            DoctorId = 88,
            Date = DateTime.Today
        });

        var service = CreateService(appointmentRepository, patientRepository, doctorRepository);

        var appointments = await service.GetAllAsync();

        Assert.Single(appointments);
        Assert.Equal("Onbekende patient", appointments[0].PatientName);
        Assert.Equal("Onbekende arts", appointments[0].DoctorName);
    }

    [Fact]
    public async Task AddAsync_SavesAppointment_WhenReferencesExist()
    {
        var appointmentRepository = new InMemoryAppointmentRepository();
        var patientRepository = new InMemoryPatientRepository();
        var doctorRepository = new InMemoryDoctorRepository();
        patientRepository.Items.Add(CreatePatient(1, "Anna"));
        doctorRepository.Items.Add(CreateDoctor(1, "Dr. Vos"));

        var service = CreateService(appointmentRepository, patientRepository, doctorRepository);

        await service.AddAsync(new Appointment
        {
            Id = 1,
            PatientId = 1,
            DoctorId = 1,
            Date = DateTime.Now.AddDays(1)
        });

        Assert.Single(appointmentRepository.Items);
    }

    [Fact]
    public async Task AddAsync_ThrowsValidationException_WhenAppointmentIsInvalid()
    {
        var appointmentRepository = new InMemoryAppointmentRepository();
        var patientRepository = new InMemoryPatientRepository();
        var doctorRepository = new InMemoryDoctorRepository();
        var service = CreateService(appointmentRepository, patientRepository, doctorRepository);

        var invalidAppointment = new Appointment
        {
            Id = 1,
            PatientId = 0,
            DoctorId = 0,
            Date = default
        };

        await Assert.ThrowsAsync<ValidationException>(() => service.AddAsync(invalidAppointment));
        Assert.Empty(appointmentRepository.Items);
    }

    [Fact]
    public async Task AddAsync_ThrowsValidationException_WhenAppointmentIsInThePast()
    {
        var appointmentRepository = new InMemoryAppointmentRepository();
        var patientRepository = new InMemoryPatientRepository();
        var doctorRepository = new InMemoryDoctorRepository();
        patientRepository.Items.Add(CreatePatient(1, "Anna"));
        doctorRepository.Items.Add(CreateDoctor(1, "Dr. Vos"));

        var service = CreateService(appointmentRepository, patientRepository, doctorRepository);

        var exception = await Assert.ThrowsAsync<ValidationException>(() => service.AddAsync(new Appointment
        {
            Id = 1,
            PatientId = 1,
            DoctorId = 1,
            Date = DateTime.Now.AddMinutes(-5)
        }));

        Assert.Contains(exception.Errors, error => error.ErrorMessage == "Een afspraak mag niet in het verleden liggen.");
        Assert.Empty(appointmentRepository.Items);
    }

    [Fact]
    public async Task AddAsync_Throws_WhenPatientDoesNotExist()
    {
        var appointmentRepository = new InMemoryAppointmentRepository();
        var patientRepository = new InMemoryPatientRepository();
        var doctorRepository = new InMemoryDoctorRepository();
        doctorRepository.Items.Add(CreateDoctor(1, "Dr. Vos"));

        var service = CreateService(appointmentRepository, patientRepository, doctorRepository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddAsync(new Appointment
        {
            Id = 1,
            PatientId = 42,
            DoctorId = 1,
            Date = DateTime.Now.AddDays(1)
        }));

        Assert.Equal("Ongeldige patient of arts geselecteerd.", exception.Message);
    }

    [Fact]
    public async Task AddAsync_Throws_WhenDoctorDoesNotExist()
    {
        var appointmentRepository = new InMemoryAppointmentRepository();
        var patientRepository = new InMemoryPatientRepository();
        var doctorRepository = new InMemoryDoctorRepository();
        patientRepository.Items.Add(CreatePatient(1, "Anna"));

        var service = CreateService(appointmentRepository, patientRepository, doctorRepository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddAsync(new Appointment
        {
            Id = 1,
            PatientId = 1,
            DoctorId = 42,
            Date = DateTime.Now.AddDays(1)
        }));

        Assert.Equal("Ongeldige patient of arts geselecteerd.", exception.Message);
    }

    private static AppointmentService CreateService(
        InMemoryAppointmentRepository appointmentRepository,
        InMemoryPatientRepository patientRepository,
        InMemoryDoctorRepository doctorRepository)
    {
        return new AppointmentService(
            appointmentRepository,
            patientRepository,
            doctorRepository,
            new AppointmentValidator());
    }

    private static Patient CreatePatient(int id, string name)
    {
        return new Patient
        {
            Id = id,
            Name = name,
            Address = "Dorpsstraat 1",
            PhoneNumber = "0612345678",
            Email = "anna@example.com",
            DateOfBirth = new DateTime(1990, 4, 25)
        };
    }

    private static Doctor CreateDoctor(int id, string name)
    {
        return new Doctor
        {
            Id = id,
            Name = name,
            Address = "Praktijkstraat 10"
        };
    }
}
