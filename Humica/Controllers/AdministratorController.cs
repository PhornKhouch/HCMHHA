using System.Web.Mvc;

namespace Humica.Controllers
{
    public class AdministratorController : Humica.EF.Controllers.MasterSaleController
    {
        //
        // GET: /Administrator/


        [ChildActionOnly]
        public ActionResult Menu()
        {
            //return View(new MenusData());               
            return View();
        }
    }
}
