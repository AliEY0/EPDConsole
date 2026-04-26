using Chipsoft.Assignments.Core.Domain;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chipsoft.Assignments.Application.Business.Validators
{
    public class AppointmentValidator : AbstractValidator<Appointment>
    {
        public AppointmentValidator()
        {
            RuleFor(appointment => appointment.ScheduledAt).NotEmpty();
            RuleFor(appointment => appointment.PatientId).GreaterThan(0);
            RuleFor(appointment => appointment.DoctorId).GreaterThan(0);
        }
    }
}
