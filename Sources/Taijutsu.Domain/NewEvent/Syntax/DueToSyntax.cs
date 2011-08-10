namespace Taijutsu.Domain.NewEvent.Syntax
{
    public static class DueToSyntax
    {

        // ReSharper disable InconsistentNaming

        #region Nested type: DueTo

        public interface DueTo<out TFact> : ISubscriptionSyntax<IEventDueToFact<TFact>> where TFact : IFact
        {
        }


        public interface DueTo<out TEntity, out TFact> : ISubscriptionSyntax<IDomainEvent<TEntity, TFact>>
            where TFact : IFact
            where TEntity : IEntity
        {
        }

        #endregion


        #region Nested type: AddressedTo

        public interface AddressedTo<out TEntity> : ISubscriptionSyntax<IExternalEvent<TEntity>>
            where TEntity : IEntity
        {
        }

        public interface AddressedTo<out TEntity, out TFact> : ISubscriptionSyntax<IExternalEvent<TEntity>>
            where TEntity : IEntity
            where TFact : IFact
        {
        }

        #endregion

        #region Nested type: All

        public interface All : IHideObjectMembers
        {
            Or Or { get; }
            IInitiatedBy<TEntity> InitiatedBy<TEntity>() where TEntity : IEntity;
            AddressedTo<TEntity> AddressedTo<TEntity>() where TEntity : IEntity;
        }

        public interface All<out TFact> : IHideObjectMembers where TFact : IFact
        {
            Or Or { get; }
            IInitiatedBy<TEntity, TFact> InitiatedBy<TEntity>() where TEntity : IEntity;
            AddressedTo<TEntity, TFact> AddressedTo<TEntity>() where TEntity : IEntity;
        }

        #endregion

        #region Nested type: IInitiatedBy

        public interface IInitiatedBy<out TEntity> : ISubscriptionSyntax<IDomainEvent<TEntity>>
            where TEntity : IEntity
        {
        }

        public interface IInitiatedBy<out TEntity, out TFact> : ISubscriptionSyntax<IDomainEvent<TEntity, TFact>>
            where TEntity : IEntity
            where TFact : IFact
        {
        }

        #endregion

        #region Nested type: Or

        public interface Or : IHideObjectMembers
        {
            All DueTo<TFact>() where TFact : IFact;
        }

        #endregion

        // ReSharper restore InconsistentNaming         
    }
}