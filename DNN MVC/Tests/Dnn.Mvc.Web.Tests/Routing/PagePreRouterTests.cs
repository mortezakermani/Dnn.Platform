using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Routing;
using Dnn.Mvc.Framework;
using Dnn.Mvc.Helpers;
using Dnn.Mvc.Tests.Utilities;
using Dnn.Mvc.Utils.Entities.Portals;
using Dnn.Mvc.Web.Framework;
using Dnn.Mvc.Web.Helpers;
using Dnn.Mvc.Web.Routing;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;

using Moq;

using NUnit.Framework;

using IPortalController = Dnn.Mvc.Utils.Entities.Portals.IPortalController;

namespace Dnn.Mvc.Web.Tests.Routing
{
    [TestFixture]
    class PagePreRouterTests
    {
        private Mock<ITabController> _tabController;
        private Mock<IPortalController> _portalController;

        [SetUp]
        public void SetUp()
        {
            _tabController = new Mock<ITabController>();
            _tabController.Setup(ac => ac.GetTabsByPortal(It.IsAny<int>())).Returns(CreatePages());
            TabController.SetTestableInstance(_tabController.Object);

            _portalController = new Mock<IPortalController>();
            PortalControllerAdapter.SetTestableInstance(_portalController.Object);
        }

        [TearDown]
        public void TearDown()
        {
            TabController.ClearInstance();
        }

        [Test]
        public void GetRouteData_Returns_Null_If_No_Page_Path_Matches_Url()
        {
            // Arrange
            var router = new PagePreRouter();
            HttpContextBase httpContext = CreateMockHttpContext("~/Zoop/Zork/Zoink");

            // Act
            RouteData routeData = router.GetRouteData(httpContext);

            // Assert
            Assert.IsNull(routeData);
        }

        [TestCase("~/Foo/Baz/Bar/Zoop/Zork/Zoink", 7)]
        [TestCase("~/Foo/Bar/Zoink", 4)]
        [TestCase("~/Foo/Baz/Bar", 6)]
        [TestCase("~/", 1)]
        public void GetRouteData_Sets_ActivePage_If_Url_Prefix_Matches_Page_Path(string url, int expectedPageId)
        {
            // Arrange
            var router = new PagePreRouter();
            HttpContextBase httpContext = CreateMockHttpContext(url);

            // Act
            RouteData routeData = router.GetRouteData(httpContext);

            // Assert
            SiteContext context = httpContext.GetSiteContext();
            Assert.AreEqual(expectedPageId, context.ActivePage.TabID);
        }

        [Test]
        public void GetRouteData_Calls_All_Routes_After_Prerouter()
        {
            // Arrange
            HttpContextBase httpContext = CreateMockHttpContext("~/Foo/Baz/Bar/Zoop/Zork/Zoink");

            var mockRoute1 = new Mock<RouteBase>();
            var mockRoute2 = new Mock<RouteBase>();

            RouteData nullRouteData = null;

            mockRoute1.Setup(r => r.GetRouteData(It.IsAny<HttpContextBase>()))
                        .Returns(nullRouteData);

            var router = new PagePreRouter();
            router.RouteCollection = new RouteCollection 
                                            {
                                                {"FirstRoute", mockRoute1.Object},
                                                {"Prerouter", router},
                                                {"NextRoute", mockRoute2.Object}
                                            };

            // Act
            RouteData routeData = router.GetRouteData(httpContext);

            // Assert
            mockRoute2.Verify(r => r.GetRouteData(It.IsAny<HttpContextBase>()));
        }

        [Test]
        public void GetRouteData_Rewrites_Request_To_Remove_Page_Path()
        {
            // Arrange
            HttpContextBase httpContext = CreateMockHttpContext("~/Foo/Baz/Bar/Zoop/Zork/Zoink");

            HttpContextBase rewrittenContext = null;
            var mockRoute = new Mock<RouteBase>();
            mockRoute.Setup(r => r.GetRouteData(It.IsAny<HttpContextBase>()))
                     .Callback<HttpContextBase>(c => rewrittenContext = c);

            var router = new PagePreRouter();
            router.RouteCollection = new RouteCollection 
                                            {
                                                {"Prerouter", router},
                                                {"NextRoute", mockRoute.Object}
                                            };

            // Act
            RouteData routeData = router.GetRouteData(httpContext);

            // Assert
            Assert.AreEqual("~/Zork/Zoink", rewrittenContext.Request.AppRelativeCurrentExecutionFilePath);
        }

