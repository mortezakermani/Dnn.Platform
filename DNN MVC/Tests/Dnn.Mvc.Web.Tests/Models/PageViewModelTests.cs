using Dnn.Mvc.Framework.Modules;
using Dnn.Mvc.Web.Models;
using NUnit.Framework;

namespace Dnn.Mvc.Web.Tests.Models
{
    [TestFixture]
    public class PageViewModelTests
    {
        [Test]
        public void Constructor_Initializes_Panes_Property()
        {
            Assert.IsNotNull(new PageViewModel().Panes);
        }

        [Test]
        public void AddModuleResultToPane_Creates_New_Pane_If_Pane_Does_Not_Exist()
        {
            //Arrange
            var page = new PageViewModel();

            //Act
            page.AddModuleResultToPane(new ModuleRequestResult(), "foo");

            //Assert
            Assert.IsNotNull(page.Panes["foo"]);
        }

        [Test]
        public void AddModuleResultToPane_Adds_To_Existing_Pane_If_Pane_Does_Exist()
        {
            //Arrange
            var page = new PageViewModel();
            var expected = new PaneViewModel() { PaneName = "foo" };
            page.Panes.Add("foo", expected);

            //Act
            page.AddModuleResultToPane(new ModuleRequestResult(), "foo");

            // Act
            var actual = page.Panes["foo"];

            // Assert
            Assert.AreSame(expected, actual);
        }

        //[Test]
        //public void AllModules_Returns_Aggregation_Of_All_Modules_In_All_Zones()
        //{
        //    // Arrange
        //    PageViewModel model = SetupAllModulesTestModel();

        //    // Act
        //    IEnumerable<ModuleRequestResult> moduleResults = model.AllModules;

        //    // Assert
        //    Assert.AreEqual(5, moduleResults.Count());
        //    EnumerableAssert.ElementsMatch(model.Zones.SelectMany(z => z.ModuleResults),
        //                                   moduleResults,
        //                                   ReferenceEquals);
        //}

        //[Test]
        //public void AllModules_Returns_ControlPanelModule_First_If_Present()
        //{
        //    // Arrange
        //    PageViewModel model = SetupAllModulesTestModel();
        //    model.ControlPanelResult = new ModuleRequestResult();

        //    // Act
        //    IEnumerable<ModuleRequestResult> moduleResults = model.AllModules;

        //    // Assert
        //    Assert.AreEqual(6, moduleResults.Count());
        //    Assert.AreSame(model.ControlPanelResult, moduleResults.First());
        //}

        //private static PageViewModel SetupAllModulesTestModel()
        //{
        //    var model = new PageViewModel();
        //    var pane1 = new PaneViewModel() {PaneName="Pane1"};
        //    pane1.ModuleResults.Add(new ModuleRequestResult());
        //    pane1.ModuleResults.Add(new ModuleRequestResult());
        //    pane1.ModuleResults.Add(new ModuleRequestResult());
        //    var pane2 = new PaneViewModel() {PaneName="Pane2"};
        //    pane2.ModuleResults.Add(new ModuleRequestResult());
        //    pane2.ModuleResults.Add(new ModuleRequestResult());
        //    model.Panes.Add("Pane1", pane1);
        //    model.Panes.Add("Pane2", pane2);
        //    return model;
        //}
    }
}
