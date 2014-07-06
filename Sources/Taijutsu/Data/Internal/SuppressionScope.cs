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

namespace Taijutsu.Data.Internal
{
    public class SuppressionScope : IDisposable
    {
        private readonly bool previousValue;

        private bool disposed;

        public SuppressionScope()
        {
            previousValue = DataEnvironment.IsInsideSuppressionScope;
            DataEnvironment.IsInsideSuppressionScope = true;
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
                DataEnvironment.IsInsideSuppressionScope = previousValue;
            }
            finally
            {
                disposed = true;
            }
        }
    }
}