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
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Tabs
{
    public class TabVersionMaker : ServiceLocator<ITabVersionMaker, TabVersionMaker>, ITabVersionMaker
    {

        public void Publish(int tabId, int createdByUserID)
        {
            CheckVersioningEnabled();

            var tabVersion = GetUnPublishedVersion(tabId);
            if (tabVersion == null)
            {
                //TODO Localize Exception message
                throw new Exception(String.Format("Tha tab {0} has not an unpublished version", tabId));
            }
            if (tabVersion.IsPublished)
            {
                //TODO Localize Exception message
                throw new Exception(String.Format("For Tab {0}, the version {1} is already published", tabId, tabVersion.Version));
            }
            PublishVersion(tabId, createdByUserID, tabVersion);

        }

        private void PublishVersion(int tabId, int createdByUserID, TabVersion tabVersion)
        {
            
            tabVersion.IsPublished = true;
            var unPublishedDetails = TabVersionDetailController.Instance.GetTabVersionDetails(tabVersion.TabVersionId);
            foreach (var unPublishedDetail in unPublishedDetails)
            {
                if (unPublishedDetail.ModuleVersion != Null.NullInteger)
                {
                    PublishDetail(tabId, unPublishedDetail);
                }
            }
            
            TabVersionController.Instance.SaveTabVersion(tabVersion, tabVersion.CreatedByUserID, createdByUserID);
        }

        public void Discard(int tabId, int createdByUserID)
        {
            CheckVersioningEnabled();

            var tabVersion = GetUnPublishedVersion(tabId);
            if (tabVersion == null)
            {
                //TODO Localize Exception message
                throw new Exception(String.Format("Tha tab {0} has not an unpublished version", tabId));
            }
            if (tabVersion.IsPublished)
            {
                //TODO Localize Exception message
                throw new Exception(String.Format("For Tab {0}, the version {1} is already published", tabId, tabVersion.Version));
            }
            if (TabVersionController.Instance.GetTabVersions(tabId).Count() == 1)
            {
                //TODO Localize Exception message
                throw new Exception(String.Format("The tab {0} has only one version created. It cannot be discarded", tabId, tabVersion.Version));
            }
            DiscardVersion(tabId, createdByUserID, tabVersion);
        }

        public void DiscardVersion(int tabId, int createdByUserID, TabVersion tabVersion)
        {
            var unPublishedDetails = TabVersionDetailController.Instance.GetTabVersionDetails(tabVersion.TabVersionId);
            var publishedChanges = GetVersionModulesInternal(tabId, GetCurrentVersion(tabId).Version);
            foreach (var unPublishedDetail in unPublishedDetails)
            {
                if (unPublishedDetail.Action == TabVersionDetailAction.Deleted)
                {
                    var restoredModule = ModuleController.Instance.GetModule(unPublishedDetail.ModuleId, tabId, true);
                    ModuleController.Instance.RestoreModule(restoredModule);
                    var restoredModuleDetail = publishedChanges.SingleOrDefault(tv => tv.ModuleId == restoredModule.ModuleID);
                    restoredModule.PaneName = restoredModuleDetail.PaneName;
                    restoredModule.ModuleOrder = restoredModuleDetail.ModuleOrder;
                    ModuleController.Instance.UpdateModule(restoredModule);
                    continue;
                }
                
                if (publishedChanges.All(tv => tv.ModuleId != unPublishedDetail.ModuleId))
                {
                    ModuleController.Instance.DeleteTabModule(tabId, unPublishedDetail.ModuleId, true);
                    continue;
                }
                
                if (unPublishedDetail.ModuleVersion != Null.NullInteger)
                {
                    DiscardDetail(tabId, unPublishedDetail);
                }
            }

            TabVersionController.Instance.DeleteTabVersion(tabId, tabVersion.TabVersionId);
        }

        public void DeleteVersion(int tabId, int createdByUserID, int version)
        {
            CheckVersioningEnabled();

            if (GetUnPublishedVersion(tabId) != null)
            {
                //TODO Localize Exception message
                throw new Exception(String.Format("For Tab {0}, the version {1} cannot be deleted because an unpublished version exists", tabId, version));
            }

            var tabVersions = TabVersionController.Instance.GetTabVersions(tabId).OrderByDescending(tv => tv.Version);
            if (tabVersions.FirstOrDefault().Version == version)
            {
                var tabVersion = tabVersions.FirstOrDefault();
                var unPublishedDetails = TabVersionDetailController.Instance.GetTabVersionDetails(tabVersion.TabVersionId);
                foreach (var unPublishedDetail in unPublishedDetails)
                {
                    if (unPublishedDetail.Action == TabVersionDetailAction.Added)
                    {
                        ModuleController.Instance.DeleteTabModule(tabId, unPublishedDetail.ModuleId, true);
                        continue;
                    }
                    if (unPublishedDetail.ModuleVersion != Null.NullInteger && unPublishedDetail.Action == TabVersionDetailAction.Modified)
                    {
                        DiscardDetail(tabId, unPublishedDetail);
                    }    
                }
                TabVersionController.Instance.DeleteTabVersion(tabId, tabVersion.TabVersionId);
            }
            else
            {
                for (int i = 1; i < tabVersions.Count(); i++)
                {
                    if (tabVersions.ElementAtOrDefault(i).Version == version)
                    {
                        CreateSnapshotOverVersion(tabId, tabVersions.ElementAtOrDefault(i-1));
                        TabVersionController.Instance.DeleteTabVersion(tabId, tabVersions.ElementAtOrDefault(i).TabVersionId);
                        return;
                    }
                }
            }
        }

        public TabVersion RollBackVesion(int tabId, int createdByUserID, int version)
        {
            CheckVersioningEnabled();

            if (GetUnPublishedVersion(tabId) != null)
            {
                //TODO Localize Exception message
                throw new Exception(String.Format("For Tab {0}, the version {1} cannot be rolled back because an unpublished version exists", tabId, version));
            }

            var rollbackDetails = CopyVersionDetails(GetVersionModulesInternal(tabId, version));
            
            var newVersion = CreateNewVersion(tabId, createdByUserID);
            TabVersionDetailController.Instance.SaveTabVersionDetail( new TabVersionDetail
            {
                PaneName = "none_resetAction",
                TabVersionId = newVersion.TabVersionId,
                Action = TabVersionDetailAction.Reset,
                ModuleId = Null.NullInteger,
                ModuleVersion = Null.NullInteger
            }, createdByUserID);

            foreach (var rollbackDetail in rollbackDetails)
            {
                rollbackDetail.TabVersionId = newVersion.TabVersionId;
                TabVersionDetailController.Instance.SaveTabVersionDetail(rollbackDetail, createdByUserID);
                RollBackDetail(tabId, rollbackDetail);
            }

            return newVersion;
        }

        private IEnumerable<TabVersionDetail> CopyVersionDetails(IEnumerable<TabVersionDetail> tabVersionDetails)
        {
            var result = new List<TabVersionDetail>();
            foreach (var tabVersionDetail in tabVersionDetails)
            {
                result.Add(new TabVersionDetail
                {
                    ModuleId = tabVersionDetail.ModuleId,
                    ModuleOrder = tabVersionDetail.ModuleOrder,
                    ModuleVersion = tabVersionDetail.ModuleVersion,
                    PaneName = tabVersionDetail.PaneName,
                    Action = tabVersionDetail.Action
                });
            }
            return result;
        }

        public TabVersion CreateNewVersion(int tabId, int createdByUserID) 
        {
            CheckVersioningEnabled();

            var maxVersionsAllowed = TabVersionSettings.Instance.MaximunNumberOfVersions;
            var tabVersionsOrdered = TabVersionController.Instance.GetTabVersions(tabId).OrderByDescending(tv => tv.Version);
            var tabVersionCount = tabVersionsOrdered.Count();
            if (tabVersionCount >= maxVersionsAllowed)
            {
                //The last existing version is going to be deleted, therefore we need to add the snapshot to the previous one
                var snapShotTabVersion = tabVersionsOrdered.ElementAtOrDefault(maxVersionsAllowed - 2);
                CreateSnapshotOverVersion(tabId, snapShotTabVersion);
                DeleteOldVersions(tabVersionsOrdered, snapShotTabVersion);
            }

            return TabVersionController.Instance.CreateTabVersion(tabId, createdByUserID);
        }

        private void CheckVersioningEnabled()
        {
            if (!TabVersionSettings.Instance.VersioningEnabled)
            {
                //TODO Localize Exception message
                throw new Exception("Tab Versioning is not enabled");
            }
        }

        private void CreateSnapshotOverVersion(int tabId, TabVersion snapshoTabVersion)
        {
            var snapShotTabVersionDetails = GetVersionModulesInternal(tabId, snapshoTabVersion.Version);

            var existingTabVersionDetails = TabVersionDetailController.Instance.GetTabVersionDetails(snapshoTabVersion.TabVersionId);
            for (int i = existingTabVersionDetails.Count(); i > 0; i--)
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

        public IEnumerable<ModuleInfo> GetVersionModules(int tabId, int version, bool ignoreCache = false)
        {
            //if we are not using the cache
            if (ignoreCache || Host.Host.PerformanceSetting == Globals.PerformanceSettings.NoCaching)
            {
                return convertToModuleInfo(GetVersionModulesInternal(tabId, version), ignoreCache);
            }

            string cacheKey = string.Format(DataCache.TabVersionModulesCacheKey, tabId, version);
            return CBO.GetCachedObject<List<ModuleInfo>>(new CacheItemArgs(cacheKey,
                                                                    DataCache.TabVersionModulesTimeOut,
                                                                    DataCache.TabVersionModulesPriority),
                                                            c =>
                                                            {
                                                                return convertToModuleInfo(GetVersionModulesInternal(tabId, version), ignoreCache);
                                                            });
        }

        public IEnumerable<ModuleInfo> GetUnPublishedVersionModules(int tabId)
        {
            var unPublishedVersion = GetUnPublishedVersion(tabId);
            if (unPublishedVersion == null)
            {
                return CBO.FillCollection<ModuleInfo>(DataProvider.Instance().GetTabModules(tabId)).Select(t => t.Clone());
            }

            var tabVersionDetails = TabVersionDetailController.Instance.GetVersionHistory(tabId, unPublishedVersion.TabVersionId);
            return convertToModuleInfo(GetSnapShot(tabVersionDetails), true);
        }

        public TabVersion GetCurrentVersion(int tabId, bool ignoreCache = false)
        {
            return TabVersionController.Instance.GetTabVersions(tabId, ignoreCache).Where(tv => tv.IsPublished).OrderByDescending(tv => tv.CreatedOnDate).FirstOrDefault();
        }

        public TabVersion GetUnPublishedVersion(int tabId)
        {
            return TabVersionController.Instance.GetTabVersions(tabId, true).SingleOrDefault(tv => !tv.IsPublished);
        }

        public IEnumerable<ModuleInfo> GetCurrentModules(int tabId, bool ignoreCache = false)
        {
            //if we are not using the cache
            if (ignoreCache || Host.Host.PerformanceSetting == Globals.PerformanceSettings.NoCaching)
            {
                return GetCurrentModulesInternal(tabId, true);
            }

            string cacheKey = string.Format(DataCache.TabVersionModulesCacheKey, tabId);
            return CBO.GetCachedObject<List<ModuleInfo>>(new CacheItemArgs(cacheKey,
                                                                    DataCache.TabVersionModulesTimeOut,
                                                                    DataCache.TabVersionModulesPriority),
                                                            c =>
                                                            {
                                                                return GetCurrentModulesInternal(tabId, true);
                                                            });
        }

        private IEnumerable<ModuleInfo> convertToModuleInfo(IEnumerable<TabVersionDetail> details, bool ignoreCache)
        {
            var modules = new List<ModuleInfo>();
            foreach (var detail in details)
            {
                var module = ModuleController.Instance.GetModule(detail.ModuleId, Null.NullInteger, ignoreCache);
                if (module == null)
                {
                    continue;
                }

                ModuleInfo cloneModule = module.Clone();
                cloneModule.ModuleVersion = detail.ModuleVersion;
                cloneModule.PaneName = detail.PaneName;
                cloneModule.ModuleOrder = detail.ModuleOrder;
                modules.Add(cloneModule);
            };

            return modules;
        }

        private IEnumerable<ModuleInfo> GetCurrentModulesInternal(int tabId, bool ignoreCache)
        {
            var currentVersion = GetCurrentVersion(tabId);

            if (currentVersion == null) //Only when a tab is on a first version and it is not published, the currentVersion object can be null
            {
                return CBO.FillCollection<ModuleInfo>(DataProvider.Instance().GetTabModules(tabId)).Select(t => t.Clone());
            }
            
            return convertToModuleInfo(GetVersionModulesInternal(tabId, currentVersion.Version), ignoreCache);
        }

        private IEnumerable<TabVersionDetail> GetVersionModulesInternal(int tabId, int version)
        {
            var tabVersionDetails = TabVersionDetailController.Instance.GetVersionHistory(tabId, version);
            
            return GetSnapShot(tabVersionDetails);
        }

        private void RollBackDetail(int tabId, TabVersionDetail unPublishedDetail)
        {
            var moduleInfo = ModuleController.Instance.GetModule(unPublishedDetail.ModuleId, tabId, true);

            var versionableController = GetVersionableController(moduleInfo);
            if (versionableController != null)
            {
                versionableController.RollBackVersion(unPublishedDetail.ModuleId, unPublishedDetail.ModuleVersion);
            }
        }

        private void PublishDetail(int tabId, TabVersionDetail unPublishedDetail)
        {
            var moduleInfo = ModuleController.Instance.GetModule(unPublishedDetail.ModuleId, tabId, true);

            var versionableController = GetVersionableController(moduleInfo);
            if (versionableController != null)
            {
                versionableController.PublishVersion(unPublishedDetail.ModuleId, unPublishedDetail.ModuleVersion);
            }
        }

        private void DiscardDetail(int tabId, TabVersionDetail unPublishedDetail)
        {
            var moduleInfo = ModuleController.Instance.GetModule(unPublishedDetail.ModuleId, tabId, true);

            var versionableController = GetVersionableController(moduleInfo);
            if (versionableController != null)
            {
                versionableController.DeleteVersion(unPublishedDetail.ModuleId, unPublishedDetail.ModuleVersion);                
            }
        }

        private IVersionable GetVersionableController(ModuleInfo moduleInfo)
        {
            if (String.IsNullOrEmpty(moduleInfo.DesktopModule.BusinessControllerClass))
            {
                return null;
            }
            
            object controller = Reflection.CreateObject(moduleInfo.DesktopModule.BusinessControllerClass, "");
            if (controller is IVersionable)
            {
                return controller as IVersionable;
            }
            return null;
        }

        private static IEnumerable<TabVersionDetail> GetSnapShot(IEnumerable<TabVersionDetail> tabVersionDetails)
        {
            var versionModules = new Dictionary<int, TabVersionDetail>();
            foreach (var tabVersionDetail in tabVersionDetails)
            {
                switch (tabVersionDetail.Action)
                {
                    case TabVersionDetailAction.Added:
                    case TabVersionDetailAction.Modified:
                        if (versionModules.ContainsKey(tabVersionDetail.ModuleId))
                        {
                            versionModules[tabVersionDetail.ModuleId] = JoinVersionDetails(versionModules[tabVersionDetail.ModuleId], tabVersionDetail);
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
                    case TabVersionDetailAction.Reset:
                        versionModules.Clear();
                        break;
                }
            }

            return versionModules.Values.ToList();
        }

        private static TabVersionDetail JoinVersionDetails(TabVersionDetail tabVersionDetail, TabVersionDetail newVersionDetail)
        {
            //Movement changes have not ModuleVersion
            if (newVersionDetail.ModuleVersion == Null.NullInteger)
            {
                newVersionDetail.ModuleVersion = tabVersionDetail.ModuleVersion;
            }

            return newVersionDetail;
        }

        protected override Func<ITabVersionMaker> GetFactory()
        {
            return () => new TabVersionMaker();
        }
    }
}
