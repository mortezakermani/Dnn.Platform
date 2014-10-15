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
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Dnn.Mvc.Framework;
using Dnn.Mvc.Framework.Modules;
using DotNetNuke.ComponentModel;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Web.Mvc
{
    public class MvcHostControl : ModuleControlBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            HttpContextBase httpContext = new HttpContextWrapper(HttpContext.Current);

            var moduleExecutionEngine = ComponentFactory.GetComponent<IModuleExecutionEngine>();

            const string moduleRoute = ""; //for now - just the default route

            ModuleRequestResult result = moduleExecutionEngine.ExecuteModule(httpContext, ModuleContext.Configuration, moduleRoute);

            Controls.Add(new LiteralControl(RenderModule(result, httpContext).ToString()));
        }

        public static MvcHtmlString RenderModule(ModuleRequestResult moduleResult, HttpContextBase httpContext)
        {
            MvcHtmlString moduleOutput;

            using (var writer = new StringWriter(CultureInfo.CurrentCulture))
            {
                var moduleExecutionEngine = ComponentFactory.GetComponent<IModuleExecutionEngine>();

                moduleExecutionEngine.ExecuteModuleResult(new SiteContext(httpContext), moduleResult, writer);

                moduleOutput = MvcHtmlString.Create(writer.ToString());
            }

            return moduleOutput;
        }
    }
}
