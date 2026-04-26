using Chipsoft.Assignments.Core.Domain;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chipsoft.Assignments.Application.Business.Validators
{
    public class PatientValidator : AbstractValidator<Patient>
    {
        public PatientValidator()
        {
            RuleFor(patient => patient.Name).NotEmpty();
            RuleFor(patient => patient.Address).NotEmpty();
            RuleFor(patient => patient.PhoneNumber).NotEmpty();
            RuleFor(patient => patient.Email).NotEmpty();
            RuleFor(patient => patient.DateOfBirth).NotEmpty();
        }
    }
}
