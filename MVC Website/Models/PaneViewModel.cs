using System.Collections.Generic;
using System.Web.Optimization;
using Dnn.Mvc.Framework.Modules;

namespace Dnn.Mvc.Web.Models
{
    public class PaneViewModel
    {
        public PaneViewModel()
        {
            ModuleResults = new List<ModuleRequestResult>();
        }

        public IList<ModuleRequestResult> ModuleResults { get; set; }

        public string PaneName { get; set; }
    }
}