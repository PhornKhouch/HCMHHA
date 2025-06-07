using DevExpress.Web;
using DevExpress.Web.Mvc;
using Humica.Core;
using Humica.Core.DB;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic.LM;
using Humica.Models.SY;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Humica.Controllers.SelfService.LeaveBalance
{
    public class LeaveRequestController : Humica.EF.Controllers.MasterSaleController
    {

        private const string SCREEN_ID = "ESS0000002";
        private const string URL_SCREEN = "/SelfService/LeaveBalance/LeaveRequest/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "TranNo";
        private string PATH_FILE = "12313123123sadfsdfsdfsdf";

        HumicaDBContext DB = new HumicaDBContext();
        HumicaDBViewContext DBV = new HumicaDBViewContext();
        public LeaveRequestController()
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

            GenerateLeaveObject BSM = new GenerateLeaveObject();
            BSM.FInYear = new Humica.Core.FT.FTINYear();
            BSM.FInYear.INYear = DateTime.Now.Year;
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (GenerateLeaveObject)Session[Index_Sess_Obj + ActionName];
            }
            var listWordFlow = DB.HRWorkFlowLeaves.ToList();
            BSM.User = SYSession.getSessionUser();
            BSM.ListEmpLeave = new List<HREmpLeave>();
            var ListLeave = DB.HREmpLeaves.Where(w => w.FromDate.Year == BSM.FInYear.INYear && w.EmpCode == BSM.User.UserName).OrderByDescending(x => x.FromDate).ToList();
            foreach (var item in ListLeave)
            {
                var result = listWordFlow.FirstOrDefault(w => w.Code == item.Status);
                if (result != null)
                {
                    item.Status = result.Description;
                }
                BSM.ListEmpLeave.Add(item);
            }
            Session[Index_Sess_Obj + ActionName] = BSM;

            return View(BSM);
        }
        [HttpPost]
        public ActionResult Index(GenerateLeaveObject collection)
        {
            DataSelector();
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            GenerateLeaveObject BSM = new GenerateLeaveObject();
            BSM.User = SYSession.getSessionUser();
            BSM.ListEmpLeave = new List<HREmpLeave>();
            var listWordFlow = DB.HRWorkFlowLeaves.ToList();
            BSM.FInYear = collection.FInYear;
            var ListLeave = DB.HREmpLeaves.Where(w => w.FromDate.Year == collection.FInYear.INYear && w.EmpCode == BSM.User.UserName).ToList();
            ListLeave = ListLeave.Where(w => w.EmpCode == BSM.User.UserName).OrderByDescending(x => x.FromDate).ToList();
            foreach (var item in ListLeave)
            {
                var result = listWordFlow.FirstOrDefault(w => w.Code == item.Status);
                if (result != null)
                {
                    item.Status = result.Description;
                }
                BSM.ListEmpLeave.Add(item);
            }
            Session[Index_Sess_Obj + ActionName] = BSM;

            return View(BSM);
        }
        public ActionResult PartialList()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            GenerateLeaveObject BSM = new GenerateLeaveObject();
            BSM.ListEmpLeaveB = new List<HREmpLeaveB>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (GenerateLeaveObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView(SYListConfuration.ListDefaultView, BSM.ListEmpLeaveB);
        }
        #endregion

        #region "Create"
        public ActionResult Create()
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm(this.KeyName);
            DataSelector();
            GenerateLeaveObject BSM = new GenerateLeaveObject();
            BSM.ListEmpLeaveD = new List<HREmpLeaveD>();
            BSM.User = SYSession.getSessionUser();
            BSM.ListEmpLeave = new List<HREmpLeave>();

            var emp = DBV.HR_STAFF_VIEW.Where(w => w.EmpCode == BSM.User.UserName).ToList();
            if (emp.Count > 0)
            {
                BSM.HeaderStaff = DBV.HR_STAFF_VIEW.FirstOrDefault(x => x.EmpCode == BSM.User.UserName);
                BSM.EmpLeaveB = new HREmpLeaveB();
                BSM.HeaderEmpLeave = new HREmpLeave();
                BSM.HeaderEmpLeave.Urgent = false;
                BSM.HeaderEmpLeave.FromDate = DateTime.Now;
                BSM.HeaderEmpLeave.ToDate = DateTime.Now;
                BSM.HeaderEmpLeave.Units = "Day";
                BSM.Units = "Day";
                Session["LEAVE_TYEP"] = null;
                Session[PATH_FILE] = null;
            }
            else
            {
                Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
            }
            Session["LEAVE_TYEP"] = null;
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        [HttpPost]
        public ActionResult Create(string ID, GenerateLeaveObject collection)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = ID;
            var BSM = new GenerateLeaveObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (GenerateLeaveObject)Session[Index_Sess_Obj + ActionName];
                BSM.HeaderEmpLeave = collection.HeaderEmpLeave;
                if (Session[PATH_FILE] != null)
                {
                    BSM.HeaderEmpLeave.Attachment = Session[PATH_FILE].ToString();
                }
            }
            if (ModelState.IsValid)
            {
                string fileName = Server.MapPath("~/Content/TEMPLATE/humica-e0886-firebase-adminsdk-95iz2-87c45a528b.json");
                string URL = SYUrl.getBaseUrl() + "/SelfService/MyTeam/MyTeamLeaveRequest/Details/";
                string msg = BSM.ESSRequestLeave(ID, URL, fileName);

                if (msg == SYConstant.OK)
                {
                    BSM.ScreenId = SCREEN_ID;
                    SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                    mess.DocumentNumber = BSM.HeaderEmpLeave.TranNo.ToString();
                    mess.Description = mess.Description + BSM.MessageError;
                    mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details/" + BSM.HeaderEmpLeave.TranNo.ToString();

                    Session[Index_Sess_Obj + ActionName] = BSM;
                    Session[SYConstant.MESSAGE_SUBMIT] = mess;
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Create");
                }
                else
                {
                    SYMessages mess = SYMessages.getMessageObject(msg, user.Lang);
                    mess.DocumentNumber = BSM.HeaderEmpLeave.TranNo.ToString();
                    mess.Description = string.Format(mess.Description, BSM.MessageError);
                    Session[SYConstant.MESSAGE_SUBMIT] = mess;
                    //ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                    Session[Index_Sess_Obj + ActionName] = BSM;
                    return View(BSM);
                }
            }
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            return View(BSM);
        }
        #endregion
        #region "Edit"
        public ActionResult Edit(string id,int TranNo)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = id;
            GenerateLeaveObject BSM = new GenerateLeaveObject();
            BSM.HeaderEmpLeave = new HREmpLeave();
            BSM.HeaderStaff = new HR_STAFF_VIEW();
            string re = id;
            if (id == "null") id = null;
            if (id != null)
            {
                BSM.ListApproverLeave = new List<HRApproverLeave>();
                BSM.HeaderEmpLeave = DB.HREmpLeaves.FirstOrDefault(w => w.TranNo == TranNo && w.EmpCode == id);
                if (Session[PATH_FILE] != null)
                {
                    BSM.HeaderEmpLeave.Attachment = Session[PATH_FILE].ToString();
                    Session[PATH_FILE] = null;
                }
                BSM.HeaderStaff = DBV.HR_STAFF_VIEW.FirstOrDefault(x => x.EmpCode == BSM.HeaderEmpLeave.EmpCode);

                if (BSM.HeaderEmpLeave != null)
                {
                    string DocumentNo = BSM.HeaderEmpLeave.Increment.ToString();
                    var LeaveType = DB.HRLeaveTypes.FirstOrDefault(w => w.Code == BSM.HeaderEmpLeave.LeaveCode);
                    var LeaveB = DB.HREmpLeaveBs.Where(w => w.EmpCode == BSM.HeaderEmpLeave.EmpCode && w.InYear == BSM.HeaderEmpLeave.FromDate.Year).ToList();
                    string LeaveCode = BSM.HeaderEmpLeave.LeaveCode;
                    if (LeaveType.IsParent == true)
                    {
                        LeaveCode = LeaveType.Parent;
                    }
                    BSM.Units = BSM.HeaderEmpLeave.Units;
                    Session["LEAVE_TYEP"] = null;
                    HREmpLeaveB LeaveBanace = LeaveB.FirstOrDefault(w => w.LeaveCode == LeaveCode);
                    BSM.EmpLeaveB = LeaveBanace;
                    BSM.ListEmpLeaveD = DB.HREmpLeaveDs.Where(x => x.LeaveTranNo == BSM.HeaderEmpLeave.Increment).ToList();
                    BSM.ListApproval = DB.ExDocApprovals.Where(w => w.DocumentNo == DocumentNo && w.DocumentType == "LR").ToList();
                    Session[Index_Sess_Obj + ActionName] = BSM;
                    return View(BSM);
                }
            }

            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            Session[Index_Sess_Obj + ActionName] = BSM;
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

        }
        [HttpPost]
        public ActionResult Edit(string id,int TranNo, GenerateLeaveObject collection)
        {
            ActionName = "Create";
            UserSession();
            this.ScreendIDControl = SCREEN_ID;
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = id;
            GenerateLeaveObject BSM = new GenerateLeaveObject();
            if (id != null)
            {
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (GenerateLeaveObject)Session[Index_Sess_Obj + ActionName];
                    if (Session[PATH_FILE] != null)
                    {
                        BSM.HeaderEmpLeave.Attachment = Session[PATH_FILE].ToString();
                        Session[PATH_FILE] = null;
                    }
                    else
                    {
                        collection.HeaderEmpLeave.Attachment = BSM.HeaderEmpLeave.Attachment;
                    }
                    BSM.HeaderEmpLeave = collection.HeaderEmpLeave;
                }
                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.EditLeaveRequest(TranNo);
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
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = id;
            GenerateLeaveObject BSM = new GenerateLeaveObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (GenerateLeaveObject)Session[Index_Sess_Obj + ActionName];
            }
            if (!string.IsNullOrEmpty(id))
            {
                BSM.ListApproval = new List<ExDocApproval>();
                int TranNo = Convert.ToInt32(id);
                BSM.HeaderEmpLeave = DB.HREmpLeaves.FirstOrDefault(w => w.TranNo == TranNo);
                BSM.HeaderStaff = DBV.HR_STAFF_VIEW.FirstOrDefault(x => x.EmpCode == BSM.HeaderEmpLeave.EmpCode);

                var ListApproval = DB.ExDocApprovals.Where(w => w.DocumentNo == BSM.HeaderEmpLeave.Increment.ToString()).OrderBy(w => w.ApproveLevel).ToList();
                BSM.ListApproval = ListApproval.ToList();

                BSM.ListEmpLeaveD = DB.HREmpLeaveDs.Where(x => x.LeaveTranNo == BSM.HeaderEmpLeave.Increment && x.EmpCode == BSM.HeaderEmpLeave.EmpCode).ToList();
                Session[Index_Sess_Obj + ActionName] = BSM;
                return View(BSM);
            }

            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            Session[Index_Sess_Obj + ActionName] = BSM;
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

        }
        #endregion

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
                        if (MModel.StartTime.HasValue)
                        {
                            var totals = MModel.EndTime.Value.Subtract(MModel.StartTime.Value).TotalHours;
                            objUpdate.StartTime = MModel.StartTime;
                            objUpdate.EndTime = MModel.EndTime;
                            objUpdate.LHour = (decimal)totals;
                        }
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
            return PartialView("GridItemDetails", BSM);
        }
        public ActionResult GridItems()
        {
            ActionName = "Index";
            UserSession();
            UserConfForm(ActionBehavior.ACC_REV);
            GenerateLeaveObject BSM = new GenerateLeaveObject();
            BSM.ScreenId = SCREEN_ID;

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (GenerateLeaveObject)Session[Index_Sess_Obj + ActionName];
            }
            DataSelector();
            Session[SYConstant.CURRENT_URL] = URL_SCREEN + "GridItems";
            return PartialView("GridItems", BSM.ListEmpLeave);
        }
        public ActionResult GridItemDetails()
        {
            ActionName = "Create";
            UserSession();
            UserConfForm(ActionBehavior.ACC_REV);
            GenerateLeaveObject BSM = new GenerateLeaveObject();
            BSM.ScreenId = SCREEN_ID;

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (GenerateLeaveObject)Session[Index_Sess_Obj + ActionName];
            }
            DataSelector();
            Session[SYConstant.CURRENT_URL] = URL_SCREEN + "GridItemDetails";
            return PartialView("GridItemDetails", BSM);
        }
        public ActionResult GridItemViewDetails()
        {
            ActionName = "Details";
            UserSession();
            UserConfForm(ActionBehavior.ACC_REV);
            GenerateLeaveObject BSM = new GenerateLeaveObject();
            BSM.ScreenId = SCREEN_ID;

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (GenerateLeaveObject)Session[Index_Sess_Obj + ActionName];
            }
            DataSelector();
            Session[SYConstant.CURRENT_URL] = URL_SCREEN + "GridItemViewDetails";
            return PartialView("GridItemViewDetails", BSM);
        }

        public ActionResult GridItemApprover()
        {
            ActionName = "Details";
            UserSession();
            UserConfForm(ActionBehavior.ACC_REV);
            GenerateLeaveObject BSM = new GenerateLeaveObject();
            BSM.ScreenId = SCREEN_ID;

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (GenerateLeaveObject)Session[Index_Sess_Obj + ActionName];
            }
            DataSelector();
            Session[SYConstant.CURRENT_URL] = URL_SCREEN + "GridItemApprover";
            return PartialView("GridItemApprover", BSM.ListApproval);
        }
        public ActionResult ReceiptTotal(string Action)
        {
            ActionName = Action;
            var BSM = new GenerateLeaveObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (GenerateLeaveObject)Session[Index_Sess_Obj + ActionName];
                updateTotalReceipt(BSM, Action);
                var result = new
                {
                    MS = SYConstant.OK,
                    NoDay = BSM.HeaderEmpLeave.NoDay,
                    NoPH = BSM.HeaderEmpLeave.NoPH,
                    NoRest = BSM.HeaderEmpLeave.NoRest,
                    Balance = BSM.EmpLeaveB.Balance - (decimal)BSM.HeaderEmpLeave.NoDay,
                };

                return Json(result, (JsonRequestBehavior)1);
            }
            var data1 = new { MS = "FAIL" };
            return Json(data1, (JsonRequestBehavior)1);
        }
        private void updateTotalReceipt(GenerateLeaveObject BSM, string Action)
        {
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (GenerateLeaveObject)Session[Index_Sess_Obj + ActionName];
            }
            if (BSM.HeaderEmpLeave == null)
            {
                BSM.HeaderEmpLeave = new HREmpLeave();
            }
            else
            {
                BSM.HeaderEmpLeave.NoDay = 0;
                BSM.HeaderEmpLeave.NoPH = 0;
                BSM.HeaderEmpLeave.NoRest = 0;
            }
            double WHour = 0;
            var _PayPram = DB.HRStaffProfiles.Where(w => w.EmpCode == BSM.HeaderStaff.EmpCode).ToList();
            if (_PayPram.Count() > 0)
            {
                var obj = _PayPram.First().PayParam;
                var Pay = DB.PRParameters.Find(obj);
                WHour = Convert.ToDouble(Pay.WHOUR);
            }
            double LHour = 0;
            foreach (var item in BSM.ListEmpLeaveD)
            {
                if (BSM.Units != "Day")
                {
                    LHour = (double)item.LHour;
                }
                LHour = WHour;// Convert.ToDouble(item.LHour);
                if (item.Remark == "Morning" || item.Remark == "Afternoon")
                {
                    LHour = Convert.ToDouble(WHour / 2);
                }
                LHour = LHour / WHour;
                if (item.Status == "Leave")
                    BSM.HeaderEmpLeave.NoDay += LHour;
                else if (item.Status == "PH")
                    BSM.HeaderEmpLeave.NoPH += LHour;
                else if (item.Status == "Rest")
                    BSM.HeaderEmpLeave.NoRest += LHour;
            }
        }
        [HttpPost]
        public ActionResult ShowData(string ID, DateTime FromDate, string Units, DateTime ToDate, string Action)
        {

            ActionName = Action;
            //DataSelector();
            GenerateLeaveObject BSM = new GenerateLeaveObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (GenerateLeaveObject)Session[Index_Sess_Obj + ActionName];
                BSM.ListEmpLeaveD = new List<HREmpLeaveD>();
            }
            if (ID == "" || ID == null)
            {
                var rs1 = new { MS = "LeaveType_INVALIT" };
                return Json(rs1, JsonRequestBehavior.DenyGet);
            }
            int result1 = 0;
            int Line = 0;
            int NPH = 0;
            decimal NRest = 0;
            string Reject = SYDocumentStatus.REJECTED.ToString();
            var Pub = DB.HRPubHolidays.ToList();
            var Leave = DB.HREmpLeaveBs;
            //decimal? Balance = 0;
            //var  ResBalance = Leave.FirstOrDefault(x => x.EmpCode == BSM.HeaderStaff.EmpCode && x.InYear==FromDate.Year && x.LeaveCode==ID);
            //if (ResBalance != null)
            //{
            //    Balance = ResBalance.Balance;
            //    //var rs1 = new { MS = "BALANCE_INVALIT" };
            //    //return Json(rs1, JsonRequestBehavior.DenyGet);
            //}
            decimal? Balance = 0;
            var ResBalance = Leave.FirstOrDefault(x => x.EmpCode == BSM.HeaderStaff.EmpCode && x.InYear == FromDate.Year && x.LeaveCode == ID);
            if (ResBalance != null)
            {
                Balance = ResBalance.Balance;
                //var rs1 = new { MS = "BALANCE_INVALIT" };
                //return Json(rs1, JsonRequestBehavior.DenyGet);
            }
            //else
            //{
            //    var LB = new { MS = "Pleas Generate Leave Entitle" };
            //    return Json(LB, JsonRequestBehavior.DenyGet);
            //}
            Pub = Pub.Where(w => w.PDate.Date >= FromDate.Date && w.PDate <= ToDate.Date).ToList();
            string remark = "Morning";
            if (BSM.HeaderStaff != null)
            {
                var _param = DB.PRParameters.Find(BSM.HeaderStaff.PayParam);
                string Status = "";
                DateTime DateNow = new DateTime();
                var LeaveType = DB.HRLeaveTypes.FirstOrDefault(w => w.Code == ID);
                for (var day = FromDate.Date; day.Date <= ToDate.Date; day = day.AddDays(1))
                {
                    DateNow = day;
                    Status = SYDocumentStatus.Leave.ToString();
                    if (Pub.Where(w => w.PDate.Date == day.Date).Any() && LeaveType.IncPub == true)
                    {
                        NPH += 1; Status = SYDocumentStatus.PH.ToString();
                    }
                    bool isRest = false;
                    if (_param.WDMON != true && day.DayOfWeek == DayOfWeek.Monday && LeaveType.InRest == true) isRest = true;
                    else if (_param.WDTUE != true && day.DayOfWeek == DayOfWeek.Tuesday && LeaveType.InRest == true) isRest = true;
                    else if (_param.WDWED != true && day.DayOfWeek == DayOfWeek.Wednesday && LeaveType.InRest == true) isRest = true;
                    else if (_param.WDTHU != true && day.DayOfWeek == DayOfWeek.Thursday && LeaveType.InRest == true) isRest = true;
                    else if (_param.WDFRI != true && day.DayOfWeek == DayOfWeek.Friday && LeaveType.InRest == true) isRest = true;
                    else if (_param.WDSAT != true && day.DayOfWeek == DayOfWeek.Saturday && LeaveType.InRest == true) isRest = true;
                    else if (_param.WDSUN != true && day.DayOfWeek == DayOfWeek.Sunday && LeaveType.InRest == true) isRest = true;
                    if (isRest == true)
                    {
                        NRest += 1; Status = SYDocumentStatus.Rest.ToString();
                    }
                    result1 += 1;
                    Line += 1;

                    var Leaved = new HREmpLeaveD();
                    Leaved.LeaveDate = day;
                    Leaved.CutMonth = day;
                    if (LeaveType.IsParent == true && LeaveType.Amount != 1)
                        Leaved.Remark = remark;
                    else Leaved.Remark = "FullDay";
                    Leaved.Remark = "FullDay";
                    if (_param.WDSAT == true && day.DayOfWeek == DayOfWeek.Saturday && LeaveType.InRest == true && _param.WDSATDay == 0.5M)
                    {
                        Leaved.Remark = remark;
                        NRest += 0.5M;
                    }
                    if (BSM.Units != "Day")
                    {
                        Leaved.StartTime = new DateTime(day.Year, day.Month, day.Day, 8, 0, 0);
                        Leaved.EndTime = new DateTime(day.Year, day.Month, day.Day, 9, 0, 0);
                        var totals = Leaved.EndTime.Value.Subtract(Leaved.StartTime.Value).TotalHours;
                        Leaved.LHour = (decimal)totals;
                        Leaved.Remark = "Hours";
                    }
                    Leaved.LHour = _param.WHOUR;
                    Leaved.LineItem = Line;
                    Leaved.Status = Status;
                    BSM.ListEmpLeaveD.Add(Leaved);
                }
                if (LeaveType.IsParent == true)
                {
                    ID = LeaveType.Parent;
                }
                var LeaveB = DB.HREmpLeaveBs.ToList();
                HREmpLeaveB LeaveBanace = LeaveB.FirstOrDefault(w => w.LeaveCode == ID && w.InYear == FromDate.Year && w.EmpCode == BSM.HeaderStaff.EmpCode);
                Session["LEAVE_TYEP"] = LeaveType.Code;
                BSM.EmpLeaveB = LeaveBanace;
                if (LeaveBanace == null) BSM.EmpLeaveB.Balance = 0;
                var result = new
                {
                    MS = SYConstant.OK,
                    NoDay = BSM.ListEmpLeaveD.Where(w => w.Status == SYDocumentStatus.Leave.ToString()).Count(),
                    NoPH = BSM.ListEmpLeaveD.Where(w => w.Status == SYDocumentStatus.PH.ToString()).Count(),
                    NoRest = BSM.ListEmpLeaveD.Where(w => w.Status == SYDocumentStatus.Rest.ToString()).Count(),
                    Balance = BSM.EmpLeaveB.Balance - BSM.ListEmpLeaveD.Where(w => w.Status == SYDocumentStatus.Leave.ToString()).Count(),
                    ToDate = ToDate,
                };
                Session[Index_Sess_Obj + ActionName] = BSM;
                return Json(result, JsonRequestBehavior.DenyGet);
            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        public ActionResult ShowUNITS(string ID, string Action)
        {
            ActionName = Action;
            GenerateLeaveObject BSM = new GenerateLeaveObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (GenerateLeaveObject)Session[Index_Sess_Obj + ActionName];
            }
            BSM.Units = ID;

            Session[Index_Sess_Obj + ActionName] = BSM;
            var rs = new
            {
                MS = SYConstant.OK,
                Units = ID
            };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        public ActionResult UploadControlCallbackActionImage(HttpPostedFileBase file_Uploader)
        {

            UserSession();
            var path = DB.CFUploadPaths.Find("IMG_UPLOAD");
            SYFileImport sfi = new SYFileImport(path);
            sfi.ObjectTemplate = new MDUploadTemplate();
            sfi.ObjectTemplate.UploadDate = DateTime.Now;
            sfi.ObjectTemplate.ScreenId = SCREEN_ID;
            sfi.ObjectTemplate.UploadBy = user.UserName;
            sfi.ObjectTemplate.Module = "STAFF";
            sfi.ObjectTemplate.IsGenerate = false;

            UploadedFile[] files = UploadControlExtension.GetUploadedFiles("UploadControl",
                sfi.ValidationSettings,
                sfi.uc_FileUploadCompleteFile);
            Session[PATH_FILE] = sfi.ObjectTemplate.UpoadPath;
            return null;
        }
        private void DataSelector()
        {
            UserSession();
            SYDataList objList = new SYDataList("LEAVE_TIME");
            if (Session["LEAVE_TYEP"] != null)
            {
                string _Leave = Session["LEAVE_TYEP"].ToString();
                var _listLeave = DB.HRLeaveTypes.Where(w => w.Code == _Leave).ToList();
                if (_listLeave.Where(w => w.IsParent == true).Any())
                {
                    //objList.ListData = objList.ListData.Where(w => w.SelectValue != "FullDay").ToList();
                    objList.ListData = objList.ListData.ToList();
                }
            }
            ViewData["LEAVE_TIME_SELECT"] = objList.ListData;
            var emp = DB.HRStaffProfiles.Where(w => w.EmpCode == user.UserName).FirstOrDefault();
            if (emp != null)
            {
                DateTime data = DateTime.Now;
                DateTime EndDate = new DateTime(data.Year, 12, 31);
                DateTime StartDate = Convert.ToDateTime(emp.LeaveConf);
                int WorkMonth = DateTimeHelper.CountMonth(StartDate, EndDate);
                var LstLeaveType = DB.HRLeaveTypes.ToList();
                LstLeaveType = GenerateLeaveObject.GetLeaveType(LstLeaveType, emp.TemLeave, emp.Sex, WorkMonth, true);
                ViewData["LeaveTypes_SELECT"] = LstLeaveType.ToList();
                ViewData["STAFF_SELECT"] = DB.HRStaffProfiles.Where(w => w.FirstLine == emp.EmpCode || w.SecondLine == emp.EmpCode || w.HODCode == emp.EmpCode).ToList();
            }
            ViewData["UNITS_SELECT"] = ClsUnits.LoadUnit();
        }
    }
}