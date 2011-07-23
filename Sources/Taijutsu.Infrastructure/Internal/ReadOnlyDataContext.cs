using System;
using Taijutsu.Infrastructure.Config;

namespace Taijutsu.Infrastructure.Internal
{
    public class ReadOnlyDataContext : IReadOnlyDataContext
    {
        private DateTime creationDate;
        private ReadOnlyDataProvider dataProvider;
        private bool isClosed;
        private UnitOfQueryConfig queryConfig;
        private ReadOnlyDataContextSupervisor supervisor;

        public ReadOnlyDataContext(UnitOfQueryConfig unitOfQueryConfig, ReadOnlyDataContextSupervisor supervisor)
        {
            queryConfig = unitOfQueryConfig;
            creationDate = DateTime.Now;
            this.supervisor = supervisor;
            dataProvider = supervisor.CreateDataProvider(unitOfQueryConfig);
        }

        public virtual UnitOfQueryConfig QueryConfig
        {
            get { return queryConfig; }
        }

        protected internal virtual ReadOnlyDataProvider DataProvider
        {
            get { return dataProvider; }
        }

        #region IReadOnlyDataContext Members

        public bool Closed
        {
            get { return isClosed; }
        }

        public IReadOnlyDataProvider ReadOnlyProvider
        {
            get { return dataProvider; }
        }


        public virtual DateTime CreationDate
        {
            get { return creationDate; }
        }

        public virtual void Close()
        {
            isClosed = true;
            dataProvider = supervisor.RegisterForTerminate(this);
        }

        void IDisposable.Dispose()
        {
            if (!Closed)
            {
                Close();
            }
        }

        #endregion
    }

    public class ReadOnlyDataContextDecorator : IReadOnlyDataContext
    {
        private readonly DateTime creationDate;
        private readonly ReadOnlyDataContext readOnlyDataContext;

        public ReadOnlyDataContextDecorator(ReadOnlyDataContext readOnlyDataContext)
        {
            this.readOnlyDataContext = readOnlyDataContext;
            creationDate = DateTime.Now;
        }

        #region IReadOnlyDataContext Members

        public bool Closed
        {
            get { return readOnlyDataContext.Closed; }
        }

        public IReadOnlyDataProvider ReadOnlyProvider
        {
            get { return readOnlyDataContext.ReadOnlyProvider; }
        }

        public DateTime CreationDate
        {
            get { return creationDate; }
        }

        public void Close()
        {
        }

        void IDisposable.Dispose()
        {
            if (!Closed)
            {
                Close();
            }
        }

        #endregion
    }
}