// Copyright 2009-2014 Nikita Govorov
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

using System;
using System.Collections.Generic;
using System.Dynamic;

using Taijutsu.Annotation;

namespace Taijutsu.Data.Internal
{
    public class DataContext : IDataContext
    {
        private readonly UnitOfWorkOptions configuration;

        private readonly Lazy<IDataSession> session;

        private readonly ITerminationPolicy terminationPolicy;

        private ExpandoObject extra;

        private int subordinatesCount;

        private bool? completed;

        private bool disposed;

        public DataContext([NotNull] UnitOfWorkOptions configuration, [NotNull] Lazy<IDataSession> session, [NotNull] ITerminationPolicy terminationPolicy)
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
            extra = new ExpandoObject();

            ((dynamic)extra).Extensions = new Dictionary<string, object>();
        }

        public event EventHandler<FinishedEventArgs> BeforeCompleted;

        public event EventHandler<FinishedEventArgs> AfterCompleted;

        public event EventHandler<FinishedEventArgs> Finished;

        public IDataSession Session
        {
            get
            {
                AssertNotDisposed();

                return session.Value;
            }
        }

        public dynamic Extra
        {
            get { return extra; }
        }

        public virtual UnitOfWorkOptions Configuration
        {
            get
            {
                AssertNotDisposed();
                return configuration;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public virtual void Complete()
        {
            AssertNotDisposed();

            if (completed.HasValue)
            {
                if (!completed.Value)
                {
                    throw new Exception(string.Format("Data context has already been completed without success."));
                }

                return;
            }

            try
            {
                if (subordinatesCount != 0)
                {
                    throw new Exception("Unit of work can not be successfully completed, because not all subordinates are completed.");
                }

                if (BeforeCompleted != null)
                {
                    try
                    {
                        BeforeCompleted(this, new FinishedEventArgs(true));
                    }
                    finally
                    {
                        BeforeCompleted = null;
                    }
                }

                if (session.IsValueCreated)
                {
                    session.Value.Complete();
                }

                completed = true;
            }
            catch
            {
                completed = false;
                throw;
            }

            if (AfterCompleted != null)
            {
                try
                {
                    AfterCompleted(this, new FinishedEventArgs(true));
                }
                finally
                {
                    AfterCompleted = null;
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed || !disposing)
            {
                return;
            }

            try
            {
                try
                {
                    if (!completed.HasValue || !completed.Value)
                    {
                        completed = false;
                    }
                }
                finally
                {
                    try
                    {
                        if (Finished != null)
                        {
                            Finished(this, new FinishedEventArgs(completed.HasValue && completed.Value));
                        }
                    }
                    finally
                    {
                        if (session.IsValueCreated)
                        {
                            terminationPolicy.Terminate(session.Value, completed.HasValue && completed.Value);
                        }
                    }
                }
            }
            finally
            {
                disposed = true;
                extra = null;
                BeforeCompleted = null;
                AfterCompleted = null;
                Finished = null;
            }
        }

        protected virtual void AssertNotDisposed()
        {
            if (disposed)
            {
                throw new Exception(string.Format("Data context has already been disposed(with success - '{0}'), so it is not usable anymore.", completed));
            }
        }

        protected virtual void RegisterCompletedSubordinate()
        {
            AssertNotDisposed();

            subordinatesCount--;
        }

        protected virtual void RegisterUncompletedSubordinate()
        {
            AssertNotDisposed();

            subordinatesCount++;
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

            event EventHandler<FinishedEventArgs> IDataContext.BeforeCompleted
            {
                add
                {
                    AssertNotDisposed();
                    master.BeforeCompleted += value;
                }

                remove
                {
                    AssertNotDisposed();
                    master.BeforeCompleted -= value;
                }
            }

            event EventHandler<FinishedEventArgs> IDataContext.AfterCompleted
            {
                add
                {
                    AssertNotDisposed();
                    master.AfterCompleted += value;
                }

                remove
                {
                    AssertNotDisposed();
                    master.AfterCompleted -= value;
                }
            }

            event EventHandler<FinishedEventArgs> IDataContext.Finished
            {
                add
                {
                    AssertNotDisposed();
                    master.Finished += value;
                }

                remove
                {
                    AssertNotDisposed();
                    master.Finished -= value;
                }
            }

            public virtual IDataSession Session
            {
                get { return master.Session; }
            }

            public dynamic Extra
            {
                get { return master.Extra; }
            }

            public virtual void Dispose()
            {
                if (disposed)
                {
                    return;
                }

                if (!completed.HasValue)
                {
                    completed = false;
                }

                disposed = true;
            }

            public virtual void Complete()
            {
                AssertNotDisposed();

                if (completed.HasValue)
                {
                    return;
                }

                master.RegisterCompletedSubordinate();
                completed = true;
            }

            protected virtual void AssertNotDisposed()
            {
                if (disposed)
                {
                    throw new Exception(string.Format("Data context has already been disposed(with success - '{0}'), so it is not usable anymore.", completed));
                }
            }
        }
    }
}