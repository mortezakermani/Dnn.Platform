using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Dnn.Mvc.Framework;
using Dnn.Mvc.Framework.ActionResults;
using Dnn.Mvc.Framework.Modules;
using Dnn.Mvc.Helpers;
using Dnn.Mvc.Tests.Utilities;
using Dnn.Mvc.Web.Controllers;
using Dnn.Mvc.Web.Models;
using Dnn.Mvc.Web.Tests.Common;

using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security.Permissions;

using Moq;

using NUnit.Framework;
using ModuleController = DotNetNuke.Entities.Modules.ModuleController;

namespace Dnn.Mvc.Web.Tests.Controllers
{
    [TestFixture]
    public class PageControllerTests
    {
        private readonly Mock<PermissionProvider> _mockPermissionProvider = new Mock<PermissionProvider>();

        [SetUp]
        public void SetUp()
        {
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<PermissionProvider>(_mockPermissionProvider.Object);
        }

        [TearDown]
        public void TearDown()
        {
            TabController.ClearInstance();
            ModuleController.ClearInstance();
        }

        [Test]
        public void View_Action_Renders_Default_View()
        {
            // Arrange
            PageController controller = SetupController();
            _mockPermissionProvider.Setup(p => p.CanViewPage(It.IsAny<TabInfo>())).Returns(true);

            SetupMockTabController();

            SetupMockModuleController(new Dictionary<int, ModuleInfo>());

            // Act
            var result = controller.Index(null, String.Empty) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(String.IsNullOrEmpty(result.ViewName));
        }

        [Test]
        public void View_Action_Renders_AccessDenied_View_If_No_Page_Permission()
        {
            // Arrange
            PageController controller = SetupController();
            _mockPermissionProvider.Setup(p => p.CanViewPage(It.IsAny<TabInfo>())).Returns(false);

            SetupMockTabController();

            SetupMockModuleController(new Dictionary<int, ModuleInfo>());

            // Act
            var result = controller.Index(null, String.Empty) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("AccessDenied", result.ViewName);
            Assert.AreEqual("No Permission to View Page", result.ViewBag.Message);
        }

        [Test]
        public void View_Action_Renders_AccessDenied_View_If_Page_Not_Active_Yet()
        {
            // Arrange
            var page = new TabInfo() {TabID = Constants.PAGE_DefaultId, StartDate = DateTime.Now.AddDays(30)};
            PageController controller = SetupController(page);

            _mockPermissionProvider.Setup(p => p.CanViewPage(It.IsAny<TabInfo>())).Returns(true);

            SetupMockTabController();

            SetupMockModuleController(new Dictionary<int, ModuleInfo>());

            // Act
            var result = controller.Index(null, String.Empty) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("AccessDenied", result.ViewName);
            Assert.AreEqual("Page not active yet", result.ViewBag.Message);
        }

        [Test]
        public void View_Action_Renders_AccessDenied_View_If_Page_Expired()
        {
            // Arrange
            var page = new TabInfo() { TabID = Constants.PAGE_DefaultId, EndDate = DateTime.Now.AddDays(-30) };
            PageController controller = SetupController(page);

            _mockPermissionProvider.Setup(p => p.CanViewPage(It.IsAny<TabInfo>())).Returns(true);

            SetupMockTabController();

            SetupMockModuleController(new Dictionary<int, ModuleInfo>());

            // Act
            var result = controller.Index(null, String.Empty) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("AccessDenied", result.ViewName);
            Assert.AreEqual("Page has expired", result.ViewBag.Message);
        }

        [Test]
        public void View_Action_Returns_Strongly_Typed_View()
        {
            // Arrange
            PageController controller = SetupController();
            _mockPermissionProvider.Setup(p => p.CanViewPage(It.IsAny<TabInfo>())).Returns(true);

            SetupMockTabController();

            SetupMockModuleController(new Dictionary<int, ModuleInfo>());

            // Act
            var result = controller.Index(null, String.Empty) as ViewResult;

            // Assert
            Assert.IsNotNull(result.Model);
            Assert.IsInstanceOf<PageViewModel>(result.Model);
        }

