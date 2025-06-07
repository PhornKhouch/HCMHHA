using DevExpress.Web;
using DevExpress.Web.Mvc;
using Humica.Core.DB;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic;
using Humica.Logic.RCM;
using Humica.Models.SY;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Humica.Controllers.HRM.RCM
{
    public class RCMVacancyController : Humica.EF.Controllers.MasterSaleController
    {
        private const string SCREEN_ID = "RCM0000002";
        private const string URL_SCREEN = "/HRM/RCM/RCMVacancy/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "Code";
        private string PATH_FILE = "123223434345532";

        SMSystemEntity SMS = new SMSystemEntity();
        HumicaDBContext DB = new HumicaDBContext();
        HumicaDBViewContext DBV = new HumicaDBViewContext();

        public RCMVacancyController()
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

            RCMVacancyObject BSM = new RCMVacancyObject();
            BSM.ListPending = new List<RCMRecruitRequest>();
            BSM.ListVacView = new List<RCM_Vacancy_VIEW>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMVacancyObject)Session[Index_Sess_Obj + ActionName];
            }

            string approved = SYDocumentStatus.APPROVED.ToString();
            var ListHeader = DBV.RCM_Vacancy_VIEW.ToList();
            var _Vac = DB.RCMVacancies.Select(x => x.DocRef).ToList();
            var ListPending = DB.RCMRecruitRequests.Where(w => w.Status == approved && !_Vac.Contains(w.RequestNo)).OrderByDescending(x => x.DocDate).ToList();
            BSM.ListPending = ListPending.ToList();
            BSM.ListVacView = ListHeader.ToList();

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
        public ActionResult PartialList()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            RCMVacancyObject BSM = new RCMVacancyObject();
            BSM.ListVacView = new List<RCM_Vacancy_VIEW>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMVacancyObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView(SYListConfuration.ListDefaultView, BSM.ListVacView);
        }

        #endregion 
        #region 'Create'
        public ActionResult Create(string RequestNo)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();

            RCMVacancyObject BSD = new RCMVacancyObject();

            BSD.Header = new RCMVacancy();
            BSD.ListHeader = new List<RCMVacancy>();
            BSD.ListInt = new List<RCMVInterviewer>();
            BSD.ListAdvertise = new List<RCMAdvertising>();
            BSD.Header.Status = SYDocumentStatus.OPEN.ToString();
            BSD.Header.VacancyType = "Employee";

            if (RequestNo != null)
            {
                var _ReqForm = DB.RCMRecruitRequests.FirstOrDefault(w => w.RequestNo == RequestNo);
                BSD.Header.DocRef = RequestNo;
                BSD.Header.Sect = _ReqForm.Sect;
                BSD.PostOfJD = _ReqForm.POST;
                BSD.JobResponsibility = _ReqForm.JobResponsibility;
                BSD.Header.ClosedDate = _ReqForm.ExpectedDate;
                BSD.JobRequirement = _ReqForm.JobRequirement;
            }

            UserConfListAndForm();
            Session[Index_Sess_Obj + ActionName] = BSD;
            return View(BSD);
        }
        [HttpPost]
        public ActionResult Create(RCMVacancyObject collection)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.ADD);

            var BSM = (RCMVacancyObject)Session[Index_Sess_Obj + ActionName];

            collection.ListInt = BSM.ListInt.ToList();
            collection.ListAdvertise = BSM.ListAdvertise.ToList();
            collection.ScreenId = SCREEN_ID;

            string msg = collection.createVAC();

            if (msg == SYConstant.OK)
            {
                SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                mess.DocumentNumber = BSM.Header.Code;
                mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?Code=" + mess.DocumentNumber;
                Session[Index_Sess_Obj + this.ActionName] = BSM;
                UserConfForm(ActionBehavior.SAVEGRID);
                Session[SYConstant.MESSAGE_SUBMIT] = mess;
                return View(BSM);
            }
            Session[Index_Sess_Obj + this.ActionName] = collection;
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
            return View(collection);
        }
        #endregion 
        #region 'Edit'
        public ActionResult Edit(string Code)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMVacancyObject BSM = new RCMVacancyObject();

            BSM.Header = DB.RCMVacancies.FirstOrDefault(w => w.Code == Code);

            if (BSM.Header != null)
            {
                string process = SYDocumentStatus.PROCESSING.ToString();
                string closed = SYDocumentStatus.CLOSED.ToString();

                if (BSM.Header.Status != process && BSM.Header.Status != closed)
                {
                    BSM.RecruitRequest = DB.RCMRecruitRequests.FirstOrDefault(w => w.RequestNo == BSM.Header.DocRef);
                    if (BSM.RecruitRequest != null)
                    {
                        BSM.PostOfJD = BSM.RecruitRequest.POST;
                        BSM.JobRequirement = BSM.RecruitRequest.JobRequirement;
                        BSM.JobResponsibility = BSM.RecruitRequest.JobResponsibility;
                    }
                    BSM.ListInt = DB.RCMVInterviewers.Where(w => w.Code == Code).ToList();
                    BSM.ListAdvertise = DB.RCMAdvertisings.Where(w => w.VacNo == Code).ToList();
                    Session[Index_Sess_Obj + ActionName] = BSM;
                    return View(BSM);
                }
                else
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("INV_DOC", user.Lang);
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
                }
            }
            Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE");
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        [HttpPost]
        public ActionResult Edit(string Code, RCMVacancyObject collection)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();

            RCMVacancyObject BSM = new RCMVacancyObject();

            BSM = (RCMVacancyObject)Session[Index_Sess_Obj + ActionName];

            collection.ListInt = BSM.ListInt;
            collection.ListAdvertise = BSM.ListAdvertise;
            collection.ScreenId = SCREEN_ID;

            if (ModelState.IsValid)
            {
                string msg = collection.updateVAC(Code);

                if (msg != SYConstant.OK)
                {
                    SYMessages mess_err = SYMessages.getMessageObject(msg, user.Lang);
                    Session[Index_Sess_Obj + this.ActionName] = BSM;
                    UserConfForm(ActionBehavior.SAVEGRID);
                    Session[SYConstant.MESSAGE_SUBMIT] = mess_err;
                    return View(collection);
                }

                SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                mess.DocumentNumber = Code;
                mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?Code=" + mess.DocumentNumber; ;
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
        public ActionResult Details(string Code)
        {
            ActionName = "Details";
            UserSession();
            RCMVacancyObject BSM = new RCMVacancyObject();
            DataSelector();
            ViewData[SYConstant.PARAM_ID] = Code;
            ViewData[ClsConstant.IS_READ_ONLY] = true;

            BSM.Header = DB.RCMVacancies.FirstOrDefault(w => w.Code == Code);

            if (BSM.Header == null)
            {
                Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE");
                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
            }

            BSM.RecruitRequest = DB.RCMRecruitRequests.FirstOrDefault(w => w.RequestNo == BSM.Header.DocRef);
            if (BSM.RecruitRequest != null)
            {
                BSM.PostOfJD = BSM.RecruitRequest.POST;
                BSM.JobResponsibility = BSM.RecruitRequest.JobResponsibility;
                BSM.JobRequirement = BSM.RecruitRequest.JobRequirement;
            }
            BSM.ListInt = DB.RCMVInterviewers.Where(w => w.Code == Code).ToList();
            BSM.ListAdvertise = DB.RCMAdvertisings.Where(w => w.VacNo == Code).ToList();

            UserConfForm(SYActionBehavior.VIEW);
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        #endregion
        #region 'Delete'  
        public ActionResult Delete(string Code)
        {
            UserSession();
            RCMVacancyObject BSM = new RCMVacancyObject();
            BSM.Header = DB.RCMVacancies.FirstOrDefault(w => w.Code == Code);
            if (BSM.Header != null)
            {
                string msg = BSM.deleteVAC(Code);

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
        #region '_Interviewer'
        public ActionResult _Interviewer()
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMVacancyObject BSM = new RCMVacancyObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMVacancyObject)Session[Index_Sess_Obj + ActionName];
            }

            return PartialView("_Interviewer", BSM.ListInt);
        }
        public ActionResult _InterviewerDetails()
        {
            ActionName = "Details";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMVacancyObject BSM = new RCMVacancyObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMVacancyObject)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("_InterviewerDetails", BSM.ListInt);
        }
        public ActionResult CreateInterviewer(RCMVInterviewer ModelObject, RCMVacancyObject Coll)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMVacancyObject BSM = new RCMVacancyObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMVacancyObject)Session[Index_Sess_Obj + ActionName];
            }
            try
            {
                var _list = BSM.ListInt.ToList();
                var objCheck = _list.FirstOrDefault(w => w.IntStep == ModelObject.IntStep && w.EmpCode == ModelObject.EmpCode);
                if (objCheck != null)
                {
                    ViewData["EditError"] = SYMessages.getMessage("RECORD_EXIST");
                }
                else
                {
                    if (BSM.ListInt.Count == 0)
                    {
                        ModelObject.LineItem = 1;
                    }
                    else
                    {
                        ModelObject.LineItem = BSM.ListInt.Max(w => w.LineItem) + 1;
                    }
                    BSM.ListInt.Add(ModelObject);
                    Session[Index_Sess_Obj + ActionName] = BSM;
                }
            }
            catch (Exception e)
            {
                ViewData["EditError"] = e.Message;
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("_Interviewer", BSM.ListInt);
        }
        public ActionResult EditInterviewer(RCMVInterviewer ModelObject)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMVacancyObject BSM = new RCMVacancyObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMVacancyObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListInt.FirstOrDefault(w => w.LineItem == ModelObject.LineItem);

                if (objCheck != null)
                {
                    objCheck.EmpCode = ModelObject.EmpCode;
                    objCheck.EmpName = ModelObject.EmpName;
                    objCheck.IntStep = ModelObject.IntStep;
                    objCheck.Remark = ModelObject.Remark;
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;


            }
            return PartialView("_Interviewer", BSM.ListInt);
        }
        public ActionResult DeleteInterviewer(int LineItem)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMVacancyObject BSM = new RCMVacancyObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMVacancyObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListInt.FirstOrDefault(w => w.LineItem == LineItem);

                if (objCheck != null)
                {
                    BSM.ListInt.Remove(objCheck);
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return PartialView("_Interviewer", BSM.ListInt);
        }
        #endregion 
        #region '_Ads'
        public ActionResult GridAds()
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMVacancyObject BSM = new RCMVacancyObject();
            BSM.ListAdvertise = new List<RCMAdvertising>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMVacancyObject)Session[Index_Sess_Obj + ActionName];
            }

            return PartialView("GridAds", BSM.ListAdvertise);
        }
        public ActionResult _AdsDetails()
        {
            ActionName = "Details";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMVacancyObject BSM = new RCMVacancyObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMVacancyObject)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("_AdsDetails", BSM.ListAdvertise);
        }
        public ActionResult CreateAds(RCMAdvertising ModelObject)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMVacancyObject BSM = new RCMVacancyObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMVacancyObject)Session[Index_Sess_Obj + ActionName];
            }
            try
            {
                if (ModelObject.Advertising == null)
                {
                    ViewData["EditError"] = SYMessages.getMessage("ADS_EN");
                }
                else
                {
                    if (BSM.ListAdvertise.Count == 0)
                    {
                        ModelObject.LineItem = 1;
                    }
                    else
                    {
                        ModelObject.LineItem = BSM.ListAdvertise.Max(w => w.LineItem) + 1;
                    }
                    var _AdsType = DB.RCMSAdvertises.FirstOrDefault(w => w.Code == ModelObject.Advertising);
                    BSM.ListAdvertise.Add(ModelObject);
                    Session[Index_Sess_Obj + ActionName] = BSM;
                }
            }
            catch (Exception e)
            {
                ViewData["EditError"] = e.Message;
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("GridAds", BSM.ListAdvertise);
        }
        public ActionResult EditAds(RCMAdvertising ModelObject)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMVacancyObject BSM = new RCMVacancyObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMVacancyObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListAdvertise.FirstOrDefault(w => w.LineItem == ModelObject.LineItem);
                if (Session[PATH_FILE] != null)
                {
                    ModelObject.Attachment = Session[PATH_FILE].ToString();
                    Session[PATH_FILE] = null;
                }
                else
                {
                    ModelObject.Attachment = objCheck.Attachment;
                }
                if (objCheck != null)
                {
                    objCheck.Description = ModelObject.Description;
                    objCheck.TotalCost = ModelObject.TotalCost;
                    objCheck.TotalBudget = ModelObject.TotalBudget;
                    objCheck.Remark = ModelObject.Remark;
                    objCheck.Attachment = ModelObject.Attachment;
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return PartialView("GridAds", BSM.ListAdvertise);
        }
        public ActionResult DeleteAds(int LineItem)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMVacancyObject BSM = new RCMVacancyObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMVacancyObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListAdvertise.FirstOrDefault(w => w.LineItem == LineItem);

                if (objCheck != null)
                {
                    BSM.ListAdvertise.Remove(objCheck);
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return PartialView("GridAds", BSM.ListAdvertise);
        }
        public ActionResult UploadControlCallbackActionIdentity(HttpPostedFileBase file_Uploader)
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

            UploadedFile[] files = UploadControlExtension.GetUploadedFiles("FileUploadIdentify",
                sfi.ValidationSettings,
                sfi.uc_FileUploadCompleteFile);
            Session[PATH_FILE] = sfi.ObjectTemplate.UpoadPath;
            return null;
        }
        #endregion 
        #region 'Convert Status'
        public ActionResult Processing(string Code)
        {
            ActionName = "Details";
            UserSession();
            UserConfForm(SYActionBehavior.EDIT);
            DataSelector();

            if (Code != null)
            {
                RCMVacancyObject BSM = new RCMVacancyObject();
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (RCMVacancyObject)Session[Index_Sess_Obj + ActionName];
                }

                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.Processing(Code);
                if (msg == SYConstant.OK)
                {
                    var mess = SYMessages.getMessageObject("PROCESS_EN", user.Lang);
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
        public ActionResult Closed(string Code)
        {
            ActionName = "Details";
            UserSession();
            UserConfForm(SYActionBehavior.EDIT);
            DataSelector();

            if (Code != null)
            {
                RCMVacancyObject BSM = new RCMVacancyObject();
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (RCMVacancyObject)Session[Index_Sess_Obj + ActionName];
                }

                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.Closed(Code);
                if (msg == SYConstant.OK)
                {
                    var mess = SYMessages.getMessageObject("CLOSED_EN", user.Lang);
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
        //public ActionResult Completed(string Code)
        //{
        //    ActionName = "Details";
        //    UserSession();
        //    UserConfForm(SYActionBehavior.EDIT);
        //    DataSelector();

        //    if (Code != null)
        //    {
        //        RCMVacancyObject BSM = new RCMVacancyObject();
        //        if (Session[Index_Sess_Obj + ActionName] != null)
        //        {
        //            BSM = (RCMVacancyObject)Session[Index_Sess_Obj + ActionName];
        //        }

        //        BSM.ScreenId = SCREEN_ID;
        //        string msg = BSM.Closed(Code);
        //        if (msg == SYConstant.OK)
        //        {
        //            var mess = SYMessages.getMessageObject("COMPLETED_EN", user.Lang);
        //            mess.Description = mess.Description + ". " + BSM.MessageError;
        //            Session[SYSConstant.MESSAGE_SUBMIT] = mess;
        //        }
        //        else
        //        {
        //            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
        //        }
        //    }
        //    else
        //    {
        //        Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE", user.Lang);
        //    }
        //    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        //}
        #endregion 
        #region 'private code'
        public ActionResult DataInterviewer(string Code, string Action)
        {
            ActionName = Action;
            RCMApplicantObject BSM = new RCMApplicantObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
            }
            var _IntVInfor = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == Code);

            if (_IntVInfor != null)
            {
                var result = new
                {
                    MS = SYConstant.OK,
                    Name = _IntVInfor.AllName,
                    Post = _IntVInfor.JobCode
                };
                return Json(result, JsonRequestBehavior.DenyGet);
            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        private void DataSelector()
        {
            SYDataList objList = new SYDataList("GENER_SELECT");
            ViewData["GENDER_SELECT"] = objList.ListData;

            objList = new SYDataList("RECRUITTYPE_SELECT");
            ViewData["RECRUITTYPE_SELECT"] = objList.ListData;


            objList = new SYDataList("VACANCYTYPE");
            ViewData["VACANCY_SELECT"] = objList.ListData;

            ViewData["COUNTRY_SELECT"] = DB.HRCountries.ToList().OrderBy(w => w.Description);
            ViewData["NATION_SELECT"] = DB.HRNations.ToList().OrderBy(w => w.Description);
            ViewData["LEVEL_SELECT"] = SMS.HRLevels.ToList().OrderBy(w => w.Description);
            ViewData["POSITION_SELECT"] = ClsFilter.LoadPosition();
            ViewData["WORKINGTYPE_SELECT"] = DB.HRWorkingTypes.ToList().OrderBy(w => w.Description);
            ViewData["DEPARTMENT_SELECT"] = ClsFilter.LoadDepartment();
            ViewData["BRANCHES_SELECT"] = SMS.HRBranches.ToList().OrderBy(w => w.Description);
            objList = new SYDataList("KINDSEARCH_SELECT");
            ViewData["Kind_Search_SELECT"] = objList.ListData;
            objList = new SYDataList("TERM_RC");
            ViewData["TERM_SELECT"] = objList.ListData;
            //objList = new SYDataList("TYPE_RC");
            //ViewData["RECRUITTYPE_SELECT"] = objList.ListData;
            ViewData["EMPCODE_SELECT"] = DBV.HR_STAFF_VIEW.ToList();
            ViewData["JD_SELECT"] = DB.RCMSJobDescs.ToList();
            ViewData["ADS_SELECT"] = DB.RCMSAdvertises.ToList();
            objList = new SYDataList("STAFF_TYPE");
            ViewData["STAFFTYPE_SELECT"] = objList.ListData;
        }
        #endregion 
    }
}
