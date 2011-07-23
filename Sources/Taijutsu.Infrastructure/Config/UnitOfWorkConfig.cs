using System.Data;

namespace Taijutsu.Infrastructure.Config
{
    public class UnitOfWorkConfig : UnitOfQueryConfig
    {
        private IsolationLevel isolationLevel = IsolationLevel.ReadCommitted;


        protected internal UnitOfWorkConfig()
        {
        }


        protected internal UnitOfWorkConfig(string sourceName, IsolationLevel isolationLevel)
            : base(sourceName)
        {
            this.isolationLevel = isolationLevel;
        }

        protected internal UnitOfWorkConfig(string sourceName)
            : base(sourceName)
        {
        }

        protected internal UnitOfWorkConfig(IsolationLevel isolationLevel)
        {
            this.isolationLevel = isolationLevel;
        }

        public virtual IsolationLevel IsolationLevel
        {
            get { return isolationLevel; }
            protected set { isolationLevel = value; }
        }

        public static implicit operator UnitOfWorkConfig(string dataSourceName)
        {
            return new UnitOfWorkConfig {SourceName = dataSourceName};
        }

        public static implicit operator UnitOfWorkConfig(IsolationLevel isolationLevel)
        {
            return new UnitOfWorkConfig {IsolationLevel = isolationLevel};
        }
    }

    public static class IsolationLevelEx
    {
        public static UnitOfWorkConfig InContextOf(this IsolationLevel isolationLevel, string source)
        {
            return new UnitOfWorkConfig(source, isolationLevel);
        }
    }
}