        [Test]
        public void GetRouteData_Properly_Rewrites_An_Exact_Page_Match()
        {
            // Arrange
            HttpContextBase httpContext = CreateMockHttpContext("~/Foo/Baz/Bar/Zoop");

            HttpContextBase rewrittenContext = null;
            var mockRoute = new Mock<RouteBase>();
            mockRoute.Setup(r => r.GetRouteData(It.IsAny<HttpContextBase>()))
                     .Callback<HttpContextBase>(c => rewrittenContext = c);

            var router = new PagePreRouter();
            router.RouteCollection = new RouteCollection 
                                            {
                                                {"Prerouter", router},
                                                {"NextRoute", mockRoute.Object}
                                            };

            // Act
            RouteData routeData = router.GetRouteData(httpContext);

            // Assert
            Assert.AreEqual("~/", rewrittenContext.Request.AppRelativeCurrentExecutionFilePath);
        }

        [Test]
        public void GetRouteData_Properly_Rewrites_An_Exact_Page_Match_With_Trailing_Slash()
        {
            // Arrange
            HttpContextBase httpContext = CreateMockHttpContext("~/Foo/Baz/Bar/Zoop/");

            HttpContextBase rewrittenContext = null;
            var mockRoute = new Mock<RouteBase>();
            mockRoute.Setup(r => r.GetRouteData(It.IsAny<HttpContextBase>()))
                     .Callback<HttpContextBase>(c => rewrittenContext = c);

            var router = new PagePreRouter();
            router.RouteCollection = new RouteCollection 
                                            {
                                                {"Prerouter", router},
                                                {"NextRoute", mockRoute.Object}
                                            };

            // Act
            RouteData routeData = router.GetRouteData(httpContext);

            // Assert
            Assert.AreEqual("~/", rewrittenContext.Request.AppRelativeCurrentExecutionFilePath);
        }

        [Test]
        public void GetRouteData_Returns_Null_If_ActivePage_Not_Null()
        {
            // Arrange
            var mockRoute1 = new Mock<RouteBase>();

            mockRoute1.Setup(r => r.GetRouteData(It.IsAny<HttpContextBase>()))
                      .Callback(() => Assert.Fail("Expected that the pre-router would be bypassed"));

            var fakeRoutes = new RouteCollection 
                                    {
                                        {"NextRoute", mockRoute1.Object},
                                    };

            var router = new PagePreRouter();
            router.RouteCollection = fakeRoutes;

            HttpContextBase httpContext = MockHelper.CreateMockHttpContext("http://localhost/Foo/Bar/Qux?id=43234&a=234#foo");
            SiteContext context = new SiteContext(httpContext) {ActivePage = new TabInfo()};
            httpContext.SetSiteContext(context);

            // Act
            RouteData routeData = router.GetRouteData(httpContext);

            // Assert
            Assert.IsNull(routeData);
        }

        [Test]
        public void GetVirtualPath_Calls_All_Routes_After_Prerouter()
        {
            // Arrange
            var mockRoute1 = new Mock<RouteBase>();
            var mockRoute2 = new Mock<RouteBase>();


            var router = new PagePreRouter();
            router.RouteCollection = new RouteCollection 
                                            {
                                                {"FirstRoute", mockRoute1.Object},
                                                {"Prerouter", router},
                                                {"NextRoute", mockRoute2.Object}
                                            };

            HttpContextBase httpContext = CreateMockHttpContext("~/Foo/Bar/Baz");
            httpContext.SetSiteContext(new SiteContext(httpContext) 
                                                { 
                                                    ActivePage = new TabInfo()
                                                                        {
                                                                            TabID = 4, TabPath="/"
                                                                        }
                                                });

            // Act
            VirtualPathData pathData = router.GetVirtualPath(new RequestContext(httpContext, new RouteData()),
                                                             new RouteValueDictionary());
            // Assert
            mockRoute1.Verify(r => r.GetVirtualPath(It.IsAny<RequestContext>(), It.IsAny<RouteValueDictionary>()), Times.Never);
            mockRoute2.Verify(r => r.GetVirtualPath(It.IsAny<RequestContext>(), It.IsAny<RouteValueDictionary>()));
        }

