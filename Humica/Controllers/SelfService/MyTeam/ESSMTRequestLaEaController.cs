using Humica.Core.DB;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic.LM;
using Humica.Logic.MD;
using Humica.Models.SY;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static Humica.Logic.LM.ReqLateEarlyObject;

namespace Humica.Controllers.SelfService.MyTeam
{

    public class ESSMTRequestLaEaController : Humica.EF.Controllers.MasterSaleController
    {

        private const string SCREEN_ID = "ESS0000016";
        private const string URL_SCREEN = "/SelfService/MyTeam/ESSMTRequestLaEa/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "ReqLaEaNo";
        private string DOCTYPE = "RLE01";
        private string _LOCATION_ = "_LOCATION2_";
        private string _DOCTYPE_ = "_DOCTYPE2_";
        HumicaDBContext DB = new HumicaDBContext();
        HumicaDBViewContext DBV = new HumicaDBViewContext();
        SMSystemEntity SMS = new SMSystemEntity();
        public ESSMTRequestLaEaController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
        }

        #region "List"
        public async Task<ActionResult> Index()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            DataSelector();

            var pendingStatus = SYDocumentStatus.PENDING.ToString();
            var openStatus = SYDocumentStatus.OPEN.ToString();
            var approvedStatus = SYDocumentStatus.APPROVED.ToString();
            var userCode = user.UserName;

            // Load data sequentially to avoid context conflicts
            var ListHeader = await DB.HRReqLateEarlies.ToListAsync();
            var ListApp = await DB.ExDocApprovals.Where(w => w.Status == openStatus).ToListAsync();
            var staff = await DBV.HR_STAFF_VIEW.FirstOrDefaultAsync(w => w.EmpCode == userCode);
            var ListBranch = await BranchDataAccess.GetBranchDataAccessAsync();

            var BSM = new ReqLateEarlyObject
            {
                ListReqPending = new List<ClsReuestLaEa>()
            };

            if (staff == null)
            {
                // Handle the case where the staff is not found
                return View(BSM);
            }

            var ListTeamStaff = DBV.HR_STAFF_VIEW
                .AsEnumerable()
                .Where(w => ListBranch.Any(x => x.Code == w.BranchID && x.CompanyCode == w.CompanyCode && w.DEPT == staff.DEPT))
                .ToList();

            var ListHeader1 = ListHeader.Where(w => ListTeamStaff.Any(x => x.EmpCode == w.EmpCode)).ToList();
            var ListAppOpen = ListApp.Where(w => w.Approver == userCode).ToList();

            var ListReqPending = ListHeader
                .Where(w => w.Status == pendingStatus && ListAppOpen.Any(x => x.DocumentNo == w.ReqLaEaNo.ToString()))
                .ToList();

            foreach (var read in ListReqPending)
            {
                var _currAppCount = ListApp.Count(w => w.DocumentNo == read.ReqLaEaNo && w.Status == approvedStatus);

                var _OT = new ClsReuestLaEa
                {
                    Remark = _currAppCount > 0 ? "PENDING HOD" : "PENDING Check",
                    EmpCode = read.EmpCode,
                    EmpName = read.EmpName,
                    LeaveDate = read.LeaveDate,
                    Reason = read.Reason,
                    Status = read.Status,
                    ReqLaEaNo = read.ReqLaEaNo,
                    Qty = read.Qty
                };

                BSM.ListReqPending.Add(_OT);
            }

