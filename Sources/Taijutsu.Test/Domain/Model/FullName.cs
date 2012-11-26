#region License

// Copyright 2009-2012 Taijutsu.
//    
//  Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
//  this file except in compliance with the License. You may obtain a copy of the 
//  License at 
//   
//  http://www.apache.org/licenses/LICENSE-2.0 
//   
//  Unless required by applicable law or agreed to in writing, software distributed 
//  under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
//  CONDITIONS OF ANY KIND, either express or implied. See the License for the 
//  specific language governing permissions and limitations under the License.

#endregion

using System;
using Taijutsu.Domain;

namespace Taijutsu.Test.Domain.Model
{
    public class FullName : ValueObject<FullName>
    {
        protected string firstName;
        protected string secondName;

        public FullName(string firstName, string secondName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new ArgumentException("firstName");
            }

            if (string.IsNullOrWhiteSpace(secondName))
            {
                throw new ArgumentException("secondName");
            }

            this.firstName = firstName;
            this.secondName = secondName;
        }

        public virtual string FirstName
        {
            get { return firstName; }
        }

        public virtual string SecondName
        {
            get { return secondName; }
        }

        protected override object BuildIdentity()
        {
            return new {firstName, secondName};
        }
    }
}