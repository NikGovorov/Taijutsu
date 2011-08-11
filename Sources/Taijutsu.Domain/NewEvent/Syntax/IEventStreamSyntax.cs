namespace Taijutsu.Domain.NewEvent.Syntax
{
    public interface IEventStreamSyntax
    {
        DueToSyntax.Init<TFact> DueTo<TFact>() where TFact : IFact;
        InitiatedBySyntax.Init<TEntity> InitiatedBy<TEntity>() where TEntity : IEntity;
        AddressedToSyntax.All<TEntity> AddressedTo<TEntity>() where TEntity : IEntity;
    }
}