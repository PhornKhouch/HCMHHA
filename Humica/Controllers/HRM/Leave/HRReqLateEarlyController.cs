using Humica.Core.DB;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic.LM;
using Humica.Models.SY;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.HRM.Leave
{

    public class HRReqLateEarlyController : Humica.EF.Controllers.MasterSaleController
    {

        private const string SCREEN_ID = "HRL0000006";
        private const string URL_SCREEN = "/HRM/Leave/HRReqLateEarly/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "ReqLaEaNo";
        private string DOCTYPE = "RLE01";
        HumicaDBContext DB = new HumicaDBContext();
        HumicaDBViewContext DBV = new HumicaDBViewContext();
        public HRReqLateEarlyController()
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
            DataSelector();

            ReqLateEarlyObject BSM = new ReqLateEarlyObject();

            BSM.ListHeader = DB.HRReqLateEarlies.ToList();

            Session[Index_Sess_Obj + ActionName] = BSM;

            return View(BSM);
        }

        public ActionResult PartialList()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            ReqLateEarlyObject BSM = new ReqLateEarlyObject();
            BSM.ListHeader = new List<HRReqLateEarly>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ReqLateEarlyObject)Session[Index_Sess_Obj + ActionName];
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
            ReqLateEarlyObject BSM = new ReqLateEarlyObject();
            BSM.User = SYSession.getSessionUser();
            BSM.Header = new HRReqLateEarly();
            BSM.HeaderStaff = new HR_STAFF_VIEW();
            BSM.Header.LeaveDate = DateTime.Now;
            BSM.Header.Qty = 0;
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        [HttpPost]
        public ActionResult Create(ReqLateEarlyObject collection)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            //ViewData[SYSConstant.PARAM_ID] = ID;
            var BSM = new ReqLateEarlyObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ReqLateEarlyObject)Session[Index_Sess_Obj + ActionName];
                BSM.Header = collection.Header;
                BSM.HeaderStaff = collection.HeaderStaff;
            }
            if (ModelState.IsValid)
            {
                BSM.DocType = DOCTYPE;
                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.CreateReqLateEarly();
                if (msg == SYConstant.OK)
                {

                    SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                    mess.DocumentNumber = BSM.Header.ReqLaEaNo;
                    mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details/" + mess.DocumentNumber;
                    Session[Index_Sess_Obj + ActionName] = BSM;
                    Session[SYConstant.MESSAGE_SUBMIT] = mess;
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Create");
                }
                else
                {
                    ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                    Session[Index_Sess_Obj + ActionName] = BSM;
                    return View(BSM);
                }
            }
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            return View(BSM);
        }
        #endregion

        #region "Edit"
        public ActionResult Edit(string id)
        {
            ActionName = "Edit";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = id;
            ReqLateEarlyObject BSM = new ReqLateEarlyObject();
            BSM.Header = DB.HRReqLateEarlies.Find(id);
            if (BSM.Header != null)
            {
                BSM.HeaderStaff = DBV.HR_STAFF_VIEW.FirstOrDefault(x => x.EmpCode == BSM.Header.EmpCode);
                Session[Index_Sess_Obj + ActionName] = BSM;
                return View(BSM);
            }

            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            Session[Index_Sess_Obj + ActionName] = BSM;
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

        }
        [HttpPost]
        public ActionResult Edit(string id, ReqLateEarlyObject collection)
        {
            ActionName = "Edit";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = id;
            ReqLateEarlyObject BSM = new ReqLateEarlyObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ReqLateEarlyObject)Session[Index_Sess_Obj + ActionName];
            }
            BSM.ScreenId = SCREEN_ID;
            BSM.Header = collection.Header;

            string msg = BSM.editReqLateEarly(id);
            if (msg == SYConstant.OK)
            {
                SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                mess.DocumentNumber = id;
                mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + mess.DocumentNumber;
                ViewData[SYConstant.MESSAGE_SUBMIT] = mess;
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
        #endregion
        #region "Delete"
        public ActionResult Delete(string id)
        {
            UserSession();
            UserConfForm(SYActionBehavior.DELETE);
            DataSelector();
            if (id == "null") id = null;
            if (id != null)
            {
                ReqLateEarlyObject Del = new ReqLateEarlyObject();
                string msg = Del.deleteReqLateEarly(id);
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
                Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("LEAVE_NE", user.Lang);
            }
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

        }
        #endregion
        #region "Details"
        public ActionResult Details(string id)
        {
            ActionName = "Details";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.VIEW);
            ViewData[SYSConstant.PARAM_ID] = id;
            ReqLateEarlyObject BSM = new ReqLateEarlyObject();
            BSM.Header = DB.HRReqLateEarlies.Find(id);
            if (BSM.Header != null)
            {
                BSM.HeaderStaff = DBV.HR_STAFF_VIEW.FirstOrDefault(x => x.EmpCode == BSM.Header.EmpCode);
                BSM.ListApproval = DB.ExDocApprovals.Where(w => w.DocumentNo == id && w.DocumentType == BSM.Header.RequestType).ToList();
                Session[Index_Sess_Obj + ActionName] = BSM;
                return View(BSM);
            }
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            Session[Index_Sess_Obj + ActionName] = BSM;
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

        }
        #endregion
        public ActionResult GridApproval()
        {
            ActionName = "Details";
            UserSession();
            UserConfForm(ActionBehavior.ACC_REV);
            ReqLateEarlyObject BSM = new ReqLateEarlyObject();
            BSM.ScreenId = SCREEN_ID;

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ReqLateEarlyObject)Session[Index_Sess_Obj + ActionName];
            }
            DataSelector();
            Session[SYConstant.CURRENT_URL] = URL_SCREEN + "GridApproval";
            return PartialView("GridApproval", BSM.ListApproval);
        }
        public ActionResult ShowDataEmp(string ID, string EmpCode)
        {
            var Stafff = DBV.HR_STAFF_VIEW;
            HR_STAFF_VIEW EmpStaff = Stafff.FirstOrDefault(w => w.EmpCode == EmpCode);
            if (EmpStaff != null)
            {
                var result = new
                {
                    MS = SYConstant.OK,
                    AllName = EmpStaff.AllName,
                    EmpType = EmpStaff.EmpType,
                    Division = EmpStaff.Division,
                    DEPT = EmpStaff.Department,
                    SECT = EmpStaff.Section,
                    LevelCode = EmpStaff.LevelCode,
                    Position = EmpStaff.Position,
                    StartDate = EmpStaff.StartDate
                };

                return Json(result, JsonRequestBehavior.DenyGet);

            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        private void DataSelector()
        {

            SYDataList objList = new SYDataList("Request_Late_Early");
            ViewData["REQUEST_SELECT"] = objList.ListData;
            var staff = DBV.HR_STAFF_VIEW.ToList();
            var ListBranch = SYConstant.getBranchDataAccess();
            var ListStaff = new List<HR_STAFF_VIEW>();
            ListStaff = staff.Where(w => ListBranch.Where(x => x.Code == w.BranchID).Any()).ToList();
            ViewData["STAFF_SELECT"] = ListStaff.ToList();
        }
    }
}
