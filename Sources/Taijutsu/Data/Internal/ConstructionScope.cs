﻿// Copyright 2009-2013 Nikita Govorov
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
namespace Taijutsu.Data.Internal
{
    using System;

    public class ConstructionScope : IDisposable
    {
        private readonly bool previousValue;

        private bool disposed;

        public ConstructionScope()
        {
            this.previousValue = InternalEnvironment.IsInsideConstructionScope;
            InternalEnvironment.IsInsideConstructionScope = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed || !disposing)
            {
                return;
            }

            try
            {
                InternalEnvironment.IsInsideConstructionScope = this.previousValue;
            }
            finally
            {
                this.disposed = true;
            }
        }
    }
}