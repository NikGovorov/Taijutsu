namespace Taijutsu.Domain.Event
{
    public interface IDomainEvent : IDomainObject, IEvent
    {
    }

    public interface IDomainEvent<out TSubject> : IDomainObject, IEvent where TSubject : IDomainObject
    {
        TSubject InitiatedBy { get; }
    }


    public interface IDomainEvent<out TSubject, out TFact> : IDomainEvent<TSubject>, IEventDueToFact<TFact>
        where TSubject : IDomainObject
        where TFact : IFact
    {
    }
}