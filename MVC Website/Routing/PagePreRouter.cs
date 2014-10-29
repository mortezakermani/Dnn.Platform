using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Dnn.Mvc.Helpers;
using Dnn.Mvc.Routing;
using Dnn.Mvc.Utils.Entities.Portals;
using Dnn.Mvc.Web.Helpers;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;

using Microsoft.SqlServer.Server;

namespace Dnn.Mvc.Web.Routing
{
    public class PagePreRouter : PreRouterBase
    {
        private TabInfo GetPage(string pagePath, int siteId)
        {
            TabInfo page;
            var pages = (TabController.Instance.GetTabsByPortal(siteId)).AsList();

            var path = pagePath;
            do
            {
                page = pages.OrderBy(p => p.TabPath.Length)
                            .FirstOrDefault(p => p.TabPath.Replace("//", "/").StartsWith(path, StringComparison.CurrentCultureIgnoreCase));
                if(page != null || String.IsNullOrEmpty(path))
                {
                    break;
                }
                path = path.Substring(0, path.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase));
            } while (!String.IsNullOrEmpty(path));

            return page;
        }

        private string NormalizePagePath(string pagePath)
        {
            if (pagePath.StartsWith("~/", StringComparison.OrdinalIgnoreCase))
            {
                pagePath = pagePath.Substring(1);
            }
            else if (!pagePath.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                pagePath = "/" + pagePath;
            }
            if (pagePath.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                pagePath = pagePath.Substring(0, pagePath.Length - 1);
            }
            if (String.IsNullOrEmpty(pagePath))
            {
                return "/";
            }
            return pagePath;
        }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            var context = httpContext.GetSiteContext();

            if (httpContext.HasSiteContext() && context.ActivePage != null)
            {
                return null; // Page has already been specified, bypass this pre-router
            }

            string pagePath = httpContext.Request.AppRelativeCurrentExecutionFilePath;

            // Normalize path to the form: /Segment/Segment/Segment
            pagePath = NormalizePagePath(pagePath);

            // Search for the page
            TabInfo page = GetPage(pagePath, context.ActiveSite.PortalID);

            // If there is no matching page, return null
            if (page == null)
            {
                return null;
            }

            // Set the page in the context
            context.ActivePage = page;

            bool usePageRoute = false; //page.TabName == "About Us";

            //Set legacy PortalSettings
            //TODO PortalControllerAdapter usage is temporary in order to make method testable
            PortalControllerAdapter.Instance.CreatePortalSettings(page, context.ActiveSiteAlias, httpContext);

            // Remove the actual page path and set as the new app-relative path
            var tabPathLength = page.TabPath.Replace("//", "/").Length;
            string appRelativePath = (pagePath.Length >= tabPathLength) ?  pagePath.Substring(tabPathLength) : pagePath;

            // Rewrite and reroute the request
            // TODO: Can HttpContext.RewritePath do what we need?  I do want to preserve the old HttpContext for use after routing
            HttpContextBase rewrittenContext = new RewrittenHttpContext(httpContext, appRelativePath);
            return ReRouteRequest(r => r.GetRouteData(rewrittenContext), usePageRoute);
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            var context = requestContext.HttpContext.GetSiteContext();

            // Remove the "page" from the route data before rerouting
            TabInfo page = null;
            if (values.ContainsKey("page"))
            {
                // If page is present, but is null, then the user explicitly requested that no page routing be done
                page = values["page"] as TabInfo;
                if (page == null)
                {
                    return null;
                }
                values.Remove("page");
            }

            // If we didn't find a page to route the request to, select the active page
            if (page == null)
            {
                page = context.ActivePage;
            }

            // If we still don't have a page, ignore this route
            if (page == null)
            {
                return null;
            }

            bool usePageRoute = false; //page.TabName == "About Us";

            // Reroute the request
            VirtualPathData pathData;

            var controller = values["controller"];
            var moduleId = values["moduleId"];
            if (controller != null && controller.ToString() == "Page" && (moduleId == null || String.IsNullOrEmpty(moduleId.ToString())))
            {
                pathData = new VirtualPathData(this, String.Empty);
            }
            else
            {
                pathData = ReRouteRequest(r => r.GetVirtualPath(requestContext, values), usePageRoute);
            }
            if (pathData == null)
            {
                return null; // Rerouting failed
            }

            // Append the page path to the virtual path received and return the new path
            string newVirtualPath = String.Format(CultureInfo.InvariantCulture, "{0}/{1}", page.TabPath.Replace("//", "/"), pathData.VirtualPath).Trim('/');
            return new VirtualPathData(this, newVirtualPath);
        }
    }
}