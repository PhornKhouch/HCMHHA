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
    public class PRBIExchangeRatesController : Humica.EF.Controllers.MasterSaleController
    {
        private const string SCREEN_ID = "PRM0000031";
        private const string URL_SCREEN = "/PR/PRM/PRBIExchangeRates/";
        private string KeyName = "TranNo";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName = "";
        HumicaDBContext DB = new HumicaDBContext();
        public PRBIExchangeRatesController()
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
            BSM.ListBiHeader = new List<PRBiExchangeRate>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                var obj = (PRMExchangeRate)Session[Index_Sess_Obj + ActionName];
            }
            var ListBiHeader = DB.PRBiExchangeRates.ToList();
            BSM.ListBiHeader = ListBiHeader.OrderByDescending(x => x.InYear).ThenByDescending(w => w.InMonth).ToList();

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
            BSM.ListBiHeader = DB.PRBiExchangeRates.ToList();
            return PartialView("GridItems", BSM.ListBiHeader.OrderByDescending(x => x.InYear).ThenByDescending(w => w.InMonth).ToList());
        }
        [HttpPost]
        public ActionResult Create(PRBiExchangeRate ModelObject)
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            PRMExchangeRate BSM = new PRMExchangeRate();
            if (ModelState.IsValid)
            {
                if (DB.PRBiExchangeRates.Any(p => p.InYear == ModelObject.InYear && p.InMonth == ModelObject.InMonth))
                {
                    ViewData["EditError"] = SYMessages.getMessage("RECORD_EXIST", user.Lang);
                }
                else
                {
                    ModelObject.CreateOn = DateTime.Now;
                    ModelObject.CreateBy = user.UserName;
                    DB.PRBiExchangeRates.Add(ModelObject);
                    DB.SaveChanges();
                }
            }
            BSM.ListBiHeader = DB.PRBiExchangeRates.ToList();
            return PartialView("GridItems", BSM.ListBiHeader.OrderByDescending(x => x.InYear).ThenByDescending(w => w.InMonth).ToList());
        }
        [HttpPost]
        public ActionResult Edit(PRBiExchangeRate ModelObject)
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
                    var ObjMatch = DB.PRBiExchangeRates.Find(ModelObject.TranNo);
                    ObjMatch.Rate = ModelObject.Rate;
                    ObjMatch.NSSFRate = ModelObject.NSSFRate;
                    ObjMatch.ChangedOn = DateTime.Now;
                    ObjMatch.ChangedBy = user.UserName;
                    DB.PRBiExchangeRates.Attach(ObjMatch);
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

            BSM.ListBiHeader = DB.PRBiExchangeRates.ToList();
            return PartialView("GridItems", BSM.ListBiHeader.OrderByDescending(x => x.InYear).ThenByDescending(w => w.InMonth).ToList());
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
                    var obj = DB.PRBiExchangeRates.FirstOrDefault(w => w.TranNo == TranNo);
                    if (obj != null)
                    {
                        DB.PRBiExchangeRates.Remove(obj);
                        int row = DB.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            BSM.ListBiHeader = DB.PRBiExchangeRates.ToList();
            return PartialView("GridItems", BSM.ListBiHeader.OrderByDescending(x => x.InYear).ThenByDescending(w => w.InMonth).ToList());
        }
        private void DataSelector()
        {
            //SYDataList objList = new SYDataList("EXCHRATE");
            //ViewData["RATE_SELECT"] = objList.ListData;

        }
    }
}