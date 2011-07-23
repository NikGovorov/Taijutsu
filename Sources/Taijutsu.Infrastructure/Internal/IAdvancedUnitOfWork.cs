using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Taijutsu.Infrastructure.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IAdvancedUnitOfWork
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        IDictionary<string, IDisposable> Extension { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        IUnitOfWorkLifeCycle Advanced { get; }
    }
}