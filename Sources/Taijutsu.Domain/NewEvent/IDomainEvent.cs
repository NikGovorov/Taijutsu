using System;

namespace Taijutsu.Domain.NewEvent
{
    public interface IDomainEvent : IDomainObject, IEvent
    {
    }

    public interface IDomainEvent<out TSubject> : IDomainObject, IEvent where TSubject : IDomainObject
    {
        TSubject InitiatedBy { get; }
    }


    public interface IEventDueToFact<out TFact> : IEvent where TFact: IFact
    {
        TFact Fact { get; }
    }

    public interface IDomainEvent<out TSubject, out TFact> : IDomainEvent<TSubject>, IEventDueToFact<TFact>
        where TSubject : IDomainObject
        where TFact : IFact
    {
    }


    public interface IExternalEvent<out TTarget> : IEvent where TTarget : IEntity
    {
        TTarget AddressedTo { get; }
        DateTime DateOfNotice  { get; } 
    }

    public interface IExternalEvent<out TTarget, out TFact> : IExternalEvent<TTarget>, IEventDueToFact<TFact>
        where TTarget : IEntity
        where TFact : IFact
    {
    }
}