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
    using System.Collections.Generic;
    using System.Linq;

    public class DelayedTerminationPolicy : ITerminationPolicy
    {
        private readonly ICollection<IOrmSession> sessions = new List<IOrmSession>();

        private bool disposed;

        void IDisposable.Dispose()
        {
            this.Dispose();
        }

        public void Terminate(IOrmSession session, bool isSuccessfully)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            if (!this.disposed)
            {
                this.sessions.Add(session);
            }
            else
            {
                session.Dispose();
            }
        }

        protected virtual void Dispose()
        {
            try
            {
                if (!this.disposed)
                {
                    var exceptions = new List<Exception>();
                    foreach (var session in this.sessions)
                    {
                        try
                        {
                            session.Dispose();
                        }
                        catch (Exception exception)
                        {
                            exceptions.Add(exception);
                        }
                    }

                    if (exceptions.Any())
                    {
                        throw new AggregateException(exceptions.First().Message, exceptions);
                    }
                }
            }
            finally
            {
                this.disposed = true;
            }
        }
    }
}