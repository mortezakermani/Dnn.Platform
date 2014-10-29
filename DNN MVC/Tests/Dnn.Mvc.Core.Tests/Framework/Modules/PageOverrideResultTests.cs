using System;
using System.Web.Mvc;
using Dnn.Mvc.Framework.ActionResults;
using Dnn.Mvc.Framework.Modules;
using Dnn.Mvc.Tests.Utilities;
using Moq;
using NUnit.Framework;

namespace Dnn.Mvc.Core.Tests.Framework.Modules
{
    [TestFixture]
    public class PageOverrideResultTests
    {
        [Test]
        public void Constructor_Throws_On_Null_ActionResult()
        {
            //Arrange
            ActionResult innerResult = null;

            //Act, Assert
            Assert.Throws<ArgumentNullException>(() => new PageOverrideResult(innerResult));
        }

        [Test]
        public void Constructor_Sets_InnerResult_On_NonNull_ActionResult()
        {
            //Arrange
            var mockInnerResult = new Mock<ActionResult>();

            //Act
            var result = new PageOverrideResult(mockInnerResult.Object);

            //Assert
            Assert.AreSame(mockInnerResult.Object, result.InnerResult);
        }

        [Test]
        public void ExecuteResult_Throws_On_Null_ControllerContext()
        {
            //Arrange
            ControllerContext context = null;
            var mockInnerResult = new Mock<ActionResult>();

            //Act
            var result = new PageOverrideResult(mockInnerResult.Object);
            
            //Assert
            Assert.Throws<ArgumentNullException>(() => result.ExecuteResult(context));
        }

        [Test]
        public void ExecuteResult_Calls_InnerResults_ExecuteResult_Method()
        {
            //Arrange
            ControllerContext context = MockHelper.CreateMockControllerContext();
            var mockInnerResult = new Mock<ActionResult>();

            //Act
            var result = new PageOverrideResult(mockInnerResult.Object);
            result.ExecuteResult(context);

            //Assert
            mockInnerResult.Verify(a => a.ExecuteResult(context));
        }
    }
}
