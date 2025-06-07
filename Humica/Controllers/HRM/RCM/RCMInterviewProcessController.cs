using DevExpress.Web.Mvc;
using Humica.Core.DB;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic;
using Humica.Logic.RCM;
using Humica.Models.SY;
using HUMICA.Models.Report.RCM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.HRM.RCM
{
    public class RCMInterviewProcessController : Humica.EF.Controllers.MasterSaleController
    {
        private const string SCREEN_ID = "RCM0000007";
        private const string URL_SCREEN = "/HRM/RCM/RCMInterviewProcess/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "ApplicantID";
        private string PATH_FILE = "12313123123sadfsdfsdfsdf";

        HumicaDBContext DB = new HumicaDBContext();
        public RCMInterviewProcessController()
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
            DataSelector();
            UserConfListAndForm(this.KeyName);
            RCMPInterviewObject BSM = new RCMPInterviewObject();

            BSM.Filter = new RCMApplicant();
            BSM.ListInterview = new List<RCMPInterview>();
            BSM.ListWaiting = new List<RCMPInterview>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMPInterviewObject)Session[Index_Sess_Obj + ActionName];
            }
            string Pending = SYDocumentStatus.PENDING.ToString();
            BSM.ListInterview = DB.RCMPInterviews.Where(w => w.ReStatus != Pending).ToList().OrderByDescending(w => w.TranNo).ToList();
            BSM.ListWaiting = DB.RCMPInterviews.Where(w => w.ReStatus == Pending).ToList().OrderByDescending(w => w.TranNo).ToList();
            BSM.ApplyPosition = "";
            BSM.Vacancy = "";
            BSM.Filter.IntvStep = 1;
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        [HttpPost]
        public ActionResult Index(RCMPInterviewObject collection)
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            DataSelector();
            RCMPInterviewObject BSM = new RCMPInterviewObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMPInterviewObject)Session[Index_Sess_Obj + ActionName];
            }
            string pass = SYDocumentStatus.PASS.ToString();
            BSM.Filter = collection.Filter;
            BSM.Vacancy = collection.Filter.VacNo;
            BSM.ApplyPosition = collection.Filter.ApplyPosition;
            BSM.IntvStep = (int)collection.Filter.IntvStep;
            var _ListInterview = DB.RCMPInterviews.ToList();
            string Pending = SYDocumentStatus.PENDING.ToString();
            BSM.ListWaiting = _ListInterview.Where(x => x.VacNo == BSM.Vacancy && x.IntVStep == BSM.IntvStep && x.Status == Pending).ToList();
            BSM.ListInterview = DB.RCMPInterviews.Where(w => w.Status != Pending).ToList();
            BSM.ListInterview = DB.RCMPInterviews.Where(w => w.IntVStep == BSM.Filter.IntvStep && w.VacNo == BSM.Filter.VacNo).ToList();
            collection.ListInterview = BSM.ListInterview;
            collection.ListWaiting = BSM.ListWaiting;
            Session[Index_Sess_Obj + ActionName] = collection;
            return View(collection);
        }
        #endregion 
        #region 'Grid'
        public ActionResult _ListInterview()
        {
            ActionName = "Index";
            DataSelector();
            UserSession();
            UserConfListAndForm(this.KeyName);
            RCMPInterviewObject BSM = new RCMPInterviewObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMPInterviewObject)Session[Index_Sess_Obj + ActionName];
            }
            BSM.ListInterview = DB.RCMPInterviews.Where(w => w.Status != "OPEN").ToList();
            return PartialView("_ListInterview", BSM.ListInterview);
        }
        public ActionResult _ListWaiting()
        {
            ActionName = "Index";
            DataSelector();
            UserSession();
            UserConfListAndForm(this.KeyName);
            RCMPInterviewObject BSM = new RCMPInterviewObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMPInterviewObject)Session[Index_Sess_Obj + ActionName];
            }
            BSM.ListWaiting = DB.RCMPInterviews.Where(w => w.Status == "OPEN").ToList();
            return PartialView("_ListWaiting", BSM.ListWaiting);
        }
        #endregion 
        #region 'Attach File'
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
        #region 'Interview'
        public ActionResult Interview(string TranNo)
        {
            ActionName = "Interview";
            UserSession();
            DataSelector();
            RCMPInterviewObject BSM = new RCMPInterviewObject();
            UserConfListAndForm();
            BSM.Header = new RCMPInterview();
            BSM.Filter = new RCMApplicant();
            BSM.ListFactor = new List<RCMInterveiwFactor>();
            BSM.ListInterviewRating = new List<RCMInterveiwRating>();
            BSM.ListRegion = new List<RCMInterveiwRegion>();
            BSM.ListScore = new List<RCMEmpEvaluateScore>();
            if (TranNo == "null") TranNo = null;
            if (TranNo != null)
            {
                int Tran = Convert.ToInt32(TranNo);
                var _Interview = DB.RCMPInterviews.FirstOrDefault(w => w.TranNo == Tran);
                if (_Interview != null)
                {
                    var _App = DB.RCMApplicants.FirstOrDefault(w => w.ApplicantID == _Interview.ApplicantID);
                    BSM.Header = _Interview;
                    BSM.Header.ProposedSalary = 0;
                    BSM.Header.SalaryAfterProb = 0;
                    BSM.Header.IntVDate = BSM.Header.AppointmentDate;
                    BSM.Filter.ExpectSalary = _App.ExpectSalary;
                    BSM.Filter.OthAllName = _App.OthAllName;
                    if (_App.Gender == "M") BSM.Filter.Gender = "Male";
                    else if (_App.Gender == "F") BSM.Filter.Gender = "Female";
                    if (_App.Nationality != null)
                    {
                        var Nation = DB.HRNations.FirstOrDefault(W => W.Code == _App.Nationality);
                        if (Nation != null) BSM.Filter.Nationality = Nation.Description;
                    }
                    BSM.Header.ApplyDate = _App.ApplyDate;
                    BSM.Header.PositionOffer = _App.ApplyPosition;
                    BSM.Header.Status = "";
                    if (BSM.Header.IntVStep > 1)
                    {
                        int Step = BSM.Header.IntVStep - 1;
                        var _chk = DB.RCMPInterviews.FirstOrDefault(w => w.ApplicantID == BSM.Header.ApplicantID && w.IntVStep == Step);
                        if (_chk != null)
                        {
                            BSM.Header.PositionOffer = _chk.PositionOffer;
                            BSM.Header.ProposedSalary = _chk.ProposedSalary;
                        }
                    }
                    BSM.ListRegion = DB.RCMInterveiwRegions.ToList();
                    if (BSM.ListRegion.Count() > 0)
                    {
                        var quiz = DB.RCMInterveiwFactors.ToList();
                        var Rating = DB.RCMInterveiwRatings.ToList();
                        BSM.ListFactor = quiz.Where(w => BSM.ListRegion.Where(x => x.Code == w.Region).Any()).ToList();
                        BSM.ListInterviewRating = Rating.ToList();
                    }
                    Session[Index_Sess_Obj + ActionName] = BSM;
                    return View(BSM);
                }
            }
            Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE");
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        [HttpPost]
        public ActionResult Interview(string TranNo, RCMPInterviewObject collection)
        {
            ActionName = "Interview";
            UserSession();
            DataSelector();
            UserConfListAndForm();

            RCMPInterviewObject BSM = new RCMPInterviewObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMPInterviewObject)Session[Index_Sess_Obj + ActionName];
                BSM.Header = collection.Header;
            }
            if (Session[PATH_FILE] != null)
            {
                BSM.Header.AttachFile = Session[PATH_FILE].ToString();
                Session[PATH_FILE] = null;
            }
            BSM.ScreenId = SCREEN_ID;

            string msg = BSM.createIntV(TranNo);

            if (msg == SYConstant.OK)
            {
                SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                Session[Index_Sess_Obj + this.ActionName] = BSM;
                UserConfForm(ActionBehavior.SAVEGRID);
                BSM.ListScore = new List<RCMEmpEvaluateScore>();
                Session[SYConstant.MESSAGE_SUBMIT] = mess;
                return View(BSM);
            }
            else
            {
                SYMessages mess_err = SYMessages.getMessageObject(msg, user.Lang);
                Session[Index_Sess_Obj + this.ActionName] = BSM;
                UserConfForm(ActionBehavior.SAVEGRID);
                BSM.ListScore = new List<RCMEmpEvaluateScore>();
                Session[SYConstant.MESSAGE_SUBMIT] = mess_err;
                return View(BSM);
            }
        }
        #endregion
        #region View
        public ActionResult Details(string TranNo)
        {
            ActionName = "Details";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMPInterviewObject BSM = new RCMPInterviewObject();
            BSM.Header = new RCMPInterview();
            BSM.Filter = new RCMApplicant();
            BSM.ListFactor = new List<RCMInterveiwFactor>();
            BSM.ListScore = new List<RCMEmpEvaluateScore>();
            BSM.ListRegion = new List<RCMInterveiwRegion>();
            BSM.ListInterviewRating = new List<RCMInterveiwRating>();
            int Tran = Convert.ToInt32(TranNo);
            BSM.Header = DB.RCMPInterviews.FirstOrDefault(w => w.TranNo == Tran);
            if (BSM.Header != null)
            {
                BSM.Filter = DB.RCMApplicants.FirstOrDefault(x => x.ApplicantID == BSM.Header.ApplicantID);
                var _Interview = DB.RCMPInterviews.FirstOrDefault(w => w.TranNo == Tran);
                if (_Interview != null)
                {
                    var _App = DB.RCMApplicants.FirstOrDefault(w => w.ApplicantID == _Interview.ApplicantID);
                    BSM.Header = _Interview;
                    BSM.Header.IntVDate = BSM.Header.AppointmentDate;
                    BSM.Filter.ExpectSalary = _App.ExpectSalary;
                    BSM.Header.ApplyDate = _App.ApplyDate;
                    BSM.Header.PositionOffer = _App.ApplyPosition;
                    BSM.Filter.OthAllName = _App.OthAllName;
                    if (_App.Gender == "M") BSM.Filter.Gender = "Male";
                    else if (_App.Gender == "F") BSM.Filter.Gender = "Female";
                    if (_App.Nationality != null)
                    {
                        var Nation = DB.HRNations.FirstOrDefault(W => W.Code == _App.Nationality);
                        if (Nation != null) BSM.Filter.Nationality = Nation.Description;
                    }
                    BSM.ListRegion = DB.RCMInterveiwRegions.ToList();
                    var Applicant = DB.RCMEmpEvaluateScores.Where(w => w.Applicant == BSM.Header.ApplicantID && w.InVStep == BSM.Header.IntVStep).ToList();
                    var EmpRating = DB.RCMInterveiwRatings.ToList();
                    BSM.ListInterviewRating = EmpRating.ToList();
                    if (Applicant.Count() > 0)
                    {
                        BSM.ListScore = Applicant.ToList();
                    }
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
                return View(BSM);
            }
            Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE");
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        #endregion
        #region Next Step
        public ActionResult GetDate(string Code, string Action)
        {
            ActionName = Action;
            RCMPInterviewObject BSM = new RCMPInterviewObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMPInterviewObject)Session[Index_Sess_Obj + ActionName];
            }
            var ObjMatch = DB.RCMPInterviews.Where(w => w.ApplicantID == Code).ToList().OrderBy(w => w.IntVStep).Last();
            if (ObjMatch != null)
            {
                var result = new
                {
                    MS = SYConstant.OK
                };
                string msg = BSM.NextStep(Code);
                if (msg != SYConstant.OK)
                {
                    var ms = new { result = SYConstant.FAIL };
                }
                else return Json(result, JsonRequestBehavior.DenyGet);
            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        #endregion
        #region 'Cancel'
        public ActionResult CancelRequest(string TranNo)
        {
            ActionName = "Details";
            UserSession();
            UserConfForm(SYActionBehavior.EDIT);
            DataSelector();
            if (TranNo != null)
            {
                RCMPInterviewObject BSM = new RCMPInterviewObject();
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (RCMPInterviewObject)Session[Index_Sess_Obj + ActionName];
                }

                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.Cancel(TranNo);
                if (msg == SYConstant.OK)
                {
                    var mess = SYMessages.getMessageObject("DOC_CANCELLED", user.Lang);
                    mess.Description = mess.Description + ". " + BSM.ErrorMessage;
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

                    FRMCandidateEvaluation sa = new FRMCandidateEvaluation();

                    var objRpt = DB.CFReportObjects.Where(w => w.ScreenID == SCREEN_ID && w.IsActive == true).ToList();

                    if (objRpt.Count > 0) sa.LoadLayoutFromXml(ClsConstant.DEFAULT_REPORT_PATH + objRpt.First().ReportObject);

                    sa.Parameters["TranNo"].Value = obj.First().TranNo;
                    sa.Parameters["TranNo"].Visible = false;
                    Session[this.Index_Sess_Obj + this.ActionName] = sa;
                    return PartialView("PrintForm", sa);
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
        public ActionResult DocumentViewerExportTo(string TranNo)
        {
            ActionName = "Print";
            FRMCandidateEvaluation reportModel = (FRMCandidateEvaluation)Session[Index_Sess_Obj + ActionName];
            ViewData[SYSConstant.PARAM_ID] = TranNo;
            return ReportViewerExtension.ExportTo(reportModel);
        }
        #endregion
        [HttpPost]
        public ActionResult CheckValue(string Value, string Region, string Action)
        {
            ActionName = Action;
            UserSession();
            UserConfListAndForm();
            RCMPInterviewObject BSM = new RCMPInterviewObject();
            BSM.ListScore = new List<RCMEmpEvaluateScore>();
            var Result = "";
            if (Value != null)
            {
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (RCMPInterviewObject)Session[Index_Sess_Obj + ActionName];

                    string[] sp = Value.Split('_');
                    string Code = sp[0].ToString();
                    int EvID = Convert.ToInt32(sp[1]);
                    int Score = Convert.ToInt32(sp[2]);
                    var checkexist = BSM.ListScore.Where(w => w.Code == Code).ToList();
                    if (checkexist.Count == 0)
                    {
                        var obj = new RCMEmpEvaluateScore();
                        obj.Code = Code;
                        obj.Region = Region;
                        obj.Score = Score;
                        BSM.ListScore.Add(obj);
                    }
                    else
                    {
                        checkexist.First().Code = Code;
                        checkexist.First().Score = Score;
                    }
                    var Rating = DB.RCMInterveiwRegions.ToList().Sum(w => w.Rating);
                    var Resul = BSM.ListScore.ToList().Sum(w => w.Score);
                    if (Resul >= Rating)
                    {
                        Result = "Score " + Resul.Value + " is Pass";

                    }
                    else Result = "Score " + Resul.Value + " is FAIL";
                    var result = new
                    {
                        MS = SYConstant.OK,
                        Total = Result
                    };
                    Session[Index_Sess_Obj + ActionName] = BSM;
                    return Json(result, JsonRequestBehavior.DenyGet);
                }
            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public ActionResult CommentValue(string Code, string Value, string Region, string Action)
        {
            ActionName = Action;
            UserSession();
            UserConfListAndForm();

            RCMPInterviewObject BSM = new RCMPInterviewObject();
            BSM.ListScore = new List<RCMEmpEvaluateScore>();
            if (Value != null)
            {
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (RCMPInterviewObject)Session[Index_Sess_Obj + ActionName];
                    var checkexist = BSM.ListScore.Where(w => w.Code == Code).ToList();
                    if (checkexist.Count == 0)
                    {
                        var obj = new RCMEmpEvaluateScore();
                        obj.Code = Code;
                        obj.Region = Region;
                        obj.Remark = Value;
                        BSM.ListScore.Add(obj);
                    }
                    else
                    {
                        checkexist.First().Code = Code;
                        checkexist.First().Remark = Value;
                    }
                    var result = new
                    {
                        MS = SYConstant.OK,
                    };
                    Session[Index_Sess_Obj + ActionName] = BSM;
                    return Json(result, JsonRequestBehavior.DenyGet);
                }
            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        #region 'Private Code'
        private void DataSelector()
        {
            var completed = SYDocumentStatus.COMPLETED.ToString();
            ViewData["VACANCY_SELECT"] = DB.RCMVacancies.Where(w => w.Status != completed).ToList().OrderBy(w => w.Code);
            SYDataList objList = new SYDataList("INTV_STATUS");
            ViewData["STATUS_SELECT"] = objList.ListData;
            //ViewData["LANG_SELECT"] = DB.RCMSLangs.ToList().OrderBy(w => w.Code);
            //ViewData["COUNTRY_SELECT"] = DB.HRCountries.ToList().OrderBy(w => w.Description);
            ViewData["POSITION_SELECT"] = ClsFilter.LoadPosition();
            objList = new SYDataList("SEX");
            //ViewData["GENDER_SELECT"] = objList.ListData;
        }
        #endregion
        #region 'Not In Use'
        //public ActionResult GirdWorkHistory()
        //{
        //    ActionName = "Index";
        //    UserSession();
        //    UserConfListAndForm();
        //    DataSelector();
        //    RCMPInterviewObject BSM = new RCMPInterviewObject();
        //    BSM.ListWorkHistory = new List<RCMAWorkHistory>();
        //    if (Session[Index_Sess_Obj + ActionName] != null)
        //    {
        //        BSM = (RCMPInterviewObject)Session[Index_Sess_Obj + ActionName];
        //    }

        //    return PartialView("TapWorkHistory", BSM);
        //}
        //public ActionResult GridLang()
        //{
        //    ActionName = "Index";
        //    UserSession();
        //    UserConfListAndForm();
        //    DataSelector();
        //    RCMPInterviewObject BSM = new RCMPInterviewObject();
        //    BSM.ListLanguage = new List<RCMALanguage>();
        //    if (Session[Index_Sess_Obj + ActionName] != null)
        //    {
        //        BSM = (RCMPInterviewObject)Session[Index_Sess_Obj + ActionName];
        //    }
        //    return PartialView("TapLang", BSM);
        //}
        //public ActionResult GridEdu()
        //{
        //    ActionName = "Index";
        //    UserSession();
        //    UserConfListAndForm();
        //    DataSelector();
        //    RCMPInterviewObject BSM = new RCMPInterviewObject();
        //    BSM.ListEdu = new List<RCMAEdu>();
        //    if (Session[Index_Sess_Obj + ActionName] != null)
        //    {
        //        BSM = (RCMPInterviewObject)Session[Index_Sess_Obj + ActionName];
        //    }

        //    return PartialView("TapEdu", BSM);
        //}
        #endregion
    }

}