using Chipsoft.Assignments.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chipsoft.Assignments.Application.Business.Services
{

    public class EpdManagementService(IPatientRepository patientRepository, IDoctorRepository doctorRepository, IAppointmentRepository appointmentRepository) : IEpdManagementService
    {
        public async Task<IReadOnlyCollection<PatientResponse>> GetPatients(CancellationToken cancellationToken = default)
        {
            var patients = await patientRepository.GetAll(cancellationToken);
            return patients
                .OrderBy(patient => patient.Name)
                .Select(MapPatient)
                .ToList();
        }

        public async Task<IReadOnlyCollection<DoctorResponse>> GetDoctors(CancellationToken cancellationToken = default)
        {
            var doctors = await doctorRepository.GetAll(cancellationToken);
            return doctors
                .OrderBy(doctor => doctor.Name)
                .Select(MapDoctor)
                .ToList();
        }

        public async Task<IReadOnlyCollection<AppointmentResponse>> GetAppointments(int? patientId = null,
            int? doctorId = null, CancellationToken cancellationToken = default)
        {
            var appointments = await appointmentRepository.GetOverview(patientId, doctorId, cancellationToken);
            return appointments
                .OrderBy(appointment => appointment.ScheduledAt)
                .Select(MapAppointment)
                .ToList();
        }

        public async Task<Result<PatientResponse>> AddPatient(CreatePatientRequest request,
            CancellationToken cancellationToken = default)
        {
            var validationResult = ValidatePatient(request);
            if (!validationResult.Succeeded)
                return Result<PatientResponse>.Failed(validationResult.Messages);

            var patient = new Patient
            {
                Name = request.Name.Trim(),
                Address = request.Address.Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                Email = request.Email.Trim(),
                DateOfBirth = request.DateOfBirth
            };

            patient = await patientRepository.Create(patient, cancellationToken);
            return await Result<PatientResponse>.SuccessAsync(MapPatient(patient));
        }

        public async Task<Result> DeletePatient(int patientId, CancellationToken cancellationToken = default)
        {
            var patient = await patientRepository.FindById(patientId, cancellationToken);
            if (patient is null)
                return await Result.FailedAsync("Patient niet gevonden.", Reason.NotFound);

            if (await appointmentRepository.HasAppointmentsForPatient(patientId, cancellationToken))
                return await Result.FailedAsync("Patient heeft nog afspraken en kan niet verwijderd worden.", Reason.NotValid);

            await patientRepository.Delete(patient, cancellationToken);
            return await Result.SuccessAsync();
        }

        public async Task<Result<DoctorResponse>> AddDoctor(CreateDoctorRequest request,
            CancellationToken cancellationToken = default)
        {
            var validationResult = ValidateDoctor(request);
            if (!validationResult.Succeeded)
                return Result<DoctorResponse>.Failed(validationResult.Messages);

            var doctor = new Doctor
            {
                Name = request.Name.Trim(),
                Address = request.Address.Trim()
            };

            doctor = await doctorRepository.Create(doctor, cancellationToken);
            return await Result<DoctorResponse>.SuccessAsync(MapDoctor(doctor));
        }

        public async Task<Result> DeleteDoctor(int doctorId, CancellationToken cancellationToken = default)
        {
            var doctor = await doctorRepository.FindById(doctorId, cancellationToken);
            if (doctor is null)
                return await Result.FailedAsync("Arts niet gevonden.", Reason.NotFound);

            if (await appointmentRepository.HasAppointmentsForDoctor(doctorId, cancellationToken))
                return await Result.FailedAsync("Arts heeft nog afspraken en kan niet verwijderd worden.", Reason.NotValid);

            await doctorRepository.Delete(doctor, cancellationToken);
            return await Result.SuccessAsync();
        }

        public async Task<Result<AppointmentResponse>> AddAppointment(CreateAppointmentRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.ScheduledAt == default)
                return await Result<AppointmentResponse>.FailedAsync("Voer een geldig afspraakmoment in.", Reason.NotValid);

            var patient = await patientRepository.FindById(request.PatientId, cancellationToken);
            if (patient is null)
                return await Result<AppointmentResponse>.FailedAsync("Gekozen patient bestaat niet.", Reason.NotFound);

            var doctor = await doctorRepository.FindById(request.DoctorId, cancellationToken);
            if (doctor is null)
                return await Result<AppointmentResponse>.FailedAsync("Gekozen arts bestaat niet.", Reason.NotFound);

            var appointment = new Appointment
            {
                ScheduledAt = request.ScheduledAt,
                PatientId = patient.Id,
                DoctorId = doctor.Id
            };

            appointment = await appointmentRepository.Create(appointment, cancellationToken);
            appointment.Patient = patient;
            appointment.Doctor = doctor;

            return await Result<AppointmentResponse>.SuccessAsync(MapAppointment(appointment));
        }

        private static Result ValidatePatient(CreatePatientRequest request)
        {
            var messages = new List<Message>();

            if (string.IsNullOrWhiteSpace(request.Name))
                messages.Add(new Message("Naam is verplicht.", Reason.NotValid, nameof(request.Name)));
            if (string.IsNullOrWhiteSpace(request.Address))
                messages.Add(new Message("Adres is verplicht.", Reason.NotValid, nameof(request.Address)));
            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                messages.Add(new Message("Telefoonnummer is verplicht.", Reason.NotValid, nameof(request.PhoneNumber)));
            if (string.IsNullOrWhiteSpace(request.Email))
                messages.Add(new Message("E-mail is verplicht.", Reason.NotValid, nameof(request.Email)));
            if (request.DateOfBirth == default)
                messages.Add(new Message("Geboortedatum is verplicht.", Reason.NotValid, nameof(request.DateOfBirth)));

            return messages.Count == 0 ? Result.Success() : Result.Failed(messages);
        }

        private static Result ValidateDoctor(CreateDoctorRequest request)
        {
            var messages = new List<Message>();

            if (string.IsNullOrWhiteSpace(request.Name))
                messages.Add(new Message("Naam is verplicht.", Reason.NotValid, nameof(request.Name)));
            if (string.IsNullOrWhiteSpace(request.Address))
                messages.Add(new Message("Adres is verplicht.", Reason.NotValid, nameof(request.Address)));

            return messages.Count == 0 ? Result.Success() : Result.Failed(messages);
        }

        private static PatientResponse MapPatient(Patient patient)
        {
            return new PatientResponse(
                patient.Id,
                patient.Name,
                patient.Address,
                patient.PhoneNumber,
                patient.Email,
                patient.DateOfBirth);
        }

        private static DoctorResponse MapDoctor(Doctor doctor)
        {
            return new DoctorResponse(doctor.Id, doctor.Name, doctor.Address);
        }

        private static AppointmentResponse MapAppointment(Appointment appointment)
        {
            return new AppointmentResponse(
                appointment.Id,
                appointment.ScheduledAt,
                appointment.PatientId,
                appointment.Patient?.Name ?? string.Empty,
                appointment.DoctorId,
                appointment.Doctor?.Name ?? string.Empty
            );
        }
    }

}
