using System;
using System.Web.Mvc;
using System.Web.Routing;
using Dnn.Mvc.Tests.Utilities;
using Dnn.Mvc.Web.Framework;
using Dnn.Mvc.Web.Routing;

using NUnit.Framework;

namespace Dnn.Mvc.Web.Tests.Framework
{
    [TestFixture]
    public class DnnMvcApplicationTests
    {
        [Test]
        public void RegisterRoutes_Requires_Non_Null_RouteCollection()
        {
            RouteCollection routes = null;
            Assert.Throws<ArgumentNullException>(() => DnnMvcApplication.RegisterRoutes(routes));
        }

        [Test]
        public void FirstRoute_Should_Ignore_AXD_Urls()
        {
            RunRouteTest<Route>(0, r =>
                            {
                                Assert.IsInstanceOf<StopRoutingHandler>(r.RouteHandler);
                                Assert.AreEqual("{resource}.axd/{*pathInfo}", r.Url);
                            });
        }

        [Test]
        public void SitePreRouter_Should_PreRoute_To_Portal()
        {
            RunRouteTest<SitePreRouter>("SiteRouting");
        }

        [Test]
        public void PageRoute_Should_PreRoute_To_Page()
        {
            RunRouteTest<PagePreRouter>("PageRouting");
        }

        [Test]
        public void DefaultRoute_Should_Route_To_Page_Controller()
        {
            RunRouteTest<Route>("DefaultMvcRoute", r =>
                                {
                                    Assert.IsInstanceOf<MvcRouteHandler>(r.RouteHandler);
                                    Assert.AreEqual("{controller}/{action}/{id}", r.Url);
                                    DictionaryAssert.ContainsEntries(new { controller = "Page", action = "Index", id = UrlParameter.Optional }, r.Defaults);
                                });
        }

        [Test]
        public void Routes_Returns_RouteTable_Routes_If_Not_Overridden()
        {
            // Assert
            Assert.AreSame(RouteTable.Routes, DnnMvcApplication.Routes);
        }

        [Test]
        public void Routes_Returns_Provided_RouteCollection_If_Overridden()
        {
            // Arrange
            var expected = new RouteCollection();
            DnnMvcApplication.Routes = expected;

            // Act
            var actual = DnnMvcApplication.Routes;

            // Assert
            Assert.AreSame(expected, actual);
            DnnMvcApplication.Routes = null;
        }


        private static void RunRouteTest<TRoute>(int routeIndex, Action<TRoute> assert) where TRoute : RouteBase
        {
            RunRouteTest(routes => routes[routeIndex], assert);
        }

        private static void RunRouteTest<TRoute>(string routeName) where TRoute : RouteBase
        {
            Action<TRoute> assert = r => { };
            RunRouteTest(routes => routes[routeName], assert);
        }

        private static void RunRouteTest<TRoute>(string routeName, Action<TRoute> assert) where TRoute : RouteBase
        {
            RunRouteTest(routes => routes[routeName], assert);
        }

        private static void RunRouteTest<TRoute>(Func<RouteCollection, RouteBase> routeSelector, Action<TRoute> assert) where TRoute : RouteBase
        {
            // Arrange
            var routes = new RouteCollection();
            DnnMvcApplication.RegisterRoutes(routes);

            // Act
            var r = routeSelector(routes) as TRoute;

            // Assert
            Assert.IsNotNull(r);
            assert(r);
        }

    }
}
