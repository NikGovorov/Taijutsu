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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Taijutsu.Data.Internal
{
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Reviewed. TerminationPolicy lifetime must be managed outside.")]
    public class DataContextSupervisor
    {
        private readonly List<DataContextDecorator> contexts = new List<DataContextDecorator>();

        private readonly Func<ReadOnlyDictionary<string, DataSource>> dataSourcesProvider;

        private readonly ITerminationPolicy terminationPolicy;

        public DataContextSupervisor(Func<ReadOnlyDictionary<string, DataSource>> dataSourcesProvider, ITerminationPolicy terminationPolicy = null)
        {
            if (dataSourcesProvider == null)
            {
                throw new ArgumentNullException("dataSourcesProvider");
            }

            this.dataSourcesProvider = dataSourcesProvider;
            this.terminationPolicy = terminationPolicy ?? new ImmediateTerminationPolicy();
        }

        public virtual IDataContext CurrentContext
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

        public virtual IDataContext Register(UnitOfWorkConfig config)
        {
            DataSource dataSource;

            if (!dataSourcesProvider().TryGetValue(config.SourceName, out dataSource))
            {
                if (config.SourceName == string.Empty)
                {
                    throw new Exception("Default data source is not registered.");
                }

                throw new Exception(string.Format("Data source with name '{0}' is not registered.", config.SourceName));
            }

            config = new UnitOfWorkConfig(
                config.SourceName, 
                config.IsolationLevel == IsolationLevel.Unspecified ? dataSource.DefaultIsolationLevel : config.IsolationLevel, 
                config.Require);

            if (config.Require == Require.New)
            {
                return new DataContextDecorator(
                    new DataContext(config, new Lazy<IOrmSession>(() => dataSource.BuildSession(config.IsolationLevel), false), terminationPolicy), 
                    contexts);
            }

            // ReSharper disable once ImplicitlyCapturedClosure
            var context = (from ctx in contexts where ctx.WrappedContext.Configuration.SourceName == config.SourceName select ctx).LastOrDefault();

            if (context != null)
            {
                if (!context.WrappedContext.Configuration.IsolationLevel.IsCompatible(config.IsolationLevel))
                {
                    throw new Exception(
                        string.Format("Isolation level '{0}' is not compatible with '{1}'.", context.WrappedContext.Configuration.IsolationLevel, config.IsolationLevel));
                }

                return new DataContext.Subordinate(context.WrappedContext);
            }

            if (config.Require == Require.Existing)
            {
                throw new Exception("Unit of work requires existing unit of work at the top level, but nothing has been found.");
            }

            context = new DataContextDecorator(
                new DataContext(config, new Lazy<IOrmSession>(() => dataSource.BuildSession(config.IsolationLevel), false), terminationPolicy), 
                contexts);

            return context;
        }

        internal class DataContextDecorator : IDataContext
        {
            private readonly DataContext wrappedContext;

            private List<DataContextDecorator> contexts;

            public DataContextDecorator(DataContext wrappedContext, List<DataContextDecorator> contexts)
            {
                this.wrappedContext = wrappedContext;
                this.contexts = contexts;
                contexts.Add(this);
            }

            event EventHandler<ScopeFinishedEventArgs> IDataContext.Finished
            {
                add { wrappedContext.Finished += value; }

                remove { wrappedContext.Finished -= value; }
            }

            public IOrmSession Session
            {
                get { return wrappedContext.Session; }
            }

            public virtual DataContext WrappedContext
            {
                get { return wrappedContext; }
            }

            public virtual void Complete()
            {
                wrappedContext.Complete();
            }

            public virtual void Dispose()
            {
                contexts.Remove(this);
                contexts = new List<DataContextDecorator>();
                wrappedContext.Dispose();
            }
        }
    }
}