            BSM.ListHeader = ListHeader1;
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
        public ActionResult PartialListPending()
        {
            ActionName = "Index";
            UserSession();
            DataSelector();
            UserConfList(KeyName);
            ReqLateEarlyObject BSM = new ReqLateEarlyObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ReqLateEarlyObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("PartialListPending", BSM.ListReqPending);
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
            BSM.Header = new HRReqLateEarly();
            BSM.HeaderStaff = new HR_STAFF_VIEW();
            BSM.Header.Qty = 0;
            BSM.ListApproval = new List<ExDocApproval>();
            BSM.Header.LeaveDate = DateTime.Now;
            BSM.Header.Status = SYDocumentStatus.PENDING.ToString();
            BSM.DocType = DOCTYPE;
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        [HttpPost]
        public ActionResult Create(string ID, ReqLateEarlyObject collection)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = ID;
            var BSM = new ReqLateEarlyObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ReqLateEarlyObject)Session[Index_Sess_Obj + ActionName];
                BSM.Header = collection.Header;
                BSM.HeaderStaff = collection.HeaderStaff;
                BSM.DocType = DOCTYPE;
                BSM.ScreenId = SCREEN_ID;
            }
            if (ModelState.IsValid)
            {
                string URL = SYUrl.getBaseUrl() + "/SelfService/MyTeam/ESSMTRequestLaEa/Details/";
                string msg = BSM.ESSRequestLaEa(URL);

                if (msg == SYConstant.OK)
                {
                    BSM.ScreenId = SCREEN_ID;
                    SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                    mess.DocumentNumber = BSM.Header.ReqLaEaNo.ToString();
                    mess.Description = mess.Description + BSM.MessageError;
                    mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details/" + BSM.Header.ReqLaEaNo;

                    Session[Index_Sess_Obj + ActionName] = BSM;
                    Session[SYConstant.MESSAGE_SUBMIT] = mess;
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Create");
                }
                else
                {
                    ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
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
            BSM.Header = DB.HRReqLateEarlies.FirstOrDefault(x => x.ReqLaEaNo == id);
            if (BSM.Header != null)
            {
                BSM.HeaderStaff = DBV.HR_STAFF_VIEW.FirstOrDefault(w => w.EmpCode == BSM.Header.EmpCode);
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
            this.ScreendIDControl = SCREEN_ID;
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = id;
            ReqLateEarlyObject BSM = new ReqLateEarlyObject();
            if (id != null)
            {
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (ReqLateEarlyObject)Session[Index_Sess_Obj + ActionName];
                    BSM.Header = collection.Header;
                }
                BSM.ScreenId = SCREEN_ID;
                string msg = "";// BSM.EditOTReq(id);
                if (msg == SYConstant.OK)
                {
                    DB = new HumicaDBContext();
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
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            return View(BSM);

        }
        #endregion

        #region "Details"
        public ActionResult Details(string id)
        {
            ActionName = "Details";
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);
            ViewData[SYSConstant.PARAM_ID] = id;
            ReqLateEarlyObject BSM = new ReqLateEarlyObject();
            BSM.Header = DB.HRReqLateEarlies.FirstOrDefault(x => x.ReqLaEaNo == id);
            if (BSM.Header != null)
            {
                BSM.HeaderStaff = DBV.HR_STAFF_VIEW.FirstOrDefault(w => w.EmpCode == BSM.Header.EmpCode);
                BSM.ListApproval = DB.ExDocApprovals.Where(w => w.DocumentNo == id && w.DocumentType == "REQ_OT").ToList();
                Session[Index_Sess_Obj + ActionName] = BSM;
                return View(BSM);
            }

            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            Session[Index_Sess_Obj + ActionName] = BSM;
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        #endregion
        #region "Approve"
        public ActionResult Approve(string id)
        {
            //ActionName = "Details";
            this.UserSession();
            DataSelector();
            UserConfListAndForm();
            ViewData[SYSConstant.PARAM_ID] = id;
            ReqLateEarlyObject BSD = new ReqLateEarlyObject();
            if (id.ToString() != "null")
            {
                string URL = SYUrl.getBaseUrl() + "/SelfService/MyTeam/HROTRequest/Details/";
                string sms = BSD.ApproveOTReq(id, URL);
                if (sms == SYConstant.OK)
                {
                    SYMessages messageObject = SYMessages.getMessageObject(sms, user.Lang);
                    messageObject.DocumentNumber = id;
                    messageObject.Description = messageObject.Description + BSD.MessageError;
                    messageObject.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + id;
                    SYMessages mess = SYMessages.getMessageObject("DOC_APPROVED", user.Lang);
                    Session[SYConstant.MESSAGE_SUBMIT] = mess;
                }
                else
                {
                    Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(sms, user.Lang);
                }
                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
            }

            Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        #endregion
        #region "Reject"
        public ActionResult Reject(string id)
        {
            //ActionName = "Details";
            this.UserSession();
            DataSelector();
            UserConfListAndForm();
            ViewData[SYSConstant.PARAM_ID] = id;
            ReqLateEarlyObject BSD = new ReqLateEarlyObject();
            if (id.ToString() != "null")
            {
                string sms = BSD.RejectOTReq(id);
                if (sms == "OK")
                {
                    SYMessages messageObject = SYMessages.getMessageObject(sms, user.Lang);
                    messageObject.DocumentNumber = id;
                    messageObject.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + id;
                    SYMessages mess = SYMessages.getMessageObject("DOC_RJ", user.Lang);
                    Session[SYConstant.MESSAGE_SUBMIT] = mess;
                }
                else
                {
                    Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(sms, user.Lang);
                }
                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + id);
            }

            Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + id);
        }
        #endregion
        #region "Cancel"
        public ActionResult Cancel(string id)
        {
            //ActionName = "Details";
            this.UserSession();
            DataSelector();
            UserConfListAndForm();
            ViewData[SYSConstant.PARAM_ID] = id;
            ReqLateEarlyObject BSD = new ReqLateEarlyObject();
            if (id.ToString() != "null")
            {
                string sms = BSD.CancelOTReq(id);
                if (sms == SYConstant.OK)
                {
                    SYMessages messageObject = SYMessages.getMessageObject(sms, user.Lang);
                    messageObject.DocumentNumber = id;
                    messageObject.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + id;
                    SYMessages mess = SYMessages.getMessageObject("DOC_CANCEL", user.Lang);
                    Session[SYConstant.MESSAGE_SUBMIT] = mess;
                }
                else
                {
                    Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(sms, user.Lang);
                }
                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + id);
            }

            Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + id);
        }
        #endregion
        #region "Ajax Approval"
        public ActionResult GridApprovalDetail()
        {
            ActionName = "Details";
            UserSession();
            UserConfListAndForm();
            ReqLateEarlyObject BSM = new ReqLateEarlyObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ReqLateEarlyObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("GridApprovalDetail", BSM.ListApproval);
        }
        public ActionResult SelectParam(string Branch)
        {
            UserSession();
            //Session[_DOCTYPE_] = docType;
            Session[_LOCATION_] = Branch;
            var rs = new { MS = SYConstant.OK };
            ActionName = "Create";
            ReqLateEarlyObject BSM = new ReqLateEarlyObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ReqLateEarlyObject)Session[Index_Sess_Obj + ActionName];
                BSM.SetAutoApproval(SCREEN_ID, "RLE01", Branch);
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        public ActionResult GridApproval()
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            ReqLateEarlyObject BSM = new ReqLateEarlyObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ReqLateEarlyObject)Session[Index_Sess_Obj + ActionName];
            }
            DataSelector();
            return PartialView("GridApproval", BSM.ListApproval);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateApproval(ExDocApproval ModelObject)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            ReqLateEarlyObject BSM = new ReqLateEarlyObject();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (ReqLateEarlyObject)Session[Index_Sess_Obj + ActionName];
                    }

                    var msg = BSM.isValidApproval(ModelObject, EnumActionGridLine.Add);
                    if (msg == SYConstant.OK)
                    {

                        if (BSM.ListApproval.Count == 0)
                        {
                            ModelObject.ID = 1;
                        }
                        else
                        {
                            ModelObject.ID = BSM.ListApproval.Max(w => w.ID) + 1;
                        }
                        //  ModelObject.DocumentType = Session[_DOCTYPE_].ToString();
                        ModelObject.DocumentNo = "N/A";
                        BSM.ListApproval.Add(ModelObject);
                    }
                    else
                    {
                        ViewData["EditError"] = SYMessages.getMessage(msg);
                    }
                    Session[Index_Sess_Obj + ActionName] = BSM;

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
            DataSelector();

            return PartialView("GridApproval", BSM.ListApproval);
        }

        public ActionResult DeleteApproval(string Approver)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            ReqLateEarlyObject BSM = new ReqLateEarlyObject();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (ReqLateEarlyObject)Session[Index_Sess_Obj + ActionName];
                    }

                    BSM.ListApproval.Where(w => w.Approver == Approver).ToList();
                    if (BSM.ListApproval.Count > 0)
                    {
                        var objDel = BSM.ListApproval.Where(w => w.Approver == Approver).First();
                        BSM.ListApproval.Remove(objDel);
                    }
                    else
                    {
                        ViewData["EditError"] = SYMessages.getMessage("APPROVER_NE");
                    }
                    Session[Index_Sess_Obj + ActionName] = BSM;

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
            DataSelector();

            return PartialView("GridApproval", BSM.ListApproval);
        }
        #endregion
        public ActionResult RequestForApproval(string id)
        {
            UserSession();
            ReqLateEarlyObject BSM = new ReqLateEarlyObject();
            if (id != null)
            {
                BSM.ScreenId = SCREEN_ID;
                string URL = SYUrl.getBaseUrl() + "/SelfService/MyTeam/HROTRequest/Details/";
                string msg = BSM.requestToApprove(id, URL);
                if (msg == SYConstant.OK)
                {
                    var mess = SYMessages.getMessageObject("DOC_RQ", user.Lang);
                    mess.DocumentNumber = id;
                    mess.Description = mess.Description + BSM.MessageError;
                    mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + id;
                    Session[Index_Sess_Obj + ActionName] = null;
                    Session[SYConstant.MESSAGE_SUBMIT] = mess;
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
                }
                else
                {
                    ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                    return View(BSM);
                }
            }
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_EV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }

        public ActionResult EditLeave(HREmpLeaveD MModel)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            GenerateLeaveObject BSM = new GenerateLeaveObject();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (GenerateLeaveObject)Session[Index_Sess_Obj + ActionName];
                    }
                    var DBU = new HumicaDBContext();
                    var ListEmpLeaveD = BSM.ListEmpLeaveD.Where(w => w.LineItem == MModel.LineItem).ToList();
                    if (ListEmpLeaveD.Count > 0)
                    {
                        var objUpdate = ListEmpLeaveD.First();
                        objUpdate.Remark = MModel.Remark;

                        Session[Index_Sess_Obj + ActionName] = BSM;
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
            DataSelector();
            return PartialView("GridItemDetails", BSM.ListEmpLeaveD);
        }

        public ActionResult ShowDataEmp(string ID, string EmpCode)
        {
            var Stafff = DBV.HR_STAFF_VIEW.ToList();
            HR_STAFF_VIEW EmpStaff = Stafff.FirstOrDefault(w => w.EmpCode == EmpCode);
            if (EmpStaff != null)
            {
                var result = new
                {
                    MS = SYConstant.OK,
                    AllName = EmpStaff.AllName,
                    EmpType = EmpStaff.EmpType,
                    Branch = EmpStaff.Branch,
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
            ViewData["STAFF_BRANCH"] = SMS.HRBranches.ToList();

            SYDataList objListt = new SYDataList("Request_Misscan");
            ViewData["LEAVE_TIME_SELECT"] = objListt.ListData;

            SYDataList objList = new SYDataList("Request_Late_Early");
            ViewData["REQUEST_SELECT"] = objList.ListData;
            var ListBranch = SYConstant.getBranchDataAccess();
            var LisCompany = SYConstant.getCompanyDataAccess();

            var userCode = user.UserName;
            var Emp = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == userCode);
            if (Emp == null)
            {
                ViewData["STAFF_SELECT"] = new List<HRStaffProfile>();
                return;
            }

            var userBranch = Emp.Branch;
            var userCompanyCode = Emp.CompanyCode;
            var userDept = Emp.DEPT;

            var ListStaff = DB.HRStaffProfiles
                .AsEnumerable()
                .Where(w => ListBranch.Any(x => x.Code == w.Branch && x.CompanyCode == w.CompanyCode))
                .Where(w => w.DEPT == userDept && w.Status == "A")
                .ToList();

            var Section = DB.HRStaffProfiles
                .Where(x => x.EmpCode == userCode)
                .Select(x => x.DEPT)
                .ToList();

            if (Section.Count > 0)
            {
                ListStaff = ListStaff.Where(x => Section.Contains(x.DEPT)).ToList();
            }

            ViewData["STAFF_SELECT"] = ListStaff;

            var objWf = new ExWorkFlowPreference();
            var location = Session[_LOCATION_] as string ?? string.Empty;
            var docType = Session[_DOCTYPE_] as string ?? string.Empty;

            ViewData["WF_LIST"] = objWf.getApproverListByDocType(docType, location, SCREEN_ID);
        }

    }
}
