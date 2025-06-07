using DevExpress.Web.Mvc;
using Humica.Core.DB;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic.RCM;
using Humica.Models.Report.HRM;
using Humica.Models.SY;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SunPeople.Controllers.HRM.RCM
{

    public class RCMERecruitmentController : Humica.EF.Controllers.MasterSaleController
    {

        private const string SCREEN_ID = "RCM0000010";
        private const string URL_SCREEN = "/HRM/RCM/RCMERecruitment/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "JobID";
        private string DocType = "ERE01";
        private string PATH_FILE = "12313123123sadfsdfsdfsdf";

        HumicaDBContext DB = new HumicaDBContext();
        public RCMERecruitmentController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
        }

        #region 'List' 
        public ActionResult Index()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            DataSelector();

            RCMERecruitObject BSM = new RCMERecruitObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMERecruitObject)Session[Index_Sess_Obj + ActionName];
            }
            BSM.ListPending = new List<RCMVacancy>();
            BSM.ListHeader = new List<RCMERecruit>();
            string approved = SYDocumentStatus.APPROVED.ToString();
            var ListHeader = DB.RCMVacancies.ToList();
            var _Vac = DB.RCMVacancies.Select(x => x.DocRef).ToList();
            var ListPending = DB.RCMRecruitRequests.Where(w => w.Status == approved && !_Vac.Contains(w.RequestNo)).OrderByDescending(x => x.RequestNo).ToList();
            //BSM.ListPending = ListPending.ToList();
            //BSM.ListHeader = ListHeader.ToList();
            BSM.ListHeader = DB.RCMERecruits.ToList();
            Session[Index_Sess_Obj + ActionName] = BSM;

            return View(BSM);
        }
        public ActionResult PendingList()
        {
            ActionName = "Index";
            UserSession();
            DataSelector();
            UserConfList(KeyName);
            RCMVacancyObject BSM = new RCMVacancyObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMVacancyObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("PendingList", BSM.ListPending);
        }
        public ActionResult GridView()
        {
            ActionName = "Index";
            UserSession();
            DataSelector();
            UserConfList(KeyName);
            RCMVacancyObject BSM = new RCMVacancyObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMVacancyObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("GridView", BSM.ListHeader);
        }
        #endregion 
        #region 'Create'
        public ActionResult Create()
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMERecruitObject BSD = new RCMERecruitObject();

            BSD.Header = new RCMERecruit();
            BSD.ListHeader = new List<RCMERecruit>();
            //BSD.ListAdvertise = new List<RCMVAdvertisePlace>();
            BSD.Header.PosterNo = 0;
            BSD.Header.DatePosting = DateTime.Now;
            BSD.Header.DateLine = DateTime.Now;
            BSD.Header.Status = SYDocumentStatus.OPEN.ToString();

            UserConfListAndForm();
            Session[Index_Sess_Obj + ActionName] = BSD;
            return View(BSD);
        }
        [HttpPost]
        public ActionResult Create(RCMERecruitObject collection)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.ADD);
            RCMERecruitObject BSM = new RCMERecruitObject();
            if (Session[PATH_FILE] != null)
            {
                collection.Header.Attachfile = Session[PATH_FILE].ToString();
                Session[PATH_FILE] = null;
            }
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMERecruitObject)Session[Index_Sess_Obj + ActionName];
            }
            BSM.DocType = DocType;
            BSM.Header = collection.Header;

            if (ModelState.IsValid)
            {
                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.createERecruit();

                if (msg == SYConstant.OK)
                {
                    SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                    mess.DocumentNumber = BSM.Header.JobID;
                    mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?JobID=" + mess.DocumentNumber;

                    Session[Index_Sess_Obj + ActionName] = BSM;
                    Session[SYConstant.MESSAGE_SUBMIT] = mess;
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Create");
                }
                Session[Index_Sess_Obj + this.ActionName] = BSM;
                ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                return View(BSM);
            }
            Session[Index_Sess_Obj + this.ActionName] = BSM;
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            return View(BSM);
        }
        #endregion
        #region 'Edit'
        public ActionResult Edit(string JobID)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            RCMERecruitObject BSM = new RCMERecruitObject();
            UserConfListAndForm();
            BSM.Header = new RCMERecruit();
            BSM.Header = DB.RCMERecruits.FirstOrDefault(w => w.JobID == JobID);
            if (BSM.Header != null)
            {
                string Posted = SYDocumentStatus.POSTED.ToString();
                if (BSM.Header.Status != Posted)
                {
                    //BSM.ListAdvertise = DB.RCMVAdvertisePlaces.Where(w => w.Code == JobID).ToList();
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
        public ActionResult Edit(string JobID, RCMERecruitObject collection)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();

            RCMERecruitObject BSM = new RCMERecruitObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMERecruitObject)Session[Index_Sess_Obj + ActionName];
            }
            if (Session[PATH_FILE] != null)
            {
                collection.Header.Attachfile = Session[PATH_FILE].ToString();
                Session[PATH_FILE] = null;
            }
            else
            {
                collection.Header.Attachfile = BSM.Header.Attachfile;
            }
            BSM.DocType = DocType;
            BSM.ScreenId = SCREEN_ID;
            //BSM.Header = collection.Header;
            if (ModelState.IsValid)
            {
                string msg = collection.updERecruit(JobID);
                if (msg != SYConstant.OK)
                {
                    SYMessages mess_err = SYMessages.getMessageObject(msg, user.Lang);
                    Session[Index_Sess_Obj + this.ActionName] = BSM;
                    Session[SYConstant.MESSAGE_SUBMIT] = mess_err;
                    return View(BSM);
                }

                SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                mess.DocumentNumber = JobID;
                mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?JobID=" + mess.DocumentNumber; ;
                Session[Index_Sess_Obj + this.ActionName] = BSM;
                UserConfForm(ActionBehavior.SAVEGRID);
                Session[SYConstant.MESSAGE_SUBMIT] = mess;
                return View(BSM);
            }
            Session[Index_Sess_Obj + this.ActionName] = BSM;
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            return View(BSM);
        }
        #endregion
        #region 'Details'
        public ActionResult Details(string JobID)
        {
            ActionName = "Details";
            UserSession();
            RCMERecruitObject BSM = new RCMERecruitObject();
            DataSelector();
            ViewData[SYConstant.PARAM_ID] = JobID;
            ViewData[ClsConstant.IS_READ_ONLY] = true;
            BSM.Header = DB.RCMERecruits.FirstOrDefault(w => w.JobID == JobID);
            if (BSM.Header == null)
            {
                Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE");
                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
            }
            //BSM.ListAdvertise = DB.RCMVAdvertisePlaces.Where(w => w.Code == JobID).ToList();
            UserConfForm(SYActionBehavior.VIEW);
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        #endregion 
        #region 'Delete'  
        public ActionResult Delete(string JobID)
        {
            UserSession();
            RCMERecruitObject BSM = new RCMERecruitObject();

            BSM.Header = DB.RCMERecruits.FirstOrDefault(w => w.JobID == JobID);

            if (BSM.Header != null)
            {

                string msg = BSM.deleteERecruit(JobID);

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
            var obj = DB.RCMERecruits.Where(w => w.JobID == id).ToList();
            if (obj.Count > 0)
            {
                try
                {
                    ViewData[SYSConstant.PARAM_ID] = id;
                    FRMERecruitment FRMERecruitment = new FRMERecruitment();
                    FRMERecruitment.Parameters["JobID"].Value = obj.First().JobID;
                    FRMERecruitment.Parameters["JobID"].Visible = false;
                    Session[this.Index_Sess_Obj + this.ActionName] = FRMERecruitment;
                    return PartialView("PrintForm", FRMERecruitment);
                }
                catch (Exception ex)
                {
                    SYEventLogObject.saveEventLog(new SYEventLog()
                    {
                        ScreenId = "RCM0000010",
                        UserId = this.user.UserID.ToString(),
                        DocurmentAction = id,
                        Action = SYActionBehavior.ADD.ToString()
                    }, ex, true);
                }
            }
            return null;
        }
        public ActionResult DocumentViewerExportTo(string JobID)
        {
            ActionName = "Print";
            FRMERecruitment reportModel = (FRMERecruitment)Session[Index_Sess_Obj + ActionName];
            ViewData[SYSConstant.PARAM_ID] = JobID;
            return ReportViewerExtension.ExportTo(reportModel);
        }
        #endregion
        public async Task<ActionResult> Processing(string JobID, int mt)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMERecruitObject BSD = new RCMERecruitObject();


            BSD.Headersocial = new SYSocialMedia();
            BSD.Header = new RCMERecruit();
            BSD.ListHeader = new List<RCMERecruit>();
            //BSD.ListAdvertise = new List<RCMVAdvertisePlace>();
            BSD.Header.PosterNo = 0;
            BSD.Header.DatePosting = DateTime.Now;
            BSD.Header.DateLine = DateTime.Now;
            BSD.Header.Status = SYDocumentStatus.OPEN.ToString();
            var listtoken = DB.SYSocialMedias.Where(w => w.ID == mt).FirstOrDefault();
            var captions = DB.RCMERecruits.Where(w => w.JobID == JobID).FirstOrDefault();
            string accessToken = listtoken.AccessToken;
            string caption = captions.ContactInfo;
            if (captions.Attachfile != null)
            {
                string[] fileName = captions.Attachfile.Split('~');
                string FileNames = Server.MapPath(captions.Attachfile);
                try
                {
                    await PostPhotoToFacebook(accessToken, FileNames, caption);
                    //Console.WriteLine("Photo posted successfully on Facebook.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
            UserConfListAndForm();
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("TRN_COM", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        public ActionResult PartialList()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            RCMERecruitObject BSM = new RCMERecruitObject();
            BSM.ListHeader = new List<RCMERecruit>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMERecruitObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView(SYListConfuration.ListDefaultView, BSM.ListHeader);
        }
        #region 'GridAdvertise'
        //public ActionResult GridAdvertise()
        //{
        //    ActionName = "Create";
        //    UserSession();
        //    UserConfListAndForm();
        //    DataSelector();
        //    RCMERecruitObject BSM = new RCMERecruitObject();

        //    if (Session[Index_Sess_Obj + ActionName] != null)
        //    {
        //        BSM = (RCMERecruitObject)Session[Index_Sess_Obj + ActionName];
        //    }

        //    return PartialView("GridAdvertise", BSM.ListAdvertise);
        //}
        //public ActionResult GridAdvertiseDetails()
        //{
        //    ActionName = "Details";
        //    UserSession();
        //    UserConfListAndForm();
        //    DataSelector();
        //    RCMERecruitObject BSM = new RCMERecruitObject();
        //    if (Session[Index_Sess_Obj + ActionName] != null)
        //    {
        //        BSM = (RCMERecruitObject)Session[Index_Sess_Obj + ActionName];
        //    }
        //    Session[Index_Sess_Obj + ActionName] = BSM;
        //    return PartialView("GridAdvertiseDetails", BSM.ListAdvertise);
        //}
        //public ActionResult CreateAds(RCMVAdvertisePlace ModelObject)
        //{
        //    ActionName = "Create";
        //    UserSession();
        //    UserConfListAndForm();
        //    DataSelector();
        //    RCMERecruitObject BSM = new RCMERecruitObject();
        //    try
        //    {
        //        if (Session[Index_Sess_Obj + ActionName] != null)
        //        {
        //            BSM = (RCMERecruitObject)Session[Index_Sess_Obj + ActionName];
        //            if (Session[PATH_FILE] != null)
        //            {
        //                ModelObject.Attachment = Session[PATH_FILE].ToString();
        //                Session[PATH_FILE] = null;
        //            }
        //            if (BSM.ListAdvertise.Count == 0)
        //            {
        //                ModelObject.LineItem = 1;
        //            }
        //            else
        //            {
        //                ModelObject.LineItem = BSM.ListAdvertise.Max(w => w.LineItem) + 1;
        //            }
        //            BSM.ListAdvertise.Add(ModelObject);
        //            Session[Index_Sess_Obj + ActionName] = BSM;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        ViewData["EditError"] = e.Message;
        //    }
        //    Session[Index_Sess_Obj + ActionName] = BSM;
        //    return PartialView("GridAdvertise", BSM.ListAdvertise);
        //}
        //public ActionResult EditAds(RCMVAdvertisePlace ModelObject)
        //{
        //    ActionName = "Create";
        //    UserSession();
        //    DataSelector();
        //    UserConfListAndForm();
        //    RCMERecruitObject BSM = new RCMERecruitObject();

        //    if (Session[Index_Sess_Obj + ActionName] != null)
        //    {
        //        BSM = (RCMERecruitObject)Session[Index_Sess_Obj + ActionName];
        //        var objCheck = BSM.ListAdvertise.Where(w => w.LineItem == ModelObject.LineItem).ToList();
        //        if (Session[PATH_FILE] != null)
        //        {
        //            ModelObject.Attachment = Session[PATH_FILE].ToString();
        //            Session[PATH_FILE] = null;
        //        }
        //        else
        //        {
        //            ModelObject.Attachment = objCheck.FirstOrDefault().Attachment;
        //        }
        //        if (objCheck.Count > 0)
        //        {
        //            objCheck.First().Advertiser = ModelObject.Advertiser;
        //            objCheck.First().Description = ModelObject.Description;
        //            objCheck.First().TotalCost = ModelObject.TotalCost;
        //            objCheck.First().TotalBudget = ModelObject.TotalBudget;
        //            objCheck.First().Remark = ModelObject.Remark;
        //            objCheck.First().Attachment = ModelObject.Attachment;
        //        }
        //        else
        //        {
        //            ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
        //        }
        //        Session[Index_Sess_Obj + ActionName] = BSM;
        //    }
        //    return PartialView("GridAdvertise", BSM.ListAdvertise);
        //}
        //public ActionResult DeleteAds(int LineItem)
        //{
        //    ActionName = "Create";
        //    UserSession();
        //    DataSelector();
        //    UserConfListAndForm();
        //    RCMERecruitObject BSM = new RCMERecruitObject();

        //    if (Session[Index_Sess_Obj + ActionName] != null)
        //    {
        //        BSM = (RCMERecruitObject)Session[Index_Sess_Obj + ActionName];

        //        var objCheck = BSM.ListAdvertise.Where(w => w.LineItem == LineItem).ToList();

        //        if (objCheck.Count > 0)
        //        {
        //            BSM.ListAdvertise.Remove(objCheck.First());
        //        }
        //        else
        //        {
        //            ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
        //        }
        //        Session[Index_Sess_Obj + ActionName] = BSM;
        //    }
        //    return PartialView("GridAdvertise", BSM.ListAdvertise);
        //}
        //public ActionResult UploadControlCallbackActionIdentity(HttpPostedFileBase file_Uploader)
        //{
        //    UserSession();
        //    var path = DB.CFUploadPaths.Find("IMG_UPLOAD");
        //    SYFileImport sfi = new SYFileImport(path);
        //    sfi.ObjectTemplate = new MDUploadTemplate();
        //    sfi.ObjectTemplate.UploadDate = DateTime.Now;
        //    sfi.ObjectTemplate.ScreenId = SCREEN_ID;
        //    sfi.ObjectTemplate.UploadBy = user.UserName;
        //    sfi.ObjectTemplate.Module = "STAFF";
        //    sfi.ObjectTemplate.IsGenerate = false;

        //    UploadedFile[] files = UploadControlExtension.GetUploadedFiles("FileUploadIdentify",
        //        sfi.ValidationSettings,
        //        sfi.uc_FileUploadCompleteFile);
        //    Session[PATH_FILE] = sfi.ObjectTemplate.UpoadPath;
        //    return null;
        //}
        #endregion 
        #region 'Convert Status'
        public ActionResult ProcessingVacancy(string JobID)
        {
            ActionName = "Details";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            if (JobID != null)
            {
                RCMERecruitObject BSM = new RCMERecruitObject();
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (RCMERecruitObject)Session[Index_Sess_Obj + ActionName];
                }

                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.Processing(JobID);
                if (msg == SYConstant.OK)
                {
                    var mess = SYMessages.getMessageObject("VAC_PROCESS", user.Lang);
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
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Details?JobID=" + JobID);

        }
        public ActionResult Post(string JobID)
        {
            ActionName = "Details";
            UserSession();
            UserConfForm(SYActionBehavior.EDIT);
            DataSelector();
            if (JobID != null)
            {
                RCMERecruitObject BSM = new RCMERecruitObject();
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (RCMERecruitObject)Session[Index_Sess_Obj + ActionName];
                }

                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.Posting(JobID);
                if (msg == SYConstant.OK)
                {
                    var mess = SYMessages.getMessageObject("DOC_POSTING", user.Lang);
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
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Details?JobID=" + JobID);

        }
        public ActionResult ClosedVacancy(string JobID)
        {
            ActionName = "Details";
            UserSession();
            UserConfForm(SYActionBehavior.EDIT);
            DataSelector();
            if (JobID != null)
            {
                RCMERecruitObject BSM = new RCMERecruitObject();
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (RCMERecruitObject)Session[Index_Sess_Obj + ActionName];
                }

                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.Closed(JobID);
                if (msg == SYConstant.OK)
                {
                    var mess = SYMessages.getMessageObject("VAC_CLOSED", user.Lang);
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
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Details?JobID=" + JobID);

        }

        #endregion 
        static async Task PostPhotoToFacebook(string accessToken, string imagePath, string caption)
        {
            using (var httpClient = new HttpClient())
            {
                var form = new MultipartFormDataContent();
                using (var stream = new FileStream(imagePath, FileMode.Open))
                {
                    form.Add(new StreamContent(stream), "source", Path.GetFileName(imagePath));
                    form.Add(new StringContent(caption), "caption");
                    form.Add(new StringContent(accessToken), "access_token");

                    var response = await httpClient.PostAsync("https://graph.facebook.com/me/photos", form);

                    if (!response.IsSuccessStatusCode)
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        throw new Exception($"Failed to post photo on Facebook. Error message: {errorMessage}");
                    }
                }
            }
        }
        #region 'private code'
        private void DataSelector()
        {
            string Approved = SYDocumentStatus.APPROVED.ToString();
            var results = DB.RCMRecruitRequests.Where(w => w.Status == Approved).GroupBy(n => new { n.POST, n.Status })
                    .Select(g => new
                    {
                        g.Key.POST,
                        g.Key.Status
                    }).ToList();
            var Position = DB.HRPositions.ToList();

            ViewData["POSITION_SELECT"] = Position.Where(w => results.Where(x => x.POST == w.Code).Any()).ToList();
            ViewData["EMPCODE_SELECT"] = DB.HRStaffProfiles.ToList();
            ViewData["CHANNELRECEIVED_SELECT"] = DB.RCMSAdvertises.ToList().OrderBy(w => w.Company);
            ViewData["ADS_SELECT"] = DB.RCMSAdvertises.ToList();
            ViewData["TOKEN_SELECT"] = DB.SYSocialMedias.ToList();
        }
        #endregion 
    }
}
