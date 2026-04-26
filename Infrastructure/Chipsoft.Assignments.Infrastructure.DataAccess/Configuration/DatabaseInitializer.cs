using Chipsoft.Assignments.Core.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chipsoft.Assignments.Infrastructure.DataAccess.Configuration
{
    public class DatabaseInitializer(EPDDbContext dbContext)
    {
        public void EnsureCreated()
        {
            dbContext.Database.EnsureCreated();
        }

        public void Reset()
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
        }
    }
}

