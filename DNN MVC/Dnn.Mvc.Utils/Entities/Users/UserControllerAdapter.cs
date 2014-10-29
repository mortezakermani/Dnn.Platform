using System;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security.Membership;

namespace Dnn.Mvc.Utils.Entities.Users
{
    public class UserControllerAdapter : ServiceLocator<IUserController, UserControllerAdapter>, IUserController
    {
        protected override Func<IUserController> GetFactory()
        {
            return () => new UserControllerAdapter();
        }

        public UserInfo GetCachedUser(int portalId, string userName)
        {
            return UserController.GetCachedUser(portalId, userName);
        }

        public UserLoginStatus UserLogin(PortalInfo portal, string username, string password, string ip, bool createPersistentCookie)
        {
            var loginStatus = UserLoginStatus.LOGIN_FAILURE;
            UserController.UserLogin(portal.PortalID, username, password, String.Empty, portal.PortalName, ip, ref loginStatus, true);
            return loginStatus;
        }
    }
}
