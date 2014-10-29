using DotNetNuke.Entities.Modules;

namespace Dnn.Mvc.Utils.Entities.Modules
{
    public interface IDesktopModuleController
    {
        DesktopModuleInfo GetDesktopModule(int desktopModuleId, int portalId);
    }
}
