using System;
using System.Web.Mvc;
using Dnn.Mvc.Framework;
using NUnit.Framework;

namespace Dnn.Mvc.Core.Tests.Framework
{
    [TestFixture]
    public class DnnRazorViewEngineTests
    {
        [Test]
        public void FindPartialView_Throws_Exception_If_Provided_ControlContext_IsNull()
        {
            const bool useCache = true;
            const string partialViewName = "ViewName";
            var viewEngine = new DnnRazorViewEngine();

            Assert.Throws<ArgumentNullException>(() => viewEngine.FindPartialView(null, partialViewName, useCache));
        }

        [Test]
        public void FindPartialView_Throws_Exception_If_Provided_ViewName_IsNull()
        {
            const bool useCache = true;
            const string partialViewName = null;
            var viewEngine = new DnnRazorViewEngine();

            Assert.Throws<ArgumentException>(() => viewEngine.FindPartialView(new ControllerContext(), partialViewName, useCache));
        }

        [Test]
        public void FindPartialView_Throws_Exception_If_Provided_ViewName_Is_EmptyString()
        {
            const bool useCache = true;
            string partialViewName = String.Empty;
            var viewEngine = new DnnRazorViewEngine();

            Assert.Throws<ArgumentException>(() => viewEngine.FindPartialView(new ControllerContext(), partialViewName, useCache));
        }

        [Test]
        public void FindView_Throws_Exception_If_Provided_ControlContext_IsNull()
        {
            const bool useCache = true;
            const string partialViewName = "ViewName";
            const string masterViewName = "MasterView";
            var viewEngine = new DnnRazorViewEngine();

            Assert.Throws<ArgumentNullException>(() => viewEngine.FindView(null, partialViewName, masterViewName, useCache));
        }

        [Test]
        public void FindView_Throws_Exception_If_Provided_ViewName_IsNull()
        {
            const bool useCache = true;
            const string partialViewName = null;
            const string masterViewName = "MasterView";
            var viewEngine = new DnnRazorViewEngine();

            Assert.Throws<ArgumentException>(() => viewEngine.FindView(new ControllerContext(), partialViewName, masterViewName, useCache));
        }

        [Test]
        public void FindView_Throws_Exception_If_Provided_ViewName_Is_EmptyString()
        {
            const bool useCache = true;
            string partialViewName = String.Empty;
            const string masterViewName = "MasterView";
            var viewEngine = new DnnRazorViewEngine();

            Assert.Throws<ArgumentException>(() => viewEngine.FindView(new ControllerContext(), partialViewName, masterViewName, useCache));
        }
    }
}
