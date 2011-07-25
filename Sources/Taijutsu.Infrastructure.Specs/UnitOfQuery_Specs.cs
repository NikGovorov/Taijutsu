// Copyright 2009-2011 Taijutsu.
//   
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
//  
//      http://www.apache.org/licenses/LICENSE-2.0 
//  
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using System;
using NUnit.Framework;
using Rhino.Mocks;
using Taijutsu.Domain.Query;
using Taijutsu.Domain.Specs.Domain;
using Taijutsu.Infrastructure.Internal;

namespace Taijutsu.Infrastructure.Specs
{
    // ReSharper disable InconsistentNaming
    public class UnitOfQuery_Specs
    {
        [Test]
        public virtual void When_unit_of_query_scope_completed_close_and_rollback_should_be_called()
        {
            var provider = MockRepository.GenerateMock<ReadOnlyDataProvider>();
            Internal.Infrastructure.ReadOnlyDataProviderFactory = cfg => provider;
            provider.Expect(p => p.Commit()).Repeat.Never();
            provider.Expect(p => p.Rollback()).Repeat.Once();
            provider.Expect(p => p.Close()).Repeat.Once();

            using (new UnitOfQuery())
            {
            }
            provider.VerifyAllExpectations();
        }


        [Test]
        public virtual void When_exception_during_rollback_occured_close_should_be_called()
        {
            var provider = MockRepository.GenerateMock<ReadOnlyDataProvider>();
            Internal.Infrastructure.ReadOnlyDataProviderFactory = cfg => provider;
            provider.Expect(p => p.Commit()).Repeat.Never();
            provider.Expect(p => p.Close()).Repeat.Once();
            provider.Stub(p => p.Rollback()).Throw(new Exception());

            try
            {
                using (new UnitOfQuery())
                {
                }
            }
            catch
            {
                provider.VerifyAllExpectations();
                return;
            }

            throw new NotSupportedException();
        }

        [Test]
        [ExpectedException(typeof(QueryNotFoundException))]
        public virtual void All_delegating_methods_should_be_delegated()
        {
            var provider = MockRepository.GenerateMock<ReadOnlyDataProvider>();
            try
            {
                Internal.Infrastructure.ReadOnlyDataProviderFactory = cfg => provider;
                provider.Expect(p => p.AllOf<Customer>()).Repeat.Once();
                provider.Expect(p => p.UniqueOf<Customer>(default(Guid)))
                    .Return(MockRepository.GenerateStub<IQueryOfEntityByKey<Customer>>())
                    .Repeat.Once();

                provider.Expect(p => p.QueryOver<Customer>()).Throw(new QueryNotFoundException());

                using (var uoq = new UnitOfQuery())
                {
                    uoq.AllOf<Customer>().Query();
                    uoq.UniqueOf<Customer>(Guid.Empty).Query();
                    uoq.Over<Customer>().By<GoldCustomerQuery>().Query();
                }
            }
            catch (QueryNotFoundException)
            {
                provider.VerifyAllExpectations();
                throw;
            }
            throw new NotSupportedException();
        }

        [Test]
        public virtual void Native_data_provider_should_be_accessible()
        {
            var provider = MockRepository.GenerateMock<ReadOnlyDataProvider>();
            Internal.Infrastructure.ReadOnlyDataProviderFactory = cfg => provider;
            var nhStatelessSession = new object();
            provider.Expect(p => p.NativeProvider).Return(nhStatelessSession);
            using (var uow = new UnitOfQuery())
            {
                Assert.AreSame(nhStatelessSession, ((INativeUnitOf)uow).Native);
            }
        }

        // ReSharper restore InconsistentNaming

        internal class GoldCustomerQuery : IQueryOfEntity<Customer>
        {
            #region IQueryOfEntity<Customer> Members

            public Customer Query()
            {
                throw new NotImplementedException();
            }

            public IQueryOf<bool> Any
            {
                get { throw new NotImplementedException(); }
            }

            public IQueryOfNotUniqueEntity<Customer> NotUniquely
            {
                get { throw new NotImplementedException(); }
            }

            public IQueryOf<Customer> Strictly
            {
                get { throw new NotImplementedException(); }
            }

            #endregion
        }

        internal class UnitOfWorkExtensionException : Exception
        {
        }

        internal class QueryNotFoundException : Exception
        {
        }
    }

    // ReSharper restore InconsistentNaming
}