#region License

//  Copyright 2009-2013 Nikita Govorov
//    
//  Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
//  this file except in compliance with the License. You may obtain a copy of the 
//  License at 
//   
//  http://www.apache.org/licenses/LICENSE-2.0 
//   
//  Unless required by applicable law or agreed to in writing, software distributed 
//  under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
//  CONDITIONS OF ANY KIND, either express or implied. See the License for the 
//  specific language governing permissions and limitations under the License.

#endregion

using System;

namespace Taijutsu.Data.Internal
{
    public class DataContext : IDataContext
    {
        private readonly UnitOfWorkConfig configuration;
        private readonly Lazy<IOrmSession> session;
        private readonly IOrmSessionTerminationPolicy terminationPolicy;
        private int subordinatesCount;
        private bool? completed;
        private bool disposed;

        public DataContext(UnitOfWorkConfig configuration, Lazy<IOrmSession> session, IOrmSessionTerminationPolicy terminationPolicy)
        {
            this.configuration = configuration;
            this.session = session;
            this.terminationPolicy = terminationPolicy;
        }

        public IOrmSession Session
        {
            get { return session.Value; }
        }

        public virtual void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (!disposed && disposing)
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
                                Finished(completed.HasValue && completed.Value);
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
            }
            finally
            {
                Finished = null;
                disposed = true;
            }
        }

        public virtual void Complete()
        {
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
        }

        public virtual UnitOfWorkConfig Configuration
        {
            get { return configuration; }
        }

        protected virtual void RegisterCompletedSubordinate()
        {
            subordinatesCount--;
        }

        protected virtual void RegisterUncompletedSubordinate()
        {
            subordinatesCount++;
        }

        public event Action<bool> Finished;

        internal class Subordinate : IDataContext
        {
            private readonly DataContext master;
            private bool? completed;

            public Subordinate(DataContext master)
            {
                if (master == null)
                {
                    throw new ArgumentNullException("master");
                }

                this.master = master;
                master.RegisterUncompletedSubordinate();
            }

            public virtual void Dispose()
            {
                if (!completed.HasValue)
                {
                    completed = false;
                }
            }

            public virtual void Complete()
            {
                if (!completed.HasValue)
                {
                    master.RegisterCompletedSubordinate();
                    completed = true;
                }
            }

            public virtual IOrmSession Session
            {
                get { return master.Session; }
            }

            event Action<bool> IDataContext.Finished
            {
                add { master.Finished += value; }
                remove { master.Finished -= value; }
            }
        }
    }
}