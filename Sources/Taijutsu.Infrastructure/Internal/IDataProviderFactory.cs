using Taijutsu.Infrastructure.Config;

namespace Taijutsu.Infrastructure.Internal
{
    public interface IDataProviderFactory
    {
        DataProvider Create(UnitOfWorkConfig config);
    }
}