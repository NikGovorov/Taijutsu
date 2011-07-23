using Taijutsu.Infrastructure.Config;

namespace Taijutsu.Infrastructure.Internal
{
    public interface IDataContext : IBaseReadOnlyDataContext, IAdvancedUnitOfWork
    {
        UnitOfWorkConfig UnitOfWorkConfig { get; }
        IDataProvider Provider { get; }
        bool Completed { get; }
        void Commit();
        void Rollback();
    }
}