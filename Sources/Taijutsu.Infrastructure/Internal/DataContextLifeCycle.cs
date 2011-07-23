using System;

namespace Taijutsu.Infrastructure.Internal
{
    internal class DataContextLifeCycle : IUnitOfWorkLifeCycle, IDisposable
    {
        #region IDisposable Members

        public virtual void Dispose()
        {
            BeforeSuccessed = null;
            AfterSuccessed = null;
            BeforeFailed = null;
            AfterFailed = null;
        }

        #endregion

        #region IUnitOfWorkLifeCycle Members

        public event Action BeforeSuccessed;
        public event Action AfterSuccessed;
        public event Action BeforeFailed;
        public event Action AfterFailed;

        #endregion

        protected internal void RaiseBeforeSuccessed()
        {
            if (BeforeSuccessed != null)
            {
                BeforeSuccessed();
            }
        }

        protected internal void RaiseAfterSuccessed()
        {
            if (AfterSuccessed != null)
            {
                AfterSuccessed();
            }
        }

        protected internal void RaiseBeforeFailed()
        {
            if (BeforeFailed != null)
            {
                BeforeFailed();
            }
        }

        protected internal void RaiseAfterFailed()
        {
            if (AfterFailed != null)
            {
                AfterFailed();
            }
        }
    }
}