        [Test]
        public void View_Action_Sets_ViewModel_Page_Property()
        {
            // Arrange
            PageController controller = SetupController();
            _mockPermissionProvider.Setup(p => p.CanViewPage(It.IsAny<TabInfo>())).Returns(true);

            SetupMockTabController();

            SetupMockModuleController(new Dictionary<int, ModuleInfo>());

            // Act
            var result = controller.Index(null, String.Empty) as ViewResult;

            // Assert
            Assert.IsNotNull(result.Model);
            var pageViewModel = result.Model as PageViewModel;
            Assert.IsNotNull(pageViewModel);
            Assert.AreEqual(Constants.PAGE_DefaultId, pageViewModel.Page.TabID);
        }

        [Test]
        public void View_Action_Sets_ViewModel_Site_Property()
        {
            // Arrange
            PageController controller = SetupController();
            _mockPermissionProvider.Setup(p => p.CanViewPage(It.IsAny<TabInfo>())).Returns(true);

            SetupMockTabController();

            SetupMockModuleController(new Dictionary<int, ModuleInfo>());

            // Act
            var result = controller.Index(null, String.Empty) as ViewResult;

            // Assert
            Assert.IsNotNull(result.Model);
            var pageViewModel = result.Model as PageViewModel;
            Assert.IsNotNull(pageViewModel);
            Assert.AreEqual(Constants.SITE_DefaultId, pageViewModel.Site.PortalID);
        }

        [Test]
        public void View_Action_Sets_ViewModel_SiteAlias_Property()
        {
            // Arrange
            PageController controller = SetupController();
            _mockPermissionProvider.Setup(p => p.CanViewPage(It.IsAny<TabInfo>())).Returns(true);

            SetupMockTabController();

            SetupMockModuleController(new Dictionary<int, ModuleInfo>());

            // Act
            var result = controller.Index(null, String.Empty) as ViewResult;

            // Assert
            Assert.IsNotNull(result.Model);
            var pageViewModel = result.Model as PageViewModel;
            Assert.IsNotNull(pageViewModel);
            Assert.AreEqual(Constants.SITEALIAS_Default, pageViewModel.SiteAlias.HTTPAlias);
        }

        [Test]
        public void View_Action_Sets_ViewModel_Pages_Property()
        {
            // Arrange
            PageController controller = SetupController();
            _mockPermissionProvider.Setup(p => p.CanViewPage(It.IsAny<TabInfo>())).Returns(true);

            SetupMockTabController();

            SetupMockModuleController(new Dictionary<int, ModuleInfo>());

            // Act
            var result = controller.Index(null, String.Empty) as ViewResult;

            // Assert
            Assert.IsNotNull(result.Model);
            var pageViewModel = result.Model as PageViewModel;
            Assert.IsNotNull(pageViewModel);
            Assert.AreEqual(2, pageViewModel.Pages.Count);
            Assert.AreEqual(Constants.PAGE_DefaultId, pageViewModel.Pages[0].TabID);
            Assert.AreEqual(Constants.PAGE_HomePageId, pageViewModel.Pages[1].TabID);
        }

        [Test]
        public void View_Action_Sets_ViewModel_HomePage_Property()
        {
            // Arrange
            PageController controller = SetupController();
            _mockPermissionProvider.Setup(p => p.CanViewPage(It.IsAny<TabInfo>())).Returns(true);

            SetupMockTabController();

            SetupMockModuleController(new Dictionary<int, ModuleInfo>());

            // Act
            var result = controller.Index(null, String.Empty) as ViewResult;

            // Assert
            Assert.IsNotNull(result.Model);
            var pageViewModel = result.Model as PageViewModel;
            Assert.IsNotNull(pageViewModel);
            Assert.AreEqual(Constants.PAGE_HomePageId, pageViewModel.HomePage.TabID);
        }

