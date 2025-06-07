using Humica.Core.DB;
using Humica.EF;
using Humica.EF.Models.SY;
using Humica.Logic;
using Humica.Logic.RCM;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Humica.Controllers.HRM.RCM
{
    public class RCMPShortListController : Humica.EF.Controllers.MasterSaleController
    {

        private const string SCREEN_ID = "RCM0000004";
        private const string URL_SCREEN = "/HRM/RCM/RCMPShortList/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "ApplicantID";

        HumicaDBContext DB = new HumicaDBContext();
        public RCMPShortListController()
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
            RCMPShortListObject BSM = new RCMPShortListObject();
            BSM.Filtering = new FilterShortLsit();
            BSM.ListHeader = new List<RCMApplicant>();
            BSM.Filtering.Status = "ALL";
            var _listApplicant = DB.RCMApplicants.ToList();
            BSM.ListHeader = _listApplicant.ToList();

            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        [HttpPost]
        public ActionResult Index(RCMPShortListObject collection)
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            DataSelector();
            RCMPShortListObject BSM = new RCMPShortListObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMPShortListObject)Session[Index_Sess_Obj + ActionName];
            }
            BSM.Filtering.Vacancy = collection.Filtering.Vacancy;
            BSM.Filtering.Status = collection.Filtering.Status;
            var _listApplicant = new List<RCMApplicant>();
            if (string.IsNullOrEmpty(BSM.Filtering.Vacancy))
            {
                _listApplicant = DB.RCMApplicants.ToList();
            }
            else
                _listApplicant = DB.RCMApplicants.Where(w => w.VacNo == BSM.Filtering.Vacancy).ToList();
            if (BSM.Filtering.Status == "ALL")
            {
                BSM.ListHeader = _listApplicant.ToList();
            }
            else
            {
                BSM.ListHeader = _listApplicant.Where(w => w.ShortList == BSM.Filtering.Status).ToList();
            }
            collection.ListHeader = BSM.ListHeader;
            Session[Index_Sess_Obj + ActionName] = collection;
            return View(collection);
        }
        #endregion 
        #region 'Grid'
        public ActionResult GridItems()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            RCMPShortListObject BSM = new RCMPShortListObject();
            BSM.ListHeader = new List<RCMApplicant>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (RCMPShortListObject)Session[Index_Sess_Obj + ActionName];
            }

            return PartialView("GridItems", BSM);
        }
        #endregion
        #region 'Status'
        public ActionResult Pass(string ApplicantIDs)
        {
            ActionName = "Index";
            UserSession();
            UserConfForm(SYActionBehavior.EDIT);
            DataSelector();

            if (ApplicantIDs != null)
            {
                RCMPShortListObject BSM = new RCMPShortListObject();
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (RCMPShortListObject)Session[Index_Sess_Obj + ActionName];
                    BSM.ScreenId = SCREEN_ID;
                    string msg = BSM.Passed(ApplicantIDs);
                    if (msg == SYConstant.OK)
                    {
                        var mess = SYMessages.getMessageObject("SHORTLIST_PASS", user.Lang);
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

            }
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        public ActionResult Keep(string ApplicantIDs)
        {
            ActionName = "Index";
            UserSession();
            UserConfForm(SYActionBehavior.EDIT);
            DataSelector();
            if (ApplicantIDs != null)
            {
                RCMPShortListObject BSM = new RCMPShortListObject();
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (RCMPShortListObject)Session[Index_Sess_Obj + ActionName];
                }
                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.Kept(ApplicantIDs);
                if (msg == SYConstant.OK)
                {
                    var mess = SYMessages.getMessageObject("SHORTLIST_KEPT", user.Lang);
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
        public ActionResult Fail(string ApplicantIDs)
        {
            ActionName = "Index";
            UserSession();
            UserConfForm(SYActionBehavior.EDIT);
            DataSelector();
            if (ApplicantIDs != null)
            {
                RCMPShortListObject BSM = new RCMPShortListObject();
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (RCMPShortListObject)Session[Index_Sess_Obj + ActionName];
                }
                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.Fail(ApplicantIDs);
                if (msg == SYConstant.OK)
                {
                    var mess = SYMessages.getMessageObject("SHORTLIST_FAIL", user.Lang);
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
        public ActionResult Reject(string ApplicantIDs)
        {
            ActionName = "Index";
            UserSession();
            UserConfForm(SYActionBehavior.EDIT);
            DataSelector();
            if (ApplicantIDs != null)
            {
                RCMPShortListObject BSM = new RCMPShortListObject();
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (RCMPShortListObject)Session[Index_Sess_Obj + ActionName];
                }
                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.Reject(ApplicantIDs);
                if (msg == SYConstant.OK)
                {
                    var mess = SYMessages.getMessageObject("SHORTLIST_REJ", user.Lang);
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
        #region 'private code'
        private void DataSelector()
        {
            var objList = new SYDataList("SHORTLIST_STATUS");
            ViewData["SHORTLIST_STATUS"] = objList.ListData;
            objList = new SYDataList("SEX");
            ViewData["GENDER_SELECT"] = objList.ListData;
            var completed = SYDocumentStatus.COMPLETED.ToString();
            ViewData["VACANCY_SELECT"] = DB.RCMVacancies.Where(w => w.Status != completed).ToList().OrderBy(w => w.Code);
            ViewData["NATION_SELECT"] = DB.HRNations.ToList().OrderBy(w => w.Description);
            ViewData["LANG_SELECT"] = DB.RCMSLangs.ToList().OrderBy(w => w.Code);
            ViewData["COUNTRY_SELECT"] = DB.HRCountries.ToList().OrderBy(w => w.Description);
            ViewData["POSITION_SELECT"] = ClsFilter.LoadPosition();
            ViewData["WORKTYPE_SELECT"] = DB.HRWorkingTypes.ToList().OrderBy(w => w.Description);
            objList = new SYDataList("STAFF_TYPE");
            ViewData["STAFFTYPE_SELECT"] = objList.ListData;
        }
        #endregion
        #region 'Not In Use'
        //public ActionResult GirdWorkHistory()
        //{
        //    ActionName = "Index";
        //    UserSession();
        //    UserConfListAndForm();
        //    DataSelector();
        //    RCMPShortListObject BSM = new RCMPShortListObject();
        //    BSM.ListWorkHistory = new List<RCMAWorkHistory>();
        //    if (Session[Index_Sess_Obj + ActionName] != null)
        //    {
        //        BSM = (RCMPShortListObject)Session[Index_Sess_Obj + ActionName];
        //    }

        //    return PartialView("TapWorkHistory", BSM);
        //}
        //public ActionResult GridRef()
        //{
        //    ActionName = "Index";
        //    UserSession();
        //    UserConfListAndForm();
        //    DataSelector();
        //    RCMPShortListObject BSM = new RCMPShortListObject();
        //    BSM.ListRef = new List<RCMAReference>();
        //    if (Session[Index_Sess_Obj + ActionName] != null)
        //    {
        //        BSM = (RCMPShortListObject)Session[Index_Sess_Obj + ActionName];
        //    }

        //    return PartialView("TapRef", BSM);
        //}
        //public ActionResult GridLang()
        //{
        //    ActionName = "Index";
        //    UserSession();
        //    UserConfListAndForm();
        //    DataSelector();
        //    RCMPShortListObject BSM = new RCMPShortListObject();
        //    BSM.ListLang = new List<RCMALanguage>();
        //    if (Session[Index_Sess_Obj + ActionName] != null)
        //    {
        //        BSM = (RCMPShortListObject)Session[Index_Sess_Obj + ActionName];
        //    }

        //    return PartialView("TapLang", BSM);
        //}
        //public ActionResult GridEdu()
        //{
        //    ActionName = "Index";
        //    UserSession();
        //    UserConfListAndForm();
        //    DataSelector();
        //    RCMPShortListObject BSM = new RCMPShortListObject();
        //    BSM.ListEdu = new List<RCMAEdu>();
        //    if (Session[Index_Sess_Obj + ActionName] != null)
        //    {
        //        BSM = (RCMPShortListObject)Session[Index_Sess_Obj + ActionName];
        //    }
        //    return PartialView("TapEdu", BSM);
        //}
        //public ActionResult SelectedChanged(string ApplicantID, string Action)
        //{
        //    ActionName = Action;
        //    RCMPShortListObject BSM = new RCMPShortListObject();
        //    if (Session[Index_Sess_Obj + ActionName] != null)
        //    {
        //        BSM = (RCMPShortListObject)Session[Index_Sess_Obj + ActionName];
        //    }
        //    var chkApp = DB.RCMApplicants.FirstOrDefault(w => w.ApplicantID == ApplicantID);
        //    var chkLTWorkHis = DB.RCMAWorkHistories.Where(w => w.ApplicantID == ApplicantID).ToList();
        //    var chkLTEdu = DB.RCMAEdus.Where(w => w.ApplicantID == ApplicantID).ToList();
        //    var chkLTLang = DB.RCMALanguages.Where(w => w.ApplicantID == ApplicantID).ToList();
        //    var chkLTRef = DB.RCMAReferences.Where(w => w.ApplicantID == ApplicantID).ToList();

        //    BSM.ListWorkHistory = chkLTWorkHis;
        //    BSM.ListEdu = chkLTEdu;
        //    BSM.ListLang = chkLTLang;
        //    BSM.ListRef = chkLTRef;

        //    Session[Index_Sess_Obj + ActionName] = BSM;
        //    if (chkApp != null)
        //    {
        //        var result = new
        //        {
        //            MS = SYConstant.OK,
        //            fname = chkApp.FirstName,
        //            lname = chkApp.LastName,
        //            lnamekh = chkApp.OthLastName,
        //            fnamekh = chkApp.OthFirstName,
        //            ExpSalary = chkApp.ExpectSalary,
        //            DoB = chkApp.DOB,
        //        };
        //        return Json(result, JsonRequestBehavior.DenyGet);
        //    }
        //    else
        //    {
        //        var result = new { MS = SYConstant.OK };
        //        return Json(result, JsonRequestBehavior.DenyGet);
        //    }
        //}
        #endregion
    }
}
