using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Dnn.Mvc.Tests.Utilities;
using Dnn.Mvc.Utils.Entities.Users;
using Dnn.Mvc.Web.Controllers;
using Dnn.Mvc.Web.Models;
using Dnn.Mvc.Web.Tests.Common;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;

using Moq;

using NUnit.Framework;

using IUserController = Dnn.Mvc.Utils.Entities.Users.IUserController;

namespace Dnn.Mvc.Web.Tests.Controllers
{
    [TestFixture]
    public class AuthenticationControllerTests
    {
        [Test]
        public void SignIn_Returns_ViewResult()
        {
            //Arrange
            var controller = new AuthenticationController();

            //Act
            var result = controller.SignIn(String.Empty);

            //Asert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public void SignIn_Returns_Strongly_Typed_ViewResult()
        {
            //Arrange
            var controller = new AuthenticationController();

            //Act
            var result = controller.SignIn(String.Empty) as ViewResult;

            //Asert
            Assert.IsNotNull(result.ViewData.Model);
            Assert.IsInstanceOf<AuthenticationViewModel>(result.ViewData.Model);
        }

        [Test]
        public void SignIn_Returns_Passed_In_Parameter_In_ViewData()
        {
            //Arrange
            var controller = new AuthenticationController();

            //Act
            var result = controller.SignIn(Constants.URL_Default) as ViewResult;

            //Asert
            Assert.AreEqual(Constants.URL_Default, result.ViewData["ReturnUrl"]);
        }

        [Test]
        public void SignIn_Overload_Returns_JsonResult()
        {
            //Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            var controller = new AuthenticationController();
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);

            var mockUserController = new Mock<IUserController>();
            mockUserController.Setup(u => u.UserLogin(It.IsAny<PortalInfo>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                        .Returns(UserLoginStatus.LOGIN_SUCCESS);

            UserControllerAdapter.SetTestableInstance(mockUserController.Object);

            //Act
            var result = controller.SignIn(new AuthenticationViewModel() { UserName = "jdoe", Password="1234", RememberMe = false}, String.Empty);

            //Asert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<JsonResult>(result);
        }

        [TestCase(UserLoginStatus.LOGIN_SUCCESS, true, "")]
        [TestCase(UserLoginStatus.LOGIN_FAILURE, false, "Login Failure")]
        [TestCase(UserLoginStatus.LOGIN_USERNOTAPPROVED, false, "User Not Approved")]
        [TestCase(UserLoginStatus.LOGIN_USERLOCKEDOUT, false, "User Locked Out")]
        public void SignIn_Overload_Returns_Correct_JsonResult(UserLoginStatus loginStatus, bool success, string message)
        {
            //Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            var controller = new AuthenticationController();
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);

            var mockUserController = new Mock<IUserController>();
            mockUserController.Setup(u => u.UserLogin(It.IsAny<PortalInfo>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                        .Returns(loginStatus);

            UserControllerAdapter.SetTestableInstance(mockUserController.Object);

            //Act
            var result = controller.SignIn(new AuthenticationViewModel() { UserName = "jdoe", Password = "1234", RememberMe = false }, String.Empty) as JsonResult;


            //Asert
            IDictionary<string, object> wrapper = new RouteValueDictionary(result.Data);
            Assert.AreEqual(success, wrapper["Success"]);
            if (!success)
            {
                Assert.AreEqual(message, wrapper["Error"]);
            }
        }
    }
}
