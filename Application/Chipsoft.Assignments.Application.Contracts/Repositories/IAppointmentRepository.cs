using Chipsoft.Assignments.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chipsoft.Assignments.Application.Contracts.Repositories
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task<ICollection<Appointment>> GetOverview(int? patientId = null, int? doctorId = null,
            CancellationToken cancellationToken = default);
        Task<bool> HasAppointmentsForPatient(int patientId, CancellationToken cancellationToken = default);
        Task<bool> HasAppointmentsForDoctor(int doctorId, CancellationToken cancellationToken = default);
    }
}
