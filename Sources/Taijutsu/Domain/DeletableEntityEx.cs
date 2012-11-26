namespace Taijutsu.Domain
{
    public static class DeletableEntityEx
    {
        public static void AsDeletedIn(this IDeletableEntity self, IUnitOfWork uow)
        {
            uow.MarkAsDeleted(self);
        }
    }
}