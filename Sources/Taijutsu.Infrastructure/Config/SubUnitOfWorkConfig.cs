using System.Data;

namespace Taijutsu.Infrastructure.Config
{
    public class SubUnitOfWorkConfig
    {
        private bool runAsRoot;
        private UnitOfWorkConfig unitOfWorkConfig;

        protected internal SubUnitOfWorkConfig(bool runAsRoot, UnitOfWorkConfig unitOfWorkConfig)
        {
            this.unitOfWorkConfig = unitOfWorkConfig;
            this.runAsRoot = runAsRoot;
        }

        public virtual bool RunAsRoot
        {
            get { return runAsRoot; }
            protected set { runAsRoot = value; }
        }

        public virtual UnitOfWorkConfig UnitOfWorkConfig
        {
            get { return unitOfWorkConfig; }
            protected set { unitOfWorkConfig = value; }
        }

        public static implicit operator SubUnitOfWorkConfig(string dataSourceName)
        {
            return new SubUnitOfWorkConfig(false, new UnitOfWorkConfig(dataSourceName));
        }
    }

    public static class AsRoot
    {
        public static SubUnitOfWorkConfig IfThereIsNoRoot(
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, string source = "")
        {
            return new SubUnitOfWorkConfig(true, new UnitOfWorkConfig(source, isolationLevel));
        }
    }
}