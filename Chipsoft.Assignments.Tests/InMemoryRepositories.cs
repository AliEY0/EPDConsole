using Chipsoft.Assignments.Application.Contracts.Repositories;
using Chipsoft.Assignments.Core.Domain;
using System.Linq.Expressions;

namespace Chipsoft.Assignments.Tests;

internal sealed class InMemoryPatientRepository : InMemoryRepository<Patient>, IPatientRepository
{
}

internal sealed class InMemoryDoctorRepository : InMemoryRepository<Doctor>, IDoctorRepository
{
}

internal sealed class InMemoryAppointmentRepository : InMemoryRepository<Appointment>, IAppointmentRepository
{
}

internal abstract class InMemoryRepository<TModel> : IRepository<TModel> where TModel : class
{
    public List<TModel> Items { get; } = [];

    public Task<IList<TModel>> GetAll(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IList<TModel>>(Items.ToList());
    }

    public Task<IList<TModel>> Find(
        Expression<Func<TModel, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var compiledPredicate = predicate.Compile();
        return Task.FromResult<IList<TModel>>(Items.Where(compiledPredicate).ToList());
    }

    public Task<TModel?> FindById(object id, CancellationToken cancellationToken = default)
    {
        var entity = Items.FirstOrDefault(item => GetId(item) == Convert.ToInt32(id));
        return Task.FromResult(entity);
    }

    public Task<TModel> Create(TModel toCreate, CancellationToken cancellationToken = default)
    {
        Items.Add(toCreate);
        return Task.FromResult(toCreate);
    }

    public Task Delete(TModel toDelete, CancellationToken cancellationToken = default)
    {
        Items.Remove(toDelete);
        return Task.CompletedTask;
    }

    private static int GetId(TModel model)
    {
        var property = typeof(TModel).GetProperty("Id")
            ?? throw new InvalidOperationException("Model heeft geen Id-property.");

        return (int)(property.GetValue(model)
            ?? throw new InvalidOperationException("Id mag niet null zijn."));
    }
}
