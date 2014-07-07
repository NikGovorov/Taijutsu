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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Taijutsu.Annotation;

namespace Taijutsu.Data.Internal
{
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Reviewed. TerminationPolicy lifetime must be managed outside.")]
    public class DataContextSupervisor
    {
        private readonly List<DataContextDecorator> contexts = new List<DataContextDecorator>();

        private readonly Func<ReadOnlyDictionary<string, DataSource>> dataSourcesProvider;

        private readonly ITerminationPolicy terminationPolicy;

        public DataContextSupervisor([NotNull] Func<ReadOnlyDictionary<string, DataSource>> dataSourcesProvider, ITerminationPolicy terminationPolicy = null)
        {
            if (dataSourcesProvider == null)
            {
                throw new ArgumentNullException("dataSourcesProvider");
            }

            this.dataSourcesProvider = dataSourcesProvider;
            this.terminationPolicy = terminationPolicy ?? new ImmediateTerminationPolicy();
        }

        [CanBeNull]
        public virtual IDataContext Current
        {
            get { return contexts.LastOrDefault(); }
        }

        public virtual IEnumerable<IDataContext> Contexts
        {
            get { return contexts.Cast<IDataContext>().ToArray(); }
        }

        public virtual bool Active
        {
            get { return contexts.Count != 0; }
        }

        public virtual IDataContext Register([NotNull] UnitOfWorkOptions options)
        {
            DataSource dataSource;

            if (!dataSourcesProvider().TryGetValue(options.Source, out dataSource))
            {
                if (options.Source == string.Empty)
                {
                    throw new Exception("Default data source is not registered.");
                }

                throw new Exception(string.Format("Data source with name '{0}' is not registered.", options.Source));
            }

            options = new UnitOfWorkOptions(options.Source, options.IsolationLevel == IsolationLevel.Unspecified ? dataSource.DefaultIsolationLevel : options.IsolationLevel, options.Require);

            if (options.Require == Require.New)
            {
                return new DataContextDecorator(new DataContext(options, new Lazy<IDataSession>(() => dataSource.BuildSession(options.IsolationLevel), false), terminationPolicy), contexts);
            }

            // ReSharper disable once ImplicitlyCapturedClosure
            var context = (from ctx in contexts where ctx.Origin.Options.Source == options.Source select ctx).LastOrDefault();

            if (context != null)
            {
                if (!context.Origin.Options.IsolationLevel.IsCompatible(options.IsolationLevel))
                {
                    throw new Exception(
                        string.Format("Isolation level '{0}' is not compatible with '{1}'.", context.Origin.Options.IsolationLevel, options.IsolationLevel));
                }

                return new DataContext.Subordinate(context.Origin);
            }

            if (options.Require == Require.Existing)
            {
                throw new Exception("Unit of work requires existing unit of work at the top level, but nothing has been found.");
            }

            context = new DataContextDecorator(new DataContext(options, new Lazy<IDataSession>(() => dataSource.BuildSession(options.IsolationLevel), false), terminationPolicy), contexts);

            return context;
        }

        internal class DataContextDecorator : IDataContext, IDecorator<DataContext>
        {
            private readonly DataContext origin;

            private List<DataContextDecorator> contexts;

            public DataContextDecorator(DataContext origin, List<DataContextDecorator> contexts)
            {
                this.origin = origin;
                this.contexts = contexts;
                contexts.Add(this);
            }

            event EventHandler<FinishedEventArgs> IDataContext.BeforeCompleted
            {
                add { origin.BeforeCompleted += value; }

                remove { origin.BeforeCompleted -= value; }
            }

            event EventHandler<FinishedEventArgs> IDataContext.AfterCompleted
            {
                add { origin.AfterCompleted += value; }

                remove { origin.AfterCompleted -= value; }
            }

            event EventHandler<FinishedEventArgs> IDataContext.Finished
            {
                add { origin.Finished += value; }

                remove { origin.Finished -= value; }
            }

            public IDataSession Session
            {
                get { return origin.Session; }
            }

            public dynamic Extra
            {
                get { return origin.Extra; }
            }

            public virtual DataContext Origin
            {
                get { return origin; }
            }

            public virtual void Complete()
            {
                origin.Complete();
            }

            public virtual void Dispose()
            {
                contexts.Remove(this);
                contexts = new List<DataContextDecorator>();
                origin.Dispose();
            }
        }
    }
}