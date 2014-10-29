using System;
using System.Web;
using Dnn.Mvc.Framework;
using Dnn.Mvc.Helpers;
using Dnn.Mvc.Tests.Utilities;
using NUnit.Framework;

namespace Dnn.Mvc.Core.Tests.Helpers
{
    [TestFixture]
    public class HttpContextBaseExtensionsTests
    {
        [Test]
        public void HasSiteContext_Returns_False_If_No_SiteContext_Key_Exists()
        {
            // Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();

            // Act and Assert
            Assert.IsFalse(context.HasSiteContext());
        }

        [Test]
        public void HasSiteContext_Returns_False_If_SiteContext_Key_Is_Null()
        {
            // Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            context.Items.Add(HttpContextBaseExtensions.GetKeyFor<SiteContext>(), null);

            // Act and Assert
            Assert.IsFalse(context.HasSiteContext());
        }


        [Test]
        public void HasSiteContext_Returns_True_If_SiteContext_Key_Exists()
        {
            // Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            var expected = new SiteContext(context);
            context.Items.Add(HttpContextBaseExtensions.GetKeyFor<SiteContext>(), expected);

            // Act and Assert
            Assert.IsTrue(context.HasSiteContext());
        }

        [Test]
        public void GetSiteContext_Returns_Null_SiteRequestContext_If_None_Present()
        {
            // Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();

            // Act and Assert
            Assert.IsNull(context.GetSiteContext());
        }

        [Test]
        public void GetSiteContext_Returns_Stored_SiteRequestContext_If_Present()
        {
            // Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            var expected = new SiteContext(context);
            context.Items.Add(HttpContextBaseExtensions.GetKeyFor<SiteContext>(), expected);

            // Act
            var actual = context.GetSiteContext();

            // Assert
            Assert.AreSame(expected, actual);
        }

        [Test]
        public void SetSiteContext_Sets_SiteRequestContext()
        {
            // Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            var siteContext = new SiteContext(context);

            // Act
            context.SetSiteContext(siteContext);

            // Assert
            var actual = context.Items[HttpContextBaseExtensions.GetKeyFor<SiteContext>()];
            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<SiteContext>(actual);
        }

        [Test]
        public void GetKeyFor_Prefixes_Full_TypeName_With_DnnMvc_Prefix()
        {
            // Assert
            Assert.AreEqual(String.Format("DnnMvc:{0}", typeof(Version).FullName), HttpContextBaseExtensions.GetKeyFor<Version>());
        }

    }
}
