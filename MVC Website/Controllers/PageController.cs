using System;
using System.Linq;
using System.Web.Mvc;
using Dnn.Mvc.Framework.ActionResults;
using Dnn.Mvc.Framework.Controllers;
using Dnn.Mvc.Framework.Modules;
using Dnn.Mvc.Web.Models;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Modules;
using Dnn.Mvc.Utils.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security.Permissions;

namespace Dnn.Mvc.Web.Controllers
{
    public class PageController : DnnControllerBase
    {
        private ModuleInjectMode GetModuleInjectMode(string mode)
        {
            ModuleInjectMode injectMode;
            switch (mode)
            {
                case "Override":
                    injectMode = ModuleInjectMode.Override;
                    break;
                case "Modal":
                    injectMode = ModuleInjectMode.Modal;
                    break;
                default:
                    injectMode = ModuleInjectMode.Inline;
                    break;
            }
            return injectMode;
        }

        public ActionResult Index(int? moduleId, string moduleRoute)
        {
            var pages = TabController.Instance.GetTabsByPortal(ActiveSite.PortalID).WithParentId(-1);

            // Start setting up the view model
            var pageModel = new PageViewModel
                                    {
                                        Page = ActivePage,
                                        HomePage = pages.SingleOrDefault(t => t.TabID == ActiveSite.HomeTabId),
                                        Site = ActiveSite,
                                        SiteAlias = ActiveSiteAlias,
                                        Pages = pages.Where(t => t.IsVisible)
                                                    .ToList()
                                    };

            if (!TabPermissionController.CanViewPage(ActivePage))
            {
                ViewBag.Message = "No Permission to View Page";
                return View("AccessDenied", pageModel);
            }

            if(ActivePage.StartDate > DateTime.Now && !TabPermissionController.CanAdminPage(ActivePage))
            {
                ViewBag.Message = "Page not active yet";
                return View("AccessDenied", pageModel);
            }

            if (ActivePage.EndDate > DateTime.MinValue && ActivePage.EndDate < DateTime.Now && !TabPermissionController.CanAdminPage(ActivePage))
            {
                ViewBag.Message = "Page has expired";
                return View("AccessDenied", pageModel);
            }

            var modules = DotNetNuke.Entities.Modules.ModuleController.Instance.GetTabModules(ActivePage.TabID);

            var moduleExecutionEngine = ComponentFactory.GetComponent<IModuleExecutionEngine>();

            //TODO PortalControllerAdapter usage is temporary in order to make method testable
            var injectMode = GetModuleInjectMode(PortalControllerAdapter.Instance.GetPortalSetting(ActiveSite.PortalID, "ModuleInjectMode", "Inline"));

            // If there is a selected module (moduleId != null), run it first
            ModuleRequestResult selectedResult = null;
            if (moduleId.HasValue && injectMode != ModuleInjectMode.Modal)
            {
                ModuleInfo selectedModule = null;

                if (modules.TryGetValue(moduleId.Value, out selectedModule))
                {
                    if (CanInjectModule(selectedModule))
                    {
                        selectedResult = moduleExecutionEngine.ExecuteModule(HttpContext, MvcMode.Standard, selectedModule, moduleRoute);

                        if (selectedResult != null && selectedResult.ActionResult is PageOverrideResult)
                        {
                            return new RenderModuleResult { ModuleRequestResult = selectedResult };
                        }

                        //Inject module into Content Pane if  moduleMode="override"
                        if (injectMode == ModuleInjectMode.Override)
                        {
                            pageModel.AddModuleResultToPane(selectedResult, "ContentPane");
                        }
                    }
                }
            }

            if (selectedResult == null || injectMode == ModuleInjectMode.Inline)
            {
                // Execute all the modules on the page            
                foreach (var module in modules.Values)
                {
                    if (CanInjectModule(module))
                    {
                        ModuleRequestResult result;

                        // If the current module is the selected module, then we already have the result, so we
                        // don't need to execute it.
                        if (selectedResult != null && moduleId == module.ModuleID)
                        {
                            result = selectedResult;
                        }
                        else
                        {
                            result = moduleExecutionEngine.ExecuteModule(HttpContext, MvcMode.Hosted, module, String.Empty);
                        }

                        //Add result to PaneViewModel
                        if (result != null)
                        {
                            result.ModuleMode = injectMode;
                            pageModel.AddModuleResultToPane(result, module.PaneName);
                        }
                    }
                }
            }

            return View(pageModel);
        }

        private bool CanInjectModule(ModuleInfo module)
        {
            return ModulePermissionController.CanViewModule(module)
                   && module.IsDeleted == false
                   && ((module.StartDate < DateTime.Now && 
                        (module.EndDate == DateTime.MinValue || module.EndDate > DateTime.Now)));
        }
    }
}