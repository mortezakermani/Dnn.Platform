using System.Web.Mvc;
using Dnn.Mvc.Core.Tests.Fakes;
using Dnn.Mvc.Framework;
using Dnn.Mvc.Helpers;
using Moq;
using NUnit.Framework;

namespace Dnn.Mvc.Web.Tests.Framework
{
    [TestFixture]
    public class DnnWebViewPageTests
    {
        [Test]
        public void InitHelpers_Sets_Dnn_Property()
        {
            //Arrange
            var mockViewPage = new Mock<DnnWebViewPage>() {CallBase = true};
            mockViewPage.Object.ViewContext = new ViewContext();

            //Act
            mockViewPage.Object.InitHelpers();

            //Assert
            Assert.NotNull(mockViewPage.Object.Dnn);
            Assert.IsInstanceOf<DnnHelper>(mockViewPage.Object.Dnn);
        }

        [Test]
        public void InitHelpers_Sets_Dnn_Property_For_Strongly_Typed_Helper()
        {
            //Arrange
            var mockViewPage = new Mock<DnnWebViewPage<Dog>>() { CallBase = true };
            mockViewPage.Object.ViewContext = new ViewContext();

            //Act
            mockViewPage.Object.InitHelpers();

            //Assert
            Assert.NotNull(mockViewPage.Object.Dnn);
            Assert.IsInstanceOf<DnnHelper<Dog>>(mockViewPage.Object.Dnn);
        }
    }
}
