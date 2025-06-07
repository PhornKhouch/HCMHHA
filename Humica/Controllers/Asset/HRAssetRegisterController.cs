using Humica.Core.DB;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic;
using Humica.Logic.Asset;
using Humica.Models.SY;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.Asset
{
    public class HRAssetRegisterController : Humica.EF.Controllers.MasterSaleController

    {
        private const string SCREEN_ID = "AM00000001";
        private const string URL_SCREEN = "/Asset/HRAssetRegister/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "AssetCode;status";
        HumicaDBContext DB = new HumicaDBContext();
        public HRAssetRegisterController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
        }
        #region "List"
        public ActionResult Index()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);

            AssetRegisterObject BSM = new AssetRegisterObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                var obj = (AssetRegisterObject)Session[Index_Sess_Obj + ActionName];
            }
            BSM.ListHeader = DB.HRAssetRegisters.ToList();

            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        public ActionResult GridItemAssetRecord()
        {
            ActionName = "Create";
            UserSession();
            UserConfForm(ActionBehavior.ACC_REV);
            AssetRegisterObject BSM = new AssetRegisterObject();
            BSM.ScreenId = SCREEN_ID;
            BSM.ListAssetStaff = new List<HRAssetStaff>();
            BSM.ListHeader = new List<HRAssetRegister>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (AssetRegisterObject)Session[Index_Sess_Obj + ActionName];
            }
            //var ListAssetStaff = DB.HRAssetStaffs.ToList();
            //var ListAsserRe = DB.HRAssetRegisters.ToList();
            //ListAssetStaff = ListAssetStaff.Where(w => ListAsserRe.Where(x => x.AssetCode == w.AssetCode).Any()).ToList();
            //BSM.ListAssetStaff = ListAssetStaff;
            DataSelector();
            Session[SYConstant.CURRENT_URL] = URL_SCREEN + "GridItemAssetRecord";
            return PartialView("GridItemAssetRecord", BSM.ListAssetStaff);
        }
        public ActionResult PartialList()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            AssetRegisterObject BSM = new AssetRegisterObject();
            BSM.ListHeader = new List<HRAssetRegister>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (AssetRegisterObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView(SYListConfuration.ListDefaultView, BSM.ListHeader);
        }
        #endregion
        #region "Create"
        public ActionResult Create()
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);
            AssetRegisterObject BSM = new AssetRegisterObject();
            BSM.Header = new HRAssetRegister();
            BSM.ListAssetStaff = new List<HRAssetStaff>();
            BSM.Header.Status = SYDocumentStatus.A.ToString();
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        [HttpPost]
        public ActionResult Create(AssetRegisterObject collection)
        {
            UserSession();
            UserConfListAndForm(this.KeyName);
            DataSelector();
            ActionName = "Create";
            AssetRegisterObject BSM = new AssetRegisterObject();
            if (ModelState.IsValid)
            {
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (AssetRegisterObject)Session[Index_Sess_Obj + ActionName];
                }
                BSM.Header = collection.Header;
                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.CreateFixAsset();
                if (msg == SYConstant.OK)
                {
                    SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                    mess.DocumentNumber = BSM.Header.AssetCode.ToString();
                    mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details/" + mess.DocumentNumber;
                    ViewData[SYConstant.MESSAGE_SUBMIT] = mess;
                    BSM = new AssetRegisterObject();
                    BSM.Header = new HRAssetRegister();
                    BSM.Header.Status = SYDocumentStatus.OPEN.ToString();
                    Session[Index_Sess_Obj + ActionName] = BSM;
                    return View(BSM);
                }
                else
                {
                    ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                    Session[Index_Sess_Obj + ActionName] = BSM;
                    return View(BSM);
                }
            }
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Create");

        }
        #endregion

        #region "Edit"
        public ActionResult Edit(string ID)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = ID;
            if (ID == "null") ID = null;
            if (ID != null)
            {
                AssetRegisterObject BSM = new AssetRegisterObject();
                BSM.Header = new HRAssetRegister();
                BSM.Header = DB.HRAssetRegisters.FirstOrDefault(w => w.AssetCode == ID);
                if (BSM.Header != null)
                {
                    Session[Index_Sess_Obj + ActionName] = BSM;
                    var ListAssetStaff = DB.HRAssetStaffs.Where(w => w.Status == "RETURN" && w.AssetCode == BSM.Header.AssetCode).ToList();
                    BSM.ListAssetStaff = ListAssetStaff;
                    return View(BSM);
                }
            }
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        [HttpPost]
        public ActionResult Edit(string id, AssetRegisterObject collection)
        {
            ActionName = "Create";
            UserSession();
            this.ScreendIDControl = SCREEN_ID;
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = id;
            AssetRegisterObject BSM = new AssetRegisterObject();
            if (id != null)
            {
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (AssetRegisterObject)Session[Index_Sess_Obj + ActionName];
                    BSM.Header = collection.Header;
                }
                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.EditFixAsset(id);
                if (msg == SYConstant.OK)
                {
                    SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                    mess.DocumentNumber = id;
                    mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + mess.DocumentNumber;
                    ViewData[SYConstant.MESSAGE_SUBMIT] = mess;
                    return View(BSM);
                }
                else
                {
                    ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                    return View(BSM);
                }
            }
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            return View(BSM);

        }
        #endregion
        #region "Delete"
        public ActionResult Delete(string id)
        {
            UserSession();
            UserConfForm(SYActionBehavior.EDIT);
            DataSelector();
            if (id == "null") id = null;
            if (id != null)
            {
                AssetRegisterObject Del = new AssetRegisterObject();
                string msg = Del.DeleteFixAsset(id);
                if (msg == SYConstant.OK)
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_RM", user.Lang);
                }
                else
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                }
            }
            else
            {
                Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            }
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

        }
        #endregion

        #region "Details"
        public ActionResult Details(string id)
        {
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = id;
            if (id == "null") id = null;
            if (id != null)
            {
                AssetRegisterObject BSM = new AssetRegisterObject();
                BSM.Header = new HRAssetRegister();
                BSM.Header = DB.HRAssetRegisters.FirstOrDefault(w => w.AssetCode == id);
                if (BSM.Header != null)
                {
                    Session[Index_Sess_Obj + ActionName] = BSM;
                    var ListAssetStaff = DB.HRAssetStaffs.Where(w => w.Status == "RETURN" && w.AssetCode == BSM.Header.AssetCode).ToList();
                    BSM.ListAssetStaff = ListAssetStaff;
                    return View(BSM);
                }
            }
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        #endregion
        public ActionResult getAssetClass(string id, string Action)
        {
            ActionName = Action;
            AssetRegisterObject BSM = new AssetRegisterObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (AssetRegisterObject)Session[Index_Sess_Obj + ActionName];
                var obj = DB.HRAssetClasses.FirstOrDefault(w => w.AssetClassCode == id);
                if (obj != null)
                {
                    var result = new
                    {
                        MS = SYConstant.OK,
                        AssetType = obj.AssetTypeCode,
                    };
                    return Json(result, JsonRequestBehavior.DenyGet);

                }
            }

            var rs = new { MS = SYConstant.OK };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        public ActionResult getAssetType(string id, string Action)
        {
            ActionName = Action;
            AssetRegisterObject BSM = new AssetRegisterObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (AssetRegisterObject)Session[Index_Sess_Obj + ActionName];

                var obj = DB.HRApprTypes.FirstOrDefault(w => w.Code == id);
                if (obj != null)
                {
                    var result = new
                    {
                        MS = SYConstant.OK,
                        AssetType = obj.Code,
                    };
                    return Json(result, JsonRequestBehavior.DenyGet);

                }
            }

            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        #region 'Private Code'
        private void DataSelector()
        {
            SYDataList objList1 = new SYDataList("PROPERTYPE_SELECT");
            ViewData["PROPERTYPE_SELECT"] = objList1.ListData;

            SYDataList objList2 = new SYDataList("STATUS_SELECT");
            ViewData["STATUS_SELECT"] = objList2.ListData;
            SYDataList objList4 = new SYDataList("CONDITION_SELECT");
            ViewData["CONDITION_SELECT"] = objList4.ListData;

            ViewData["FIXASSETTYPE"] = DB.HRAssetTypes.ToList();
            ViewData["FIXASSETCLASS"] = DB.HRAssetClasses.ToList();
            ViewData["FIXED_ASSET_LIST"] = DB.HRAssetRegisters.Where(w => w.IsActive == true).ToList();
            ViewData["DEPARTMENT_SELECT"] = ClsFilter.LoadDepartment();

            var ListBranch = SYConstant.getBranchDataAccess();
            ListBranch.Add(new HRBranch());
            ViewData["BRANCH_SELECT"] = ListBranch;
        }
        #endregion
    }
}
