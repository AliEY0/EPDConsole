using Chipsoft.Assignments.Application.Contracts.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Chipsoft.Assignments.Infrastructure.DataAccess.Repositories
{
    public abstract class Repository<TModel>(DbContext dbContext) : IRepository<TModel> where TModel : class
    {
        protected DbContext DbContext { get; } = dbContext;
        protected DbSet<TModel> DbSet { get; } = dbContext.Set<TModel>();

        public async Task<IList<TModel>> GetAll(CancellationToken cancellationToken = default)
        {
            return await DbSet.ToListAsync(cancellationToken);
        }

        public async Task<IList<TModel>> Find(Expression<Func<TModel, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await DbSet.Where(predicate).ToListAsync(cancellationToken);
        }

        public async Task<TModel?> FindById(object id, CancellationToken cancellationToken = default)
        {
            return await DbSet.FindAsync([id], cancellationToken);
        }

        public async Task<TModel> Create(TModel toCreate, CancellationToken cancellationToken = default)
        {
            await DbSet.AddAsync(toCreate, cancellationToken);
            await DbContext.SaveChangesAsync(cancellationToken);
            return toCreate;
        }

        public async Task Delete(TModel toDelete, CancellationToken cancellationToken = default)
        {
            DbSet.Remove(toDelete);
            await DbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
