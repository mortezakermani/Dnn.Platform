using System.Web.Mvc;
using Dnn.Mvc.Framework.Controllers;
using Dnn.Mvc.Framework.Modules;

namespace Dnn.Mvc.Core.Tests.Fakes
{
    public class FakeDnnController : DnnController
    {
        public ActionResult Index()
        {
            return new ViewResult();
        }
    }
}
