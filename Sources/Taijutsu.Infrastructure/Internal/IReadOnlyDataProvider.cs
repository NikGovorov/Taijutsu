
using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Infrastructure.Internal
{
    public interface IReadOnlyDataProvider
    {
        object NativeProvider { get; }
        IQueryOfEntities<TEntity> AllOf<TEntity>() where TEntity : class, IQueryableEntity;
        IQueryOfEntityByKey<TEntity> UniqueOf<TEntity>(object key) where TEntity : class, IQueryableEntity;
        IQueryOverBuilder<TEntity> QueryOver<TEntity>() where TEntity : class, IQueryableEntity;
    }
}