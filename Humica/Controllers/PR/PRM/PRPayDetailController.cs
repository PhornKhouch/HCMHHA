using Humica.Core.DB;
using Humica.EF;
using Humica.EF.Models.SY;
using Humica.Logic.PR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace HR.Controllers.PR.PRM
{

    public class PRPayDetailController : Humica.EF.Controllers.MasterSaleController
    {

        private const string SCREEN_ID = "PRM0000007";
        private const string URL_SCREEN = "/PR/PRM/PRPayDetail/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName = "";
        private string KeyName = "ID";
        HumicaDBContext DB = new HumicaDBContext();
        HumicaDBViewContext DBV = new HumicaDBViewContext();
        public PRPayDetailController()
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

            PRGenerate_Salary BSM = new PRGenerate_Salary();
            BSM.HeaderSalary = new HISGenSalary();
            BSM.Filter = new Humica.Core.FT.FTFilterEmployee();
            BSM.ListEmployeeGen = new List<HR_View_EmpGenSalary>();
            BSM.Filter.InMonth = DateTime.Now;
            BSM.Filter.FromDate = new DateTime(2019, 10, 1);
            BSM.Filter.ToDate = new DateTime(2019, 10, 30);

            BSM.ListBasicSalary = new List<HISGenSalaryD>();
            BSM.ListOverTime = new List<HISGenOTHour>();
            BSM.ListAllowance = new List<HISGenAllowance>();
            BSM.ListBonus = new List<HISGenBonu>();
            BSM.ListDeduction = new List<HISGenDeduction>();
            BSM.ListLeaveDed = new List<LeaveDeduction>();
            BSM.ListPaySlip = new List<HISPaySlip>();
            BSM.ListGLCharge = new List<PR_GLCharge_View>();
            BSM.ListCostCharge = new List<HisCostCharge>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                var obj = (PRGenerate_Salary)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;

            return View(BSM);
        }
        [HttpPost]
        public ActionResult Index(PRGenerate_Salary BSM)
        {
            DataSelector();
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            BSM.ListEmployeeGen = BSM.LoadDataEmpGen(BSM.Filter, SYConstant.getBranchDataAccess());

            Session[Index_Sess_Obj + ActionName] = BSM;

            return View(BSM);
        }
        #endregion

        public ActionResult Delete(string EmpCode, DateTime INMonth)
        {
            UserSession();
            UserConfForm(SYActionBehavior.DELETE);
            DataSelector();
            if (EmpCode != null)
            {
                PRGenerate_Salary GLA = new PRGenerate_Salary();
                string msg = GLA.Delete_PayRecord(EmpCode, INMonth);
                if (msg == SYConstant.OK)
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_RM", user.Lang);
                }
                else
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                }
            }
            else
            {
                Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            }
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

        }
        public ActionResult _BasicSalary()
        {
            ActionName = "Index";
            UserSession();
            DataSelector();
            UserConfForm(ActionBehavior.ACC_REV);
            PRGenerate_Salary BSM = new PRGenerate_Salary();
            BSM.ScreenId = SCREEN_ID;
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (PRGenerate_Salary)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            Session[SYConstant.CURRENT_URL] = URL_SCREEN + "_BasicSalary";
            return PartialView("_BasicSalary", BSM.ListBasicSalary);
        }

        public ActionResult _Allowance()
        {
            ActionName = "Index";
            UserSession();
            DataSelector();
            UserConfForm(ActionBehavior.ACC_REV);
            PRGenerate_Salary BSM = new PRGenerate_Salary();
            BSM.ScreenId = SCREEN_ID;
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (PRGenerate_Salary)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            Session[SYConstant.CURRENT_URL] = URL_SCREEN + "_Allowance";
            return PartialView("_Allowance", BSM.ListAllowance);
        }
        public ActionResult _Bonus()
        {
            ActionName = "Index";
            UserSession();
            DataSelector();
            UserConfForm(ActionBehavior.ACC_REV);
            PRGenerate_Salary BSM = new PRGenerate_Salary();
            BSM.ScreenId = SCREEN_ID;
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (PRGenerate_Salary)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            Session[SYConstant.CURRENT_URL] = URL_SCREEN + "_Bonus";
            return PartialView("_Bonus", BSM.ListBonus);
        }
        public ActionResult _OverTime()
        {
            ActionName = "Index";
            UserSession();
            DataSelector();
            UserConfForm(ActionBehavior.ACC_REV);
            PRGenerate_Salary BSM = new PRGenerate_Salary();
            BSM.ScreenId = SCREEN_ID;
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (PRGenerate_Salary)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            Session[SYConstant.CURRENT_URL] = URL_SCREEN + "_OverTime";
            return PartialView("_OverTime", BSM.ListOverTime);
        }
        public ActionResult _Deduction()
        {
            ActionName = "Index";
            UserSession();
            DataSelector();
            UserConfForm(ActionBehavior.ACC_REV);
            PRGenerate_Salary BSM = new PRGenerate_Salary();
            BSM.ScreenId = SCREEN_ID;
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (PRGenerate_Salary)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            Session[SYConstant.CURRENT_URL] = URL_SCREEN + "_Deduction";
            return PartialView("_Deduction", BSM.ListDeduction);
        }
        public ActionResult _LeaveDed()
        {
            ActionName = "Index";
            UserSession();
            DataSelector();
            UserConfForm(ActionBehavior.ACC_REV);
            PRGenerate_Salary BSM = new PRGenerate_Salary();
            BSM.ScreenId = SCREEN_ID;
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (PRGenerate_Salary)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            Session[SYConstant.CURRENT_URL] = URL_SCREEN + "_LeaveDed";
            return PartialView("_LeaveDed", BSM.ListLeaveDed);
        }
        public ActionResult _GLCharge()
        {
            ActionName = "Index";
            UserSession();
            DataSelector();
            UserConfForm(ActionBehavior.ACC_REV);
            PRGenerate_Salary BSM = new PRGenerate_Salary();
            BSM.ScreenId = SCREEN_ID;
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (PRGenerate_Salary)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            Session[SYConstant.CURRENT_URL] = URL_SCREEN + "_GLCharge";
            return PartialView("_GLCharge", BSM.ListGLCharge);
        }
        public ActionResult _CostCharge()
        {
            ActionName = "Index";
            UserSession();
            DataSelector();
            UserConfForm(ActionBehavior.ACC_REV);
            PRGenerate_Salary BSM = new PRGenerate_Salary();
            BSM.ScreenId = SCREEN_ID;
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (PRGenerate_Salary)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            Session[SYConstant.CURRENT_URL] = URL_SCREEN + "_CostCharge";
            return PartialView("_CostCharge", BSM.ListCostCharge);
        }
        public ActionResult _PaySlip()
        {
            ActionName = "Index";
            UserSession();
            DataSelector();
            UserConfForm(ActionBehavior.ACC_REV);
            PRGenerate_Salary BSM = new PRGenerate_Salary();
            BSM.ScreenId = SCREEN_ID;
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (PRGenerate_Salary)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            Session[SYConstant.CURRENT_URL] = URL_SCREEN + "_PaySlip";
            return PartialView("_PaySlip", BSM.ListPaySlip);
        }
        public ActionResult ShowData(string ID, string EmpCode, DateTime InMonth)
        {

            ActionName = "Index";
            var Stafff = DBV.HR_STAFF_VIEW;
            HR_STAFF_VIEW EmpStaff = Stafff.FirstOrDefault(w => w.EmpCode == EmpCode);
            HISGenSalary GenSalary = DB.HISGenSalaries.FirstOrDefault(w => w.EmpCode == EmpCode && w.INYear == InMonth.Year && w.INMonth == InMonth.Month);
            if (EmpStaff != null)
            {
                if (GenSalary != null)
                {
                    var result = new
                    {
                        MS = SYConstant.OK,
                        AllName = EmpStaff.AllName,
                        EmpType = EmpStaff.EmpType,
                        Division = EmpStaff.DivisionDesc,
                        DEPT = EmpStaff.Department,
                        SECT = EmpStaff.Section,
                        LevelCode = EmpStaff.Level,
                        Position = EmpStaff.Position,
                        StartDate = EmpStaff.StartDate,
                        Salary = GenSalary.Salary,
                        Spouse = GenSalary.UTAXSP,
                        Child = GenSalary.UTAXCH,
                        Tax = GenSalary.TAXAM,
                        AmToBeTax = GenSalary.AMTOBETAX,
                        FTax = GenSalary.AMFRINGTAX,
                        AmToBeFTax = GenSalary.FRINGAM,
                        GrossPay = GenSalary.GrossPay,
                        NetPay = GenSalary.NetWage,
                        EmpPensionFunRate = GenSalary.StaffPensionFundRate,
                        EmpPensionFunAmount = GenSalary.StaffPensionFundAmount,
                        ComPensionFunRate = GenSalary.CompanyPensionFundRate,
                        ComPensionFunAmount = GenSalary.CompanyPensionFundAmount,
                        SeniorityTaxable = GenSalary.SeniorityTaxable,
                        FirstPaymentAmount = GenSalary.FirstPaymentAmount
                    };
                    GetData(EmpCode, "Index", InMonth);
                    return Json(result, JsonRequestBehavior.DenyGet);
                }
                else
                {
                    var result = new
                    {
                        MS = SYConstant.OK,
                        AllName = EmpStaff.AllName,
                        EmpType = EmpStaff.EmpType,
                        Division = EmpStaff.Division,
                        DEPT = EmpStaff.Department,
                        SECT = EmpStaff.Section,
                        LevelCode = EmpStaff.Level,
                        Position = EmpStaff.Position,
                        StartDate = EmpStaff.StartDate,
                        Salary = 0,
                        Spouse = 0,
                        Child = 0,
                        Tax = 0,
                        AmToBeTax = 0,
                        FTax = 0,
                        AmToBeFTax = 0,
                        GrossPay = 0,
                        NetPay = 0,
                        EmpPensionFunRate = 0,
                        EmpPensionFunAmount = 0,
                        ComPensionFunRate = 0,
                        ComPensionFunAmount = 0,
                        SeniorityTaxable = 0,
                        Ded = 0
                    };
                    GetData(EmpCode, "Index", InMonth);
                    return Json(result, JsonRequestBehavior.DenyGet);
                }

            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        public string GetData(string EmpCode, string Action, DateTime InMonth)
        {
            this.ActionName = Action;
            UserSession();
            UserConfListAndForm();

            PRGenerate_Salary BSM = new PRGenerate_Salary();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (PRGenerate_Salary)Session[Index_Sess_Obj + ActionName];
                BSM.ListPaySlip = new List<HISPaySlip>();
                BSM.ListLeaveDed = new List<LeaveDeduction>();
                BSM.ListBasicSalary = DB.HISGenSalaryDs.Where(w => w.EmpCode == EmpCode && w.INYear == InMonth.Year && w.INMonth == InMonth.Month).ToList();
                BSM.ListOverTime = DB.HISGenOTHours.Where(w => w.EmpCode == EmpCode && w.INYear == InMonth.Year && w.INMonth == InMonth.Month).ToList();
                BSM.ListAllowance = DB.HISGenAllowances.Where(w => w.EmpCode == EmpCode && w.INYear == InMonth.Year && w.INMonth == InMonth.Month).ToList();
                BSM.ListBonus = DB.HISGenBonus.Where(w => w.EmpCode == EmpCode && w.INYear == InMonth.Year && w.INMonth == InMonth.Month).ToList();
                BSM.ListDeduction = DB.HISGenDeductions.Where(w => w.EmpCode == EmpCode && w.INYear == InMonth.Year && w.INMonth == InMonth.Month).ToList();
                BSM.ListGLCharge = DBV.PR_GLCharge_View.Where(w => w.EmpCode == EmpCode && w.InYear == InMonth.Year && w.InMonth == InMonth.Month).ToList();
                if (BSM.ListGLCharge.Count() > 0)
                {
                    BSM.ListGLCharge = BSM.ListGLCharge.OrderBy(w => w.SortKey).ToList();
                }
                BSM.ListCostCharge = DB.HisCostCharges.Where(w => w.EmpCode == EmpCode && w.InYear == InMonth.Year && w.InMonth == InMonth.Month).ToList();
                var LeaveDed = DB.HISGenLeaveDeducts.Where(w => w.EmpCode == EmpCode && w.INYear == InMonth.Year && w.INMonth == InMonth.Month).ToList();
                var result = (from Leave in LeaveDed
                              group Leave by new { Leave.LeaveCode, Leave.LeaveDesc, Leave.Measure, Leave.Rate }
                  into myGroup
                              where myGroup.Count() > 0
                              select new
                              {
                                  myGroup.Key.LeaveCode,
                                  myGroup.Key.LeaveDesc,
                                  FromDate = myGroup.Min(w => w.LeaveDate),
                                  ToDate = myGroup.Max(w => w.LeaveDate),
                                  DayLeave = myGroup.Sum(w => w.Qty),
                                  myGroup.Key.Rate,
                                  Amount = myGroup.Sum(w => w.Qty) * myGroup.Key.Rate
                              }).ToList();
                foreach (var item in result)
                {
                    var res = new LeaveDeduction()
                    {
                        LeaveCode = item.LeaveCode,
                        LeaveDescription = item.LeaveDesc,
                        FromDate = item.FromDate.Value,
                        ToDate = item.ToDate.Value,
                        DayLeave = Convert.ToDecimal(item.DayLeave.Value),
                        Rate = Convert.ToDecimal(item.Rate),
                        Amount = Convert.ToDecimal(item.Amount)
                    };
                    BSM.ListLeaveDed.Add(res);
                }
                var ListPay = DB.HISPaySlips.Where(w => w.EmpCode == EmpCode && w.INYear == InMonth.Year && w.INMonth == InMonth.Month).ToList();
                foreach (var item in ListPay)
                {
                    var Pay = new HISPaySlip();
                    Pay.TranLine = item.TranLine;
                    Pay.EmpCode = item.EmpCode;
                    if (item.EarnDesc != "EARNING")
                    {
                        Pay.EarnDesc = item.EarnDesc;
                        Pay.EAmount = item.EAmount;
                    }
                    if (item.DeductDesc != "DEDUCTIONS")
                    {
                        Pay.DeductDesc = item.DeductDesc;
                        Pay.DeductAmount = item.DeductAmount;
                    }
                    BSM.ListPaySlip.Add(Pay);
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
                return SYConstant.OK;
            }
            else
            {
                return SYMessages.getMessage("PLEASE_SEARCH_Employee");
            }
        }
        private void DataSelector()
        {
            var Staff = DBV.HR_STAFF_VIEW.ToList();
            var _Branch = SYConstant.getBranchDataAccess();
            Staff = Staff.Where(x => _Branch.Where(w => w.Code == x.BranchID).Any()).ToList();
            ViewData["STAFF_SELECT"] = Staff;
        }

    }
}
