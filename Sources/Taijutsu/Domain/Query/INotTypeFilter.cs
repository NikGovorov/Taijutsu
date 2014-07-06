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

using Taijutsu.Annotation;

namespace Taijutsu.Domain.Query
{
    [PublicApi]
    public interface INotTypeFilter<out TQuery> : IQueryContinuation
    {
        TQuery IsNot([NotNull] params Type[] derivedTypes);
    }

    [PublicApi]
    public interface INotTypeFilter<in TBase, out TQuery> : IQueryContinuation
    {
        TQuery IsNot<TDerived>() where TDerived : TBase;

        TQuery IsNot<TDerivedFirst, TDerivedSecond>() where TDerivedFirst : TBase where TDerivedSecond : TBase;

        TQuery IsNot<TDerivedFirst, TDerivedSecond, TDerivedThird>() where TDerivedFirst : TBase where TDerivedSecond : TBase where TDerivedThird : TBase;

        TQuery IsNot<TDerivedFirst, TDerivedSecond, TDerivedThird, TDerivedFourth>() where TDerivedFirst : TBase where TDerivedSecond : TBase where TDerivedThird : TBase
            where TDerivedFourth : TBase;
    }
}