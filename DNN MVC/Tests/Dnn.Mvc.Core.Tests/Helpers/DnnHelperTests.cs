using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Dnn.Mvc.Core.Tests.Fakes;
using Dnn.Mvc.Framework;
using Dnn.Mvc.Helpers;
using Dnn.Mvc.Tests.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using Moq;
using NUnit.Framework;

namespace Dnn.Mvc.Core.Tests.Helpers
{
    [TestFixture]
    public class DnnHelperTests
    {
        [Test]
        public void Constructor_Throws_On_Null_ViewContext()
        {
            //Arrange
            var mockContainer = new Mock<IViewDataContainer>();

            //Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnHelper(null, mockContainer.Object));
        }

        [Test]
        public void Constructor_Overload_Throws_On_Null_ViewContext()
        {
            //Arrange
            var mockContainer = new Mock<IViewDataContainer>();

            //Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnHelper(null, mockContainer.Object, new RouteCollection()));
        }

        [Test]
        public void Constructor_Throws_On_Null_DataContainer()
        {
            //Arrange

            //Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnHelper(new ViewContext(), null));
        }

        [Test]
        public void Constructor_Overload_Throws_On_Null_DataContainer()
        {
            //Arrange

            //Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnHelper(new ViewContext(), null, new RouteCollection()));
        }

        [Test]
        public void Constructor_Overload_Throws_On_Null_RouteCollection()
        {
            //Arrange
            var mockContainer = new Mock<IViewDataContainer>();

            //Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnHelper(new ViewContext(), mockContainer.Object, null));
        }

        [Test]
        public void Constructor_Sets_ViewContext_Property()
        {
            //Arrange
            var mockContainer = new Mock<IViewDataContainer>();
            mockContainer.Setup(c => c.ViewData).Returns(new ViewDataDictionary());

            var viewContext = new ViewContext();

            //Act
            var helper = new DnnHelper(viewContext, mockContainer.Object, new RouteCollection());

            //Assert
            Assert.AreEqual(helper.ViewContext, viewContext);
        }

        [Test]
        public void Constructor_Sets_ViewDataContainer_Property()
        {
            //Arrange
            var mockContainer = new Mock<IViewDataContainer>();
            mockContainer.Setup(c => c.ViewData).Returns(new ViewDataDictionary());

            var viewContext = new ViewContext();

            //Act
            var helper = new DnnHelper(viewContext, mockContainer.Object, new RouteCollection());

            //Assert
            Assert.AreEqual(helper.ViewDataContainer, mockContainer.Object);
        }

        [Test]
        public void Constructor_Sets_RouteCollection_Property()
        {
            //Arrange
            var mockContainer = new Mock<IViewDataContainer>();
            mockContainer.Setup(c => c.ViewData).Returns(new ViewDataDictionary());

            var routes = new RouteCollection();

            //Act
            var helper = new DnnHelper(new ViewContext(), mockContainer.Object, routes);

            //Assert
            Assert.AreEqual(helper.RouteCollection, routes);
        }

        [Test]
        public void Constructor_Sets_ViewData_Property()
        {
            //Arrange
            var mockContainer = new Mock<IViewDataContainer>();
            var viewData = new ViewDataDictionary();
            mockContainer.Setup(c => c.ViewData).Returns(viewData);

            var viewContext = new ViewContext();

            //Act
            var helper = new DnnHelper(viewContext, mockContainer.Object, new RouteCollection());

            //Assert
            Assert.AreEqual(helper.ViewData, viewData);
        }

        [Test]
        public void Constructor_Sets_ViewData_Model_Property()
        {
            //Arrange
            var mockContainer = new Mock<IViewDataContainer>();
            var dog = new Dog();
            var viewData = new ViewDataDictionary(dog);
            mockContainer.Setup(c => c.ViewData).Returns(viewData);

            var viewContext = new ViewContext();

            //Act
            var helper = new DnnHelper<Dog>(viewContext, mockContainer.Object, new RouteCollection());

            //Assert
            Assert.AreEqual(helper.ViewData.Model, dog);
        }

        [Test]
        public void SiteContext_Property_Returns_SiteContext()
        {
            //Arrange
            var mockContainer = new Mock<IViewDataContainer>();
            var viewData = new ViewDataDictionary();
            mockContainer.Setup(c => c.ViewData).Returns(viewData);

            HttpContextBase context = MockHelper.CreateMockHttpContext();
            var siteContext = new SiteContext(context);
            context.SetSiteContext(siteContext);

            var viewContext = new ViewContext { HttpContext = context };

            //Act
            var helper = new DnnHelper(viewContext, mockContainer.Object, new RouteCollection());

            //Assert
            Assert.AreEqual(helper.SiteContext, siteContext);
        }

        [Test]
        public void ActivePage_Property_Returns_ActivePage_From_SiteContext()
        {
            //Arrange
            var mockContainer = new Mock<IViewDataContainer>();
            var viewData = new ViewDataDictionary();
            mockContainer.Setup(c => c.ViewData).Returns(viewData);

            HttpContextBase context = MockHelper.CreateMockHttpContext();
            var page = new TabInfo();
            var siteContext = new SiteContext(context) {ActivePage = page};
            context.SetSiteContext(siteContext);

            var viewContext = new ViewContext{ HttpContext = context };

            //Act
            var helper = new DnnHelper(viewContext, mockContainer.Object, new RouteCollection());

            //Assert
            Assert.AreEqual(helper.ActivePage, page);
        }

        [Test]
        public void ActiveSite_Property_Returns_ActiveSite_From_SiteContext()
        {
            //Arrange
            var mockContainer = new Mock<IViewDataContainer>();
            var viewData = new ViewDataDictionary();
            mockContainer.Setup(c => c.ViewData).Returns(viewData);

            HttpContextBase context = MockHelper.CreateMockHttpContext();
            var site = new PortalInfo();
            var siteContext = new SiteContext(context) { ActiveSite = site };
            context.SetSiteContext(siteContext);

            var viewContext = new ViewContext { HttpContext = context };

            //Act
            var helper = new DnnHelper(viewContext, mockContainer.Object, new RouteCollection());

            //Assert
            Assert.AreEqual(helper.ActiveSite, site);
        }

        [Test]
        public void ActiveSiteAlias_Property_Returns_ActiveSiteAlias_From_SiteContext()
        {
            //Arrange
            var mockContainer = new Mock<IViewDataContainer>();
            var viewData = new ViewDataDictionary();
            mockContainer.Setup(c => c.ViewData).Returns(viewData);

            HttpContextBase context = MockHelper.CreateMockHttpContext();
            var siteAlias = new PortalAliasInfo();
            var siteContext = new SiteContext(context) { ActiveSiteAlias = siteAlias };
            context.SetSiteContext(siteContext);

            var viewContext = new ViewContext() { HttpContext = context };

            //Act
            var helper = new DnnHelper(viewContext, mockContainer.Object, new RouteCollection());

            //Assert
            Assert.AreEqual(helper.ActiveSiteAlias, siteAlias);
        }
    }
}