        [Test]
        public void GetVirtualPath_Removes_Page_RouteValue_If_Present_Before_Rerouting()
        {
            // Arrange
            var mockRoute = new Mock<RouteBase>();

            var router = new PagePreRouter();
            router.RouteCollection = new RouteCollection 
                                                {
                                                    {"Prerouter", router},
                                                    {"NextRoute", mockRoute.Object}
                                                };

            HttpContextBase httpContext = CreateMockHttpContext("~/Foo/Bar/Baz");
            var values = new RouteValueDictionary(new { page = new TabInfo { TabPath = "/" }, foo = "bar" });

            // Act
            var expectedRequestContext = new RequestContext(httpContext, new RouteData());
            VirtualPathData pathData = router.GetVirtualPath(expectedRequestContext, values);

            // Assert
            mockRoute.Verify(r => r.GetVirtualPath(expectedRequestContext, It.Is<RouteValueDictionary>(rvd =>
                String.Equals((string)rvd["foo"], "bar") && !rvd.ContainsKey("page")
            )));
        }

        [Test]
        public void GetVirtualPath_Prepends_Path_Of_Page_From_Values_To_Path_Returned_By_Rerouting()
        {
            // Arrange
            var mockRoute = new Mock<RouteBase>();

            mockRoute.Setup(r => r.GetVirtualPath(It.IsAny<RequestContext>(), It.IsAny<RouteValueDictionary>()))
                     .Returns(new VirtualPathData(mockRoute.Object, "Zoop/Zork/Zoink"));

            var router = new PagePreRouter();
            router.RouteCollection = new RouteCollection 
                                        {
                                            {"Prerouter", router},
                                            {"NextRoute", mockRoute.Object}
                                        };

            HttpContextBase httpContext = CreateMockHttpContext("~/Foo/Bar/Baz");
            var values = new RouteValueDictionary(new { page = new TabInfo { TabPath = "/Foo/Bar" } });

            // Act
            VirtualPathData pathData = router.GetVirtualPath(new RequestContext(httpContext, new RouteData()), values);

            // Assert
            Assert.AreEqual("Foo/Bar/Zoop/Zork/Zoink", pathData.VirtualPath);
        }

        [Test]
        public void GetVirtualPath_Prepends_Path_Of_ActivePage_From_Context_If_No_Path_In_Values()
        {
            // Arrange
            var mockRoute = new Mock<RouteBase>();

            mockRoute.Setup(r => r.GetVirtualPath(It.IsAny<RequestContext>(), It.IsAny<RouteValueDictionary>()))
                     .Returns(new VirtualPathData(mockRoute.Object, "Zoop/Zork/Zoink"));

            var router = new PagePreRouter();
            router.RouteCollection = new RouteCollection 
                                            {
                                                {"Prerouter", router},
                                                {"NextRoute", mockRoute.Object}
                                            };

            HttpContextBase httpContext = CreateMockHttpContext("~/Foo/Bar/Baz");
            httpContext.SetSiteContext(new SiteContext(httpContext) { ActivePage = new TabInfo { TabPath = "/Foo/Bar" } });
            var values = new RouteValueDictionary();

            // Act
            VirtualPathData pathData = router.GetVirtualPath(new RequestContext(httpContext, new RouteData()), values);

            // Assert
            Assert.AreEqual("Foo/Bar/Zoop/Zork/Zoink", pathData.VirtualPath);
        }

        [Test]
        public void GetVirtualPath_Correctly_Prepends_Page_Path_If_Route_Result_Is_Empty()
        {
            // Arrange
            var mockRoute = new Mock<RouteBase>();

            mockRoute.Setup(r => r.GetVirtualPath(It.IsAny<RequestContext>(), It.IsAny<RouteValueDictionary>()))
                     .Returns(new VirtualPathData(mockRoute.Object, String.Empty));

            var router = new PagePreRouter();
            router.RouteCollection = new RouteCollection 
                                            {
                                                {"Prerouter", router},
                                                {"NextRoute", mockRoute.Object}
                                            };

            HttpContextBase httpContext = CreateMockHttpContext("~/Foo/Bar/Baz");
            httpContext.SetSiteContext(new SiteContext(httpContext) { ActivePage = new TabInfo { TabPath = "/Foo/Bar" } });
            var values = new RouteValueDictionary();

            // Act
            VirtualPathData pathData = router.GetVirtualPath(new RequestContext(httpContext, new RouteData()), values);

            // Assert
            Assert.AreEqual("Foo/Bar", pathData.VirtualPath);
        }

