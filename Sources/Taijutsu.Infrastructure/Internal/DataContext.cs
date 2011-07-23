// Copyright 2009-2011 Taijutsu.
//   
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
//  
//      http://www.apache.org/licenses/LICENSE-2.0 
//  
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.


using System;
using System.Collections.Generic;
using System.Diagnostics;
using Taijutsu.Infrastructure.Config;

namespace Taijutsu.Infrastructure.Internal
{
    public class DataContext : IDataContext
    {
        private readonly DateTime creationDate;
        private readonly DataContextSupervisor supervisor;
        private readonly UnitOfWorkConfig unitOfWorkConfig;
        private DataContextLifeCycle advanced;
        private bool commited;
        private DataProvider dataProvider;
        private IDictionary<string, IDisposable> extension;
        private bool closed;
        private bool rolledback;
        private int slaveCount;

        public DataContext(UnitOfWorkConfig unitOfWorkConfig, DataContextSupervisor supervisor)
        {
            this.unitOfWorkConfig = unitOfWorkConfig;
            creationDate = DateTime.Now;
            this.supervisor = supervisor;
            dataProvider = supervisor.CreateDataProvider(unitOfWorkConfig);
            dataProvider.BeginTransaction(unitOfWorkConfig.IsolationLevel);
        }

        protected internal virtual DataProvider DataProvider
        {
            get { return dataProvider; }
        }

        public virtual bool Commited
        {
            get { return commited; }
        }

        public virtual bool Rolledback
        {
            get { return rolledback; }
        }

        #region IDataContext Members

        public virtual UnitOfWorkConfig UnitOfWorkConfig
        {
            get { return unitOfWorkConfig; }
        }


        public virtual IDataProvider Provider
        {
            get { return DataProvider; }
        }


        IReadOnlyDataProvider IBaseReadOnlyDataContext.ReadOnlyProvider
        {
            get { return Provider; }
        }

        public virtual DateTime CreationDate
        {
            get { return creationDate; }
        }

        public virtual bool Completed
        {
            get { return slaveCount == 0; }
        }

        public virtual bool Closed
        {
            get { return closed; }
        }

        public virtual void Commit()
        {
            if (!rolledback && !closed)
            {
                commited = true;

                if (advanced != null)
                {
                    advanced.RaiseBeforeSuccessed();
                }
                DataProvider.Commit();

                if (advanced != null)
                {
                    advanced.RaiseAfterSuccessed();
                }
            }
        }

        public virtual void Rollback()
        {
            if (!commited && !closed)
            {
                rolledback = true;

                if (advanced != null)
                {
                    advanced.RaiseBeforeFailed();
                }

                try
                {
                    DataProvider.Rollback();
                }

                finally
                {
                    if (advanced != null)
                    {
                        advanced.RaiseAfterFailed();
                    }
                }
            }
        }

        public virtual void Close()
        {

            closed = true;

            if (extension != null)
            {
                foreach (var disposable in extension.Values)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception exception)
                    {
                        Trace.TraceWarning(exception.ToString());
                    }
                }
            }

            if (advanced != null)
            {
                advanced.Dispose();
            }

            extension = null;
            advanced = null;

            dataProvider = supervisor.RegisterForTerminate(this);
        }

        void IDisposable.Dispose()
        {
            if (!Closed)
            {
                Close();
            }
        }

        public virtual IDictionary<string, IDisposable> Extension
        {
            get { return extension ?? (extension = new Dictionary<string, IDisposable>()); }
        }

        public virtual IUnitOfWorkLifeCycle Advanced
        {
            get { return advanced ?? (advanced = new DataContextLifeCycle()); }
        }

        #endregion

        public virtual void RegisterCompletedSlave()
        {
            slaveCount++;
        }

        public virtual void RegisterUncompletedSlave()
        {
            slaveCount--;
        }
    }

    public class DataContextDecorator : IDataContext
    {
        private readonly DateTime creationDate;
        private readonly DataContext dataContext;
        private bool? completed;

        public DataContextDecorator(DataContext dataContext)
        {
            this.dataContext = dataContext;
            creationDate = DateTime.Now;
            dataContext.RegisterUncompletedSlave();
        }

        #region IDataContext Members

        public virtual bool Closed
        {
            get { return dataContext.Closed; }
        }

        IReadOnlyDataProvider IBaseReadOnlyDataContext.ReadOnlyProvider
        {
            get { return ((IBaseReadOnlyDataContext) dataContext).ReadOnlyProvider; }
        }

        public virtual DateTime CreationDate
        {
            get { return creationDate; }
        }

        public virtual UnitOfWorkConfig UnitOfWorkConfig
        {
            get { return dataContext.UnitOfWorkConfig; }
        }

        public virtual IDataProvider Provider
        {
            get { return dataContext.Provider; }
        }

        public virtual bool Completed
        {
            get { return !completed.HasValue ? false : completed.Value; }
        }

        public virtual void Commit()
        {
            if (!completed.HasValue)
            {
                dataContext.RegisterCompletedSlave();
                completed = true;
            }
        }

        public virtual void Rollback()
        {
            if (!completed.HasValue || !completed.Value)
            {
                completed = false;
            }
        }

        void IBaseReadOnlyDataContext.Close()
        {
        }

        void IDisposable.Dispose()
        {
            if (!Closed)
            {
                ((IBaseReadOnlyDataContext) this).Close();
            }
        }

        public virtual IDictionary<string, IDisposable> Extension
        {
            get { return dataContext.Extension; }
        }

        public virtual IUnitOfWorkLifeCycle Advanced
        {
            get { return dataContext.Advanced; }
        }

        #endregion
    }
}