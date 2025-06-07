using Humica.Core.DB;
using Humica.EF;
using Humica.EF.Models.SY;
using Humica.Logic.PR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace HR.Controllers.PR.PRM
{
    public class PRExchangeRatesController : Humica.EF.Controllers.MasterSaleController
    {
        private const string SCREEN_ID = "PRM0000005";
        private const string URL_SCREEN = "/PR/PRM/PRExchangeRates/";
        private string KeyName = "TranNo";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName = "";
        HumicaDBContext DB = new HumicaDBContext();
        public PRExchangeRatesController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
        }
        public ActionResult Index()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            PRMExchangeRate BSM = new PRMExchangeRate();
            BSM.ListHeader = new List<PRExchRate>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                var obj = (PRMExchangeRate)Session[Index_Sess_Obj + ActionName];
            }
            var LIstHeader = DB.PRExchRates.ToList();
            BSM.ListHeader = LIstHeader.OrderByDescending(x => x.InYear).ThenByDescending(w => w.InMonth).ToList();

            Session[Index_Sess_Obj + ActionName] = BSM;

            return View(BSM);
        }
        public ActionResult GridItems()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            PRMExchangeRate BSM = new PRMExchangeRate();
            BSM.ListHeader = DB.PRExchRates.ToList();
            return PartialView("GridItems", BSM.ListHeader.OrderByDescending(x => x.InYear).ThenByDescending(w => w.InMonth).ToList());
        }
        [HttpPost]
        public ActionResult Create(PRExchRate ModelObject)
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            PRMExchangeRate BSM = new PRMExchangeRate();
            if (ModelState.IsValid)
            {
                if (DB.PRExchRates.Any(p => p.InYear == ModelObject.InYear && p.InMonth == ModelObject.InMonth))
                {
                    ViewData["EditError"] = SYMessages.getMessage("RECORD_EXIST", user.Lang);
                }
                else
                {
                    ModelObject.CreateOn = DateTime.Now;
                    ModelObject.CreateBy = user.UserName;
                    DB.PRExchRates.Add(ModelObject);
                    DB.SaveChanges();
                }
            }
            BSM.ListHeader = DB.PRExchRates.ToList();
            return PartialView("GridItems", BSM.ListHeader.OrderByDescending(x => x.InYear).ThenByDescending(w => w.InMonth).ToList());
        }
        [HttpPost]
        public ActionResult Edit(PRExchRate ModelObject)
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            PRMExchangeRate BSM = new PRMExchangeRate();
            if (ModelState.IsValid)
            {
                try
                {
                    var ObjMatch = DB.PRExchRates.Find(ModelObject.TranNo);
                    ObjMatch.Rate = ModelObject.Rate;
                    ObjMatch.NSSFRate = ModelObject.NSSFRate;
                    ObjMatch.ChangedOn = DateTime.Now;
                    ObjMatch.ChangedBy = user.UserName;
                    DB.PRExchRates.Attach(ObjMatch);
                    DB.Entry(ObjMatch).Property(w => w.Rate).IsModified = true;
                    DB.Entry(ObjMatch).Property(w => w.NSSFRate).IsModified = true;
                    DB.Entry(ObjMatch).Property(w => w.ChangedBy).IsModified = true;
                    DB.Entry(ObjMatch).Property(w => w.ChangedOn).IsModified = true;
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

            BSM.ListHeader = DB.PRExchRates.ToList();
            return PartialView("GridItems", BSM.ListHeader.OrderByDescending(x => x.InYear).ThenByDescending(w => w.InMonth).ToList());
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Delete(int TranNo)
        {
            UserSession();
            UserConfListAndForm();
            DataSelector();
            PRMExchangeRate BSM = new PRMExchangeRate();
            if (TranNo != null)
            {
                try
                {
                    var obj = DB.PRExchRates.FirstOrDefault(w => w.TranNo == TranNo);
                    if (obj != null)
                    {
                        DB.PRExchRates.Remove(obj);
                        int row = DB.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            BSM.ListHeader = DB.PRExchRates.ToList();
            return PartialView("GridItems", BSM.ListHeader.OrderByDescending(x => x.InYear).ThenByDescending(w => w.InMonth).ToList());
        }
        private void DataSelector()
        {
            SYDataList objList = new SYDataList("EXCHRATE");
            ViewData["RATE_SELECT"] = objList.ListData;

        }
    }
}