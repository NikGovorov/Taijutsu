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

namespace Taijutsu.Infrastructure.Internal
{
    public class ReadOnlyDataContext : IReadOnlyDataContext
    {
        private ReadOnlyDataProvider dataProvider;
        private bool closed;
        private bool rolledback;
        private UnitOfQueryConfig queryConfig;
        private ReadOnlyDataContextSupervisor supervisor;

        public ReadOnlyDataContext(UnitOfQueryConfig unitOfQueryConfig, ReadOnlyDataContextSupervisor supervisor)
        {
            queryConfig = unitOfQueryConfig;
            this.supervisor = supervisor;
            dataProvider = supervisor.CreateDataProvider(unitOfQueryConfig);
            dataProvider.BeginTransaction(unitOfQueryConfig.IsolationLevel);
        }


        public virtual UnitOfQueryConfig UnitOfQueryConfig
        {
            get { return queryConfig; }
        }


        protected internal virtual ReadOnlyDataProvider DataProvider
        {
            get { return dataProvider; }
        }

        public virtual bool Rolledback
        {
            get { return rolledback; }
        }


        #region IReadOnlyDataContext Members

        public virtual bool IsRoot
        {
            get { return true; }
        }

        public bool Closed
        {
            get { return closed; }
        }

        public IReadOnlyDataProvider ReadOnlyProvider
        {
            get { return dataProvider; }
        }


        public virtual void Close()
        {
            if (closed)
            {
                throw new Exception(
                    string.Format(
                        "Read only data context can't close data provider, because it has been already closed."));
            }
            closed = true;
            dataProvider = supervisor.RegisterForTerminate(this);
        }


        public virtual void Rollback()
        {
            if (closed || rolledback)
            {
                throw new Exception(
                    string.Format(
                        "Read only data context can't rollback data provider. State map: rolledback '{0}', closed '{1}'.",
                            rolledback, closed));
            }
            rolledback = true;
            dataProvider.Rollback();
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

    public class ChildReadOnlyDataContext : IReadOnlyDataContext
    {
        private readonly ReadOnlyDataContext readOnlyDataContext;

        public ChildReadOnlyDataContext(ReadOnlyDataContext readOnlyDataContext)
        {
            this.readOnlyDataContext = readOnlyDataContext;
        }

        #region IReadOnlyDataContext Members

        public bool IsRoot
        {
            get { return false; }
        }

        public bool Closed
        {
            get { return readOnlyDataContext.Closed; }
        }

        public IReadOnlyDataProvider ReadOnlyProvider
        {
            get { return readOnlyDataContext.ReadOnlyProvider; }
        }

        public void Close()
        {
        }

        public void Commit()
        {
         
        }

        public void Rollback()
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