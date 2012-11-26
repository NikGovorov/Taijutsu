namespace Taijutsu.Domain
{
    public static class AggregateRootEx
    {
        public static object AsCreatedIn(this IAggregateRoot self, IUnitOfWork uow)
        {
            return uow.MarkAsCreated(self);
        }
    }
}