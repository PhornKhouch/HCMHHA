using DevExpress.Web.Mvc;
using Humica.Core.DB;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic;
using Humica.Logic.MD;
using Humica.Logic.RCM;
using Humica.Models.Report.HRM;
using Humica.Models.SY;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Humica.Controllers.HRM.RCM
{
    public class RCMRecruitRequestController : Humica.EF.Controllers.MasterSaleController
    {
        private const string SCREEN_ID = "RCM0000001";
        private const string URL_SCREEN = "/HRM/RCM/RCMRecruitRequest/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "RequestNo";
        private string _DOCTYPE_ = "_DOCTYPE_";
        private string _LOCATION_ = "_LOCATION_";

        SMSystemEntity SMS = new SMSystemEntity();
        HumicaDBContext DB = new HumicaDBContext();
        HumicaDBViewContext DBV = new HumicaDBViewContext();
        public RCMRecruitRequestController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
        }
        #region 'List'
        public async Task<ActionResult> Index()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            DataSelector();

            RCMRecruitRequestObject BSM = new RCMRecruitRequestObject();
            BSM.ListHeader = new List<RCMRecruitRequest>();
            BSM.ListHeader = await DB.RCMRecruitRequests.ToListAsync();
            Session[_DOCTYPE_] = "";
            Session[_LOCATION_] = "";

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                var obj = (RCMRecruitRequestObject)Session[Index_Sess_Obj + ActionName];
            }
            string Open = SYDocumentStatus.OPEN.ToString();
            string Pending = SYDocumentStatus.PENDING.ToString();
            string Appro = SYDocumentStatus.APPROVED.ToString();
            var ApprovedBy_ = await DBV.RCM_RecruitRequest_VIEW.Where(w => w.Employee == user.UserName && w.StatusAPP == Open).ToListAsync();

            var WaitList = new List<RCM_RecruitRequest_VIEW>();
            foreach (var read in ApprovedBy_)
            {

                if (read.ApproveLevel > 1)
                {
                    // select all approver base on document
                    var approvers = await DBV.RCM_RecruitRequest_VIEW.Where(w => w.RequestNo == read.RequestNo).ToListAsync();
                    // count pending previous approver
                    int count = approvers.Where(w => w.ApproveLevel < read.ApproveLevel && w.StatusAPP != Appro).Count();
                    if (count > 0) continue;
                }
                var List_ = new RCM_RecruitRequest_VIEW();
                List_ = read;
                WaitList.Add(List_);
            }
            BSM.ListRecruitRequest = WaitList;
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        public ActionResult PartialList()
        {
            ActionName = "Index";
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);
            RCMRecruitRequestObject BSM = new RCMRecruitRequestObject();
            BSM.ListRecruitRequest = new List<RCM_RecruitRequest_VIEW>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMRecruitRequestObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView(SYListConfuration.ListDefaultView, BSM.ListRecruitRequest);
        }
        public ActionResult _ListRequest()
        {
            ActionName = "Index";
            DataSelector();
            UserSession();
            UserConfListAndForm(this.KeyName);
            RCMRecruitRequestObject BSM = new RCMRecruitRequestObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMRecruitRequestObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("_ListRequest", BSM.ListHeader);
        }
        #endregion 
        #region 'Create'
        public ActionResult Create()
        {
            ActionName = "Create";
            UserSession();
            DataSelector();

            RCMRecruitRequestObject BSD = new RCMRecruitRequestObject();

            BSD.Header = new RCMRecruitRequest();
            BSD.ListHeader = new List<RCMRecruitRequest>();
            BSD.ExCfWFApprover = new ExCfWFApprover();
            BSD.Header.ExpectedDate = DateTime.Now;
            BSD.Header.RequestedDate = DateTime.Now;
            BSD.Header.Gender = "B";
            BSD.Header.StaffType = "OS";
            BSD.Header.WorkingType = "FT";
            BSD.Header.RecruitFor = "New";
            BSD.Header.RecruitType = "INE";
            BSD.Header.ProposedSalaryFrom = 0;
            BSD.Header.ProposedSalaryTo = 0;
            BSD.Header.Status = SYDocumentStatus.OPEN.ToString();
            BSD.ListApproval = new List<ExDocApproval>();
            Session["Position"] = null;
            Session["JD"] = null;
            UserConfListAndForm();
            Session[Index_Sess_Obj + ActionName] = BSD;
            return View(BSD);
        }
        [HttpPost]
        public ActionResult Create(RCMRecruitRequestObject collection)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.ADD);

            var BSM = (RCMRecruitRequestObject)Session[Index_Sess_Obj + ActionName];
            collection.ListApproval = BSM.ListApproval;
            collection.ScreenId = SCREEN_ID;

            string RequestNo = string.Empty;
            string msg = collection.createRRequest(out RequestNo);

            if (msg == SYConstant.OK)
            {
                #region template
                FRMRequest sa = new FRMRequest();
                string receiver = string.Empty;
                int approvelevel = 0;
                receiver = BSM.ListApproval.OrderBy(w => w.ApproveLevel).FirstOrDefault().Approver;
                approvelevel = BSM.ListApproval.OrderBy(w => w.ApproveLevel).FirstOrDefault().ApproveLevel;
                var objRpt = DB.CFReportObjects.Where(w => w.ScreenID == SCREEN_ID && w.IsActive == true).ToList();
                if (objRpt.Count > 0)
                {
                    sa.LoadLayoutFromXml(ClsConstant.DEFAULT_REPORT_PATH + objRpt.First().ReportObject);
                }
                var _Appro = DB.RCMRecruitRequests.FirstOrDefault(w => w.RequestNo == RequestNo);
                _Appro.ApprovedBy = user.UserName;
                sa.Parameters["RequestNo"].Value = RequestNo;
                sa.Parameters["APROLEVEL"].Value = approvelevel;

                sa.Parameters["RequestNo"].Visible = false;
                sa.Parameters["APROLEVEL"].Visible = false;
                Session[this.Index_Sess_Obj + this.ActionName] = sa;

                string fileName = Server.MapPath("~/Content/UPLOAD/" + "RECRUITMENT REQUEST FORM.pdf");
                string UploadDirectory = Server.MapPath("~/Content/UPLOAD");
                if (!Directory.Exists(UploadDirectory))
                {
                    Directory.CreateDirectory(UploadDirectory);
                }
                sa.ExportToPdf(fileName);
                BSM.SendEmail(fileName, receiver);
                #endregion
            }
            Session[Index_Sess_Obj + this.ActionName] = collection;
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
            return View(collection);
        }
        #endregion 
        #region 'Details'
        public ActionResult Details(string RequestNo)
        {
            ActionName = "Details";
            UserSession();
            RCMRecruitRequestObject BSM = new RCMRecruitRequestObject();
            DataSelector();
            ViewData[SYConstant.PARAM_ID] = RequestNo;
            ViewData[ClsConstant.IS_READ_ONLY] = true;

            BSM.Header = DB.RCMRecruitRequests.FirstOrDefault(w => w.RequestNo == RequestNo);
            if (BSM.Header == null)
            {
                Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE");
                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
            }
            JobType(BSM.Header.DEPT, "Department");
            JobType(BSM.Header.POST, "");
            BSM.ListApproval = DB.ExDocApprovals.Where(w => w.DocumentNo == RequestNo && w.DocumentType == BSM.Header.DocType).ToList();
            UserConfForm(SYActionBehavior.VIEW);
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        #endregion
        #region 'Edit'
        public ActionResult Edit(string RequestNo)
        {
            ActionName = "Edit";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMRecruitRequestObject BSM = new RCMRecruitRequestObject();
            if (!string.IsNullOrEmpty(RequestNo))
            {
                BSM.Header = DB.RCMRecruitRequests.FirstOrDefault(w => w.RequestNo == RequestNo);

                if (BSM.Header != null)
                {
                    string Approve = SYDocumentStatus.APPROVED.ToString();
                    JobType(BSM.Header.DEPT, "Department");
                    JobType(BSM.Header.POST, "");
                    if (BSM.Header.Status != Approve)
                    {
                        BSM.ListApproval = DB.ExDocApprovals.Where(w => w.DocumentNo == RequestNo && w.DocumentType == BSM.Header.DocType).ToList();
                        Session[Index_Sess_Obj + ActionName] = BSM;
                        return View(BSM);
                    }
                    else
                    {
                        Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV");
                        return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
                    }
                }
            }
            Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("INV_DOC");
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        [HttpPost]
        public ActionResult Edit(string RequestNo, RCMRecruitRequestObject collection)
        {
            ActionName = "Edit";
            UserSession();
            DataSelector();
            UserConfListAndForm();

            RCMRecruitRequestObject BSM = new RCMRecruitRequestObject();

            BSM = (RCMRecruitRequestObject)Session[Index_Sess_Obj + ActionName];
            collection.ListApproval = BSM.ListApproval;
            collection.ScreenId = SCREEN_ID;

            if (ModelState.IsValid)
            {
                string msg = collection.UpdRRequest(RequestNo);

                if (msg != SYConstant.OK)
                {
                    SYMessages mess_err = SYMessages.getMessageObject(msg, user.Lang);
                    Session[Index_Sess_Obj + this.ActionName] = BSM;
                    UserConfForm(ActionBehavior.SAVEGRID);
                    Session[SYConstant.MESSAGE_SUBMIT] = mess_err;
                    return View(collection);
                }
                SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                mess.DocumentNumber = RequestNo;
                mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?RequestNo=" + mess.DocumentNumber; ;
                Session[Index_Sess_Obj + this.ActionName] = collection;
                UserConfForm(ActionBehavior.SAVEGRID);
                Session[SYConstant.MESSAGE_SUBMIT] = mess;
                return View(collection);
            }
            Session[Index_Sess_Obj + this.ActionName] = BSM;
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            return View(BSM);
        }
        #endregion
        #region 'Delete'  
        public ActionResult Delete(string RequestNo)
        {
            UserSession();
            RCMRecruitRequestObject BSM = new RCMRecruitRequestObject();
            BSM.Header = DB.RCMRecruitRequests.FirstOrDefault(w => w.RequestNo == RequestNo);
            if (BSM.Header != null)
            {
                string msg = BSM.deleteRRequest(RequestNo);

                if (msg == SYConstant.OK)
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DELETE_M", user.Lang);
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
                }
                else
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
                }
            }
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        #endregion 
        #region 'Convert Status'
        public ActionResult Reject(string RequestNo)
        {
            ActionName = "Details";
            UserSession();
            UserConfForm(SYActionBehavior.EDIT);
            DataSelector();
            if (RequestNo != null)
            {
                RCMRecruitRequestObject BSM = new RCMRecruitRequestObject();
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (RCMRecruitRequestObject)Session[Index_Sess_Obj + ActionName];
                }

                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.Cancel(RequestNo);
                if (msg == SYConstant.OK)
                {
                    var mess = SYMessages.getMessageObject("DOC_RJ", user.Lang);
                    mess.Description = mess.Description + ". " + BSM.MessageError;
                    Session[SYSConstant.MESSAGE_SUBMIT] = mess;
                }
                else
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                }
            }
            else
            {
                Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE", user.Lang);
            }
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        public ActionResult Approve(string RequestNo)
        {
            ActionName = "Details";
            UserSession();
            UserConfForm(SYActionBehavior.EDIT);
            DataSelector();
            if (RequestNo != null)
            {
                RCMRecruitRequestObject BSM = new RCMRecruitRequestObject();
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (RCMRecruitRequestObject)Session[Index_Sess_Obj + ActionName];
                }

                BSM.ScreenId = SCREEN_ID;
                FRMRequest sa = new FRMRequest();
                var objRpt = DB.CFReportObjects.Where(w => w.ScreenID == SCREEN_ID && w.IsActive == true).ToList();
                if (objRpt.Count > 0)
                {
                    sa.LoadLayoutFromXml(ClsConstant.DEFAULT_REPORT_PATH + objRpt.First().ReportObject);
                }
                var _Appro = DB.ExDocApprovals.FirstOrDefault(w => w.DocumentNo == RequestNo && w.Approver == user.UserName);

                sa.Parameters["RequestNo"].Value = RequestNo;
                sa.Parameters["APROLEVEL"].Value = _Appro.ApproveLevel;
                sa.Parameters["RequestNo"].Visible = false;
                sa.Parameters["APROLEVEL"].Visible = false;
                Session[this.Index_Sess_Obj + this.ActionName] = sa;

                string fileName = Server.MapPath("~/Content/UPLOAD/" + "STAFF REQUISITION FORM.pdf");
                string UploadDirectory = Server.MapPath("~/Content/UPLOAD");
                if (!Directory.Exists(UploadDirectory))
                {
                    Directory.CreateDirectory(UploadDirectory);
                }
                sa.ExportToPdf(fileName);
                string msg = BSM.Approved(RequestNo, fileName);

                if (msg == SYConstant.OK)
                {

                    var mess = SYMessages.getMessageObject("DOC_APPROVED", user.Lang);
                    mess.Description = mess.Description + ". " + BSM.MessageError;
                    Session[SYSConstant.MESSAGE_SUBMIT] = mess;
                }
                else
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                }
            }
            else
            {
                Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE", user.Lang);
            }
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        #endregion 
        #region 'Print'
        public ActionResult Print(string id)
        {
            this.UserSession();
            this.UserConf(ActionBehavior.VIEWONLY);
            ViewData[SYSConstant.PARAM_ID] = id;
            this.UserMVCReportView();
            return View("ReportView");
        }
        public ActionResult DocumentViewerPartial(string id)
        {
            this.UserSession();
            this.UserConf(ActionBehavior.VIEWONLY);
            this.ActionName = "Print";
            var obj = DB.RCMRecruitRequests.Where(w => w.RequestNo == id).ToList();
            if (obj.Count > 0)
            {
                try
                {
                    ViewData[SYSConstant.PARAM_ID] = id;
                    FRMRequest FRMRequest = new FRMRequest();

                    var objRpt = DB.CFReportObjects.Where(w => w.ScreenID == SCREEN_ID && w.IsActive == true).ToList();
                    if (objRpt.Count > 0)
                    {
                        FRMRequest.LoadLayoutFromXml(ClsConstant.DEFAULT_REPORT_PATH + objRpt.First().ReportObject);
                    }

                    FRMRequest.Parameters["RequestNo"].Value = obj.First().RequestNo;
                    FRMRequest.Parameters["RequestNo"].Visible = false;

                    Session[this.Index_Sess_Obj + this.ActionName] = FRMRequest;
                    return PartialView("PrintForm", FRMRequest);
                }
                catch (Exception ex)
                {
                    SYEventLogObject.saveEventLog(new SYEventLog()
                    {
                        ScreenId = SCREEN_ID,
                        UserId = this.user.UserID.ToString(),
                        DocurmentAction = id,
                        Action = SYActionBehavior.ADD.ToString()
                    }, ex, true);
                }
            }
            return null;
        }
        public ActionResult DocumentViewerExportTo(string RequestNo)
        {
            ActionName = "Print";
            FRMRequest reportModel = (FRMRequest)Session[Index_Sess_Obj + ActionName];
            ViewData[SYSConstant.PARAM_ID] = RequestNo;
            return ReportViewerExtension.ExportTo(reportModel);
        }
        #endregion 
        #region 'Private Code'
        private RCMRecruitRequestObject NewRecruit()
        {
            ActionName = "Create";
            UserSession();
            DataSelector();

            RCMRecruitRequestObject BSD = new RCMRecruitRequestObject();

            BSD.Header = new RCMRecruitRequest();
            BSD.ListHeader = new List<RCMRecruitRequest>();
            BSD.Header.ExpectedDate = DateTime.Now;
            BSD.Header.RequestedDate = DateTime.Now;
            BSD.Header.Gender = "B";
            BSD.Header.StaffType = "OS";
            BSD.Header.WorkingType = "FT";
            BSD.Header.RecruitFor = "NEW";
            BSD.Header.RecruitType = "INE";
            BSD.Header.TermEmp = "Permanent";
            BSD.Header.ProposedSalaryFrom = 0;
            BSD.Header.ProposedSalaryTo = 0;
            BSD.Header.Status = SYDocumentStatus.OPEN.ToString();

            UserConfListAndForm();
            Session[Index_Sess_Obj + ActionName] = BSD;
            return BSD;
        }
        public ActionResult ShowData(string Code, string Action)
        {
            ActionName = Action;

            RCMRecruitRequestObject BSM = new RCMRecruitRequestObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMRecruitRequestObject)Session[Index_Sess_Obj + ActionName];
            }

            var _JDesc = DB.RCMSJobDescs.FirstOrDefault(w => w.Code == Code);
            if (_JDesc != null)
            {
                var result = new
                {
                    MS = SYConstant.OK,
                    //JD = "Job Responsibilities" + "\n" + _JDesc.JobResponsibility + "\n" + "Job Requirements" + "\n" + _JDesc.JobRequirement,
                    JobRespon = _JDesc.JobResponsibility,
                    JobRequire = _JDesc.JobRequirement
                };
                return Json(result, JsonRequestBehavior.DenyGet);
            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        [HttpPost]
        public ActionResult JobType(string code, string addType)
        {
            if (!string.IsNullOrEmpty(code))
            {
                var _listCom = DB.HRCompanyGroups.Where(w => w.ParentWorkGroupID == addType).ToList();
                string Res = "";
                if (_listCom.Count() > 0)
                {
                    var obj = _listCom.FirstOrDefault();
                    Res = obj.WorkGroup;
                    if (obj.WorkGroup == "Department")
                        Session["Department"] = code;
                    else if (obj.WorkGroup == "Position")
                    {
                        Session["Position"] = code;
                        Session["JD"] = code;
                    }
                }
                else
                    Session["JD"] = code;
                var result = new
                {
                    MS = SYConstant.OK,
                };
                return Json(result, JsonRequestBehavior.DenyGet);
            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        #endregion
        #region "Ajax Approval"
        public ActionResult SelectParam(string docType, string location)
        {
            UserSession();
            Session[_DOCTYPE_] = docType;
            Session[_LOCATION_] = location;
            var rs = new { MS = SYConstant.OK };
            //Auto Approval
            ActionName = "Create";
            RCMRecruitRequestObject BSM = new RCMRecruitRequestObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMRecruitRequestObject)Session[Index_Sess_Obj + ActionName];
                BSM.SetAutoApproval(docType, location, SCREEN_ID);
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        public ActionResult GridApproval()
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            RCMRecruitRequestObject BSM = new RCMRecruitRequestObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMRecruitRequestObject)Session[Index_Sess_Obj + ActionName];
            }
            DataSelector();
            return PartialView("GridApproval", BSM.ListApproval.OrderBy(w => w.ApproveLevel));
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateApproval(ExDocApproval ModelObject)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            RCMRecruitRequestObject BSM = new RCMRecruitRequestObject();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (RCMRecruitRequestObject)Session[Index_Sess_Obj + ActionName];
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
            RCMRecruitRequestObject BSM = new RCMRecruitRequestObject();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (RCMRecruitRequestObject)Session[Index_Sess_Obj + ActionName];
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

        //Edit

        [HttpPost, ValidateInput(false)]
        public ActionResult EditApproval(ExDocApproval approval)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            RCMRecruitRequestObject BSM = new RCMRecruitRequestObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMRecruitRequestObject)Session[Index_Sess_Obj + ActionName];
                var listcheck = BSM.ListApproval.Where(w => w.Approver == approval.Approver).ToList();
                if (listcheck.ToList().Count > 0)
                {
                    var objUpdate = listcheck.First();
                    objUpdate.ApproveLevel = approval.ApproveLevel;
                    Session[Index_Sess_Obj + ActionName] = BSM;
                }
            }
            DataSelector();
            return PartialView("GridApproval", BSM.ListApproval);
        }
        #endregion

        #region "Cancel"
        public ActionResult Cancel(string id)
        {
            this.UserSession();
            ViewData[SYSConstant.PARAM_ID] = id;
            RCMRecruitRequestObject BSM = new RCMRecruitRequestObject();
            if (id.ToString() != "null")
            {
                string msg = BSM.CancelDoc(id);
                if (msg == SYConstant.OK)
                {
                    SYMessages messageObject = SYMessages.getMessageObject(msg, user.Lang);
                    messageObject.DocumentNumber = id;
                    messageObject.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + id;
                    SYMessages mess = SYMessages.getMessageObject("DOC_CANCEL", user.Lang);
                    Session[SYConstant.MESSAGE_SUBMIT] = mess;
                }
                else
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                }
                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + id);
            }
            else
            {
                Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("INV_DOC", user.Lang);
            }
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + id);

        }
        #endregion
        public ActionResult GetJD()
        {
            UserSession();
            return GridViewExtension.GetComboBoxCallbackResult(cboProperties =>
            {
                cboProperties.CallbackRouteValues = new { Controller = "RCMRecruitRequest", Action = "GetJD" };
                cboProperties.Width = Unit.Percentage(100);
                cboProperties.ValueField = "Code";
                cboProperties.TextField = "Description";
                cboProperties.TextFormatString = "{1}";
                cboProperties.Columns.Add("Code", SYSettings.getLabel("Code"), 70);
                cboProperties.Columns.Add("Description", SYSettings.getLabel("Description"), 250);
                cboProperties.BindList(RCMRecruitRequestObject.GetJD());
            });
        }
        public ActionResult GetPosition()
        {
            UserSession();
            return GridViewExtension.GetComboBoxCallbackResult(cboProperties =>
            {
                cboProperties.CallbackRouteValues = new { Controller = "RCMRecruitRequest", Action = "GetPosition" };
                cboProperties.Width = Unit.Percentage(100);
                cboProperties.ValueField = "Code";
                cboProperties.TextField = "Description";
                cboProperties.TextFormatString = "{1}";
                cboProperties.Columns.Add("Code", SYSettings.getLabel("Code"), 70);
                cboProperties.Columns.Add("Description", SYSettings.getLabel("Description"), 250);
                cboProperties.Columns.Add("SecDescription", SYSettings.getLabel("Second Description"), 250);
                cboProperties.BindList(RCMRecruitRequestObject.GetPosition());
            });
        }
        private void DataSelector()
        {
            var objWf = new ExWorkFlowPreference();
            var location = "";

            if (Session[_LOCATION_] != null)
            {
                location = Session[_LOCATION_].ToString();
            }
            var docType = "";
            if (Session[_DOCTYPE_] != null)
            {
                docType = Session[_DOCTYPE_].ToString();
            }
            ViewData["WF_LIST"] = objWf.getApproverListByDocType(docType, location, SCREEN_ID);
            SYDataList objList = new SYDataList("GENDER_SELECT");
            ViewData["GENDER_SELECT"] = objList.ListData;
            ViewData["COUNTRY_SELECT"] = DB.HRCountries.ToList().OrderBy(w => w.Description);
            ViewData["NATION_SELECT"] = DB.HRNations.ToList().OrderBy(w => w.Description);
            ViewData["LEVEL_SELECT"] = SMS.HRLevels.ToList().OrderBy(w => w.SecDescription);
            //ViewData["POSITION_SELECT"] = DB.HRPositions.ToList().OrderBy(w => w.Description);
            ViewData["WORKINGTYPE_SELECT"] = DB.HRWorkingTypes.ToList().OrderBy(w => w.Description);
            ViewData["DEPARTMENT_SELECT"] = ClsFilter.LoadDepartment();
            ViewData["SECTION_SELECT"] = ClsFilter.LoadSection();
            //ViewData["BRANCHES_SELECT"] = SMS.HRBranches.ToList().OrderBy(w => w.Description);
            ViewData["BRANCHES_SELECT"] = SYConstant.getBranchDataAccess();
            objList = new SYDataList("KINDSEARCH_SELECT");
            ViewData["Kind_Search_SELECT"] = objList.ListData;
            objList = new SYDataList("TERM_RC");
            ViewData["TERM_SELECT"] = objList.ListData;
            objList = new SYDataList("RECRUITFOR_SELECT");
            ViewData["RECRUITFOR_SELECT"] = objList.ListData;
            objList = new SYDataList("RECRUITTYPE_SELECT");
            ViewData["RECRUITTYPE_SELECT"] = objList.ListData;
            ViewData["EMPCODE_SELECT"] = DBV.HR_STAFF_VIEW.ToList();
            //ViewData["JD_SELECT"] = DB.RCMSJobDescs.ToList();
            objList = new SYDataList("STAFF_TYPE");
            ViewData["STAFFTYPE_SELECT"] = objList.ListData;
        }
    }
}
