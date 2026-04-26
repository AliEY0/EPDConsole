using Chipsoft.Assignments.Application.Business.Models;
using Chipsoft.Assignments.Application.Business.Validators;
using Chipsoft.Assignments.Application.Contracts.Repositories;
using Chipsoft.Assignments.Core.Domain;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chipsoft.Assignments.Application.Business.Services
{
    public class AppointmentService(
    IAppointmentRepository appointmentRepository,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    AppointmentValidator appointmentValidator)
    {
        public async Task<IReadOnlyList<AppointmentOverview>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var appointments = await appointmentRepository.GetAll(cancellationToken);
            var patients = await patientRepository.GetAll(cancellationToken);
            var doctors = await doctorRepository.GetAll(cancellationToken);

            var patientNames = patients.ToDictionary(patient => patient.Id, patient => patient.Name);
            var doctorNames = doctors.ToDictionary(doctor => doctor.Id, doctor => doctor.Name);

            return appointments
                .OrderBy(appointment => appointment.Date)
                .Select(appointment => new AppointmentOverview
                {
                    Id = appointment.Id,
                    Date = appointment.Date,
                    PatientName = patientNames.GetValueOrDefault(appointment.PatientId, "Onbekende patient"),
                    DoctorName = doctorNames.GetValueOrDefault(appointment.DoctorId, "Onbekende arts")
                })
                .ToList();
        }

        public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
        {
            await ValidateAndThrowAsync(appointmentValidator, appointment, cancellationToken);

            var patient = await patientRepository.FindById(appointment.PatientId, cancellationToken);
            var doctor = await doctorRepository.FindById(appointment.DoctorId, cancellationToken);

            if (patient is null || doctor is null)
                throw new InvalidOperationException("Ongeldige patient of arts geselecteerd.");

            await appointmentRepository.Create(appointment, cancellationToken);
        }

        private static async Task ValidateAndThrowAsync<T>(
            IValidator<T> validator,
            T entity,
            CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(entity, cancellationToken);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
        }
    }
}
