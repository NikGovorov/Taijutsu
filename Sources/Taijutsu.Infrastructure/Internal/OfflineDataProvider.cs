using System;
using System.Data;
using Taijutsu.Domain;
using Taijutsu.Domain.Query;


namespace Taijutsu.Infrastructure.Internal
{
    public class OfflineDataProvider : DataProvider
    {
        public override object NativeProvider
        {
            get { return new object(); }
        }

        public override void Close()
        {
        }

        public override void BeginTransaction(IsolationLevel level)
        {
        }

        public override void Commit()
        {
        }

        public override void Rollback()
        {
        }

        protected virtual Exception GenerateException()
        {
            return new Exception("It's impossible to use offline data provider.");
        }

        public override object MarkAsCreated<TEntity>(TEntity entity)
        {
            throw GenerateException();
        }

        public override void MarkAsRemoved<TEntity>(TEntity entity)
        {
            throw GenerateException();
        }

        public override IQueryOfEntities<TEntity> AllOf<TEntity>()
        {
            throw GenerateException();
        }

        public override IQueryOfEntityByKey<TEntity> UniqueOf<TEntity>(object key)
        {
            throw GenerateException();
        }


        public override IQueryOverBuilder<TEntity> QueryOver<TEntity>()
        {
            throw GenerateException();
        }


        public override IMarkingStep<TEntity> Mark<TEntity>(TEntity entity)
        {
            throw GenerateException();
        }
    }
}