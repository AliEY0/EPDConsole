using System;
using System.Collections.Generic;
using System.Text;

namespace Chipsoft.Assignments.Application.Business.Models
{
    public class AppointmentOverview
    {
        public int Id { get; init; }
        public DateTime Date { get; init; }
        public string PatientName { get; init; } = string.Empty;
        public string DoctorName { get; init; } = string.Empty;
    }
}
