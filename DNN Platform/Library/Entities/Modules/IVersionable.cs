using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetNuke.Entities.Modules
{
    public interface IVersionable
    {
        void SetModuleVersion(int version);
    }
}
