using Chipsoft.Assignments.Application.Business.Services;
using Chipsoft.Assignments.Application.Business.Validators;
using Chipsoft.Assignments.Core.Domain;
using FluentValidation;

namespace Chipsoft.Assignments.Tests;

public class DoctorServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsDoctorsSortedByName()
    {
        var doctorRepository = new InMemoryDoctorRepository();
        var appointmentRepository = new InMemoryAppointmentRepository();
        doctorRepository.Items.AddRange(
        [
            CreateDoctor(1, "Zorg"),
            CreateDoctor(2, "Anita"),
            CreateDoctor(3, "Bert")
        ]);

        var service = CreateService(doctorRepository, appointmentRepository);

        var doctors = await service.GetAllAsync();

        Assert.Equal(["Anita", "Bert", "Zorg"], doctors.Select(doctor => doctor.Name));
    }

    [Fact]
    public async Task AddAsync_SavesDoctor_WhenDoctorIsValid()
    {
        var doctorRepository = new InMemoryDoctorRepository();
        var appointmentRepository = new InMemoryAppointmentRepository();
        var service = CreateService(doctorRepository, appointmentRepository);

        await service.AddAsync(CreateDoctor(1, "Dr. De Vries"));

        Assert.Single(doctorRepository.Items);
    }

    [Fact]
    public async Task AddAsync_ThrowsValidationException_WhenDoctorIsInvalid()
    {
        var doctorRepository = new InMemoryDoctorRepository();
        var appointmentRepository = new InMemoryAppointmentRepository();
        var service = CreateService(doctorRepository, appointmentRepository);

        var invalidDoctor = new Doctor
        {
            Id = 1,
            Name = string.Empty,
            Address = string.Empty
        };

        await Assert.ThrowsAsync<ValidationException>(() => service.AddAsync(invalidDoctor));
        Assert.Empty(doctorRepository.Items);
    }

    [Fact]
    public async Task DeleteAsync_RemovesDoctor_WhenNoAppointmentsExist()
    {
        var doctorRepository = new InMemoryDoctorRepository();
        var appointmentRepository = new InMemoryAppointmentRepository();
        doctorRepository.Items.Add(CreateDoctor(1, "Dr. De Vries"));

        var service = CreateService(doctorRepository, appointmentRepository);

        await service.DeleteAsync(1);

        Assert.Empty(doctorRepository.Items);
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenDoctorHasAppointments()
    {
        var doctorRepository = new InMemoryDoctorRepository();
        var appointmentRepository = new InMemoryAppointmentRepository();
        doctorRepository.Items.Add(CreateDoctor(1, "Dr. De Vries"));
        appointmentRepository.Items.Add(new Appointment
        {
            Id = 1,
            PatientId = 2,
            DoctorId = 1,
            Date = DateTime.Today
        });

        var service = CreateService(doctorRepository, appointmentRepository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(1));

        Assert.Equal("Deze arts kan niet verwijderd worden omdat er afspraken aan gekoppeld zijn.", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenDoctorDoesNotExist()
    {
        var doctorRepository = new InMemoryDoctorRepository();
        var appointmentRepository = new InMemoryAppointmentRepository();
        var service = CreateService(doctorRepository, appointmentRepository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(99));

        Assert.Equal("Arts niet gevonden.", exception.Message);
    }

    private static DoctorService CreateService(
        InMemoryDoctorRepository doctorRepository,
        InMemoryAppointmentRepository appointmentRepository)
    {
        return new DoctorService(doctorRepository, appointmentRepository, new DoctorValidator());
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
