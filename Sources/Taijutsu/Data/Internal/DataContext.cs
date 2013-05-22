// Copyright 2009-2013 Nikita Govorov
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Taijutsu.Data.Internal
{
    using System;

    public class DataContext : IDataContext
    {
        private readonly UnitOfWorkConfig configuration;

        private readonly Lazy<IOrmSession> session;

        private readonly ITerminationPolicy terminationPolicy;

        private int subordinatesCount;

        private bool? completed;

        private bool disposed;

        public DataContext(UnitOfWorkConfig configuration, Lazy<IOrmSession> session, ITerminationPolicy terminationPolicy)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            if (terminationPolicy == null)
            {
                throw new ArgumentNullException("terminationPolicy");
            }

            this.configuration = configuration;
            this.session = session;
            this.terminationPolicy = terminationPolicy;
        }

        public event EventHandler<ScopeFinishedEventArgs> Finished;

        public IOrmSession Session
        {
            get
            {
                this.AssertNotDisposed();

                return this.session.Value;
            }
        }

        public virtual UnitOfWorkConfig Configuration
        {
            get
            {
                this.AssertNotDisposed();
                return this.configuration;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public virtual void Complete()
        {
            this.AssertNotDisposed();

            if (this.completed.HasValue)
            {
                if (!this.completed.Value)
                {
                    throw new Exception(string.Format("Data context has already been completed without success."));
                }

                return;
            }

            try
            {
                if (this.subordinatesCount != 0)
                {
                    throw new Exception("Unit of work can not be successfully completed, because not all subordinates are completed.");
                }

                if (this.session.IsValueCreated)
                {
                    this.session.Value.Complete();
                }

                this.completed = true;
            }
            catch
            {
                this.completed = false;
                throw;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed || !disposing)
            {
                return;
            }

            try
            {
                try
                {
                    if (!this.completed.HasValue || !this.completed.Value)
                    {
                        this.completed = false;
                    }
                }
                finally
                {
                    try
                    {
                        if (this.Finished != null)
                        {
                            this.Finished(this, new ScopeFinishedEventArgs(this.completed.HasValue && this.completed.Value));
                        }
                    }
                    finally
                    {
                        if (this.session.IsValueCreated)
                        {
                            this.terminationPolicy.Terminate(this.session.Value, this.completed.HasValue && this.completed.Value);
                        }
                    }
                }
            }
            finally
            {
                this.Finished = null;
                this.disposed = true;
            }
        }

        protected virtual void AssertNotDisposed()
        {
            if (this.disposed)
            {
                throw new Exception(string.Format("Data context has already been disposed(with success - '{0}'), so it is not usable anymore.", this.completed));
            }
        }

        protected virtual void RegisterCompletedSubordinate()
        {
            this.AssertNotDisposed();

            this.subordinatesCount--;
        }

        protected virtual void RegisterUncompletedSubordinate()
        {
            this.AssertNotDisposed();

            this.subordinatesCount++;
        }

        internal class Subordinate : IDataContext
        {
            private readonly DataContext master;

            private bool? completed;

            private bool disposed;

            public Subordinate(DataContext master)
            {
                if (master == null)
                {
                    throw new ArgumentNullException("master");
                }

                this.master = master;
                master.RegisterUncompletedSubordinate();
            }

            event EventHandler<ScopeFinishedEventArgs> IDataContext.Finished
            {
                add
                {
                    this.AssertNotDisposed();
                    this.master.Finished += value;
                }

                remove
                {
                    this.AssertNotDisposed();
                    this.master.Finished -= value;
                }
            }

            public virtual IOrmSession Session
            {
                get
                {
                    return this.master.Session;
                }
            }

            public virtual void Dispose()
            {
                if (this.disposed)
                {
                    return;
                }

                if (!this.completed.HasValue)
                {
                    this.completed = false;
                }

                this.disposed = true;
            }

            public virtual void Complete()
            {
                this.AssertNotDisposed();

                if (this.completed.HasValue)
                {
                    return;
                }

                this.master.RegisterCompletedSubordinate();
                this.completed = true;
            }

            protected virtual void AssertNotDisposed()
            {
                if (this.disposed)
                {
                    throw new Exception(string.Format("Data context has already been disposed(with success - '{0}'), so it is not usable anymore.", this.completed));
                }
            }
        }
    }
}