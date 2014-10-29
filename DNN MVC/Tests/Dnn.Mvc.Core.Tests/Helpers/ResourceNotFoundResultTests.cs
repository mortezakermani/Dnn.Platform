using System.Web.Mvc;
using Dnn.Mvc.Framework.ActionResults;
using Dnn.Mvc.Helpers;
using Dnn.Mvc.Tests.Utilities;
using Moq;
using NUnit.Framework;

namespace Dnn.Mvc.Web.Tests.Helpers
{
    [TestFixture]
    public class ResourceNotFoundResultTests
    {
        [Test]
        public void DefaultInnerResultFactory_Creates_EmptyResult_If_No_Default_Set()
        {
            ResourceNotFoundResult.DefaultInnerResultFactory = null;
            ResultAssert.IsEmpty(ResourceNotFoundResult.DefaultInnerResultFactory());
        }

        [Test]
        public void DefaultInnerResultFactory_Can_Be_Overridden()
        {
            ResourceNotFoundResult.DefaultInnerResultFactory = () => new HttpUnauthorizedResult();
            ResultAssert.IsUnauthorized(ResourceNotFoundResult.DefaultInnerResultFactory());
            ResourceNotFoundResult.DefaultInnerResultFactory = null;
        }

        [Test]
        public void ExecuteResult_Executes_Default_InnerResult_With_Context_If_No_InnerResult_Provided()
        {
            // Arrange
            ControllerContext context = MockHelper.CreateMockControllerContext();
            var mockResult = new Mock<ActionResult>();
            ResourceNotFoundResult.DefaultInnerResultFactory = () => mockResult.Object;
            ResourceNotFoundResult result = new ResourceNotFoundResult();

            // Act
            result.ExecuteResult(context);

            // Assert
            mockResult.Verify(r => r.ExecuteResult(context));
        }

        [Test]
        public void ExecuteResult_Executes_Provided_InnerResult_With_Context_If_No_InnerResult_Provided()
        {
            // Arrange
            ControllerContext context = MockHelper.CreateMockControllerContext();
            ResourceNotFoundResult.DefaultInnerResultFactory = () =>
            {
                Assert.Fail("Expected that the default inner result factory would not be used");
                return null;
            };
            var mockResult = new Mock<ActionResult>();
            ResourceNotFoundResult result = new ResourceNotFoundResult()
            {
                InnerResult = mockResult.Object
            };

            // Act
            result.ExecuteResult(context);

            // Assert
            mockResult.Verify(r => r.ExecuteResult(context));
        }
    }
}
