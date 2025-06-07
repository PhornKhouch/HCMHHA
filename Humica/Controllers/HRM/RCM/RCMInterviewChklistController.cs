using DevExpress.Web.Mvc;
using Humica.Core.DB;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic;
using Humica.Logic.RCM;
using Humica.Models.Report.HRM;
using Humica.Models.SY;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.HRM.RCM
{
    public class RCMInterviewChklistController : Humica.EF.Controllers.MasterSaleController
    {
        private const string SCREEN_ID = "RCM0000006";
        private const string URL_SCREEN = "/HRM/RCM/RCMInterviewChklist/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "ApplicantID";

        HumicaDBContext DB = new HumicaDBContext();
        HumicaDBViewContext DBV = new HumicaDBViewContext();
        public RCMInterviewChklistController()
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
            RCMIntVChklstObject BSM = new RCMIntVChklstObject();
            BSM.Filtering = new FilterCandidate();
            BSM.ListCandidate = new List<RCMApplicant>();
            BSM.ListInterViewer = new List<RCMVInterviewer>();
            BSM.ListInt = new List<RCMPInterview>();
            BSM.Filtering.IntVStep = 1;
            BSM.ListInt = DB.RCMPInterviews.ToList().OrderByDescending(w => w.TranNo).ToList();
            string Open = SYDocumentStatus.OPEN.ToString();
            BSM.ListCandidate = DB.RCMApplicants.Where(w => w.IntVStatus == Open).ToList();
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        [HttpPost]
        public ActionResult Index(RCMIntVChklstObject collection)
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            DataSelector();
            RCMIntVChklstObject BSM = new RCMIntVChklstObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMIntVChklstObject)Session[Index_Sess_Obj + ActionName];
            }
            string Open = SYDocumentStatus.OPEN.ToString();
            BSM.Filtering.Vacancy = collection.Filtering.Vacancy;
            BSM.Filtering.IntVStep = collection.Filtering.IntVStep;
            var _listApplicant = DB.RCMApplicants.Where(w => w.IntvStep == BSM.Filtering.IntVStep && w.VacNo == BSM.Filtering.Vacancy).ToList();
            BSM.ListCandidate = _listApplicant.Where(w => w.IntVStatus == Open).ToList();
            BSM.ListInt = DB.RCMPInterviews.Where(w => w.IntVStep == BSM.Filtering.IntVStep && w.VacNo == BSM.Filtering.Vacancy).ToList();
            collection.ListCandidate = BSM.ListCandidate;
            collection.ListInt = BSM.ListInt;
            Session[Index_Sess_Obj + ActionName] = collection;
            return View(collection);
        }
        #endregion 
        #region 'Create'
        public ActionResult Create(string ApplicantID)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();

            RCMIntVChklstObject BSD = new RCMIntVChklstObject();

            BSD.Header = new RCMPInterview();
            BSD.ListQuestionnair = new List<RCMIntVQuestionnaire>();
            BSD.ListInterViewer = new List<RCMVInterviewer>();
            var _App = DB.RCMApplicants.FirstOrDefault(w => w.ApplicantID == ApplicantID);
            if (_App != null)
            {
                BSD.VacNo = _App.VacNo;
                BSD.IntVStep = _App.IntvStep;
                BSD.Header.ApplicantID = _App.ApplicantID;
                BSD.Header.CandidateName = _App.AllName;
                BSD.Header.ApplyPost = _App.ApplyPosition;
                BSD.Header.IntVStep = Convert.ToInt32(_App.IntvStep);
                BSD.Header.Status = SYDocumentStatus.OPEN.ToString();
                BSD.Header.StartTime = DateTime.Now;
                BSD.Header.EndTime = DateTime.Now;
                BSD.ExpectSalary = Convert.ToDecimal(_App.ExpectSalary);
                BSD.WorkType = _App.WorkingType;
                BSD.Gender = _App.Gender;
                BSD.Header.AppointmentDate = DateTime.Now;
                BSD.Header.AlertToInterviewer = "EM";
                //BSD.Header.StageAssignTo = ShowData(ApplicantID);
                var _ListQuest = DB.RCMSQuestionnaires.ToList();
                _ListQuest = _ListQuest.Where(w => w.Position == _App.ApplyPosition && w.Step == _App.IntvStep).ToList();
                if (_ListQuest.Count() > 0)
                {
                    int num = 0;
                    foreach (var read in _ListQuest)
                    {
                        var _Q = new RCMIntVQuestionnaire();
                        _Q.IntVStep = Convert.ToInt32(read.Step);
                        _Q.LineItem = num + 1;
                        _Q.Description = read.Description;
                        _Q.QType = read.Position;
                        BSD.ListQuestionnair.Add(_Q);
                        num++;
                    }
                }
                var _ListIntVer = DB.RCMVInterviewers.ToList();
                _ListIntVer = _ListIntVer.Where(w => w.Code == _App.VacNo && w.IntStep == _App.IntvStep).ToList();
                if (_ListIntVer.Count() > 0)
                {
                    BSD.ListInterViewer = _ListIntVer.ToList();
                }
                UserConfListAndForm();
                Session[Index_Sess_Obj + ActionName] = BSD;
                return View(BSD);
            }

            Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE");
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        [HttpPost]
        public ActionResult Create(RCMIntVChklstObject collection)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.ADD);

            var BSM = (RCMIntVChklstObject)Session[Index_Sess_Obj + ActionName];

            collection.ListQuestionnair = BSM.ListQuestionnair.ToList();
            collection.ListInterViewer = BSM.ListInterViewer.ToList();
            collection.IntVStep = BSM.IntVStep;
            collection.VacNo = BSM.VacNo;

            if (ModelState.IsValid)
            {
                string msg = collection.createJobIntV();

                if (msg == SYConstant.OK)
                {
                    SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                    //mess.DocumentNumber = collection.Header.ApplicantID;
                    //mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Edit?ApplicantID=" + mess.DocumentNumber;
                    Session[Index_Sess_Obj + this.ActionName] = BSM;
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
            Session[Index_Sess_Obj + this.ActionName] = collection;
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            return View(BSM);
        }
        #endregion 
        #region 'Edit'
        public ActionResult Edit(string TranNo)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMIntVChklstObject BSM = new RCMIntVChklstObject();

            int Tran = Convert.ToInt32(TranNo);
            BSM.Header = DB.RCMPInterviews.FirstOrDefault(w => w.TranNo == Tran);

            if (BSM.Header != null)
            {
                var chkData = DB.RCMApplicants.FirstOrDefault(w => w.ApplicantID == BSM.Header.ApplicantID);
                BSM.VacNo = chkData.VacNo;
                BSM.IntVStep = chkData.IntvStep;
                BSM.WorkType = chkData.WorkingType;
                BSM.Gender = chkData.Gender;
                BSM.ExpectSalary = Convert.ToDecimal(chkData.ExpectSalary);
                BSM.ListQuestionnair = DB.RCMIntVQuestionnaires.Where(w => w.ApplicantID == BSM.Header.ApplicantID && w.IntVStep == BSM.Header.IntVStep).ToList();
                BSM.ListInterViewer = DB.RCMVInterviewers.Where(w => w.Code == BSM.VacNo && w.IntStep == BSM.Header.IntVStep).ToList();
                Session[Index_Sess_Obj + ActionName] = BSM;
                return View(BSM);
            }
            Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE");
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        [HttpPost]
        public ActionResult Edit(string TranNo, RCMIntVChklstObject collection)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();

            RCMIntVChklstObject BSM = new RCMIntVChklstObject();

            BSM = (RCMIntVChklstObject)Session[Index_Sess_Obj + ActionName];

            collection.ListQuestionnair = BSM.ListQuestionnair;
            collection.ListInterViewer = BSM.ListInterViewer;
            collection.ScreenId = SCREEN_ID;

            if (ModelState.IsValid)
            {
                string msg = collection.updateCheckList(TranNo);

                if (msg != SYConstant.OK)
                {
                    SYMessages mess_err = SYMessages.getMessageObject(msg, user.Lang);
                    Session[Index_Sess_Obj + this.ActionName] = BSM;
                    UserConfForm(ActionBehavior.SAVEGRID);
                    Session[SYConstant.MESSAGE_SUBMIT] = mess_err;
                    return View(collection);
                }
                SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                //mess.DocumentNumber = ApplicantID;
                //mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Edit?ApplicantID=" + mess.DocumentNumber; ;
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
        public ActionResult Details(string TranNo)
        {
            ActionName = "Details";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            ViewData[SYConstant.PARAM_ID] = TranNo;
            ViewData[ClsConstant.IS_READ_ONLY] = true;
            RCMIntVChklstObject BSM = new RCMIntVChklstObject();

            int Tran = Convert.ToInt32(TranNo);
            BSM.Header = DB.RCMPInterviews.FirstOrDefault(w => w.TranNo == Tran);

            if (BSM.Header != null)
            {
                var chkData = DB.RCMApplicants.FirstOrDefault(w => w.ApplicantID == BSM.Header.ApplicantID);
                BSM.VacNo = chkData.VacNo;
                BSM.IntVStep = chkData.IntvStep;
                BSM.WorkType = chkData.WorkingType;
                BSM.Gender = chkData.Gender;
                BSM.ExpectSalary = Convert.ToDecimal(chkData.ExpectSalary);
                BSM.ListQuestionnair = DB.RCMIntVQuestionnaires.Where(w => w.ApplicantID == BSM.Header.ApplicantID && w.IntVStep == BSM.Header.IntVStep).ToList();
                BSM.ListInterViewer = DB.RCMVInterviewers.Where(w => w.Code == BSM.VacNo && w.IntStep == BSM.Header.IntVStep).ToList();
                Session[Index_Sess_Obj + ActionName] = BSM;
                return View(BSM);
            }
            Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE");
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        #endregion
        #region 'Grid'
        public ActionResult GridItems()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMIntVChklstObject BSM = new RCMIntVChklstObject();
            BSM.ListInt = new List<RCMPInterview>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMIntVChklstObject)Session[Index_Sess_Obj + ActionName];
            }

            return PartialView("GridItems", BSM.ListInt);
        }
        public ActionResult _Candidate()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMIntVChklstObject BSM = new RCMIntVChklstObject();
            BSM.ListCandidate = new List<RCMApplicant>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMIntVChklstObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("_Candidate", BSM.ListCandidate);
        }
        public ActionResult _Question()
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMIntVChklstObject BSM = new RCMIntVChklstObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMIntVChklstObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("_Question", BSM.ListQuestionnair);
        }
        public ActionResult CreateItem(RCMIntVQuestionnaire ModelObject)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMIntVChklstObject BSM = new RCMIntVChklstObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMIntVChklstObject)Session[Index_Sess_Obj + ActionName];
            }
            if (ModelState.IsValid)
            {
                try
                {
                    if (BSM.ListQuestionnair.Count == 0)
                    {
                        ModelObject.LineItem = 1;
                    }
                    else
                    {
                        ModelObject.LineItem = BSM.ListQuestionnair.Max(w => w.LineItem) + 1;
                    }

                    BSM.ListQuestionnair.Add(ModelObject);
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
            return PartialView("_Question", BSM.ListQuestionnair);
        }
        public ActionResult EditItem(RCMIntVQuestionnaire ModelObject)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMIntVChklstObject BSM = new RCMIntVChklstObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMIntVChklstObject)Session[Index_Sess_Obj + ActionName];


                var objCheck = BSM.ListQuestionnair.Where(w => w.LineItem == ModelObject.LineItem).ToList();

                if (objCheck.Count > 0)
                {
                    objCheck.First().Description = ModelObject.Description;
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;


            }
            return PartialView("_Question", BSM.ListQuestionnair);
        }
        public ActionResult DeleteItem(int LineItem)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMIntVChklstObject BSM = new RCMIntVChklstObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMIntVChklstObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListQuestionnair.Where(w => w.LineItem == LineItem).ToList();

                if (objCheck.Count > 0)
                {
                    BSM.ListQuestionnair.Remove(objCheck.First());
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return PartialView("_Question", BSM.ListQuestionnair);
        }
        public ActionResult _InterviewerAssign()
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMIntVChklstObject BSM = new RCMIntVChklstObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMIntVChklstObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("_InterviewerAssign", BSM.ListInterViewer);
        }
        public ActionResult CreateInt(RCMVInterviewer ModelObject)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMIntVChklstObject BSM = new RCMIntVChklstObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMIntVChklstObject)Session[Index_Sess_Obj + ActionName];
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var _list = BSM.ListInterViewer.ToList();
                    var objCheck = _list.FirstOrDefault(w => w.IntStep == ModelObject.IntStep && w.EmpCode == ModelObject.EmpCode);
                    if (objCheck != null)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("RECORD_EXIST");
                    }
                    else
                    {
                        if (_list.Count == 0)
                        {
                            ModelObject.LineItem = 1;
                        }
                        else
                        {
                            ModelObject.LineItem = _list.Max(w => w.LineItem) + 1;
                        }

                        BSM.ListInterViewer.Add(ModelObject);
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
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                ViewData["EditError"] = SYMessages.getMessage("EE001", user.Lang);
            }

            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("_InterviewerAssign", BSM.ListInterViewer);
        }
        public ActionResult DeleteInt(int LineItem)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMIntVChklstObject BSM = new RCMIntVChklstObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMIntVChklstObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListInterViewer.Where(w => w.LineItem == LineItem).ToList();

                if (objCheck.Count > 0)
                {
                    BSM.ListInterViewer.Remove(objCheck.First());
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return PartialView("_InterviewerAssign", BSM.ListInterViewer);
        }
        #endregion
        #region 'Print'
        public ActionResult Print(string TranNo)
        {
            this.UserSession();
            this.UserConf(ActionBehavior.VIEWONLY);
            ViewData[SYSConstant.PARAM_ID] = TranNo;
            this.UserMVCReportView();
            return View("ReportView");
        }
        public ActionResult DocumentViewerPartial(string TranNo)
        {
            this.UserSession();
            this.UserConf(ActionBehavior.VIEWONLY);
            this.ActionName = "Print";
            int Tran = Convert.ToInt32(TranNo);
            var obj = DB.RCMPInterviews.Where(w => w.TranNo == Tran).ToList();
            if (obj.Count > 0)
            {
                try
                {
                    ViewData[SYSConstant.PARAM_ID] = Tran;
                    FRMInterviewChklstTakeNote FRMInterviewChklstTakeNote = new FRMInterviewChklstTakeNote();
                    FRMInterviewChklstTakeNote.Parameters["TranNo"].Value = obj.First().TranNo;
                    FRMInterviewChklstTakeNote.Parameters["TranNo"].Visible = false;
                    Session[this.Index_Sess_Obj + this.ActionName] = FRMInterviewChklstTakeNote;
                    return PartialView("PrintForm", FRMInterviewChklstTakeNote);
                }
                catch (Exception ex)
                {
                    SYEventLogObject.saveEventLog(new SYEventLog()
                    {
                        ScreenId = "RCM0000006",
                        UserId = this.user.UserID.ToString(),
                        DocurmentAction = Tran.ToString(),
                        Action = SYActionBehavior.ADD.ToString()
                    }, ex, true);
                }
            }
            return null;
        }
        public ActionResult DocumentViewerExportTo(string JobID)
        {
            ActionName = "Print";
            FRMInterviewChklstTakeNote reportModel = (FRMInterviewChklstTakeNote)Session[Index_Sess_Obj + ActionName];
            ViewData[SYSConstant.PARAM_ID] = JobID;
            return ReportViewerExtension.ExportTo(reportModel);
        }
        #endregion
        public ActionResult ReleaseDoc(string id)
        {
            UserSession();
            RCMIntVChklstObject BSM = new RCMIntVChklstObject();
            if (!string.IsNullOrEmpty(id))
            {
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (RCMIntVChklstObject)Session[Index_Sess_Obj + ActionName];
                }
                BSM.ScreenId = SCREEN_ID;

                int Tran = Convert.ToInt32(id);
                string msg = BSM.ReleaseDoc(Tran);
                if (msg == SYConstant.OK)
                {
                    var mess = SYMessages.getMessageObject("DOC_RQ", user.Lang);
                    mess.DocumentNumber = id;
                    mess.Description = mess.Description + BSM.ErrorMessage;
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
        #region 'Private Code'
        //private string ShowData(string ApplicantID)
        //{
        //    var _VacCode = DB.RCMApplicants.Where(w => w.ApplicantID == ApplicantID).Select(x=>x.VacNo);
        //    var _Interviewer = DB.RCMVInterviewers.Where(w => _VacCode.Contains(w.Code)).ToList();
        //    ViewData["SELECT_STAGE"] = _Interviewer;
        //    return "";
        //}
        private void DataSelector()
        {
            SYDataList objList = new SYDataList("INTV_STATUS");
            ViewData["STATUS_SELECT"] = objList.ListData;
            ViewData["POSITION_SELECT"] = ClsFilter.LoadPosition();
            ViewData["EMPCODE_SELECT"] = DBV.HR_STAFF_VIEW.ToList();
            ViewData["SELECT"] = DB.RCMApplicants.ToList();
            objList = new SYDataList("SEX");
            ViewData["GENDER_SELECT"] = objList.ListData;
            ViewData["WORKTYPE_SELECT"] = DB.HRWorkingTypes.ToList().OrderBy(w => w.Description);
            var completed = SYDocumentStatus.COMPLETED.ToString();
            ViewData["VACANCY_SELECT"] = DB.RCMVacancies.Where(w => w.Status != completed).ToList().OrderBy(w => w.Code);
            objList = new SYDataList("STAFF_TYPE");
            ViewData["STAFFTYPE_SELECT"] = objList.ListData;
        }
        #endregion 
    }
}