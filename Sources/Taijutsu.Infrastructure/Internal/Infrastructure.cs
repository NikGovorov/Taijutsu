using System;
using System.ComponentModel;
using Taijutsu.Domain;

namespace Taijutsu.Infrastructure.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class Infrastructure
    {
        private static IDataProviderFactory dataProviderFactory;
        public static string DefaultUnitSourceName = "main.";
        private static IReadOnlyDataProviderFactory readOnlyDataProviderFactory;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IReadOnlyDataProviderFactory ReadOnlyDataProviderFactory
        {
            get
            {
                if (readOnlyDataProviderFactory == null)
                {
                    throw new Exception("ReadOnlyDataProviderFactory has not been initialized.");
                }

                return readOnlyDataProviderFactory;
            }
            set
            {
                if (readOnlyDataProviderFactory != null)
                {
                    throw new Exception("ReadOnlyDataProviderFactory has been already initialized.");
                }

                readOnlyDataProviderFactory = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IDataProviderFactory DataProviderFactory
        {
            get
            {
                if (dataProviderFactory == null)
                {
                    throw new Exception("DataProviderFactory has not been initialized.");
                }

                return dataProviderFactory;
            }
            set
            {
                if (dataProviderFactory != null)
                {
                    throw new Exception("DataProviderFactory has been already initialized.");
                }

                dataProviderFactory = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Maybe<IAdvancedUnitOfWork> CurrentUnitOfWork
        {
            get
            {
                if (!SupervisorContext.DataContextSupervisor.CurrentContext)
                {
                    return Maybe<IAdvancedUnitOfWork>.Empty;
                }

                return new Maybe<IAdvancedUnitOfWork>(SupervisorContext.DataContextSupervisor.CurrentContext.Value);
            }
        }
    }
}