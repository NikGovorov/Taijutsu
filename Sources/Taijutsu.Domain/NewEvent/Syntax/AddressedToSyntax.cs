namespace Taijutsu.Domain.NewEvent.Syntax
{
    public static class AddressedToSyntax
    {
        // ReSharper disable InconsistentNaming
        public interface All : IHideObjectMembers
        {
            Or Or { get; }
            DueToSyntax.DueTo<TFact> DueTo<TFact>() where TFact : IFact;
        }

        public interface All<out TEntity> : IHideObjectMembers where TEntity : IEntity
        {
            Or Or { get; }
            DueToSyntax.DueTo<TEntity, TFact> DueTo<TFact>() where TFact : IFact;
        }

        public interface Or : IHideObjectMembers
        {
            All AddressedTo<TEntity>() where TEntity : IEntity;
        }


        // ReSharper restore InconsistentNaming       
    }
}