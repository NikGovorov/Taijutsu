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
namespace Taijutsu.Domain.Query
{
    using System;

    [PublicApi]
    public interface INotTypeFilter<out TQuery> : IQueryContinuation
    {
        TQuery IsNotOf(params Type[] derivedTypes);
    }

    [PublicApi]
    public interface INotTypeFilter<in TBase, out TQuery> : IQueryContinuation
    {
        TQuery IsNotOf<TDerived>() where TDerived : TBase;

        TQuery IsNotOf<TDerivedFirst, TDerivedSecond>() where TDerivedFirst : TBase where TDerivedSecond : TBase;

        TQuery IsNotOf<TDerivedFirst, TDerivedSecond, TDerivedThird>() where TDerivedFirst : TBase where TDerivedSecond : TBase where TDerivedThird : TBase;

        TQuery IsNotOf<TDerivedFirst, TDerivedSecond, TDerivedThird, TDerivedFourth>() where TDerivedFirst : TBase where TDerivedSecond : TBase where TDerivedThird : TBase
            where TDerivedFourth : TBase;
    }
}