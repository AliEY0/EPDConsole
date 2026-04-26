using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Chipsoft.Assignments.Core.Domain
{
    public class Patient
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Email { get; set; }
        public required DateTime DateOfBirth { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
