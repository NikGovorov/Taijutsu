#region License

//  Copyright 2009-2013 Nikita Govorov
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
using System.Collections.Generic;

namespace Taijutsu.Domain
{
    public static class EntityConversionRegistry
    {
        private static readonly IEntityConversion nativeConversion = new EntityConversion();
        private static IList<IEntityConversion> conversions = new List<IEntityConversion>();

        public static IEntityConversion NativeConversion
        {
            get { return nativeConversion; }
        }

        public static IEnumerable<IEntityConversion> CustomConversions
        {
            get { return conversions; }
        }

        public static void CustomizeWith(IEnumerable<IEntityConversion> newConversions)
        {
            if (newConversions == null) throw new ArgumentNullException("newConversions");
            conversions = new List<IEntityConversion>(newConversions);
        }
    }
}