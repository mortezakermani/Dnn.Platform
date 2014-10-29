using System;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using Dnn.Mvc.Framework.Controllers;
using Dnn.Mvc.Framework.Modules;
using Dnn.Mvc.Utils.Entities.Users;
using Dnn.Mvc.Web.Models;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Membership;

namespace Dnn.Mvc.Web.Controllers
{
    public class AuthenticationController : DnnControllerBase
    {
        private string GetIPAddress()
        {
            string _IPAddress = String.Empty;
            if (HttpContext.Request.UserHostAddress != null)
            {
                _IPAddress = HttpContext.Request.UserHostAddress;
            }
            return _IPAddress;
        }

        [AllowAnonymous]
        public ActionResult SignIn(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new AuthenticationViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult SignIn(AuthenticationViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                string userName = new PortalSecurity().InputFilter(model.UserName,
                                                         PortalSecurity.FilterFlag.NoScripting |
                                                         PortalSecurity.FilterFlag.NoAngleBrackets |
                                                         PortalSecurity.FilterFlag.NoMarkup);

                //TODO UserControllerAdapter usage is temporary in order to make method testable
                UserLoginStatus loginStatus = UserControllerAdapter.Instance.UserLogin(ActiveSite, userName, model.Password, GetIPAddress(), model.RememberMe);

                switch (loginStatus)
                {
                    case UserLoginStatus.LOGIN_USERNOTAPPROVED:
                        return Json(new { Success = false, Error="User Not Approved" });
                    case UserLoginStatus.LOGIN_USERLOCKEDOUT:
                        return Json(new { Success = false, Error = "User Locked Out" });
                    case UserLoginStatus.LOGIN_FAILURE:
                        return Json(new { Success = false, Error = "Login Failure" });
                    default:
                        return Json(new { Success = true });
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //[ValidateAntiForgeryToken]
        public ActionResult SignOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Page");
        }
    }
}