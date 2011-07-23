using Taijutsu.Infrastructure.Config;

namespace Taijutsu.Infrastructure.Internal
{
    public interface IReadOnlyDataProviderFactory
    {
        ReadOnlyDataProvider Create(UnitOfQueryConfig config);
    }
}