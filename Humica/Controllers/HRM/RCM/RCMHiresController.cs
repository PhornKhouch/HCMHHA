using DevExpress.Web.Mvc;
using Humica.Core.DB;
using Humica.EF;
using Humica.EF.Models.SY;
using Humica.Logic;
using Humica.Logic.RCM;
using Humica.Models.SY;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Humica.Controllers.HRM.RCM
{
    public class RCMHiresController : Humica.EF.Controllers.MasterSaleController
    {
        private const string SCREEN_ID = "RCM0000009";
        private const string URL_SCREEN = "/HRM/RCM/RCMHires/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "ApplicantID";
        //private string PATH_FILE = "12313123123sadfsdfsdfsdf";

        HumicaDBContext DB = new HumicaDBContext();
        public RCMHiresController()
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
            string Hired = SYDocumentStatus.HIRED.ToString();
            string pass = SYDocumentStatus.PASS.ToString();
            BSM.ListApplicant = DB.RCMApplicants.Where(w => w.IsHired == false && w.IsConfirm == true).ToList();
            BSM.ListHire = DB.RCMHires.ToList();
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

            // BSM.Vacancy = collection.Filter.Vacancy;
            //  BSM.ApplyPosition = collection.Filter.ApplyPosition;
            string pass = SYDocumentStatus.PASS.ToString();
            var chkHire = DB.RCMHires.Select(w => w.ApplicantID);
            var chkInt = DB.RCMPInterviews.Where(x => x.Status == pass).Select(x => x.ApplicantID);
            var chkRef = DB.RCMRefCheckPersons.Select(x => x.ApplicantID);
            BSM.ListApplicant = DB.RCMApplicants.Where(x => chkInt.Contains(x.ApplicantID) && !chkHire.Contains(x.ApplicantID)
                                                                                           && !chkRef.Contains(x.ApplicantID)).ToList();
            //  && x.Vacancy == BSM.Vacancy && x.ApplyPosition == BSM.ApplyPosition ).ToList();
            //  BSM.ListRefPerson = DB.RCMReferencePersons.Where(x=> !chkHire.Contains(x.ApplicantID)).ToList();
            // collection.ListRefPerson = BSM.ListRefPerson;
            collection.ListApplicant = BSM.ListApplicant;
            Session[Index_Sess_Obj + ActionName] = collection;
            return View(collection);
        }
        public ActionResult PartialList()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            RCMRefChkPersonObject BSM = new RCMRefChkPersonObject();
            BSM.ListHire = new List<RCMHire>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMRefChkPersonObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView(SYListConfuration.ListDefaultView, BSM.ListHire.ToList());
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
        #endregion 
        #region 'Hire'
        public ActionResult Hire(string ApplicantID)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();

            RCMRefChkPersonObject BSM = new RCMRefChkPersonObject();

            BSM.Hire = new RCMHire();
            BSM.Header = new RCMRefCheckPerson();

            DateTime DateNow = DateTime.Now;
            DateTime Probation = new DateTime();
            var ProType = DB.HRProbationTypes.FirstOrDefault();
            int ProMonth = 3;
            int day = -1;
            if (ProType != null)
            {
                day = ProType.Day;
                ProMonth = ProType.InMonth;
                BSM.Hire.ProbationType = ProType.Code;
            }
            Probation = DateNow.AddMonths(ProMonth).AddDays(day);
            BSM.Hire.StartDate = DateNow;
            BSM.Hire.ProbationEndDate = Probation;
            BSM.Hire.LeaveConf = DateNow;
            BSM.Hire.IsResident = true;
            BSM.Hire.IsAtten = false;
            BSM.Hire.IsBiSalary = false;
            BSM.Filter = DB.RCMApplicants.FirstOrDefault(x => x.ApplicantID == ApplicantID);

            if (BSM.Filter != null)
            {
                BSM.Hire.ApplicantID = BSM.Filter.ApplicantID;
                BSM.Hire.ApplicantName = BSM.Filter.AllName;
                BSM.Hire.ApplyPosition = BSM.Filter.ApplyPosition;
                BSM.Hire.Position = BSM.Filter.PostOffer;
                BSM.Hire.Section = BSM.Filter.Sect;
                BSM.Hire.Branch = BSM.Filter.ApplyBranch;
                BSM.Hire.Department = BSM.Filter.ApplyDept;
                BSM.Hire.Salary = BSM.Filter.Salary;
                BSM.Hire.ApplyDate = BSM.Filter.ApplyDate;
                BSM.Hire.WorkingType = BSM.Filter.WorkingType;
                BSM.Hire.Salary = 0;
            }

            if (BSM.Hire != null)
            {
                Session[Index_Sess_Obj + ActionName] = BSM;
                return View(BSM);
            }

            Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE");
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        [HttpPost]
        public ActionResult Hire(string ApplicantID, RCMRefChkPersonObject collection)
        {
            UserSession();
            DataSelector();
            UserConfListAndForm();

            RCMRefChkPersonObject BSM = new RCMRefChkPersonObject();

            BSM = (RCMRefChkPersonObject)Session[Index_Sess_Obj + ActionName];

            collection.ScreenId = SCREEN_ID;

            if (ModelState.IsValid)
            {

                string msg = collection.saveHire(ApplicantID);
                if (msg == SYConstant.OK)
                {
                    SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);

                    Session[Index_Sess_Obj + ActionName] = collection;
                    Session[SYConstant.MESSAGE_SUBMIT] = mess;
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

                }
                ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                Session[Index_Sess_Obj + ActionName] = collection;
                return View(collection);
            }
            Session[Index_Sess_Obj + this.ActionName] = collection;
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            return View(BSM);
        }
        #endregion
        #region 'Details'
        public ActionResult Details(string ApplicantID)
        {
            ActionName = "Details";
            UserSession();
            RCMRefChkPersonObject BSM = new RCMRefChkPersonObject();
            DataSelector();
            ViewData[SYConstant.PARAM_ID] = ApplicantID;
            ViewData[ClsConstant.IS_READ_ONLY] = true;
            BSM.ListHire = new List<RCMHire>();
            BSM.Hire = new RCMHire();
            BSM.Header = new RCMRefCheckPerson();
            BSM.Header = DB.RCMRefCheckPersons.FirstOrDefault(w => w.ApplicantID == ApplicantID);
            BSM.Hire = DB.RCMHires.FirstOrDefault(w => w.ApplicantID == ApplicantID);
            if (BSM.Header == null)
            {
                Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE");
                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
            }
            BSM.ListHire = DB.RCMHires.Where(w => w.ApplicantID == ApplicantID).ToList();
            UserConfForm(SYActionBehavior.VIEW);
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        #endregion
        #region "Ajax"
        [HttpPost]
        public string FitlerLevel(string code)
        {

            UserSession();
            UserConfListAndForm();
            if (code != null)
            {
                Session["Level"] = code;
            }

            return SYConstant.OK;
        }
        public ActionResult JobGrade()
        {
            UserSession();
            UserConfListAndForm();
            if (Session["Level"] != null)
            {
                string LevelCode = Session["Level"].ToString();
                var District = DB.HRJobGrades.ToList();
                ViewData["JOBGRADE_SELECT"] = District.ToList();
                return PartialView("JobGrade");
            }
            return null;
        }
        #endregion
        #region "Ajax Image'
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
            UploadControlExtension.GetUploadedFiles("uc_image", objFile.ValidationSettings, objFile.uc_FileUploadComplete);
            Session["PATH_IMG"] = objFile.ObjectTemplate.UpoadPath;
            return null;
        }
        public string DeleteFile(string FileName)
        {
            try
            {
                string[] sp = FileName.Split('/');
                string file = sp[sp.Length - 1];
                string[] spf = file.Split('.');
                string sfile = spf[0];

                var obj = DB.MDUploadImages.FirstOrDefault(w => w.TokenCode == sfile);
                if (obj != null)
                {
                    DB.MDUploadImages.Remove(obj);
                    int row = DB.SaveChanges();
                    if (row > 0)
                    {
                        var path = DB.CFUploadPaths.Find("IMG_UPLOAD");
                        string root = HostingEnvironment.ApplicationPhysicalPath;
                        // obj.UpoadPath = obj.UpoadPath.Replace("~", "").Replace("/", "\\");
                        obj.UpoadPath = obj.UpoadPath.Replace("~/", "").Replace("/", "\\");
                        string fileNameDelete = root + obj.UpoadPath;
                        if (System.IO.File.Exists(fileNameDelete))
                        {
                            System.IO.File.Delete(fileNameDelete);
                        }
                    }
                }
                return SYConstant.OK;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        #endregion
        #region 'Private Code'

        [HttpPost]
        public ActionResult ShowData(DateTime StartDate, string ProType)
        {
            var Pro = DB.HRProbationTypes.Find(ProType);
            int Months = 0;
            int day = 0;
            if (Pro != null)
            {
                Months = Pro.InMonth;
                day = Pro.Day;
            }
            DateTime Probation = StartDate.AddMonths(Months).AddDays(day);
            DateTime LeaveCof = StartDate;
            var result = new
            {
                MS = SYConstant.OK,
                Probation = Probation,
                LeaveCof = LeaveCof
            };
            return Json(result, JsonRequestBehavior.DenyGet);
        }
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
            var SalaryType = new SYDataList("SALARYTYPE_SELECT");
            ViewData["SALARYTYPE_SELECT"] = SalaryType.ListData;
            ViewData["STAFF_SELECT"] = DB.HRStaffProfiles.ToList();
            //var results = DB.RCMApplicants.GroupBy(n => new { n.Vacancy, n.ApplyPosition })
            //        .Select(g => new {
            //            g.Key.Vacancy,
            //            g.Key.ApplyPosition
            //        }).ToList();
            //ViewData["VACANCY_SELECT"] = results.ToList();
        }
        #endregion 
    }

}