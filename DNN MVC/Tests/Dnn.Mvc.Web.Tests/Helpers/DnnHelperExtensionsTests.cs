using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using Dnn.Mvc.Framework;
using Dnn.Mvc.Framework.Modules;
using Dnn.Mvc.Helpers;
using Dnn.Mvc.Tests.Utilities;
using Dnn.Mvc.Web.Helpers;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Modules;
using Moq;
using NUnit.Framework;

namespace Dnn.Mvc.Web.Tests.Helpers
{
    [TestFixture]
    public class DnnHelperExtensionsTests
    {
        [SetUp]
        public void SetUp()
        {
            ComponentFactory.Container = new SimpleContainer();    
        }

        [Test]
        public void RenderModule_Calls_ModuleExecutionEngine_ExecuteModuleResult()
        {
            //Arrange
            var viewContext = MockHelper.CreateViewContext("http://localhost/Foo/Bar");
            var httpContext = viewContext.HttpContext;
            httpContext.SetSiteContext(new SiteContext(httpContext));

            var moduleResult = new ModuleRequestResult() { Module = new ModuleInfo { ModuleID = 1 } };

            var viewData = new Mock<IViewDataContainer>();
            viewData.Setup(vd => vd.ViewData).Returns(new ViewDataDictionary(moduleResult));
            var routes = new RouteCollection();

            var mockEngine = new Mock<IModuleExecutionEngine>();
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(mockEngine.Object);

            var helper = new DnnHelper<ModuleRequestResult>(viewContext, viewData.Object, routes);

            //Action
            helper.RenderModule();

            //Assert
            mockEngine.Verify(me => me.ExecuteModuleResult(It.IsAny<SiteContext>(), It.IsAny<ModuleRequestResult>(), It.IsAny<TextWriter>()) );
        }

        [Test]
        public void RenderModule_Calls_ModuleExecutionEngine_ExecuteModuleResult_With_The_Same_ModuleRequestResult()
        {
            //Arrange
            var viewContext = MockHelper.CreateViewContext("http://localhost/Foo/Bar");
            var httpContext = viewContext.HttpContext;
            httpContext.SetSiteContext(new SiteContext(httpContext));

            var moduleResult = new ModuleRequestResult() { Module = new ModuleInfo { ModuleID = 1 } };

            var viewData = new Mock<IViewDataContainer>();
            viewData.Setup(vd => vd.ViewData).Returns(new ViewDataDictionary(moduleResult));
            var routes = new RouteCollection();

            var mockEngine = new Mock<IModuleExecutionEngine>();
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(mockEngine.Object);

            var helper = new DnnHelper<ModuleRequestResult>(viewContext, viewData.Object, routes);

            //Action
            helper.RenderModule();

            //Assert
            mockEngine.Verify(me => me.ExecuteModuleResult(It.IsAny<SiteContext>(), moduleResult, It.IsAny<TextWriter>()));
        }

        [Test]
        public void RenderModule_Does_Not_Use_ViewContext_TextWriter_But_Creates_Its_Own()
        {
            //Arrange
            var viewContext = MockHelper.CreateViewContext("http://localhost/Foo/Bar");
            var httpContext = viewContext.HttpContext;
            httpContext.SetSiteContext(new SiteContext(httpContext));
            var textWriter = viewContext.Writer;

            var moduleResult = new ModuleRequestResult() { Module = new ModuleInfo { ModuleID = 1 } };

            var viewData = new Mock<IViewDataContainer>();
            viewData.Setup(vd => vd.ViewData).Returns(new ViewDataDictionary(moduleResult));
            var routes = new RouteCollection();

            var mockEngine = new Mock<IModuleExecutionEngine>();
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(mockEngine.Object);

            var helper = new DnnHelper<ModuleRequestResult>(viewContext, viewData.Object, routes);

            //Action
            helper.RenderModule();

            //Assert
            mockEngine.Verify(me => me.ExecuteModuleResult(It.IsAny<SiteContext>(), moduleResult, textWriter), Times.Never);
        }

        [Test]
        public void RenderModule_Returns_MvcHtmlString()
        {
            //Arrange
            var viewContext = MockHelper.CreateViewContext("http://localhost/Foo/Bar");
            var httpContext = viewContext.HttpContext;
            httpContext.SetSiteContext(new SiteContext(httpContext));

            var moduleResult = new ModuleRequestResult() { Module = new ModuleInfo { ModuleID = 1 } };

            var viewData = new Mock<IViewDataContainer>();
            viewData.Setup(vd => vd.ViewData).Returns(new ViewDataDictionary(moduleResult));
            var routes = new RouteCollection();

            var mockEngine = new Mock<IModuleExecutionEngine>();
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(mockEngine.Object);

            var helper = new DnnHelper<ModuleRequestResult>(viewContext, viewData.Object, routes);

            //Action
            var content = helper.RenderModule();

            //Assert
            Assert.IsInstanceOf<MvcHtmlString>(content);
            Assert.True(content.ToHtmlString().Contains("<!-- Start Module#1Body -->"));
            Assert.True(content.ToHtmlString().Contains("<!-- End Module#1Body -->"));
        }
    }
}
