using Chipsoft.Assignments.Application.Business.Services;
using Chipsoft.Assignments.Application.Business.Validators;
using Chipsoft.Assignments.Core.Domain;
using FluentValidation;

namespace Chipsoft.Assignments.Tests;

public class PatientServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsPatientsSortedByName()
    {
        var patientRepository = new InMemoryPatientRepository();
        var appointmentRepository = new InMemoryAppointmentRepository();
        patientRepository.Items.AddRange(
        [
            CreatePatient(1, "Zara"),
            CreatePatient(2, "Anna"),
            CreatePatient(3, "Bram")
        ]);

        var service = CreateService(patientRepository, appointmentRepository);

        var patients = await service.GetAllAsync();

        Assert.Equal(["Anna", "Bram", "Zara"], patients.Select(patient => patient.Name));
    }

    [Fact]
    public async Task AddAsync_SavesPatient_WhenPatientIsValid()
    {
        var patientRepository = new InMemoryPatientRepository();
        var appointmentRepository = new InMemoryAppointmentRepository();
        var service = CreateService(patientRepository, appointmentRepository);

        await service.AddAsync(CreatePatient(1, "Jan Jansen"));

        Assert.Single(patientRepository.Items);
    }

    [Fact]
    public async Task AddAsync_ThrowsValidationException_WhenPatientIsInvalid()
    {
        var patientRepository = new InMemoryPatientRepository();
        var appointmentRepository = new InMemoryAppointmentRepository();
        var service = CreateService(patientRepository, appointmentRepository);

        var invalidPatient = CreatePatient(1, string.Empty);

        await Assert.ThrowsAsync<ValidationException>(() => service.AddAsync(invalidPatient));
        Assert.Empty(patientRepository.Items);
    }

    [Fact]
    public async Task DeleteAsync_RemovesPatient_WhenNoAppointmentsExist()
    {
        var patientRepository = new InMemoryPatientRepository();
        var appointmentRepository = new InMemoryAppointmentRepository();
        patientRepository.Items.Add(CreatePatient(1, "Jan Jansen"));

        var service = CreateService(patientRepository, appointmentRepository);

        await service.DeleteAsync(1);

        Assert.Empty(patientRepository.Items);
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenPatientHasAppointments()
    {
        var patientRepository = new InMemoryPatientRepository();
        var appointmentRepository = new InMemoryAppointmentRepository();
        patientRepository.Items.Add(CreatePatient(1, "Jan Jansen"));
        appointmentRepository.Items.Add(new Appointment
        {
            Id = 1,
            PatientId = 1,
            DoctorId = 2,
            Date = DateTime.Today
        });

        var service = CreateService(patientRepository, appointmentRepository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(1));

        Assert.Equal("Deze patient kan niet verwijderd worden omdat er afspraken aan gekoppeld zijn.", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenPatientDoesNotExist()
    {
        var patientRepository = new InMemoryPatientRepository();
        var appointmentRepository = new InMemoryAppointmentRepository();
        var service = CreateService(patientRepository, appointmentRepository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(99));

        Assert.Equal("Patient niet gevonden.", exception.Message);
    }

    private static PatientService CreateService(
        InMemoryPatientRepository patientRepository,
        InMemoryAppointmentRepository appointmentRepository)
    {
        return new PatientService(patientRepository, appointmentRepository, new PatientValidator());
    }

    private static Patient CreatePatient(int id, string name)
    {
        return new Patient
        {
            Id = id,
            Name = name,
            Address = "Dorpsstraat 1",
            PhoneNumber = "0612345678",
            Email = "jan@example.com",
            DateOfBirth = new DateTime(1990, 4, 25)
        };
    }
}
