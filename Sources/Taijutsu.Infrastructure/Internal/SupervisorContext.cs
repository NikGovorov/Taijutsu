
using System.ComponentModel;
using Taijutsu.Domain;

namespace Taijutsu.Infrastructure.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SupervisorContext
    {
        private const string DefaultDataContextSupervisorKey = "DataContextSupervisor";
        private const string DefaultReadOnlyDataContextSupervisorKey = "ReadOnlyDataContextSupervisor";

        internal static IDataContextSupervisor DataContextSupervisor
        {
            get
            {
                var supervisor = LogicalContext.FindData<DataContextSupervisor>(DefaultDataContextSupervisorKey);
                if (supervisor == null)
                {
                    supervisor = new DataContextSupervisor();
                    LogicalContext.SetData(DefaultDataContextSupervisorKey, supervisor);
                }
                return supervisor;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IReadOnlyDataContextSupervisor ReadOnlyDataContextSupervisor
        {
            get
            {
                var supervisor = LogicalContext.FindData<ReadOnlyDataContextSupervisor>(DefaultReadOnlyDataContextSupervisorKey);
                if (supervisor == null)
                {
                    supervisor = new ReadOnlyDataContextSupervisor();
                    LogicalContext.SetData(DefaultReadOnlyDataContextSupervisorKey, supervisor);
                }
                return supervisor;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void RegisterUnitScopeWith(IDataProviderPlanningPolicy dataContextSharing)
        {
            LogicalContext.SetData(DefaultDataContextSupervisorKey,
                                           new DataContextSupervisor(dataContextSharing));
            LogicalContext.SetData(DefaultReadOnlyDataContextSupervisorKey,
                                           new ReadOnlyDataContextSupervisor(dataContextSharing));
        }
    }
}