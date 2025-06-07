using Humica.Core.DB;
using Humica.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Humica.EF.Repo;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using System.Data.Entity;
using Humica.Core.SY;
using Humica.EF;
using Humica.Core.FT;
using Humica.Logic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;

namespace Humica.Attendance
{
    public class ClsAttendanceMeal : IClsAttendanceMeal
    {
        IUnitOfWork unitOfWork;
        public string ScreenId { get; set; }
        public string EmpID { get; set; }
        public SYUser User { get; set; }
        public ATEmpSchedule Header { get; set; }
        public FTMonthlySum FMonthly { get; set; }
        public List<ATEmpSchedule> ListHeader { get; set; }
        public List<ATGenMeal> ListMeal { get; set; }
        public void OnLoad()
        {
            unitOfWork = new UnitOfWork<HumicaDBViewContext>(new HumicaDBViewContext());
        }
        public ClsAttendanceMeal()
        {
            User = SYSession.getSessionUser();
            OnLoad();
        }
        public void OnIndexLoading()
        {
            ListMeal = new List<ATGenMeal>();
            FMonthly = new FTMonthlySum();
            var period = unitOfWork.Set<PRpayperiod>().OrderByDescending(w => w.PayPeriodId).FirstOrDefault();
            if (period != null)
            {
                FMonthly.PayPeriodId = period.PayPeriodId;
                ListMeal = unitOfWork.Set<ATGenMeal>().Where(w => w.PayPeriodID == FMonthly.PayPeriodId).ToList();
            }
        }
        public void OnLoadingFilter(int PayPeriodId)
        {
            ListMeal = unitOfWork.Set<ATGenMeal>().Where(w => w.PayPeriodID == PayPeriodId).ToList();
        }
        public string GenerateMeal(int PayPeriodId)
        {
            try
            {
                Header = new ATEmpSchedule();
                ListHeader = new List<ATEmpSchedule>();

                var PeriodId = unitOfWork.Set<PRpayperiod>().FirstOrDefault(w => w.PayPeriodId == PayPeriodId);
                if (PeriodId == null) return "PERIOD";
                var _PFromDate = PeriodId.AttendanceStartDate;
                var _PToDate = PeriodId.AttendanceEndDate;
                DateTime EffecDate = _PToDate.StartDateOfMonth();
                DateTime DateForResign = _PToDate.EndDateOfMonth();
                List<HRStaffProfile> ListStaff = unitOfWork.Set<HRStaffProfile>().Where(w => w.IsMealAllowance == true && (w.Status == "A" || (w.Status == "I" && w.DateTerminate > EffecDate))).ToList();//Cuz 21-> endofmonth pay already
                var MealSetup = unitOfWork.Set<ATMealSetup>().FirstOrDefault();
                var MealSetupItem = unitOfWork.Set<ATMealSetupItem>().ToList();
                ListStaff = ListStaff.Where(w => MealSetupItem.Where(x => x.LevelCode == w.LevelCode).Any()).ToList();

                var _listShift = unitOfWork.Set<ATShift>().Where(w => w.BreakFast == true || w.Lunch == true || w.Dinner == true || w.NightMeal == true).ToList();

                var _list = unitOfWork.Set<ATEmpSchedule>().Where(w => DbFunctions.TruncateTime(w.TranDate) >= _PFromDate.Date && DbFunctions.TruncateTime(w.TranDate) <= DateForResign.Date).ToList();
                _list = _list.Where(w => _listShift.Where(x => w.SHIFT == x.Code).Any()).ToList();
                _list = _list.Where(w => ListStaff.Where(x => w.EmpCode == x.EmpCode).Any()).ToList();
                if (!_list.Any()) return "NO_DATA";
                var FromDate = PeriodId.AttendanceStartDate.AddDays(-1);
                var ToDate = PeriodId.AttendanceEndDate.AddDays(1);
                var ListInOut = (from s in unitOfWork.Set<ATInOut>()
                                 where DbFunctions.TruncateTime(s.FullDate) >= FromDate.Date
                                 && DbFunctions.TruncateTime(s.FullDate) <= ToDate.Date
                                 && s.EmpCode.Trim() != ""
                                 select s).ToList();
                ListInOut = ListInOut.Where(w => ListStaff.Where(x => x.EmpCode == w.EmpCode).Any()).ToList();

                var _GenMeal = unitOfWork.Set<ATGenMeal>().Where(w => w.PayPeriodID == PeriodId.PayPeriodId).ToList();
                if (_GenMeal.Count() > 0)
                {
                    unitOfWork.BulkDelete(_GenMeal);
                }
                List<ATGenMeal> ListMeal = new List<ATGenMeal>();
                var _Empsch = _list.ToList();
                List<ATEmpSchedule> ListUpdateDataATT = new List<ATEmpSchedule>();
                _Empsch = _Empsch.Where(w => w.LeaveDesc != "ABS").ToList();
                foreach (var item in _Empsch)
                {
                    if (item.LeaveDesc == "ABS" || item.LeaveCode == "ABS") continue;
                    DateTime PFromDate = item.TranDate;

                    var Staff = ListStaff.FirstOrDefault(w => w.EmpCode == item.EmpCode);
                    if (Staff.Status != "A" && item.TranDate >= Staff.DateTerminate) continue;
                    if (Staff.Status == "A" && item.TranDate > _PToDate) continue;
                    if (Staff.Status != "A")
                    {
                        DateTime dat = Staff.DateTerminate.AddDays(-1);
                        if ((dat > DateForResign) && item.TranDate > _PToDate) continue;
                    }
                    var MealRate = MealSetupItem.FirstOrDefault(w => w.LevelCode == w.LevelCode);
                    var CheckInOut = ListInOut.Where(w => w.FullDate.Date >= PFromDate.AddDays(-1) && w.FullDate.Date <= PFromDate.AddDays(1)).ToList();

                    Header.EmpCode = item.EmpCode;
                    var _shift = _listShift.FirstOrDefault(w => w.Code == item.SHIFT);
                    GenerateMeal(item, Staff, _shift, MealSetup, CheckInOut);
                    var Obj = new ATGenMeal();
                    Obj.BreakFast = item.BreakFast;
                    Obj.Lunch = item.Lunch;
                    Obj.Dinner = item.Dinner;
                    Obj.OTFood = item.OTFood;
                    Obj.EmpCode = Staff.EmpCode;
                    Obj.EmpName = Staff.AllName;
                    if (ListMeal.Where(w => w.EmpCode == item.EmpCode).Any())
                    {
                        ListMeal.Where(w => w.EmpCode == item.EmpCode).ToList().ForEach(w => w.BreakFast += item.BreakFast);
                        ListMeal.Where(w => w.EmpCode == item.EmpCode).ToList().ForEach(w => w.Lunch += item.Lunch);
                        ListMeal.Where(w => w.EmpCode == item.EmpCode).ToList().ForEach(w => w.Dinner += item.Dinner);
                        ListMeal.Where(w => w.EmpCode == item.EmpCode).ToList().ForEach(w => w.OTFood += item.OTFood);
                    }
                    else
                    {
                        Obj.BreakFastRate = MealRate.BreakFast;
                        Obj.LunchRate = MealRate.Lunch;
                        Obj.DinnerRate = MealRate.Dinner;
                        Obj.OTFoodRate = 2;
                        ListMeal.Add(Obj);
                    }
                    ListUpdateDataATT.Add(item);
                }
                List<ATGenMeal> ListMealADD = new List<ATGenMeal>();
                foreach (var _Meal in ListMeal)
                {
                    var Obj = new ATGenMeal();
                    Obj = _Meal;
                    Obj.PayPeriodID = PeriodId.PayPeriodId;
                    Obj.FromDate = _PFromDate;
                    Obj.ToDate = _PToDate;
                    Obj.BreakFastAM = Obj.BreakFast * Obj.BreakFastRate;
                    Obj.LunchAM = Obj.Lunch * Obj.LunchRate;
                    Obj.DinnerAM = Obj.Dinner * Obj.DinnerRate;
                    Obj.OTFoodAM = Obj.OTFood * Obj.OTFoodRate;
                    Obj.Amount = Obj.BreakFastAM + Obj.LunchAM + Obj.DinnerAM + Obj.OTFoodAM;
                    ListMealADD.Add(Obj);
                }
                if (ListUpdateDataATT.Count > 0)
                {
                    unitOfWork.BulkUpdate(ListUpdateDataATT);
                }
                if (ListMealADD.Count > 0)
                {
                    unitOfWork.BulkInsert(ListMealADD);
                }
                return SYConstant.OK;

            }
            catch (Exception e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, Header.EmpCode, SYActionBehavior.ADD.ToString(), e, true);
            }
        }
        public ATEmpSchedule GenerateMeal(ATEmpSchedule Atenn, HRStaffProfile staffProfile, ATShift Shift,
           ATMealSetup MealSetup, List<ATInOut> CheckInOut)
        {
            Atenn.BreakFast = 0;
            Atenn.Lunch = 0;
            Atenn.Dinner = 0;
            Atenn.NightMeal = 0;
            Atenn.OTFood = 0;
            if (staffProfile.IsMealAllowance == true && Shift != null && MealSetup != null)
            {
                DateTime tempDate = Atenn.TranDate;

                if (Shift.BreakFast == true && MealSetup.BreakfastFrom.HasValue && MealSetup.BreakfastTo.HasValue)
                {
                    DateTime DFrom = DateTimeHelper.SetTimeInDate(tempDate, MealSetup.BreakfastFrom.Value);
                    DateTime DTo = DateTimeHelper.SetTimeInDate(tempDate, MealSetup.BreakfastTo.Value);

                    int countMeal = CheckInOut.Where(x => x.EmpCode == Atenn.EmpCode & x.FullDate >= DFrom & x.FullDate <= DTo).Count();
                    if (countMeal > 0)
                        Atenn.BreakFast = 1;
                }
                if (Shift.Lunch == true && MealSetup.LunchFrom.HasValue && MealSetup.LunchTo.HasValue)
                {
                    DateTime DFrom = DateTimeHelper.SetTimeInDate(tempDate, MealSetup.LunchFrom.Value);
                    DateTime DTo = DateTimeHelper.SetTimeInDate(tempDate, MealSetup.LunchTo.Value);

                    int countMeal = CheckInOut.Where(x => x.EmpCode == Atenn.EmpCode & x.FullDate >= DFrom & x.FullDate <= DTo).Count();
                    if (countMeal > 0 && Atenn.Flag1 == 1 && Atenn.Flag2 == 1)
                    {
                        if (Atenn.Start1.Value.Year != 1900 && Atenn.Start2.Value.Year != 1900 && Atenn.End1.Value.Year != 1900 && Atenn.End2.Value.Year != 1900)
                            Atenn.Lunch = 1;
                    }
                    else if (countMeal > 0 && Atenn.Flag1 == 1 && Atenn.Flag2 == 2)
                    {
                        if (Atenn.Start1.Value.Year != 1900 && Atenn.End1.Value.Year != 1900)
                            Atenn.Lunch = 1;
                    }
                }
                if (Shift.Dinner == true && MealSetup.DinnerFrom.HasValue && MealSetup.DinnerTo.HasValue)
                {
                    DateTime DFrom = DateTimeHelper.SetTimeInDate(tempDate, MealSetup.DinnerFrom.Value);
                    DateTime DTo = DateTimeHelper.SetTimeInDate(tempDate, MealSetup.DinnerTo.Value);

                    int countMeal = CheckInOut.Where(x => x.EmpCode == Atenn.EmpCode & x.FullDate >= DFrom & x.FullDate <= DTo).Count();
                    if (countMeal > 0)
                        Atenn.Dinner = 1;
                }
                if (Shift.NightMeal == true && MealSetup.NightFrom.HasValue && MealSetup.NightTo.HasValue)
                {
                    DateTime DFrom = DateTimeHelper.SetTimeInDate(tempDate, MealSetup.NightFrom.Value);
                    DateTime DTo = DateTimeHelper.SetTimeInDate(tempDate, MealSetup.NightTo.Value);

                    int countMeal = CheckInOut.Where(x => x.EmpCode == Atenn.EmpCode & x.FullDate >= DFrom & x.FullDate <= DTo).Count();
                    if (countMeal > 0)
                        Atenn.NightMeal = 1;
                }
                if (Atenn.EmpCode != null && Atenn.OTApproval >= 2)
                {
                    int countMeal = CheckInOut.Where(x => x.EmpCode == Atenn.EmpCode).Count();
                    if (countMeal > 0)
                        Atenn.OTFood = 1;
                }
            }
            return Atenn;
        }
        public string TransferMeal(string ID, int payperiod)
        {
            try
            {
                var PeriodId = unitOfWork.Set<PRpayperiod>().FirstOrDefault(w => w.PayPeriodId == payperiod);
                if (PeriodId == null) return "PERIOD";
                DateTime FromDate = PeriodId.AttendanceStartDate;
                DateTime ToDate = PeriodId.AttendanceEndDate;
                DateTime tempFromDate = FromDate;
                List<PRParameter> ListParameter = unitOfWork.Set<PRParameter>().ToList();


                var ListReward = unitOfWork.Set<PR_RewardsType>().Where(w => w.ReCode == "ALLW").ToList();
                var MealSetup = unitOfWork.Set<ATMealSetup>().FirstOrDefault();
                var MealAllowanceType = ListReward.FirstOrDefault(w => w.Code == MealSetup.MealAllowanceType);
                if (MealAllowanceType == null) return "MealAllowanceType";


                var EmpAllw = unitOfWork.Set<PREmpAllw>().Where(w => w.InvoceNo == payperiod.ToString()).ToList();
                if (EmpAllw.Count() > 0)
                {
                    unitOfWork.BulkInsert(EmpAllw);
                }
                string[] _TranNo = ID.Split(',');
                var ListMealGen = unitOfWork.Set<ATGenMeal>().Where(w => w.PayPeriodID == payperiod && w.Amount > 0).ToList();
                var Tran_No = unitOfWork.Set<PREmpAllw>().OrderByDescending(u => u.TranNo).FirstOrDefault();
                int No = 1;
                List<ATGenMeal> ListMealADD = new List<ATGenMeal>();
                List<PREmpAllw> ListAllowMealADD = new List<PREmpAllw>();
                foreach (var TranNo in _TranNo)
                {

                    var _MealGen = ListMealGen.FirstOrDefault(w => w.EmpCode == TranNo);
                    if (_MealGen == null) continue;
                    _MealGen.IsTransfer = true;
                    ListMealADD.Add(_MealGen);
                    //Allowance
                    if (Tran_No != null)
                    {
                        No = Tran_No.TranNo + 1;
                    }
                    var objAllw = new PREmpAllw();
                    objAllw.EmpCode = _MealGen.EmpCode;
                    objAllw.EmpName = _MealGen.EmpName;
                    objAllw.Amount = _MealGen.Amount;
                    objAllw.Status = 0;
                    objAllw.FromDate = FromDate;
                    objAllw.ToDate = ToDate;
                    objAllw.AllwCode = MealAllowanceType.Code;
                    objAllw.AllwDescription = MealAllowanceType.Description;
                    objAllw.Remark = "Transfer from Attendance";
                    objAllw.CreateBy = User.UserName;
                    objAllw.CreateOn = DateTime.Now;
                    objAllw.InvoceNo = payperiod.ToString();
                    objAllw.TranNo = No;
                    ListAllowMealADD.Add(objAllw);
                }
                if (ListAllowMealADD.Count > 0)
                {
                    unitOfWork.BulkUpdate(ListMealADD);
                    unitOfWork.BulkInsert(ListAllowMealADD);
                }
                return SYConstant.OK;
            }
            catch (Exception e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, payperiod.ToString(), SYActionBehavior.ADD.ToString(), e, true);
            }
        }
        public Dictionary<string, dynamic> OnDataSelector(params object[] keys)
        {
            Dictionary<string, dynamic> keyValues = new Dictionary<string, dynamic>();

            var ListBranch = SYConstant.getBranchDataAccess();
            keyValues.Add("POSITION_SELECT", ClsFilter.LoadPosition());
            keyValues.Add("BRANCHES_SELECT", ListBranch);
            keyValues.Add("DIVISION_SELECT", ClsFilter.LoadDivision());
            keyValues.Add("DEPARTMENT_SELECT", ClsFilter.LoadDepartment());
            keyValues.Add("LOCATION_SELECT", unitOfWork.Set<HRLocation>().ToList());
            keyValues.Add("PERIOD_SELECT", unitOfWork.Set<PRpayperiod>().ToList().OrderByDescending(w => w.PayPeriodId));

            return keyValues;
        }
    }
}