// Copyright 2009-2012 Taijutsu.
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

namespace Taijutsu.PrevData.Internal
{
    public class ReadOnlyDataContext : IReadOnlyDataContext
    {
        private ReadOnlyDataProvider dataProvider;
        private readonly Action<ReadOnlyDataContext> onClosed;
        private readonly UnitOfQueryConfig queryConfig;
        private bool closed;
        private bool commited;
        private bool rolledback;
        
        public ReadOnlyDataContext(UnitOfQueryConfig queryConfig, ReadOnlyDataProvider dataProvider, Action<ReadOnlyDataContext> onClosed)
        {
            this.queryConfig = queryConfig;
            this.dataProvider = dataProvider;
            this.onClosed = onClosed;
            dataProvider.BeginTransaction(queryConfig.IsolationLevel);
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

        public bool IsClosed
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
            onClosed(this);
            dataProvider = new OfflineReadOnlyDataProvider();
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
            dataProvider.RollbackTransaction();
        }

        public virtual void Commit()
        {
            if (commited || closed || rolledback)
            {
                throw new Exception(
                    string.Format(
                        "Read only data context can't commit data provider. State map: commited '{0}', rolledback '{1}', closed '{2}'.",
                        commited, rolledback, closed));
            }
            commited = true;
            dataProvider.CommitTransaction();
        }

        void IDisposable.Dispose()
        {
            if (!IsClosed)
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

        public bool IsClosed
        {
            get { return readOnlyDataContext.IsClosed; }
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
            if (!IsClosed)
            {
                Close();
            }
        }

        #endregion
    }
}