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
            bool newTabVersion;
            var unPublishedVersion = GetUnPublishedTabVersion(tabId, createdByUserID, out newTabVersion);
            TabVersionDetail tabVersionDetail = GetTabVersionDetailFromModule(unPublishedVersion.TabVersionId, module, moduleVersion, TabVersionDetailAction.Modified);

            TabVersionDetailController.Instance.SaveTabVersionDetail(tabVersionDetail, createdByUserID);
        }

        public void TrackModuleDeletion(int tabId, int createdByUserID, ModuleInfo module, int moduleVersion)
        {
            bool newTabVersion;
            var unPublishedVersion = GetUnPublishedTabVersion(tabId, createdByUserID, out newTabVersion);
            TabVersionDetail tabVersionDetail = GetTabVersionDetailFromModule(unPublishedVersion.TabVersionId, module, moduleVersion, TabVersionDetailAction.Deleted);
            
            TabVersionDetailController.Instance.SaveTabVersionDetail(tabVersionDetail, createdByUserID);
        }

        public IEnumerable<TabVersionDetail> GetVersionModules(int tabId, int version, bool ignoreCache = false)
        {
            //if we are not using the cache
            if (ignoreCache || Host.Host.PerformanceSetting == Globals.PerformanceSettings.NoCaching)
            {
                return GetVersionModulesInternal(tabId, version);
            }

            string cacheKey = string.Format(DataCache.TabVersionModulesCacheKey, tabId, version);
            return CBO.GetCachedObject<List<TabVersionDetail>>(new CacheItemArgs(cacheKey,
                                                                    DataCache.TabVersionModulesTimeOut,
                                                                    DataCache.TabVersionModulesPriority),
                                                            c =>
                                                            {
                                                                return GetVersionModulesInternal(tabId, version);
                                                            });
        }

        private IEnumerable<TabVersionDetail> GetVersionModulesInternal(int tabId, int version)
        {
            var tabVersionDetails = TabVersionDetailController.Instance.GetVersionHistory(tabId, version);

            var versionModules = new Dictionary<int, TabVersionDetail>();
            foreach (var tabVersionDetail in tabVersionDetails)
            {
                switch (tabVersionDetail.Action)
                {
                    case TabVersionDetailAction.Added:                        
                    case TabVersionDetailAction.Modified:
                        if (versionModules.ContainsKey(tabVersionDetail.ModuleId))
                        {
                            versionModules[tabVersionDetail.ModuleId] = tabVersionDetail;
                        }
                        else
                        {
                            versionModules.Add(tabVersionDetail.ModuleId, tabVersionDetail);
                        }
                        break;
                    case TabVersionDetailAction.Deleted:
                        if (versionModules.ContainsKey(tabVersionDetail.ModuleId))
                        {
                            versionModules.Remove(tabVersionDetail.ModuleId);
                        }
                        break;
                }
            }

            return versionModules.Values.ToList();
        }

        private TabVersion GetUnPublishedTabVersion(int tabId, int createdByUserID, out bool newTabVersion)
        {
            if (TabVersionController.Instance.GetTabVersions(tabId).All(tv => tv.IsPublished))
            {
                newTabVersion = true;
                return CreateNewVersion(tabId, createdByUserID);
            }
            newTabVersion = false;
            return TabVersionController.Instance.GetTabVersions(tabId).SingleOrDefault(tv => !tv.IsPublished);
        }

        private TabVersion CreateNewVersion(int tabId, int createdByUserID)
        {
            //TODO Get this value from Settings
            var maxVersionsAllowed = 5;
            var tabVersionsOrdered = TabVersionController.Instance.GetTabVersions(tabId).OrderByDescending(tv => tv.Version);
            var tabVersionCount = tabVersionsOrdered.Count();
            if ( tabVersionCount >= maxVersionsAllowed)
            {
                //The last existing version is going to be deleted, therefore we need to add the snapshot to the previous one
                var snapShotTabVersion = tabVersionsOrdered.ElementAtOrDefault(tabVersionCount - 2);
                CreateSnapshotOverVersion(tabId, tabVersionsOrdered, snapShotTabVersion);
                DeleteOldVersions(tabVersionsOrdered, snapShotTabVersion);
            }

            return TabVersionController.Instance.CreateTabVersion(tabId, createdByUserID);
        }
        
        private void CreateSnapshotOverVersion(int tabId, IOrderedEnumerable<TabVersion> tabVersionsOrdered, TabVersion snapshoTabVersion)
        {
            var snapShotTabVersionDetails = GetVersionModulesInternal(tabId, snapshoTabVersion.Version);
            
            var existingTabVersionDetails = TabVersionDetailController.Instance.GetTabVersionDetails(snapshoTabVersion.TabVersionId);
            for(int i = existingTabVersionDetails.Count(); i > 0; i--)
            {
                var existingDetail = existingTabVersionDetails.ElementAtOrDefault(i - 1);
                if (snapShotTabVersionDetails.All(tvd => tvd.TabVersionDetailId != existingDetail.TabVersionDetailId))
                {
                    TabVersionDetailController.Instance.DeleteTabVersionDetail(existingDetail.TabVersionId, existingDetail.TabVersionDetailId);                    
                }
            }

            foreach (var tabVersionDetail in snapShotTabVersionDetails)
            {
                tabVersionDetail.TabVersionId = snapshoTabVersion.TabVersionId;
                TabVersionDetailController.Instance.SaveTabVersionDetail(tabVersionDetail);
            }

        }
        
        private void DeleteOldVersions(IEnumerable<TabVersion> tabVersionsOrdered, TabVersion snapShotTabVersion)
        {
            var oldVersions = tabVersionsOrdered.Where(tv => tv.Version < snapShotTabVersion.Version);
            for (int i = oldVersions.Count(); i > 0; i--)
            {
                var oldVersion = oldVersions.ElementAtOrDefault(i - 1);
                var oldVersionDetails = TabVersionDetailController.Instance.GetTabVersionDetails(oldVersion.TabVersionId);
                for (int j = oldVersionDetails.Count(); j > 0; j--)
                {
                    var oldVersionDetail = oldVersionDetails.ElementAtOrDefault(j - 1);
                    TabVersionDetailController.Instance.DeleteTabVersionDetail(oldVersionDetail.TabVersionId, oldVersionDetail.TabVersionDetailId);
                }
                TabVersionController.Instance.DeleteTabVersion(oldVersion.TabId, oldVersion.TabVersionId);
            }
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
        
        protected override Func<ITabVersionTracker> GetFactory()
        {
            return () => new TabVersionTracker();
        }
    }
}
