using Chipsoft.Assignments.Application.Business.Services;
using Chipsoft.Assignments.Application.Business.Validators;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chipsoft.Assignments.Application.Business.Configuration
{
    public static class BusinessServiceRegistration
    {
        public static IServiceCollection RegisterBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<PatientValidator>();
            services.AddScoped<DoctorValidator>();
            services.AddScoped<AppointmentValidator>();

            services.AddScoped<PatientService>();
            services.AddScoped<DoctorService>();
            services.AddScoped<AppointmentService>();

            return services;
        }
    }
}
