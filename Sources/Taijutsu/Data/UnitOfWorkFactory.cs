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
using System.Data;

using Taijutsu.Domain;

namespace Taijutsu.Data
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        public IUnitOfWork Begin()
        {
            return new UnitOfWork();
        }

        public IUnitOfWork Begin(string source)
        {
            return new UnitOfWork(source);
        }

        public IUnitOfWork Begin(string source, IsolationLevel isolationLevel)
        {
            return new UnitOfWork(source, isolationLevel);
        }

        public IUnitOfWork Begin(string source, Require require)
        {
            return new UnitOfWork(source, require);
        }

        public IUnitOfWork Begin(string source, IsolationLevel isolationLevel, Require require)
        {
            return new UnitOfWork(source, isolationLevel, require);
        }

        public IUnitOfWork Begin(IsolationLevel isolationLevel)
        {
            return new UnitOfWork(isolationLevel);
        }

        public IUnitOfWork Begin(IsolationLevel isolationLevel, Require require)
        {
            return new UnitOfWork(string.Empty, isolationLevel, require);
        }

        public IUnitOfWork Begin(Require require)
        {
            return new UnitOfWork(require);
        }

        public IUnitOfWork Begin(UnitOfWorkOptions options)
        {
            return new UnitOfWork(options);
        }
    }
}