using System;
using Taijutsu.Infrastructure.Config;

namespace Taijutsu.Infrastructure.Internal
{
    public interface IDataProviderPlanningPolicy : IDisposable
    {
        void Terminate(DataProvider dataProvider);
        void Terminate(ReadOnlyDataProvider dataProvider);
        DataProvider Register(UnitOfWorkConfig config);
        ReadOnlyDataProvider Register(UnitOfQueryConfig config);
    }
}