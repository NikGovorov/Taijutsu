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

namespace Taijutsu.Domain.Event.Syntax.Publishing
{
    public static class PublishingSyntax
    {
        #region Nested type: Prepared

        // ReSharper disable InconsistentNaming
        public interface Prepared
        // ReSharper restore InconsistentNaming
        {
            void Publish();
        }

        #endregion

        #region Nested type: PreparedImpl

        public class PreparedImpl : Prepared
        {
            protected readonly Action publishAction;

            public PreparedImpl(Action publishAction)
            {
                this.publishAction = publishAction;
            }

            void Prepared.Publish()
            {
                publishAction();
            }
        }

        #endregion
    }
}