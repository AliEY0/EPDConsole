using Chipsoft.Assignments.Application.Contracts.Repositories;
using Chipsoft.Assignments.Core.Domain;
using Chipsoft.Assignments.Infrastructure.DataAccess.Configuration;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chipsoft.Assignments.Infrastructure.DataAccess.Repositories
{
    public class AppointmentRepository(EPDDbContext dbContext) : Repository<Appointment>(dbContext), IAppointmentRepository
    {
    }
}
