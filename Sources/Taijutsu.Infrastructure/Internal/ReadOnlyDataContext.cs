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
        private bool isClosed;
        private UnitOfQueryConfig queryConfig;
        private ReadOnlyDataContextSupervisor supervisor;

        public ReadOnlyDataContext(UnitOfQueryConfig unitOfQueryConfig, ReadOnlyDataContextSupervisor supervisor)
        {
            queryConfig = unitOfQueryConfig;
            this.supervisor = supervisor;
            dataProvider = supervisor.CreateDataProvider(unitOfQueryConfig);
        }

        public virtual UnitOfQueryConfig UnitOfQueryConfig
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
        private readonly ReadOnlyDataContext readOnlyDataContext;

        public ReadOnlyDataContextDecorator(ReadOnlyDataContext readOnlyDataContext)
        {
            this.readOnlyDataContext = readOnlyDataContext;
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