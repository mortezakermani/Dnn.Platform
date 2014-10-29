using System.Linq;
using System.Web.Mvc;
using Dnn.Mvc.Framework;
using Dnn.Mvc.Framework.Modules;
using Dnn.Mvc.Helpers;
using Dnn.Mvc.Tests.Utilities;
using Moq;
using NUnit.Framework;

namespace Dnn.Mvc.Web.Tests.Framework.Modules
{
    [TestFixture]
    public class ModuleDelegatingViewEngineTests
    {
        [Test]
        public void Should_Forward_FindPartialView_To_Current_ModuleApplication_ViewEngineCollection()
        {
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
            var result = new ViewEngineResult(new[] { "foo", "bar", "baz" });
            var context = MockHelper.CreateMockControllerContext();
            string viewName = "Foo";
            mockEngines.Setup(e => e.FindPartialView(context, viewName))
                       .Returns(result);

            SetupMockModuleApplication(context, mockEngines.Object);

            var viewEngine = new ModuleDelegatingViewEngine();

            // Act
            ViewEngineResult engineResult = viewEngine.FindPartialView(context, viewName, true);

            // Assert
            mockEngines.Verify(e => e.FindPartialView(context, viewName));
            Assert.AreEqual("foo", engineResult.SearchedLocations.ElementAt(0));
            Assert.AreEqual("bar", engineResult.SearchedLocations.ElementAt(1));
            Assert.AreEqual("baz", engineResult.SearchedLocations.ElementAt(2));
        }

        [Test]
        public void Should_Forward_FindView_To_Current_ModuleApplication_ViewEngineCollection()
        {
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
            var result = new ViewEngineResult(new[] { "foo", "bar", "baz" });
            ControllerContext context = MockHelper.CreateMockControllerContext();
            string viewName = "Foo";
            string masterName = "Bar";
            mockEngines.Setup(e => e.FindView(context, viewName, masterName))
                       .Returns(result);

            SetupMockModuleApplication(context, mockEngines.Object);

            var viewEngine = new ModuleDelegatingViewEngine();

            // Act
            var engineResult = viewEngine.FindView(context, viewName, masterName, true);

            // Assert
            mockEngines.Verify(e => e.FindView(context, viewName, masterName));
            Assert.AreEqual("foo", engineResult.SearchedLocations.ElementAt(0));
            Assert.AreEqual("bar", engineResult.SearchedLocations.ElementAt(1));
            Assert.AreEqual("baz", engineResult.SearchedLocations.ElementAt(2));
        }

        [Test]
        public void Should_Track_ViewEngine_View_Pairs_On_FindView_And_Releases_View_Appropriately()
        {
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
            var mockEngine = new Mock<IViewEngine>();
            var mockView = new Mock<IView>();
            var result = new ViewEngineResult(mockView.Object, mockEngine.Object);
            ControllerContext context = MockHelper.CreateMockControllerContext();
            string viewName = "Foo";
            string masterName = "Bar";
            mockEngines.Setup(e => e.FindView(context, viewName, masterName))
                       .Returns(result);

            SetupMockModuleApplication(context, mockEngines.Object);

            var viewEngine = new ModuleDelegatingViewEngine();

            // Act
            ViewEngineResult engineResult = viewEngine.FindView(context, viewName, masterName, true);
            viewEngine.ReleaseView(context, engineResult.View);

            // Assert
            mockEngine.Verify(e => e.ReleaseView(context, mockView.Object));
        }

        [Test]
        public void Should_Track_ViewEngine_View_Pairs_On_FindPartialView_And_Releases_View_Appropriately()
        {
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
            var mockEngine = new Mock<IViewEngine>();
            var mockView = new Mock<IView>();
            var result = new ViewEngineResult(mockView.Object, mockEngine.Object);
            ControllerContext context = MockHelper.CreateMockControllerContext();
            string viewName = "Foo";
            mockEngines.Setup(e => e.FindPartialView(context, viewName))
                       .Returns(result);

            SetupMockModuleApplication(context, mockEngines.Object);

            var viewEngine = new ModuleDelegatingViewEngine();

            // Act
            ViewEngineResult engineResult = viewEngine.FindPartialView(context, viewName, true);
            viewEngine.ReleaseView(context, engineResult.View);

            // Assert
            mockEngine.Verify(e => e.ReleaseView(context, mockView.Object));
        }

        [Test]
        public void Should_Return_Failed_ViewEngineResult_For_FindView_If_No_Current_Module_Application()
        {
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
            var viewEngine = new ModuleDelegatingViewEngine();
            ControllerContext context = MockHelper.CreateMockControllerContext();

            var siteContext = new SiteContext(context.HttpContext);

            context.HttpContext.SetSiteContext(siteContext);
            // Act
            var engineResult = viewEngine.FindView(context, "Foo", "Bar", true);

            // Assert
            Assert.IsNotNull(engineResult);
            Assert.IsNull(engineResult.View);
            Assert.AreEqual(0, engineResult.SearchedLocations.Count());
        }

        [Test]
        public void Should_Return_Failed_ViewEngineResult_For_FindPartialView_If_No_Current_Module_Application()
        {
            // Arrange
            var mockEngines = new Mock<ViewEngineCollection>();
            var viewEngine = new ModuleDelegatingViewEngine();
            ControllerContext context = MockHelper.CreateMockControllerContext();

            var siteContext = new SiteContext(context.HttpContext);

            context.HttpContext.SetSiteContext(siteContext);
            // Act
            var engineResult = viewEngine.FindPartialView(context, "Foo", true);

            // Assert
            Assert.IsNotNull(engineResult);
            Assert.IsNull(engineResult.View);
            Assert.AreEqual(0, engineResult.SearchedLocations.Count());
        }

        private static void SetupMockModuleApplication(ControllerContext context, ViewEngineCollection engines)
        {
            var mockApp = new Mock<ModuleApplication>();
            mockApp.Object.ViewEngines = engines;

            var siteContext = new SiteContext(context.HttpContext)
                                    {
                                        ActiveModuleRequest = new ModuleRequestResult
                                                                    {
                                                                        Application = mockApp.Object
                                                                    }
                                    };

            context.HttpContext.SetSiteContext(siteContext);
        }
    }

}
