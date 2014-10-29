using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Moq;

namespace Dnn.Mvc.Tests.Utilities
{
    public class MockHelper
    {
        public static HttpContextBase CreateMockHttpContext()
        {
            var mockContext = new Mock<HttpContextBase>();
            mockContext.SetupGet(c => c.Items)
                       .Returns(new Dictionary<string, object>());

            var mockRequest = new Mock<HttpRequestBase>();
            mockRequest.Setup(r => r.QueryString)
                       .Returns(new NameValueCollection());

            var mockResponse = new Mock<HttpResponseBase>();

            mockContext.SetupGet(c => c.Request)
                       .Returns(mockRequest.Object);
            mockContext.SetupGet(c => c.Response)
                       .Returns(mockResponse.Object);

            return mockContext.Object;
        }

        public static HttpContextBase CreateMockHttpContext(string requestUrl)
        {
            HttpContextBase httpContext = CreateMockHttpContext();

            var mockRequest = Mock.Get(httpContext.Request);
            mockRequest.Setup(r => r.Url)
                       .Returns(new Uri(requestUrl));
            mockRequest.Setup(r => r.ApplicationPath)
                       .Returns("/");
            mockRequest.Setup(r => r.RawUrl)
                       .Returns(requestUrl);

            var mockResponse = new Mock<HttpResponseBase>();
            mockResponse.Setup(r => r.ApplyAppPathModifier(It.IsAny<string>()))
                        .Returns<string>(s => s);

            Mock.Get(httpContext)
                .SetupGet(c => c.Response)
                .Returns(mockResponse.Object);

            return httpContext;
        }

        public static ControllerContext CreateMockControllerContext(ControllerBase controller)
        {
            return new ControllerContext(CreateMockHttpContext(), new RouteData(), controller);
        }

        public static ControllerContext CreateMockControllerContext(HttpContextBase httpContext)
        {
            return new ControllerContext(httpContext, new RouteData(), new Mock<ControllerBase>().Object);
        }

        public static ControllerContext CreateMockControllerContext()
        {
            return CreateMockControllerContext(new Mock<ControllerBase>().Object);
        }

        public static ControllerContext CreateMockControllerContext(RouteData routeDatae)
        {
            return new ControllerContext(CreateMockHttpContext(), routeDatae, new Mock<ControllerBase>().Object);
        }

        public static ViewContext CreateViewContext(string url)
        {
            var routeData = new RouteData();
            routeData.Values["controller"] = "Controller";
            routeData.Values["action"] = "Action";
            routeData.Values["id"] = "Id";
            var controllerContext = new ControllerContext(CreateMockHttpContext(url),
                                                                        routeData,
                                                                        new Mock<ControllerBase>().Object);
            return new ViewContext(controllerContext,
                                   new Mock<IView>().Object,
                                   new ViewDataDictionary(),
                                   new TempDataDictionary(),
                                   new StringWriter());
        }
    }
}
