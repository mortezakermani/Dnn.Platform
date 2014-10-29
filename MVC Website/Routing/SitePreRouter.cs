using System;
using System.Text;
using System.Web;
using System.Web.Routing;
using Dnn.Mvc.Framework;
using Dnn.Mvc.Helpers;
using Dnn.Mvc.Routing;
using Dnn.Mvc.Utils.Entities.Users;
using Dnn.Mvc.Web.Framework;
using Dnn.Mvc.Web.Helpers;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;

namespace Dnn.Mvc.Web.Routing
{
    public class SitePreRouter : PreRouterBase
    {
        private string ExtractPortalRelativeUrl(Uri requestUrl, PortalAliasInfo alias)
        {
            var urlBuilder = new StringBuilder();
            urlBuilder.Append(GetHostString(requestUrl));
            urlBuilder.Append(requestUrl.AbsolutePath);
            if (!requestUrl.AbsolutePath.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                urlBuilder.Append("/");
            }
            return urlBuilder.ToString().Substring(alias.HTTPAlias.Length);
        }

        private static string GetHostString(Uri requestUrl)
        {
            var hostBuilder = new StringBuilder();
            hostBuilder.Append(requestUrl.Host);
            if (requestUrl.Port != 80)
            {
                hostBuilder.Append(":");
                hostBuilder.Append(requestUrl.Port);
            }
            return hostBuilder.ToString();
        }

        private PortalAliasInfo GetSiteAlias(Uri url)
        {
            PortalAliasInfo alias = null;

            for(int seg = url.Segments.Length - 1; seg > -1; seg--)
            {
                var urlBuilder = new StringBuilder();

                // www.domain.com:port
                urlBuilder.Append(url.Host);
                if (url.Port != 80)
                {
                    urlBuilder.Append(":");
                    urlBuilder.Append(url.Port);
                }
                for (int seg1 = 0; seg1 <= seg; seg1++)
                {
                    urlBuilder.Append(url.Segments[seg1]);
                }

                var httpAlias = urlBuilder.ToString().TrimEnd('/');

                var aliases = PortalAliasController.Instance.GetPortalAliases();
                if (aliases.Contains(httpAlias.ToLowerInvariant()))
                {
                    alias = aliases[httpAlias.ToLowerInvariant()];
                }
                if (alias != null)
                {
                    break;
                }
            }

            return alias;
        }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            if (httpContext.HasSiteContext() && httpContext.GetSiteContext().ActiveSite != null)
            {
                return null; // Site has already been specified, bypass this pre-router
            }

            Uri requestUrl = httpContext.Request.Url;

            PortalAliasInfo alias = GetSiteAlias(requestUrl);

            if (alias == null)
            {
                return null;
            }

            PortalInfo site = PortalController.Instance.GetPortal(alias.PortalID);

            if (site == null)
            {
                return null;
            }

            var siteContext = new SiteContext(httpContext);
            httpContext.SetSiteContext(siteContext);
            siteContext.ActiveSite = site;
            siteContext.ActiveSiteAlias = alias;

            SetUpUser(siteContext);

            // Determine the Portal-relative url
            string portalRelativeUrl = ExtractPortalRelativeUrl(requestUrl, alias);

            // Rewrite and re-route the request
            // TODO: Can HttpContext.RewritePath do what we need?  I do want to preserve the old HttpContext for use after routing
            HttpContextBase rewrittenContext = new RewrittenHttpContext(httpContext, portalRelativeUrl);
            return ReRouteRequest(route => route.GetRouteData(rewrittenContext), false);
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            SiteContext siteContext = requestContext.HttpContext.GetSiteContext();
            if (siteContext.ActiveSiteAlias == null)
            {
                return null;
            }

            // Use the other routes to get the base path
            VirtualPathData pathData = ReRouteRequest(route => route.GetVirtualPath(requestContext, values), false);
            if (pathData == null)
            {
                return null;
            }

            // Remove the host string from the portal prefix
            string httpAlias = siteContext.ActiveSiteAlias.HTTPAlias;
            string host = GetHostString(requestContext.HttpContext.Request.Url);
            if (!httpAlias.StartsWith(host, StringComparison.OrdinalIgnoreCase))
            {
                // TODO: Should throw exception?
                return null; // Cannot generate link, active portal prefix does not match request url
            }
            httpAlias = httpAlias.Substring(host.Length);

            if (!String.IsNullOrEmpty(httpAlias))
            {
                // Remove the application path from the portal prefix
                string applicationPath = requestContext.HttpContext.Request.ApplicationPath;
                if (!httpAlias.StartsWith(applicationPath, StringComparison.OrdinalIgnoreCase))
                {
                    // TODO: Should throw exception?
                    return null; // Cannot generate link, active portal prefix does not match request url
                }
                httpAlias = httpAlias.Substring(applicationPath.Length);

                // Verify the format of the prefix
                if (httpAlias.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    httpAlias = httpAlias.Substring(1);
                }
            }
            else
            {
                httpAlias = "/";
            }
            if (!httpAlias.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                httpAlias += "/";
            }

            // Append the prefix and return the path data
            return new VirtualPathData(this, httpAlias + pathData.VirtualPath);
        }

        private void SetUpUser(SiteContext siteContext)
        {
            if (siteContext.HttpContext.Request.IsAuthenticated && siteContext.ActiveSite != null)
            {
                //TODO UserControllerAdapter usage is temporary in order to make method testable
                var user = UserControllerAdapter.Instance.GetCachedUser(siteContext.ActiveSite.PortalID, siteContext.HttpContext.User.Identity.Name);

                //save userinfo object in Context
                siteContext.SetUser(user);

                //Localization.SetLanguage also updates the user profile, so this needs to go after the profile is loaded
                Localization.SetLanguage(user.Profile.PreferredLocale);
            }
        }
    }
}