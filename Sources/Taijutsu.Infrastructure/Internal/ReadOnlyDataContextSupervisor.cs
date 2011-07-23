using System.Collections.Generic;
using System.Linq;
using Taijutsu.Infrastructure.Config;

namespace Taijutsu.Infrastructure.Internal
{
    public interface IReadOnlyDataContextSupervisor
    {
        IReadOnlyDataContext RegisterUnitOfQueryBasedOn(UnitOfQueryConfig unitOfQueryConfig);
    }

    public class ReadOnlyDataContextSupervisor : AbstractDataContextSupervisor, IReadOnlyDataContextSupervisor
    {
        private readonly IList<ReadOnlyDataContext> unitsOfQuery = new List<ReadOnlyDataContext>();

        public ReadOnlyDataContextSupervisor()
        {
        }

        public ReadOnlyDataContextSupervisor(IDataProviderPlanningPolicy dataContextSharingPolicy)
            : base(dataContextSharingPolicy)
        {
        }

        #region IReadOnlyDataContextSupervisor Members

        public virtual IReadOnlyDataContext RegisterUnitOfQueryBasedOn(UnitOfQueryConfig unitOfQueryConfig)
        {
            var context = (from query in unitsOfQuery
                           where query.QueryConfig.SourceName == unitOfQueryConfig.SourceName
                           select query).LastOrDefault();

            if (context != null)
            {
                return new ReadOnlyDataContextDecorator(context);
            }
            context = new ReadOnlyDataContext(unitOfQueryConfig, this);
            unitsOfQuery.Add(context);
            return context;
        }

        #endregion

        protected internal ReadOnlyDataProvider RegisterForTerminate(ReadOnlyDataContext context)
        {
            try
            {
                DataContextSharingPolicy.Terminate(context.DataProvider);
            }
            finally
            {
                unitsOfQuery.Remove(context);
            }

            return new OfflineReadOnlyDataProvider();
        }

        protected internal ReadOnlyDataProvider CreateDataProvider(UnitOfQueryConfig unitOfQueryConfig)
        {
            return DataContextSharingPolicy.Register(unitOfQueryConfig);
        }
    }
}