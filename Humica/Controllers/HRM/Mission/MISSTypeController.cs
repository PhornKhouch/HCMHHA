using Humica.Core.DB;
using Humica.EF;
using Humica.EF.Models.SY;
using Humica.Logic.HRS;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.HRM.Mission
{
    public class MISSTypeController : Humica.EF.Controllers.MasterSaleController

    {
        private const string SCREEN_ID = "MISS000001";
        private const string URL_SCREEN = "/HRM/Mission/MISSType";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "Code";
        private string PATH_FILE = "12313123123sadfsdfsdfsdf";
        HumicaDBContext DB = new HumicaDBContext();
        public MISSTypeController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
        }
        // GET: Branch
        public ActionResult Index()
        {
            UserSession();
            UserConfListAndForm();
            DataSelector();
            MDMISSType BSM = new MDMISSType();
            BSM.ListMissType = DB.HRMissTypes.ToList();
            BSM.ListMissItem = DB.HRMissItems.ToList();
            return View(BSM);
        }
        #region MissType

        public ActionResult GridItemMissTypes()
        {
            UserConf(ActionBehavior.EDIT);
            DataSelector();
            MDMISSType BSM = new MDMISSType();
            BSM.ListMissType = DB.HRMissTypes.ToList();
            return PartialView("GridviewMissType", BSM.ListMissType);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateMissType(HRMissType MModel)
        {
            UserSession();
            DataSelector();
            MDMISSType BSM = new MDMISSType();
            if (ModelState.IsValid)
            {
                try
                {
                    string Code = MModel.Code.ToUpper().Trim();
                    var listDivi = DB.HRMissTypes.Find(Code);
                    if (listDivi == null)
                    {
                        MModel.Code = Code;
                        DB.HRMissTypes.Add(MModel);
                        int row = DB.SaveChanges();
                    }
                    else
                    {
                        ViewData["EditError"] = SYMessages.getMessage("DUPL_KEY", user.Lang);
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
            BSM.ListMissType = DB.HRMissTypes.ToList();
            return PartialView("GridviewMissType", BSM.ListMissType);
        }
        //edit
        [HttpPost, ValidateInput(false)]
        public ActionResult EditMissType(HRMissType MModel)
        {
            UserSession();
            DataSelector();
            MDMISSType BSM = new MDMISSType();
            if (ModelState.IsValid)
            {
                try
                {
                    var DBU = new HumicaDBContext();
                    var objUpdate = DBU.HRMissTypes.Find(MModel.Code);
                    DB.HRMissTypes.Attach(MModel);
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
            BSM.ListMissType = DB.HRMissTypes.OrderBy(w => w.Code).ToList();
            return PartialView("GridviewMissType", BSM.ListMissType);
        }
        //delete
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteMissType(string Code)
        {
            UserSession();
            MDMISSType BSM = new MDMISSType();
            if (Code != null)
            {
                try
                {
                    var obj = DB.HRMissTypes.Find(Code);
                    if (obj != null)
                    {
                        DB.HRMissTypes.Remove(obj);
                        int row = DB.SaveChanges();
                    }
                    BSM.ListMissType = DB.HRMissTypes.OrderBy(w => w.Code).ToList();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }

            return PartialView("GridviewMissType", BSM.ListMissType);
        }

        #endregion

        #region MissItem
        public ActionResult GridItemMissItem()
        {
            UserConf(ActionBehavior.EDIT);
            DataSelector();
            MDMISSType BSM = new MDMISSType();
            BSM.ListMissItem = DB.HRMissItems.ToList();
            return PartialView("GridviewMissItem", BSM.ListMissItem);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateMissItem(HRMissItem MModel)
        {
            UserSession();
            DataSelector();
            MDMISSType BSM = new MDMISSType();
            if (ModelState.IsValid)
            {
                try
                {
                    MModel.Code = MModel.Code.ToUpper().Trim();
                    DB.HRMissItems.Add(MModel);
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
            BSM.ListMissItem = DB.HRMissItems.ToList();
            return PartialView("GridviewMissItem", BSM.ListMissItem);
        }
        //edit
        [HttpPost, ValidateInput(false)]
        public ActionResult EditMissItem(HRMissItem MModel)
        {
            UserSession();

            MDMISSType BSM = new MDMISSType();
            if (ModelState.IsValid)
            {
                try
                {
                    DB.HRMissItems.Attach(MModel);
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
            BSM.ListMissItem = DB.HRMissItems.OrderBy(w => w.Code).ToList();
            DataSelector();
            return PartialView("GridviewMissItem", BSM.ListMissItem);
        }
        //delete
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteMissItem(string Type, string Code)
        {
            UserSession();
            MDMISSType BSM = new MDMISSType();
            try
            {
                var objEmp = DB.HREmpCareers.ToList();
                var obj = DB.HRMissItems.Where(w => w.Type == Type && w.Code == Code).FirstOrDefault();
                if (obj != null)
                {
                    DB.HRMissItems.Remove(obj);
                    int row = DB.SaveChanges();
                }
                BSM.ListMissItem = DB.HRMissItems.OrderBy(w => w.Code).ToList();
            }
            catch (Exception e)
            {
                ViewData["EditError"] = e.Message;
            }
            return PartialView("GridviewMissItem", BSM.ListMissItem);
        }

        #endregion

        private void DataSelector()
        {
            ViewData["MissType_SELECT"] = DB.HRMissTypes.ToList();
        }
    }
}