        [Test]
        public void GetVirtualPath_Correctly_Prepends_Page_Path_If_ActivePage_Is_Root_Page()
        {
            // Arrange
            var mockRoute = new Mock<RouteBase>();

            mockRoute.Setup(r => r.GetVirtualPath(It.IsAny<RequestContext>(), It.IsAny<RouteValueDictionary>()))
                     .Returns(new VirtualPathData(mockRoute.Object, "Zoop/Zork/Zoink"));

            var router = new PagePreRouter();
            router.RouteCollection = new RouteCollection 
                                            {
                                                {"Prerouter", router},
                                                {"NextRoute", mockRoute.Object}
                                            };

            HttpContextBase httpContext = CreateMockHttpContext("~/Foo/Bar/Baz");
            httpContext.SetSiteContext(new SiteContext(httpContext) { ActivePage = new TabInfo { TabPath = "/" } });
            RouteValueDictionary values = new RouteValueDictionary();

            // Act
            VirtualPathData pathData = router.GetVirtualPath(new RequestContext(httpContext, new RouteData()), values);

            // Assert
            Assert.AreEqual("Zoop/Zork/Zoink", pathData.VirtualPath);
        }

        [Test]
        public void GetVirtualPath_Returns_Null_If_Null_Page_Present_In_Values()
        {
            // Arrange
            var mockRoute = new Mock<RouteBase>();

            var router = new PagePreRouter();
            router.RouteCollection = new RouteCollection 
                                            {
                                                {"Prerouter", router},
                                                {"NextRoute", mockRoute.Object}
                                            };

            HttpContextBase httpContext = CreateMockHttpContext("~/Foo/Bar/Baz");
            var values = new RouteValueDictionary(new { page = (TabInfo)null });

            // Act
            VirtualPathData pathData = router.GetVirtualPath(new RequestContext(httpContext, new RouteData()), values);

            // Assert
            mockRoute.Verify(r => r.GetVirtualPath(It.IsAny<RequestContext>(), It.IsAny<RouteValueDictionary>()), Times.Never);
            Assert.IsNull(pathData);
        }

        private static HttpContextBase CreateMockHttpContext(string appRelativePath)
        {
            var mockContext = new Mock<HttpContextBase>();

            var mockRequest = new Mock<HttpRequestBase>();
            mockRequest.SetupGet(r => r.ApplicationPath)
                       .Returns("/TestAppPath");
            mockRequest.SetupGet(r => r.Url)
                       .Returns(new Uri(String.Format("http://localhost/TestAppPath{0}?query=q#bar", appRelativePath.TrimStart('~'))));
            mockRequest.SetupGet(r => r.AppRelativeCurrentExecutionFilePath)
                       .Returns(appRelativePath);
            mockContext.SetupGet(c => c.Request)
                       .Returns(mockRequest.Object);
            mockContext.SetupGet(c => c.Items)
                       .Returns(new Dictionary<string, object>());

            var httpContext = mockContext.Object;
            var siteContext = new SiteContext(httpContext) { ActiveSite = new PortalInfo() };
            httpContext.SetSiteContext(siteContext);

            return httpContext;
        }

        private static TabCollection CreatePages()
        {
            return new TabCollection {
                new TabInfo {TabID = 1, TabName = "Test Page 1", TabPath = "//", CultureCode = "en-US"}, 
                new TabInfo {TabID = 2, TabName = "Test Page 2", TabPath = "//Foo", CultureCode = "en-US"}, 
                new TabInfo {TabID = 3, TabName = "Test Page 3", TabPath = "//Bar",  CultureCode = "en-US"}, 
                new TabInfo {TabID = 4, TabName = "Test Page 4", TabPath = "//Foo//Bar",  CultureCode = "en-US"}, 
                new TabInfo {TabID = 5, TabName = "Test Page 5", TabPath = "//Bar//Qux//Baz",  CultureCode = "en-US"}, 
                new TabInfo {TabID = 6, TabName = "Test Page 6", TabPath = "//Foo//Baz//Bar",  CultureCode = "en-US"}, 
                new TabInfo {TabID = 7, TabName = "Test Page 6", TabPath = "//Foo//Baz//Bar//Zoop",  CultureCode = "en-US"}, 
                new TabInfo {TabID = 8, TabName = "Test Page 7", TabPath = "//Foo//Baz//Foo//Bar//Baz",  CultureCode = "en-US"}, 
            };
        }

    }
}
