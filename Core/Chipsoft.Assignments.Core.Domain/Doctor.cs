using System;
using System.Collections.Generic;
using System.Text;

namespace Chipsoft.Assignments.Core.Domain
{
    public class Doctor
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
