using Chipsoft.Assignments.Core.Domain;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chipsoft.Assignments.Application.Business.Validators
{
    public class DoctorValidator : AbstractValidator<Doctor>
    {
        public DoctorValidator()
        {
            RuleFor(doctor => doctor.Name).NotEmpty();
            RuleFor(doctor => doctor.Address).NotEmpty();
        }
    }
}
