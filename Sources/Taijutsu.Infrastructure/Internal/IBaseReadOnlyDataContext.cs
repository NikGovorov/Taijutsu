using System;

namespace Taijutsu.Infrastructure.Internal
{
    public interface IBaseReadOnlyDataContext : IDisposable
    {
        bool Closed { get; }
        IReadOnlyDataProvider ReadOnlyProvider { get; }
        DateTime CreationDate { get; }
        void Close();
    }

    public interface IReadOnlyDataContext : IBaseReadOnlyDataContext
    {
    }
}