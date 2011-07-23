using System;
using System.Collections.Generic;
using System.Diagnostics;
using Taijutsu.Infrastructure.Config;
using Taijutsu.Infrastructure.Internal;

namespace Taijutsu.Infrastructure
{
    public class LifeTime : IDisposable
    {
        private readonly IDataProviderPlanningPolicy dataContextSharing;

        public LifeTime()
            : this(new DataProviderPlanningPolicy())
        {
        }

        public LifeTime(IDataProviderPlanningPolicy dataContextSharing)
        {
            SupervisorContext.RegisterUnitScopeWith(dataContextSharing);
            this.dataContextSharing = dataContextSharing;
        }

        #region IDisposable Members

        public void Dispose()
        {
            dataContextSharing.Dispose();
        }

        #endregion

        #region Nested type: DataProviderPlanningPolicy

        private sealed class DataProviderPlanningPolicy : IDataProviderPlanningPolicy
        {
            private readonly IList<DataProvider> providers = new List<DataProvider>();
            private readonly IList<ReadOnlyDataProvider> readOnlyProviders = new List<ReadOnlyDataProvider>();

            #region IDataProviderPlanningPolicy Members

            public DataProvider Register(UnitOfWorkConfig config)
            {
                return Internal.Infrastructure.DataProviderFactory.Create(config);
            }

            public ReadOnlyDataProvider Register(UnitOfQueryConfig config)
            {
                return Internal.Infrastructure.ReadOnlyDataProviderFactory.Create(config);
            }

            public void Dispose()
            {
                try
                {
                    foreach (var dataProvider in providers)
                    {
                        try
                        {
                            dataProvider.Close();
                        }
                        catch (Exception exception)
                        {
                            Trace.TraceWarning(exception.ToString());
                        }
                    }
                    
                    foreach (var readOnlyDataProvider in readOnlyProviders)
                    {
                        try
                        {
                            readOnlyDataProvider.Close();
                        }
                        catch (Exception exception)
                        {
                            Trace.TraceWarning(exception.ToString());
                        }
                    }
                }
                finally
                {
                    providers.Clear();
                    readOnlyProviders.Clear();
                }
            }

            public void Terminate(DataProvider dataProvider)
            {
                providers.Add(dataProvider);
            }

            public void Terminate(ReadOnlyDataProvider readOnlyDataProvider)
            {
                readOnlyProviders.Add(readOnlyDataProvider);
            }

            #endregion
        }

        #endregion
    }
}