namespace Taijutsu.Domain.NewEvent.Syntax
{
    public static class InitiatedBySyntax
    {
        // ReSharper disable InconsistentNaming

        #region Nested type: All

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

        #endregion

        #region Nested type: Or

        public interface Or : IHideObjectMembers
        {
            All InitiatedBy<TEntity>() where TEntity : IEntity;
        }

        #endregion

        // ReSharper restore InconsistentNaming         
    }
}