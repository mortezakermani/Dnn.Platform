using System;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;

namespace Dnn.Mvc.Utils.Entities.Modules
{
    public class DesktopModuleControllerAdapter : ServiceLocator<IDesktopModuleController, DesktopModuleControllerAdapter>, IDesktopModuleController
    {
        protected override Func<IDesktopModuleController> GetFactory()
        {
            return () => new DesktopModuleControllerAdapter();
        }

        public DesktopModuleInfo GetDesktopModule(int desktopModuleId, int portalId)
        {
            return DesktopModuleController.GetDesktopModule(desktopModuleId, portalId);
        }
    }
}
