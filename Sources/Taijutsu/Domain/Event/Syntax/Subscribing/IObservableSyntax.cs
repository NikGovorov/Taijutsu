namespace Taijutsu.Domain.Event.Syntax.Subscribing
{
    public interface IObservableSyntax : IHiddenObjectMembers
    {
        IEventStreamSyntax OfEvents { get; }
        SubscriptionSyntax.All<TEvent> Of<TEvent>() where TEvent : class, IDomainEvent;
    }
}