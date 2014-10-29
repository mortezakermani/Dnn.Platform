using System;
using System.Web;
using Dnn.Mvc.Framework;
using Dnn.Mvc.Tests.Utilities;
using NUnit.Framework;

namespace Dnn.Mvc.Core.Tests.Framework
{
    [TestFixture]
    public class SiteContextTests
    {
        [Test]
        public void Constructor_Requires_Non_Null_HttpContext()
        {
            HttpContextBase context = null;
            Assert.Throws<ArgumentNullException>(() => new SiteContext(context));
        }

        [Test]
        public void Constructor_Sets_HttpContext_Property()
        {
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            Assert.AreSame(context, new SiteContext(context).HttpContext);
        }
    }
}
