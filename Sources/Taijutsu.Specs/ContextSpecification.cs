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

using NUnit.Framework;
using Rhino.Mocks;

namespace Taijutsu.Specs
{
    [TestFixture]
    public abstract class ContextSpecification
    {
        [TestFixtureSetUp]
        protected virtual void MainSetup()
        {
            Given();
            When();
        }

        [TestFixtureTearDown]
        protected virtual void MainTearDown()
        {
            CleanUp();
        }


        protected abstract void When();

        protected abstract void Given();

        protected virtual void CleanUp()
        {
        }

        protected virtual T CreateStub<T>(params object[] ctorParams) where T : class
        {
            return MockRepository.GenerateStub<T>(ctorParams);
        }

        protected virtual T CreateMock<T>(params object[] ctorParams) where T : class
        {
            return MockRepository.GenerateMock<T>(ctorParams);
        }

        protected virtual T CreatePartialMock<T>(params object[] ctorParams) where T : class
        {
            return MockRepository.GeneratePartialMock<T>(ctorParams);
        }
    }
}