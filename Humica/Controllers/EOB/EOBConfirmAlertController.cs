using DevExpress.Web.Mvc;
using Humica.Core.DB;
using Humica.EF;
using Humica.EF.Models.SY;
using Humica.Logic;
using Humica.Logic.EOB;
using Humica.Models.SY;
using HUMICA.Models.Report.EOB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.EOB
{
    public class EOBConfirmAlertController : Humica.EF.Controllers.MasterSaleController
    {
        private const string SCREEN_ID = "EOB0000001";
        private const string URL_SCREEN = "/EOB/EOBConfirmAlert/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "ID";
        private string PATH_FILE = "12313123123sadfsdfsdfsdf";
        HumicaDBContext DB = new HumicaDBContext();
        public EOBConfirmAlertController()
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
            ConfirmAlertObject BSM = new ConfirmAlertObject();
            BSM.ListHeader = new List<EOBConfirmAlert>();
            BSM.ListApplicant = new List<RCMApplicant>();
            BSM.ListHeader = DB.EOBConfirmAlerts.ToList();
            var _Confirmed = DB.EOBConfirmAlerts.Select(x => x.ID).ToList();
            BSM.ListApplicant = DB.RCMApplicants.Where(w => !_Confirmed.Contains(w.ApplicantID) && w.IntVStatus == "PASS").ToList();
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        #endregion
        #region 'Create'
        public ActionResult Create(string ApplicantID)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();

            ConfirmAlertObject BSD = new ConfirmAlertObject();

            BSD.Header = new EOBConfirmAlert();
            BSD.ListHeader = new List<EOBConfirmAlert>();
            BSD.Applicant = new RCMApplicant();
            BSD.Applicant = DB.RCMApplicants.FirstOrDefault(w => w.ApplicantID == ApplicantID);
            var _chkID = DB.RCMPInterviews.Select(w => w.ApplicantID);
            var _chkApplicant = DB.RCMApplicants.FirstOrDefault(w => _chkID.Contains(w.ApplicantID));
            if (_chkApplicant != null)
            {
                BSD.Header.Remark = _chkApplicant.Email;
            }
            if (BSD.Applicant != null)
            {
                BSD.Header.ID = BSD.Applicant.ApplicantID;
            }
            BSD.Header.Confirmed = false;
            BSD.Header.DateOfSending = DateTime.Now;
            BSD.Header.SendingSelected = "EM";

            UserConfListAndForm();
            Session[Index_Sess_Obj + ActionName] = BSD;
            return View(BSD);
        }
        [HttpPost]
        public ActionResult Create(string ApplicantID, ConfirmAlertObject collection)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.ADD);

            var BSM = (ConfirmAlertObject)Session[Index_Sess_Obj + ActionName];

            if (Session[PATH_FILE] != null)
            {
                collection.Header.AttachForm = Session[PATH_FILE].ToString();
                Session[PATH_FILE] = null;
            }
            string msg = collection.ConfAlert(ApplicantID);

            if (msg == SYConstant.OK)
            {
                SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                Session[Index_Sess_Obj + ActionName] = BSM;
                Session[SYConstant.MESSAGE_SUBMIT] = mess;
                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Create");
            }
            Session[Index_Sess_Obj + this.ActionName] = collection;
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
            return View(collection);
        }
        #endregion 
        #region 'Edit'
        public ActionResult Edit(string ID)
        {
            ActionName = "Edit";
            UserSession();
            DataSelector();
            ConfirmAlertObject BSM = new ConfirmAlertObject();
            UserConfListAndForm();

            BSM.Header = DB.EOBConfirmAlerts.FirstOrDefault(w => w.ID == ID);
            if (BSM.Header != null)
            {
                if (BSM.Header.Confirmed != true)
                {
                    BSM.Header.Confirmed = true;
                    Session[Index_Sess_Obj + ActionName] = BSM;
                    return View(BSM);
                }
                else
                {
                    Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV");
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
                }
            }
            Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE");
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        [HttpPost]
        public ActionResult Edit(string ID, ConfirmAlertObject collection)
        {
            ActionName = "Edit";
            UserSession();
            DataSelector();
            UserConfListAndForm();

            ConfirmAlertObject BSM = new ConfirmAlertObject();

            BSM = (ConfirmAlertObject)Session[Index_Sess_Obj + ActionName];
            collection.ScreenId = SCREEN_ID;

            if (Session[PATH_FILE] != null)
            {
                collection.Header.AttachForm = Session[PATH_FILE].ToString();
                Session[PATH_FILE] = null;
            }
            else
            {
                collection.Header.AttachForm = BSM.Header.AttachForm;
            }
            if (ModelState.IsValid)
            {
                string msg = collection.updConfirm(ID);

                if (msg != SYConstant.OK)
                {
                    SYMessages mess_err = SYMessages.getMessageObject(msg, user.Lang);
                    Session[Index_Sess_Obj + this.ActionName] = BSM;
                    UserConfForm(ActionBehavior.SAVEGRID);
                    Session[SYConstant.MESSAGE_SUBMIT] = mess_err;
                    return View(collection);
                }

                SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                mess.DocumentNumber = ID;
                mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?ID=" + mess.DocumentNumber; ;
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
        #region 'Details'
        public ActionResult Details(string ID)
        {
            ActionName = "Details";
            UserSession();
            ConfirmAlertObject BSM = new ConfirmAlertObject();
            DataSelector();
            ViewData[SYConstant.PARAM_ID] = ID;
            ViewData[ClsConstant.IS_READ_ONLY] = true;
            BSM.Header = DB.EOBConfirmAlerts.FirstOrDefault(w => w.ID == ID);
            if (BSM.Header == null)
            {
                Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("Error");
                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
            }
            UserConfForm(SYActionBehavior.VIEW);
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        #endregion
        #region 'Grid'
        public ActionResult GridItems()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            ConfirmAlertObject BSM = new ConfirmAlertObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ConfirmAlertObject)Session[Index_Sess_Obj + ActionName];
            }

            return PartialView("GridItems", BSM.ListApplicant);
        }
        public ActionResult GridConfirms()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            ConfirmAlertObject BSM = new ConfirmAlertObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ConfirmAlertObject)Session[Index_Sess_Obj + ActionName];
            }

            return PartialView("GridConfirms", BSM.ListHeader);
        }
        #endregion 
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
        #endregion 
        #region "Confirm"
        public ActionResult Confirm(string id)
        {
            this.UserSession();
            DataSelector();
            UserConfListAndForm();
            ViewData[SYSConstant.PARAM_ID] = id;
            ConfirmAlertObject BSD = new ConfirmAlertObject();
            if (id.ToString() != "null")
            {
                string sms = BSD.ApproveTheDoc(id);
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
        public ActionResult ReleaseDoc(string id)
        {
            UserSession();
            ConfirmAlertObject BSM = new ConfirmAlertObject();
            if (!string.IsNullOrEmpty(id))
            {
                BSM.ScreenId = SCREEN_ID;
                FRM_ConfirmAlert sa = new FRM_ConfirmAlert();
                var objRpt = DB.CFReportObjects.Where(w => w.ScreenID == SCREEN_ID && w.IsActive == true).ToList();
                if (objRpt.Count > 0)
                {
                    sa.LoadLayoutFromXml(ClsConstant.DEFAULT_REPORT_PATH + objRpt.First().ReportObject);
                }
                sa.Parameters["ID"].Value = id;
                sa.Parameters["ID"].Visible = false;

                Session[this.Index_Sess_Obj + this.ActionName] = sa;

                string fileName = Server.MapPath("~/Content/UPLOAD/" + id + ".pdf");
                string UploadDirectory = Server.MapPath("~/Content/UPLOAD");
                if (!Directory.Exists(UploadDirectory))
                {
                    Directory.CreateDirectory(UploadDirectory);
                }
                sa.ExportToPdf(fileName);

                string msg = BSM.requestApprove(id, fileName);
                if (msg == SYConstant.OK)
                {
                    var mess = SYMessages.getMessageObject("DOC_RELEASED", user.Lang);
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
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
                }
            }
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_EV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        public ActionResult Consider(string id)
        {
            UserSession();
            ConfirmAlertObject BSM = new ConfirmAlertObject();
            if (!string.IsNullOrEmpty(id))
            {
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (ConfirmAlertObject)Session[Index_Sess_Obj + ActionName];
                }
                BSM.ScreenId = SCREEN_ID;

                string msg = BSM.Consider(id);
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
                }
            }
            else
            {
                Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_EV", user.Lang);
            }
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        #region "Reject"
        public ActionResult Reject(string id)
        {
            this.UserSession();
            DataSelector();
            UserConfListAndForm();
            ViewData[SYSConstant.PARAM_ID] = id;
            ConfirmAlertObject BSD = new ConfirmAlertObject();
            if (!string.IsNullOrEmpty(id))
            {
                string sms = BSD.RejectTheDoc(id);
                if (sms == SYConstant.OK)
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
        #region 'private code'
        private void DataSelector()
        {
            SYDataList objList = new SYDataList("SEX");
            ViewData["GENDER_SELECT"] = objList.ListData;
            objList = new SYDataList("MARITAL");
            ViewData["MARITAL_SELECT"] = objList.ListData;
            objList = new SYDataList("LANG_SKILLS");
            ViewData["POSITION_SELECT"] = ClsFilter.LoadPosition();
        }
        #endregion 
    }
}
