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
using NUnit.Framework;
using SharpTestsEx;
using Taijutsu.Data.Internal;
using Taijutsu.Domain.Query;

namespace Taijutsu.Test.Data
{
    [TestFixture]
    public class OrmSpecificSessionFixture
    {
        [Test]
        public virtual void ShouldExposeNativeSessionAsOnlyService()
        {
            var actualNativeSession = new NativeSession();
            var ormSpecificSession = new OrmSpecificSessionStub(actualNativeSession);
            ormSpecificSession.As<NativeSession>(new {}).Should().Be.SameInstanceAs(actualNativeSession);
            ((IHasNativeObject)ormSpecificSession).NativeObject.Should().Be.SameInstanceAs(actualNativeSession);

            Assert.That(() =>
                {
                    ormSpecificSession.As<OtherNativeSession>();

                }, Throws.Exception.With.Message.EqualTo("Unable to cast native session of 'Taijutsu.Test.Data.OrmSpecificSessionFixture+NativeSession' to 'Taijutsu.Test.Data.OrmSpecificSessionFixture+OtherNativeSession'."));
        }

        private abstract class OtherNativeSession
        {

        }


        private class NativeSession
        {
             
        }

        private class OrmSpecificSessionStub : OrmSpecificSession<NativeSession>
        {
            public OrmSpecificSessionStub(NativeSession nativeSession)
                : base(nativeSession)
            {
            }

            public override object MarkAsCreated<TEntity>(TEntity entity, object options = null)
            {
                throw new NotImplementedException();
            }

            public override object MarkAsCreated<TEntity>(Func<TEntity> entityFactory, object options = null)
            {
                throw new NotImplementedException();
            }

            public override void MarkAsDeleted<TEntity>(TEntity entity, object options = null)
            {
                throw new NotImplementedException();
            }

            public override IEntitiesQuery<TEntity> All<TEntity>(object options = null)
            {
                throw new NotImplementedException();
            }

            public override IUniqueEntityQuery<TEntity> Unique<TEntity>(object key, object options = null)
            {
                throw new NotImplementedException();
            }

            public override TQuery QueryWith<TEntity, TQuery>(string name = null)
            {
                throw new NotImplementedException();
            }

            public override TRepository QueryFrom<TEntity, TRepository>(string name = null)
            {
                throw new NotImplementedException();
            }

            public override void Dispose()
            {
                throw new NotImplementedException();
            }

            public override void Complete()
            {
                throw new NotImplementedException();
            }
        }

    }
}