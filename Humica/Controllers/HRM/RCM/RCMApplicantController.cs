using DevExpress.Web;
using DevExpress.Web.Mvc;
using Humica.Core.DB;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic;
using Humica.Logic.CF;
using Humica.Logic.RCM;
using Humica.Models.SY;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Humica.Controllers.HRM.RCM
{
    public class RCMApplicantController : Humica.EF.Controllers.MasterSaleController
    {
        private const string SCREEN_ID = "RCM0000003";
        private const string URL_SCREEN = "/HRM/RCM/RCMApplicant/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "ApplicantID";
        private string PATH_FILE = "12313123123sadfsdfsdfsdf";
        private string PATH_IDENT = "identity";

        SMSystemEntity SMS = new SMSystemEntity();
        HumicaDBContext DB = new HumicaDBContext();
        public RCMApplicantController()
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
            RCMApplicantObject BSM = new RCMApplicantObject();
            BSM.ListHeader = new List<RCMApplicant>();
            BSM.Filters = new Core.FT.Filters();
            BSM.Filtering = new Filtering();
            BSM.Filters.Status = "All";
            BSM.ListHeader = DB.RCMApplicants.ToList();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                var obj = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        [HttpPost]
        public async Task<ActionResult> Index(RCMApplicantObject collection)
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            DataSelector();
            RCMApplicantObject BSM = new RCMApplicantObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
            }

            BSM.Filters = collection.Filters;
            DateTime fromDate = BSM.Filters.FromDate.HasValue ? BSM.Filters.FromDate.Value : Core.DateTimeHelper.MinValue;
            DateTime toDate = BSM.Filters.ToDate.HasValue ? BSM.Filters.ToDate.Value : Core.DateTimeHelper.MaxValue;
            var _listApplicant = await DB.RCMApplicants.Where(w => EntityFunctions.TruncateTime(w.ApplyDate) >= fromDate.Date &&
                EntityFunctions.TruncateTime(w.ApplyDate) <= toDate.Date).ToListAsync();
            if (BSM.Filters.Status?.ToUpper() == "ALL")
            {
                BSM.ListHeader = _listApplicant.ToList();
            }
            else
            {
                BSM.ListHeader = _listApplicant.Where(w => w.ShortList == BSM.Filters.Status).ToList();
            }
            collection.ListHeader = BSM.ListHeader;
            Session[Index_Sess_Obj + ActionName] = collection;
            return View(collection);
        }
        //[HttpPost]
        //public ActionResult Index(RCMApplicantObject BSM)
        //{
        //    ActionName = "Index";
        //    DataSelector();
        //    UserSession();
        //    UserConfListAndForm(this.KeyName);

        //    var LstApp = DB.RCMApplicants.ToList();
        //    BSM.ListHeader = LstApp.Where(w => w.VacNo == BSM.Filtering.Vacancy).ToList();

        //    Session[Index_Sess_Obj + ActionName] = BSM;
        //    return View(BSM);
        //}
        public ActionResult GridItem()
        {
            ActionName = "Index";
            UserSession();
            DataSelector();
            UserConfList(KeyName);
            RCMApplicantObject BSM = new RCMApplicantObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("GridItem", BSM.ListHeader);
        }
        public ActionResult PartialList()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            RCMApplicantObject BSM = new RCMApplicantObject();
            //BSM.ListRecruitRequest = new List<RCM_RecruitRequest_VIEW>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
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

            RCMApplicantObject BSD = new RCMApplicantObject();

            BSD.Header = new RCMApplicant();
            BSD.ListHeader = new List<RCMApplicant>();
            BSD.ListDependent = new List<RCMADependent>();
            BSD.ListEdu = new List<RCMAEdu>();
            BSD.ListLang = new List<RCMALanguage>();
            BSD.ListTraining = new List<RCMATraining>();
            BSD.ListWorkHistory = new List<RCMAWorkHistory>();
            BSD.ListRef = new List<RCMAReference>();
            BSD.ListIdentity = new List<RCMAIdentity>();
            BSD.Header.ApplyDate = DateTime.Now;
            BSD.Header.DOB = DateTime.Now;
            BSD.Header.ExpectSalary = 0;

            UserConfListAndForm();
            Session[Index_Sess_Obj + ActionName] = BSD;
            return View(BSD);
        }
        [HttpPost]
        public ActionResult Create(RCMApplicantObject collection)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.ADD);

            var BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];

            BSM.Header = collection.Header;
            collection = BSM;

            if (Session[PATH_FILE] != null)
            {
                collection.Header.ResumeFile = Session[PATH_FILE].ToString();
                Session[PATH_FILE] = null;
            }
            collection.ScreenId = SCREEN_ID;
            string msg = collection.createApplicant();

            if (msg == SYConstant.OK)
            {
                SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                mess.DocumentNumber = collection.Header.ApplicantID;
                mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?ApplicantID=" + mess.DocumentNumber;
                Session[SYConstant.MESSAGE_SUBMIT] = mess;

                BSM = NewRCMApplicant();

                Session[Index_Sess_Obj + this.ActionName] = BSM;
                UserConfForm(ActionBehavior.SAVEGRID);
                Session[SYConstant.MESSAGE_SUBMIT] = mess;

                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Create");
            }
            else
            {
                Session[Index_Sess_Obj + this.ActionName] = collection;
                ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                return View(collection);
            }
        }
        #endregion 
        #region 'Edit'
        public ActionResult Edit(string ApplicantID)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            RCMApplicantObject BSM = new RCMApplicantObject();
            UserConfListAndForm();

            BSM.Header = DB.RCMApplicants.FirstOrDefault(w => w.ApplicantID == ApplicantID);
            if (BSM.Header != null)
            {
                BSM.ListDependent = DB.RCMADependents.Where(w => w.ApplicantID == ApplicantID).ToList();
                BSM.ListEdu = DB.RCMAEdus.Where(w => w.ApplicantID == BSM.Header.ApplicantID).ToList();
                BSM.ListLang = DB.RCMALanguages.Where(w => w.ApplicantID == BSM.Header.ApplicantID).ToList();
                BSM.ListTraining = DB.RCMATrainings.Where(w => w.ApplicantID == BSM.Header.ApplicantID).ToList();
                BSM.ListWorkHistory = DB.RCMAWorkHistories.Where(w => w.ApplicantID == BSM.Header.ApplicantID).ToList();
                BSM.ListRef = DB.RCMAReferences.Where(w => w.ApplicantID == BSM.Header.ApplicantID).ToList();
                BSM.ListIdentity = DB.RCMAIdentities.Where(w => w.ApplicantID == BSM.Header.ApplicantID).ToList();
                Session[Index_Sess_Obj + ActionName] = BSM;
                return View(BSM);
            }
            Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE");
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        [HttpPost]
        public ActionResult Edit(string ApplicantID, RCMApplicantObject collection)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();

            RCMApplicantObject BSM = new RCMApplicantObject();

            BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];

            BSM.Header = collection.Header;
            collection = BSM;

            if (Session[PATH_FILE] != null)
            {
                collection.Header.ResumeFile = Session[PATH_FILE].ToString();
                Session[PATH_FILE] = null;
            }
            else
            {
                collection.Header.ResumeFile = BSM.Header.ResumeFile;
            }

            collection.ScreenId = SCREEN_ID;
            string msg = collection.updateApplicant(ApplicantID);

            if (msg == SYConstant.OK)
            {
                SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                mess.DocumentNumber = ApplicantID;
                mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?ApplicantID=" + mess.DocumentNumber; ;
                Session[Index_Sess_Obj + this.ActionName] = collection;
                UserConfForm(ActionBehavior.SAVEGRID);
                Session[SYConstant.MESSAGE_SUBMIT] = mess;
                return View(collection);
            }
            else
            {
                SYMessages mess_err = SYMessages.getMessageObject(msg, user.Lang);
                Session[Index_Sess_Obj + this.ActionName] = BSM;
                UserConfForm(ActionBehavior.SAVEGRID);
                Session[SYConstant.MESSAGE_SUBMIT] = mess_err;
                return View(collection);
            }
        }
        #endregion 
        #region 'Details'
        public ActionResult Details(string ApplicantID)
        {
            ActionName = "Details";
            UserSession();
            RCMApplicantObject BSM = new RCMApplicantObject();
            DataSelector();
            ViewData[SYConstant.PARAM_ID] = ApplicantID;
            ViewData[ClsConstant.IS_READ_ONLY] = true;
            BSM.Header = DB.RCMApplicants.FirstOrDefault(w => w.ApplicantID == ApplicantID);
            if (BSM.Header == null)
            {
                Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE");
                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
            }
            BSM.ListDependent = DB.RCMADependents.Where(w => w.ApplicantID == ApplicantID).ToList();
            BSM.ListEdu = DB.RCMAEdus.Where(w => w.ApplicantID == ApplicantID).ToList();
            BSM.ListLang = DB.RCMALanguages.Where(w => w.ApplicantID == ApplicantID).ToList();
            BSM.ListTraining = DB.RCMATrainings.Where(w => w.ApplicantID == ApplicantID).ToList();
            BSM.ListWorkHistory = DB.RCMAWorkHistories.Where(w => w.ApplicantID == ApplicantID).ToList();
            BSM.ListRef = DB.RCMAReferences.Where(w => w.ApplicantID == ApplicantID).ToList();
            UserConfForm(SYActionBehavior.VIEW);
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        #endregion
        #region 'Delete'  
        public ActionResult Delete(string ApplicantID)
        {
            UserSession();
            RCMApplicantObject BSM = new RCMApplicantObject();
            BSM.Header = DB.RCMApplicants.FirstOrDefault(w => w.ApplicantID == ApplicantID);
            if (BSM.Header != null)
            {
                string msg = BSM.deleteApplicant(ApplicantID);

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
        #region 'Edu'
        public ActionResult _Edu()
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMApplicantObject BSM = new RCMApplicantObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
            }

            return PartialView("_Edu", BSM.ListEdu);
        }
        public ActionResult _EduDetails()
        {
            ActionName = "Details";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMApplicantObject BSM = new RCMApplicantObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("_EduDetails", BSM);
        }
        public ActionResult CreateEdu(RCMAEdu ModelObject)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMApplicantObject BSM = new RCMApplicantObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
            }
            if (ModelState.IsValid)
            {
                try
                {
                    if (BSM.ListEdu.Count == 0)
                    {
                        ModelObject.LineItem = 1;
                    }
                    else if (ModelObject.EduType == null)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("EE_EDUTYPE", user.Lang);
                    }
                    else if (ModelObject.EduCenter == null)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("EE_EDUC", user.Lang);
                    }
                    else if (ModelObject.Major == null)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("EE_MAJOR", user.Lang);
                    }
                    else
                    {
                        ModelObject.LineItem = BSM.ListEdu.Max(w => w.LineItem) + 1;
                    }

                    BSM.ListEdu.Add(ModelObject);
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
            return PartialView("_Edu", BSM.ListEdu);
        }
        public ActionResult EditEdu(RCMAEdu ModelObject)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMApplicantObject BSM = new RCMApplicantObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListEdu.FirstOrDefault(w => w.LineItem == ModelObject.LineItem);

                if (objCheck != null)
                {
                    objCheck.EduType = ModelObject.EduType;
                    objCheck.EduCenter = ModelObject.EduCenter;
                    objCheck.Major = ModelObject.Major;
                    objCheck.Result = ModelObject.Result;
                    objCheck.StartDate = ModelObject.StartDate;
                    objCheck.EndDate = ModelObject.EndDate;
                    objCheck.Remark = ModelObject.Remark;
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;

            }
            return PartialView("_Edu", BSM.ListEdu);
        }
        public ActionResult DeleteEdu(int LineItem)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMApplicantObject BSM = new RCMApplicantObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListEdu.FirstOrDefault(w => w.LineItem == LineItem);

                if (objCheck != null)
                {
                    BSM.ListEdu.Remove(objCheck);
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return PartialView("_Edu", BSM.ListEdu);
        }
        #endregion
        #region 'Lang'
        public ActionResult _Lang()
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMApplicantObject BSM = new RCMApplicantObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
            }

            return PartialView("_Lang", BSM.ListLang);
        }
        public ActionResult _LangDetails()
        {
            ActionName = "Details";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMApplicantObject BSM = new RCMApplicantObject();


            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
            }

            return PartialView("_LangDetails", BSM);
        }
        public ActionResult CreateLang(RCMALanguage ModelObject)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMApplicantObject BSM = new RCMApplicantObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var objCheck = BSM.ListLang.Where(w => w.Lang == ModelObject.Lang).ToList();
                    if (objCheck.Count > 0)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("DUPL_KEY", user.Lang);
                    }
                    else if (ModelObject.Lang == null)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("EE_LANG", user.Lang);
                    }
                    else
                    {
                        BSM.ListLang.Add(ModelObject);
                    }
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
            return PartialView("_Lang", BSM.ListLang);
        }
        public ActionResult EditLang(RCMALanguage ModelObject)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMApplicantObject BSM = new RCMApplicantObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListLang.FirstOrDefault(w => w.Lang == ModelObject.Lang);
                if (objCheck != null)
                {
                    objCheck.Speaking = ModelObject.Speaking;
                    objCheck.Reading = ModelObject.Reading;
                    objCheck.Listening = ModelObject.Listening;
                    objCheck.Writing = ModelObject.Writing;
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return PartialView("_Lang", BSM.ListLang);
        }
        public ActionResult DeleteLang(string Lang)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMApplicantObject BSM = new RCMApplicantObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListLang.FirstOrDefault(w => w.Lang == Lang);

                if (objCheck != null)
                {
                    BSM.ListLang.Remove(objCheck);
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return PartialView("_Lang", BSM.ListLang);
        }
        #endregion 
        #region 'Training'
        public ActionResult _Training()
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMApplicantObject BSM = new RCMApplicantObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
            }

            return PartialView("_Training", BSM.ListTraining);
        }
        public ActionResult _TrainingDetails()
        {
            ActionName = "Details";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMApplicantObject BSM = new RCMApplicantObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
            }

            return PartialView("_TrainingDetails", BSM);
        }
        public ActionResult CreateTraining(RCMATraining ModelObject)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMApplicantObject BSM = new RCMApplicantObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (BSM.ListTraining.Count == 0)
                    {
                        ModelObject.LineItem = 1;
                    }
                    else
                    {
                        ModelObject.LineItem = BSM.ListTraining.Max(w => w.LineItem) + 1;
                    }
                    if (ModelObject.TrainingTopic == null)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("EE_TRAINTOPIC", user.Lang);
                    }
                    else
                    {
                        BSM.ListTraining.Add(ModelObject);
                    }
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
            return PartialView("_Training", BSM.ListTraining);
        }
        public ActionResult EditTraining(RCMATraining ModelObject)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMApplicantObject BSM = new RCMApplicantObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListTraining.FirstOrDefault(w => w.LineItem == ModelObject.LineItem);
                if (objCheck != null)
                {
                    objCheck.TrainingTopic = ModelObject.TrainingTopic;
                    objCheck.TrainingProvider = ModelObject.TrainingProvider;
                    objCheck.TrainingPlace = ModelObject.TrainingPlace;
                    objCheck.FromDate = ModelObject.FromDate;
                    objCheck.ToDate = ModelObject.ToDate;
                    objCheck.Remark = ModelObject.Remark;
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return PartialView("_Training", BSM.ListTraining);
        }
        public ActionResult DeleteTraining(int LineItem)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMApplicantObject BSM = new RCMApplicantObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListTraining.FirstOrDefault(w => w.LineItem == LineItem);

                if (objCheck != null)
                {
                    BSM.ListTraining.Remove(objCheck);
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return PartialView("_Training", BSM.ListTraining);
        }
        #endregion
        #region 'Experience'
        public ActionResult _Experience()
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMApplicantObject BSM = new RCMApplicantObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
            }

            return PartialView("_Experience", BSM.ListWorkHistory);
        }
        public ActionResult _ExperienceDetails()
        {
            ActionName = "Details";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMApplicantObject BSM = new RCMApplicantObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("_ExperienceDetails", BSM);
        }
        public ActionResult CreateExperience(RCMAWorkHistory ModelObject)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMApplicantObject BSM = new RCMApplicantObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
            }
            if (ModelState.IsValid)
            {
                try
                {
                    if (BSM.ListWorkHistory.Count == 0)
                    {
                        ModelObject.LineItem = 1;
                    }
                    else
                    {
                        ModelObject.LineItem = BSM.ListWorkHistory.Max(w => w.LineItem) + 1;
                    }

                    BSM.ListWorkHistory.Add(ModelObject);
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
            return PartialView("_Experience", BSM.ListWorkHistory);
        }
        public ActionResult EditExperience(RCMAWorkHistory ModelObject)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMApplicantObject BSM = new RCMApplicantObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListWorkHistory.FirstOrDefault(w => w.LineItem == ModelObject.LineItem);

                if (objCheck != null)
                {
                    objCheck.Company = ModelObject.Company;
                    objCheck.Position = ModelObject.Position;
                    objCheck.FromDate = ModelObject.FromDate;
                    objCheck.ToDate = ModelObject.ToDate;
                    objCheck.SupervisorName = ModelObject.SupervisorName;
                    objCheck.SupervisorPhone = ModelObject.SupervisorPhone;
                    objCheck.LeaveReason = ModelObject.LeaveReason;
                    objCheck.Duties = ModelObject.Duties;
                    objCheck.StartSalary = ModelObject.StartSalary;
                    objCheck.EndSalary = ModelObject.EndSalary;
                    objCheck.Remark = ModelObject.Remark;
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;

            }
            return PartialView("_Experience", BSM.ListWorkHistory);
        }
        public ActionResult DeleteExperience(int LineItem)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMApplicantObject BSM = new RCMApplicantObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListWorkHistory.FirstOrDefault(w => w.LineItem == LineItem);

                if (objCheck != null)
                {
                    BSM.ListWorkHistory.Remove(objCheck);
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return PartialView("_Experience", BSM.ListWorkHistory);
        }
        #endregion
        #region 'Reference'
        public ActionResult _Reference()
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMApplicantObject BSM = new RCMApplicantObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
            }

            return PartialView("_Reference", BSM.ListRef);
        }
        public ActionResult _ReferenceDetails()
        {
            ActionName = "Details";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMApplicantObject BSM = new RCMApplicantObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("_ReferenceDetails", BSM);
        }
        public ActionResult CreateRef(RCMAReference ModelObject)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMApplicantObject BSM = new RCMApplicantObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
            }
            if (ModelState.IsValid)
            {
                try
                {
                    if (BSM.ListRef.Count == 0)
                    {
                        ModelObject.LineItem = 1;
                    }
                    else if (ModelObject.RefName == null)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("EE_REFNAME", user.Lang);
                    }
                    else if (ModelObject.Phone1 == null)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("EE_REFPHONE", user.Lang);
                    }
                    else
                    {
                        ModelObject.LineItem = BSM.ListRef.Max(w => w.LineItem) + 1;
                    }

                    BSM.ListRef.Add(ModelObject);
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
            return PartialView("_Reference", BSM.ListRef);
        }
        public ActionResult EditRef(RCMAReference ModelObject)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMApplicantObject BSM = new RCMApplicantObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListRef.FirstOrDefault(w => w.LineItem == ModelObject.LineItem);

                if (objCheck != null)
                {
                    objCheck.RefName = ModelObject.RefName;
                    objCheck.Company = ModelObject.Company;
                    objCheck.Occupation = ModelObject.Occupation;
                    objCheck.Phone1 = ModelObject.Phone1;
                    objCheck.Phone2 = ModelObject.Phone2;
                    objCheck.Address = ModelObject.Address;
                    objCheck.Email = ModelObject.Email;
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return PartialView("_Reference", BSM.ListRef);
        }
        public ActionResult DeleteRef(int LineItem)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMApplicantObject BSM = new RCMApplicantObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListRef.FirstOrDefault(w => w.LineItem == LineItem);

                if (objCheck != null)
                {
                    BSM.ListRef.Remove(objCheck);
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return PartialView("_Reference", BSM.ListRef);
        }
        #endregion
        #region 'PersonalDoc'
        public ActionResult _Identity()
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMApplicantObject BSM = new RCMApplicantObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("_Identity", BSM.ListIdentity);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult CreateIden(RCMAIdentity ModelObject)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            RCMApplicantObject BSM = new RCMApplicantObject();
            try
            {
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
                    if (ModelObject.IdentityType == null)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("EE_DOCTYPE", user.Lang);
                    }
                    var List = BSM.ListIdentity.Where(w => w.IdentityType == ModelObject.IdentityType);
                    if (List.Count() > 0)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("RECORD_EXIST", user.Lang);
                    }
                    else if (Session[PATH_IDENT] != null)
                    {
                        ModelObject.Attachment = Session[PATH_IDENT].ToString();
                        Session[PATH_IDENT] = null;
                    }
                    else
                    {
                        BSM.ListIdentity.Add(ModelObject);
                        Session[Index_Sess_Obj + ActionName] = BSM;
                    }
                }
            }
            catch (Exception e)
            {
                ViewData["EditError"] = e.Message;
            }
            DataSelector();
            return PartialView("_Identity", BSM.ListIdentity);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult EditIden(RCMAIdentity ObjType)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            RCMApplicantObject BSM = new RCMApplicantObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
                var listCheck = BSM.ListIdentity.Where(w => w.IdentityType == ObjType.IdentityType).ToList();
                if (Session[PATH_IDENT] != null)
                {
                    ObjType.Attachment = Session[PATH_IDENT].ToString();
                    Session[PATH_IDENT] = null;
                }
                else
                {
                    ObjType.Attachment = listCheck.FirstOrDefault().Attachment;
                }
                if (listCheck.ToList().Count > 0)
                {
                    var objUpdate = listCheck.First();
                    objUpdate.IDCardNo = ObjType.IDCardNo;
                    objUpdate.IssueDate = ObjType.IssueDate;
                    objUpdate.ExpiryDate = ObjType.ExpiryDate;
                    objUpdate.Attachment = ObjType.Attachment;
                    Session[Index_Sess_Obj + ActionName] = BSM;
                }
            }
            DataSelector();
            return PartialView("_Identity", BSM.ListIdentity);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteIden(string IdentityType)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            RCMApplicantObject BSM = new RCMApplicantObject();
            try
            {
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
                }
                var error = 0;

                var checkList = BSM.ListIdentity.Where(w => w.IdentityType == IdentityType).ToList();
                if (checkList.Count == 0)
                {
                    ViewData["EditError"] = SYMessages.getMessage("NO_ITEM_DELETE");
                    error = 1;
                }

                if (error == 0)
                {
                    BSM.ListIdentity.Remove(checkList.First());
                    Session[Index_Sess_Obj + ActionName] = BSM;
                }
            }
            catch (Exception e)
            {
                ViewData["EditError"] = e.Message;
            }
            ViewData["EditError"] = SYMessages.getMessage("EE001", user.Lang);
            DataSelector();

            return PartialView("_Identity", BSM.ListIdentity);
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
            Session[PATH_IDENT] = sfi.ObjectTemplate.UpoadPath; ;
            return null;
        }
        #endregion
        #region 'Upload'
        [HttpPost]
        public ActionResult UploadControlCallbackActionImage()
        {
            UserSession();

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
        #region Import
        public ActionResult Import()
        {

            UserSession();
            ActionName = "Import";
            UserConfListAndForm(this.KeyName);
            var obj = new RCMApplicantObject();
            UserConfListAndForm(ActionBehavior.IMPORT, "UploadName", "RCMApplicant", SYSConstant.DEFAULT_UPLOAD_LIST);
            obj.ListTemplate = DB.MDUploadTemplates.Where(w => w.ScreenId == SCREEN_ID).OrderByDescending(w => w.UploadDate).ToList();
            Session[Index_Sess_Obj + ActionName] = obj;
            return View(obj);

        }

        [HttpPost]
        public ActionResult UploadControlCallbackAction(HttpPostedFileBase file_Uploader)
        {
            UserSession();
            ActionName = "Import";
            this.ScreendIDControl = SYSConstant.DEFAULT_UPLOAD_LIST;
            UserConfListAndForm(ActionBehavior.IMPORT, "UploadName", "RCMApplicant", SYSConstant.DEFAULT_UPLOAD_LIST);
            SYFileImport sfi = new SYFileImport(DB.CFUploadPaths.Find("FILE_UPLOAD"));
            sfi.ObjectTemplate = new MDUploadTemplate();
            sfi.ObjectTemplate.UploadDate = DateTime.Now;
            sfi.ObjectTemplate.ScreenId = SCREEN_ID;
            sfi.ObjectTemplate.UploadBy = user.UserName;
            sfi.ObjectTemplate.Module = "Recruitment";
            sfi.ObjectTemplate.IsGenerate = false;

            UploadedFile[] files = UploadControlExtension.GetUploadedFiles("FileUploadOPB",
              sfi.ValidationSettings,
              sfi.uc_FileUploadComplete);
            return Redirect(SYUrl.getBaseUrl() + ScreenUrl + "Import");
        }

        public ActionResult UploadList()
        {
            UserSession();
            ActionName = "Import";
            UserConfListAndForm(ActionBehavior.IMPORT, "UploadName", "RCMApplicant", SYSConstant.DEFAULT_UPLOAD_LIST);
            IEnumerable<MDUploadTemplate> listu = DB.MDUploadTemplates.Where(w => w.ScreenId == SCREEN_ID).OrderByDescending(w => w.UploadDate);
            return PartialView(SYListConfuration.ListDefaultUpload, listu.ToList());
        }

        [HttpGet]
        public ActionResult GenerateUpload(int id)
        {
            UserSession();
            ActionName = "Import";
            MDUploadTemplate obj = DB.MDUploadTemplates.Find(id);
            RCMApplicantObject BSM = new RCMApplicantObject();
            if (obj != null)
            {
                var DBB = new HumicaDBContext();
                SYExcel excel = new SYExcel();
                excel.FileName = obj.UpoadPath;
                DataTable dtHeader = excel.GenerateExcelData();
                var objStaff = new RCMApplicantObject();
                if (obj.IsGenerate == true)
                {
                    SYMessages mess = SYMessages.getMessageObject("FILE_RG", user.Lang);
                    Session[SYSConstant.MESSAGE_SUBMIT] = mess;
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Import");
                }
                if (dtHeader != null)
                {
                    try
                    {
                        string msg = SYConstant.OK;

                        DateTime create = DateTime.Now;
                        if (dtHeader.Rows.Count > 0)
                        {
                            objStaff.ListHeader = new List<RCMApplicant>();
                            for (int i = 0; i < dtHeader.Rows.Count; i++)
                            {
                                var objCF = DB.ExCfWorkFlowItems.Where(w => w.ScreenID == SCREEN_ID).ToList();
                                var objNumber = new CFNumberRank(objCF.First().NumberRank, SCREEN_ID);

                                var objHeader = new RCMApplicant();
                                objHeader.ApplicantID = objNumber.NextNumberRank;
                                objHeader.VacNo = dtHeader.Rows[i][1].ToString();
                                objHeader.FirstName = dtHeader.Rows[i][2].ToString();
                                objHeader.LastName = dtHeader.Rows[i][3].ToString();
                                objHeader.OthFirstName = dtHeader.Rows[i][4].ToString();
                                objHeader.OthLastName = dtHeader.Rows[i][5].ToString();
                                objHeader.Gender = dtHeader.Rows[i][6].ToString();
                                objHeader.Title = dtHeader.Rows[i][7].ToString();
                                objHeader.Marital = dtHeader.Rows[i][8].ToString();
                                objHeader.DOB = SYSettings.getDateValue(dtHeader.Rows[i][9].ToString());
                                objHeader.ApplyBranch = dtHeader.Rows[i][10].ToString();
                                objHeader.ApplyPosition = dtHeader.Rows[i][11].ToString();
                                objHeader.WorkingType = dtHeader.Rows[i][12].ToString();
                                objHeader.ApplyDate = SYSettings.getDateValue(dtHeader.Rows[i][15].ToString());
                                objHeader.ExpectSalary = SYSettings.getNumberValue(dtHeader.Rows[i][16].ToString());
                                objHeader.Phone1 = dtHeader.Rows[i][17].ToString();
                                objHeader.Email = dtHeader.Rows[i][18].ToString();
                                objHeader.CurAddr = dtHeader.Rows[i][19].ToString();
                                objHeader.ResumeFile = dtHeader.Rows[i][20].ToString();
                                objStaff.ListHeader.Add(objHeader);
                            }

                            msg = objStaff.upload();
                            if (msg == SYConstant.OK)
                            {

                                obj.Message = SYConstant.OK;
                                //obj.DocumentNo = DocBatch.NextNumberRank;
                                obj.IsGenerate = true;
                                DBB.MDUploadTemplates.Attach(obj);
                                DBB.Entry(obj).Property(w => w.Message).IsModified = true;
                                DBB.Entry(obj).Property(w => w.DocumentNo).IsModified = true;
                                DBB.Entry(obj).Property(w => w.IsGenerate).IsModified = true;
                                DBB.SaveChanges();

                            }
                            else
                            {
                                obj.Message = SYMessages.getMessage(msg);
                                obj.Message += ":" + objStaff.MessageCode;
                                obj.IsGenerate = false;
                                DB.MDUploadTemplates.Attach(obj);
                                DB.Entry(obj).Property(w => w.Message).IsModified = true;
                                DB.Entry(obj).Property(w => w.IsGenerate).IsModified = true;
                                DB.SaveChanges();
                                Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessage(msg, user.Lang) + "(" + objStaff.MessageCode + ")";
                                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "/Import");
                            }


                        }
                    }
                    catch (DbUpdateException e)
                    {
                        /*------------------SaveLog----------------------------------*/
                        SYEventLog log = new SYEventLog();
                        log.ScreenId = SCREEN_ID;
                        log.UserId = user.UserID.ToString();
                        log.DocurmentAction = "UPLOAD";
                        log.Action = SYActionBehavior.ADD.ToString();

                        SYEventLogObject.saveEventLog(log, e, true);
                        /*----------------------------------------------------------*/
                        obj.Message = e.Message;
                        obj.IsGenerate = false;
                        DB.MDUploadTemplates.Attach(obj);
                        DB.Entry(obj).Property(w => w.Message).IsModified = true;
                        DB.Entry(obj).Property(w => w.IsGenerate).IsModified = true;
                        DB.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        /*------------------SaveLog----------------------------------*/
                        SYEventLog log = new SYEventLog();
                        log.ScreenId = SCREEN_ID;
                        log.UserId = user.UserID.ToString();
                        log.DocurmentAction = "UPLOAD";
                        log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                        SYEventLogObject.saveEventLog(log, e, true);
                        obj.Message = e.Message;
                        obj.IsGenerate = false;
                        DB.MDUploadTemplates.Attach(obj);
                        DB.Entry(obj).Property(w => w.Message).IsModified = true;
                        DB.Entry(obj).Property(w => w.IsGenerate).IsModified = true;
                        DB.SaveChanges();
                    }
                }

            }

            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Import");
        }

        public ActionResult DownloadTemplate()
        {
            string fileName = Server.MapPath("~/Content/TEMPLATE/TEMPLATE_APPLICANT.xlsx");
            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;filename=TEMPLATE_APPLICANT.xlsx");
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.WriteFile(fileName);
            Response.End();
            return null;
        }

        #endregion
        #region 'private code'
        public ActionResult ShowData(string Code, string Action)
        {
            ActionName = Action;
            RCMApplicantObject BSM = new RCMApplicantObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMApplicantObject)Session[Index_Sess_Obj + ActionName];
            }
            var _vac = DB.RCMVacancies.FirstOrDefault(w => w.Code == Code);

            if (_vac != null)
            {
                var result = new
                {
                    MS = SYConstant.OK,
                    Branch = _vac.Branch,
                    Post = _vac.Position,
                    Dept = _vac.Dept,
                    WorkType = _vac.WorkingType
                };
                return Json(result, JsonRequestBehavior.DenyGet);
            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        private RCMApplicantObject NewRCMApplicant()
        {
            ActionName = "Create";
            UserSession();
            DataSelector();

            RCMApplicantObject BSD = new RCMApplicantObject();
            BSD.Header = new RCMApplicant();
            BSD.ListHeader = new List<RCMApplicant>();
            BSD.ListDependent = new List<RCMADependent>();
            BSD.ListEdu = new List<RCMAEdu>();
            BSD.ListLang = new List<RCMALanguage>();
            BSD.ListTraining = new List<RCMATraining>();
            BSD.ListWorkHistory = new List<RCMAWorkHistory>();
            BSD.ListRef = new List<RCMAReference>();
            BSD.ListIdentity = new List<RCMAIdentity>();
            BSD.Header.ApplyDate = DateTime.Now;
            BSD.Header.DOB = DateTime.Now;
            BSD.Header.ExpectSalary = 0;

            UserConfListAndForm();
            Session[Index_Sess_Obj + ActionName] = BSD;
            return BSD;
        }
        private void DataSelector()
        {
            var objLists = new SYDataList("SHORTLIST_STATUS");
            ViewData["SHORTLIST_STATUS"] = objLists.ListData;
            var IdentityTye = new SYDataList("IdentityTye");
            ViewData["IdentityTye_SELECT"] = IdentityTye.ListData;
            SYDataList objList = new SYDataList("SEX");
            ViewData["GENDER_SELECT"] = objList.ListData;
            objList = new SYDataList("INITIAL");
            ViewData["INITIAL_SELECT"] = objList.ListData;
            objList = new SYDataList("MARITAL");
            ViewData["MARITAL_SELECT"] = objList.ListData;
            objList = new SYDataList("LANG_SKILLS");
            ViewData["LANGSKILLS_SELECT"] = objList.ListData;
            ViewData["BRANCHES_SELECT"] = SMS.HRBranches.ToList();
            ViewData["COUNTRY_SELECT"] = DB.HRCountries.ToList().OrderBy(w => w.Description);
            ViewData["NATION_SELECT"] = DB.HRNations.ToList().OrderBy(w => w.Description);
            ViewData["RelationshipType_LIST"] = DB.HRRelationshipTypes.ToList();
            ViewData["HREmpEduType_LIST"] = DB.HREduTypes.ToList();
            var Processing = SYDocumentStatus.PROCESSING.ToString();
            ViewData["VACANCY_SELECT"] = DB.RCMVacancies.Where(m => m.Status == Processing).ToList();
            ViewData["LANG_SELECT"] = DB.RCMSLangs.ToList();
            ViewData["DEPARTMENT_SELECT"] = ClsFilter.LoadDepartment();
            ViewData["POSITION_SELECT"] = ClsFilter.LoadPosition();
            ViewData["RECEIVED_SELECT"] = DB.RCMSAdvertises.ToList().OrderBy(w => w.Company);
            ViewData["WORKTYPE_SELECT"] = DB.HRWorkingTypes.ToList().OrderBy(w => w.Description);
        }
        #endregion 
    }
}
