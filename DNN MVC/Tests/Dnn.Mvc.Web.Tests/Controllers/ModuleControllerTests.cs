using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Dnn.Mvc.Framework;
using Dnn.Mvc.Framework.ActionResults;
using Dnn.Mvc.Framework.Modules;
using Dnn.Mvc.Helpers;
using Dnn.Mvc.Tests.Utilities;
using Dnn.Mvc.Web.Tests.Common;

using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;

using Moq;

using NUnit.Framework;

using ModuleController = Dnn.Mvc.Web.Controllers.ModuleController;

namespace Dnn.Mvc.Web.Tests.Controllers
{
    [TestFixture]
    public class ModuleControllerTests
    {
        [SetUp]
        public void SetUp()
        {
            ComponentFactory.Container = new SimpleContainer();
        }

        [TearDown]
        public void TearDown()
        {
            DotNetNuke.Entities.Modules.ModuleController.ClearInstance();
        }

        [Test]
        public void Render_Action_Renders_Default_View()
        {
            // Arrange
            ModuleController controller = SetupController();

            SetupMockModuleController(new ModuleInfo());

            // Act
            var result = controller.Render(null, String.Empty) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(String.IsNullOrEmpty(result.ViewName));
        }

        [Test]
        public void Render_Action_Returns_ResourceNotFoundResults_If_No_Module()
        {
            // Arrange
            ModuleController controller = SetupController();

            SetupMockModuleController(null);

            // Act
            var result = controller.Render(1, String.Empty);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ResourceNotFoundResult>(result);
        }

        [Test]
        public void Render_Action_Executes_Module_Executor_With_Module_Object_Representing_Current_Module()
        {
            // Arrange
            var executionEngine = new Mock<IModuleExecutionEngine>();
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(executionEngine.Object);

            ModuleController controller = SetupController();

            SetupMockModuleController(new ModuleInfo {ModuleID = Constants.MODULE_DefaultId});

            // Act
            var result = controller.Render(Constants.MODULE_DefaultId, String.Empty);

            // Assert
            executionEngine.Verify(e => e.ExecuteModule(It.IsAny<HttpContextBase>(), It.Is<ModuleInfo>(m => m.ModuleID == Constants.MODULE_DefaultId), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void Render_Action_Provides_Original_HttpContextBase_To_Module_Executor()
        {
            // Arrange
            var executionEngine = new Mock<IModuleExecutionEngine>();
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(executionEngine.Object);

            ModuleController controller = SetupController();
            var mockContext = Mock.Get(controller.HttpContext);

            SetupMockModuleController(new ModuleInfo { ModuleID = Constants.MODULE_DefaultId });

            // Act
            var result = controller.Render(Constants.MODULE_DefaultId, String.Empty);

            // Assert
            executionEngine.Verify(e => e.ExecuteModule(It.Is<HttpContextBase>(c => c == mockContext.Object), It.Is<ModuleInfo>(m => m.ModuleID == Constants.MODULE_DefaultId), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void Render_Action_Returns_Strongly_Typed_View()
        {
            // Arrange
            var executionEngine = new Mock<IModuleExecutionEngine>();
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(executionEngine.Object);
            executionEngine.Setup(e => e.ExecuteModule(It.IsAny<HttpContextBase>(), It.IsAny<ModuleInfo>(), It.IsAny<string>()))
                            .Returns(new ModuleRequestResult());

            ModuleController controller = SetupController();

            SetupMockModuleController(new ModuleInfo { ModuleID = Constants.MODULE_DefaultId });

            // Act
            var result = controller.Render(Constants.MODULE_DefaultId, String.Empty) as ViewResult;

            // Assert
            Assert.IsNotNull(result.Model);
            Assert.IsInstanceOf<ModuleRequestResult>(result.Model);
        }

        private ModuleController SetupController()
        {
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            context.SetSiteContext(new SiteContext(context));

            var controller = new ModuleController();
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);

            controller.SiteContext.ActiveSiteAlias = new PortalAliasInfo() { HTTPAlias = Constants.SITEALIAS_Default };
            controller.SiteContext.ActiveSite = new PortalInfo() { PortalID = Constants.SITE_DefaultId, HomeTabId = Constants.PAGE_HomePageId };
            controller.SiteContext.ActivePage = new TabInfo() { TabID = Constants.PAGE_DefaultId };

            return controller;
        }

        private void SetupMockModuleController(ModuleInfo module)
        {
            var mockModuleController = new Mock<IModuleController>();
            mockModuleController.Setup(c => c.GetModule(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                                .Returns(module);
            DotNetNuke.Entities.Modules.ModuleController.SetTestableInstance(mockModuleController.Object);
        }

    }
}
