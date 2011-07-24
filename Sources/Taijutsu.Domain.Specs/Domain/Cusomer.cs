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

namespace Taijutsu.Domain.Specs.Domain
{
    public class Cusomer : Entity<Guid>
    {
        protected DateTime creationDate;
        protected string name;

        public Cusomer()
        {
            entityKey = SeqGuid.NewGuid();
            name = string.Empty;
            creationDate = SystemTime.Now;
        }

        public virtual DateTime CreationDate
        {
            get { return creationDate; }
        }

        public virtual string Name
        {
            get { return name; }
        }
    }
}