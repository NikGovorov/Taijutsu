using System;
using Taijutsu.Domain;

namespace Taijutsu.Infrastructure.Internal
{
    public class MarkingStep<TEntity> : IMarkingStep<TEntity> where TEntity : IRemovableEntity, IAggregateRoot
    {
        private readonly Func<object> creationFuction;
        private readonly Action removingAction;

        public MarkingStep(Func<object> creationFuction, Action removingAction)
        {
            this.creationFuction = creationFuction;
            this.removingAction = removingAction;
        }

        #region IMarkingStep<TEntity> Members

        public object AsCreated()
        {
            return creationFuction();
        }

        public void AsRemoved()
        {
            removingAction();
        }

        #endregion
    }
}