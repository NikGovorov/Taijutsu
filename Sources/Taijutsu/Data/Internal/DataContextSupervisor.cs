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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace Taijutsu.Data.Internal
{
    public class DataContextSupervisor
    {
        private readonly List<DataContextDecorator> contexts = new List<DataContextDecorator>();
        private readonly Func<ReadOnlyDictionary<string, DataSource>> dataSourcesProvider;
        private readonly IOrmSessionTerminationPolicy terminationPolicy;

        public DataContextSupervisor(Func<ReadOnlyDictionary<string, DataSource>> dataSourcesProvider, IOrmSessionTerminationPolicy terminationPolicy = null)
        {
            this.dataSourcesProvider = dataSourcesProvider;
            this.terminationPolicy = terminationPolicy ?? new ImmediateOrmSessionTerminationPolicy();
        }

        public virtual bool IsActive
        {
            get { return contexts.Count != 0; }
        }

        public virtual IDataContext Register(UnitOfWorkConfig config)
        {
            DataSource dataSource;

            if (!dataSourcesProvider().TryGetValue(config.SourceName, out dataSource))
            {
                if (config.SourceName == string.Empty)
                    throw new Exception("Default data source is not registered.");

                throw new Exception(string.Format("Data source with name '{0}' is not registered.", config.SourceName));
            }

            config = new UnitOfWorkConfig(config.SourceName,
                                          config.IsolationLevel == IsolationLevel.Unspecified
                                              ? dataSource.DefaultIsolationLevel
                                              : config.IsolationLevel, config.Require);

            if (config.Require == Require.New)
            {
                return new DataContextDecorator(new DataContext(config, new Lazy<IOrmSession>(() => dataSource.BuildSession(config.IsolationLevel), false), terminationPolicy), contexts);
            }

            // ReSharper disable ImplicitlyCapturedClosure
            var context =
                (from ctx in contexts where ctx.WrappedContext.Configuration.SourceName == config.SourceName select ctx)
                    .LastOrDefault();
            // ReSharper restore ImplicitlyCapturedClosure

            if (context != null)
            {
                if (!context.WrappedContext.Configuration.IsolationLevel.IsCompatible(config.IsolationLevel))
                {
                    throw new Exception(string.Format("Isolation level '{0}' is not compatible with '{1}'.",
                                                      context.WrappedContext.Configuration.IsolationLevel,
                                                      config.IsolationLevel));
                }
                return new DataContext.Subordinate(context.WrappedContext);
            }

            if (config.Require == Require.Existing)
                throw new Exception(
                    "Unit of work requires existing unit of work at the top level, but nothing has been found.");

            context = new DataContextDecorator(new DataContext(config, new Lazy<IOrmSession>(() => dataSource.BuildSession(config.IsolationLevel), false), terminationPolicy), contexts);

            return context;
        }

        public virtual IDataContext CurrentContext
        {
            get
            {
                var context = contexts.LastOrDefault();
                return context != null ? context.WrappedContext : null;
            }
        }

        protected class DataContextDecorator : IDataContext
        {
            private readonly DataContext wrappedContext;
            private List<DataContextDecorator> contexts;

            public DataContextDecorator(DataContext wrappedContext, List<DataContextDecorator> contexts)
            {
                this.wrappedContext = wrappedContext;
                this.contexts = contexts;
                contexts.Add(this);
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

            event Action<bool> IDataContext.Finished
            {
                add { wrappedContext.Finished += value; }
                remove { wrappedContext.Finished -= value; }
            }

            public IOrmSession Session
            {
                get { return wrappedContext.Session; }
            }
        }
    }
}