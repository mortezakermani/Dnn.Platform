using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using Dnn.Mvc.Framework;
using Dnn.Mvc.Framework.Modules;
using Dnn.Mvc.Web.Routing;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Framework.Reflections;

namespace Dnn.Mvc.Web.Framework
{
    public class DnnMvcApplication : HttpApplication
    {
        private static RouteCollection _routes;

        public static RouteCollection Routes
        {
            get { return _routes ?? (_routes = RouteTable.Routes); }

            // We really don't want people playing with this outside of the test
            internal set { _routes = value; }
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            RegisterRoutes(Routes);

            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var name = Config.GetSetting("ServerName");
            Globals.ServerName = String.IsNullOrEmpty(name) ? Dns.GetHostName() : name;

            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(new ModuleExecutionEngine());
            ProviderConfig.RegisterProviders();
            RegisterModules();

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new ModuleDelegatingViewEngine());
            ViewEngines.Engines.Add(new RazorViewEngine());

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                if (authTicket != null)
                {
                    var identity = new GenericIdentity(authTicket.Name, "Forms");
                    var principal = new GenericPrincipal(identity, null);

                    Context.User = principal;
                }
            }
        }

        public static void RegisterModules()
        {
            var moduleApplications = GetModules().ToDictionary(module => module.ModuleName);

            ComponentFactory.RegisterComponentInstance<IDictionary<string, ModuleApplication>>(moduleApplications);
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.Add("SiteRouting", new SitePreRouter());

            routes.Add("PageRouting", new PagePreRouter());

            routes.MapRoute("ModuleRoute",
                            "{moduleId}/{*moduleRoute}",
                            new
                            {
                                controller = "Page",
                                action = "Index", 
                                moduleId = String.Empty, 
                                moduleRoute = String.Empty
                            },
                            new
                            {
                                moduleId = @"[0-9]*"
                            });

            routes.MapRoute("ModuleRenderRoute",
                            "_Module/{moduleId}/{*moduleRoute}",
                            new
                            {
                                controller = "Module",
                                action = "Render",
                                moduleId = String.Empty,
                                moduleRoute = String.Empty
                            },
                            new
                            {
                                moduleId = @"[0-9]*"
                            });

            routes.MapRoute("DefaultMvcRoute", 
                            "{controller}/{action}/{id}", 
                            new 
                            { 
                                controller = "Page", 
                                action = "Index", 
                                id = UrlParameter.Optional 
                            });

        }

        private static IEnumerable<ModuleApplication> GetModules()
        {
            var typeLocator = new TypeLocator();
            IEnumerable<Type> types = typeLocator.GetAllMatchingTypes(t => t != null 
                                                                                && t.IsClass 
                                                                                && !t.IsAbstract 
                                                                                && t.IsVisible 
                                                                                && typeof(ModuleApplication).IsAssignableFrom(t));

            foreach (var moduleType in types)
            {
                ModuleApplication module;
                try
                {
                    module = Activator.CreateInstance(moduleType) as ModuleApplication;
                }
                catch (Exception)
                {
                    //Logger.ErrorFormat("Unable to create {0} while registering module injection filters.  {1}", filterType.FullName, e.Message);

                    module = null;
                }

                if (module != null)
                {
                    yield return module;
                }
            }
        }
    }
}