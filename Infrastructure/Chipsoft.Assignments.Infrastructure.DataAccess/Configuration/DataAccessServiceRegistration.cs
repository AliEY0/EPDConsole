using Chipsoft.Assignments.Application.Contracts.Repositories;
using Chipsoft.Assignments.Infrastructure.DataAccess.Repositories;
using Microsoft.Extensions.DependencyInjection;


namespace Chipsoft.Assignments.Infrastructure.DataAccess.Configuration
{
    public static class DataAccessServiceRegistration
    {
        public static IServiceCollection RegisterEpdRepositories(this IServiceCollection services)
        {
            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddScoped<IDoctorRepository, DoctorRepository>();
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            services.AddScoped<DatabaseInitializer>();
            return services;
        }
    }
}
