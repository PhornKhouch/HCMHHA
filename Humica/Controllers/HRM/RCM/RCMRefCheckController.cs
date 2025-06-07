using Humica.Core.DB;
using Humica.EF;
using Humica.EF.Models.SY;
using Humica.Logic;
using Humica.Logic.RCM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.HRM.RCM
{
    public class RCMRefCheckController : Humica.EF.Controllers.MasterSaleController
    {
        private const string SCREEN_ID = "RCM0000008";
        private const string URL_SCREEN = "/HRM/RCM/RCMRefCheck/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "ApplicantID";
        private string PATH_FILE = "12313123123sadfsdfsdfsdf";

        HumicaDBContext DB = new HumicaDBContext();
        public RCMRefCheckController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
        }
        #region 'list'
        public ActionResult Index()
        {
            ActionName = "Index";
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);
            RCMRefChkPersonObject BSM = new RCMRefChkPersonObject();
            string pass = SYDocumentStatus.PASS.ToString();
            string Hired = SYDocumentStatus.HIRED.ToString();
            BSM.ListApplicant = DB.RCMApplicants.Where(w => w.RefCHK != true && w.IsHired == false && w.IntVStatus == "PASS").ToList();
            BSM.ListRefPerson = DB.RCMRefCheckPersons.ToList();
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        [HttpPost]
        public ActionResult Index(RCMRefChkPersonObject collection)
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            DataSelector();
            RCMRefChkPersonObject BSM = new RCMRefChkPersonObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMRefChkPersonObject)Session[Index_Sess_Obj + ActionName];
            }
            string pass = SYDocumentStatus.PASS.ToString();
            var chkHire = DB.RCMHires.Select(w => w.ApplicantID);
            var chkInt = DB.RCMPInterviews.Where(x => x.Status == pass).Select(x => x.ApplicantID);
            var chkRef = DB.RCMRefCheckPersons.Select(x => x.ApplicantID);
            BSM.ListApplicant = DB.RCMApplicants.Where(x => chkInt.Contains(x.ApplicantID) && !chkHire.Contains(x.ApplicantID)
                                                                                           && !chkRef.Contains(x.ApplicantID)).ToList();
            BSM.ListRefPerson = DB.RCMRefCheckPersons.Where(x => !chkHire.Contains(x.ApplicantID)).ToList();
            collection.ListRefPerson = BSM.ListRefPerson;
            collection.ListApplicant = BSM.ListApplicant;
            Session[Index_Sess_Obj + ActionName] = collection;
            return View(collection);
        }
        #endregion  
        #region 'Grid' 
        public ActionResult GridCandidate()
        {
            ActionName = "Index";
            RCMRefChkPersonObject BSM = new RCMRefChkPersonObject();
            UserSession();
            UserConfListAndForm(this.KeyName);
            BSM.ListApplicant = new List<RCMApplicant>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMRefChkPersonObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("GridCandidate", BSM.ListApplicant);
        }
        public ActionResult _ListRefCheck()
        {
            ActionName = "Index";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMRefChkPersonObject BSM = new RCMRefChkPersonObject();
            BSM.ListRefPerson = new List<RCMRefCheckPerson>();
            BSM.ListRefPerson = DB.RCMRefCheckPersons.ToList();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMRefChkPersonObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("_ListRefCheck", BSM.ListRefPerson);
        }
        #endregion
        #region 'Ref Questionnaire'
        public ActionResult GridQuestionnaire()
        {
            DataSelector();
            RCMRefChkPersonObject BSM = new RCMRefChkPersonObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMRefChkPersonObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("GridQuestionnaire", BSM.ListRefQuestion);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult CreateRefQuest(RCMRefQuestionnaire MModel)
        {
            DataSelector();
            UserSession();
            RCMRefChkPersonObject BSM = new RCMRefChkPersonObject();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (RCMRefChkPersonObject)Session[Index_Sess_Obj + ActionName];
                    }
                    if (MModel.Question == null)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("DOC_NE", user.Lang);
                    }
                    var obj = BSM.ListRefQuestion.Where(w => w.ApplicantID == BSM.Header.ApplicantID && w.Question == MModel.Question).ToList();
                    if (obj.Count > 0)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("DUPL_KEY", user.Lang);
                    }
                    if (obj.Count == 0)
                    {
                        MModel.ApplicantID = BSM.Header.ApplicantID;
                        BSM.ListRefQuestion.Add(MModel);
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
            return PartialView("GridQuestionnaire", BSM.ListRefQuestion);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult EdiRefQuest(RCMRefQuestionnaire MModel)
        {
            DataSelector();
            UserSession();
            RCMRefChkPersonObject BSM = new RCMRefChkPersonObject();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (RCMRefChkPersonObject)Session[Index_Sess_Obj + ActionName];
                    }
                    var objCheck = BSM.ListRefQuestion.Where(w => w.ApplicantID == BSM.Header.ApplicantID && w.Question == MModel.Question).ToList();
                    if (objCheck.Count > 0)
                    {
                        objCheck.First().Question = MModel.Question;
                        objCheck.First().Answer = MModel.Answer;
                    }
                    else
                    {
                        ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
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
            return PartialView("GridQuestionnaire", BSM.ListRefQuestion);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteRefQuest(string Question)
        {
            DataSelector();
            RCMRefChkPersonObject BSM = new RCMRefChkPersonObject();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (RCMRefChkPersonObject)Session[Index_Sess_Obj + ActionName];
                    }
                    var error = 0;

                    var checkList = BSM.ListRefQuestion.Where(w => w.ApplicantID == BSM.Header.ApplicantID && w.Question == Question).ToList();
                    if (checkList.Count() == 0)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("NO_ITEM_DELETE");
                        error = 1;
                    }
                    if (error == 0)
                    {
                        BSM.ListRefQuestion.Remove(checkList.First());
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
            return PartialView("GridQuestionnaire", BSM.ListRefQuestion);
        }
        #endregion 'Languages'
        #region 'RefChk'
        public ActionResult RefChk(string ApplicantID)
        {
            UserSession();
            UserConfListAndForm();
            RCMRefChkPersonObject BSM = new RCMRefChkPersonObject();
            BSM.Header = new RCMRefCheckPerson();
            BSM.Filter = new RCMApplicant();
            BSM.RefPerson = new RCMAReference();
            BSM.ListRefQuestion = new List<RCMRefQuestionnaire>();
            BSM.Filter = DB.RCMApplicants.FirstOrDefault(x => x.ApplicantID == ApplicantID);
            BSM.RefPerson = DB.RCMAReferences.FirstOrDefault(x => x.ApplicantID == ApplicantID);
            BSM.Header.ApplicantID = BSM.Filter.ApplicantID;
            BSM.Header.Name = BSM.Filter.AllName;
            BSM.Header.RefChkDate = DateTime.Now;
            if (BSM.Filter.ApplyPosition != null)
            {
                var Post = DB.HRPositions.FirstOrDefault(w => w.Code == BSM.Filter.ApplyPosition);
                if (Post != null) { BSM.Filter.ApplyPosition = Post.Description; }
            }
            if (BSM.RefPerson != null)
            {
                BSM.Header.NameOfRef = BSM.RefPerson.RefName;
                BSM.Header.PhoneNo = BSM.RefPerson.Phone1;
                BSM.Header.OccupationOfRef = BSM.RefPerson.Occupation;
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        [HttpPost]
        public ActionResult RefChk(RCMRefChkPersonObject collection)
        {
            UserSession();
            UserConfListAndForm();
            var BSM = new RCMRefChkPersonObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMRefChkPersonObject)Session[Index_Sess_Obj + ActionName];
                BSM.Header = collection.Header;
            }
            BSM.ScreenId = SCREEN_ID;
            string msg = BSM.RefCheck();

            if (msg == SYConstant.OK)
            {
                SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                Session[Index_Sess_Obj + this.ActionName] = BSM;
                UserConfForm(ActionBehavior.SAVEGRID);
                Session[SYConstant.MESSAGE_SUBMIT] = mess;
                return View(BSM);
            }
            SYMessages mess_err = SYMessages.getMessageObject(msg, user.Lang);
            Session[Index_Sess_Obj + this.ActionName] = BSM;
            UserConfForm(ActionBehavior.SAVEGRID);
            Session[SYConstant.MESSAGE_SUBMIT] = mess_err;
            return View(BSM);
        }

        #endregion
        #region 'Details'
        public ActionResult Edit(string ApplicantID)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            RCMRefChkPersonObject BSM = new RCMRefChkPersonObject();
            BSM.ListRefQuestion = new List<RCMRefQuestionnaire>();
            var _RefCheck = DB.RCMRefCheckPersons.FirstOrDefault(w => w.ApplicantID == ApplicantID);
            if (_RefCheck != null)
            {
                BSM.Filter = DB.RCMApplicants.FirstOrDefault(w => w.ApplicantID == ApplicantID);
                if (BSM.Filter.ApplyPosition != null)
                {
                    var Post = DB.HRPositions.FirstOrDefault(w => w.Code == BSM.Filter.ApplyPosition);
                    if (Post != null) { BSM.Filter.ApplyPosition = Post.Description; }
                }
                BSM.ListRefQuestion = DB.RCMRefQuestionnaires.Where(w => w.ApplicantID == ApplicantID).ToList();
                BSM.Header = _RefCheck;
                Session[Index_Sess_Obj + ActionName] = BSM;
                return View(BSM);
            }
            Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE");
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        [HttpPost]
        public ActionResult Edit(string ApplicantID, RCMRefChkPersonObject collection)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();

            RCMRefChkPersonObject BSM = new RCMRefChkPersonObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMRefChkPersonObject)Session[Index_Sess_Obj + ActionName];
                BSM.Header = collection.Header;
            }
            collection.ScreenId = SCREEN_ID;

            if (ModelState.IsValid)
            {
                string msg = BSM.UpdateRefCheck(ApplicantID);

                if (msg != SYConstant.OK)
                {
                    SYMessages mess_err = SYMessages.getMessageObject(msg, user.Lang);
                    Session[Index_Sess_Obj + this.ActionName] = BSM;
                    UserConfForm(ActionBehavior.SAVEGRID);
                    Session[SYConstant.MESSAGE_SUBMIT] = mess_err;
                    return View(collection);
                }
                SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                Session[Index_Sess_Obj + this.ActionName] = collection;
                UserConfForm(ActionBehavior.SAVEGRID);
                Session[SYConstant.MESSAGE_SUBMIT] = mess;
                BSM.ListRefQuestion = new List<RCMRefQuestionnaire>();
                return View(BSM);

            }
            Session[Index_Sess_Obj + this.ActionName] = BSM;
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            return View(BSM);
        }
        #endregion
        #region 'Private Code'
        private void DataSelector()
        {
            SYDataList objList = new SYDataList("SEX");
            ViewData["GENDER_SELECT"] = objList.ListData;
            objList = new SYDataList("INITIAL");
            ViewData["INITIAL_SELECT"] = objList.ListData;
            objList = new SYDataList("MARITAL");
            ViewData["MARITAL_SELECT"] = objList.ListData;
            var objStatus = new SYDataList("STATUS_EMPLOYEE");
            ViewData["STATUS_EMPLOYEE"] = objStatus.ListData;
            var _listBranch = SYConstant.getBranchDataAccess();
            ViewData["BRANCHES_SELECT"] = _listBranch.ToList();
            ViewData["DIVISION_SELECT"] = ClsFilter.LoadDivision();
            ViewData["EMPTYPE_SELECT"] = DB.HREmpTypes.ToList().OrderBy(w => w.Description);
            var ObjTax = new SYDataList("TAXPAID");
            ViewData["TAXPAID_SELECT"] = ObjTax.ListData;
            ViewData["SECTION_SELECT"] = ClsFilter.LoadSection();
            ViewData["LEVEL_SELECT"] = SYConstant.getLevelDataAccess();
            ViewData["JOBGRADE_SELECT"] = DB.HRJobGrades.ToList().OrderBy(w => w.Description);
            ViewData["POSITION_SELECT"] = ClsFilter.LoadPosition();
            ViewData["DEPARTMENT_SELECT"] = ClsFilter.LoadDepartment();
            ViewData["LOCATION_SELECT"] = DB.HRLocations.ToList().OrderBy(w => w.Description);
            ViewData["LINE_SELECT"] = DB.HRLines.ToList().OrderBy(w => w.Description);
            ViewData["PERAMETER_SELECT"] = DB.PRParameters.ToList().OrderBy(w => w.Description);
            ViewData["POSITIONFAMILY_SELECT"] = DB.HRPositionFamilies.Where(w => w.IsActive == true).ToList().OrderBy(w => w.Description);
            var IdentityTye = new SYDataList("IdentityTye");
            ViewData["IdentityTye_SELECT"] = IdentityTye.ListData;
            ViewData["PROBATIONTYPE_SELECT"] = DB.HRProbationTypes.ToList();
            ViewData["ROSTER_SELECT"] = DB.ATBatches.ToList();
            ViewData["WORKINGTYPE_SELECT"] = DB.HRWorkingTypes.ToList();
            ViewData["QUESTION_SELECT"] = DB.RCMSRefQuestions.ToList().OrderBy(w => w.step);

            var results = DB.RCMApplicants.GroupBy(n => new { n.VacNo, n.ApplyPosition })
                    .Select(g => new
                    {
                        g.Key.VacNo,
                        g.Key.ApplyPosition
                    }).ToList();
            ViewData["VACANCY_SELECT"] = results.ToList();
        }
        #endregion 
    }

}