using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetNuke.Entities.Content.Workflow;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Tabs
{
    class TabWorkflowTracker : ServiceLocator<ITabChangeTracker, TabWorkflowTracker>, ITabChangeTracker
    {


        protected override Func<ITabChangeTracker> GetFactory()
        {
            return () => new TabWorkflowTracker();
        }

        public void TrackModuleAddition(ModuleInfo module, int moduleVersion, int userId)
        {
            NotifyWorkflowAboutChanges(module.PortalID, module.TabID, userId);
        }

        public void TrackModuleModification(ModuleInfo module, int moduleVersion, int userId)
        {
            NotifyWorkflowAboutChanges(module.PortalID, module.TabID, userId);
        }

        public void TrackModuleDeletion(ModuleInfo module, int moduleVersion, int userId)
        {
            NotifyWorkflowAboutChanges(module.PortalID, module.TabID, userId);
        }

        #region Private Statics Methods
        private void NotifyWorkflowAboutChanges(int portalId, int tabId, int userId)
        {
            if (!WorkflowSettings.Instance.IsWorkflowEnabled(portalId, tabId))
            {
                return;
            }

            var tabInfo = TabController.Instance.GetTab(tabId, portalId);
            if (WorkflowEngine.Instance.IsWorkflowCompleted(tabInfo))
            {
                var workflow = WorkflowManager.Instance.GetCurrentOrDefaultWorkflow(tabInfo, portalId);
                WorkflowEngine.Instance.StartWorkflow(workflow.WorkflowID, tabInfo.ContentItemId, userId);
                TabController.Instance.ClearCache(portalId);
            }
        }
        #endregion
    }
}
