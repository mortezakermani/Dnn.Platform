using System.Web;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;

namespace Dnn.Mvc.Utils.Entities.Portals
{
    public interface IPortalController
    {
        string GetPortalSetting(int portalId, string settingName, string defaultValue);

        void CreatePortalSettings(TabInfo page, PortalAliasInfo siteAlias, HttpContextBase httpContext);
    }
}
