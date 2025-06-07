using Humica.Core.DB;
using Humica.Core.FT;
using Humica.EF;
using Humica.EF.Models.SY;
using Humica.Logic;
using Humica.Logic.Att;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Humica.Controllers.Attendance.Attendance
{
    public class ATGenerateMealController : Humica.EF.Controllers.MasterSaleController
    {
        private const string SCREEN_ID = "ATM0000011";
        private const string URL_SCREEN = "/Attendance/Attendance/ATGenerateMeal/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName = "";
        private string KeyName = "TranNo";
        HumicaDBContext DB = new HumicaDBContext();
        HumicaDBViewContext DBV = new HumicaDBViewContext();
        public ATGenerateMealController()
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

            ATEmpSchObject BSM = new ATEmpSchObject();
            BSM.ListMeal = new List<ATGenMeal>();
            BSM.FMonthly = new FTMonthlySum();
            //BSM.MealGen = new ATGenMeal();
            var period = DB.PRpayperiods.OrderByDescending(w => w.PayPeriodId).FirstOrDefault();
            if (period != null)
            {
                BSM.FMonthly.PayPeriodId = period.PayPeriodId;
                BSM.ListMeal = DB.ATGenMeals.Where(w => w.PayPeriodID == BSM.FMonthly.PayPeriodId).ToList();
            }
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;

            return View(BSM);
        }
        [HttpPost]
        public ActionResult Index(ATEmpSchObject BSM)
        {
            ActionName = "Index";
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);

            var ListMeal = DB.ATGenMeals.Where(w => w.PayPeriodID == BSM.FMonthly.PayPeriodId).ToList();
            BSM.ListMeal = ListMeal.OrderBy(x => x.EmpCode).ToList();

            Session[Index_Sess_Obj + ActionName] = BSM;

            return View(BSM);
        }
        #endregion
        #region 'Generate'
        public async Task<ActionResult> Generate(int PayPeriodId)
        {
            ActionName = "Index";
            UserSession();
            UserConfForm(SYActionBehavior.EDIT);
            DataSelector();
            var BSM = new ATEmpSchObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
            }
            var msg = await BSM.GenerateMeal(BSM.FMonthly.PayPeriodId);
            if (msg == SYConstant.OK)
            {
                Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("GENERATER_COMPLATED", user.Lang);
            }
            else
            {
                Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
            }
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        public ActionResult Transfer(string EmpCode)
        {
            ActionName = "Index";
            UserSession();
            UserConfForm(SYActionBehavior.EDIT);
            DataSelector();
            var BSM = new ATEmpSchObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
            }
            var Period = DB.PRpayperiods.FirstOrDefault(w => w.PayPeriodId == BSM.FMonthly.PayPeriodId);
            if (Period != null)
            {
                var msg = BSM.TransferMeal(EmpCode, BSM.FMonthly.PayPeriodId);
                if (msg == SYConstant.OK)
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("TANSFER_COMPLATED", user.Lang);
                }
                else
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                }
            }
            else
            {
                Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("INV_DOC", user.Lang);
            }
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        #endregion
        #region 'GridItem'
        public ActionResult GridItems()
        {
            ActionName = "Index";
            UserSession();
            UserConfForm(ActionBehavior.ACC_REV);
            ATEmpSchObject BSM = new ATEmpSchObject();
            BSM.ScreenId = SCREEN_ID;

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
            }
            DataSelector();
            Session[SYConstant.CURRENT_URL] = URL_SCREEN + "GridItems";
            return PartialView("GridItems", BSM.ListMeal);
        }
        #endregion
        #region 'Private Code'
        [HttpPost]
        public string getPeriod(int Period, string Action)
        {
            this.ActionName = Action;
            UserSession();
            UserConfListAndForm();

            ATEmpSchObject BSM = new ATEmpSchObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
                BSM.Filter.PayPeriodId = Period;
                Session[Index_Sess_Obj + ActionName] = BSM;
                return SYConstant.OK;
            }
            else
            {
                return SYMessages.getMessage("PLEASE_SEARCH_ALLOWANCE");
            }
        }
        [HttpPost]
        public string getBranch(string Branch, string Action)
        {
            this.ActionName = Action;
            UserSession();
            UserConfListAndForm();

            ATEmpSchObject BSM = new ATEmpSchObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
                BSM.Filter.Branch = Branch;
                BSM.LIstEmplSch = new List<ListEmpSch>();

                var dtHeader = DB.HRStaffProfiles.ToList();
                var ListBranch = SYConstant.getBranchDataAccess();
                dtHeader = dtHeader.Where(w => ListBranch.Where(x => x.Code == w.Branch).Any()).ToList();

                var Pos = DB.HRPositions.ToList();

                if (BSM.Filter.Branch != null && BSM.Filter.Branch != "")
                {
                    dtHeader = dtHeader.Where(w => w.Branch == BSM.Filter.Branch).ToList();
                }
                if (BSM.Filter.Locations != null && BSM.Filter.Locations != "")
                {
                    dtHeader = dtHeader.Where(w => w.LOCT == BSM.Filter.Locations).ToList();
                }
                if (BSM.Filter.Division != null && BSM.Filter.Division != "")
                {
                    dtHeader = dtHeader.Where(w => w.Division == BSM.Filter.Division).ToList();
                }
                if (BSM.Filter.Department != null && BSM.Filter.Department != "")
                {
                    dtHeader = dtHeader.Where(w => w.DEPT == BSM.Filter.Department).ToList();
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
                return SYConstant.OK;
            }
            else
            {
                return SYMessages.getMessage("PLEASE_SEARCH_ALLOWANCE");
            }
        }
        [HttpPost]
        public string getLocation(string Location, string Action)
        {
            this.ActionName = Action;
            UserSession();
            UserConfListAndForm();

            ATEmpSchObject BSM = new ATEmpSchObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
                BSM.Filter.Locations = Location;
                BSM.LIstEmplSch = new List<ListEmpSch>();

                var dtHeader = DB.HRStaffProfiles.ToList();

                var Pos = DB.HRPositions.ToList();
                var ListBranch = SYConstant.getBranchDataAccess();
                dtHeader = dtHeader.Where(w => ListBranch.Where(x => x.Code == w.Branch).Any()).ToList();
                if (BSM.Filter.Branch != null && BSM.Filter.Branch != "")
                {
                    dtHeader = dtHeader.Where(w => w.Branch == BSM.Filter.Branch).ToList();
                }
                if (BSM.Filter.Locations != null && BSM.Filter.Locations != "")
                {
                    dtHeader = dtHeader.Where(w => w.LOCT == BSM.Filter.Locations).ToList();
                }
                if (BSM.Filter.Division != null && BSM.Filter.Division != "")
                {
                    dtHeader = dtHeader.Where(w => w.Division == BSM.Filter.Division).ToList();
                }
                if (BSM.Filter.Department != null && BSM.Filter.Department != "")
                {
                    dtHeader = dtHeader.Where(w => w.DEPT == BSM.Filter.Department).ToList();
                }

                Session[Index_Sess_Obj + ActionName] = BSM;
                return SYConstant.OK;
            }
            else
            {
                return SYMessages.getMessage("PLEASE_SEARCH_ALLOWANCE");
            }
        }
        [HttpPost]
        public string getDivision(string Division, string Action)
        {
            this.ActionName = Action;
            UserSession();
            UserConfListAndForm();

            ATEmpSchObject BSM = new ATEmpSchObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
                BSM.Filter.Division = Division;

                BSM.LIstEmplSch = new List<ListEmpSch>();

                var dtHeader = DB.HRStaffProfiles.ToList();
                var ListBranch = SYConstant.getBranchDataAccess();
                dtHeader = dtHeader.Where(w => ListBranch.Where(x => x.Code == w.Branch).Any()).ToList();

                var Pos = DB.HRPositions.ToList();

                if (BSM.Filter.Branch != null && BSM.Filter.Branch != "")
                {
                    dtHeader = dtHeader.Where(w => w.Branch == BSM.Filter.Branch).ToList();
                }
                if (BSM.Filter.Locations != null && BSM.Filter.Locations != "")
                {
                    dtHeader = dtHeader.Where(w => w.LOCT == BSM.Filter.Locations).ToList();
                }
                if (BSM.Filter.Division != null && BSM.Filter.Division != "")
                {
                    dtHeader = dtHeader.Where(w => w.Division == BSM.Filter.Division).ToList();
                }
                if (BSM.Filter.Department != null && BSM.Filter.Department != "")
                {
                    dtHeader = dtHeader.Where(w => w.DEPT == BSM.Filter.Department).ToList();
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
                return SYConstant.OK;
            }
            else
            {
                return SYMessages.getMessage("PLEASE_SEARCH_ALLOWANCE");
            }
        }
        [HttpPost]
        public string getDepartment(string Department, string Action)
        {
            this.ActionName = Action;
            UserSession();
            UserConfListAndForm();

            ATEmpSchObject BSM = new ATEmpSchObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
                BSM.Filter.Department = Department;

                BSM.LIstEmplSch = new List<ListEmpSch>();

                var dtHeader = DB.HRStaffProfiles.ToList();

                var Pos = DB.HRPositions.ToList();
                var ListBranch = SYConstant.getBranchDataAccess();
                dtHeader = dtHeader.Where(w => ListBranch.Where(x => x.Code == w.Branch).Any()).ToList();

                if (BSM.Filter.Branch != null && BSM.Filter.Branch != "")
                {
                    dtHeader = dtHeader.Where(w => w.Branch == BSM.Filter.Branch).ToList();
                }
                if (BSM.Filter.Locations != null && BSM.Filter.Locations != "")
                {
                    dtHeader = dtHeader.Where(w => w.LOCT == BSM.Filter.Locations).ToList();
                }
                if (BSM.Filter.Division != null && BSM.Filter.Division != "")
                {
                    dtHeader = dtHeader.Where(w => w.Division == BSM.Filter.Division).ToList();
                }
                if (BSM.Filter.Department != null && BSM.Filter.Department != "")
                {
                    dtHeader = dtHeader.Where(w => w.DEPT == BSM.Filter.Department).ToList();
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
                return SYConstant.OK;
            }
            else
            {
                return SYMessages.getMessage("PLEASE_SEARCH_ALLOWANCE");
            }
        }
        [HttpPost]
        public string getEmpCode(string EmpCode, string Action)
        {
            this.ActionName = Action;
            UserSession();
            UserConfListAndForm();

            ATEmpSchObject BSM = new ATEmpSchObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];

                BSM.EmpID = EmpCode;
                Session[Index_Sess_Obj + ActionName] = BSM;
                return SYConstant.OK;
            }
            else
            {
                return SYMessages.getMessage("PLEASE_SEARCH_ALLOWANCE");
            }
        }
        private void DataSelector()
        {
            var ListBranch = SYConstant.getBranchDataAccess();
            ViewData["POSITION_SELECT"] = ClsFilter.LoadPosition();
            ViewData["BRANCHES_SELECT"] = ListBranch;
            ViewData["DIVISION_SELECT"] = ClsFilter.LoadDivision();
            ViewData["DEPARTMENT_SELECT"] = ClsFilter.LoadDepartment();
            ViewData["LOCATION_SELECT"] = DB.HRLocations.ToList().OrderBy(w => w.Description);
            ViewData["PERIOD_SELECT"] = DB.PRpayperiods.ToList().OrderByDescending(w => w.PayPeriodId);
        }
        #endregion
    }
}
