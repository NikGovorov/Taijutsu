using System.Collections.Generic;
using NHibernate.Criterion;
using Taijutsu.Domain;

namespace Taijutsu.Data.NHibernate
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
    {
        private readonly ISessionDecorator session;

        public Repository(ISessionDecorator session)
        {
            this.session = session;
        }

        protected virtual ISessionDecorator Session
        {
            get { return session; }
        }

        public virtual TDerivedEntity Retrieve<TDerivedEntity>(object key) where TDerivedEntity : class, TEntity
        {
            var entity = Session.Get<TDerivedEntity>(key);

            if (entity == null)
            {
                throw new EntityNotFoundException<TDerivedEntity>(key);
            }

            return entity;
        }

        public virtual TEntity Retrieve(object key)
        {
            var entity = Session.Get<TEntity>(key);

            if (entity == null)
            {
                throw new EntityNotFoundException<TEntity>(key);
            }
            return entity;
        }

        public virtual Maybe<TDerivedEntity> FindBy<TDerivedEntity>(object key) where TDerivedEntity : class, TEntity
        {
            return Session.Get<TDerivedEntity>(key); 
        }

        public virtual Maybe<TEntity> FindBy(object key)
        {
            return Session.Get<TEntity>(key); 
        }

        public virtual bool TryFindBy<TDerivedEntity>(object key, out TDerivedEntity value) where TDerivedEntity : class, TEntity
        {
            value = Session.Get<TDerivedEntity>(key);
            return value != null; 
        }

        public virtual bool TryFindBy(object key, out TEntity value)
        {
            value = Session.Get<TEntity>(key);
            return value != null; 
        }

        public virtual IEnumerable<TDerivedEntity> FindAll<TDerivedEntity>() where TDerivedEntity : class, TEntity
        {
            return Session.CreateCriteria(typeof(TDerivedEntity)).List<TDerivedEntity>(); 
        }

        public virtual IEnumerable<TEntity> FindAll()
        {
            return Session.CreateCriteria(typeof(TEntity)).List<TEntity>(); 
        }

        public virtual bool Exists(object key)
        {
            return FindBy(key)!=null;
        }

        public virtual bool Exists()
        {
            return Count() > 0;
        } 

        public virtual long Count()
        {
            return Session.CreateCriteria(typeof(TEntity)).SetProjection(Projections.RowCountInt64()).UniqueResult<long>(); 

        }

        public virtual TEntity this[object key]
        {
            get { return Retrieve(key); }
        }
    }
}