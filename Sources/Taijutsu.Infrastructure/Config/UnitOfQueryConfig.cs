using Taijutsu.Infrastructure.Internal;

namespace Taijutsu.Infrastructure.Config
{
    public class UnitOfQueryConfig
    {
        protected string dataSourceName = Internal.Infrastructure.DefaultUnitSourceName;

        protected internal UnitOfQueryConfig()
        {
        }

        protected internal UnitOfQueryConfig(string sourceName)
        {
            if (string.IsNullOrEmpty(sourceName))
            {
                sourceName = Internal.Infrastructure.DefaultUnitSourceName;
            }
            dataSourceName = sourceName;
        }

        public virtual string SourceName
        {
            get { return dataSourceName; }
            protected set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = Internal.Infrastructure.DefaultUnitSourceName;
                }
                dataSourceName = value;
            }
        }

        public static implicit operator UnitOfQueryConfig(string dataSourceName)
        {
            return new UnitOfQueryConfig {SourceName = dataSourceName};
        }
    }
}