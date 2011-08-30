namespace Taijutsu.Domain.Event.Syntax
{
    public interface IEventStreamSyntax
    {
        DueToSyntax.Init<TFact> DueTo<TFact>() where TFact : IFact;
        InitiatedBySyntax.Init<TEntity> InitiatedBy<TEntity>() where TEntity : IEntity;
        AddressedToSyntax.Init<TEntity> AddressedTo<TEntity>() where TEntity : IEntity;
    }
}