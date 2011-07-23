
using Taijutsu.Domain;

namespace Taijutsu.Infrastructure.Internal
{
    public interface IDataProvider : IReadOnlyDataProvider
    {
        object MarkAsCreated<TEntity>(TEntity entity) where TEntity : IAggregateRoot;
        void MarkAsRemoved<TEntity>(TEntity entity) where TEntity : IRemovableEntity;
    }
}