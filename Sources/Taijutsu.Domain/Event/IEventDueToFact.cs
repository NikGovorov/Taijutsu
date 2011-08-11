namespace Taijutsu.Domain.Event
{
    public interface IEventDueToFact<out TFact> : IEvent where TFact: IFact
    {
        TFact Fact { get; }
    }
}