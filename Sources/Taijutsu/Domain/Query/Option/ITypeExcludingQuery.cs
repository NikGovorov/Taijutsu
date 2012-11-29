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

namespace Taijutsu.Domain.Query.Option
{
    public interface ITypeExcludingQuery<out TQuery> : IQuery
    {
        TQuery ExcludeTypes(params Type[] derivedTypes);
    }

    public interface IStrongTypeExcludingQuery<in TBase, out TQuery> : IQuery
    {
        TQuery ExcludeType<TDerived>() where TDerived : TBase;

        TQuery ExcludeTypes<TDerivedFirst, TDerivedSecond>()
            where TDerivedFirst : TBase
            where TDerivedSecond : TBase;

        TQuery ExcludeTypes<TDerivedFirst, TDerivedSecond, TDerivedThird>()
            where TDerivedFirst : TBase
            where TDerivedSecond : TBase
            where TDerivedThird : TBase;

        TQuery ExcludeTypes<TDerivedFirst, TDerivedSecond, TDerivedThird, TDerivedFourth>()
            where TDerivedFirst : TBase
            where TDerivedSecond : TBase
            where TDerivedThird : TBase
            where TDerivedFourth : TBase;
    }
}