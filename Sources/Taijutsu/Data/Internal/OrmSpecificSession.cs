#region License

// Copyright 2009-2013 Nikita Govorov
//    
//  Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
//  this file except in compliance with the License. You may obtain a copy of the 
//  License at 
//   
//  http://www.apache.org/licenses/LICENSE-2.0 
//   
//  Unless required by applicable law or agreed to in writing, software distributed 
//  under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
//  CONDITIONS OF ANY KIND, either express or implied. See the License for the 
//  specific language governing permissions and limitations under the License.

#endregion

using System;
using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Data.Internal
{
    public abstract class OrmSpecificSession<TNative> : IOrmSession
    {
        private readonly TNative nativeSession;

        protected OrmSpecificSession(TNative nativeSession)
        {
            this.nativeSession = nativeSession;
        }

        protected virtual TNative NativeSession
        {
            get { return nativeSession; }
        }

        public virtual T As<T>(object options = null) where T : class
        {
            var native = NativeSession as T;

            if (native == null)
            {
                throw new Exception(string.Format("Unable to cast native session of '{0}' to '{1}'.", NativeSession.GetType().FullName, typeof (T).FullName));
            }

            return native;
        }

        public abstract object MarkAsCreated<TEntity>(TEntity entity, object options = null) where TEntity : IAggregateRoot;
        public abstract object MarkAsCreated<TEntity>(Func<TEntity> entityFactory, object options = null) where TEntity : IAggregateRoot;
        
        public abstract void MarkAsDeleted<TEntity>(TEntity entity, object options = null) where TEntity : IDeletableEntity;
        
        public abstract IQueryOfEntities<TEntity> AllOf<TEntity>(object options = null) where TEntity : class, IEntity;
        public abstract IQueryOfEntityByKey<TEntity> UniqueOf<TEntity>(object key, object options = null) where TEntity : class, IEntity;
        public abstract IQueryOverContinuation<TEntity> QueryOver<TEntity>() where TEntity : class, IEntity;

        public abstract void Dispose();

        public abstract void Complete();

        object IHasNativeObject.NativeObject { get { return NativeSession; } }
    }
}