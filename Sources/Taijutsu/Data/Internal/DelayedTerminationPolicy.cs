using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                            Trace.TraceError(exception.ToString());
                            exceptions.Add(exception);
                        }
                    }
                    if (exceptions.Any())
                    {
                        throw exceptions.First();
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