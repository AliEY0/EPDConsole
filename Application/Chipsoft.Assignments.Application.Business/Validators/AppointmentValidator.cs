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
            RuleFor(appointment => appointment.Date)
                .NotEmpty()
                .Must(date => date > DateTime.Now)
                .WithMessage("Een afspraak mag niet in het verleden liggen.");
            RuleFor(appointment => appointment.PatientId).GreaterThan(0);
            RuleFor(appointment => appointment.DoctorId).GreaterThan(0);
        }
    }
}
