#region License

//  Copyright 2009-2013 Nikita Govorov
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

namespace Taijutsu.Test.Data
{
    public class EntityQuery<TEntity> : IEntityQuery<TEntity> where TEntity : IEntity
    {
        public TEntity Single()
        {
            throw new NotImplementedException();
        }

        public TEntity SingleOrDefault()
        {
            throw new NotImplementedException();
        }

        public TEntity First()
        {
            throw new NotImplementedException();
        }

        public TEntity FirstOrDefault()
        {
            throw new NotImplementedException();
        }

        public bool Any()
        {
            throw new NotImplementedException();
        }

        public long Count()
        {
            throw new NotImplementedException();
        }

        public IEntityQuery<TEntity> IdentifiedAs(params object[] ids)
        {
            throw new NotImplementedException();
        }

        public IEntityQuery<TEntity> IsNotOf(params Type[] derivedTypes)
        {
            throw new NotImplementedException();
        }

        public IEntityQuery<TEntity> Lock(bool pessimistically = true)
        {
            throw new NotImplementedException();
        }
    }
}