using Humica.Core.DB;
using Humica.EF;
using Humica.EF.Models.SY;
using Humica.Logic.PR;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.WH.OP
{
    public class PRNSSFSettingController : Humica.EF.Controllers.MasterSaleController
    {
        private const string SCREEN_ID = "PRS0000003";
        private const string URL_SCREEN = "/PR/PRS/PRNSSFSetting/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "ID";
        HumicaDBContext DB = new HumicaDBContext();

        public PRNSSFSettingController()
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

            PRSNSSFSetting BSM = new PRSNSSFSetting();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                var obj = (PRSNSSFSetting)Session[Index_Sess_Obj + ActionName];
            }

            BSM.ListHeader = DB.PRSocsecs.ToList();

            Session[Index_Sess_Obj + ActionName] = BSM;

            return View(BSM);
        }
        #region NSSF
        public ActionResult NSSF()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm();
            PRSNSSFSetting BSM = new PRSNSSFSetting();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (PRSNSSFSetting)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("NSSF", BSM.ListHeader);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult CreateItem(PRSocsec MModel)
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm();
            PRSNSSFSetting BSM = new PRSNSSFSetting();
            if (ModelState.IsValid)
            {
                try
                {
                    var listPH = DB.PRSocsecs.ToList();
                    if (listPH.Where(w => w.ID == MModel.ID).ToList().Count() == 0)
                    {
                        MModel.CreateOn = DateTime.Now.Date;
                        MModel.CreateBy = user.UserName;
                        DB.PRSocsecs.Add(MModel);
                        int row = DB.SaveChanges();
                    }
                    else
                    {
                        ViewData["EditError"] = SYMessages.getMessage("PUB_EN");
                    }

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
            BSM.ListHeader = DB.PRSocsecs.OrderByDescending(w => w.ID).ToList();
            return PartialView("NSSF", BSM.ListHeader);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult EditItem(PRSocsec ModelObject)
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm();
            PRSNSSFSetting BSM = new PRSNSSFSetting();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (PRSNSSFSetting)Session[Index_Sess_Obj + ActionName];
                    }
                    var DBU = new HumicaDBContext();
                    var ListHeader = BSM.ListHeader.Where(w => w.ID == ModelObject.ID).ToList();
                    if (ListHeader.Count > 0)
                    {
                        var objUpdate = ListHeader.First();
                        objUpdate.Grwagef = ModelObject.Grwagef;
                        objUpdate.GrwaGet = ModelObject.GrwaGet;
                        objUpdate.AvgGr = ModelObject.AvgGr;
                        objUpdate.ContriBut = ModelObject.ContriBut;
                        objUpdate.HealthComp = ModelObject.HealthComp;
                        objUpdate.HealthStaff = ModelObject.HealthStaff;
                        objUpdate.ChangedOn = DateTime.Now.Date;
                        objUpdate.ChangedBy = user.UserName;
                        DB.PRSocsecs.Attach(objUpdate);
                        DB.Entry(objUpdate).State = System.Data.Entity.EntityState.Modified;
                        int row = DB.SaveChanges();
                    }
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
            return PartialView("NSSF", BSM.ListHeader);
        }
        //delete
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteItem(int ID)
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm();
            PRSNSSFSetting BSM = new PRSNSSFSetting();
            if (ID != 0)
            {
                try
                {
                    var listPH = DB.PRSocsecs.ToList();
                    var objUpdate = listPH.FirstOrDefault(w => w.ID == ID);
                    if (objUpdate != null)
                    {
                        DB.PRSocsecs.Remove(objUpdate);
                        int row = DB.SaveChanges();
                    }
                    BSM.ListHeader = DB.PRSocsecs.OrderByDescending(w => w.ID).ToList();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }

            return PartialView("NSSF", BSM.ListHeader);
        }
        #endregion
    }
}