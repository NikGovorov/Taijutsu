using System;
using Taijutsu.Domain.Query;

namespace Taijutsu.Infrastructure.Internal
{
    public class OfflineReadOnlyDataProvider : ReadOnlyDataProvider
    {
        public override object NativeProvider
        {
            get { return new object(); }
        }

        public override void Close()
        {
        }

        protected virtual Exception GenerateException()
        {
            return new Exception("It's impossible to use offline read only data provider.");
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
    }
}