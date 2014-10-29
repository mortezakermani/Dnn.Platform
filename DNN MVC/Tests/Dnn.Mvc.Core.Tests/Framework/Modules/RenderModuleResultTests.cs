using System;
using System.Web.Mvc;
using Dnn.Mvc.Framework;
using Dnn.Mvc.Framework.ActionResults;
using Dnn.Mvc.Framework.Modules;
using Dnn.Mvc.Tests.Utilities;
using DotNetNuke.ComponentModel;
using Moq;
using NUnit.Framework;

namespace Dnn.Mvc.Core.Tests.Framework.Modules
{
    [TestFixture]
    public class RenderModuleResultTests
    {
        [SetUp]
        public void SetUp()
        {
            ComponentFactory.Container= new SimpleContainer();  
        }

        [Test]
        public void ExecuteResult_Throws_On_Null_Context()
        {
            //Arrange
            ControllerContext context = null;
            var result= new RenderModuleResult();

            //Act,Assert
            Assert.Throws<ArgumentNullException>(() => result.ExecuteResult(context));
        }

        [Test]
        public void ExecuteResult_Does_Not_Call_ModuleExecutionEngine_On_Null_ModuleRequestResult()
        {
            //Arrange
            ControllerContext context = MockHelper.CreateMockControllerContext();
            var result = new RenderModuleResult();

            var mockEngine = new Mock<IModuleExecutionEngine>();
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(mockEngine.Object);

            //Act
            result.ExecuteResult(context);

            //Assert
            mockEngine.Verify(e => e.ExecuteModuleResult(It.IsAny<SiteContext>(), It.IsAny<ModuleRequestResult>()), Times.Never);
        }

        [Test]
        public void ExecuteResult_Calls_ModuleExecutionEngine_On_ModuleRequestResult()
        {
            //Arrange
            ControllerContext context = MockHelper.CreateMockControllerContext();
            var result = new RenderModuleResult();
            result.ModuleRequestResult = new ModuleRequestResult();

            var mockEngine = new Mock<IModuleExecutionEngine>();
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(mockEngine.Object);

            //Act
            result.ExecuteResult(context);

            //Assert
            mockEngine.Verify(e => e.ExecuteModuleResult(It.IsAny<SiteContext>(), It.IsAny<ModuleRequestResult>()), Times.Once);
        }
    }
}
