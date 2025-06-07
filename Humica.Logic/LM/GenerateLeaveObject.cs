using Humica.Core;
using Humica.Core.DB;
using Humica.Core.FT;
using Humica.Core.Helper;
using Humica.Core.SY;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic.PR;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Humica.Logic.LM
{
    public class GenerateLeaveObject
    {
        public HumicaDBContext DB = new HumicaDBContext();
        public SMSystemEntity DP = new SMSystemEntity();
        public HumicaDBViewContext DBV = new HumicaDBViewContext();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public string ScreenId { get; set; }
        public string MessageError { get; set; }
        public HREmpLeave HeaderEmpLeave { get; set; }
        public HREmpEditLeaveEntitle HeaderEditEntitle { get; set; }
        public HR_STAFF_VIEW HeaderStaff { get; set; }
        public FTFilterData Filter { get; set; }
        public FTINYear FInYear { get; set; }
        public HREmpLeaveB EmpLeaveB { get; set; }
        public List<HREmpLeaveB> ListEmpLeaveB { get; set; }
        public List<HREmpEditLeaveEntitle> ListEmpEditLeaveEntitle { get; set; }
        public List<HREmpLeave> ListEmpLeave { get; set; }
        public List<HREmpLeaveD> ListEmpLeaveD { get; set; }
        public List<Employee_Generate_Leave> ListEmpGen { get; set; }
        public HRApproverLeave ApprovalWorkFlow { get; set; }
        public List<HRApproverLeave> ListApproverLeave { get; set; }
        public List<Employee_ListForwardLeave> ListForward { get; set; }
        public List<HR_VIEW_EmpLeave> ListEmpLeaveReq { get; set; }
        public List<HR_VIEW_EmpLeave> ListEmpLeavePending { get; set; }
        public List<ExDocApproval> ListApproval { get; set; }
        public List<HR_STAFF_VIEW> ListStaff { get; set; }
        public string EmpID { get; set; }
        public string Units { get; set; }
        public GenerateLeaveObject()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }
        public static List<HRLeaveType> GetLeaveType(List<HRLeaveType> ListLeaveType, string TemLeave, string Sex, int WorkMonth, bool IsProcess = false)
        {
            List<HRLeaveType> Temp = ListLeaveType.Where(w => w.IsParent == true).ToList();
            var DBI = new HumicaDBContext();
            List<HRSetEntitleD> ListTemp = DBI.HRSetEntitleDs.Where(w => w.CodeH == TemLeave && w.FromMonth <= WorkMonth).ToList();
            //List<HRSetEntitleD> ListTemp = DBI.HRSetEntitleDs.Where(w => w.CodeH == TemLeave).ToList();
            ListLeaveType = ListLeaveType.Where(w => ListTemp.Where(x => w.Code == x.LeaveCode).Any()).ToList();
            if (IsProcess == true)
            {
                foreach (var item in Temp)
                {
                    if (!ListLeaveType.Where(w => w.Code == item.Code).Any())
                    {
                        ListLeaveType.Add(item);
                    }
                }
            }

            ListLeaveType = ListLeaveType.Where(w => w.Gender == Sex || w.Gender == "B").ToList();
            return ListLeaveType;
        }
        public void ReGenerateLeaveToken(string EmpCode, DateTime FromDate, DateTime ToDate)
        {
            int Year = FromDate.Year;
            GET_Leave_LeaveBalance(EmpCode, Year);
            if (FromDate.Year != ToDate.Year)
            {
                GET_Leave_LeaveBalance(EmpCode, ToDate.Year);
            }
        }
        public void GET_Leave_LeaveBalance(string EmpCode, int InYear)
        {
            var DBI = new HumicaDBContext();
            DB = new HumicaDBContext();
            try
            {
                DBI.Configuration.AutoDetectChangesEnabled = false;

                string approved = SYDocumentStatus.APPROVED.ToString();
                var Employee = DB.HRStaffProfiles.FirstOrDefault(x => x.EmpCode == EmpCode);
                var LstEmpClaim = DB.HRClaimLeaves.Where(w => w.EmpCode == EmpCode && w.Status == approved && w.WorkingDate.Year == InYear && (w.IsUsed.Value == true || w.IsExpired.Value == false)).ToList();
                var _listLEaveB = DB.HREmpLeaveBs.Where(w => w.EmpCode == EmpCode && w.InYear == InYear).ToList();
                List<HRLeaveType> ListLeaveType = DB.HRLeaveTypes.ToList();
                decimal _DayLeave = 0;
                DateTime EndDate = new DateTime(InYear, 12, 31);
                DateTime StartDate = Convert.ToDateTime(Employee.LeaveConf);
                int WorkMonth = DateTimeHelper.CountMonth(StartDate, EndDate);
                List<HREmpLeaveD> _ListLeaveD = GetLeaveToken(InYear).ToList();
                var LeaveByDay = _ListLeaveD.Where(w => w.EmpCode == Employee.EmpCode).ToList();
                var payParam = DB.PRParameters.FirstOrDefault(w => w.Code == Employee.PayParam);
                var ListLeaveCode = LeaveByDay.GroupBy(w => w.LeaveCode).ToList();
                List<HRSetEntitleD> ListetEntitleD = DB.HRSetEntitleDs.Where(w => w.CodeH == Employee.TemLeave).ToList();
                var ListLeave_Rate = DB.HRLeaveProRates.ToList();

                HRLeaveProRate prorateLeave = new HRLeaveProRate();

                if (_listLEaveB.Count > 0)
                {
                    string LeaveCode = "";
                    var _LT = ListLeaveType.Where(w => w.IsParent == true).ToList();
                    List<HRLeaveType> TempLeave = GetLeaveType(ListLeaveType, Employee.TemLeave, Employee.Sex, WorkMonth);
                    foreach (var Code in ListLeaveCode)
                    {
                        if (!TempLeave.Where(w => w.Code == Code.Key).Any())
                        {
                            HRLeaveType _L = ListLeaveType.FirstOrDefault(w => w.Code == Code.Key);
                            if (_L == null) continue;
                            TempLeave.Add(_L);
                        }
                    }
                    foreach (var item in TempLeave)
                    {
                        LeaveCode = item.Code;

                        var _LeaveB = _listLEaveB.FirstOrDefault(w => w.LeaveCode == LeaveCode);
                        bool IsNew = false;
                        if (_LeaveB == null)
                        {
                            IsNew = true;
                            _LeaveB = NewEmpLeaveB(EmpCode, Employee.AllName, LeaveCode, InYear);
                        }

                        ClsPeriodLeave periodLeave = new ClsPeriodLeave();
                        periodLeave.payParam = payParam;
                        periodLeave.ListLeaveProRate = ListLeave_Rate.Where(w => w.LeaveType == LeaveCode).ToList();
                        periodLeave = GetPeriod(periodLeave, Employee, ListetEntitleD, InYear, _LeaveB.LeaveCode);

                        decimal Balance = _LeaveB.DayEntitle.Value + periodLeave.SeniorityBalance;
                        if (item.IsCurrent == false)
                        {
                            Balance = periodLeave.Balance;
                        }
                        HREmpLeaveB lB = Calculate_Token(_LeaveB, _ListLeaveD, ListLeaveType, payParam, Balance, LstEmpClaim);

                        decimal DayLeave = lB.Balance.Value;
                        if (IsNew == false)
                        {
                            _DayLeave = DayLeave;
                            _LeaveB.AllName = Employee.AllName;
                            _LeaveB.DayLeave = lB.Balance;
                            //_LeaveB.Balance = _LeaveB.DayEntitle + periodLeave.SeniorityBalance + _LeaveB.Rest_Edit + _LeaveB.PH_Edit - _DayLeave;

                            _LeaveB.ChangeBy = User.UserName;
                            _LeaveB.ChangeOn = DateTime.Now;
                            _LeaveB.ForwardUse = lB.ForwardUse;
                            _LeaveB.SeniorityBalance = periodLeave.SeniorityBalance;
                            _LeaveB.YTD = _LeaveB.DayEntitle + periodLeave.SeniorityBalance + _LeaveB.Rest_Edit + _LeaveB.PH_Edit;
                            _LeaveB = Calculate_Balance(_LeaveB);
                            DBI.HREmpLeaveBs.Attach(_LeaveB);
                            DBI.Entry(_LeaveB).Property(w => w.AllName).IsModified = true;
                            DBI.Entry(_LeaveB).Property(w => w.DayLeave).IsModified = true;
                            DBI.Entry(_LeaveB).Property(w => w.TakenHour).IsModified = true;
                            DBI.Entry(_LeaveB).Property(w => w.ForWardExp).IsModified = true;
                            DBI.Entry(_LeaveB).Property(w => w.Balance).IsModified = true;
                            DBI.Entry(_LeaveB).Property(w => w.BalanceHour).IsModified = true;
                            DBI.Entry(_LeaveB).Property(w => w.YTD).IsModified = true;
                            DBI.Entry(_LeaveB).Property(w => w.Rest_Edit).IsModified = true;
                            DBI.Entry(_LeaveB).Property(w => w.PH_Edit).IsModified = true;
                            DBI.Entry(_LeaveB).Property(w => w.ForwardUse).IsModified = true;
                            DBI.Entry(_LeaveB).Property(w => w.SeniorityBalance).IsModified = true;
                            DBI.Entry(_LeaveB).Property(w => w.Current_AL).IsModified = true;
                            DBI.Entry(_LeaveB).Property(w => w.ChangeOn).IsModified = true;
                            DBI.Entry(_LeaveB).Property(w => w.ChangeBy).IsModified = true;
                        }
                        else
                        {
                            if (DayLeave != 0)
                            {
                                var EmpLeaveB = new HREmpLeaveB();
                                EmpLeaveB = _LeaveB;
                                EmpLeaveB.DayLeave = DayLeave;
                                DBI.HREmpLeaveBs.Add(EmpLeaveB);
                            }
                        }
                    }
                    int row = DBI.SaveChanges();
                }
            }
            finally
            {
                DBI.Configuration.AutoDetectChangesEnabled = true;
            }
        }
        public void Generate_Leave_Entitle(string EmpCode, int InYear, HRStaffProfile _StaffProfile, List<HRLeaveType> leaveTypes)
        {
            DateTime EndDate = new DateTime(InYear, 12, 31);
            DateTime FromDate = new DateTime(InYear, 1, 1);
            DateTime StartDate = Convert.ToDateTime(_StaffProfile.LeaveConf);
            int WorkMonth = DateTimeHelper.CountMonth(StartDate, EndDate);
            List<HRLeaveType> ListLeaveType = leaveTypes.Where(w => w.IsParent != true).ToList();
            List<HRSetEntitleD> ListetEntitleD = DB.HRSetEntitleDs.Where(w => w.CodeH == _StaffProfile.TemLeave).ToList();
            List<HRLeaveType> TempLeave = GetLeaveType(ListLeaveType, _StaffProfile.TemLeave, _StaffProfile.Sex, WorkMonth);
            var _listLeaveProRate = DB.HRLeaveProRates.ToList();
            var ListPH = DB.HRPubHolidays.Where(w => w.PDate.Year == InYear).ToList();
            PRParameter payParam = DB.PRParameters.Find(_StaffProfile.PayParam);
            int CountMonth = DateTimeHelper.CountMonth(FromDate, EndDate);
            foreach (var leave in TempLeave)
            {
                if (leave.Code == "AL")
                {
                    var leaveB_AL = DB.HREmpLeaveBs.Where(w => w.LeaveCode == leave.Code && w.EmpCode == EmpCode && w.InYear == InYear);
                    if (leaveB_AL.Any()) continue;
                }
                decimal ENTITLE = 0;
                decimal SeniorityBalance = 0;
                var LeaveTemp = ListetEntitleD.Where(w => w.LeaveCode == leave.Code & w.FromMonth <= WorkMonth && w.ToMonth >= WorkMonth).ToList();
                foreach (var temp in LeaveTemp)
                {
                    SeniorityBalance = (decimal)temp.SeniorityBalance;
                    if (temp.IsProRate == true)
                    {
                        decimal Rate = (decimal)(temp.Entitle / CountMonth);
                        List<HRLeaveProRate> LeaveProRate = _listLeaveProRate.Where(w => w.Status == "NEWJOIN" && w.LeaveType == leave.Code).ToList();
                        if (LeaveProRate.Count() > 0)
                        {
                            if (StartDate.Year == InYear)
                            {
                                DateTime FromDateInMonth = StartDate.StartDateOfMonth();
                                DateTime EndDatetOfMonth = StartDate.EndDateOfMonth();
                                decimal actWorkDay = PRPayParameterObject.Get_WorkingDay(payParam, FromDateInMonth, EndDatetOfMonth, StartDate, EndDatetOfMonth, 0);
                                ENTITLE = Convert.ToDecimal(Rate * DateTimeHelper.MonthDiff(EndDate, StartDate));
                                decimal prorateAmount = 0;
                                if (LeaveProRate.Count > 0)
                                {
                                    HRLeaveProRate prorateLeave = LeaveProRate.Where(w => w.ActWorkDayFrom <= actWorkDay && w.ActWorkDayTo >= actWorkDay).FirstOrDefault();
                                    prorateAmount = prorateLeave == null ? 0 : prorateLeave.Rate;
                                    ENTITLE += prorateAmount;
                                }
                            }
                            else
                            {
                                ENTITLE = (decimal)temp.Entitle;
                            }
                        }
                        else
                        {

                            if (WorkMonth >= temp.ToMonth)
                                ENTITLE = (decimal)temp.Entitle;
                            else
                            {
                                int C_Day = DateTimeHelper.GetDay(StartDate, EndDate);
                                decimal EDay = (decimal)temp.Entitle.Value;
                                ENTITLE = ClsRounding.RoundNormal(C_Day * (EDay / 365), 2);
                                if (ENTITLE > (decimal)temp.Entitle.Value)
                                {
                                    ENTITLE = (decimal)temp.Entitle;
                                }

                            }
                        }
                    }
                    else
                    {
                        ENTITLE = (decimal)temp.Entitle;
                    }
                }
                EmpLeaveB = new HREmpLeaveB();
                if (leave.Code == "PH")
                {
                    ENTITLE = ListPH.Count();
                }
                if (leave.Code == "DO" || leave.Code == "OFF")
                {
                    ENTITLE = 0;
                    if (StartDate.Year == InYear)
                    {
                        FromDate = StartDate;
                    }
                    for (var day = FromDate.Date; day.Date <= EndDate.Date; day = day.AddDays(1))
                    {
                        if (day.DayOfWeek == DayOfWeek.Sunday)
                        {
                            ENTITLE += 1;
                        }
                    }
                }
                EmpLeaveB.AllName = _StaffProfile.AllName;
                EmpLeaveB.DayEntitle = ENTITLE;
                EmpLeaveB.EmpCode = EmpCode;
                EmpLeaveB.InYear = InYear;
                EmpLeaveB.YTD = ENTITLE + SeniorityBalance;
                EmpLeaveB.LeaveCode = leave.Code;
                EmpLeaveB.Balance = ENTITLE + SeniorityBalance;
                EmpLeaveB.Forward = 0;
                EmpLeaveB.DayLeave = 0;
                EmpLeaveB.ForWardExp = new DateTime(1900, 1, 1);
                EmpLeaveB.SeniorityBalance = SeniorityBalance;
                EmpLeaveB.CreateBy = User.UserName;
                EmpLeaveB.CreateOn = DateTime.Now;
                ListEmpLeaveB.Add(EmpLeaveB);
            }
        }
        public void Generate_Leave_Cu(int InYear)
        {
            var DBI = new HumicaDBContext();
            try
            {
                DB.Configuration.AutoDetectChangesEnabled = false;
                DBI.Configuration.AutoDetectChangesEnabled = false;
                DateTime DateNow = DateTime.Now;
                DateTime StartDate = new DateTime(InYear, 1, 1);
                DateTime FromDate = new DateTime(InYear, 1, 1);
                DateTime ToDate = new DateTime(InYear, DateNow.Month, DateTime.DaysInMonth(InYear, DateNow.Month));
                var Employee = DB.HRStaffProfiles.Where(w => (w.Status == "A" || (w.Status == "I" && w.DateTerminate.Year >= InYear))).ToList();
                var ListParam = DB.PRParameters.ToList();
                List<HRStaffProfile> _listStaff = new List<HRStaffProfile>();
                _listStaff = Employee.Where(w => w.StartDate <= ToDate && (w.DateTerminate.Year == 1900 || w.DateTerminate.AddDays(-1) >= FromDate)).ToList();

                var EmpLeaveB = DB.HREmpLeaveBs.Where(w => w.InYear == InYear).ToList();
                var LeaveType = DB.HRLeaveTypes.ToList();

                EmpLeaveB = EmpLeaveB.Where(w => _listStaff.Where(x => w.EmpCode == x.EmpCode).Any()).ToList();
                var ListLeave_Rate = DB.HRLeaveProRates.ToList();
                List<HRSetEntitleD> ListetEntitleD = DB.HRSetEntitleDs.Where(w => w.IsProRate == true).ToList();
                EmpLeaveB = EmpLeaveB.Where(w => w.InYear == InYear && ListetEntitleD.Where(x => x.LeaveCode == w.LeaveCode).Any()).ToList();
                List<HREmpLeaveD> _ListLeaveD = GetLeaveToken(InYear).ToList();
                _ListLeaveD = _ListLeaveD.Where(w => w.LeaveDate.Year == InYear).ToList();// && ListetEntitleD.Where(x => x.LeaveCode == w.LeaveCode).Any()).ToList();
                //DateTime EndDate = new DateTime(InYear, 12, 31);
                //int CountMonth = DateTimeHelper.GetMonth(FromDate, EndDate);
                string approved = SYDocumentStatus.APPROVED.ToString();
                List<HRClaimLeave> LstEmpClaim = DB.HRClaimLeaves.Where(w => w.Status == approved && w.WorkingDate.Year == InYear && (w.IsUsed.Value == true || w.IsExpired.Value == false)).ToList();

                foreach (var emp in EmpLeaveB)
                {
                    var Staff = _listStaff.FirstOrDefault(w => w.EmpCode == emp.EmpCode);
                    PRParameter payParam = ListParam.FirstOrDefault(w => w.Code == Staff.PayParam);

                    ClsPeriodLeave periodLeave = new ClsPeriodLeave();
                    periodLeave.payParam = payParam;
                    periodLeave.ListLeaveProRate = ListLeave_Rate.Where(w => w.LeaveType == emp.LeaveCode).ToList();
                    periodLeave = GetPeriod(periodLeave, Staff, ListetEntitleD, InYear, emp.LeaveCode);

                    var EmpClaim = LstEmpClaim.Where(w => w.EmpCode == emp.EmpCode).ToList();
                    decimal Balance = periodLeave.SeniorityBalance + periodLeave.Entitle;//emp.DayEntitle.Value;
                    var item = LeaveType.FirstOrDefault(w => w.Code == emp.LeaveCode);
                    if (item != null)
                    {
                        if (item.IsCurrent == false)
                        {
                            Balance = periodLeave.Balance;
                        }
                    }
                    HREmpLeaveB lB = Calculate_Token(emp, _ListLeaveD, LeaveType, payParam, Balance, EmpClaim);

                    var ObjMatch = emp;
                    ObjMatch.CurrentEntitle = periodLeave.Balance + periodLeave.SeniorityBalance;
                    ObjMatch.Current_AL = periodLeave.Balance - lB.Balance.Value + periodLeave.SeniorityBalance;
                    //ObjMatch.YTD = ObjMatch.DayEntitle + periodLeave.SeniorityBalance;
                    ObjMatch.SeniorityBalance = periodLeave.SeniorityBalance;
                    ObjMatch.ForwardUse = lB.ForwardUse;
                    ObjMatch.DayLeave = lB.Balance;
                    ObjMatch.TakenHour = lB.TakenHour;
                    ObjMatch.DayEntitle = periodLeave.Entitle;
                    //ObjMatch.Balance = ObjMatch.DayEntitle + periodLeave.SeniorityBalance - lB.Balance;
                    ObjMatch = Calculate_Balance(ObjMatch);
                    DBI.HREmpLeaveBs.Attach(ObjMatch);
                    DBI.Entry(ObjMatch).Property(w => w.Balance).IsModified = true;
                    DBI.Entry(ObjMatch).Property(w => w.DayLeave).IsModified = true;
                    DBI.Entry(ObjMatch).Property(w => w.Current_AL).IsModified = true;
                    DBI.Entry(ObjMatch).Property(w => w.CurrentEntitle).IsModified = true;
                    DBI.Entry(ObjMatch).Property(w => w.YTD).IsModified = true;
                    DBI.Entry(ObjMatch).Property(w => w.SeniorityBalance).IsModified = true;
                    DBI.Entry(ObjMatch).Property(w => w.TakenHour).IsModified = true;
                    DBI.Entry(ObjMatch).Property(w => w.ForwardUse).IsModified = true;
                }
                DBI.SaveChanges();
            }
            finally
            {
                DB.Configuration.AutoDetectChangesEnabled = true;
                DBI.Configuration.AutoDetectChangesEnabled = true;
            }
        }
        public string GenerateLeave(string EmpCode)
        {
            try
            {
                try
                {
                    DB = new HumicaDBContext();
                    DB.Configuration.AutoDetectChangesEnabled = false;

                    ListEmpLeaveB = new List<HREmpLeaveB>();
                    string[] Emp = EmpCode.Split(';');
                    List<HREmpLeaveB> LEmpLeave = DB.HREmpLeaveBs.Where(x => x.InYear == Filter.INYear).ToList();
                    var TemLeave = DB.HRSetEntitleHs.ToList();
                    List<HRLeaveType> leaveTypes = DB.HRLeaveTypes.ToList();
                    var Employee = DB.HRStaffProfiles.Where(w => (w.Status == "A" || (w.Status == "I" && w.DateTerminate.Year >= Filter.INYear))).ToList();
                    foreach (var Code in Emp)
                    {
                        var objEmpB = LEmpLeave.Where(w => w.EmpCode == Code && w.LeaveCode != "AL").ToList();
                        foreach (var item in objEmpB)
                        {
                            DB.HREmpLeaveBs.Attach(item);
                            DB.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                        }
                        if (Code.Trim() != "")
                        {
                            var staff = Employee.FirstOrDefault(w => w.EmpCode == Code);
                            if (staff != null)
                            {
                                if (staff.TemLeave == null)
                                {
                                    return "Pls assign Template Leave to EmpCode: " + Code;
                                }
                                if (TemLeave.Where(w => w.Code == staff.TemLeave).Any())
                                {
                                    Generate_Leave_Entitle(Code, Filter.INYear, staff, leaveTypes);
                                }
                            }
                        }
                    }
                    foreach (var leave in ListEmpLeaveB)
                    {
                        var staff = Employee.FirstOrDefault(w => w.EmpCode == leave.EmpCode);

                        EmpLeaveB = new HREmpLeaveB();
                        EmpLeaveB = leave;
                        //if (LEmpLeave.Where(x => x.InYear == Filter.INYear && x.EmpCode == leave.EmpCode && x.LeaveCode == leave.LeaveCode).ToList().Count() > 0)
                        //{
                        //    var objUpdate = LEmpLeave.Where(x => x.InYear == Filter.INYear && x.EmpCode == leave.EmpCode && x.LeaveCode == leave.LeaveCode).First();
                        //    objUpdate.DayEntitle = leave.DayEntitle;
                        //    objUpdate.Balance = leave.DayEntitle;
                        //    objUpdate.SeniorityBalance = leave.SeniorityBalance;
                        //    objUpdate.YTD = leave.DayEntitle;
                        //    objUpdate.ChangeBy = User.UserName;
                        //    objUpdate.ChangeOn = DateTime.Now;
                        //    DB.HREmpLeaveBs.Attach(objUpdate);
                        //    DB.Entry(objUpdate).Property(w => w.DayEntitle).IsModified = true;
                        //    DB.Entry(objUpdate).Property(w => w.Balance).IsModified = true;
                        //    DB.Entry(objUpdate).Property(w => w.SeniorityBalance).IsModified = true;
                        //    DB.Entry(objUpdate).Property(w => w.YTD).IsModified = true;
                        //    DB.Entry(objUpdate).Property(w => w.ChangeOn).IsModified = true;
                        //    DB.Entry(objUpdate).Property(w => w.ChangeBy).IsModified = true;
                        //}
                        //else
                        //{
                        EmpLeaveB.CreateOn = DateTime.Now; ;
                        EmpLeaveB.CreateBy = User.UserName;
                        DB.HREmpLeaveBs.Add(EmpLeaveB);
                    }

                    int row = DB.SaveChanges();

                    //ReCalcuate Leave
                    foreach (var Code in Emp)
                    {
                        if (Code.Trim() != "")
                        {
                            GET_Leave_LeaveBalance(Code, Filter.INYear);
                        }
                    }

                    return SYConstant.OK;
                }
                catch (DbEntityValidationException e)
                {
                    /*------------------SaveLog----------------------------------*/
                    SYEventLog log = new SYEventLog();
                    log.ScreenId = ScreenId;
                    log.UserId = User.UserName;
                    log.DocurmentAction = Filter.INYear.ToString();
                    log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                    SYEventLogObject.saveEventLog(log, e);
                    /*----------------------------------------------------------*/
                    return "EE001";
                }
                catch (DbUpdateException e)
                {
                    /*------------------SaveLog----------------------------------*/
                    SYEventLog log = new SYEventLog();
                    log.ScreenId = ScreenId;
                    log.UserId = User.UserName;
                    log.DocurmentAction = Filter.INYear.ToString();
                    log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                    SYEventLogObject.saveEventLog(log, e, true);
                    /*----------------------------------------------------------*/
                    return "EE001";
                }
                catch (Exception e)
                {
                    /*------------------SaveLog----------------------------------*/
                    SYEventLog log = new SYEventLog();
                    log.ScreenId = ScreenId;
                    log.UserId = User.UserName;
                    log.DocurmentAction = Filter.INYear.ToString();
                    log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                    SYEventLogObject.saveEventLog(log, e, true);
                    /*----------------------------------------------------------*/
                    return "EE001";
                }
            }
            finally
            {
                DB.Configuration.AutoDetectChangesEnabled = true;
            }
        }
        public string TransferLeave(string EmpCode)
        {
            try
            {
                var DBI = new HumicaDBContext();
                ListEmpLeaveB = new List<HREmpLeaveB>();
                string[] Emp = EmpCode.Split(';');
                //var EmpLeave = DB.HREmpLeaveBs.ToList();
                var LEmpLeave = new List<HREmpLeaveB>();
                //LEmpLeave = EmpLeave.ToList();
                LEmpLeave = DB.HREmpLeaveBs.Where(x => x.InYear == Filter.TYear).ToList();
                if (Filter.FYear >= Filter.TYear)
                {
                    return "INVALID_YEAR";
                }
                foreach (var Code in Emp)
                {
                    if (Code.Trim() != "")
                    {
                        var result = LEmpLeave.Where(w => w.EmpCode == Code && w.InYear == Filter.TYear && w.LeaveCode == Filter.LeaveType).ToList();
                        var resultFor = ListForward.Where(w => w.EmpCode == Code).ToList().FirstOrDefault();
                        foreach (var item in result)
                        {
                            if (resultFor.ForWard <= 0)
                            {
                                item.Forward = 0;
                                item.ForWardExp = new DateTime(1900, 1, 1);
                            }
                            else
                            {
                                item.Forward = resultFor.ForWard;
                                item.ForWardExp = Filter.ForwardExp;
                            }

                            DB.HREmpLeaveBs.Attach(item);
                            DB.Entry(item).Property(w => w.Forward).IsModified = true;
                            DB.Entry(item).Property(w => w.ForWardExp).IsModified = true;

                        }
                    }
                }

                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Filter.TYear.ToString();
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (DbUpdateException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Filter.TYear.ToString();
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (Exception e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Filter.TYear.ToString();
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public void getLeaveDay(string EmpCode)
        {
            double NoLeave = 0;
            double PH = 0;
            double Rest = 0;
            double LHour = 0;
            double WHour = 0;
            if (ListEmpLeaveD.Count > 0)
            {
                string PayPram = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == EmpCode).PayParam;
                var Pay = DB.PRParameters.Find(PayPram);
                WHour = Convert.ToDouble(Pay.WHOUR);
                foreach (var item in ListEmpLeaveD)
                {
                    // LHour = Convert.ToDouble(item.LHour)/WHour;
                    LHour = WHour;// Convert.ToDouble(item.LHour);
                    if (HeaderEmpLeave.Units != "Day")
                    {
                        LHour = (double)item.LHour;
                    }
                    if (item.Remark == "Morning" || item.Remark == "Afternoon")
                    {
                        LHour = Convert.ToDouble(WHour / 2);
                    }
                    LHour = LHour / WHour;
                    if (item.Status == "Leave")
                        NoLeave += LHour;
                    else if (item.Status == "PH")
                        PH += LHour;
                    else if (item.Status == "Rest")
                        Rest += LHour;
                }
                HeaderEmpLeave.NoDay = NoLeave;
                HeaderEmpLeave.NoPH = PH;
                HeaderEmpLeave.NoRest = Rest;
            }
        }

        public string CreateLeaveRequest(string id)
        {
            try
            {
                DB = new HumicaDBContext();
                DB.Configuration.AutoDetectChangesEnabled = false;
                var LeaveD = new List<HREmpLeaveD>();
                //var StaffList = DB.HRStaffProfiles.ToList();
                var staff = DB.HRStaffProfiles.FirstOrDefault(x => x.EmpCode == HeaderStaff.EmpCode);
                string Status = SYDocumentStatus.APPROVED.ToString();
                string Reject = SYDocumentStatus.REJECTED.ToString();
                string Cancel = SYDocumentStatus.CANCELLED.ToString();
                var lstempLeave = DB.HREmpLeaves.Where(w => w.Status != Cancel && w.Status != Reject && w.EmpCode == HeaderStaff.EmpCode).ToList();
                HeaderEmpLeave.EmpCode = HeaderStaff.EmpCode;
                var ATSche = DB.ATEmpSchedules.Where(w => w.EmpCode == HeaderEmpLeave.EmpCode
                            && EntityFunctions.TruncateTime(w.TranDate) >= HeaderEmpLeave.FromDate.Date
                            && EntityFunctions.TruncateTime(w.TranDate) <= HeaderEmpLeave.ToDate.Date).ToList();
                int Increment = GenerateLeaveObject.GetLastIncrement();
                //int Increment;
                //if (lstempLeave.Count == 0) Increment = 0;
                //else Increment = lstempLeave.Max(w => w.Increment);
                if (HeaderEmpLeave.Reason == "" || HeaderEmpLeave.Reason == null)
                    return "REASON";
                if (ListEmpLeaveD.Count <= 0)
                    return "INV_DOC";
                decimal balance = EmpLeaveB.Balance ?? 0;
                if (HeaderEmpLeave.FromDate.Date > HeaderEmpLeave.ToDate.Date)
                    return "INVALID_DATE";
                var leaveH = lstempLeave.ToList();

                if (leaveH.Where(w => ((w.FromDate.Date >= HeaderEmpLeave.FromDate.Date && w.FromDate.Date <= HeaderEmpLeave.ToDate.Date) ||
                  (w.ToDate.Date >= HeaderEmpLeave.FromDate.Date && w.ToDate.Date <= HeaderEmpLeave.ToDate.Date) ||
                        (HeaderEmpLeave.FromDate.Date >= w.FromDate.Date && HeaderEmpLeave.FromDate.Date <= w.ToDate.Date) || (HeaderEmpLeave.ToDate.Date >= w.FromDate.Date && HeaderEmpLeave.ToDate.Date <= w.ToDate.Date))).Any())
                {
                    var EmpLeaveD = DB.HREmpLeaveDs.Where(w => w.EmpCode == HeaderStaff.EmpCode).ToList();
                    EmpLeaveD = EmpLeaveD.Where(w => w.LeaveDate.Date >= HeaderEmpLeave.FromDate.Date && w.LeaveDate.Date <= HeaderEmpLeave.ToDate.Date).ToList();
                    var Result = ListEmpLeaveD.Where(w => EmpLeaveD.Where(x => x.LeaveDate.Date == w.LeaveDate.Date && w.Remark != "Hours").Any()).ToList();
                    var Result_ = ListEmpLeaveD.Where(w => EmpLeaveD.Where(x => x.LeaveDate.Date == w.LeaveDate.Date && w.Remark != "FullDay").Any()).ToList();
                    var ResultHour = ListEmpLeaveD.Where(w => EmpLeaveD.Where(x => x.LeaveDate.Date == w.LeaveDate.Date && (x.Remark == "Hours" || w.Remark == "Hours")).Any()).ToList();
                    var empATSche = ATSche.Where(w => ResultHour.Where(x => w.TranDate == x.LeaveDate && w.EmpCode == HeaderStaff.EmpCode).Any()).ToList();
                    if (Result.Where(w => w.Remark == "FullDay").Any())
                        if (ResultHour.Count > 0) return "INV_DATE";
                    if (EmpLeaveD.Where(w => Result.Where(x => x.LeaveDate.Date == w.LeaveDate.Date && x.Remark == w.Remark).Any()).Any())
                        return "INV_DATE";
                    if (EmpLeaveD.Where(w => ResultHour.Where(x => x.LeaveDate.Date == w.LeaveDate.Date).Any()).Any())
                    {
                        if (empATSche.Count > 0)
                        {
                            foreach (var read in empATSche)
                            {
                                TimeSpan time = new TimeSpan(0, 4, 0, 0);
                                DateTime brackstart = read.IN1.Value.Add(time);
                                DateTime brackend = read.OUT1.Value.Subtract(time);
                                if (read.Flag1 == 1 && read.Flag2 == 1 && ResultHour.Count > 0)
                                {
                                    if (EmpLeaveD.Where(w => Result_.Where(x => x.LeaveDate.Date == read.TranDate.Date && ((read.IN1 >= w.StartTime && read.IN1 <= w.EndTime)
                                                        || (read.OUT1 >= w.StartTime && read.OUT1 <= w.EndTime) || (w.StartTime >= read.IN1 && w.StartTime <= read.OUT1)
                                                        || (w.EndTime >= read.IN1 && w.EndTime <= read.OUT1)) && x.Remark == "Morning").Any()).Any()) return "INV_DATE";
                                    else if (EmpLeaveD.Where(w => Result_.Where(x => x.LeaveDate.Date == read.TranDate.Date && ((read.IN2 >= w.StartTime && read.IN2 <= w.EndTime)
                                                        || (read.OUT2 >= w.StartTime && read.OUT2 <= w.EndTime) || (w.StartTime >= read.IN2 && w.StartTime <= read.OUT2)
                                                        || (w.EndTime >= read.IN2 && w.EndTime <= read.OUT2)) && x.Remark == "Afternoon").Any()).Any()) return "INV_DATE";
                                }
                                else if (read.Flag1 == 1 && read.Flag2 == 2 && ResultHour.Count > 0)
                                {
                                    if (EmpLeaveD.Where(w => Result_.Where(x => x.LeaveDate.Date == read.TranDate.Date && ((read.IN1 >= w.StartTime && read.IN1 <= w.EndTime)
                                                        || (brackstart >= w.StartTime && brackstart <= w.EndTime) || (w.StartTime >= read.IN1 && w.StartTime <= brackstart)
                                                        || (w.EndTime >= read.IN1 && w.EndTime <= brackstart)) && x.Remark == "Morning").Any()).Any()) return "INV_DATE";
                                    else if (EmpLeaveD.Where(w => Result_.Where(x => x.LeaveDate.Date == read.TranDate.Date && ((brackend >= w.StartTime && brackend <= w.EndTime)
                                                        || (read.OUT2 >= w.StartTime && read.OUT2 <= w.EndTime) || (w.StartTime >= brackend && w.StartTime <= read.OUT2)
                                                        || (w.EndTime >= brackend && w.EndTime <= read.OUT2)) && x.Remark == "Afternoon").Any()).Any()) return "INV_DATE";
                                }
                            }
                        }
                    }
                    if (EmpLeaveD.Where(w => w.Remark == "FullDay").Any())
                    {
                        if (ResultHour.Count > 0) return "INV_DATE";
                        if (Result.Count > 0) return "INV_DATE";
                    }
                    if (ResultHour.Count > 0)
                    {
                        var _EmpLeaveItem = EmpLeaveD.Where(w => w.Remark == "Hours").ToList();
                        if (_EmpLeaveItem.Any())
                        {
                            foreach (var item in ResultHour)
                            {
                                if (_EmpLeaveItem.Where(w => w.StartTime <= item.StartTime.Value.AddMinutes(1) && w.EndTime >= item.StartTime.Value.AddMinutes(1)).Any())
                                    return "INV_DATE";
                                else if (_EmpLeaveItem.Where(w => w.StartTime <= item.EndTime.Value.AddMinutes(1) && w.EndTime >= item.EndTime.Value.AddMinutes(1)).Any())
                                    return "INV_DATE";
                            }
                        }
                    }
                    if (ResultHour.Count > 0)
                    {
                        if (EmpLeaveD.Where(w => w.Remark == "FullDay").Any()) return "INV_DATE";
                        if (Result.Where(w => w.Remark == "FullDay").Any()) return "INV_DATE";
                    }
                }
                getLeaveDay(HeaderStaff.EmpCode);
                var LeaveType = DB.HRLeaveTypes.Find(HeaderEmpLeave.LeaveCode);
                PRParameter payParam = DB.PRParameters.FirstOrDefault(w => w.Code == staff.PayParam);
                if (LeaveType.IsCurrent == false)
                {
                    ClsPeriodLeave periodLeave = new ClsPeriodLeave();
                    var ListLeave_Rate = DB.HRLeaveProRates.ToList();
                    List<HRSetEntitleD> ListetEntitleD = DB.HRSetEntitleDs.Where(w => w.CodeH == staff.TemLeave).ToList();
                    periodLeave.payParam = payParam;
                    periodLeave.ListLeaveProRate = ListLeave_Rate;
                    periodLeave = GetPeriod(periodLeave, staff, ListetEntitleD, HeaderEmpLeave.ToDate.Year, HeaderEmpLeave.LeaveCode);
                  
                    if (periodLeave.Balance - Convert.ToDecimal(HeaderEmpLeave.NoDay) < 0)
                        return "INV_BALANCE";
                }
                if (LeaveType.IsOverEntitle == false)
                {
                    int year = HeaderEmpLeave.ToDate.Year;
                    List<HREmpLeaveD> _ListLeaveD = GetLeaveToken(year).ToList();
                    _ListLeaveD = _ListLeaveD.Where(w => w.LeaveDate.Year == year && w.EmpCode == HeaderStaff.EmpCode).ToList();
                    var EmpLeaveB = DB.HREmpLeaveBs.FirstOrDefault(w => w.InYear == year && w.LeaveCode == HeaderEmpLeave.LeaveCode && w.EmpCode == HeaderStaff.EmpCode);

                    if (balance - Convert.ToDecimal(HeaderEmpLeave.NoDay) < 0)
                    {
                        _ListLeaveD.AddRange(GetLeave_D(ListEmpLeaveD, HeaderStaff.EmpCode, HeaderEmpLeave.LeaveCode, HeaderEmpLeave.Units));
                        var ListLeaveType = DB.HRLeaveTypes.ToList();
                        List<HRClaimLeave> LstEmpClaim = DB.HRClaimLeaves.Where(w => w.Status == Status && w.WorkingDate.Year == year && (w.IsUsed.Value == true || w.IsExpired.Value == false)).ToList();
                        var EmpClaim = LstEmpClaim.Where(w => w.EmpCode == HeaderEmpLeave.EmpCode).ToList();
                        HREmpLeaveB lB = Calculate_Token(EmpLeaveB, _ListLeaveD, ListLeaveType, payParam, EmpLeaveB.YTD.Value, LstEmpClaim);
                        if ((EmpLeaveB.YTD - lB.Balance) < 0)
                            return "INT_Entile";
                    }
                }
                if (LeaveType != null)
                {
                    if (LeaveType.Probation == true)
                    {
                        if (staff.Probation.Value.Date > HeaderEmpLeave.FromDate.Date)
                            return "CANNOT_PROBATION";
                    }
                }
                HeaderEmpLeave.Status = Status;
                HeaderEmpLeave.Attachment = HeaderEmpLeave.Attachment;
                HeaderEmpLeave.RequestDate = DateTime.Now;
                HeaderEmpLeave.Increment = Increment + 1;
                HeaderEmpLeave.CreateBy = User.UserName;
                HeaderEmpLeave.CreateOn = DateTime.Now;
                DB.HREmpLeaves.Add(HeaderEmpLeave);

                DB.HREmpLeaveDs.AddRange(GetLeave_D(ListEmpLeaveD, HeaderStaff.EmpCode, HeaderEmpLeave.LeaveCode, HeaderEmpLeave.Units));
                var ListAtt = ATSche.ToList();
                foreach (var item in ListEmpLeaveD)
                {
                    if (ListAtt.Where(w => w.TranDate.Date == item.LeaveDate.Date).Any())
                    {
                        var attFirst = ListAtt.First(w => w.TranDate.Date == item.LeaveDate.Date);
                        attFirst.LeaveDesc = "";
                        attFirst.LeaveCode = HeaderEmpLeave.LeaveCode;
                        attFirst.LeaveNo = Increment + 1;
                        DB.ATEmpSchedules.Attach(attFirst);
                        DB.Entry(attFirst).Property(w => w.LeaveDesc).IsModified = true;
                        DB.Entry(attFirst).Property(w => w.LeaveCode).IsModified = true;
                        DB.Entry(attFirst).Property(w => w.LeaveNo).IsModified = true;
                    }
                }
                string Approval = SYDocumentStatus.APPROVED.ToString();
                List<HRClaimLeave> _listClaim = DB.HRClaimLeaves.Where(w => w.EmpCode == HeaderEmpLeave.EmpCode
                && (w.IsExpired.Value != true || w.IsUsed.Value != true) && w.Status == Approval).ToList();
                DateTime DateNow = DateTime.Now;
                bool Isused = false;
                foreach (var claim in _listClaim.ToList().OrderBy(w => w.WorkingDate))
                {
                    if (Isused == true) continue;
                    if (claim.Expired.Value.Date < DateNow.Date)
                    {
                        claim.IsExpired = true;
                        DB.HRClaimLeaves.Attach(claim);
                        DB.Entry(claim).Property(x => x.IsExpired).IsModified = true;
                    }
                    else
                    {
                        Isused = true;
                        claim.IsUsed = true;
                        claim.DocumentRef = HeaderEmpLeave.Increment.ToString();
                        DB.HRClaimLeaves.Attach(claim);
                        DB.Entry(claim).Property(x => x.IsUsed).IsModified = true;
                        DB.Entry(claim).Property(x => x.DocumentRef).IsModified = true;
                    }
                }
                int row = DB.SaveChanges();
                ReGenerateLeaveToken(HeaderEmpLeave.EmpCode, HeaderEmpLeave.FromDate, HeaderEmpLeave.ToDate);
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserID.ToString();
                log.ScreenId = HeaderEmpLeave.EmpCode;
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            finally
            {
                DB.Configuration.AutoDetectChangesEnabled = true;
            }
        }

        public string EditLeaveRequest(int id)
        {
            try
            {
                HumicaDBContext DBM = new HumicaDBContext();
                var objMatch = DB.HREmpLeaves.FirstOrDefault(w => w.TranNo == id);
                if (objMatch == null)
                    return "LEAVE_NE";
                if (HeaderEmpLeave.FromDate.Date > HeaderEmpLeave.ToDate)
                    return "INVALID_DATE";
                var objEmp = DB.HREmpLeaveDs.Where(w => w.LeaveTranNo == objMatch.Increment && w.EmpCode == objMatch.EmpCode).ToList();
                foreach (var item in objEmp)
                {
                    DB.HREmpLeaveDs.Attach(item);
                    DB.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                }
                var staff = DB.HRStaffProfiles.FirstOrDefault(x => x.EmpCode == objMatch.EmpCode);
                var LeaveType = DB.HRLeaveTypes.Find(HeaderEmpLeave.LeaveCode);
                if (LeaveType.IsCurrent == false)
                {
                    ClsPeriodLeave periodLeave = new ClsPeriodLeave();
                    PRParameter payParam = DB.PRParameters.FirstOrDefault(w => w.Code == staff.PayParam);
                    var ListLeave_Rate = DB.HRLeaveProRates.ToList();
                    List<HRSetEntitleD> ListetEntitleD = DB.HRSetEntitleDs.Where(w => w.CodeH == staff.TemLeave).ToList();
                    periodLeave.payParam = payParam;
                    periodLeave.ListLeaveProRate = ListLeave_Rate;
                    periodLeave = GetPeriod(periodLeave, staff, ListetEntitleD, HeaderEmpLeave.ToDate.Year, HeaderEmpLeave.LeaveCode);

                    if (periodLeave.Balance - Convert.ToDecimal(HeaderEmpLeave.NoDay) < 0)
                        return "INV_BALANCE";

                }
                if (LeaveType.IsOverEntitle == false)
                {
                    var EmpLeaveB = DB.HREmpLeaveBs.FirstOrDefault(w => w.InYear == DateTime.Now.Year && w.LeaveCode == HeaderEmpLeave.LeaveCode && w.EmpCode == objMatch.EmpCode);

                    if (EmpLeaveB.Balance - Convert.ToDecimal(HeaderEmpLeave.NoDay) < 0)
                        return "INT_Entile";
                }
                getLeaveDay(objMatch.EmpCode);
                HeaderEmpLeave.EmpCode = objMatch.EmpCode;
                objMatch.LeaveCode = HeaderEmpLeave.LeaveCode;
                objMatch.FromDate = HeaderEmpLeave.FromDate;
                objMatch.ToDate = HeaderEmpLeave.ToDate;
                objMatch.Units = HeaderEmpLeave.Units;
                objMatch.TaskHand_Over = HeaderEmpLeave.TaskHand_Over;
                objMatch.Attachment = HeaderEmpLeave.Attachment;
                objMatch.NoDay = HeaderEmpLeave.NoDay;
                objMatch.NoPH = HeaderEmpLeave.NoPH;
                objMatch.NoRest = HeaderEmpLeave.NoRest;
                objMatch.Remark1 = HeaderEmpLeave.Remark1;
                objMatch.Reason = HeaderEmpLeave.Reason;
                objMatch.ChangedBy = User.UserName;
                objMatch.ChangedOn = DateTime.Now;
                DB.HREmpLeaves.Attach(objMatch);
                DB.Entry(objMatch).Property(w => w.LeaveCode).IsModified = true;
                DB.Entry(objMatch).Property(w => w.FromDate).IsModified = true;
                DB.Entry(objMatch).Property(w => w.ToDate).IsModified = true;
                DB.Entry(objMatch).Property(w => w.Units).IsModified = true;
                DB.Entry(objMatch).Property(w => w.NoDay).IsModified = true;
                DB.Entry(objMatch).Property(w => w.NoPH).IsModified = true;
                DB.Entry(objMatch).Property(w => w.Attachment).IsModified = true;
                DB.Entry(objMatch).Property(w => w.NoRest).IsModified = true;
                DB.Entry(objMatch).Property(w => w.Remark1).IsModified = true;
                DB.Entry(objMatch).Property(w => w.Reason).IsModified = true;
                DB.Entry(objMatch).Property(w => w.TaskHand_Over).IsModified = true;
                DB.Entry(objMatch).Property(w => w.ChangedBy).IsModified = true;
                DB.Entry(objMatch).Property(w => w.ChangedOn).IsModified = true;
                string PayPram = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == HeaderStaff.EmpCode).PayParam;
                var Pay = DB.PRParameters.Find(PayPram);
                decimal WHour = Convert.ToDecimal(Pay.WHOUR);
                int Line = 0;
                var ListAtt = DB.ATEmpSchedules.Where(w => w.EmpCode == HeaderEmpLeave.EmpCode).ToList();
                foreach (var item in ListAtt.Where(w => w.LeaveNo == objMatch.Increment).ToList())
                {
                    item.LeaveCode = "";
                    item.LeaveNo = -1;
                    DB.Entry(item).Property(w => w.LeaveNo).IsModified = true;
                    DB.Entry(item).Property(w => w.LeaveCode).IsModified = true;
                }
                foreach (var item in ListEmpLeaveD)
                {
                    Line += 1;
                    var result = new HREmpLeaveD();
                    result.LeaveTranNo = objMatch.Increment;
                    result.EmpCode = HeaderStaff.EmpCode;
                    result.LeaveCode = HeaderEmpLeave.LeaveCode;
                    result.LeaveDate = item.LeaveDate;
                    result.CutMonth = item.CutMonth;
                    result.Status = item.Status;
                    result.Remark = item.Remark;
                    result.LineItem = Line;
                    result.LHour = item.LHour;
                    if (HeaderEmpLeave.Units != "Day")
                    {
                        if (item.StartTime.Value.Year == 100)
                        {
                            item.StartTime = item.LeaveDate + item.StartTime.Value.TimeOfDay;
                            item.EndTime = item.LeaveDate + item.EndTime.Value.TimeOfDay;
                        }
                        result.Remark = "Hours";
                        result.StartTime = item.StartTime;
                        result.EndTime = item.EndTime;
                    }
                    if (item.Remark == "Morning" || item.Remark == "Afternoon")
                        result.LHour = WHour / 2;

                    result.CreateBy = User.UserName;
                    result.CreateOn = DateTime.Now;
                    DB.HREmpLeaveDs.Add(result);
                    if (ListAtt.Where(w => w.TranDate.Date == result.LeaveDate.Date).Any())
                    {
                        var attFirst = ListAtt.FirstOrDefault(w => w.TranDate.Date == result.LeaveDate.Date);
                        attFirst.LeaveDesc = "";
                        attFirst.LeaveCode = result.LeaveCode;
                        attFirst.LeaveNo = result.LeaveTranNo;
                        DB.ATEmpSchedules.Attach(attFirst);
                        DB.Entry(attFirst).Property(w => w.LeaveDesc).IsModified = true;
                        DB.Entry(attFirst).Property(w => w.LeaveCode).IsModified = true;
                        DB.Entry(attFirst).Property(w => w.LeaveNo).IsModified = true;
                    }
                }
                int row = DB.SaveChanges();
                ReGenerateLeaveToken(HeaderEmpLeave.EmpCode, HeaderEmpLeave.FromDate, HeaderEmpLeave.ToDate);
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = HeaderEmpLeave.EmpCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (DbUpdateException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = HeaderEmpLeave.EmpCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (Exception e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = HeaderEmpLeave.EmpCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string ESSRequestLeave(string id, string URL, string fileName)
        {
            MessageError = "";
            try
            {
                DB = new HumicaDBContext();
                var DBI = new HumicaDBContext();
                var LeaveD = new List<HREmpLeaveD>();
                var _LeaveType = DB.HRLeaveTypes.Find(HeaderEmpLeave.LeaveCode);
                if (_LeaveType == null)
                    return "INV_DOC";
                if (ListEmpLeaveD.Count <= 0)
                    return "INV_DOC";
                decimal balance = EmpLeaveB.Balance ?? 0;
                if (HeaderEmpLeave.Urgent == false)
                {
                    if (_LeaveType.BeforeDay > 0)
                    {
                        if (HeaderEmpLeave.Units == "Day")
                        {
                            int day = _LeaveType.BeforeDay.Value; 
                            DateTime currentDate = DateTime.Now;
                            DateTime requiredDate = currentDate.AddDays(day);

                            var message = DP.SYMessages.FirstOrDefault(w => w.MessageID == "REQUIRE_DATE" && w.Lang == User.Lang);
                            if (message == null)
                            {
                                return "Message not found.";
                            }

                            if (HeaderEmpLeave.FromDate.Date < requiredDate.Date)
                            {
                                MessageError = _LeaveType.BeforeDay.ToString();
                                return "REQUIRE_DATE";
                                //return string.Format(message.Description, _LeaveType.BeforeDay);
                            }
                        }
                        else
                        {
                            var staff1 = DB.HRStaffProfiles.FirstOrDefault(x => x.EmpCode == HeaderStaff.EmpCode);
                            PRParameter payParam_ = DB.PRParameters.FirstOrDefault(w => w.Code == staff1.PayParam);
                            decimal WHour = Convert.ToDecimal(payParam_.WHOUR) / 2.00M;
                            foreach (var item in ListEmpLeaveD)
                            {
                                double totals = item.EndTime.Value.Subtract(item.StartTime.Value).TotalHours;
                                if ((decimal)totals > WHour)
                                {
                                    var str = "ការស្នើសុំមិនអាចលើសពីកន្លះថ្ងៃ";
                                    str += "\nThe requests cannot be exceeded half day";
                                    return str;
                                }
                            }
                        }
                          
                    }
                }
                string LeaveType = _LeaveType.Description;

                if (HeaderEmpLeave.FromDate.Date > HeaderEmpLeave.ToDate.Date)
                    return "INVALID_DATE";
                DateTime D_Now = DateTime.Now;
                var B_Date = D_Now.AddDays(-Convert.ToDouble(_LeaveType.Beforebackward));
                if (_LeaveType.Allowbackward == false && HeaderEmpLeave.FromDate.Date < D_Now.Date)
                    return "NOT_ALLOW_BACKWARD";
                else if (_LeaveType.Allowbackward == true && HeaderEmpLeave.FromDate.Date < B_Date.Date)
                    return "NOT_ALLOW_BACKWARD";
                if (_LeaveType.ReqDocument == true && _LeaveType.NumDay <= HeaderEmpLeave.NoDay)
                {
                    if (HeaderEmpLeave.Attachment == null)
                        return "REQUIRED_DOCUMENT";
                }
                string Reject = SYDocumentStatus.REJECTED.ToString();
                string Cancel = SYDocumentStatus.CANCELLED.ToString();
                var lstempLeave = DB.HREmpLeaves.Where(w => w.Status != Cancel && w.Status != Reject && w.EmpCode == HeaderStaff.EmpCode 
                                                && w.FromDate.Year >= HeaderEmpLeave.FromDate.Year).ToList();
                HeaderEmpLeave.EmpCode = HeaderStaff.EmpCode;
                var ATSche = DB.ATEmpSchedules.Where(w => w.EmpCode == HeaderEmpLeave.EmpCode
                                                && EntityFunctions.TruncateTime(w.TranDate) >= HeaderEmpLeave.FromDate.Date
                                                && EntityFunctions.TruncateTime(w.TranDate) <= HeaderEmpLeave.ToDate.Date).ToList();
                var leaveH = lstempLeave.ToList();
                if (leaveH.Where(w => ((w.FromDate.Date >= HeaderEmpLeave.FromDate.Date && w.FromDate.Date <= HeaderEmpLeave.ToDate.Date) ||
                   (w.ToDate.Date >= HeaderEmpLeave.FromDate.Date && w.ToDate.Date <= HeaderEmpLeave.ToDate.Date) ||
                         (HeaderEmpLeave.FromDate.Date >= w.FromDate.Date && HeaderEmpLeave.FromDate.Date <= w.ToDate.Date) || (HeaderEmpLeave.ToDate.Date >= w.FromDate.Date && HeaderEmpLeave.ToDate.Date <= w.ToDate.Date))).Any())
                {
                    var EmpLeaveD = DB.HREmpLeaveDs.Where(w => w.EmpCode == HeaderStaff.EmpCode && EntityFunctions.TruncateTime(w.LeaveDate) >= EntityFunctions.TruncateTime(HeaderEmpLeave.FromDate)
                                                    && EntityFunctions.TruncateTime(w.LeaveDate) <= EntityFunctions.TruncateTime(HeaderEmpLeave.ToDate)).ToList();
                    //EmpLeaveD = EmpLeaveD.Where(w => w.LeaveDate.Date >= HeaderEmpLeave.FromDate.Date && w.LeaveDate.Date <= HeaderEmpLeave.ToDate.Date).ToList();
                    var Result = ListEmpLeaveD.Where(w => EmpLeaveD.Where(x => x.LeaveDate.Date == w.LeaveDate.Date && w.Remark != "Hours").Any()).ToList();
                    var Result_ = ListEmpLeaveD.Where(w => EmpLeaveD.Where(x => x.LeaveDate.Date == w.LeaveDate.Date && w.Remark != "FullDay").Any()).ToList();
                    var ResultHour = ListEmpLeaveD.Where(w => EmpLeaveD.Where(x => x.LeaveDate.Date == w.LeaveDate.Date && (x.Remark == "Hours" || w.Remark == "Hours")).Any()).ToList();
                    var empATSche = ATSche.Where(w => ResultHour.Where(x => w.TranDate == x.LeaveDate && w.EmpCode == HeaderStaff.EmpCode).Any()).ToList();
                    if (Result.Where(w => w.Remark == "FullDay").Any())
                        if (ResultHour.Count > 0) return "INV_DATE";
                    if (EmpLeaveD.Where(w => Result.Where(x => x.LeaveDate.Date == w.LeaveDate.Date && x.Remark == w.Remark).Any()).Any()) return "INV_DATE";
                    if (EmpLeaveD.Where(w => ResultHour.Where(x => x.LeaveDate.Date == w.LeaveDate.Date).Any()).Any())
                    {
                        if (empATSche.Count > 0)
                        {
                            foreach (var read in empATSche)
                            {
                                TimeSpan time = new TimeSpan(0, 4, 0, 0);
                                DateTime brackstart = read.IN1.Value.Add(time);
                                DateTime brackend = read.OUT1.Value.Subtract(time);
                                if (read.Flag1 == 1 && read.Flag2 == 1 && ResultHour.Count > 0)
                                {
                                    if (EmpLeaveD.Where(w => Result_.Where(x => x.LeaveDate.Date == read.TranDate.Date && ((read.IN1 >= w.StartTime && read.IN1 <= w.EndTime)
                                                        || (read.OUT1 >= w.StartTime && read.OUT1 <= w.EndTime) || (w.StartTime >= read.IN1 && w.StartTime <= read.OUT1)
                                                        || (w.EndTime >= read.IN1 && w.EndTime <= read.OUT1)) && x.Remark == "Morning").Any()).Any()) return "INV_DATE";
                                    else if (EmpLeaveD.Where(w => Result_.Where(x => x.LeaveDate.Date == read.TranDate.Date && ((read.IN2 >= w.StartTime && read.IN2 <= w.EndTime)
                                                        || (read.OUT2 >= w.StartTime && read.OUT2 <= w.EndTime) || (w.StartTime >= read.IN2 && w.StartTime <= read.OUT2)
                                                        || (w.EndTime >= read.IN2 && w.EndTime <= read.OUT2)) && x.Remark == "Afternoon").Any()).Any()) return "INV_DATE";
                                }
                                else if (read.Flag1 == 1 && read.Flag2 == 2 && ResultHour.Count > 0)
                                {
                                    if (EmpLeaveD.Where(w => Result_.Where(x => x.LeaveDate.Date == read.TranDate.Date && ((read.IN1 >= w.StartTime && read.IN1 <= w.EndTime)
                                                        || (brackstart >= w.StartTime && brackstart <= w.EndTime) || (w.StartTime >= read.IN1 && w.StartTime <= brackstart)
                                                        || (w.EndTime >= read.IN1 && w.EndTime <= brackstart)) && x.Remark == "Morning").Any()).Any()) return "INV_DATE";
                                    else if (EmpLeaveD.Where(w => Result_.Where(x => x.LeaveDate.Date == read.TranDate.Date && ((brackend >= w.StartTime && brackend <= w.EndTime)
                                                        || (read.OUT2 >= w.StartTime && read.OUT2 <= w.EndTime) || (w.StartTime >= brackend && w.StartTime <= read.OUT2)
                                                        || (w.EndTime >= brackend && w.EndTime <= read.OUT2)) && x.Remark == "Afternoon").Any()).Any()) return "INV_DATE";
                                }
                            }
                        }
                    }
                    if (EmpLeaveD.Where(w => w.Remark == "FullDay").Any())
                        if (ResultHour.Count > 0) return "INV_DATE";
                    if (ResultHour.Count > 0)
                    {
                        var _EmpLeaveItem = EmpLeaveD.Where(w => w.Remark == "Hours").ToList();
                        if (_EmpLeaveItem.Any())
                        {
                            foreach (var item in ResultHour)
                            {
                                if (_EmpLeaveItem.Where(w => w.StartTime <= item.StartTime.Value.AddMinutes(1) && w.EndTime >= item.StartTime.Value.AddMinutes(1)).Any())
                                    return "INV_DATE";
                                else if (_EmpLeaveItem.Where(w => w.StartTime <= item.EndTime.Value.AddMinutes(1) && w.EndTime >= item.EndTime.Value.AddMinutes(1)).Any())
                                    return "INV_DATE";
                            }
                        }
                    }
                    if (ResultHour.Count > 0)
                    {
                        if (EmpLeaveD.Where(w => w.Remark == "FullDay").Any()) return "INV_DATE";
                        if (Result.Where(w => w.Remark == "FullDay").Any()) return "INV_DATE";
                    }
                }
                if (HeaderEmpLeave.Reason == "" || HeaderEmpLeave.Reason == null)
                    return "REASON";
                if (ListEmpLeaveD.Count <= 0)
                    return "INV_DOC";
                var staff = DB.HRStaffProfiles.FirstOrDefault(x => x.EmpCode == HeaderStaff.EmpCode);
                if (_LeaveType != null)
                {
                    if (_LeaveType.Probation == true)
                    {
                        if (staff.Probation.Value.Date > HeaderEmpLeave.FromDate.Date)
                            return "CANNOT_PROBATION";
                    }
                }
                getLeaveDay(HeaderStaff.EmpCode);
                HeaderEmpLeave.EmpCode = HeaderStaff.EmpCode;
                PRParameter payParam = DB.PRParameters.FirstOrDefault(w => w.Code == staff.PayParam);
                if (_LeaveType.IsCurrent == false)
                {
                    ClsPeriodLeave periodLeave = new ClsPeriodLeave();
                    var ListLeave_Rate = DB.HRLeaveProRates.ToList();
                    List<HRSetEntitleD> ListetEntitleD = DB.HRSetEntitleDs.Where(w => w.CodeH == staff.TemLeave).ToList();
                    periodLeave.payParam = payParam;
                    periodLeave.ListLeaveProRate = ListLeave_Rate;
                    periodLeave = GetPeriod(periodLeave, staff, ListetEntitleD, HeaderEmpLeave.ToDate.Year, HeaderEmpLeave.LeaveCode);
                    if (periodLeave.Balance - Convert.ToDecimal(HeaderEmpLeave.NoDay) < 0)
                        return "INV_BALANCE";
                }

                if (_LeaveType.IsOverEntitle == false)
                {
                    int year = HeaderEmpLeave.ToDate.Year;
                    List<HREmpLeaveD> _ListLeaveD = GetLeaveToken(year).ToList();
                    _ListLeaveD = _ListLeaveD.Where(w => w.LeaveDate.Year == year && w.EmpCode == HeaderStaff.EmpCode).ToList();
                    var EmpLeaveB = DB.HREmpLeaveBs.FirstOrDefault(w => w.InYear == year && w.LeaveCode == HeaderEmpLeave.LeaveCode && w.EmpCode == HeaderStaff.EmpCode);

                    if (balance - Convert.ToDecimal(HeaderEmpLeave.NoDay) < 0)
                    {
                        _ListLeaveD.AddRange(GetLeave_D(ListEmpLeaveD, HeaderStaff.EmpCode, HeaderEmpLeave.LeaveCode, HeaderEmpLeave.Units));
                        var ListLeaveType = DB.HRLeaveTypes.ToList();
                        var approved = SYDocumentStatus.APPROVED.ToString();
                        List<HRClaimLeave> LstEmpClaim = DB.HRClaimLeaves.Where(w => w.Status == approved && w.WorkingDate.Year == year && (w.IsUsed.Value == true || w.IsExpired.Value == false)).ToList();
                        var EmpClaim = LstEmpClaim.Where(w => w.EmpCode == HeaderEmpLeave.EmpCode).ToList();
                        HREmpLeaveB lB = Calculate_Token(EmpLeaveB, _ListLeaveD, ListLeaveType, payParam, EmpLeaveB.YTD.Value, LstEmpClaim);
                        if ((EmpLeaveB.YTD - lB.Balance) < 0)
                            return "INT_Entile";
                    }
                }
                SetAutoApproval(staff.EmpCode, staff.Branch, HeaderEmpLeave.ToDate.Date);
                if (ListApproval.Count() <= 0)
                    return "NO_LINE_MN";
                var ListWorkFlow = DB.HRWorkFlowLeaves.ToList();
                string Status = SYDocumentStatus.PENDING.ToString();

                if (ListWorkFlow.Count > 0)
                {
                    ListWorkFlow = ListWorkFlow.OrderBy(w => w.Step).ToList();
                    var result = ListWorkFlow.OrderBy(w => w.Step);
                    Status = result.Select(w => w.Code).First().ToString();
                }
                int Increment = GenerateLeaveObject.GetLastIncrement();

                if (staff.IsAutoAppLeave == true)
                    Status = SYDocumentStatus.APPROVED.ToString();
                HeaderEmpLeave.Increment = Increment + 1;
                HeaderEmpLeave.Status = Status;
                HeaderEmpLeave.RequestDate = DateTime.Now;
                HeaderEmpLeave.CreateBy = User.UserName;
                HeaderEmpLeave.CreateOn = DateTime.Now;
                DB.HREmpLeaves.Add(HeaderEmpLeave);

                DB.HREmpLeaveDs.AddRange(GetLeave_D(ListEmpLeaveD, HeaderStaff.EmpCode, HeaderEmpLeave.LeaveCode, HeaderEmpLeave.Units));

                //Add approver
                foreach (var read in ListApproval)
                {
                    read.ID = 0;
                    read.LastChangedDate = DateTime.Now;
                    read.DocumentNo = Convert.ToString(Increment + 1);
                    read.Status = SYDocumentStatus.OPEN.ToString();
                    read.ApprovedBy = "";
                    read.ApprovedName = "";
                    DB.ExDocApprovals.Add(read);
                }
                SYHRAnnouncement _announ = new SYHRAnnouncement();
                if (ListApproval.Count() > 0)
                {
                    _announ.Type = "LeaveRequest";
                    _announ.Subject = staff.AllName;
                    _announ.Description = @"Leave type of " + _LeaveType.Description +
                        " from " + HeaderEmpLeave.FromDate.ToString("yyyy.MM.dd") + " to " + HeaderEmpLeave.ToDate.ToString("yyyy.MM.dd") + " My Reason: " + HeaderEmpLeave.Reason;
                    _announ.DocumentNo = HeaderEmpLeave.Increment.ToString();
                    _announ.DocumentDate = DateTime.Now;
                    _announ.IsRead = false;
                    _announ.UserName = ListApproval.First().Approver;
                    _announ.CreatedBy = User.UserName;
                    _announ.CreatedOn = DateTime.Now;
                    DB.SYHRAnnouncements.Add(_announ);
                }

                int row = DB.SaveChanges();
                URL += HeaderEmpLeave.TranNo;
                HR_VIEW_EmpLeave EmpLeave = new HR_VIEW_EmpLeave();
                EmpLeave = DBV.HR_VIEW_EmpLeave.FirstOrDefault(w => w.TranNo == HeaderEmpLeave.TranNo);

                #region Send Email
                SYWorkFlowEmailObject wfo =
                           new SYWorkFlowEmailObject("ESSLR", WorkFlowType.REQUESTER,
                                UserType.N, SYDocumentStatus.PENDING.ToString());
                wfo.SelectListItem = new SYSplitItem(Convert.ToString(Increment + 1));
                wfo.User = User;
                wfo.BS = BS;
                wfo.UrlView = SYUrl.getBaseUrl();
                wfo.ScreenId = ScreenId;
                wfo.Module = "HR";// CModule.PURCHASE.ToString();
                wfo.ListLineRef = new List<BSWorkAssign>();
                wfo.DocNo = HeaderEmpLeave.TranNo.ToString();
                wfo.Action = SYDocumentStatus.PENDING.ToString();
                wfo.ObjectDictionary = HeaderEmpLeave;
                wfo.ListObjectDictionary = new List<object>();
                wfo.ListObjectDictionary.Add(EmpLeave);
                HRStaffProfile Staff = getNextApprover(Convert.ToString(Increment + 1), "");
                wfo.ListObjectDictionary.Add(Staff);
                if (!string.IsNullOrEmpty(Staff.Email))
                {
                    WorkFlowResult result1 = wfo.InsertProcessWorkFlowLeave(Staff);
                    //MessageError = wfo.getErrorMessage(result1);
                }
                #endregion
                var LeaveBalance = DB.HREmpLeaveBs.FirstOrDefault(w => w.EmpCode == HeaderEmpLeave.EmpCode
               && w.InYear == HeaderEmpLeave.ToDate.Year && w.LeaveCode == HeaderEmpLeave.LeaveCode);

                decimal? _Balance = 0;
                if (LeaveBalance != null)
                {
                    if (LeaveBalance.DayEntitle > 0)
                        _Balance = LeaveBalance.Balance - Convert.ToDecimal(HeaderEmpLeave.NoDay);
                }

                #region ---Send To Telegram---
                //string Urgent = "";
                //if (HeaderEmpLeave.Urgent == true) Urgent = "<b style=\"color: red\">Urgent</b>%0A%0A";
                var EmailTemplate = DP.TPEmailTemplates.Find("TELEGRAM");
                if (EmailTemplate != null && !string.IsNullOrEmpty(staff.TeleGroup))
                {
                    string strComp = "";
                    string text = "";
                    var staff_ = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == User.UserName);
                    var _Company = DP.SYHRCompanies.FirstOrDefault(w => w.CompanyCode == staff_.CompanyCode);
                    if (_Company != null && payParam != null)
                    {
                        strComp = _Company.CompanyCode;
                        text = GetCconMessage(EmpLeave, ListEmpLeaveD, Staff, HeaderEmpLeave.Units, HeaderStaff.Phone1, URL, _Balance, payParam.WHOUR);
                    }

                    SYSendTelegramObject Tel = new SYSendTelegramObject();
                    List<object> ListObjectDictionary = new List<object>();
                    ListObjectDictionary.Add(EmpLeave);
                    ListObjectDictionary.Add(Staff);
                    Tel.Send_SMS_Telegrams(EmailTemplate.RequestContent, staff.TeleGroup, ListObjectDictionary, URL, strComp, text);
                }
                #endregion
                #region ---Telegram alert to Line Manager---
                var EmailTemplate1 = DP.TPEmailTemplates.Find("TELEGRAM");
                var HOD = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == staff.FirstLine);
                if (EmailTemplate1 != null && !string.IsNullOrEmpty(HOD.TeleChartID))
                {
                    SYSendTelegramObject Tel = new SYSendTelegramObject();
                    Tel.User = User;
                    Tel.BS = BS;
                    List<object> ListObjectDictionary = new List<object>();
                    ListObjectDictionary.Add(EmpLeave);
                    ListObjectDictionary.Add(Staff);
                    if (Staff.TeleChartID != null && Staff.TeleChartID != "")
                    {
                        WorkFlowResult result2 = Tel.Send_SMS_Telegram("TELEGRAM", EmailTemplate1.RequestContent, HOD.TeleChartID, ListObjectDictionary, URL);
                        MessageError = Tel.getErrorMessage(result2);
                    }
                }
                #endregion

                #region Notifican on Mobile
                var access = DB.TokenResources.FirstOrDefault(w => w.UserName == _announ.UserName);
                if (access != null)
                {
                    if (!string.IsNullOrEmpty(access.FirebaseID))
                    {
                        string Desc = staff.AllName + @" to request Leave type of " + _LeaveType.Description +
                            " from " + HeaderEmpLeave.FromDate.ToString("yyyy.MM.dd") + " to " + HeaderEmpLeave.ToDate.ToString("yyyy.MM.dd");
                        Notification.Notificationf Noti = new Notification.Notificationf();
                        var clientToken = new List<string>();
                        clientToken.Add(access.FirebaseID);
                        //clientToken.Add("d7Xt0qR7JkfnnLKGf4xCw2:APA91bHfJMAlQRQlZDwDqG9h8FQfbf8lEijFo4zlzI1i17tEVhZVT7lzTAy3q7ePb7vbgok5bxJWQjdSgBM37NKkSQ_mYnsQInV7ZmRHyVOmM6xektGYp0e8AhGSulzpZZnhvuR19v32");
                        var dd = Noti.SendNotification(clientToken, "LeaveRequest", Desc, fileName);
                    }
                }
                #endregion
                if (HeaderEmpLeave.Status == SYDocumentStatus.APPROVED.ToString())
                {
                    ReGenerateLeaveToken(HeaderEmpLeave.EmpCode, HeaderEmpLeave.FromDate, HeaderEmpLeave.ToDate);
                    var ListAtt = DB.ATEmpSchedules.Where(w => w.EmpCode == HeaderEmpLeave.EmpCode
                                  && EntityFunctions.TruncateTime(w.TranDate) >= HeaderEmpLeave.FromDate.Date
                                  && EntityFunctions.TruncateTime(w.TranDate) <= HeaderEmpLeave.ToDate.Date).ToList();
                    foreach (var item in ListAtt)
                    {
                        item.LeaveDesc = "";
                        item.LeaveCode = HeaderEmpLeave.LeaveCode;
                        item.LeaveNo = HeaderEmpLeave.Increment;
                        DBI.ATEmpSchedules.Attach(item);
                        DBI.Entry(item).Property(w => w.LeaveDesc).IsModified = true;
                        DBI.Entry(item).Property(w => w.LeaveCode).IsModified = true;
                        DBI.Entry(item).Property(w => w.LeaveNo).IsModified = true;
                    }
                    DBI.SaveChanges();
                }
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserID.ToString();
                log.ScreenId = HeaderEmpLeave.EmpCode;
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string AssetBusinessClosures(string EmpCode, DateTime FromDate, DateTime ToDate, string Remark, string LeaveType, string TimeShift)
        {
            string ErrorCode = "";
            try
            {
                var _LeaveType = DB.HRLeaveTypes.Find(LeaveType);
                if (_LeaveType == null)
                {
                    return "INV_DOC";
                }
                if (FromDate.Date > ToDate.Date)
                {
                    return "INVALID_DATE";
                }
                string[] Emp = EmpCode.Split(';');
                //List<HRStaffProfile> ListStaffPro = DB.HRStaffProfiles.ToList();
                List<PRParameter> _listParam = DB.PRParameters.ToList();
                string Units = "Day";
                int Increment = GetLastIncrement();
                foreach (var Code in Emp)
                {
                    ErrorCode = Code;
                    HRStaffProfile Staff = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == Code);
                    if (Staff == null) continue;
                    PRParameter _param = _listParam.FirstOrDefault(w => w.Code == Staff.PAEMAIL);
                    decimal WHOUR = 8;
                    if (_param != null) WHOUR = (decimal)_param.WHOUR;
                    HeaderEmpLeave = new HREmpLeave();
                    HeaderEmpLeave.EmpCode = Staff.EmpCode;
                    HeaderEmpLeave.FromDate = FromDate;
                    HeaderEmpLeave.ToDate = ToDate;
                    HeaderEmpLeave.LeaveCode = LeaveType;
                    HeaderEmpLeave.Units = Units;
                    HeaderEmpLeave.Status = SYDocumentStatus.APPROVED.ToString();
                    HeaderEmpLeave.Increment = Increment + 1;
                    HeaderEmpLeave.RequestDate = DateTime.Now;
                    HeaderEmpLeave.Reason = Remark;
                    HeaderEmpLeave.CreateBy = User.UserName;
                    HeaderEmpLeave.CreateOn = DateTime.Now;
                    int Line = 0;
                    decimal NoDay = 0;
                    for (DateTime date = FromDate; date <= ToDate; date = date.AddDays(1))
                    {
                        Line += 1;
                        HREmpLeaveD Obj = new HREmpLeaveD();
                        Obj.LeaveTranNo = Increment + 1;
                        Obj.EmpCode = Staff.EmpCode;
                        Obj.LeaveCode = LeaveType;
                        Obj.CutMonth = date;
                        Obj.LeaveDate = date;
                        Obj.Status = "Leave";
                        Obj.Remark = TimeShift;
                        Obj.LHour = WHOUR;
                        if (Obj.Remark == "Morning" || Obj.Remark == "Afternoon")
                        {
                            Obj.LHour = WHOUR / 2;
                        }
                        Obj.LineItem = Line;
                        NoDay += (decimal)(Obj.LHour / WHOUR);
                        DB.HREmpLeaveDs.Add(Obj);
                    }
                    HeaderEmpLeave.NoDay = (double)NoDay;
                    HeaderEmpLeave.NoPH = 0;
                    HeaderEmpLeave.NoRest = 0;
                    DB.HREmpLeaves.Add(HeaderEmpLeave);
                    Increment += 1;
                }
                DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ErrorCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (DbUpdateException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ErrorCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (Exception e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ErrorCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string approveTheDoc(string id, string URL, string fileName)
        {
            try
            {
                DB = new HumicaDBContext();
                int TranNo = Convert.ToInt32(id);
                var objMatch = DB.HREmpLeaves.Find(TranNo);
                if (objMatch == null)
                {
                    return "REQUEST_NE";
                }
                var _LeaveType = DB.HRLeaveTypes.Find(objMatch.LeaveCode);
                string open = SYDocumentStatus.OPEN.ToString();
                string DocNo = objMatch.Increment.ToString();
                var listApproval = DB.ExDocApprovals
                    .Where(w => w.DocumentType == "LR" && w.DocumentNo == DocNo)
                    .OrderBy(w => w.ApproveLevel)
                    .ToList();
                var listUser = DB.HRStaffProfiles
                    .Where(w => w.EmpCode == User.UserName)
                    .ToList();
                var b = false;
                if (listApproval.Count == 0)
                {
                    return "RESTRICT_ACCESS";
                }
                var APPROVED = SYDocumentStatus.APPROVED.ToString();
                var REJECTED = SYDocumentStatus.REJECTED.ToString();
                var CANCELLED = SYDocumentStatus.CANCELLED.ToString();
                var approver = "";
                foreach (var read in listApproval)
                {
                    if (listApproval.Where(w => w.ApproveLevel == read.ApproveLevel
                        && (w.Status == APPROVED || w.Status == REJECTED || w.Status == CANCELLED)).Any())
                        continue;
                    var checklist = listUser.Where(w => w.EmpCode == read.Approver).ToList();
                    if (checklist.Count > 0)
                    {
                        if (read.Status == SYDocumentStatus.APPROVED.ToString())
                        {
                            return "USER_ALREADY_APP";
                        }

                        if (read.ApproveLevel > listApproval.Where(w => w.Status == open).Min(w => w.ApproveLevel))
                        {
                            return "REQUIRED_PRE_LEVEL";
                        }
                        var objStaff = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == read.Approver);
                        if (objStaff != null)
                        {
                            foreach (var item in listApproval.Where(w => w.ApproveLevel == read.ApproveLevel))
                            {
                                item.ApprovedBy = objStaff.EmpCode;
                                item.ApprovedName = objStaff.AllName;
                                item.LastChangedDate = DateTime.Now.Date;
                                item.ApprovedDate = DateTime.Now;
                                item.Status = SYDocumentStatus.APPROVED.ToString();
                                DB.ExDocApprovals.Attach(item);
                                DB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                approver = objStaff.EmpCode;
                            }
                            b = true;
                            break;
                        }
                    }
                }
                if (listApproval.Count > 0)
                {
                    if (b == false)
                    {
                        return "USER_NOT_APPROVOR";
                    }
                }
                var status = SYDocumentStatus.APPROVED.ToString();
                //var open = SYDocumentStatus.OPEN.ToString();
                // objMatch.IsApproved = true;
                if (listApproval.Where(w => w.Status == open).ToList().Count > 0)
                {
                    status = SYDocumentStatus.PENDING.ToString();
                }

                objMatch.Status = status;
                DB.HREmpLeaves.Attach(objMatch);
                DB.Entry(objMatch).Property(w => w.Status).IsModified = true;

                //Update Leave in Att
                var ListAtt = DB.ATEmpSchedules.Where(w => w.EmpCode == objMatch.EmpCode
                && EntityFunctions.TruncateTime(w.TranDate) >= objMatch.FromDate.Date
                && EntityFunctions.TruncateTime(w.TranDate) <= objMatch.ToDate.Date).ToList();
                foreach (var item in ListAtt)
                {
                    item.LeaveDesc = "";
                    item.LeaveCode = objMatch.LeaveCode;
                    item.LeaveNo = objMatch.Increment;
                    DB.ATEmpSchedules.Attach(item);
                    DB.Entry(item).Property(w => w.LeaveDesc).IsModified = true;
                    DB.Entry(item).Property(w => w.LeaveCode).IsModified = true;
                    DB.Entry(item).Property(w => w.LeaveNo).IsModified = true;
                }

                SYHRAnnouncement _announ = new SYHRAnnouncement();
                var _Staff = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == objMatch.EmpCode);
                var _Lstapp = listApproval.Where(w => w.Status == SYDocumentStatus.OPEN.ToString()).ToList();
                _announ.Type = "LeaveRequest";
                if (_Lstapp.Count() > 0)
                {
                    var min = _Lstapp.Min(w => w.ApproveLevel);
                    _announ.Subject = _Staff.AllName;
                    _announ.UserName = _Lstapp.FirstOrDefault(w => w.ApproveLevel == min).Approver;
                    _announ.Description = @"Leave type of " + _LeaveType.Description +
                        " from " + objMatch.FromDate.ToString("yyyy.MM.dd") + " to " + objMatch.ToDate.ToString("yyyy.MM.dd") + " My Reason: " + objMatch.Reason;
                }
                if (status == SYDocumentStatus.APPROVED.ToString())
                {
                    _announ.Type = "LeaveApproved";
                    _announ.Subject = "Approved";
                    _announ.UserName = objMatch.EmpCode;
                    _announ.Description = "Leave type of " + _LeaveType.Description;
                }
                if (!string.IsNullOrEmpty(_announ.Description))
                {
                    _announ.DocumentNo = objMatch.Increment.ToString();
                    _announ.DocumentDate = DateTime.Now;
                    _announ.IsRead = false;
                    _announ.CreatedBy = User.UserName;
                    _announ.CreatedOn = DateTime.Now;
                    DB.SYHRAnnouncements.Add(_announ);
                }
              
                string Approval = SYDocumentStatus.APPROVED.ToString();
                List<HRClaimLeave> _listClaim = DB.HRClaimLeaves.Where(w => w.EmpCode == objMatch.EmpCode
                && (w.IsExpired.Value != true || w.IsUsed.Value != true) && w.Status == Approval).ToList();
                DateTime DateNow = DateTime.Now;
                bool Isused = false;
                foreach (var claim in _listClaim.ToList().OrderBy(w => w.WorkingDate))
                {
                    if (Isused == true) continue;
                    if (claim.Expired.Value.Date < DateNow.Date)
                    {
                        claim.IsExpired = true;
                        DB.HRClaimLeaves.Attach(claim);
                        DB.Entry(claim).Property(x => x.IsExpired).IsModified = true;
                    }
                    else
                    {
                        Isused = true;
                        claim.IsUsed = true;
                        claim.DocumentRef = objMatch.Increment.ToString();
                        DB.HRClaimLeaves.Attach(claim);
                        DB.Entry(claim).Property(x => x.IsUsed).IsModified = true;
                        DB.Entry(claim).Property(x => x.DocumentRef).IsModified = true;
                    }
                }

                int row = DB.SaveChanges();
                DBV = new HumicaDBViewContext();
                if (objMatch.Status == SYDocumentStatus.APPROVED.ToString())
                {
                    ReGenerateLeaveToken(objMatch.EmpCode, objMatch.FromDate, objMatch.ToDate);
                    #region Send Email
                    HR_VIEW_EmpLeave EmpLeave = new HR_VIEW_EmpLeave();
                    EmpLeave = DBV.HR_VIEW_EmpLeave.FirstOrDefault(w => w.TranNo == objMatch.TranNo);
                    SYWorkFlowEmailObject wfo =
                               new SYWorkFlowEmailObject("ESSLA", WorkFlowType.REQUESTER,
                                    UserType.N, SYDocumentStatus.PENDING.ToString());
                    wfo.SelectListItem = new SYSplitItem(Convert.ToString(DocNo));
                    wfo.User = User;
                    wfo.BS = BS;
                    wfo.UrlView = URL;
                    wfo.ScreenId = ScreenId;
                    wfo.Module = "HR";// CModule.PURCHASE.ToString();
                    wfo.DocNo = objMatch.TranNo.ToString();
                    wfo.ListLineRef = new List<BSWorkAssign>();
                    wfo.Action = SYDocumentStatus.PENDING.ToString();
                    wfo.ObjectDictionary = HeaderEmpLeave;
                    wfo.ListObjectDictionary = new List<object>();
                    wfo.ListObjectDictionary.Add(EmpLeave);
                    HRStaffProfile Staff = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == EmpLeave.EmpCode);
                    if (Staff.Email != null && Staff.Email != "")
                    {
                        wfo.ListObjectDictionary.Add(Staff);
                        WorkFlowResult result1 = wfo.InsertProcessWorkFlowLeave(Staff);
                        MessageError = wfo.getErrorMessage(result1);
                    }
                    #endregion

                    #region *****Send to Telegram
                    if (Staff.TeleGroup != null && Staff.TeleGroup != "")
                    {
                        Humica.Core.SY.SYSendTelegramObject wfo1 =
                          new Humica.Core.SY.SYSendTelegramObject("ESSLA", WorkFlowType.APPROVER, objMatch.Status);
                        wfo1.User = User;
                        wfo1.ListObjectDictionary = new List<object>();
                        wfo1.ListObjectDictionary.Add(EmpLeave);
                        wfo1.ListObjectDictionary.Add(Staff);
                        #region Tembody
                            var EMP= DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == approver);
                            var ObjectStaff= DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == objMatch.EmpCode);
                            var Handover= DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == objMatch.TaskHand_Over);
                            var leaveTypeKH = DB.HRLeaveTypes.FirstOrDefault(w => w.Code == objMatch.LeaveCode);
                        var CurrentAL = DB.HREmpLeaveBs.FirstOrDefault(w => w.LeaveCode == objMatch.LeaveCode && w.EmpCode == objMatch.EmpCode && w.InYear == objMatch.FromDate.Year);
                            var ListLeave = DB.HREmpLeaveDs.Where(w => w.LeaveTranNo == objMatch.Increment && w.EmpCode== objMatch.EmpCode).ToList();
                            var title="";
                            if (EMP != null)
                            {
                                title = EMP.Title;
                            }
                            string str = "កម្មវត្ថុ៖ Leave Approval"; 
                            if (title == "Ms")
                            {
                                title = "Miss";
                            }
                            str += "<b>%0Aអនុម័តដោយ៖</b> " + title + (". ") + EMP.AllName + "(" + EMP.EmpCode + ")";
                            str += "<b>%0Aទៅកាន់៖</b> " + ObjectStaff.Title + (". ") + ObjectStaff.AllName + "(" + ObjectStaff.EmpCode + ")";
                            str += "<b>%0Aកាលបរិច្ឆេទ៖</b> " + objMatch.CreateOn.Value.Date.ToString("dd-MMM-yyyy");
                            str += "<b>%0Aប្រភេទសុំច្បាប់៖</b> " + leaveTypeKH.Description;
                            str += "<b>%0Aសមតុល្យ៖</b> " + CurrentAL.CurrentEntitle;
                            str += "<b>%0Aកាលបរិច្ឆេទឈប់សម្រាក៖</b> " + objMatch.FromDate.ToString("dd-MMM-yyyy") + " - "+objMatch.ToDate.Date.ToString("dd-MMM-yyyy");
                            foreach(var item in ListLeave)
                            {
                                if(item.LeaveCode!= "ML")
                                {
                                    if (item.Remark == "Hours")
                                    {
                                        item.Remark = item.LHour + " ម៉ោង";
                                    }
                                    else if (item.Remark == "FullDay")
                                    {
                                        item.Remark = "ពេញមួយថ្ងៃ";
                                    }
                                    else if (item.Remark == "Morning")
                                    {
                                        item.Remark = "ពេលព្រឹក";
                                    }
                                    else
                                    {
                                        item.Remark = "ពេលរសៀល";
                                    }
                                    str += "%0A" + item.LeaveDate.Date.ToString("dd-MMM-yyyy") + ":" + item.Remark;
                                }
                            
                            }
                            str += "<b>%0Aចំនួនថ្ងៃបានឈប់៖</b> " +objMatch.NoDay;
                            str += "<b>%0Aមូលហេតុ៖</b> " +objMatch.Reason;
                        //str += "<b>%0Aផ្ទេរសិទ្ធិជួន៖</b> " + Handover.Title + (". ") + Handover.AllName + "(" + Handover.EmpCode + ")";
                            if (Handover != null)
                            {
                                str += "<b>%0Aផ្ទេរសិទ្ធិជួន៖</b> " + Handover.EmpCode + (": ") + Handover.AllName;
                            }
                            

                            #endregion
                        wfo1.Send_SMS_TelegramLeaveApproval(Staff.TeleGroup, "",str);
                    }
                    #endregion
                    //Alert to HR

                    var EmailTemplateCC = DP.TPEmailTemplates.Find("ESSLEAVE_TELECC_HR");
                    var Sett = DB.SYHRSettings.First();
                    if (EmailTemplateCC != null && !string.IsNullOrEmpty(Sett.TelegLeave))
                    {
                        SYSendTelegramObject Tel = new SYSendTelegramObject();
                        Tel.User = User;
                        Tel.BS = BS;
                        List<object> ListObjectDictionary = new List<object>();
                        ListObjectDictionary.Add(EmpLeave);
                        ListObjectDictionary.Add(Staff);
                        WorkFlowResult result2 = Tel.Send_SMS_Telegram("ESSLEAVE_TELECC_HR", EmailTemplateCC.RequestContent, Sett.TelegLeave, ListObjectDictionary, URL);
                        MessageError = Tel.getErrorMessage(result2);
                    }
                    //Alert to HOD
                    var EmailTemplate_HOD = DP.TPEmailTemplates.Find("ESSLEAVE_TELECC_HOD");
                    if (EmailTemplate_HOD != null)
                    {
                        var HOD = DBV.HR_STAFF_VIEW.FirstOrDefault(w => w.EmpCode == Staff.HODCode);
                        if (HOD != null && !string.IsNullOrEmpty(HOD.TeleChartID))
                        {
                            SYSendTelegramObject Tel = new SYSendTelegramObject();
                            Tel.User = User;
                            Tel.BS = BS;
                            List<object> ListObjectDictionary = new List<object>();
                            ListObjectDictionary.Add(EmpLeave);
                            ListObjectDictionary.Add(Staff);
                            ListObjectDictionary.Add(HOD);
                            WorkFlowResult result2 = Tel.Send_SMS_Telegram(EmailTemplate_HOD.EMTemplateObject, EmailTemplate_HOD.RequestContent, HOD.TeleChartID, ListObjectDictionary, URL);
                            MessageError = Tel.getErrorMessage(result2);
                        }
                    }
                    //Alert to Requester 
                    var Template_Req = DP.TPEmailTemplates.Find("TAPPLEAVE");
                    if (Template_Req != null)
                    {
                        if (!string.IsNullOrEmpty(Staff.TeleChartID))
                        {
                            SYSendTelegramObject Tel = new SYSendTelegramObject();
                            Tel.User = User;
                            Tel.BS = BS;
                            List<object> ListObjectDictionary = new List<object>();
                            ListObjectDictionary.Add(EmpLeave);
                            ListObjectDictionary.Add(Staff);
                            WorkFlowResult result2 = Tel.Send_SMS_Telegram(Template_Req.EMTemplateObject, Template_Req.RequestContent, Staff.TeleChartID, ListObjectDictionary, URL);
                            MessageError = Tel.getErrorMessage(result2);
                        }
                    }

                    #region Notifican on Mobile
                    var access = DB.TokenResources.FirstOrDefault(w => w.UserName == _Staff.EmpCode);
                    if (access != null)
                    {
                        if (!string.IsNullOrEmpty(access.FirebaseID))
                        {
                            string Desc = _announ.Description;
                            Notification.Notificationf Noti = new Notification.Notificationf();
                            var clientToken = new List<string>();
                            clientToken.Add(access.FirebaseID);
                            //clientToken.Add("d7Xt0qR7JkfnnLKGf4xCw2:APA91bHfJMAlQRQlZDwDqG9h8FQfbf8lEijFo4zlzI1i17tEVhZVT7lzTAy3q7ePb7vbgok5bxJWQjdSgBM37NKkSQ_mYnsQInV7ZmRHyVOmM6xektGYp0e8AhGSulzpZZnhvuR19v32");
                            var dd = Noti.SendNotification(clientToken, "LeaveApproved", Desc, fileName);
                        }
                    }
                    #endregion
                }
                else
                {
                    #region Send Email
                    HR_VIEW_EmpLeave EmpLeave = new HR_VIEW_EmpLeave();
                    EmpLeave = DBV.HR_VIEW_EmpLeave.FirstOrDefault(w => w.TranNo == objMatch.TranNo);
                    SYWorkFlowEmailObject wfo =
                               new SYWorkFlowEmailObject("ESSLR", WorkFlowType.REQUESTER,
                                    UserType.N, SYDocumentStatus.PENDING.ToString());
                    wfo.SelectListItem = new SYSplitItem(Convert.ToString(DocNo));
                    wfo.User = User;
                    wfo.BS = BS;
                    wfo.UrlView = URL;
                    wfo.ScreenId = ScreenId;
                    wfo.Module = "HR";// CModule.PURCHASE.ToString();
                    wfo.ListLineRef = new List<BSWorkAssign>();
                    wfo.Action = SYDocumentStatus.PENDING.ToString();
                    wfo.ObjectDictionary = HeaderEmpLeave;
                    wfo.ListObjectDictionary = new List<object>();
                    wfo.ListObjectDictionary.Add(EmpLeave);
                    HRStaffProfile Staff = getNextApprover(DocNo, "");
                    if (!string.IsNullOrEmpty(Staff.Email))
                    {
                        wfo.ListObjectDictionary.Add(Staff);
                        WorkFlowResult result1 = wfo.InsertProcessWorkFlowLeave(Staff);
                        MessageError = wfo.getErrorMessage(result1);
                    }
                    #endregion

                }
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = id;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (DbUpdateException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = id;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (Exception e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = id;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string ApproveHRLeave(string ApprovalID, string URL)
        {
            HumicaDBContext DBI = new HumicaDBContext();
            try
            {
                DBI.Configuration.AutoDetectChangesEnabled = false;
                try
                {
                    string[] c = ApprovalID.Split(';');
                    foreach (var r in c)
                    {
                        if (r == "") continue;
                        //var ListStaff = DBI.HRStaffProfiles.ToList();
                        string approved = SYDocumentStatus.APPROVED.ToString();
                        int TranNo = Convert.ToInt32(r);
                        var objmatch = DBI.HREmpLeaves.Find(TranNo);
                        //var ObjLeaveB = DBI.HREmpLeaveBs;
                        if (objmatch == null)
                            return "INV_EN";
                        objmatch.Status = approved;
                        objmatch.ChangedBy = User.UserName;
                        objmatch.ChangedOn = DateTime.Now;
                        DBI.HREmpLeaves.Attach(objmatch);
                        DBI.Entry(objmatch).Property(w => w.RejectDate).IsModified = true;
                        DBI.Entry(objmatch).Property(w => w.Status).IsModified = true;
                        DBI.Entry(objmatch).Property(w => w.ChangedBy).IsModified = true;
                        DBI.Entry(objmatch).Property(w => w.ChangedOn).IsModified = true;
                        //Update Leave in Att
                        var ListAtt = DB.ATEmpSchedules.Where(w => w.EmpCode == objmatch.EmpCode
                                                        && EntityFunctions.TruncateTime(w.TranDate) >= objmatch.FromDate.Date
                                                        && EntityFunctions.TruncateTime(w.TranDate) <= objmatch.ToDate.Date).ToList();
                        foreach (var item in ListAtt)
                        {
                            item.LeaveDesc = "";
                            item.LeaveCode = objmatch.LeaveCode;
                            item.LeaveNo = objmatch.Increment;
                            DBI.ATEmpSchedules.Attach(item);
                            DBI.Entry(item).Property(w => w.LeaveDesc).IsModified = true;
                            DBI.Entry(item).Property(w => w.LeaveCode).IsModified = true;
                            DBI.Entry(item).Property(w => w.LeaveNo).IsModified = true;
                        }
                        string Approval = SYDocumentStatus.APPROVED.ToString();
                        List<HRClaimLeave> _listClaim = DBI.HRClaimLeaves.Where(w => w.EmpCode == objmatch.EmpCode
                && (w.IsExpired.Value != true || w.IsUsed.Value != true) && w.Status == Approval).ToList();
                        DateTime DateNow = DateTime.Now;
                        bool Isused = false;
                        foreach (var claim in _listClaim.ToList().OrderBy(w => w.WorkingDate))
                        {
                            if (Isused == true) continue;
                            if (claim.Expired.Value.Date < DateNow.Date)
                            {
                                claim.IsExpired = true;
                                DBI.HRClaimLeaves.Attach(claim);
                                DBI.Entry(claim).Property(x => x.IsExpired).IsModified = true;
                            }
                            else
                            {
                                Isused = true;
                                claim.IsUsed = true;
                                claim.DocumentRef = HeaderEmpLeave.Increment.ToString();
                                DBI.HRClaimLeaves.Attach(claim);
                                DBI.Entry(claim).Property(x => x.IsUsed).IsModified = true;
                                DBI.Entry(claim).Property(x => x.DocumentRef).IsModified = true;
                            }
                        }

                        DBI.SaveChanges();
                        ReGenerateLeaveToken(objmatch.EmpCode, objmatch.FromDate, objmatch.ToDate);
                        string DocNo = objmatch.Increment.ToString();
                        DBV = new HumicaDBViewContext();
                        HR_VIEW_EmpLeave EmpLeave = new HR_VIEW_EmpLeave();
                        EmpLeave = DBV.HR_VIEW_EmpLeave.FirstOrDefault(w => w.TranNo == objmatch.TranNo);
                        HRStaffProfile Staff = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == EmpLeave.EmpCode);
                        SYWorkFlowEmailObject wfo =
                                   new SYWorkFlowEmailObject("ESSLA", WorkFlowType.REQUESTER,
                                        UserType.N, SYDocumentStatus.PENDING.ToString());
                        #region Send Email
                        if (!string.IsNullOrEmpty(Staff.Email))
                        {
                            wfo.SelectListItem = new SYSplitItem(Convert.ToString(DocNo));
                            wfo.User = User;
                            wfo.BS = BS;
                            wfo.UrlView = URL;
                            wfo.ScreenId = ScreenId;
                            wfo.Module = "HR";// CModule.PURCHASE.ToString();
                            wfo.ListLineRef = new List<BSWorkAssign>();
                            wfo.Action = SYDocumentStatus.PENDING.ToString();
                            wfo.ObjectDictionary = HeaderEmpLeave;
                            wfo.ListObjectDictionary = new List<object>();
                            wfo.ListObjectDictionary.Add(EmpLeave);

                            wfo.ListObjectDictionary.Add(Staff);
                            WorkFlowResult result1 = wfo.InsertProcessWorkFlowLeave(Staff);
                            MessageError = wfo.getErrorMessage(result1);
                        }
                        #endregion

                        #region *****Send to Telegram
                        if (!string.IsNullOrEmpty(Staff.TeleGroup))
                        {
                            Humica.Core.SY.SYSendTelegramObject wfo1 =
                           new Humica.Core.SY.SYSendTelegramObject("ESSLA", WorkFlowType.APPROVER, objmatch.Status);
                            wfo1.User = User;
                            wfo1.ListObjectDictionary = new List<object>();
                            wfo1.ListObjectDictionary.Add(EmpLeave);
                            wfo1.ListObjectDictionary.Add(Staff);
                            wfo1.Send_SMS_Telegram(Staff.TeleGroup, "");
                        }
                        #endregion
                    }
                    return SYConstant.OK;
                }
                catch (DbEntityValidationException e)
                {
                    /*------------------SaveLog----------------------------------*/
                    SYEventLog log = new SYEventLog();
                    log.ScreenId = ScreenId;
                    log.UserId = User.UserName;
                    log.DocurmentAction = ApprovalID;
                    log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                    SYEventLogObject.saveEventLog(log, e);
                    /*----------------------------------------------------------*/
                    return "EE001";
                }
                catch (DbUpdateException e)
                {
                    /*------------------SaveLog----------------------------------*/
                    SYEventLog log = new SYEventLog();
                    log.ScreenId = ScreenId;
                    log.UserId = User.UserName;
                    log.DocurmentAction = ApprovalID;
                    log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                    SYEventLogObject.saveEventLog(log, e, true);
                    /*----------------------------------------------------------*/
                    return "EE001";
                }
                catch (Exception e)
                {
                    /*------------------SaveLog----------------------------------*/
                    SYEventLog log = new SYEventLog();
                    log.ScreenId = ScreenId;
                    log.UserId = User.UserName;
                    log.DocurmentAction = ApprovalID;
                    log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                    SYEventLogObject.saveEventLog(log, e, true);
                    /*----------------------------------------------------------*/
                    return "EE001";
                }
            }
            finally
            {
                DBI.Configuration.AutoDetectChangesEnabled = true;
            }
        }
        public string RejectLeave(string ApprovalID)
        {
            try
            {
                HumicaDBContext DBI = new HumicaDBContext();
                string[] c = ApprovalID.Split(';');
                foreach (var r in c)
                {
                    if (r == "") continue;
                    int TranNo = Convert.ToInt32(r);
                    string Reject = SYDocumentStatus.REJECTED.ToString();
                    HREmpLeave objmatch = DB.HREmpLeaves.First(w => w.TranNo == TranNo);
                    if (objmatch == null)
                        return "INV_EN";
                    var _obj = DB.ExDocApprovals.Where(x => x.DocumentNo == objmatch.Increment.ToString());
                    foreach (var read in _obj)
                    {
                        read.Status = Reject;
                        read.LastChangedDate = DateTime.Now;
                        DB.Entry(read).Property(w => w.Status).IsModified = true;
                        DB.Entry(read).Property(w => w.LastChangedDate).IsModified = true;
                    }
                    objmatch.RejectDate = DateTime.Now;
                    objmatch.Status = Reject;
                    objmatch.ChangedBy = User.UserName;
                    objmatch.ChangedOn = DateTime.Now;
                    DB.HREmpLeaves.Attach(objmatch);
                    DB.Entry(objmatch).Property(w => w.RejectDate).IsModified = true;
                    DB.Entry(objmatch).Property(w => w.Status).IsModified = true;
                    DB.Entry(objmatch).Property(w => w.ChangedBy).IsModified = true;
                    DB.Entry(objmatch).Property(w => w.ChangedOn).IsModified = true;


                    var Staff = DB.HRStaffProfiles.FirstOrDefault(x => x.EmpCode == objmatch.EmpCode);

                    DB.SaveChanges();

                    #region *****Send to Telegram
                    if (!string.IsNullOrEmpty(Staff.TeleGroup))
                    {
                        Humica.Core.SY.SYSendTelegramObject wfo =
                        new Humica.Core.SY.SYSendTelegramObject("ESSLA", WorkFlowType.REJECTOR, objmatch.Status);
                        wfo.User = User;
                        wfo.ListObjectDictionary = new List<object>();
                        wfo.ListObjectDictionary.Add(objmatch);
                        #region Tembody
                        var Rejector= DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == User.UserName);
                        var ObjectStaff= DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == objmatch.EmpCode);
                        var Handover= DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == objmatch.TaskHand_Over);
                        var leaveTypeKH = DB.HRLeaveTypes.FirstOrDefault(w => w.Code == objmatch.LeaveCode);
                        
                        var title="";
                        if (Rejector != null)
                        {
                            title = Rejector.Title;
                        }
                        string str = "កម្មវត្ថុ៖ Leave Reject"; 
                        if (title == "Ms")
                        {
                            title = "Miss";
                        }
                        str += "<b>%0Aបដិសេដទៅកាន់៖</b> " + ObjectStaff.Title + (". ") + ObjectStaff.AllName + "(" + ObjectStaff.EmpCode + ")";
                        str += "<b>%0Aដោយ៖</b> " + Rejector.Title + (". ") + Rejector.AllName + "(" + Rejector.EmpCode + ")";
                        str += "<b>%0Aកាលបរិច្ឆេទ៖</b> " + objmatch.ChangedOn.Value.Date.ToString("dd-MMM-yyyy");
                        str += "<b>%0Aប្រភេទសុំច្បាប់៖</b> " + leaveTypeKH.Description;
                        str += "<b>%0Aកាលបរិច្ឆេទឈប់សម្រាក៖</b> " + objmatch.FromDate.ToString("dd-MMM-yyyy") + " - "+ objmatch.ToDate.Date.ToString("dd-MMM-yyyy");
                        if (User.UserName == objmatch.APP1Code)
                        {
                            str += "<b>%0Aមតិយោបល់​៖</b>  " + objmatch.APP1Comments;
                        }
                        else if (User.UserName == objmatch.APP2Code)
                        {
                            str += "<b>%0Aមតិយោបល់​៖</b>  " + objmatch.APP2Comments;
                        }
                        else if (User.UserName == objmatch.APP3Code)
                        {
                            str += "<b>%0Aមតិយោបល់​៖</b>  " + objmatch.APP3Comments;
                        }
                        else if (User.UserName == objmatch.APP4Code)
                        {
                            str += "<b>%0Aមតិយោបល់​៖</b>  " + objmatch.APP4Comments;
                        }
                        else if (User.UserName == objmatch.APP5Code)
                        {
                            str += "<b>%0Aមតិយោបល់​៖</b>  " + objmatch.APP5Commrnts;
                        }
                        

                            #endregion
                        wfo.Send_SMS_TelegramLeaveApproval(Staff.TeleGroup, "",str);
                    }
                    #endregion
                }
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ApprovalID;
                log.Action = Humica.EF.SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (DbUpdateException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ApprovalID;
                log.Action = Humica.EF.SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (Exception e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ApprovalID;
                log.Action = Humica.EF.SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string CancelLeave(string ApprovalID)
        {
            try
            {
                HumicaDBContext DBI = new HumicaDBContext();
                string[] c = ApprovalID.Split(';');
                foreach (var r in c)
                {
                    if (r == "") continue;
                    int TranNo = Convert.ToInt32(r);
                    string Reject = SYDocumentStatus.CANCELLED.ToString();
                    HREmpLeave objmatch = DB.HREmpLeaves.First(w => w.TranNo == TranNo);
                    if (objmatch == null)
                        return "INV_EN";
                    var _obj = DB.ExDocApprovals.Where(x => x.DocumentNo == objmatch.Increment.ToString());
                    foreach (var read in _obj)
                    {
                        read.Status = Reject;
                        read.LastChangedDate = DateTime.Now;
                        DB.Entry(read).Property(w => w.Status).IsModified = true;
                        DB.Entry(read).Property(w => w.LastChangedDate).IsModified = true;
                    }
                    objmatch.RejectDate = DateTime.Now;
                    objmatch.Status = Reject;
                    objmatch.ChangedBy = User.UserName;
                    objmatch.ChangedOn = DateTime.Now;
                    DB.HREmpLeaves.Attach(objmatch);
                    DB.Entry(objmatch).Property(w => w.RejectDate).IsModified = true;
                    DB.Entry(objmatch).Property(w => w.Status).IsModified = true;
                    DB.Entry(objmatch).Property(w => w.ChangedBy).IsModified = true;
                    DB.Entry(objmatch).Property(w => w.ChangedOn).IsModified = true;

                    var Staff = DB.HRStaffProfiles.FirstOrDefault(x => x.EmpCode == objmatch.EmpCode);
                    DB.SaveChanges();

                    #region *****Send to Telegram
                    if (!string.IsNullOrEmpty(Staff.TeleGroup))
                    {
                        Humica.Core.SY.SYSendTelegramObject wfo =
                        new Humica.Core.SY.SYSendTelegramObject("ESSLA", WorkFlowType.COLLECTOR, objmatch.Status);
                        wfo.User = User;
                        wfo.ListObjectDictionary = new List<object>();
                        wfo.ListObjectDictionary.Add(objmatch);
                        wfo.Send_SMS_Telegram(Staff.TeleGroup, "");
                    }
                    #endregion
                }
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ApprovalID;
                log.Action = Humica.EF.SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (DbUpdateException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ApprovalID;
                log.Action = Humica.EF.SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (Exception e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ApprovalID;
                log.Action = Humica.EF.SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string DeleteLeave(int id)
        {
            try
            {
                HumicaDBContext DBM = new HumicaDBContext();
                HeaderEmpLeave = new HREmpLeave();
                HeaderEmpLeave.TranNo = id;
                var objLeave = DB.HREmpLeaves.Find(id);
                if (objLeave == null)
                    return "LEAVE_NE";
                HeaderEmpLeave = objLeave;
                var objEmp = DB.HREmpLeaveDs.Where(w => w.LeaveTranNo == objLeave.Increment && w.EmpCode == objLeave.EmpCode).ToList();
                foreach (var item in objEmp)
                {
                    DB.HREmpLeaveDs.Attach(item);
                    DB.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                }
                string Approval = SYDocumentStatus.APPROVED.ToString();
                var objEmpClaim = DB.HRClaimLeaves.Where(w => w.DocumentRef == objLeave.Increment.ToString() && w.Status == Approval).ToList();
                foreach (var item in objEmpClaim)
                {
                    item.IsUsed = false;
                    item.DocumentRef = "";
                    DB.HRClaimLeaves.Attach(item);
                    DB.Entry(item).Property(x => x.IsUsed).IsModified = true;
                    DB.Entry(item).Property(x => x.DocumentRef).IsModified = true;
                }
                //Att
                var ListAtt = DB.ATEmpSchedules.Where(w => w.LeaveNo == objLeave.Increment && w.EmpCode == objLeave.EmpCode).ToList();
                foreach (var item in ListAtt)
                {
                    item.LeaveCode = "";
                    item.LeaveNo = -1;
                    DB.ATEmpSchedules.Attach(item);
                    DB.Entry(item).Property(w => w.LeaveNo).IsModified = true;
                    DB.Entry(item).Property(w => w.LeaveCode).IsModified = true;
                }
                DB.HREmpLeaves.Attach(objLeave);
                DB.Entry(objLeave).State = System.Data.Entity.EntityState.Deleted;
                string Increment = objLeave.Increment.ToString();
                var ListApp = DB.ExDocApprovals.Where(w => w.DocumentNo == Increment).ToList();
                foreach (var item in ListApp)
                {
                    DB.ExDocApprovals.Attach(item);
                    DB.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                }

                int row = DB.SaveChanges();
                ReGenerateLeaveToken(HeaderEmpLeave.EmpCode, HeaderEmpLeave.FromDate, HeaderEmpLeave.ToDate);
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserID.ToString();
                log.ScreenId = HeaderEmpLeave.TranNo.ToString();
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (Exception e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserID.ToString();
                log.ScreenId = HeaderEmpLeave.TranNo.ToString();
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public void SetAutoApproval(string EmpCode, string Branch,DateTime ToDate)
        {
            ListApproval = new List<ExDocApproval>();
            var DBX = new HumicaDBContext();
           // var LstStaff = DB.HRStaffProfiles.ToList();
            var LstStaff = DB.HRStaffProfiles.Where(w => w.Status == "A" || (EntityFunctions.TruncateTime(w.DateTerminate) > EntityFunctions.TruncateTime(ToDate) && w.Status == "I")).ToList();
            var ListWorkFlow = DB.HRWorkFlowLeaves.ToList();
            var _staffApp = new HRStaffProfile();
            foreach (var item in ListWorkFlow)
            {
                var Staff = LstStaff.FirstOrDefault(w => w.EmpCode == EmpCode);
                if (item.ApproveBy == "1st")
                {
                    var Read = LstStaff.Where(w => w.EmpCode == Staff.FirstLine).ToList();
                    _staffApp = Read.FirstOrDefault();
                    if (_staffApp != null)
                    {
                        ExDocApproval objApp1 = AddDocApproval(_staffApp, item.Step);
                        ListApproval.Add(objApp1);
                    }
                    HRStaffProfile _staff = LstStaff.FirstOrDefault(w => w.EmpCode == Staff.FirstLine2);
                    if (_staff != null)
                    {
                        ExDocApproval objApp1 = AddDocApproval(_staff, item.Step);
                        ListApproval.Add(objApp1);
                    }
                }
                else if (item.ApproveBy == "2nd")
                {
                    List<HRStaffProfile> Read = LstStaff.Where(w => w.EmpCode == Staff.SecondLine).ToList();
                    _staffApp = Read.FirstOrDefault();
                    if (_staffApp != null)
                    {
                        ExDocApproval objApp1 = AddDocApproval(_staffApp, item.Step);
                        ListApproval.Add(objApp1);
                    }
                    HRStaffProfile _staff = LstStaff.FirstOrDefault(w => w.EmpCode == Staff.SecondLine2);
                    if (_staff != null)
                    {
                        ExDocApproval objApp1 = AddDocApproval(_staff, item.Step);
                        ListApproval.Add(objApp1);
                    }
                }
                else
                {
                    _staffApp = LstStaff.FirstOrDefault(w => w.JobCode == item.ApproveBy && w.Branch == Branch);
                    if (_staffApp == null)
                        _staffApp = LstStaff.FirstOrDefault(w => w.JobCode == item.ApproveBy);
                    if (_staffApp == null) continue;

                    if (ListApproval.Where(w => w.Approver == _staffApp.EmpCode).Count() > 0) continue;
                    var objApp = new ExDocApproval();
                    objApp.Approver = _staffApp.EmpCode;
                    objApp.ApproverName = _staffApp.AllName;
                    objApp.DocumentType = "LR";
                    objApp.ApproveLevel = item.Step;
                    objApp.WFObject = "WF02";
                    ListApproval.Add(objApp);
                }
            }
        }
        public HRStaffProfile getNextApprover(string id, string pro)
        {
            var objStaff = new HRStaffProfile();
            var DBX = new HumicaDBContext();
            //var objHeader = DBX.HRReqPayments.Find(id);
            //if (objHeader == null)
            //{
            //    return new HRStaffProfile();
            //}
            string open = SYDocumentStatus.OPEN.ToString();
            var listCanApproval = DBX.ExDocApprovals.Where(w => w.DocumentNo == id && w.Status == open && w.DocumentType == "LR").ToList();

            if (listCanApproval.Count == 0)
            {
                return new HRStaffProfile();
            }

            var min = listCanApproval.Min(w => w.ApproveLevel);
            var NextApp = listCanApproval.Where(w => w.ApproveLevel == min).First();
            objStaff = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == NextApp.Approver);//, objHeader.Property);
            return objStaff;
        }

        #region Edit Entitle
        public string CreateEmpEditEnTitle(string id)
        {
            try
            {
                DB = new HumicaDBContext();
                DB.Configuration.AutoDetectChangesEnabled = false;
                var staff = DB.HRStaffProfiles.FirstOrDefault(x => x.EmpCode == HeaderEditEntitle.EmpCode);
                var lstempLeave = DB.HREmpEditLeaveEntitles.ToList();

                HeaderEditEntitle.Position = HeaderEditEntitle.Position;
                HeaderEditEntitle.DocumentDate = HeaderEditEntitle.DocumentDate;
                HeaderEditEntitle.Balance = HeaderEditEntitle.Balance;
                HeaderEditEntitle.CreatedBy = User.UserName;
                HeaderEditEntitle.CreatedOn = DateTime.Now;
                DB.HREmpEditLeaveEntitles.Add(HeaderEditEntitle);


                int row = DB.SaveChanges();
                Generate_Balance_Edit(HeaderEditEntitle.EmpCode, HeaderEditEntitle.LeaveType, HeaderEditEntitle.InYear);
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserID.ToString();
                log.ScreenId = HeaderEmpLeave.EmpCode;
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            finally
            {
                DB.Configuration.AutoDetectChangesEnabled = true;
            }
        }
        public string EditLeaveEnTitle(int id)
        {
            try
            {

                var objMatch = DB.HREmpEditLeaveEntitles.FirstOrDefault(w => w.ID == id);
                if (objMatch == null)
                {
                    return "DOC_NE";
                }
                HeaderEditEntitle.EmpCode = objMatch.EmpCode;
                objMatch.EmpName = HeaderEditEntitle.EmpName;
                objMatch.Balance = HeaderEditEntitle.Balance;
                objMatch.DocumentDate = HeaderEditEntitle.DocumentDate;
                objMatch.LeaveType = HeaderEditEntitle.LeaveType;
                objMatch.Position = HeaderEditEntitle.Position;
                objMatch.InYear = HeaderEditEntitle.InYear;
                objMatch.ChangedBy = User.UserName;
                objMatch.ChangeOn = DateTime.Now;

                DB.HREmpEditLeaveEntitles.Attach(objMatch);
                DB.Entry(objMatch).State = System.Data.Entity.EntityState.Modified;
                int row1 = DB.SaveChanges();
                Generate_Balance_Edit(objMatch.EmpCode, objMatch.LeaveType, objMatch.InYear);
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = HeaderEditEntitle.EmpCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string DeleteEditLeaveEnTitle(int id)
        {
            try
            {
                HumicaDBContext DBM = new HumicaDBContext();
                HeaderEditEntitle = new HREmpEditLeaveEntitle();
                HeaderEditEntitle.ID = id;
                var objLeave = DB.HREmpEditLeaveEntitles.Find(id);
                if (objLeave == null)
                {
                    return "LEAVE_NE";
                }
                HeaderEditEntitle = objLeave;

                DB.HREmpEditLeaveEntitles.Attach(objLeave);
                DB.Entry(objLeave).State = System.Data.Entity.EntityState.Deleted;

                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserID.ToString();
                log.ScreenId = HeaderEditEntitle.ID.ToString();
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (Exception e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserID.ToString();
                log.ScreenId = HeaderEditEntitle.ID.ToString();
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        #endregion

        public List<Employee_Generate_Leave> LoadDataEmpGen(FTFilterData Filter1, List<HRBranch> ListBranch)
        {
            DateTime FromDate = new DateTime(Filter1.INYear, 1, 1);
            DateTime ToData = new DateTime(Filter1.INYear, 12, 31);
            var _List = new List<Employee_Generate_Leave>();
            var _listStaff = DB.HRStaffProfiles.AsEnumerable().Where(w => ListBranch.Where(x => x.Code == w.Branch).Any()).ToList();
            var _listBanaceLeave = DB.HREmpLeaveBs.Where(w => w.InYear == Filter1.INYear).ToList();
            _listStaff = _listStaff.Where(w => w.LeaveConf.Value.Date <= ToData.Date && (w.TerminateStatus == "" || w.TerminateStatus == null)).ToList();
            if (Filter1.Department != null)
                _listStaff = _listStaff.Where(w => w.DEPT == Filter1.Department).ToList();
            if (Filter1.Branch != null)
                _listStaff = _listStaff.Where(w => w.Branch == Filter1.Branch).ToList();
            foreach (var item in _listStaff)
            {
                string StrAction = "No Action";
                if (_listBanaceLeave.Where(w => w.EmpCode == item.EmpCode).Any())
                    StrAction = "Action";
                int years = -1, months = -1, days = -1;
                DateTimeHelper.TimeSpanToDate(DateTime.Now, item.LeaveConf.Value, out years, out months, out days);
                string SLength = "";
                if (years != 0) SLength += years + "y ";
                if (months != 0) SLength += months + "m ";
                if (days != 0) SLength += days + "d";
                DateTime? Enddate = new DateTime();
                Enddate = null;
                string Status = "Active";
                if (item.DateTerminate.Year != 1900)
                {
                    Status = "InActive";
                    Enddate = item.DateTerminate;
                }
                var emp = new Employee_Generate_Leave()
                {
                    Action = StrAction,
                    EmpCode = item.EmpCode,
                    AllName = item.AllName,
                    Gender = item.Sex,
                    StartDate = item.StartDate.Value,
                    EndDate = Enddate,
                    Status = Status,
                    ServiceLength = SLength
                };
                _List.Add(emp);
            }

            return _List;
        }
        public List<HR_STAFF_VIEW> LoadDataEmp(FTFilterData Filter1, List<HRBranch> ListBranch)
        {
            var branchCodes = ListBranch.Select(b => b.Code).ToList();
            var _staffQuery = DBV.HR_STAFF_VIEW.Where(w => w.StatusCode == "A" && branchCodes.Contains(w.BranchID));

            if (!string.IsNullOrEmpty(Filter1.Branch))
                _staffQuery = _staffQuery.Where(w => w.BranchID == Filter1.Branch);

            if (!string.IsNullOrEmpty(Filter1.Division))
                _staffQuery = _staffQuery.Where(w => w.Division == Filter1.Division);

            if (!string.IsNullOrEmpty(Filter1.Department))
                _staffQuery = _staffQuery.Where(w => w.DEPT == Filter1.Department);

            if (!string.IsNullOrEmpty(Filter1.Section))
                _staffQuery = _staffQuery.Where(w => w.Section == Filter1.Section);

            if (!string.IsNullOrEmpty(Filter1.Position))
                _staffQuery = _staffQuery.Where(w => w.JobCode == Filter1.Position);

            if (!string.IsNullOrEmpty(Filter1.LevelCode))
                _staffQuery = _staffQuery.Where(w => w.LevelCode == Filter1.LevelCode);

            return _staffQuery.ToList();
        }

        public List<Employee_ListForwardLeave> LoadDataEmpForward(FTFilterData Filter1, List<HRBranch> ListBranch)
        {
            var _List = new List<Employee_ListForwardLeave>();
            var _listEmpB = DB.HREmpLeaveBs.Where(w => w.InYear == Filter1.FYear).ToList();
            //var staff = DB.HRStaffProfiles.ToList();
            var dep = DB.HRDepartments.ToList();
            var Post = DB.HRPositions.ToList();
            var Level = DP.HRLevels.ToList();
            var ListLeave_Rate = DB.HRLeaveProRates.ToList();
            var ListParam = DB.PRParameters.ToList();
            var forward_up = DP.SYSettings.FirstOrDefault(w => w.SettingName == "LEAVE_FORWARD_BANANCE_UP");
            var staff = DB.HRStaffProfiles.AsEnumerable().Where(w => ListBranch.Where(x => x.Code == w.Branch).Any()).ToList();
            var _listStaff = from Staff1 in staff
                             join EmB in _listEmpB on Staff1.EmpCode equals EmB.EmpCode
                             join d in dep on Staff1.DEPT equals d.Code
                             join p in Post on Staff1.JobCode equals p.Code
                             join L in Level on Staff1.LevelCode equals L.Code
                             where EmB.LeaveCode == Filter1.LeaveType && EmB.InYear == Filter1.FYear && Staff1.Status != "I"
                             select new
                             {
                                 Staff1.EmpCode,
                                 Staff1.AllName,
                                 EmB.Balance,
                                 Departmetn = d.Description,
                                 Position = p.Description,
                                 Staff1.StartDate,
                                 Level = L.Description,
                                 Staff1.Branch,
                                 Staff1.DEPT,
                                 Staff1.LevelCode
                             };
            _listStaff = _listStaff.ToList();
            if (Filter1.Department != null)
                _listStaff = _listStaff.Where(w => w.DEPT == Filter1.Department).ToList();
            if (Filter1.Branch != null)
                _listStaff = _listStaff.Where(w => w.Branch == Filter1.Branch).ToList();
            if (Filter1.LevelCode != null)
                _listStaff = _listStaff.Where(w => w.LevelCode == Filter1.LevelCode).ToList();
            decimal _forward = 0;
            if (forward_up != null) _forward = Convert.ToDecimal(forward_up.SettinValue);
            _listStaff = _listStaff.Where(_w => _w.Balance >= _forward).ToList();
            foreach (var item in _listStaff)
            {
                var fw = new Employee_ListForwardLeave();
                fw.EmpCode = item.EmpCode;
                fw.AllName = item.AllName;
                fw.Level = item.Level;
                fw.Department = item.Departmetn;
                fw.Position = item.Position;
                fw.StartDate = item.StartDate.Value;
                fw.Balance = item.Balance.Value;
                if (item.Balance > Filter1.MaxForward)
                    fw.ForWard = Filter1.MaxForward;
                else fw.ForWard = Convert.ToDecimal(item.Balance);
                _List.Add(fw);
            }

            return _List;

        }

        private WorkFlowResult Send_SMS_Telegram(string TemBody, string ChatID, List<object> ListObjectDictionary, string URL)
        {
            try
            {
                // var Telegram = DB.TelegramBots.FirstOrDefault(w=>w.Description== "LeaveRequest");
                var Telegram = DB.TelegramBots.FirstOrDefault(w => w.ChatID == ChatID);
                if (Telegram != null)
                {
                    string urlString = "https://api.telegram.org/bot{0}/sendMessage?chat_id={1}&text={2}&parse_mode=HTML";
                    string apiToken = Telegram.TokenID;// "835670290:AAGoq8pHBgi0vGHJgCimeMLVGhpNrYzdEfM";
                                                       // "872155850:AAHlcg1gcH6MjZaKtzaPhtQu03PxHQN4ZZU";
                                                       //  string chatId = "504467938"; -1001405576397,-1001429819055 
                    string chatId = Telegram.ChatID;//1001446018688
                    string text = getEmailContentByParam(TemBody, ListObjectDictionary, URL); //TemBody;
                    urlString = String.Format(urlString, apiToken, chatId, text);
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    System.Net.WebRequest request = System.Net.WebRequest.Create(urlString);
                    Stream rs = request.GetResponse().GetResponseStream();
                    StreamReader reader = new StreamReader(rs);
                    string line = "";
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    while (line != null)
                    {
                        line = reader.ReadLine();
                        if (line != null)
                            sb.Append(line);
                    }
                    string response = sb.ToString();
                    return WorkFlowResult.COMPLETED;
                }
                return WorkFlowResult.TELEGRAM_NOT_SEND;
            }
            catch (Exception e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserID.ToString();
                log.ScreenId = HeaderEmpLeave.EmpCode;
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return WorkFlowResult.ERROR;
            }

        }
        public string getEmailContentByParam(string text, List<object> ListObjectDictionary, string URL)
        {
            string[] textsp = text.Split(' ');
            if (textsp.LongLength > 0)
            {
                foreach (string param in textsp)
                {
                    if (param.Trim() == "") continue;
                    if (param.Substring(0, 1) == "@")
                    {
                        var objParam = DP.SYEmailParameters.FirstOrDefault(w => w.Parameter == param
                        && w.TemplateID == "TELEGRAM");
                        if (objParam != null)
                        {

                            if (ListObjectDictionary.Count > 0)
                            {
                                string textstr = ClsInformation.GetFieldValues(objParam.ObjectName, ListObjectDictionary, objParam.FieldName, param);
                                if (textstr != null)
                                {
                                    text = text.Replace(param, textstr);
                                }
                            }
                        }
                    }

                }

            }

            string link = URL;
            // text = text.Replace("@NUMBER", number);
            text = text.Replace("@LINK", link);
            // text = text.Replace("@BATCH_NUMBER", batch);

            return text;
        }
        public decimal GetEnitile(string LeaveCode, ClsPeriodLeave periodLeave, PRParameter payParam, List<HRLeaveProRate> ListLeave_Rate, decimal Rate, bool IsResign = false)
        {
            decimal _Balance = 0;
            decimal prorate_Amount_END = 0;
            decimal prorate_Amount_new = 0;
            DateTime EDate_OfMonth_new = periodLeave.StartDate.EndDateOfMonth();
            if (periodLeave.servicelength == 1) EDate_OfMonth_new = DateTime.Now;
            if (ListLeave_Rate.Count() > 0)
            {
                var LeaveRate = ListLeave_Rate.Where(w => w.Status == "NEWJOIN" && w.LeaveType == LeaveCode).ToList();


                decimal _actWorkDayNew = PRPayParameterObject.Get_WorkingDay(payParam, periodLeave.StartDate, EDate_OfMonth_new);
                HRLeaveProRate _prorateNew = LeaveRate.Where(w => w.ActWorkDayFrom <= _actWorkDayNew && w.ActWorkDayTo
                >= _actWorkDayNew).FirstOrDefault();
                prorate_Amount_new = _prorateNew == null ? 0 : _prorateNew.Rate;
                if (IsResign == true)
                {
                    if (!(periodLeave.StartDate.Year == periodLeave.EndDate.Year && periodLeave.StartDate.Month == periodLeave.EndDate.Month))
                    {
                        var LeaveRate_RESIGN = ListLeave_Rate.Where(w => w.Status == "RESIGN" && w.LeaveType == LeaveCode).ToList();
                        DateTime EndDate_OfMonth_END = periodLeave.EndDate.StartDateOfMonth();
                        decimal _actWorkDayEND = PRPayParameterObject.Get_WorkingDay(payParam, EndDate_OfMonth_END, periodLeave.EndDate);
                        HRLeaveProRate _prorateEnd = LeaveRate_RESIGN.Where(w => w.ActWorkDayFrom <= _actWorkDayEND
                        && w.ActWorkDayTo >= _actWorkDayEND).FirstOrDefault();
                        prorate_Amount_END = _prorateEnd == null ? 0 : _prorateEnd.Rate;
                    }
                }
                int C_Month = DateTimeHelper.GetMonth(periodLeave.StartDate.AddMonths(1), periodLeave.EndDate.AddMonths(-1));
                _Balance = Convert.ToDecimal(Rate * C_Month);
                _Balance += prorate_Amount_new + prorate_Amount_END;
            }
            else
            {
                //int C_Month = DateTimeHelper.GetMonth(periodLeave.StartDate, periodLeave.EndDate);
                //_Balance = Convert.ToDecimal(Rate * C_Month);
                var _Entitle = periodLeave.ListTempEntitle.FirstOrDefault();
                decimal WorkLen = periodLeave.servicelength;
                if (WorkLen >= _Entitle.ToMonth)
                    _Balance = (decimal)_Entitle.Entitle;
                else
                {
                    int C_Day = DateTimeHelper.GetDay(periodLeave.StartDate, periodLeave.EndDate);
                    decimal EDay = (decimal)_Entitle.Entitle.Value;
                    _Balance = ClsRounding.RoundNormal(C_Day * (EDay / 365), 2);
                    if (_Balance > (decimal)_Entitle.Entitle.Value)
                    {
                        _Balance = (decimal)_Entitle.Entitle;
                    }

                }
            }

            return _Balance;
        }
        public IQueryable<HREmpLeaveD> GetLeaveToken(int InYear)
        {
            string approved = SYDocumentStatus.APPROVED.ToString();
            IQueryable<HREmpLeaveD> _list = from Leave in DB.HREmpLeaves
                                            join Item in DB.HREmpLeaveDs on new { Increment = (int)Leave.Increment, EmpCode = Leave.EmpCode }
                                                                         equals new { Increment = (int)Item.LeaveTranNo, EmpCode = Item.EmpCode }
                                            where Leave.Status == approved && Item.Status == "Leave" && Item.LeaveDate.Year == InYear
                                            select Item;
            return _list;
        }
        public HREmpLeaveB Calculate_Token(HREmpLeaveB LeaveB, List<HREmpLeaveD> ListLeaveD, List<HRLeaveType> leaveTypes,
            PRParameter payParam, decimal _Balance, List<HRClaimLeave> LstEmpClaim)
        {
            decimal? WHPDay = 8;
            decimal Forward = 0;
            decimal ForwardUse = 0;
            //decimal ForwardUse2 = 0;
            if (payParam != null) WHPDay = payParam.WHOUR;
            List<ListLeaveType> _lstStr = new List<ListLeaveType>();
            _lstStr = GetLeave_SubParent(leaveTypes, LeaveB.LeaveCode);
            var _LeaveD = ListLeaveD.Where(w => w.EmpCode == LeaveB.EmpCode && _lstStr.Where(x => x.LeaveCode == w.LeaveCode).Any()).ToList();
            var _lstLeaveByHour = ListLeaveD.Where(w => w.Remark == "Hours" && w.EmpCode == LeaveB.EmpCode && _lstStr.Where(x => x.LeaveCode == w.LeaveCode).Any()).ToList();

            var Claim = LstEmpClaim.Where(w => _lstStr.Where(x => x.LeaveCode == w.ClaimLeave).Any()).ToList();
            var Rest = Claim.Where(w => w.WorkingType == "RD").Sum(x => x.WorkingHour);
            var PH = Claim.Where(w => w.WorkingType == "PH").Sum(x => x.WorkingHour);
            LeaveB.Rest_Edit = Convert.ToDecimal(Rest / WHPDay);
            LeaveB.PH_Edit = Convert.ToDecimal(PH / WHPDay);

            if (LeaveB.ForWardExp.HasValue)
            {
                if (LeaveB.ForWardExp.Value.Year != 1900)
                {
                    var Result = _LeaveD.Where(w => w.LeaveDate.Date <= LeaveB.ForWardExp.Value.Date).ToList();
                    Forward = Convert.ToDecimal(Result.Sum(x => x.LHour) / WHPDay);
                    if (Forward >= LeaveB.Forward)
                        Forward = Convert.ToDecimal(LeaveB.Forward);
                }
            }

            decimal DayLeave = Convert.ToDecimal(_LeaveD.Sum(w => w.LHour) / WHPDay);
            decimal LeaveByHour = Convert.ToDecimal(_lstLeaveByHour.Sum(w => w.LHour));
            decimal TempLeave = DayLeave;
            //if (DayLeave > _Balance)
            //{
            //    DayLeave = DayLeave - _Balance;
            clsForwards _Forward = new clsForwards();
            _Forward = Calculate_Forward(DayLeave, Forward, ForwardUse, LeaveB.ForwardUse);
            DayLeave = _Forward.DayLeave;
            ForwardUse = _Forward.ForwardUse;
            //}
            DayLeave = DayLeave - _Balance - Forward;
            if (ForwardUse > 0) LeaveB.ForwardUse = ForwardUse;
            LeaveB.Balance = TempLeave - ForwardUse;
            LeaveB.TakenHour = LeaveByHour;
            return LeaveB;
        }
        public List<ListLeaveType> GetLeave_SubParent(List<HRLeaveType> leaveTypes, string Code)
        {
            List<ListLeaveType> _lstStr = new List<ListLeaveType>();
            _lstStr.Add(new ListLeaveType() { LeaveCode = Code });
            foreach (var read in leaveTypes.Where(w => w.IsParent == true && w.Parent == Code).ToList())
            {
                if (!_lstStr.Where(x => x.LeaveCode == read.Code).Any())
                    _lstStr.Add(new ListLeaveType() { LeaveCode = read.Code });
            }
            return _lstStr;
        }
        public List<HREmpLeaveD> GetLeave_D(List<HREmpLeaveD> ListLeaveD, string EmpCode,string LeaveCode, string Unit)
        {
            List<HREmpLeaveD> _lstStr = new List<HREmpLeaveD>();
            int Line_ = 0;
            int Increment_ = GetLastIncrement();
            foreach (var item in ListLeaveD)
            {
                Line_ += 1;
                var result = new HREmpLeaveD();
                result.LeaveTranNo = Increment_ + 1;
                result.EmpCode = EmpCode;
                result.LeaveCode = LeaveCode;
                result.LeaveDate = item.LeaveDate;
                result.CutMonth = item.CutMonth;
                result.Status = item.Status;
                result.Remark = item.Remark;
                result.LineItem = Line_;
                result.LHour = item.LHour;
                if (Unit != "Day")
                {
                    result.Remark = "Hours";
                    result.StartTime = item.StartTime;
                    result.EndTime = item.EndTime;
                }
                if (item.Remark == "Morning" || item.Remark == "Afternoon")
                    result.LHour = item.LHour / 2;
                result.CreateBy = User.UserName;
                result.CreateOn = DateTime.Now;
                _lstStr.Add(result);
            }
            return _lstStr;
        }
        public HREmpLeaveB NewEmpLeaveB(string EmpCode, string EmpName, string LeaveCode, int? InYear)
        {
            var EmpLeaveB = new HREmpLeaveB();
            EmpLeaveB.EmpCode = EmpCode;
            EmpLeaveB.AllName = EmpName;
            EmpLeaveB.LeaveCode = LeaveCode;
            EmpLeaveB.InYear = InYear;
            EmpLeaveB.DayLeave = 0;
            EmpLeaveB.DayEntitle = 0;
            EmpLeaveB.TakenHour = 0;
            EmpLeaveB.BalanceHour = 0;
            EmpLeaveB.YTD = 0;
            EmpLeaveB.Balance = 0;
            EmpLeaveB.Forward = 0;
            EmpLeaveB.ForWardExp = new DateTime(1900, 1, 1);
            EmpLeaveB.CreateOn = DateTime.Now;
            EmpLeaveB.CreateBy = User.UserName;
            return EmpLeaveB;
        }
        public string GetCconMessage(HR_VIEW_EmpLeave Leave, List<HREmpLeaveD> ListEmpleave, HRStaffProfile StaffApp, string Units, string Phone, string URL, decimal? Balance, decimal? WHOUR)
        {
            decimal NoDay = 0;
            decimal TotalHour = 0;
            var data = DP.SYDatas.Where(w => w.DropDownType == "LEAVE_TIME");
            var Approve = DB.HRStaffProfiles.Where(w => w.EmpCode == Leave.EmpCode).FirstOrDefault();
            var Requestor = DB.HRStaffProfiles.Where(w => w.EmpCode == User.UserName).FirstOrDefault();
            var First = DB.HRStaffProfiles.Where(w => w.EmpCode == Approve.FirstLine).FirstOrDefault();
            var second = DB.HRStaffProfiles.Where(w => w.EmpCode == Approve.SecondLine).FirstOrDefault();
            var handOver = DB.HREmpLeaves.Where(w => w.TranNo == Leave.TranNo).FirstOrDefault();
            var NameHandOver = DB.HRStaffProfiles.Where(W => W.EmpCode == handOver.TaskHand_Over).FirstOrDefault();
            //var Handover = DB.HREmpLeaves.Where(w => w.EmpCode == Handover.).FirstOrDefault();

            string str = "<b>កម្មវត្ថុ៖</b> ការស្នើរសុំច្បាប់";
            if (StaffApp.Title == "Ms")
            {
                StaffApp.Title = "Miss";
            }
            str += "<b>%0Aអ្នកស្នើរសុំ៖</b> " + Requestor.Title + (" ") + Requestor.AllName + "(" + Requestor.EmpCode + ")";
            str += "<b>%0Aរង់ចាំអនុម័តដោយ៖</b> " + First.Title + (" ") + First.AllName + "(" + First.EmpCode + ")";
            if(second != null)
                str += "<b>%0Aរង់ចាំអនុម័តដោយ៖</b> " + second.Title + (" ") + second.AllName + "(" + second.EmpCode + ")";
            str += "<b>%0Aកាលបរិច្ឆេទស្នើសុំ៖</b> " + Leave.RequestDate.Value.ToString("dd-MMM-yyyy");
            str += "<b>%0Aប្រភេទសុំច្បាប់៖</b> " + Leave.LeaveType;
            str += "<b>%0Aសមតុល្យ៖</b> " + Math.Round((decimal)Balance,3);
            str += "<b>%0Aកាលបរិច្ឆេទឈប់សម្រាក៖</b> " + Leave.FromDate.ToString("dd-MMM-yyyy") + " to " + Leave.ToDate.ToString("dd-MMM-yyyy");
            //foreach (var item in ListEmpleave)
            //{
            //    if (Units == "Day")
            //    {
            //        NoDay += 1;
            //        string remark = "FullDay";
            //        var result = data.FirstOrDefault(w => w.SelectValue == item.Remark);
            //        if (result != null) { remark = result.SelectText.ToString(); }
            //        str += "%0A" + item.LeaveDate.ToString("<b>dd-MMM-yyyy</b>") + "<b>:</B> " + remark;
            //        if (item.Remark != "FullDay") NoDay -= 0.5m;
            //    }
            //    else if (Units == "Hours") str += "%0A<b>Leave Time:</b>" + item.LeaveDate.ToString("hh:mm tt") + " to " + item.LeaveDate.ToString("hh:mm tt"); //Math.Round(Convert.ToDecimal(item.LHour), 2) + " hours";
            //}
            if ((decimal)ListEmpleave.Sum(w => w.LHour.Value) / WHOUR < WHOUR)
            {
                foreach (var item in ListEmpleave)
                {
                    if (Units == "Day")
                    {
                        if (item.Status != "Rest" && item.Status != "PH")
                            NoDay += 1;
                        string remark = "FullDay";
                        var result = data.FirstOrDefault(w => w.SelectValue == item.Remark);
                        if (result != null) { remark = result.SelectText.ToString(); }
                        if (item.Status != "Leave") remark = item.Status;
                        str += "%0A<b>" + item.LeaveDate.ToString("dd-MMM-yyyy") + ":</b> " + remark;
                        if (item.Remark != "FullDay") NoDay -= 0.5m;
                    }
                    else if (Units == "Hours")
                    {
                        str += "%0A<b>ម៉ោងត្រូវឈប់៖</b> " + item.StartTime.Value.ToString("hh:mm tt") + " to " + item.EndTime.Value.ToString("hh:mm tt");
                        TotalHour += Convert.ToDecimal(item.LHour);
                    }
                }
            }
            else
            {
                foreach (var item in ListEmpleave)
                {
                    if (Units == "Day")
                    {
                        NoDay += 1;
                        if (item.Remark != "FullDay") NoDay -= 0.5m;
                    }
                }
            }
            if (Units == "Day")
                str += "<b>%0Aចំនួនថ្ងៃបានឈប់៖</b> " + (Leave.NoDay - Leave.NoRest);
            else if (Units == "Hours") str += "<b>%0Aចំនួនម៉ោងឈប់៖</b> " + Math.Round(Convert.ToDecimal(ListEmpleave.Sum(w => w.LHour)), 2);
            str += "<b>%0Aមូលហេតុ៖ </b> " + Leave.Reason;
            if (handOver.TaskHand_Over != null)
            {
                str += "<b>%0Aផ្ទេរសិទ្ធិជូន៖</b> " + NameHandOver.Title + (" ") + NameHandOver.AllName + "(" + handOver.TaskHand_Over + ")";
            }
            //str += "<b>%0Aលេខទូរស័ព្ទ  ៖</b> " + Phone;
            //str += "%0A%0A<b>សូមចូលទៅកាន់ប្រព័ន្ធតាមរយៈតំណរភ្ជាប់ ៖</b> <a href=\"@LINK\\\">Link</a>";
            //str = str.Replace("@LINK", URL);
            return str;
        }
        public ClsPeriodLeave GetPeriod(ClsPeriodLeave Period, HRStaffProfile Employee, List<HRSetEntitleD> ListetEntitleD,
            int InYear, string LeaveCode)
        {

            decimal Rate = 0;
            decimal _Balance = 0;
            decimal _Entitle = 0;
            Period.SeniorityBalance = 0;
            DateTime FromDate = new DateTime(InYear, 1, 1);
            if (Employee.LeaveConf.Value.Date > FromDate.Date)
                Period.StartDate = Employee.LeaveConf.Value;
            else
                Period.StartDate = FromDate;
            if (Employee.DateTerminate.Year != 1900)
                Period.EndDate = Employee.DateTerminate.AddDays(-1);
            else
                Period.EndDate = DateTime.Now;

            Period.servicelength = DateTimeHelper.GetMonth(Employee.LeaveConf.Value, Period.EndDate);
            Period.ListTempEntitle = ListetEntitleD.Where(w => w.LeaveCode == LeaveCode && w.CodeH == Employee.TemLeave &&
                   w.FromMonth <= Period.servicelength && w.ToMonth >= Period.servicelength).ToList();
            Period.EmpCode = Employee.EmpCode;
            DateTime StartDate = new DateTime(InYear, 1, 1);
            DateTime EndDate = new DateTime(InYear, 12, 31);
            Period.ServicePeriod = DateTimeHelper.GetMonth(StartDate, EndDate);

            foreach (var temp in Period.ListTempEntitle)
            {
                Period.SeniorityBalance = temp.SeniorityBalance.Value;
                if (temp.IsProRate == true)
                {
                    Rate = (decimal)(temp.Entitle / Period.ServicePeriod);
                    _Balance = GetEnitile(LeaveCode, Period, Period.payParam, Period.ListLeaveProRate, Rate, true);
                    if ((Employee.Status != "A"))
                        _Entitle = _Balance;
                    else if((Employee.Status == "A"
                        && Employee.StartDate.Value.Year == InYear))
                    {
                        Period.EndDate = EndDate;
                        _Entitle = GetEnitile(LeaveCode, Period, Period.payParam, Period.ListLeaveProRate, Rate, true);
                    }
                    else if (Employee.DateTerminate.Year == 1900  && Employee.StartDate.Value.Year < InYear)
                        _Entitle = (decimal)temp.Entitle.Value;
                }
                else
                {
                    _Balance = (decimal)temp.Entitle.Value;
                    _Entitle = (decimal)temp.Entitle.Value;
                }
            }
            Period.Balance = _Balance;
            Period.Entitle = _Entitle;
            return Period;
        }
        public clsForwards Calculate_Forward(decimal DayLeave, decimal Forward, decimal ForwardUse, decimal ForwardUsed)
        {
            clsForwards _Forward = new clsForwards();
            decimal Used = 0;
            if (Forward == ForwardUsed)
            {
                DayLeave = DayLeave - Forward;
                Used = Forward;
            }
            else
            {
                if (DayLeave > Forward)
                {
                    DayLeave = DayLeave - Forward;
                    Used = Forward;
                }
                else if (DayLeave > 0)
                {
                    decimal Use = DayLeave;
                    DayLeave = 0;
                    Used = Use;
                }
            }
            _Forward.ForwardUse = Used;
            _Forward.DayLeave = DayLeave;
            return _Forward;
        }
        public static int GetLastIncrement()
        {
            HumicaDBContext DBI = new HumicaDBContext();
            int Increment = DBI.HREmpLeaves.Select(w => w.Increment).DefaultIfEmpty(0).Max();

            return Increment;
        }
        public void Generate_Balance_Edit(string EmpCode, string LeaveType, int? Year)
        {
            HumicaDBContext DBI = new HumicaDBContext();
            string Approval = SYDocumentStatus.APPROVED.ToString();
            var EmpClaim = DBI.HREmpEditLeaveEntitles.Where(w => w.EmpCode == EmpCode && w.InYear == Year).ToList();
            HREmpLeaveB LeaveB = DBI.HREmpLeaveBs.Where(w => w.EmpCode == EmpCode && w.LeaveCode == LeaveType
                  && w.InYear == Year).FirstOrDefault();
            HRStaffProfile _Staff = DBI.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == EmpCode);
            DateTime DateNow = DateTime.Now;

            decimal LeaveAdj = EmpClaim.Sum(w => w.Balance).Value;
            if (LeaveB == null)
            {
                LeaveB = new HREmpLeaveB();
                LeaveB = NewEmpLeaveB(EmpCode, _Staff.AllName, LeaveType, Year);
                LeaveB.YTD = (LeaveB.DayEntitle ?? 0) + LeaveAdj;
                LeaveB.Adjustment = LeaveAdj;
                LeaveB.CreateBy = User.UserName;
                LeaveB.CreateOn = DateTime.Now;
                LeaveB = Calculate_Balance(LeaveB);
                DBI.HREmpLeaveBs.Add(LeaveB);
            }
            else
            {
                LeaveB.Adjustment = LeaveAdj;
                LeaveB.YTD = LeaveB.DayEntitle + LeaveB.PH_Edit + LeaveB.Rest_Edit + LeaveB.SeniorityBalance + LeaveAdj;
                LeaveB.Balance = LeaveB.DayEntitle + LeaveB.PH_Edit + LeaveB.Rest_Edit - LeaveB.DayLeave + LeaveB.SeniorityBalance + LeaveAdj;
                LeaveB = Calculate_Balance(LeaveB);
                DBI.HREmpLeaveBs.Attach(LeaveB);
                DBI.Entry(LeaveB).Property(w => w.Balance).IsModified = true;
                DBI.Entry(LeaveB).Property(w => w.YTD).IsModified = true;
                DBI.Entry(LeaveB).Property(w => w.Rest_Edit).IsModified = true;
                DBI.Entry(LeaveB).Property(w => w.PH_Edit).IsModified = true;
                DBI.Entry(LeaveB).Property(w => w.Adjustment).IsModified = true;
            }
            DBI.SaveChanges();
        }
        public HREmpLeaveB Calculate_Balance(HREmpLeaveB LeaveB)
        {
            if (!LeaveB.Adjustment.HasValue) LeaveB.Adjustment = 0;
            if (!LeaveB.SeniorityBalance.HasValue) LeaveB.SeniorityBalance = 0;
            LeaveB.YTD = LeaveB.DayEntitle + LeaveB.SeniorityBalance + LeaveB.Rest_Edit + LeaveB.PH_Edit + LeaveB.Adjustment;
            LeaveB.Balance = LeaveB.DayEntitle + LeaveB.SeniorityBalance + LeaveB.Rest_Edit + LeaveB.PH_Edit + LeaveB.Adjustment - LeaveB.DayLeave;

            return LeaveB;
        }
        public ExDocApproval AddDocApproval(HRStaffProfile Staff, int Step)
        {
            ExDocApproval objApp = new ExDocApproval();
            objApp.Approver = Staff.EmpCode;
            objApp.ApproverName = Staff.AllName;
            objApp.DocumentType = "LR";
            objApp.ApproveLevel = Step;
            objApp.WFObject = "WF02";

            return objApp;
        }
    }
    public class Employee_Generate_Leave
    {
        public string Action { get; set; }
        public string EmpCode { get; set; }
        public string AllName { get; set; }
        public string Gender { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public string ServiceLength { get; set; }
    }
    public class ListLeaveType
    {
        public string LeaveCode { get; set; }
    }
    public partial class Employee_ListForwardLeave
    {
        public string EmpCode { get; set; }
        public string AllName { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public DateTime StartDate { get; set; }
        public decimal Balance { get; set; }
        public string Level { get; set; }
        public int InYear { get; set; }
        public string LeaveCode { get; set; }
        public string DEPT { get; set; }
        public string LevelCode { get; set; }
        public string Branch { get; set; }
        public decimal ForWard { get; set; }
    }
    public class ClsUnits
    {
        public string Description { get; set; }
        public string SecDescription { get; set; }
        public static List<ClsUnits> LoadUnit()
        {
            List<ClsUnits> _lst = new List<ClsUnits>();
            _lst.Add(new ClsUnits { Description = "Day", SecDescription = "ថ្ងៃ" });
            _lst.Add(new ClsUnits { Description = "Hours", SecDescription = "ម៉ោង" });
            return _lst;
        }
    }
    public class ClsPeriodLeave
    {
        public PRParameter payParam { get; set; }
        public List<HRSetEntitleD> ListTempEntitle { get; set; }
        public List<HRLeaveProRate> ListLeaveProRate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int servicelength { get; set; }
        public int ServicePeriod { get; set; }
        public decimal SeniorityBalance { get; set; }
        public decimal Balance { get; set; }
        public decimal Entitle { get; set; }
        public string EmpCode { get; set; }
    }
    public class clsForwards
    {
        public decimal DayLeave { get; set; }
        public decimal Forward { get; set; }
        public decimal ForwardUse { get; set; }
        public decimal ForwardUsed { get; set; }
    }
}//2757