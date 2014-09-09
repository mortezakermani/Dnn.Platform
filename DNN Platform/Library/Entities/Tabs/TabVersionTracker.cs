#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Tabs
{
    public class TabVersionTracker: ServiceLocator<ITabVersionTracker, TabVersionTracker>, ITabVersionTracker
    {
        public void TrackModuleAddition(int tabId, int createdByUserID, ModuleInfo module, int moduleVersion)
        {
            bool newTabVersion;
            var unPublishedVersion = GetUnPublishedTabVersion(tabId, createdByUserID, out newTabVersion);
            TabVersionDetail tabVersionDetail;
            if (newTabVersion)
            {
                tabVersionDetail = GetTabVersionDetailFromModule(unPublishedVersion.TabVersionId, module, moduleVersion, TabVersionDetailAction.Added);

                TabVersionDetailController.Instance.SaveTabVersionDetail(tabVersionDetail, createdByUserID);
                return;
            }
            
            if (TabVersionDetailController.Instance.GetTabVersionDetails(unPublishedVersion.TabVersionId).Any(tvd => tvd.ModuleId == module.ModuleID))
            {
                //TODO localize Exception messages
                throw new Exception(String.Format("For Tab {0}, the unpublished Version already contains a version of the module {1}", tabId, module.ModuleID));
            }

            tabVersionDetail = GetTabVersionDetailFromModule(unPublishedVersion.TabVersionId, module, moduleVersion, TabVersionDetailAction.Added);

            TabVersionDetailController.Instance.SaveTabVersionDetail(tabVersionDetail, createdByUserID);

        }

        public void TrackModuleModification(int tabId, int createdByUserID, ModuleInfo module, int moduleVersion)
        {
            TrackModuleModification(tabId, createdByUserID, module.ModuleID, module.PaneName, module.ModuleOrder, moduleVersion);
        }

        public void TrackModuleModification(int tabId, int createdByUserID, int moduleId, string paneName, int moduleOrder, int moduleVersion)
        {
            bool newTabVersion;
            var unPublishedVersion = GetUnPublishedTabVersion(tabId, createdByUserID, out newTabVersion);
            TabVersionDetail tabVersionDetail = GetTabVersionDetailFromModule(unPublishedVersion.TabVersionId, moduleId, paneName, moduleOrder, moduleVersion, TabVersionDetailAction.Modified);
            if (TabVersionDetailController.Instance.GetTabVersionDetails(unPublishedVersion.TabVersionId).Any(tvd => tvd.ModuleId == moduleId))
            {
                var existingTabDetail = TabVersionDetailController.Instance.GetTabVersionDetails(unPublishedVersion.TabVersionId).SingleOrDefault(tvd => tvd.ModuleId == moduleId);
                tabVersionDetail.TabVersionDetailId = existingTabDetail.TabVersionDetailId;
                if (moduleVersion == Null.NullInteger)
                {
                    tabVersionDetail.ModuleVersion = existingTabDetail.ModuleVersion;
                }
            }

            TabVersionDetailController.Instance.SaveTabVersionDetail(tabVersionDetail, createdByUserID);
        }

        public void TrackModuleDeletion(int tabId, int createdByUserID, ModuleInfo module, int moduleVersion)
        {
            bool newTabVersion;
            var unPublishedVersion = GetUnPublishedTabVersion(tabId, createdByUserID, out newTabVersion);
            TabVersionDetail tabVersionDetail = GetTabVersionDetailFromModule(unPublishedVersion.TabVersionId, module, moduleVersion, TabVersionDetailAction.Deleted);
            
            if (TabVersionDetailController.Instance.GetTabVersionDetails(unPublishedVersion.TabVersionId).Any(tvd => tvd.ModuleId == module.ModuleID))
            {
                var existingTabDetail = TabVersionDetailController.Instance.GetTabVersionDetails(unPublishedVersion.TabVersionId).SingleOrDefault(tvd => tvd.ModuleId == module.ModuleID);
                TabVersionDetailController.Instance.DeleteTabVersionDetail(existingTabDetail.TabVersionId, existingTabDetail.TabVersionDetailId);
                return;
            }

            TabVersionDetailController.Instance.SaveTabVersionDetail(tabVersionDetail, createdByUserID);
        }
        
        private TabVersion GetUnPublishedTabVersion(int tabId, int createdByUserID, out bool newTabVersion)
        {
            if (TabVersionController.Instance.GetTabVersions(tabId).All(tv => tv.IsPublished))
            {
                newTabVersion = true;
                return TabVersionMaker.Instance.CreateNewVersion(tabId, createdByUserID);
            }
            newTabVersion = false;
            return TabVersionController.Instance.GetTabVersions(tabId).SingleOrDefault(tv => !tv.IsPublished);
        }

        
        
        
        private TabVersionDetail GetTabVersionDetailFromModule(int tabVersionId, ModuleInfo module, int moduleVersion, TabVersionDetailAction action)
        {
            return new TabVersionDetail
            {
                TabVersionDetailId = 0,
                TabVersionId = tabVersionId,
                ModuleId = module.ModuleID,
                ModuleVersion = moduleVersion,
                ModuleOrder = module.ModuleOrder,
                PaneName = module.PaneName,
                Action = action              
            };

        }

        private TabVersionDetail GetTabVersionDetailFromModule(int tabVersionId, int moduleId, string paneName, int moduleOrder, int moduleVersion, TabVersionDetailAction action)
        {
            return new TabVersionDetail
            {
                TabVersionDetailId = 0,
                TabVersionId = tabVersionId,
                ModuleId = moduleId,
                ModuleVersion = moduleVersion,
                ModuleOrder = moduleOrder,
                PaneName = paneName,
                Action = action
            };

        }
        
        protected override Func<ITabVersionTracker> GetFactory()
        {
            return () => new TabVersionTracker();
        }
    }
}
