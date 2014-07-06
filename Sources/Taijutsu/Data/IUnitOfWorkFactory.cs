// Copyright 2009-2014 Nikita Govorov
//    
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
//   
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using System.Data;

using Taijutsu.Annotation;
using Taijutsu.Domain;

namespace Taijutsu.Data
{
    [PublicApi]
    public interface IUnitOfWorkFactory
    {
        IUnitOfWork Begin();

        IUnitOfWork Begin(string source);

        IUnitOfWork Begin(string source, IsolationLevel isolationLevel);

        IUnitOfWork Begin(string source, Require require);

        IUnitOfWork Begin(string source, IsolationLevel isolationLevel, Require require);

        IUnitOfWork Begin(IsolationLevel isolationLevel);

        IUnitOfWork Begin(IsolationLevel isolationLevel, Require require);

        IUnitOfWork Begin(Require require);

        IUnitOfWork Begin(UnitOfWorkOptions options);
    }
}