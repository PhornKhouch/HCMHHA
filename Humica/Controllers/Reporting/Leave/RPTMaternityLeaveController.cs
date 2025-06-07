using Humica.Core.DB;
using Humica.EF;
using Humica.EF.Models.SY;
using Humica.Logic;
using Humica.Logic.Inquiry;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Humica.Controllers.Reporting
{
    public class RPTMaternityLeaveController : Humica.EF.Controllers.MasterSaleController

    {
        private const string SCREEN_ID = "RPTLM00003";
        private const string URL_SCREEN = "/Reporting/RPTMaternityLeave/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName = "";
        HumicaDBContext DB = new HumicaDBContext();

        public RPTMaternityLeaveController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
        }
        public async Task< ActionResult> Index()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm();
            ClsInqLeave BSM = new ClsInqLeave();
            BSM.Filter = new Humica.Core.FT.FTFilterEmployee();
            DateTime date = DateTime.Now;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            BSM.Filter.FromDate = firstDayOfMonth;
            BSM.Filter.ToDate = lastDayOfMonth;
            await BSM.SetDataReportHRPayHistoryFilter();

            UserConfReportPartial(BSM.Filter.STReportType, ControllerContext.RouteData.Values["action"].ToString() + "Partail", BSM.ScreenID);

            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        [HttpPost]
        public async Task<ActionResult> Index(ClsInqLeave BSM)
        {
            UserSession();
            UserConfListAndForm();

            ActionName = "Index";
            await BSM.SetDataReportHRPayHistoryFilter();

            UserConfReportPartial(BSM.Filter.STReportType, ControllerContext.RouteData.Values["action"].ToString() + "Partail", BSM.ScreenID);

            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        public ActionResult PivotLeave()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm();
            ClsInqLeave BSM = new ClsInqLeave();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ClsInqLeave)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("PivotLeave", BSM);
        }

        private void DataList()
        {
            SYDataList objList = new SYDataList("STATUS_EMPLOYEE");
            ViewData["STATUS_EMPLOYEE"] = objList.ListData;
            ViewData["DEPARTMENT_SELECT"] = ClsFilter.LoadDepartment();
            ViewData["BRANCHES_SELECT"] = SYConstant.getBranchDataAccess();
            ViewData["COMPANY_SELECT"] = SYConstant.getCompanyDataAccess();
            ViewData["SECTION_SELECT"] = ClsFilter.LoadSection();
            ViewData["POSITION_SELECT"] = ClsFilter.LoadPosition();
            ViewData["DIVISION_SELECT"] = ClsFilter.LoadDivision();
            ViewData["LEVEL_SELECT"] = SYConstant.getLevelDataAccess();
            ViewData["LEAVE_TYPE_SELECT"] = DB.HRLeaveTypes.ToList();
        }
    }
}