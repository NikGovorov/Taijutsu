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
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Taijutsu.Data.Internal
{
    internal class DeferredTerminationPolicy : ITerminationPolicy
    {
        private readonly ICollection<IDataSession> sessions = new List<IDataSession>();

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
        }

        public void Terminate(IDataSession session, bool isSuccessfully)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            if (!disposed)
            {
                sessions.Add(session);
            }
            else
            {
                session.Dispose();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Reviewed. Method is called in try-finally block.")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposed || !disposing)
            {
                return;
            }

            try
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
                    throw new AggregateException(exceptions.First().Message, exceptions);
                }
            }
            finally
            {
                disposed = true;
            }
        }
    }
}