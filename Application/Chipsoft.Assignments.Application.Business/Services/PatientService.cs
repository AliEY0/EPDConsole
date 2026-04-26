using Chipsoft.Assignments.Application.Business.Validators;
using Chipsoft.Assignments.Application.Contracts.Repositories;
using Chipsoft.Assignments.Core.Domain;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chipsoft.Assignments.Application.Business.Services
{
    public class PatientService(
    IPatientRepository patientRepository,
    IAppointmentRepository appointmentRepository,
    PatientValidator patientValidator)
    {
        public async Task<IReadOnlyList<Patient>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var patients = await patientRepository.GetAll(cancellationToken);
            return patients.OrderBy(patient => patient.Name).ToList();
        }

        public async Task AddAsync(Patient patient, CancellationToken cancellationToken = default)
        {
            await ValidateAndThrowAsync(patientValidator, patient, cancellationToken);
            await patientRepository.Create(patient, cancellationToken);
        }

        public async Task DeleteAsync(int patientId, CancellationToken cancellationToken = default)
        {
            var patient = await patientRepository.FindById(patientId, cancellationToken)
                ?? throw new InvalidOperationException("Patient niet gevonden.");

            var appointments = await appointmentRepository.Find(
                appointment => appointment.PatientId == patientId,
                cancellationToken);

            if (appointments.Count > 0)
                throw new InvalidOperationException("Deze patient kan niet verwijderd worden omdat er afspraken aan gekoppeld zijn.");

            await patientRepository.Delete(patient, cancellationToken);
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
