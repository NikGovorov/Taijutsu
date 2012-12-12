using System;

namespace Taijutsu.Data.Internal
{
    public class ImmediateTerminationPolicy : ITerminationPolicy
    {
        public void Dispose()
        {
        }

        public void Terminate(IOrmSession session, bool isSuccessfully)
        {
            if (session == null) throw new ArgumentNullException("session");

            session.Dispose();
            Dispose();
        }
    }
}