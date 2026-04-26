using Chipsoft.Assignments.Application.Business.Validators;
using Chipsoft.Assignments.Application.Contracts.Repositories;
using Chipsoft.Assignments.Core.Domain;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chipsoft.Assignments.Application.Business.Services
{
    public class DoctorService(
    IDoctorRepository doctorRepository,
    IAppointmentRepository appointmentRepository,
    DoctorValidator doctorValidator)
    {
        public async Task<IReadOnlyList<Doctor>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var doctors = await doctorRepository.GetAll(cancellationToken);
            return doctors.OrderBy(doctor => doctor.Name).ToList();
        }

        public async Task AddAsync(Doctor doctor, CancellationToken cancellationToken = default)
        {
            await ValidateAndThrowAsync(doctorValidator, doctor, cancellationToken);
            await doctorRepository.Create(doctor, cancellationToken);
        }

        public async Task DeleteAsync(int doctorId, CancellationToken cancellationToken = default)
        {
            var doctor = await doctorRepository.FindById(doctorId, cancellationToken)
                ?? throw new InvalidOperationException("Arts niet gevonden.");

            var appointments = await appointmentRepository.Find(
                appointment => appointment.DoctorId == doctorId,
                cancellationToken);

            if (appointments.Count > 0)
                throw new InvalidOperationException("Deze arts kan niet verwijderd worden omdat er afspraken aan gekoppeld zijn.");

            await doctorRepository.Delete(doctor, cancellationToken);
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
