using Humica.Core.DB;
using Humica.EF;
using Humica.EF.Models.SY;
using Humica.Logic.PR;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.PR.PRS
{
    public class TaxSettingController : Humica.EF.Controllers.MasterSaleController

    {
        private const string SCREEN_ID = "PRS0000005";
        private const string URL_SCREEN = "/PR/PRS/TaxSetting";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "TranNo";
        HumicaDBContext DB = new HumicaDBContext();

        public TaxSettingController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
        }

        public ActionResult Index()
        {
            UserSession();
            UserConfListAndForm();
            PRSTaxSetting BSM = new PRSTaxSetting();
            BSM.ListHeader = DB.PRTaxSettings.ToList();
            return View(BSM);
        }
        public ActionResult GridItems()
        {
            UserConf(ActionBehavior.EDIT);

            PRSTaxSetting BSM = new PRSTaxSetting();
            BSM.ListHeader = DB.PRTaxSettings.ToList().OrderBy(w => w.TaxFrom).ToList();
            return PartialView("Gridview", BSM.ListHeader);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Create(PRTaxSetting MModel)
        {
            UserSession();
            PRSTaxSetting BSM = new PRSTaxSetting();
            if (ModelState.IsValid)
            {
                try
                {
                    //  MModel.Code = MModel.Code.ToUpper();
                    DB.PRTaxSettings.Add(MModel);
                    int row = DB.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
            {
                ViewData["EditError"] = SYMessages.getMessage("EE001", user.Lang);
            }
            BSM.ListHeader = DB.PRTaxSettings.ToList();
            return PartialView("Gridview", BSM.ListHeader);
        }
        //edit
        [HttpPost, ValidateInput(false)]
        public ActionResult Edit(PRTaxSetting MModel)
        {
            UserSession();
            PRSTaxSetting BSM = new PRSTaxSetting();
            if (ModelState.IsValid)
            {
                try
                {
                    var DBU = new HumicaDBContext();
                    var ListTaxSetting = DBU.PRTaxSettings.ToList();
                    foreach (var item in ListTaxSetting.ToList().OrderBy(w => w.TaxPercent))
                    {
                        if (item.TranNo == MModel.TranNo)
                        {

                        }

                    }
                    var objUpdate = DBU.PRTaxSettings.Find(MModel.TranNo);
                    DB.PRTaxSettings.Attach(MModel);
                    DB.Entry(MModel).State = System.Data.Entity.EntityState.Modified;
                    int row = DB.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
            {
                ViewData["EditError"] = SYMessages.getMessage("EE001", user.Lang);
            }
            BSM.ListHeader = DB.PRTaxSettings.OrderBy(w => w.TaxFrom).ToList();
            return PartialView("Gridview", BSM.ListHeader);
        }
        //delete
        [HttpPost, ValidateInput(false)]
        public ActionResult Delete(int TranNo)
        {
            UserSession();
            PRSTaxSetting BSM = new PRSTaxSetting();
            if (TranNo != null)
            {
                try
                {
                    var obj = DB.PRTaxSettings.Find(TranNo);
                    if (obj != null)
                    {
                        DB.PRTaxSettings.Remove(obj);
                        int row = DB.SaveChanges();
                    }

                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            BSM.ListHeader = DB.PRTaxSettings.OrderBy(w => w.TaxFrom).ToList();
            return PartialView("Gridview", BSM.ListHeader);
        }
    }
}
