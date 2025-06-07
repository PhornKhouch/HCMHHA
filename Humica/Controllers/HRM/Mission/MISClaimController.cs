using DevExpress.Web;
using DevExpress.Web.Mvc;
using Humica.Core.DB;
using Humica.EF;
using Humica.EF.Models.SY;
using Humica.Logic.MD;
using Humica.Logic.Mission;
using Humica.Models.SY;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Humica.Controllers.HRM.RCM
{
    public class MISClaimController : Humica.EF.Controllers.MasterSaleController
    {
        private const string SCREEN_ID = "MIS0000004";
        private const string URL_SCREEN = "/HRM/Mission/MISClaim/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "ClaimCode;Status";
        private string _DOCTYPE_ = "_DOCTYPE_";
        private string _LOCATION_ = "_LOCATION_";
        private string PATH_FILE = "12313123123sadfsdfsdfsdf";
        private string PATH_FILE1 = "12313123123sadfsdfsdfsdf1";
        HumicaDBContext DB = new HumicaDBContext();
        HumicaDBViewContext DBV = new HumicaDBViewContext();
        public MISClaimController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
        }

        #region 'List' 
        public ActionResult Index()
        {
            ActionName = "Index";
            DataSelector();
            UserSession();
            UserConfListAndForm(this.KeyName);
            HRMissClaimObject BSM = new HRMissClaimObject();
            BSM.ListHeader = new List<HRMissionClaim>();
            BSM.ListHeader = DB.HRMissionClaims.ToList();
            Session[_DOCTYPE_] = "";
            Session[_LOCATION_] = "";
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        public ActionResult PartialList()
        {
            ActionName = "Index";
            HRMissClaimObject BSM = new HRMissClaimObject();
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissClaimObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView(SYListConfuration.ListDefaultView, BSM.ListHeader);
        }
        #endregion
        #region 'Create'
        public ActionResult Create()
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            HRMissClaimObject BSM = new HRMissClaimObject();
            BSM.Header = new HRMissionClaim();
            BSM.ListItem = new List<HRMissionClaimItem>();
            BSM.ListApproval = new List<ExDocApproval>();
            BSM.Header.ClaimDate = DateTime.Now;
            BSM.Header.Status = SYDocumentStatus.OPEN.ToString();
            BSM.Header.TotalAmount = 0;
            Session["Type"] = "";
            if (Session[PATH_FILE] != null)
            {
                BSM.Header.WorkingPlan = Session[PATH_FILE].ToString();
                Session[PATH_FILE] = null;
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            LoadSession(BSM.Header.ClaimType, BSM.Header.Branch);
            return View(BSM);
        }
        [HttpPost]
        public ActionResult Create(HRMissClaimObject collection)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.ADD);

            HRMissClaimObject BSM = new HRMissClaimObject();
            if (ModelState.IsValid)
            {
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (HRMissClaimObject)Session[Index_Sess_Obj + ActionName];
                    BSM.Header = collection.Header;
                }

                if (Session[PATH_FILE] != null)
                {
                    BSM.Header.WorkingPlan = Session[PATH_FILE].ToString();
                    Session[PATH_FILE] = null;
                }
                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.CreateMissClaim();

                if (msg == SYConstant.OK)
                {
                    SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                    mess.DocumentNumber = BSM.Header.ClaimCode;
                    mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + mess.DocumentNumber;

                    Session[Index_Sess_Obj + ActionName] = BSM;
                    Session[SYConstant.MESSAGE_SUBMIT] = mess;
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Create");
                }
                else
                {
                    Session[Index_Sess_Obj + this.ActionName] = BSM;
                    ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                    return View(BSM);
                }
            }
            Session[Index_Sess_Obj + this.ActionName] = BSM;
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            return View(BSM);
        }
        #endregion 
        #region 'Edit'
        public ActionResult Edit(string id)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            HRMissClaimObject BSM = new HRMissClaimObject();
            UserConfListAndForm();
            Session["Type"] = "";

            BSM.Header = DB.HRMissionClaims.FirstOrDefault(w => w.ClaimCode == id);
            if (BSM.Header != null)
            {
                if (Session[PATH_FILE] != null)
                {
                    BSM.Header.WorkingPlan = Session[PATH_FILE].ToString();
                    Session[PATH_FILE] = null;
                }
                BSM.ListItem = DB.HRMissionClaimItems.Where(w => w.ClaimCode == id).ToList();
                BSM.ListApproval = DB.ExDocApprovals.Where(w => w.DocumentNo == id && w.DocumentType == BSM.Header.ClaimType).ToList();
                Session[Index_Sess_Obj + ActionName] = BSM;
                LoadSession(BSM.Header.ClaimType, BSM.Header.Branch);
                return View(BSM);
            }
            Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE");
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        [HttpPost]
        public ActionResult Edit(string id, HRMissClaimObject collection)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();

            HRMissClaimObject BSM = new HRMissClaimObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissClaimObject)Session[Index_Sess_Obj + ActionName];
                BSM.Header = collection.Header;
            }

            if (Session[PATH_FILE] != null)
            {
                BSM.Header.WorkingPlan = Session[PATH_FILE].ToString();
                Session[PATH_FILE] = null;
            }
            else
            {
                BSM.Header.WorkingPlan = collection.Header.WorkingPlan;
            }
            BSM.ScreenId = SCREEN_ID;

            if (ModelState.IsValid)
            {
                string msg = BSM.UpdateClaim(id);

                if (msg != SYConstant.OK)
                {
                    SYMessages mess_err = SYMessages.getMessageObject(msg, user.Lang);
                    Session[Index_Sess_Obj + this.ActionName] = BSM;
                    UserConfForm(ActionBehavior.SAVEGRID);
                    Session[SYConstant.MESSAGE_SUBMIT] = mess_err;
                    return View(BSM);
                }

                SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                mess.DocumentNumber = id;
                mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + mess.DocumentNumber; ;
                Session[Index_Sess_Obj + this.ActionName] = BSM;

                Session[SYConstant.MESSAGE_SUBMIT] = mess;
                return View(BSM);
            }
            Session[Index_Sess_Obj + this.ActionName] = BSM;
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            return View(BSM);
        }
        #endregion 
        #region 'Details'
        public ActionResult Details(string id)
        {
            ActionName = "Details";
            UserSession();
            UserConfForm(SYActionBehavior.VIEW);
            DataSelector();

            HRMissClaimObject BSM = new HRMissClaimObject();
            ViewData[SYConstant.PARAM_ID] = id;
            ViewData[ClsConstant.IS_READ_ONLY] = true;

            BSM.Header = DB.HRMissionClaims.FirstOrDefault(w => w.ClaimCode == id);
            if (BSM.Header == null)
            {
                Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE");
                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
            }
            if (Session[PATH_FILE] != null)
            {
                BSM.Header.WorkingPlan = Session[PATH_FILE].ToString();
                Session[PATH_FILE] = null;
            }
            BSM.ListItem = DB.HRMissionClaimItems.Where(w => w.ClaimCode == id).ToList();
            BSM.ListApproval = DB.ExDocApprovals.Where(w => w.DocumentNo == id && w.DocumentType == BSM.Header.ClaimType).ToList();
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        #endregion
        #region 'Delete'  
        public ActionResult Delete(string id)
        {
            UserSession();
            HRMissClaimObject BSM = new HRMissClaimObject();
            if (id != null)
            {
                string msg = BSM.DeleteClaim(id);

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
        #region 'Grid Item'
        public ActionResult GridItem()
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRMissClaimObject BSM = new HRMissClaimObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissClaimObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("GridItem", BSM.ListItem);
        }
        public ActionResult GridItemDetails()
        {
            ActionName = "Details";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRMissClaimObject BSM = new HRMissClaimObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissClaimObject)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("GridItemDetails", BSM.ListItem);
        }
        public ActionResult CreatePItem(HRMissionClaimItem ModelObject)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRMissClaimObject BSM = new HRMissClaimObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissClaimObject)Session[Index_Sess_Obj + ActionName];
            }
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (HRMissClaimObject)Session[Index_Sess_Obj + ActionName];
                    }
                    if (BSM.ListItem.Count == 0)
                    {
                        ModelObject.LineItem = 1;
                    }
                    else
                    {
                        ModelObject.LineItem = BSM.ListItem.Max(w => w.LineItem) + 1;
                    }
                    if (Session[PATH_FILE1] != null)
                    {
                        ModelObject.Attachment = Session[PATH_FILE1].ToString();
                        Session[PATH_FILE] = null;
                    }
                    BSM.ListItem.Add(ModelObject);
                    Session[Index_Sess_Obj + ActionName] = BSM;

                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                ViewData["EditError"] = SYMessages.getMessage("EE001", user.Lang);
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("GridItem", BSM.ListItem);
        }
        public ActionResult EditPItem(HRMissionClaimItem ModelObject)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            HRMissClaimObject BSM = new HRMissClaimObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissClaimObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListItem.Where(w => w.LineItem == ModelObject.LineItem).ToList();
                if (Session[PATH_FILE1] != null)
                {
                    ModelObject.Attachment = Session[PATH_FILE1].ToString();
                    Session[PATH_FILE] = null;
                }
                else
                {
                    ModelObject.Attachment = objCheck.FirstOrDefault().Attachment;
                }
                if (objCheck.Count > 0)
                {
                    objCheck.First().MissionDate = ModelObject.MissionDate;
                    objCheck.First().WorkingPlan = ModelObject.WorkingPlan;
                    objCheck.First().NumOfPer = ModelObject.NumOfPer;
                    objCheck.First().QtyInvoice = ModelObject.QtyInvoice;
                    objCheck.First().Duration = ModelObject.Duration;
                    objCheck.First().Amount = ModelObject.Amount;
                    objCheck.First().Remark = ModelObject.Remark;
                    objCheck.First().Attachment = ModelObject.Attachment;
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return PartialView("GridItem", BSM.ListItem);
        }
        public ActionResult DeletePItem(int LineItem)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            HRMissClaimObject BSM = new HRMissClaimObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissClaimObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListItem.Where(w => w.LineItem == LineItem).ToList();

                if (objCheck.Count > 0)
                {
                    BSM.ListItem.Remove(objCheck.First());
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return PartialView("GridItem", BSM.ListItem);
        }
        #endregion 
        public ActionResult RequestForApproval(string id)
        {
            UserSession();
            HRMissClaimObject BSM = new HRMissClaimObject();
            if (id != null)
            {
                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.requestToApprove(id);
                if (msg == SYConstant.OK)
                {
                    var mess = SYMessages.getMessageObject("DOC_RQ", user.Lang);
                    mess.DocumentNumber = id;
                    mess.Description = mess.Description;
                    mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + id;
                    Session[Index_Sess_Obj + ActionName] = null;
                    Session[SYConstant.MESSAGE_SUBMIT] = mess;
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
                }
            }
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_EV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        public ActionResult Approve(string id)
        {
            UserSession();
            HRMissClaimObject BSM = new HRMissClaimObject();
            if (id != null)
            {

                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.approveTheDoc(id);
                if (msg == SYConstant.OK)
                {
                    var mess = SYMessages.getMessageObject("DOC_APPROVED", user.Lang);
                    mess.DocumentNumber = id;
                    mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + id;
                    Session[Index_Sess_Obj + ActionName] = null;
                    Session[SYConstant.MESSAGE_SUBMIT] = mess;
                }
                else
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                }
                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
            }
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_EV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        #region "Cancel"
        public ActionResult Cancel(string id)
        {
            this.UserSession();
            ViewData[SYSConstant.PARAM_ID] = id;
            HRMissClaimObject BSM = new HRMissClaimObject();
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
        public ActionResult Refreshvalue(string id, string action)
        {
            ActionName = action;
            HRMissClaimObject BSM = new HRMissClaimObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissClaimObject)Session[Index_Sess_Obj + ActionName];
                if (BSM.Header == null)
                {
                    BSM.Header = new HRMissionClaim();
                }

                BSM.Header.TotalAmount = 0;
                foreach (var item in BSM.ListItem)
                {
                    BSM.Header.TotalAmount += Convert.ToDecimal(item.Amount);
                }

                var result = new
                {
                    MS = SYConstant.OK,
                    TotalAmount = BSM.Header.TotalAmount,
                };

                return Json(result, JsonRequestBehavior.DenyGet);

            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }

        #region 'Upload'
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
        public ActionResult UploadControlCallbackActionClaim(HttpPostedFileBase file_Uploader)
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

            UploadedFile[] files = UploadControlExtension.GetUploadedFiles("FileUploadClaim",
                sfi.ValidationSettings,
                sfi.uc_FileUploadCompleteFile);
            Session[PATH_FILE1] = sfi.ObjectTemplate.UpoadPath; ;
            return null;
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
            HRMissClaimObject BSM = new HRMissClaimObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissClaimObject)Session[Index_Sess_Obj + ActionName];
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
            HRMissClaimObject BSM = new HRMissClaimObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissClaimObject)Session[Index_Sess_Obj + ActionName];
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
            HRMissClaimObject BSM = new HRMissClaimObject();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (HRMissClaimObject)Session[Index_Sess_Obj + ActionName];
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
            HRMissClaimObject BSM = new HRMissClaimObject();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (HRMissClaimObject)Session[Index_Sess_Obj + ActionName];
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

        #region 'private code'
        public ActionResult SelectMItemElement(string Actionname, string Code)
        {
            ActionName = Actionname;
            Session["Type"] = Code;
            UserSession();
            HRMissClaimObject BSM = new HRMissClaimObject();
            var listBranch = SYConstant.getBranchDataAccess();
            ViewData[SYSConstant.PARAM_ID] = Code;
            if (Session[Index_Sess_Obj + this.ActionName] != null)
            {
                BSM = (HRMissClaimObject)Session[Index_Sess_Obj + this.ActionName];
            }
            var data = new
            {
                MS = SYSConstant.OK
            };
            return Json(data, (JsonRequestBehavior)1);

        }

        public ActionResult MissSetupItem()
        {
            UserSession();
            return GridViewExtension.GetComboBoxCallbackResult(cboProperties =>
            {
                cboProperties.CallbackRouteValues = new { Controller = "MISPlan", Action = "MissSetupItem" };
                cboProperties.ValueField = "Code";
                cboProperties.TextField = "Description";
                cboProperties.TextFormatString = "{0}:{1}";
                cboProperties.Columns.Add("Code", Humica.EF.Models.SY.SYSettings.getLabel("Code"));
                cboProperties.Columns.Add("Description", Humica.EF.Models.SY.SYSettings.getLabel("Description"));
                cboProperties.BindList(Humica.Logic.Mission.HRMissClaimObject.GetAllMissItem());
            });
        }

        private void DataSelector()
        {
            ViewData["MISSION_TYPE_SELECT"] = DB.ExCfWorkFlowItems.Where(w => w.ScreenID == SCREEN_ID).ToList();
            SYDataList objListMiss_Type = new SYDataList("TRAVEL_BY");
            ViewData["TRAVEL_BY_SELECT"] = objListMiss_Type.ListData;
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

            //objList = new SYDataList("MARITAL");
            //ViewData["MARITAL_SELECT"] = objList.ListData;
            //objList = new SYDataList("LANG_SKILLS");
            //ViewData["LANGSKILLS_SELECT"] = objList.ListData;
            //ViewData["BRANCHES_SELECT"] = DH.HRBranches.ToList().OrderBy(w => w.Description);
            //ViewData["COUNTRY_SELECT"] = DH.HRCountries.ToList().OrderBy(w => w.Description);
            //ViewData["NATION_SELECT"] = DH.HRNations.ToList().OrderBy(w => w.Description);
            //ViewData["RelationshipType_LIST"] = DH.HRRelationshipTypes.ToList();
            //ViewData["HREmpEduType_LIST"] = DH.HREduTypes.ToList();
            //var Processing = SYDocumentStatus.PROCESSING.ToString();
            //ViewData["VACANCY_SELECT"] = DB.RCMVacancies.Where(m => m.Status == Processing).ToList();
            //ViewData["LANG_SELECT"] = DH.RCLangs.ToList().OrderBy(w => w.Lang);
            //ViewData["POSITION_SELECT"] = DH.HRPositions.ToList().OrderBy(w => w.Description);
            ViewData["BRANCHES_SELECT"] = SYConstant.getBranchDataAccess();
            //ViewData["CHANNELRECEIVED_SELECT"] = DH.RCAdvertisers.ToList().OrderBy(w => w.Company);
            ViewData["MissType_SELECT"] = DB.HRMissTypes.ToList().OrderBy(w => w.Description);
            ViewData["PROVICES_SELECT"] = DB.HRProvices.ToList().OrderBy(w => w.Description);
            ViewData["STAFF_SELECT"] = DBV.HR_STAFF_VIEW.ToList();
        }
        private void LoadSession(string docType, string location)
        {
            Session[_DOCTYPE_] = docType;
            Session[_LOCATION_] = location;
        }
        #endregion 
    }
}
