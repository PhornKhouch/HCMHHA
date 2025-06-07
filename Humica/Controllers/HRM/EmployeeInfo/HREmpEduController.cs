using DevExpress.Web.Mvc;
using Humica.Core.DB;
using Humica.EF;
using Humica.EF.Models.SY;
using Humica.Logic.HR;
using Humica.Models.SY;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.HRM.EmployeeInfo
{
    public class HREmpEduController : Humica.EF.Controllers.MasterSaleController
    {
        private const string SCREEN_ID = "HRE0000004";
        private const string URL_SCREEN = "/HRM/EmployeeInfo/HREmpEdu/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "TranNo";
        private string PATH_FILE = "12313123123sadfsdfsdfsdf";
        HumicaDBContext DB = new HumicaDBContext();
        HumicaDBViewContext DBV = new HumicaDBViewContext();
        public HREmpEduController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
        }
        #region List
        public ActionResult Index()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            HREmpEduObject BSM = new HREmpEduObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                var obj = (HREmpEduObject)Session[Index_Sess_Obj + ActionName];
            }
            BSM.ListEducation = BSM.LoadData();
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        public ActionResult PartialList()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            HREmpEduObject BSM = new HREmpEduObject();
            BSM.ListStaffView = new List<HR_STAFF_VIEW>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HREmpEduObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView(SYListConfuration.ListDefaultView, BSM.ListEducation);
        }
        public ActionResult GridItems()
        {
            ActionName = "Create";
            UserSession();
            UserConfForm(ActionBehavior.ACC_REV);
            HREmpEduObject BSM = new HREmpEduObject();
            BSM.ScreenId = SCREEN_ID;
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HREmpEduObject)Session[Index_Sess_Obj + ActionName];
            }
            //BSM.ListHeader = new List<HR_Family_View>();
            Session[SYConstant.CURRENT_URL] = URL_SCREEN + "GridItems";
            return PartialView("GridItems", BSM);
        }
        #endregion
        #region Create
        public ActionResult Create()
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);
            HREmpEduObject BSM = new HREmpEduObject();
            BSM.ListHeader = new List<HREmpEduc>();
            BSM.Header = new HREmpEduc();
            BSM.Header.StartDate = DateTime.Now;
            BSM.Header.EndDate = DateTime.Now;
            BSM.HeaderStaff = new HR_STAFF_VIEW();
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        [HttpPost]
        public ActionResult Create(HREmpEduObject collection)
        {
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);
            ActionName = "Create";
            HREmpEduObject BSM = new HREmpEduObject();
            if (ModelState.IsValid)
            {
                if (Session[PATH_FILE] != null)
                {
                    collection.Header.AttachFile = Session[PATH_FILE].ToString();
                    Session[PATH_FILE] = null;
                }
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (HREmpEduObject)Session[Index_Sess_Obj + ActionName];
                }
                BSM.Header = collection.Header;
                BSM.HeaderStaff = collection.HeaderStaff;
                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.CreateEmpEdu();
                if (msg == SYConstant.OK)
                {
                    SYMessages mess_err = SYMessages.getMessageObject("MS001", user.Lang);
                    mess_err.DocumentNumber = BSM.Header.TranNo.ToString();
                    mess_err.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + mess_err.DocumentNumber;
                    BSM.Header = new HREmpEduc();
                    BSM.Header.StartDate = DateTime.Now;
                    BSM.Header.EndDate = DateTime.Now;
                    BSM.HeaderStaff = new HR_STAFF_VIEW();
                    Session[Index_Sess_Obj + this.ActionName] = BSM;
                    UserConfForm(ActionBehavior.SAVEGRID);
                    Session[SYConstant.MESSAGE_SUBMIT] = mess_err;
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
        #region Edit
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
                HREmpEduObject BSM = new HREmpEduObject();
                BSM.Header = new HREmpEduc();
                BSM.HeaderStaff = new HR_STAFF_VIEW();
                int TranNo = Convert.ToInt32(ID);
                BSM.Header = DB.HREmpEducs.FirstOrDefault(w => w.TranNo == TranNo);
                var resualt = DB.HREmpEducs;
                if (BSM.Header != null)
                {
                    List<HREmpEduc> listEmpfa = resualt.Where(x => x.EmpCode == BSM.Header.EmpCode).ToList();
                    BSM.ListHeader = listEmpfa.ToList();
                    BSM.HeaderStaff = DBV.HR_STAFF_VIEW.FirstOrDefault(w => w.EmpCode == BSM.Header.EmpCode);
                    Session[Index_Sess_Obj + ActionName] = BSM;
                    return View(BSM);
                }
            }
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        [HttpPost]
        public ActionResult Edit(string id, HREmpEduObject collection)
        {
            ActionName = "Create";
            UserSession();
            this.ScreendIDControl = SCREEN_ID;
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = id;
            HREmpEduObject BSM = new HREmpEduObject();

            if (id != null)
            {
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (HREmpEduObject)Session[Index_Sess_Obj + ActionName];
                }
                BSM.Header = collection.Header;
                if (Session[PATH_FILE] != null)
                {
                    collection.Header.AttachFile = Session[PATH_FILE].ToString();
                    Session[PATH_FILE] = null;
                }
                else
                {
                    collection.Header.AttachFile = BSM.Header.AttachFile;
                }
                BSM.ScreenId = SCREEN_ID;
                int TranNo = Convert.ToInt32(id);
                string msg = BSM.EditEmpEdu(TranNo);
                if (msg == SYConstant.OK)
                {
                    DB = new HumicaDBContext();
                    SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                    mess.DocumentNumber = id;
                    mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + mess.DocumentNumber;
                    ViewData[SYConstant.MESSAGE_SUBMIT] = mess;
                    // BSM.ListHeader = DB.HREmpFamilies.ToList();

                    return View(BSM);
                }
                else
                {
                    BSM.ListHeader = DB.HREmpEducs.ToList();
                    ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                    return View(BSM);
                }
            }
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            return View(BSM);

        }
        #endregion
        #region Delete
        public ActionResult Delete(string id)
        {
            UserSession();
            UserConfForm(SYActionBehavior.EDIT);
            DataSelector();
            if (id == "null") id = null;
            if (id != null)
            {
                int TranNo = Convert.ToInt32(id);
                HREmpEduObject Del = new HREmpEduObject();
                string msg = Del.DeleteEmpEdu(TranNo);
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
        #region View
        public ActionResult Details(string id)
        {
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = id;
            if (id == "null") id = null;
            if (id != null)
            {
                HREmpEduObject BSM = new HREmpEduObject();
                BSM.Header = new HREmpEduc();
                BSM.HeaderStaff = new HR_STAFF_VIEW();
                int TranNo = Convert.ToInt32(id);
                BSM.Header = DB.HREmpEducs.FirstOrDefault(w => w.TranNo == TranNo);
                var resualt = DB.HREmpEducs;
                if (BSM.Header != null)
                {
                    List<HREmpEduc> listEmpfa = resualt.Where(x => x.EmpCode == BSM.Header.EmpCode).ToList();
                    BSM.ListHeader = listEmpfa.ToList();
                    BSM.HeaderStaff = DBV.HR_STAFF_VIEW.FirstOrDefault(w => w.EmpCode == BSM.Header.EmpCode);
                    Session[Index_Sess_Obj + ActionName] = BSM;
                    return View(BSM);
                }
            }
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        #endregion
        #region Upload
        [HttpPost]
        public ActionResult UploadControlCallbackActionImage()
        {
            UserSession();

            if (Session[SYSConstant.IMG_SESSION_KEY_1] != null)
            {
                //DeleteFile(Session[SYSConstant.IMG_SESSION_KEY_1].ToString());
            }

            var path = DB.CFUploadPaths.Find("IMG_UPLOAD");
            var objFile = new SYFileImportImage(path);
            objFile.TokenKey = ClsCrypo.GetUniqueKey(15);

            objFile.ObjectTemplate = new MDUploadImage();
            objFile.ObjectTemplate.ScreenId = SCREEN_ID;
            objFile.ObjectTemplate.Module = "MASTER";
            objFile.ObjectTemplate.TokenCode = objFile.TokenKey;
            objFile.ObjectTemplate.UploadBy = user.UserName;

            Session[SYSConstant.IMG_SESSION_KEY_1] = objFile.TokenKey;
            UploadControlExtension.GetUploadedFiles("UploadControl", objFile.ValidationSettings, objFile.uc_FileUploadComplete);
            Session[PATH_FILE] = objFile.ObjectTemplate.UpoadPath;
            return null;
        }
        #endregion 'Upload'
        public ActionResult ShowDataEmp(string ID, string EmpCode)
        {

            ActionName = "Details";
            var Stafff = DBV.HR_STAFF_VIEW;
            HR_STAFF_VIEW EmpStaff = Stafff.FirstOrDefault(w => w.EmpCode == EmpCode);
            if (EmpStaff != null)
            {
                var result = new
                {
                    MS = SYConstant.OK,
                    AllName = EmpStaff.AllName,
                    EmpType = EmpStaff.EmpType,
                    Division = EmpStaff.DivisionDesc,
                    DEPT = EmpStaff.Department,
                    SECT = EmpStaff.Section,
                    LevelCode = EmpStaff.LevelCode,
                    Position = EmpStaff.Position,
                    StartDate = EmpStaff.StartDate
                };
                GetData(EmpCode, "Create");
                return Json(result, JsonRequestBehavior.DenyGet);
            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        public string GetData(string EmpCode, string Action)
        {
            this.ActionName = Action;
            UserSession();
            UserConfListAndForm();
            HREmpEduObject BSM = new HREmpEduObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                var resualt = DB.HREmpEducs;
                List<HREmpEduc> listEmpFa = resualt.Where(x => x.EmpCode == EmpCode).ToList();
                BSM.ListHeader = listEmpFa.OrderByDescending(x => x.TranNo).ToList();
                //int tranNo = Convert.ToInt32(listEmpFa.Max(w => w.TranNo));
                // BSM.IsValidChecking(tranNo);
                Session[Index_Sess_Obj + ActionName] = BSM;
                return SYConstant.OK;
            }
            else
            {
                return SYMessages.getMessage("PLEASE_SEARCH_Employee");
            }
        }
        private void DataSelector()
        {
            var ListBranch = SYConstant.getBranchDataAccess();
            var ListStaff = DBV.HR_STAFF_VIEW.ToList();
            ListStaff = ListStaff.Where(w => ListBranch.Where(x => x.Code == w.BranchID).Any()).ToList();

            ViewData["STAFF_SELECT"] = ListStaff;// DB.HR_STAFF_VIEW.ToList();
            ViewData["HREmpEduType_LIST"] = DB.HREduTypes.ToList();
        }
    }
}