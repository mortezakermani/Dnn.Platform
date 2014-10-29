using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Dnn.Mvc.Framework.Modules;
using Dnn.Mvc.Helpers;
using Dnn.Mvc.Web.Models;

using DotNetNuke.Common;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.FileSystem;

namespace Dnn.Mvc.Web.Helpers
{
    public static class DnnHelperExtensions
    {
        private static TagBuilder CreateLink(DnnHelper helper, string text, string actionName, string controllerName, object routeValues)
        {
            var urlHelper = new DnnUrlHelper(helper.ViewContext.RequestContext, helper.RouteCollection);

            var url = urlHelper.Action(actionName, controllerName, routeValues);

            var linkBuilder = new TagBuilder("a");
            linkBuilder.MergeAttribute("href", url);
            linkBuilder.InnerHtml += text;

            return linkBuilder;
        }

        public static MvcHtmlString RenderBreadCrumbs(this DnnHelper helper)
        {
            var page = helper.ActivePage;

            TabController.Instance.PopulateBreadCrumbs(ref page);

            var breadCrumbBuilder = new TagBuilder("ol");
            breadCrumbBuilder.MergeAttribute("class", "breadcrumb");

            foreach (TabInfo breadCrumbPage in page.BreadCrumbs)
            {
                var breadCrumbItemBuilder = new TagBuilder("li");
                if (breadCrumbPage.TabID == page.TabID || breadCrumbPage.DisableLink)
                {
                    breadCrumbItemBuilder.MergeAttribute("class", "active");
                    breadCrumbItemBuilder.InnerHtml = breadCrumbPage.TabName;
                }
                else
                {
                    var linkBuilder = CreateLink(helper, breadCrumbPage.TabName, "Index", "Page", new { page = breadCrumbPage });
                    breadCrumbItemBuilder.InnerHtml = linkBuilder.ToString(TagRenderMode.Normal);
                }

                breadCrumbBuilder.InnerHtml += breadCrumbItemBuilder.ToString(TagRenderMode.Normal);
            }

            return new MvcHtmlString(breadCrumbBuilder.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString RenderLogo(this DnnHelper helper, string text)
        {
            var site = helper.ActiveSite;

            var logoBuilder = new TagBuilder("h1");

            if (!String.IsNullOrEmpty(site.LogoFile))
            {
                var logoFile = FileManager.Instance.GetFile(site.PortalID, site.LogoFile);

                if (logoFile != null)
                {
                    var imageUrl = Path.Combine(Globals.ApplicationPath, site.HomeDirectory, logoFile.RelativePath).Replace("\\", "/");
                    var imageBuilder = new TagBuilder("img");
                    imageBuilder.MergeAttribute("src", imageUrl);
                    imageBuilder.MergeAttribute("alt", site.PortalName);

                    logoBuilder.InnerHtml += imageBuilder.ToString(TagRenderMode.Normal);
                }

                if (!String.IsNullOrEmpty(text))
                {
                    logoBuilder.InnerHtml += text;
                }
            }

            return new MvcHtmlString(logoBuilder.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString RenderMenu(this DnnHelper helper, IList<TabInfo> pages)
        {
            var currentPage = helper.ActivePage;

            var menuBuilder = new TagBuilder("ul");
            menuBuilder.MergeAttribute("class", "nav navbar-nav");
            
            foreach (var page in pages)
            {
                var menuItemBuilder = new TagBuilder("li");


                if (page.HasChildren)
                {
                    menuItemBuilder.MergeAttribute("class", "dropdown");

                    var linkBuilder = CreateLink(helper, page.TabName, "Index", "Page", new {page = page});
                    linkBuilder.MergeAttribute("class", "dropdown-toggle");
                    linkBuilder.MergeAttribute("data-toggle", "dropdown");
                    linkBuilder.InnerHtml += "&nbsp;";

                    var caretBuilder = new TagBuilder("span");
                    caretBuilder.MergeAttribute("class", "caret");

                    linkBuilder.InnerHtml += caretBuilder.ToString(TagRenderMode.Normal);

                    menuItemBuilder.InnerHtml += linkBuilder.ToString(TagRenderMode.Normal);
                  
                    var childMenuBuilder = new TagBuilder("ul");
                    childMenuBuilder.MergeAttribute("class", "dropdown-menu");
                    childMenuBuilder.MergeAttribute("role", "menu");

                    foreach (var childPage in TabController.Instance.GetTabsByPortal(page.PortalID).WithParentId(page.TabID))
                    {
                        var childMenuItemBuilder = new TagBuilder("li");

                        var childLinkBuilder = CreateLink(helper, childPage.TabName, "Index", "Page", new { page = childPage });
                        childMenuItemBuilder.InnerHtml = childLinkBuilder.ToString(TagRenderMode.Normal);

                        childMenuBuilder.InnerHtml += childMenuItemBuilder.ToString(TagRenderMode.Normal);
                    }

                    menuItemBuilder.InnerHtml += childMenuBuilder.ToString(TagRenderMode.Normal);
                }
                else
                {
                    if (page.TabID == currentPage.TabID)
                    {
                        menuItemBuilder.MergeAttribute("class", "active");
                    }

                    var linkBuilder = CreateLink(helper, page.TabName, "Index", "Page", new { page = page });
                    menuItemBuilder.InnerHtml = linkBuilder.ToString(TagRenderMode.Normal);
                }
 
                menuBuilder.InnerHtml += menuItemBuilder.ToString(TagRenderMode.Normal);
            }

            return new MvcHtmlString(menuBuilder.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString RenderModule(this DnnHelper<ModuleRequestResult> helper)
        {
            var moduleResult = helper.ViewData.Model;

            MvcHtmlString moduleOutput;

            using (var writer = new StringWriter(CultureInfo.CurrentCulture))
            {
                var moduleExecutionEngine = ComponentFactory.GetComponent<IModuleExecutionEngine>();
                RenderWithinCommentedBlock(writer,
                                                "Body",
                                                moduleResult.Module.ModuleID,
                                                () => moduleExecutionEngine.ExecuteModuleResult(
                                                    helper.ViewContext.HttpContext.GetSiteContext(),
                                                    moduleResult,
                                                    writer)
                                            );
                moduleOutput = MvcHtmlString.Create(writer.ToString());
            }

            return moduleOutput;
        }

        public static MvcHtmlString RenderModuleActions(this DnnHelper<ModuleRequestResult> helper)
        {
            var request = helper.ViewContext.RequestContext.HttpContext.Request;
            var model = helper.ViewData.Model;

            var actionMenuBuilder = new TagBuilder("div");
            actionMenuBuilder.MergeAttribute("class", "btn-group");

            if (request.IsAuthenticated && model.ModuleMode != ModuleInjectMode.None)
            {
                if(ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "CONTENT", model.Module))
                {
                    if (model.ModuleMode == ModuleInjectMode.Inline || model.ModuleMode == ModuleInjectMode.Override)
                    {
                        var spanBuilder = new TagBuilder("span");
                        var route = String.Format("{0}/{1}", model.Application.DefaultControllerName, "Edit");
                        var linkBuilder = CreateLink(helper, "Edit", "Index", "Page", new { moduleId = model.Module.ModuleID, moduleRoute = route });

                        spanBuilder.InnerHtml = linkBuilder.ToString(TagRenderMode.Normal);
                        actionMenuBuilder.InnerHtml = spanBuilder.ToString(TagRenderMode.Normal);
                    }
                }
            }

            return new MvcHtmlString(actionMenuBuilder.ToString(TagRenderMode.Normal));
        }

        public static void RenderPane(this DnnHelper<PageViewModel> helper, string paneName)
        {
            var htmlHelper = new HtmlHelper(helper.ViewContext, helper.ViewDataContainer, helper.RouteCollection);

            var model = helper.ViewData.Model;
            if (model.Panes.ContainsKey(paneName))
            {
                foreach (var moduleResult in model.Panes[paneName].ModuleResults)
                {
                    htmlHelper.RenderPartial("Module", moduleResult);
                }
            }
        }

        private static void RenderWithinCommentedBlock(TextWriter writer, string blockName, int? moduleId, Action renderAction)
        {
            writer.WriteLine();
            writer.WriteLine("<!-- Start Module{0}{1} -->", moduleId.ToFormattedString("#{0}", String.Empty), blockName);
            renderAction();
            writer.WriteLine();
            writer.WriteLine("<!-- End Module{0}{1} -->", moduleId.ToFormattedString("#{0}", String.Empty), blockName);
        }

        private static string ToFormattedString<T>(this T? nullable, string formatString, string nullString) where T : struct
        {
            return nullable.HasValue ? String.Format(formatString, nullable.Value) : nullString;
        }
    }
}