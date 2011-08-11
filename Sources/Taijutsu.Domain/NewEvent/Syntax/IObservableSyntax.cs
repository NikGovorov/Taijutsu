namespace Taijutsu.Domain.NewEvent.Syntax
{
    public interface IObservableSyntax : IHideObjectMembers
    {
        IEventStreamSyntax OfEvents { get; }
        SubscriptionSyntax.All<TEvent> Of<TEvent>() where TEvent : class, IDomainEvent;
    }
}