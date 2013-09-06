// Copyright 2009-2013 Nikita Govorov
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using System;

using NUnit.Framework;

using SharpTestsEx;

using Taijutsu.Annotation;
using Taijutsu.Data.Internal;
using Taijutsu.Domain.Query;

namespace Taijutsu.Test.Data
{
    [TestFixture]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class DataSessionFixture
    {
        [Test]
        public virtual void ShouldExposeNativeSessionAsOnlyService()
        {
            var nativeSession = new NativeSession();
            var dataSession = new DataSessionStub(nativeSession);
            dataSession.Resolve<NativeSession>(new { }).Should().Be.SameInstanceAs(nativeSession);
            ((IWrapper)dataSession).WrappedObject.Should().Be.SameInstanceAs(nativeSession);

            // ReSharper disable once ConvertToConstant.Local
            var message = "Unable to cast native session of 'Taijutsu.Test.Data.DataSessionFixture+NativeSession' to " +
                          "'Taijutsu.Test.Data.DataSessionFixture+OtherNativeSession'.";

            Assert.That(() => { dataSession.Resolve<OtherNativeSession>(); }, Throws.Exception.With.Message.EqualTo(message));
        }

        private abstract class OtherNativeSession
        {
        }

        private class NativeSession
        {
        }

        private class DataSessionStub : DataSession<NativeSession>
        {
            public DataSessionStub(NativeSession session)
                : base(session)
            {
            }

            public override object MarkAsCreated<TEntity>(TEntity entity, object options = null)
            {
                throw new NotSupportedException();
            }

            public override object MarkAsCreated<TEntity>(Func<TEntity> entityFactory, object options = null)
            {
                throw new NotSupportedException();
            }

            public override void MarkAsDeleted<TEntity>(TEntity entity, object options = null)
            {
                throw new NotSupportedException();
            }

            public override IEntitiesQuery<TEntity> All<TEntity>(object options = null)
            {
                throw new NotSupportedException();
            }

            public override IUniqueEntityQuery<TEntity> Unique<TEntity>(object key, object options = null)
            {
                throw new NotSupportedException();
            }

            public override void Complete()
            {
                throw new NotSupportedException();
            }

            protected override void Dispose(bool disposing)
            {
                throw new NotSupportedException();
            }
        }
    }
}