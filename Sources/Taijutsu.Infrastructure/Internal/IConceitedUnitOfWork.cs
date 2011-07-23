namespace Taijutsu.Infrastructure.Internal
{
    public interface IConceitedUnitOfWork
    {
        IUnitOfWorkCompletion AsCompleted();
    }
}