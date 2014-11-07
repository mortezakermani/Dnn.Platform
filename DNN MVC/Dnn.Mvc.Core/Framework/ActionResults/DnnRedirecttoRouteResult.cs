#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2014
// by DNN Corporation
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
using System.Web.Mvc;
using System.Web.Routing;
using Dnn.Mvc.Framework.Modules;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;

namespace Dnn.Mvc.Framework.ActionResults
{
    internal class DnnRedirecttoRouteResult : RedirectToRouteResult
    {
        private RouteCollection _routes;

        public DnnRedirecttoRouteResult(string actionName, string controllerName, string routeName, RouteValueDictionary routeValues, bool permanent, MvcMode mvcMode)
            : base(routeName, routeValues, permanent)
        {
            ActionName = actionName;
            ControllerName = controllerName;
            MvcMode = mvcMode;
        }

        public string ActionName { get; private set; }

        public string ControllerName { get; private set; }

        public MvcMode MvcMode { get; private set; }

        internal RouteCollection Routes
        {
            get { return _routes ?? (_routes = RouteTable.Routes); }
            set { _routes = value; }
        }

        public override void ExecuteResult(ControllerContext context)
        {
            Requires.NotNull("context", context);

            Guard.Against(context.IsChildAction, "Cannot Redirect In Child Action");

            string url;
            if (MvcMode == MvcMode.Standard)
            {
                url = UrlHelper.GenerateUrl(RouteName, ActionName, ControllerName, RouteValues, Routes, context.RequestContext, false);

                Guard.Against(string.IsNullOrEmpty(url), "No Route Matches");

                url = url.Replace("//", "/");

                context.Controller.TempData.Keep();

                if (Permanent)
                {
                    context.HttpContext.Response.RedirectPermanent(url, false);
                }
                else
                {
                    context.HttpContext.Response.Redirect(url, false);
                }
            }
            else
            {
                //TODO - match other actions
                url = Globals.NavigateURL();

                if (Permanent)
                {
                    context.HttpContext.Response.RedirectPermanent(url, true);
                }
                else
                {
                    context.HttpContext.Response.Redirect(url, true);
                }
            }

        }
    }
}
