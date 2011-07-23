using System;

namespace Taijutsu.Infrastructure.Internal
{
    public interface IUnitOfWorkLifeCycle
    {
        event Action BeforeSuccessed;
        event Action AfterSuccessed;
        event Action BeforeFailed;
        event Action AfterFailed;
    }
}