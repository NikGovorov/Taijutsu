namespace Taijutsu.Data.Internal
{
    public class ImmediateTerminationPolicy : ITerminationPolicy
    {
        public void Dispose()
        {
        }

        public void Terminate(IOrmSession session, bool isSuccessfully)
        {
            session.Dispose();
            Dispose();
        }
    }
}