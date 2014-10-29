using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Dnn.Mvc.Core.Tests.Fakes;
using Dnn.Mvc.Framework;
using Dnn.Mvc.Framework.Modules;
using Dnn.Mvc.Helpers;
using Dnn.Mvc.Tests.Utilities;
using NUnit.Framework;

namespace Dnn.Mvc.Web.Tests.Framework.Modules
{
    [TestFixture]
    public class ResultCapturingActionInvokerTests
    {
        [Test]
        public void InvokeActionResult_Sets_ResultOfLastInvoke()
        {
            //Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            context.SetSiteContext(new SiteContext(context));

            var controller = new FakeController();
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);
            
            var actionInvoker = new ResultCapturingActionInvoker();

            //Act
            actionInvoker.InvokeAction(controller.ControllerContext, "Index");

            //Assert
            Assert.IsNotNull(actionInvoker.ResultOfLastInvoke);
            Assert.IsInstanceOf<ViewResult>(actionInvoker.ResultOfLastInvoke);
        }
    }
}
