// Copyright 2009-2014 Nikita Govorov
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
using Taijutsu.Domain;
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
            ((IDecorator)dataSession).Origin.Should().Be.SameInstanceAs(nativeSession);

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
            public DataSessionStub(NativeSession session) : base(session)
            {
            }

            protected override bool? Completed { get; set; }

            protected override bool Disposed { get; set; }

            public override IQuerySourceProvider<TEntity> Query<TEntity>(object options = null)
            {
                throw new NotImplementedException();
            }

            public override TEntity Load<TEntity>(object id, bool required = true, bool locked = false, bool optimized = false, object options = null)
            {
                throw new NotImplementedException();
            }

            public override void Flush()
            {
                throw new NotImplementedException();
            }

            public override void Complete()
            {
                throw new NotImplementedException();
            }

            protected override object InternalSave<TEntity>(TEntity entity, object options = null)
            {
                throw new NotImplementedException();
            }

            protected override object InternalSave<TEntity>(TEntity entity, EntitySaveMode mode, object options = null)
            {
                throw new NotImplementedException();
            }

            protected override object InternalSave<TEntity>(Func<TEntity> entityFactory, object options = null)
            {
                throw new NotImplementedException();
            }

            protected override void InternalDelete<TEntity>(TEntity entity, object options = null)
            {
                throw new NotImplementedException();
            }

            protected override void Dispose(bool disposing)
            {
                throw new NotImplementedException();
            }
        }
    }
}