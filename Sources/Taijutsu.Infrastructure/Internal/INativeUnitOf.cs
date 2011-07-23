using System.ComponentModel;

namespace Taijutsu.Infrastructure.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface INativeUnitOf
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        object Native { get; }
    }
}