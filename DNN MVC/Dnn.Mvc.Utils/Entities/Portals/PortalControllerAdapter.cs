using System;
using System.Web;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;

namespace Dnn.Mvc.Utils.Entities.Portals
{
    public class PortalControllerAdapter : ServiceLocator<IPortalController, PortalControllerAdapter>, IPortalController
    {
        protected override Func<IPortalController> GetFactory()
        {
            return () => new PortalControllerAdapter();
        }

        public string GetPortalSetting(int portalId, string settingName, string defaultValue)
        {
            return PortalController.GetPortalSetting(settingName, portalId, defaultValue);
        }

        public void CreatePortalSettings(TabInfo page, PortalAliasInfo siteAlias, HttpContextBase httpContext)
        {
            var portalSettings = new PortalSettings(page.TabID, siteAlias);
            httpContext.Items.Add("PortalSettings", portalSettings);
        }
    }
}
