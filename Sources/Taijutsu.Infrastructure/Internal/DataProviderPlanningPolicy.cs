using Taijutsu.Infrastructure.Config;

namespace Taijutsu.Infrastructure.Internal
{
    public class DataProviderPlanningPolicy : IDataProviderPlanningPolicy
    {
        #region IDataProviderPlanningPolicy Members

        public virtual DataProvider Register(UnitOfWorkConfig unitofWorkConfig)
        {
            return Infrastructure.DataProviderFactory.Create(unitofWorkConfig);
        }

        public virtual void Terminate(ReadOnlyDataProvider readOnlyDataProvider)
        {
            readOnlyDataProvider.Close();
        }

        public virtual ReadOnlyDataProvider Register(UnitOfQueryConfig config)
        {
            return Infrastructure.ReadOnlyDataProviderFactory.Create(config);
        }

        public virtual void Dispose()
        {
        }

        public virtual void Terminate(DataProvider dataProvider)
        {
            dataProvider.Close();
        }

        #endregion
    }
}