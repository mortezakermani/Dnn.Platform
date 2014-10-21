using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs.TabVersions;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Tabs
{
    public class TabChangeTracker : ServiceLocator<ITabChangeTracker, TabChangeTracker>, ITabChangeTracker
    {
        public void TrackModuleAddition(ModuleInfo module, int moduleVersion, int userId)
        {
            TabVersionTracker.Instance.TrackModuleAddition(module, moduleVersion, userId);
            TabWorkflowTracker.Instance.TrackModuleAddition(module, moduleVersion, userId);
        }

        public void TrackModuleModification(ModuleInfo module, int moduleVersion, int userId)
        {
            TabVersionTracker.Instance.TrackModuleModification(module, moduleVersion, userId);
            TabWorkflowTracker.Instance.TrackModuleModification(module, moduleVersion, userId);
        }

        public void TrackModuleDeletion(ModuleInfo module, int moduleVersion, int userId)
        {
            TabVersionTracker.Instance.TrackModuleDeletion(module, moduleVersion, userId);
            TabWorkflowTracker.Instance.TrackModuleDeletion(module, moduleVersion, userId);
        }

        protected override Func<ITabChangeTracker> GetFactory()
        {
            return () => new TabChangeTracker();
        }
    }
}
