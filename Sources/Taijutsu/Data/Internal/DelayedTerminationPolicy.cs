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
using System.Collections.Generic;
using System.Linq;

namespace Taijutsu.Data.Internal
{
    public class DelayedTerminationPolicy : ITerminationPolicy
    {
        private bool disposed;
        private readonly ICollection<IOrmSession> sessions = new List<IOrmSession>();

        void IDisposable.Dispose()
        {
            Dispose();
        }

        protected virtual void Dispose()
        {
            try
            {
                if (!disposed)
                {
                    var exceptions = new List<Exception>();
                    foreach (var session in sessions)
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
                        throw new AggregateException(exceptions);
                    }
                }
            }
            finally
            {
                disposed = true;
            }
        }

        public void Terminate(IOrmSession session, bool isSuccessfully)
        {
            if (session == null) throw new ArgumentNullException("session");

            if (!disposed)
            {
                sessions.Add(session);
            }
            else
            {
                throw new Exception("TerminationPolicy has already been disposed.");
            }
        }
    }
}