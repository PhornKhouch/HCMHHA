using System.Web.Mvc;

namespace Humica.Controllers
{
    public class ExportingController : Controller
    {

        public ActionResult Index()
        {
            return RedirectToAction("Export");
        }

    }
}
