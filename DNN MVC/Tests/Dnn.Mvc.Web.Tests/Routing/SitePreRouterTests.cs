using System.Collections.Specialized;
using System.Web;
using System.Web.Routing;
using Dnn.Mvc.Framework;
using Dnn.Mvc.Helpers;
using Dnn.Mvc.Tests.Utilities;
using Dnn.Mvc.Web.Routing;

using DotNetNuke.Entities.Portals;

using Moq;

using NUnit.Framework;

namespace Dnn.Mvc.Web.Tests.Routing
{
    [TestFixture]
    public class SitePreRouterTests
    {
        private Mock<IPortalAliasController> _aliasController;

        [SetUp]
        public void SetUp()
        {
            _aliasController = new Mock<IPortalAliasController>();
            _aliasController.Setup(ac => ac.GetPortalAliases()).Returns(CreateTestAliases());
            PortalAliasController.SetTestableInstance(_aliasController.Object);
        }

        [TearDown]
        public void TearDown()
        {
            PortalAliasController.ClearInstance();
            PortalController.ClearInstance();
        }

        [Test]
        public void GetRouteData_Returns_Null_If_No_SiteAlias_Matches_Url()
        {
            // Arrange
            var router = new SitePreRouter();
            HttpContextBase httpContext = MockHelper.CreateMockHttpContext("http://www.test.example/Foo/Bar/Baz?id=42");

            // Act
            RouteData routeData = router.GetRouteData(httpContext);

            // Assert
            Assert.IsNull(routeData);
        }

        [TestCase("http://localhost/Foo/Bar/Qux?id=43234&a=234#foo", 4)]
        [TestCase("http://localhost/Foo/Bar/Baz?id=43234&a=234#foo", 3)]
        [TestCase("http://localhost:8080/Quz/Baz?id=43234&a=234#foo", 6)]
        public void GetRouteData_Selects_Correct_SiteAlias(string url, int expectedAliasId)
        {
            // Arrange
            var site = new PortalInfo();
            var siteController = new Mock<IPortalController>();
            siteController.Setup(sc => sc.GetPortal(It.IsAny<int>())).Returns(site);
            PortalController.SetTestableInstance(siteController.Object);

            HttpContextBase httpContext = MockHelper.CreateMockHttpContext(url);

            var router = new SitePreRouter {RouteCollection = new RouteCollection()};

            // Act
            RouteData routeData = router.GetRouteData(httpContext);

            // Assert
            Assert.AreEqual(expectedAliasId, httpContext.GetSiteContext().ActiveSiteAlias.PortalAliasID);
        }

        [TestCase("http://localhost/Foo/Bar/Qux?id=43234&a=234#foo", 4)]
        [TestCase("http://localhost/Foo/Bar/Baz?id=43234&a=234#foo", 3)]
        [TestCase("http://localhost:8080/Quz/Baz?id=43234&a=234#foo", 6)]
        public void GetRouteData_Selects_Correct_Site(string url, int expectedId)
        {
            // Arrange
            var siteController = new Mock<IPortalController>();
            siteController.Setup(sc => sc.GetPortal(expectedId)).Returns(new PortalInfo() {PortalID = expectedId});
            PortalController.SetTestableInstance(siteController.Object);

            HttpContextBase httpContext = MockHelper.CreateMockHttpContext(url);

            var router = new SitePreRouter { RouteCollection = new RouteCollection() };

            // Act
            RouteData routeData = router.GetRouteData(httpContext);

            // Assert
            Assert.AreEqual(expectedId, httpContext.GetSiteContext().ActiveSite.PortalID);
        }

        [Test]
        public void GetRouteData_Calls_All_Routes_After_PreRouter()
        {
            // Arrange
            var siteController = new Mock<IPortalController>();
            siteController.Setup(sc => sc.GetPortal(It.IsAny<int>())).Returns(new PortalInfo { PortalID = 1 });
            PortalController.SetTestableInstance(siteController.Object);

            var expectedRouteData = new RouteData();

            var mockRoute1 = new Mock<RouteBase>();
            var mockRoute2 = new Mock<RouteBase>();

            mockRoute2.Setup(r => r.GetRouteData(It.IsAny<HttpContextBase>()))
                        .Returns(expectedRouteData);

            var router = new SitePreRouter();
            var fakeRoutes = new RouteCollection 
                                {
                                    {"NextRoute", mockRoute1.Object},
                                    {"Prerouter", router},
                                    {"AnotherRoute", mockRoute2.Object}
                                };

            HttpContextBase httpContext = MockHelper.CreateMockHttpContext("http://localhost/Foo/Bar/Qux?id=43234&a=234#foo");
            router.RouteCollection = fakeRoutes;

            // Act
            RouteData actualRouteData = router.GetRouteData(httpContext);

            // Assert
            mockRoute1.Verify(r => r.GetRouteData(It.IsAny<HttpContextBase>()), Times.Never());
            mockRoute2.Verify(r => r.GetRouteData(It.IsAny<HttpContextBase>()));
            Assert.AreEqual(expectedRouteData, actualRouteData);
        }

