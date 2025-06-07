using Humica.Core.DB;
using Humica.Core.FT;
using Humica.EF;
using Humica.EF.Models.SY;
using Humica.Logic;
using Humica.Logic.Att;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Humica.Controllers.Attendance.Attendance
{
    public class ATEmployeeOTController : Humica.EF.Controllers.MasterSaleController
    {

        private const string SCREEN_ID = "ATM0000012";
        private const string URL_SCREEN = "/Attendance/Attendance/ATEmployeeOT/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName = "";
        private string KeyName = "TranNo";
        HumicaDBContext DB = new HumicaDBContext();
        HumicaDBViewContext DBV = new HumicaDBViewContext();
        public ATEmployeeOTController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
        }

        #region "List"
        public ActionResult Index()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            DataSelector();

            ATEmpSchObject BSM = new ATEmpSchObject();
            FTFilterEmployee Filter = new FTFilterEmployee();
            BSM.ListEmpSchdule = new List<VIEW_ATEmpSchedule>();
            BSM.ListEmpSchduleDetail = new List<VIEW_ATEmpSchedule>();
            BSM.FMonthly = new Humica.Core.FT.FTMonthlySum();
            DateTime DNow = DateTime.Now;
            BSM.FMonthly.FromDate = new DateTime(DNow.Year, DNow.Month, 1);
            BSM.FMonthly.ToDate = new DateTime(DNow.Year, DNow.Month, DateTime.DaysInMonth(DNow.Year, DNow.Month));
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                var obj = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;

            return View(BSM);
        }
        [HttpPost]
        public async Task<ActionResult> Index(ATEmpSchObject BSM)
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            DataSelector();
            var payperiord = DB.PRpayperiods.FirstOrDefault(w=>w.PayPeriodId == BSM.FMonthly.PayPeriodId);  
            var ListEmpSchdule = await DBV.VIEW_ATEmpSchedule.Where(w => w.TranDate >= payperiord.AttendanceStartDate
            && w.TranDate <= payperiord.AttendanceEndDate && w.OTApproval != 0 ).ToListAsync();
            var listBranch = SYConstant.getBranchDataAccess();

            if (BSM.FMonthly.Branch != null)
            {
                string[] Branch = BSM.FMonthly.Branch.Split(',');
                List<string> LstBranch = new List<string>();
                foreach (var read in Branch)
                {
                    if (read.Trim() != "")
                    {
                        LstBranch.Add(read.Trim());
                    }
                }

                ListEmpSchdule = ListEmpSchdule.Where(w => LstBranch.Contains(w.Branch)).ToList();
            }

            if (BSM.FMonthly.Department != null)
                ListEmpSchdule = ListEmpSchdule.Where(w => w.DEPT == BSM.FMonthly.Department).ToList();
            if (BSM.FMonthly.Location != null)
                ListEmpSchdule = ListEmpSchdule.Where(w => w.LOCT == BSM.FMonthly.Location).ToList();
            if (BSM.FMonthly.Division != null)
                ListEmpSchdule = ListEmpSchdule.Where(w => w.DivisionCode == BSM.FMonthly.Division).ToList();
            if (BSM.FMonthly.EmpCode != null)
                ListEmpSchdule = ListEmpSchdule.Where(w => w.EmpCode == BSM.FMonthly.EmpCode).ToList();
            BSM.ListEmpSchdule = ListEmpSchdule.OrderBy(x => x.TranDate).ToList();
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }

        #endregion
        public ActionResult TransferOT(string TranNo)
        {
            ActionName = "Index";
            UserSession();
            UserConfForm(SYActionBehavior.EDIT);
            DataSelector();
            var BSM = new ATEmpSchObject();
            if (TranNo != null)
            {
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
                }
                var msg = BSM.TransferOT(TranNo, BSM.ListEmpSchdule);

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
            return PartialView("GridItems", BSM.ListEmpSchdule);
        }
        public ActionResult ShowData(string ID, string EmpCode)
        {

            ActionName = "Details";
            var Stafff = DBV.HR_STAFF_VIEW;
            HR_STAFF_VIEW EmpStaff = Stafff.FirstOrDefault(w => w.EmpCode == EmpCode);
            if (EmpStaff != null)
            {
                var result = new
                {
                    MS = SYConstant.OK,
                    AllName = EmpStaff.AllName,
                    EmpType = EmpStaff.EmpType,
                    Division = EmpStaff.Division,
                    DEPT = EmpStaff.Department,
                    SECT = EmpStaff.Section,
                    LevelCode = EmpStaff.LevelCode,
                    Position = EmpStaff.Position,
                    StartDate = EmpStaff.StartDate
                };

                return Json(result, JsonRequestBehavior.DenyGet);

            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
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
    }
}
