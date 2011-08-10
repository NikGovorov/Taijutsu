using System;

namespace Taijutsu.Domain.NewEvent.Syntax
{
    public interface ISubscriptionSyntax<out T> : IHideObjectMembers
    {
        ISubscriptionSyntax<T> Where(Func<T, bool> filter);
        ISubscriptionSyntax<TProjection> Select<TProjection>(Func<T, TProjection> projection);
        Action Subscribe(Action<T> subscriber, uint priority = 0);
    }
}