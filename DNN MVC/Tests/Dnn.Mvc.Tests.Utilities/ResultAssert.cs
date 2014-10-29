using System;
using System.Web.Mvc;
using System.Web.Routing;
using Dnn.Mvc.Framework.ActionResults;
using Dnn.Mvc.Helpers;
using NUnit.Framework;

namespace Dnn.Mvc.Tests.Utilities
{
    public static class ResultAssert
    {
        public static void IsEmpty(ActionResult result)
        {
            Assert.IsInstanceOf<EmptyResult>(result);
        }
        public static void IsResourceNotFound(ActionResult result)
        {
            IsResourceNotFound(result, innerResult => Assert.IsNull(innerResult,
                                                                    "Expected that the default inner result would be used"));
        }

        public static void IsResourceNotFound(ActionResult result, Action<ActionResult> innerResultAssert)
        {
            var notFoundResult = result.AssertCast<ResourceNotFoundResult>();
            innerResultAssert(notFoundResult.InnerResult);
        }

        public static void IsResourceNotFound(ActionResult result, string viewName)
        {
            var notFoundResult = result.AssertCast<ResourceNotFoundResult>();
            IsView(notFoundResult.InnerResult, viewName);
        }

        public static void IsUnauthorized(ActionResult result)
        {
            Assert.IsInstanceOf<HttpUnauthorizedResult>(result);
        }

        public static void IsView(ActionResult result, string viewName)
        {
            IsView(result, viewName, String.Empty, new RouteValueDictionary());
        }

        public static void IsView(ActionResult result, string viewName, string masterName, RouteValueDictionary expectedViewData)
        {
            ViewResult viewResult = result.AssertCast<ViewResult>();
            StringsEqualOrBothNullOrEmpty(viewName, viewResult.ViewName);
            StringsEqualOrBothNullOrEmpty(viewName, viewResult.ViewName);

            DictionaryAssert.ContainsEntries(expectedViewData, viewResult.ViewData);
        }
        private static TCast AssertCast<TCast>(this ActionResult result) where TCast : class
        {
            var castResult = result as TCast;
            Assert.IsNotNull(castResult);
            return castResult;
        }

        private static void StringsEqualOrBothNullOrEmpty(string expected, string actual)
        {
            if (String.IsNullOrEmpty(expected))
            {
                Assert.IsTrue(String.IsNullOrEmpty(actual));
            }
            else
            {
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
