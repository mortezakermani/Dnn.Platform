using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Dnn.Mvc.Core.Tests.Fakes;
using Dnn.Mvc.Framework;
using Dnn.Mvc.Framework.Controllers;
using Dnn.Mvc.Framework.Modules;
using Dnn.Mvc.Helpers;
using Dnn.Mvc.Tests.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using NUnit.Framework;

namespace Dnn.Mvc.Core.Tests.Framework.Modules
{
    [TestFixture]
    public class DnnControllerTests
    {
        private const string TestViewName = "Foo";

        [Test]
        public void SiteContext_Property_Is_Null_If_Not_Set() 
        {
            //Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();

            //Act
            var controller = SetupController(context);

            //Assert
            Assert.IsNull(controller.SiteContext);
        }

        [Test]
        public void SiteContext_Property_Is_Set_If_Added_To_HttpContext()
        {
            //Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            context.SetSiteContext(new SiteContext(context));

            //Act
            var controller = SetupController(context);

            //Assert
            Assert.IsNotNull(controller.SiteContext);
        }

        [Test]
        public void ActiveSite_Property_Is_Null_If_Nt_Set_In_SiteContext()
        {
            //Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            context.SetSiteContext(new SiteContext(context));

            //Act
            var controller = SetupController(context);

            //Assert
            Assert.IsNull(controller.ActiveSite);
        }

        [Test]
        public void ActiveSite_Property_Is_Set_If_Set_In_SiteContext()
        {
            //Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            context.SetSiteContext(new SiteContext(context) { ActiveSite = new PortalInfo()});

            //Act
            var controller = SetupController(context);

            //Assert
            Assert.IsNotNull(controller.ActiveSite);
        }

        [Test]
        public void ActiveSiteAlias_Property_Is_Set_If_Set_In_SiteContext()
        {
            //Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            context.SetSiteContext(new SiteContext(context) { ActiveSiteAlias = new PortalAliasInfo() });

            //Act
            var controller = SetupController(context);

            //Assert
            Assert.IsNotNull(controller.ActiveSiteAlias);
        }

        [Test]
        public void ActiveSiteAlias_Property_Is_Null_If_Nt_Set_In_SiteContext()
        {
            //Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            context.SetSiteContext(new SiteContext(context));

            //Act
            var controller = SetupController(context);

            //Assert
            Assert.IsNull(controller.ActiveSiteAlias);
        }

        [Test]
        public void ActivePage_Property_Is_Set_If_Set_In_SiteContext()
        {
            //Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            context.SetSiteContext(new SiteContext(context) { ActivePage = new TabInfo() });

            //Act
            var controller = SetupController(context);

            //Assert
            Assert.IsNotNull(controller.ActivePage);
        }

        [Test]
        public void ActivePage_Property_Is_Null_If_Nt_Set_In_SiteContext()
        {
            //Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            context.SetSiteContext(new SiteContext(context));

            //Act
            var controller = SetupController(context);

            //Assert
            Assert.IsNull(controller.ActivePage);
        }

        [Test]
        public void ResourceNotFound_Requires_Non_NullOrEmpty_ViewName()
        {
            var controller = new FakeDnnController();
            string viewName = null;
            Assert.Throws<ArgumentException>(() => controller.ResourceNotFound(viewName));
        }

        [Test]
        public void ResourceNotFound_Requires_Non_Null_InnerResult()
        {
            var controller = new FakeDnnController();
            ActionResult actionResult = null;
            Assert.Throws<ArgumentNullException>(() => controller.ResourceNotFound(actionResult));
        }

        [Test]
        public void ResourceNotFound_With_No_Args_Returns_ResourceNotFoundResult_With_No_InnerResult()
        {
            // Arrange
            var controller = new FakeDnnController();

            // Act
            ActionResult result = controller.ResourceNotFound();

            // Assert
            ResultAssert.IsResourceNotFound(result);
        }

        [Test]
        public void ResourceNotFound_With_ViewName_Returns_ResourceNotFoundResult_Inner_ViewResult()
        {
            // Arrange
            var controller = new FakeDnnController();

            // Act
            ActionResult result = controller.ResourceNotFound(TestViewName);

            // Assert
            ResultAssert.IsResourceNotFound(result, TestViewName);
        }

        [Test]
        public void ResourceNotFound_With_ActionResult_Returns_ResourceNotFoundResult_Inner_ActionResult()
        {
            // Arrange
            var controller = new FakeDnnController();

            // Act
            var expected = new EmptyResult();
            ActionResult result = controller.ResourceNotFound(expected);

            // Assert
            ResultAssert.IsResourceNotFound(result, ResultAssert.IsEmpty);
        }

        private DnnController SetupController(HttpContextBase context)
        {
            var controller = new FakeDnnController();
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller );
            return controller;
        }
    }
}
