using System;

namespace Taijutsu.Domain
{
    [Serializable]
    public class EntityNotFoundException : EntityException
    {
        public EntityNotFoundException(object key, Type type, Exception innnerException = null)
            : this(key, (object) type, innnerException)
        {
        }

        public EntityNotFoundException(object key, object type, Exception innnerException = null)
            : base(string.Format("Entity with '{0}' key and type of '{1}' was not found.", key, type), innnerException)
        {
            entityKey = key;
            entityType = type;
        }

        public EntityNotFoundException(string queryDescription, Type type, Exception innnerException = null)
            : base(
                string.Format("Entity type of '{1}' was not found. Query description: '{0}'.", queryDescription, type),
                innnerException)
        {
            entityKey = "unknown";
            entityType = type;
        }
    }

    [Serializable]
    public class EntityNotFoundException<TEntity> : EntityNotFoundException where TEntity : IEntity
    {
        public EntityNotFoundException(object key, Exception innnerException = null)
            : base(key, typeof(TEntity), innnerException)
        {
        }

        public EntityNotFoundException(string queryDescription, Exception innnerException = null)
            : base(queryDescription, typeof(TEntity), innnerException)
        {
        }
    }
}