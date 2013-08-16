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

using Taijutsu.Data.Internal;

namespace Taijutsu.Data
{
    public class OperationScope : IDisposable
    {
        private readonly ITerminationPolicy terminationPolicy;

        private bool disposed;

        public OperationScope(ITerminationPolicy terminationPolicy = null)
        {
            this.terminationPolicy = terminationPolicy ?? new DelayedTerminationPolicy();
            InternalEnvironment.RegisterOperationScope(this.terminationPolicy);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed || !disposing)
            {
                return;
            }

            try
            {
                terminationPolicy.Dispose();
            }
            finally
            {
                try
                {
                    InternalEnvironment.UnregisterOperationScope();
                }
                finally
                {
                    disposed = true;
                }
            }
        }
    }
}