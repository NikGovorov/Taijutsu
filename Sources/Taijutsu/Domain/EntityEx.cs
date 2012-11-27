#region License

// Copyright 2009-2013 Nikita Govorov
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

using System.Linq;

namespace Taijutsu.Domain
{
    public static class EntityEx
    {
        public static T As<T>(this IEntity entity) where T : class
        {
            var conversion = EntityConversionRegistry.CustomConversions.FirstOrDefault(ec => ec.IsApplicableFor(entity));

            return conversion != null ? conversion.SafeConvert<T>(entity) : EntityConversionRegistry.NativeConversion.SafeConvert<T>(entity);
        }

        public static bool Is<T>(this IEntity entity) where T : class
        {
            return As<T>(entity) != null;
        }
    }
}