        [Test]
        public void View_Action_Executes_No_Modules_On_Page_With_No_Modules()
        {
            // Arrange
            var executionEngine = new Mock<IModuleExecutionEngine>();
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(executionEngine.Object);

            _mockPermissionProvider.Setup(p => p.CanViewPage(It.IsAny<TabInfo>())).Returns(true);

            PageController controller = SetupController();

            SetupMockTabController();

            SetupMockModuleController(new Dictionary<int, ModuleInfo>());

            // Act
            var result = controller.Index(null, String.Empty) as ViewResult;

            // Assert
            executionEngine.Verify(e => e.ExecuteModule(It.IsAny<HttpContextBase>(), It.IsAny<ModuleInfo>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void View_Action_Executes_Each_Module_On_Page()
        {
            // Arrange
            var executionEngine = new Mock<IModuleExecutionEngine>();
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(executionEngine.Object);

            _mockPermissionProvider.Setup(p => p.CanViewPage(It.IsAny<TabInfo>())).Returns(true);
            _mockPermissionProvider.Setup(p => p.CanViewModule(It.IsAny<ModuleInfo>())).Returns(true);
           
            PageController controller = SetupController();

            SetupMockTabController();

            var tabModules = new Dictionary<int, ModuleInfo>
                                    {
                                        {1, new ModuleInfo() {ModuleID = 1}},
                                        {2, new ModuleInfo() {ModuleID = 2}},
                                        {3, new ModuleInfo() {ModuleID = 3}}
                                    };
            SetupMockModuleController(tabModules);

            // Act
            var result = controller.Index(null, String.Empty) as ViewResult;

            // Assert
            executionEngine.Verify(e => e.ExecuteModule(It.IsAny<HttpContextBase>(), It.IsAny<ModuleInfo>(), It.IsAny<string>()), Times.Exactly(3));
        }

        [Test]
        public void View_Action_Executes_No_Modules_On_Page_If_No_View_Permission()
        {
            // Arrange
            var executionEngine = new Mock<IModuleExecutionEngine>();
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(executionEngine.Object);

            _mockPermissionProvider.Setup(p => p.CanViewPage(It.IsAny<TabInfo>())).Returns(true);
            _mockPermissionProvider.Setup(p => p.CanViewModule(It.IsAny<ModuleInfo>())).Returns(false);

            PageController controller = SetupController();

            SetupMockTabController();

            var tabModules = new Dictionary<int, ModuleInfo>
                                    {
                                        {1, new ModuleInfo() {ModuleID = 1}},
                                        {2, new ModuleInfo() {ModuleID = 2}},
                                        {3, new ModuleInfo() {ModuleID = 3}}
                                    };
            SetupMockModuleController(tabModules);

            // Act
            var result = controller.Index(null, String.Empty) as ViewResult;

            // Assert
            executionEngine.Verify(e => e.ExecuteModule(It.IsAny<HttpContextBase>(), It.IsAny<ModuleInfo>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void View_Action_Only_Executes_Modules_On_Page_That_Are_Not_Deleted()
        {
            // Arrange
            var executionEngine = new Mock<IModuleExecutionEngine>();
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(executionEngine.Object);

            _mockPermissionProvider.Setup(p => p.CanViewPage(It.IsAny<TabInfo>())).Returns(true);
            _mockPermissionProvider.Setup(p => p.CanViewModule(It.IsAny<ModuleInfo>())).Returns(true);

            PageController controller = SetupController();

            SetupMockTabController();

            var tabModules = new Dictionary<int, ModuleInfo>
                                    {
                                        {1, new ModuleInfo() {ModuleID = 1, IsDeleted = true}},
                                        {2, new ModuleInfo() {ModuleID = 2}},
                                        {3, new ModuleInfo() {ModuleID = 3}}
                                    };
            SetupMockModuleController(tabModules);

            // Act
            var result = controller.Index(null, String.Empty) as ViewResult;

            // Assert
            executionEngine.Verify(e => e.ExecuteModule(It.IsAny<HttpContextBase>(), It.IsAny<ModuleInfo>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Test]
        public void View_Action_Only_Executes_Modules_On_Page_That_Are_Active_And_Not_Expired()
        {
            // Arrange
            var executionEngine = new Mock<IModuleExecutionEngine>();
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(executionEngine.Object);

            _mockPermissionProvider.Setup(p => p.CanViewPage(It.IsAny<TabInfo>())).Returns(true);
            _mockPermissionProvider.Setup(p => p.CanViewModule(It.IsAny<ModuleInfo>())).Returns(true);

            PageController controller = SetupController();

            SetupMockTabController();

            var tabModules = new Dictionary<int, ModuleInfo>
                                    {
                                        {1, new ModuleInfo() {ModuleID = 1, StartDate = DateTime.Now.AddDays(30)}},
                                        {2, new ModuleInfo() {ModuleID = 2}},
                                        {3, new ModuleInfo() {ModuleID = 3, EndDate = DateTime.Now.AddDays(-30)}}
                                    };
            SetupMockModuleController(tabModules);

            // Act
            var result = controller.Index(null, String.Empty) as ViewResult;

            // Assert
            executionEngine.Verify(e => e.ExecuteModule(It.IsAny<HttpContextBase>(), It.IsAny<ModuleInfo>(), It.IsAny<string>()), Times.Exactly(1));
        }

        [Test]
        public void View_Action_Returns_PageViewModel_Containing_All_Module_Results()
        {
            // Arrange
            var executionEngine = new Mock<IModuleExecutionEngine>();
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(executionEngine.Object);

            _mockPermissionProvider.Setup(p => p.CanViewPage(It.IsAny<TabInfo>())).Returns(true);
            _mockPermissionProvider.Setup(p => p.CanViewModule(It.IsAny<ModuleInfo>())).Returns(true);

            PageController controller = SetupController();

            SetupMockTabController();

            ActionResult module1Result = new Mock<ActionResult>().Object;
            ActionResult module2Result = new Mock<ActionResult>().Object;

            ControllerContext module1Context = MockHelper.CreateMockControllerContext(controller);
            ControllerContext module2Context = MockHelper.CreateMockControllerContext(controller);

            executionEngine.Setup(e => e.ExecuteModule(It.IsAny<HttpContextBase>(), It.Is<ModuleInfo>(m => m.DesktopModuleID == 1), It.IsAny<string>()))
                           .Returns(new ModuleRequestResult
                                            {
                                                ActionResult = module1Result,
                                                ControllerContext = module1Context
                                            });

            executionEngine.Setup(e => e.ExecuteModule(It.IsAny<HttpContextBase>(), It.Is<ModuleInfo>(m => m.DesktopModuleID == 2), It.IsAny<string>()))
                           .Returns(new ModuleRequestResult
                                           {
                                               ActionResult = module2Result,
                                               ControllerContext = module2Context
                                           });

            var tabModules = new Dictionary<int, ModuleInfo>
                                    {
                                        {1, new ModuleInfo() {ModuleID = 1, DesktopModuleID = 1, PaneName="LeftPane"}},
                                        {2, new ModuleInfo() {ModuleID = 2, DesktopModuleID = 2, PaneName="ContentPane"}},
                                        {3, new ModuleInfo() {ModuleID = 3, DesktopModuleID = 1, PaneName="LeftPane"}}
                                    };
            SetupMockModuleController(tabModules);

            // Act
            var result = controller.Index(null, String.Empty) as ViewResult;

            // Assert
            Assert.IsNotNull(result);

            var pageModel = result.Model as PageViewModel;
            Assert.IsNotNull(pageModel);
            Assert.AreEqual(2, pageModel.Panes.Count);

            Assert.AreSame(module1Result, pageModel.Panes["LeftPane"].ModuleResults[0].ActionResult);
            Assert.AreSame(module1Context, pageModel.Panes["LeftPane"].ModuleResults[0].ControllerContext);

            Assert.AreSame(module1Result, pageModel.Panes["LeftPane"].ModuleResults[1].ActionResult);
            Assert.AreSame(module1Context, pageModel.Panes["LeftPane"].ModuleResults[1].ControllerContext);

            // NOTE: (3rd module also uses the "first" module application)
            Assert.AreSame(module2Result, pageModel.Panes["ContentPane"].ModuleResults[0].ActionResult);
            Assert.AreSame(module2Context, pageModel.Panes["ContentPane"].ModuleResults[0].ControllerContext);
        }

        [Test]
        public void View_Action_Provides_Module_Executor_With_Module_Object_Representing_Current_Module()
        {
            RunSimpleModuleExecutionTest((ctl, ctx, mod, route) => Assert.AreEqual(1, mod.ModuleID));
        }

        [Test]
        public void View_Action_Provides_Original_HttpContextBase_To_Module_Executor()
        {
            RunSimpleModuleExecutionTest((ctl, ctx, mod, route) => Assert.AreSame(ctl.HttpContext, ctx));
        }

        [Test]
        public void View_Action_Returns_RenderModuleResult_From_SelectedModule_If_It_Is_PageOverrideResult()
        {
            // Arrange
            var executionEngine = new Mock<IModuleExecutionEngine>();
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(executionEngine.Object);

            _mockPermissionProvider.Setup(p => p.CanViewPage(It.IsAny<TabInfo>())).Returns(true);
            _mockPermissionProvider.Setup(p => p.CanViewModule(It.IsAny<ModuleInfo>())).Returns(true);

            PageController controller = SetupController();

            SetupMockTabController();

            var tabModules = new Dictionary<int, ModuleInfo>
                                    {
                                        {1, new ModuleInfo() {ModuleID = 1, DesktopModuleID = 1, PaneName="LeftPane"}},
                                        {2, new ModuleInfo() {ModuleID = 2, DesktopModuleID = 2, PaneName="ContentPane"}},
                                        {3, new ModuleInfo() {ModuleID = 3, DesktopModuleID = 1, PaneName="LeftPane"}}
                                    };

            SetupMockModuleController(tabModules);

            executionEngine.Setup(e => e.ExecuteModule(It.IsAny<HttpContextBase>(), It.Is<ModuleInfo>(m => m.DesktopModuleID == 1), It.IsAny<string>()))
                           .Returns(new ModuleRequestResult { ActionResult = new PageOverrideResult(new ViewResult())});

            // Act
            ActionResult result = controller.Index(1, String.Empty);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<RenderModuleResult>(result);
        }

        [Test]
        public void View_Action_Provides_Empty_ModuleRoute_If_ModuleId_Does_Not_Match_ModuleId_Parameter()
        {
            _mockPermissionProvider.Setup(p => p.CanViewPage(It.IsAny<TabInfo>())).Returns(true);
            _mockPermissionProvider.Setup(p => p.CanViewModule(It.IsAny<ModuleInfo>())).Returns(true);

            RunSimpleModuleExecutionTest((ctl, ctx, mod, route) => Assert.AreEqual(String.Empty, route, "Expected that the routing url provided would be empty"));
        }

        [Test]
        public void View_Action_Provides_ModuleRoute_From_Parameter_If_ModuleId_Matches_Parameter()
        {
            _mockPermissionProvider.Setup(p => p.CanViewPage(It.IsAny<TabInfo>())).Returns(true);
            _mockPermissionProvider.Setup(p => p.CanViewModule(It.IsAny<ModuleInfo>())).Returns(true);

            RunSimpleModuleExecutionTest(1, "Zoop/Zork/Zoink", (ctl, ctx, mod, route) => Assert.AreEqual("Zoop/Zork/Zoink", route));
        }

        private  void RunSimpleModuleExecutionTest(Action<PageController, HttpContextBase, ModuleInfo, string> assert)
        {
            //Arrange
            var executionEngine = new Mock<IModuleExecutionEngine>();
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(executionEngine.Object);

            PageController controller = SetupController();

            SetupMockTabController();

            var tabModules = new Dictionary<int, ModuleInfo>
                                    {
                                        {1, new ModuleInfo() {ModuleID = 1, DesktopModuleID = 1, PaneName="LeftPane"}},
                                    };
            SetupMockModuleController(tabModules);

            bool callbackHit = false;
            executionEngine.Setup(e => e.ExecuteModule(It.IsAny<HttpContextBase>(), It.Is<ModuleInfo>(m => m.DesktopModuleID == 1), It.IsAny<string>()))
                            .Callback<HttpContextBase, ModuleInfo, string>((c, m, s) =>
                            {
                                assert(controller, c, m, s);
                                callbackHit = true;
                            });

            // Act
            var result = controller.Index(null, String.Empty) as ViewResult;

            // Assert
            Assert.IsTrue(callbackHit);
        }

        private void RunSimpleModuleExecutionTest(int moduleId, string moduleRoute, Action<PageController, HttpContextBase, ModuleInfo, string> assert)
        {
            //Arrange
            var executionEngine = new Mock<IModuleExecutionEngine>();
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(executionEngine.Object);

            PageController controller = SetupController();

            SetupMockTabController();

            var tabModules = new Dictionary<int, ModuleInfo>
                                    {
                                        {moduleId, new ModuleInfo() {ModuleID = moduleId, DesktopModuleID = 1, PaneName="LeftPane"}},
                                    };
            SetupMockModuleController(tabModules);

            bool callbackHit = false;
            executionEngine.Setup(e => e.ExecuteModule(It.IsAny<HttpContextBase>(), It.Is<ModuleInfo>(m => m.DesktopModuleID == 1), moduleRoute))
                            .Callback<HttpContextBase, ModuleInfo, string>((c, m, s) =>
                            {
                                assert(controller, c, m, s);
                                callbackHit = true;
                            });

            // Act
            var result = controller.Index(moduleId, moduleRoute);

            // Assert
            Assert.IsTrue(callbackHit);
        }

        private PageController SetupController()
        {
            return SetupController(new TabInfo() { TabID = Constants.PAGE_DefaultId });
        }

        private PageController SetupController(TabInfo page)
        {
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            context.SetSiteContext(new SiteContext(context));

            var controller = new PageController();
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);

            controller.SiteContext.ActiveSiteAlias = new PortalAliasInfo() { HTTPAlias = Constants.SITEALIAS_Default };
            controller.SiteContext.ActiveSite = new PortalInfo() { PortalID = Constants.SITE_DefaultId, HomeTabId = Constants.PAGE_HomePageId };
            controller.SiteContext.ActivePage = page;

            return controller;
        }

        private void SetupMockTabController()
        {
            var mockPageController = new Mock<ITabController>();
            mockPageController.Setup(c => c.GetTabsByPortal(It.IsAny<int>()))
                                .Returns(new TabCollection()
                                {
                                    new TabInfo() {TabID = Constants.PAGE_DefaultId, CultureCode = Constants.CULTURE_Default},
                                    new TabInfo() {TabID = Constants.PAGE_HomePageId, CultureCode = Constants.CULTURE_Default}
                                });
            TabController.SetTestableInstance(mockPageController.Object);
           
        }

        private void SetupMockModuleController(Dictionary<int, ModuleInfo> moduleDictionary)
        {
            var mockModuleController = new Mock<IModuleController>();
            mockModuleController.Setup(c => c.GetTabModules(It.IsAny<int>()))
                                .Returns(moduleDictionary);
            ModuleController.SetTestableInstance(mockModuleController.Object);            
        }

    }
}