        [Test]
        public void GetRouteData_Rewrites_Request_Before_Rerouting()
        {
            // Arrange
            var siteController = new Mock<IPortalController>();
            siteController.Setup(sc => sc.GetPortal(It.IsAny<int>())).Returns(new PortalInfo { PortalID = 1 });
            PortalController.SetTestableInstance(siteController.Object);

            HttpContextBase rewrittenContext = null;
            var mockRoute1 = new Mock<RouteBase>();

            mockRoute1.Setup(r => r.GetRouteData(It.IsAny<HttpContextBase>()))
                      .Callback<HttpContextBase>(c => rewrittenContext = c);

            var router = new SitePreRouter();
            var fakeRoutes = new RouteCollection 
                                    {
                                        {"Prerouter", router},
                                        {"NextRoute", mockRoute1.Object},
                                    };

            HttpContextBase httpContext = MockHelper.CreateMockHttpContext("http://localhost/Foo/Bar/Qux?id=43234#foo");
            var queryString = new NameValueCollection();
            Mock.Get(httpContext.Request).Setup(r => r.QueryString).Returns(queryString);
            router.RouteCollection = fakeRoutes;

            // Act
            router.GetRouteData(httpContext);

            // Assert
            Assert.AreEqual("~/Qux/", rewrittenContext.Request.AppRelativeCurrentExecutionFilePath);
            Assert.AreSame(queryString, rewrittenContext.Request.QueryString);
            Assert.AreEqual("http://localhost/Qux/?id=43234#foo", rewrittenContext.Request.Url.ToString());
        }

        [Test]
        public void GetRouteData_Returns_Null_If_ActiveSite_Not_Null()
        {
            // Arrange
            var mockRoute1 = new Mock<RouteBase>();

            mockRoute1.Setup(r => r.GetRouteData(It.IsAny<HttpContextBase>()))
                      .Callback(Assert.Fail);

            var fakeRoutes = new RouteCollection 
                                    {
                                        {"NextRoute", mockRoute1.Object}
                                    };

            var router = new SitePreRouter {RouteCollection = fakeRoutes};

            HttpContextBase httpContext = MockHelper.CreateMockHttpContext("http://localhost/Foo/Bar/Qux?id=43234&a=234#foo");
            var siteContext = new SiteContext(httpContext) {ActiveSite = new PortalInfo()};
            httpContext.SetSiteContext(siteContext);

            // Act
            RouteData routeData = router.GetRouteData(httpContext);

            // Assert
            Assert.IsNull(routeData);
        }

        [Test]
        public void GetVirtualPath_Calls_All_Other_Routes_To_Route_Request()
        {
            // Arrange
            HttpContextBase httpContext = MockHelper.CreateMockHttpContext("http://localhost/Foo/Bar/Qux?id=43234&a=234");

            SetActiveSiteAlias(httpContext, "localhost/Foo/Bar/Qux");
            SetMockApplicationPath(httpContext, "/Foo");

            var mockRoute1 = new Mock<RouteBase>();
            var mockRoute2 = new Mock<RouteBase>();

            var router = new SitePreRouter();
            var fakeRoutes = new RouteCollection 
                                {
                                    {"FirstRoute", mockRoute1.Object},
                                    {"Prerouter", router},
                                    {"NextRoute", mockRoute2.Object}
                                };

            router.RouteCollection = fakeRoutes;

            // Act
            var expectedRequestContext = new RequestContext(httpContext, new RouteData());
            var expectedValues = new RouteValueDictionary();
            router.GetVirtualPath(expectedRequestContext,
                                  expectedValues);

            // Assert
            mockRoute1.Verify(r => r.GetVirtualPath(It.IsAny<RequestContext>(), It.IsAny<RouteValueDictionary>()), Times.Never());
            mockRoute2.Verify(r => r.GetVirtualPath(expectedRequestContext, expectedValues));
        }

        [Test]
        public void GetVirtualPath_Returns_Null_If_No_Active_SiteAlias()
        {
            // Arrange
            var mockRoute1 = new Mock<RouteBase>();
            mockRoute1.Setup(r => r.GetVirtualPath(It.IsAny<RequestContext>(), It.IsAny<RouteValueDictionary>()))
                      .Returns(new VirtualPathData(mockRoute1.Object, "Foo"));
            RouteCollection fakeRoutes = new RouteCollection 
                                            {
                                                {"NextRoute", mockRoute1.Object},
                                            };

            HttpContextBase httpContext = MockHelper.CreateMockHttpContext("http://localhost/Foo/Bar/Qux?id=43234&a=234");
            httpContext.SetSiteContext(new SiteContext(httpContext));
            var router = new SitePreRouter();

            // Act
            VirtualPathData data = router.GetVirtualPath(new RequestContext(httpContext, new RouteData()),
                                                         new RouteValueDictionary());

            // Assert
            Assert.IsNull(data);
        }

