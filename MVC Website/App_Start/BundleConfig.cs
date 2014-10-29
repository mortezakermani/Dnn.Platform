using System.Web.Optimization;

namespace Dnn.Mvc.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Resources/Libraries/jQuery/02_01_01/jquery.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Resources/Libraries/jQuery-Validate/01_13_00/jquery.validate.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Resources/Libraries/Modernizr/02_07_02/modernizr.js*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Resources/Libraries/Bootstrap/03_02_00/bootstrap.js",
                      "~/Resources/Libraries/Respond/01_04_00/respond.js"));

            bundles.Add(new StyleBundle("~/bundles/css").Include(
                      "~/Resources/Libraries/Bootstrap/03_02_00/bootstrap.css",
                      "~/Portals/_default/site.css"));

            // Set EnableOptimizations to false for debugging. For more information,
            // visit http://go.microsoft.com/fwlink/?LinkId=301862
            BundleTable.EnableOptimizations = false;
        }
    }
}
