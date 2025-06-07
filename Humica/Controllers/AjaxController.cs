using Humica.EF.MD;
using Humica.EF.Models.SY;
using System.Web.Mvc;

namespace Humica.Controllers
{
    public class AjaxController : Humica.EF.Controllers.MasterSaleController
    {
        SMSystemEntity DB = new SMSystemEntity();
        // GET: /Ajax/
        public string FilterMatGroup()
        {
            string result = "";

            return result;
        }

        [HttpPost]
        public string menuSession(int MenuEnable)
        {
            if (MenuEnable == 0)
            {
                Session[SYConstant.MENU_HIDE_SHOW_SESSION] = "T";
            }
            else if (MenuEnable == 1)
            {
                Session[SYConstant.MENU_HIDE_SHOW_SESSION] = "F";
            }

            return MenuEnable.ToString();
        }

        [HttpPost]
        public int SaveHeigh(int heigh)
        {
            UserSession();
            Session[SYConstant.WINDOW_HIGH] = heigh;
            return 0;
        }

        public ActionResult GetSubMenuItemJson(int HeaderMenuId)
        {
            SYMenuObject mobj = new SYMenuObject();

            mobj.GetSubMenuItem(HeaderMenuId);

            var result = new
            {
                MS = SYConstant.OK,
                AM = mobj.GetSubMenuItem(HeaderMenuId)
            };
            return Json(result, JsonRequestBehavior.DenyGet);

        }
        public ActionResult GetSubHeaderMenu3Json(int parent)
        {
            SYMenuObject mobj = new SYMenuObject();

            mobj.GetSubHeaderMenu3(parent);

            var result = new
            {
                MS = SYConstant.OK,
                AM = mobj.GetSubHeaderMenu3(parent)
            };
            return Json(result, JsonRequestBehavior.DenyGet);

        }





    }
}