        [Test]
        public void GetVirtualPath_Forwards_RequestContext_And_RouteValues_To_Routes()
        {
            // Arrange
            HttpContextBase httpContext = MockHelper.CreateMockHttpContext("http://localhost/Foo/Bar/Qux?id=43234&a=234");
            SetActiveSiteAlias(httpContext, "localhost/Foo/Bar/Qux");
            SetMockApplicationPath(httpContext, "/Foo");

            var context = new RequestContext(httpContext, new RouteData());
            var values = new RouteValueDictionary();

            var mockRoute1 = new Mock<RouteBase>();
            var mockRoute2 = new Mock<RouteBase>();

            var router = new SitePreRouter();
            var fakeRoutes = new RouteCollection 
                                    {
                                        {"Prerouter", router},
                                        {"NextRoute", mockRoute1.Object},
                                        {"AnotherRoute", mockRoute2.Object},
                                    };

            router.RouteCollection = fakeRoutes;

            // Act
            VirtualPathData pathData = router.GetVirtualPath(context, values);

            // Assert
            mockRoute1.Verify(r => r.GetVirtualPath(context, values));
            mockRoute2.Verify(r => r.GetVirtualPath(context, values));

            Assert.IsNull(pathData);
        }

        [Test]
        public void GetVirtualPath_Prepends_AppRelativePortion_Of_ActiveSiteALias_To_VirtualPath_From_Other_Routes()
        {
            // Arrange
            HttpContextBase httpContext = MockHelper.CreateMockHttpContext("http://localhost/Foo/Bar/Qux?id=43234&a=234");
            SetActiveSiteAlias(httpContext, "localhost/Foo/Bar/Qux");
            SetMockApplicationPath(httpContext, "/Foo");

            var mockRoute1 = new Mock<RouteBase>();
            mockRoute1.Setup(r => r.GetVirtualPath(It.IsAny<RequestContext>(), It.IsAny<RouteValueDictionary>()))
                      .Returns(new VirtualPathData(mockRoute1.Object, "Incoming/Routed/Path"));

            var router = new SitePreRouter();
            var fakeRoutes = new RouteCollection 
                                    {
                                        {"Prerouter", router},
                                        {"NextRoute", mockRoute1.Object},
                                    };

            router.RouteCollection = fakeRoutes;

            // Act
            VirtualPathData pathData = router.GetVirtualPath(new RequestContext(httpContext, new RouteData()), new RouteValueDictionary());

            // Assert
            Assert.AreEqual("Bar/Qux/Incoming/Routed/Path", pathData.VirtualPath);
        }

        private static void SetMockApplicationPath(HttpContextBase httpContext, string applicationPath)
        {
            Mock.Get(httpContext.Request)
                .SetupGet(r => r.ApplicationPath)
                .Returns(applicationPath);
        }

        private static void SetActiveSiteAlias(HttpContextBase httpContext, string siteAlias)
        {
            var siteContext = new SiteContext(httpContext)
                                    {
                                        ActiveSiteAlias = new PortalAliasInfo()
                                                                {
                                                                    PortalAliasID = 1, PortalID = 1, HTTPAlias = siteAlias
                                                                }
                                    };
            httpContext.SetSiteContext(siteContext);
        }

        private PortalAliasCollection CreateTestAliases()
        {
            return new PortalAliasCollection {
                {"localhost", new PortalAliasInfo {PortalAliasID = 1, HTTPAlias = "localhost", PortalID = 1}}, 
                {"localhost/foo", new PortalAliasInfo {PortalAliasID = 2, HTTPAlias = "localhost/foo", PortalID = 2 }},
                {"localhost/foo/bar/baz", new PortalAliasInfo {PortalAliasID = 3, HTTPAlias = "localhost/foo/bar/baz", PortalID = 3 }},
                {"localhost/foo/bar", new PortalAliasInfo {PortalAliasID = 4, HTTPAlias = "localhost/foo/bar", PortalID = 4 }},
                {"localhost:8080", new PortalAliasInfo {PortalAliasID = 5, HTTPAlias = "localhost:8080", PortalID = 5 }},
                {"localhost:8080/quz", new PortalAliasInfo {PortalAliasID = 6, HTTPAlias = "localhost:8080/quz", PortalID = 6 }}
            };
        }
    }
}
