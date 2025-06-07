using Humica.Core.DB;
using Humica.Core.SY;
using Humica.EF;
using Humica.EF.Models.SY;
using Humica.Logic.MD;
using Humica.Logic.PR;
using Humica.Models.Report.Payroll;
using Humica.Models.SY;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HR.Controllers.PR.PRM
{

    public class PRApproveSalaryController : Humica.EF.Controllers.MasterSaleController
    {

        private const string SCREEN_ID = "PRM0000022";
        private const string URL_SCREEN = "/PR/PRM/PRApproveSalary/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "ASNumber;Status";
        private string _DOCTYPE_ = "_DOCTYPE2_";
        HumicaDBContext DB = new HumicaDBContext();
        public PRApproveSalaryController()
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

            PRGenerate_Salary BSM = new PRGenerate_Salary();
            BSM.Filter = new Humica.Core.FT.FTFilterEmployee();
            BSM.ListApproveSalary = new List<HISApproveSalary>();
            BSM.ListAppSalaryPending = DB.HisPendingAppSalaries.Where(w => w.IsLock == false).ToList();
            BSM.ListApproveSalary = DB.HISApproveSalaries.Where(w => w.IsPayPartial != true).ToList();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                var obj = (PRGenerate_Salary)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;

            return View(BSM);
        }
        public ActionResult PartialList()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            PRGenerate_Salary BSM = new PRGenerate_Salary();
            BSM.ListApproveSalary = new List<HISApproveSalary>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (PRGenerate_Salary)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView(SYListConfuration.ListDefaultView, BSM.ListApproveSalary);
        }
        public ActionResult PartialProcess()
        {
            ActionName = "Index";
            UserSession();
            UserConfList(this.KeyName);
            PRGenerate_Salary BSM = new PRGenerate_Salary();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (PRGenerate_Salary)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("PartialProcess", BSM.ListAppSalaryPending);
        }
        #endregion
        #region "Create"
        public ActionResult Create(int InYear, int InMonth)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            PRGenerate_Salary BSM = new PRGenerate_Salary();
            var SalaryPending = DB.HisPendingAppSalaries.Where(w => w.InYear == InYear && w.InMonth == InMonth && w.IsLock == false).ToList();

            if (SalaryPending.Count > 0)
            {
                ViewData[SYConstant.PARAM_ID] = InMonth;
                if (InMonth == null)
                {
                    DataList();
                    BSM.HeaderAppSalary = new HISApproveSalary();
                    BSM.ListApproval = new List<ExDocApproval>();
                    BSM.HeaderAppSalary.Status = SYDocumentStatus.OPEN.ToString();
                    BSM.HeaderAppSalary.DocumentDate = DateTime.Now;
                }
                else
                {
                    BSM.HeaderAppSalary = new HISApproveSalary();
                    BSM.HeaderAppSalary.InYear = SalaryPending.First().InYear;
                    BSM.HeaderAppSalary.InMonth = SalaryPending.First().InMonth;
                    BSM.HeaderAppSalary.Status = SYDocumentStatus.OPEN.ToString();
                    BSM.HeaderAppSalary.PayInMonth = new DateTime(InYear, InMonth, DateTime.DaysInMonth(InYear, InMonth));
                    BSM.HeaderAppSalary.DocumentDate = DateTime.Now.Date;
                    BSM.ListApproval = new List<ExDocApproval>();
                    BSM.ScreenId = SCREEN_ID;
                    DataList();
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            else
            {
                Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("NO_REQUEST");
                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
            }

            return View(BSM);

        }

        [HttpPost]
        public ActionResult Create(PRGenerate_Salary BSM, string id)
        {
            UserSession();
            //DataSelector();
            UserConfForm(SYActionBehavior.ADD);

            ActionName = "Create";
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                var obj = (PRGenerate_Salary)Session[Index_Sess_Obj + ActionName];
                BSM.ListApproval = obj.ListApproval;
            }
            if (ModelState.IsValid)
            {
                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.CreateAppSalary();
                if (msg == SYConstant.OK)
                {
                    SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                    mess.DocumentNumber = BSM.HeaderAppSalary.ASNumber.ToString();
                    mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details/" + mess.DocumentNumber;

                    Session[Index_Sess_Obj + ActionName] = BSM;
                    Session[SYConstant.MESSAGE_SUBMIT] = mess;
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
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
        #region "Details"
        public ActionResult Details(string ID)
        {
            ActionName = "Details";
            UserSession();
            UserConfForm(SYActionBehavior.EDIT);
            DataList();
            ViewData[SYSConstant.PARAM_ID] = ID;
            ViewData[ClsConstant.IS_READ_ONLY] = true;
            if (ID != null)
            {
                PRGenerate_Salary BSM = new PRGenerate_Salary();
                BSM.HeaderAppSalary = DB.HISApproveSalaries.Find(ID);
                if (BSM.HeaderAppSalary != null)
                {
                    BSM.ListApproval = DB.ExDocApprovals.Where(w => w.DocumentNo == ID && w.DocumentType == BSM.HeaderAppSalary.DocumentType).ToList();

                    Session[Index_Sess_Obj + ActionName] = BSM;
                    return View(BSM);
                }
            }

            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        #endregion
        #region Request
        public ActionResult RequestForApproval(string id)
        {
            UserSession();
            PRGenerate_Salary BSM = new PRGenerate_Salary();
            if (id != null)
            {
                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.requestToApprove(id);
                if (msg == SYConstant.OK)
                {
                    var Objmatch = DB.HISApproveSalaries.FirstOrDefault(w => w.ASNumber == id);

                    /*------------------WorkFlow--------------------------------*/
                    var excfObject = DB.ExCfWorkFlowItems.Find(SCREEN_ID, BSM.HeaderAppSalary.DocumentType);
                    string wfMsg = "";
                    string URL = SYUrl.getBaseUrl() + "/PR/PRM/PRApproveSalary/Details/";
                    if (excfObject != null)
                    {
                        SYWorkFlowEmailObject wfo =
                            new SYWorkFlowEmailObject(excfObject.ApprovalFlow, WorkFlowType.REQUESTER,
                                 UserType.N, BSM.HeaderAppSalary.Status);
                        wfo.SelectListItem = new SYSplitItem(id);
                        wfo.User = BSM.User;
                        wfo.BS = BSM.BS;
                        wfo.UrlView = SYUrl.getBaseUrl();
                        wfo.ScreenId = SCREEN_ID;
                        wfo.Module = "HR";// CModule.PURCHASE.ToString();
                        wfo.ListLineRef = new List<BSWorkAssign>();
                        wfo.Action = SYDocumentStatus.PENDING.ToString();
                        HRStaffProfile Staff = BSM.getNextApprover(BSM.HeaderAppSalary.ASNumber, "");
                        RptMonthlyPay sa = new RptMonthlyPay();
                        var objRpt = DB.CFReportObjects.Where(w => w.ScreenID == "RPTPR00002"
                            && w.DocType == "Details" && w.IsActive == true).ToList();
                        if (objRpt.Count > 0)
                        {
                            sa.LoadLayoutFromXml(ClsConstant.DEFAULT_REPORT_PATH + objRpt.First().ReportObject);
                        }
                        sa.Parameters["Branch"].Value = SYConstant.Branch_Condition;
                        sa.Parameters["Branch"].Visible = false;
                        sa.Parameters["InMonth"].Value = Objmatch.PayInMonth;
                        sa.Parameters["InMonth"].Visible = false;

                        Session[this.Index_Sess_Obj + this.ActionName] = sa;
                        string fileName = Server.MapPath("~/Content/UPLOAD/" + id + "_" + Objmatch.PayInMonth.ToString("MMMM-yyyy") + ".pdf");
                        string UploadDirectory = Server.MapPath("~/Content/UPLOAD");
                        if (!Directory.Exists(UploadDirectory))
                        {
                            Directory.CreateDirectory(UploadDirectory);
                        }
                        sa.ExportToPdf(fileName);
                        if (Staff.Email != null && Staff.Email != "")
                        {
                            wfo.ListObjectDictionary = new List<object>();
                            wfo.ListObjectDictionary.Add(Objmatch);
                            wfo.ListObjectDictionary.Add(Staff);
                            Objmatch.PayInMonth.ToString("MMM yyyy");
                            var _staff = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == Objmatch.Requestor);
                            Objmatch.Requestor = _staff.AllName;
                            WorkFlowResult result = wfo.ApproProcessWorkFlow(Staff, fileName, URL);
                            wfMsg = wfo.getErrorMessage(result);
                        }
                        else
                        {
                            wfMsg = wfo.getErrorMessage(WorkFlowResult.EMAIL_NOT_SEND);
                        }
                    }
                    var mess = SYMessages.getMessageObject("DOC_RQ", user.Lang);
                    mess.DocumentNumber = id;
                    mess.Description = mess.Description + wfMsg;
                    mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + id;
                    Session[Index_Sess_Obj + ActionName] = null;
                    Session[SYConstant.MESSAGE_SUBMIT] = mess;
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
                }
            }
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_EV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        #endregion
        #region Approve
        public ActionResult Approve(string id)
        {
            UserSession();
            UserConfForm(SYActionBehavior.EDIT);
            PRGenerate_Salary BSM = new PRGenerate_Salary();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (PRGenerate_Salary)Session[Index_Sess_Obj + ActionName];
            }
            BSM.ScreenId = SCREEN_ID;
            string msg = BSM.approveTheDoc(id);
            if (msg == SYConstant.OK)
            {
                var mess = SYMessages.getMessageObject("DOC_APPROVED", user.Lang);
                mess.Description = mess.Description + ". " + BSM.MessageCode;
                Session[SYSConstant.MESSAGE_SUBMIT] = mess;
            }
            else
            {
                Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
            }
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

        }
        #endregion
        public ActionResult Download(string ID)
        {
            ActionName = "Index";
            UserSession();
            UserConfForm(SYActionBehavior.EDIT);
            var report = DB.HISApproveSalaries.FirstOrDefault(w => w.ASNumber == ID);
            if (report != null)
            {
                RptMonthlyPay sa = new RptMonthlyPay();
                DateTime ToDate = new DateTime((int)report.InYear, report.InMonth, 1);
                var objRpt = DB.CFReportObjects.Where(w => w.ScreenID == "RPTPR00002"
                    && w.DocType == "Details" && w.IsActive == true).ToList();
                if (objRpt.Count > 0)
                {
                    sa.LoadLayoutFromXml(ClsConstant.DEFAULT_REPORT_PATH + objRpt.First().ReportObject);
                }
                sa.Parameters["Branch"].Value = SYConstant.Branch_Condition;
                sa.Parameters["Branch"].Visible = false;
                sa.Parameters["InMonth"].Value = ToDate;
                sa.Parameters["InMonth"].Visible = false;

                string fileName = Server.MapPath("~/Content/UPLOAD/MonthLyPay.xls");
                string UploadDirectory = Server.MapPath("~/Content/UPLOAD");
                if (!Directory.Exists(UploadDirectory))
                {
                    Directory.CreateDirectory(UploadDirectory);
                }
                sa.ExportToXls(fileName);
                //var _ReportStore = report.FirstOrDefault();
                //string name = _ReportStore.PathStore;
                //string FileSource = Server.MapPath(_ReportStore.PathStore);

                //Response.Clear();
                //Response.Buffer = true;
                //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                //Response.AddHeader("content-disposition", "attachment;filename=" + _ReportStore.ObjectName + ".repx");
                //Response.Cache.SetCacheability(HttpCacheability.NoCache);
                //Response.WriteFile(name);
                //Response.End();

                Response.Clear();
                Response.Buffer = true;
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename=MonthLyPay.xls");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.WriteFile(fileName);
                Response.End();
            }

            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        #region "Cancel"
        public ActionResult Cancel(string id)
        {
            this.UserSession();
            UserConfListAndForm();
            PRGenerate_Salary BSM = new PRGenerate_Salary();
            string sms = BSM.CancelAppSalary(id);
            if (sms == SYConstant.OK)
            {
                var mess = SYMessages.getMessageObject("DOC_CANCEL", user.Lang);
                mess.Description = mess.Description + ". " + BSM.MessageCode;
                Session[SYSConstant.MESSAGE_SUBMIT] = mess;
            }
            else
            {
                Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(sms, user.Lang);
            }
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        #endregion
        #region "Ajax Approval"
        public ActionResult SelectParam(string docType)
        {
            UserSession();
            Session[_DOCTYPE_] = docType;
            var rs = new { MS = SYConstant.OK };
            //Auto Approval
            ActionName = "Create";
            PRGenerate_Salary BSM = new PRGenerate_Salary();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (PRGenerate_Salary)Session[Index_Sess_Obj + ActionName];
                BSM.SetAutoApproval(SCREEN_ID, docType);
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        public ActionResult GridApproval()
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            PRGenerate_Salary BSM = new PRGenerate_Salary();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (PRGenerate_Salary)Session[Index_Sess_Obj + ActionName];
            }
            DataList();
            return PartialView("GridApproval", BSM.ListApproval);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateApproval(ExDocApproval ModelObject)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            PRGenerate_Salary BSM = new PRGenerate_Salary();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (PRGenerate_Salary)Session[Index_Sess_Obj + ActionName];
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
            DataList();

            return PartialView("GridApproval", BSM.ListApproval);
        }

        public ActionResult DeleteApproval(string Approver)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            PRGenerate_Salary BSM = new PRGenerate_Salary();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (PRGenerate_Salary)Session[Index_Sess_Obj + ActionName];
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
            DataList();

            return PartialView("GridApproval", BSM.ListApproval);
        }
        #endregion
        private void DataList()
        {
            ViewData["DOCUMENT_TYPE"] = DB.ExCfWorkFlowItems.Where(w => w.ScreenID == SCREEN_ID).ToList();
            ViewData["STAFF_LOCATION"] = SYConstant.getBranchDataAccess();
            ViewData["STAFF_SELECT"] = DB.HRStaffProfiles.ToList();
            var objWf = new ExWorkFlowPreference();
            var docType = "";
            if (Session[_DOCTYPE_] != null)
            {
                docType = Session[_DOCTYPE_].ToString();
            }
            ViewData["WF_LIST"] = objWf.getApproverListByDocType(docType, SCREEN_ID);
            // ViewData["PERIODID_SELECT"] = DB.PRpayperiods.OrderByDescending(w => w.PayPeriodId).ToList();

        }
    }
}
