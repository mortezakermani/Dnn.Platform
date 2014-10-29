using System.Collections;
using System.Collections.Generic;
using Dnn.Mvc.Framework.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;

namespace Dnn.Mvc.Web.Models
{
    public class PageViewModel
    {
        public PageViewModel()
        {
            Panes = new Dictionary<string, PaneViewModel>();
        }

        public TabInfo HomePage { get; set; }

        public TabInfo Page { get; set; }

        public IList<TabInfo> Pages { get; set; }

        public IDictionary<string, PaneViewModel> Panes { get; private set; }

        public PortalInfo Site { get; set; }

        public PortalAliasInfo SiteAlias { get; set; }
        
        public void AddModuleResultToPane(ModuleRequestResult result, string paneName)
        {
            PaneViewModel pane;

            // Store the result, only the selected module can override the page
            if (!Panes.ContainsKey(paneName))
            {
                pane = new PaneViewModel { PaneName = paneName };
                Panes[paneName] = pane;
            }
            else
            {
                pane = Panes[paneName];
            }
            pane.ModuleResults.Add(result);
        }
    }
}