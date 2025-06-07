using Hangfire;
using Humica.Core;
using Humica.Core.DB;
using Humica.Core.FT;
using Humica.Core.Helper;
using Humica.Core.SY;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic.Atts;
using Humica.Logic.CF;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.OleDb;
using System.Linq;
using System.Threading.Tasks;

namespace Humica.Logic.PR
{
    public class PRGenerate_Salary
    {
        public HumicaDBContext DB = new HumicaDBContext();
        SMSystemEntity DMS = new SMSystemEntity();
        public HumicaDBViewContext DBV = new HumicaDBViewContext();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public PREmpAllw Header { get; set; }
        public FTFilterEmployee Filter { get; set; }
        public string ScreenId { get; set; }
        public string CountryCode { get; set; }
        public string MessageCode { get; set; }
        public bool IsInUse { get; set; }
        public bool IsEditable { get; set; }
        public HISGenSalary HeaderSalary { get; set; }
        public List<PREmpAllw> ListHeader { get; set; }
        public List<HR_STAFF_VIEW> ListStaff { get; set; }
        public List<HR_View_EmpGenSalary> ListEmployeeGen { get; set; }
        public List<HR_PR_EmpSalary> ListEmpPaySlip { get; set; }
        public List<HISGenSalaryD> ListBasicSalary { get; set; }
        public List<HISGenOTHour> ListOverTime { get; set; }
        public List<HISGenAllowance> ListAllowance { get; set; }
        public List<HISGenBonu> ListBonus { get; set; }
        public List<HISGenDeduction> ListDeduction { get; set; }
        public List<LeaveDeduction> ListLeaveDed { get; set; }
        public List<PR_GLCharge_View> ListGLCharge { get; set; }
        public List<HisCostCharge> ListCostCharge { get; set; }
        public List<HISPaySlip> ListPaySlip { get; set; }
        public List<EmpSeniority> ListEmpSeniority { get; set; }
        public List<HISApproveSalary> ListApproveSalary { get; set; }
        public List<HisPendingAppSalary> ListAppSalaryPending { get; set; }
        public GEN_Filter GenFilter { get; set; }
        public string EmpID { get; set; }
        public int Progress { get; set; }
        public int InYear { get; set; }
        public bool IsJan { get; set; }
        public bool IsFeb { get; set; }
        public bool IsMar { get; set; }
        public bool IsApr { get; set; }
        public bool IsMay { get; set; }
        public bool IsJun { get; set; }
        public bool IsJul { get; set; }
        public bool IsAug { get; set; }
        public bool IsSep { get; set; }
        public bool IsOct { get; set; }
        public bool IsNov { get; set; }
        public bool IsDec { get; set; }

        public static List<ListProgress> ListProgress { get; set; }
        public List<ExDocApproval> ListApproval { get; set; }
        public HISApproveSalary HeaderAppSalary { get; set; }
        public PRGenerate_Salary()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }
        public void SetAutoApproval(string SCREEN_ID, string DocType)
        {
            ListApproval = new List<ExDocApproval>();
            var DBX = new HumicaDBContext();
            var objDoc = DBX.ExCfWorkFlowItems.Find(SCREEN_ID, DocType);
            if (objDoc != null)
            {
                if (objDoc.IsRequiredApproval == true)
                {
                    var listDefaultApproval = DBX.ExCfWFSalaryApprovers.Where(w => w.WFObject == objDoc.ApprovalFlow && w.IsSelected == true).ToList();
                    foreach (var read in listDefaultApproval)
                    {
                        var objApp = new ExDocApproval();
                        objApp.Approver = read.Employee;
                        objApp.ApproverName = read.EmployeeName;
                        objApp.DocumentType = DocType;
                        objApp.ApproveLevel = read.ApproveLevel;
                        objApp.WFObject = objDoc.ApprovalFlow;
                        ListApproval.Add(objApp);
                    }
                }
            }
        }
        public string isValidApproval(ExDocApproval Approver, EnumActionGridLine Action)
        {
            if (Action == EnumActionGridLine.Add)
            {
                if (ListApproval.Where(w => w.Approver == Approver.Approver).ToList().Count > 0)
                {
                    return "DUPLICATED_ITEM";
                }
            }

            return SYConstant.OK;
        }
        public HRStaffProfile getNextApprover(string id, string pro)
        {
            var objStaff = new HRStaffProfile();
            var DBX = new HumicaDBContext();
            var objHeader = DBX.HISApproveSalaries.Find(id);
            if (objHeader == null)
            {
                return new HRStaffProfile();
            }
            string open = SYDocumentStatus.OPEN.ToString();
            var listCanApproval = DBX.ExDocApprovals.Where(w => w.DocumentNo == id && w.Status == open && w.DocumentType == objHeader.DocumentType).ToList();

            if (listCanApproval.Count == 0)
            {
                return new HRStaffProfile();
            }

            var min = listCanApproval.Min(w => w.ApproveLevel);
            var NextApp = listCanApproval.Where(w => w.ApproveLevel == min).First();
            objStaff = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == NextApp.Approver);//, objHeader.Property);
            return objStaff;
        }
        public List<HR_View_EmpGenSalary> LoadDataEmpGen(FTFilterEmployee Filter1, List<HRBranch> _ListBranch)
        {
            Filter1.FromDate = new DateTime(Filter1.InMonth.Year, Filter1.InMonth.Month, 1);
            Filter1.ToDate = new DateTime(Filter1.InMonth.Year, Filter1.InMonth.Month, DateTime.DaysInMonth(Filter.InMonth.Year, Filter.InMonth.Month));
            var _List = new List<HR_View_EmpGenSalary>();
            var staff = DBV.HR_View_EmpGenSalary.ToList();
            var _listStaff = staff.ToList();
            DateTime date = new DateTime(1900, 1, 1);
            _listStaff = _listStaff.Where(w => w.StartDate.Value.Date <= Filter1.ToDate.Date && ((w.DateTerminate.Date == date.Date
            || w.DateTerminate.AddDays(-1) >= Filter1.FromDate.Date) || (w.SalaryFlag == true
            && w.ReSalary.Year == Filter1.InMonth.Year && w.ReSalary.Month == Filter1.InMonth.Month))).ToList();
            _listStaff = _listStaff.Where(w => _ListBranch.Where(x => x.Code == w.Branch).Any()).ToList();
            if (Filter1.Branch != null)
                _listStaff = _listStaff.Where(w => w.Branch == Filter1.Branch).ToList();
            if (Filter1.Division != null)
                _listStaff = _listStaff.Where(w => w.Division == Filter1.Division).ToList();
            if (Filter1.Department != null)
                _listStaff = _listStaff.Where(w => w.DEPT == Filter1.Department).ToList();
            if (Filter1.Section != null)
                _listStaff = _listStaff.Where(w => w.SECT == Filter1.Section).ToList();
            if (Filter1.Position != null)
                _listStaff = _listStaff.Where(w => w.JobCode == Filter1.Position).ToList();
            if (Filter1.Level != null)
                _listStaff = _listStaff.Where(w => w.LevelCode == Filter1.Level).ToList();
            _listStaff = _listStaff.Where(w => w.IsHold != true).ToList();
            _List = _listStaff;
            return _List.OrderBy(w => w.EmpCode).ToList();
        }
        public List<EmpSeniority> LoadDataSeniority(FTFilterEmployee Filter1, List<HRBranch> _lstBranch)
        {
            DateTime FromDate = new DateTime(Filter1.FromDate.Year, Filter1.FromDate.Month, 1);
            DateTime ToDate = new DateTime(Filter1.ToDate.Year, Filter1.ToDate.Month, DateTime.DaysInMonth(Filter1.ToDate.Year, Filter1.ToDate.Month));
            var _List = new List<EmpSeniority>();
            var _staff = DB.HRStaffProfiles.ToList();
            _staff = _staff.ToList();
            var staff = DBV.HR_View_EmpGenSalary.ToList();
            var _listStaff = staff.ToList();
            DateTime date = new DateTime(1900, 1, 1);
            _listStaff = _listStaff.Where(w => w.StartDate.Value.Date <= ToDate.Date && (w.DateTerminate.Date == date.Date
            || w.DateTerminate.AddDays(-1) >= ToDate.Date)).ToList();
            //Staff Atfer probation
            var _StaffPro = _staff.Where(w => w.Probation.Value.Date >= FromDate.Date && w.Probation.Value.Date <= ToDate.Date).ToList();
            var pro = new List<HRStaffProfile>();
            var PayPar = DB.PRParameters.ToList();
            _StaffPro = _StaffPro.Where(w => w.Probation.Value.Year == ToDate.Year && w.Probation.Value.Month == ToDate.Month).ToList();
            foreach (var item in _StaffPro)
            {
                var _parm = PayPar.FirstOrDefault(w => w.Code == item.PayParam);
                var Day = Get_WorkingDay(_parm, item.Probation.Value, ToDate);
                if (Day < 21)
                {
                    pro.Add(item);
                }
            }
            // _listStaff = _listStaff.Where(w => w.SericeLenghtDay >= 21).ToList();
            _listStaff = _listStaff.Where(w => !pro.Where(x => x.EmpCode == w.EmpCode).Any()).ToList();

            _listStaff = _listStaff.Where(x => _lstBranch.Where(w => w.Code == x.Branch).Any()).ToList();
            if (Filter1.Branch != null)
                _listStaff = _listStaff.Where(w => w.Branch == Filter1.Branch).ToList();
            if (Filter1.Division != null)
                _listStaff = _listStaff.Where(w => w.Division == Filter1.Division).ToList();
            if (Filter1.Department != null)
                _listStaff = _listStaff.Where(w => w.DEPT == Filter1.Department).ToList();
            if (Filter1.Section != null)
                _listStaff = _listStaff.Where(w => w.SECT == Filter1.Section).ToList();
            if (Filter1.Position != null)
                _listStaff = _listStaff.Where(w => w.JobCode == Filter1.Position).ToList();
            if (Filter1.Level != null)
                _listStaff = _listStaff.Where(w => w.LevelCode == Filter1.Level).ToList();
            // if (Filter1.SalaryType != null)
            //    _listStaff = _listStaff.Where(w => w.SalaryType == Filter1.SalaryType).ToList();

            var Emp_Salary = (from C in DB.HISGenSalaries
                              where ((C.FromDate >= FromDate && C.FromDate <= ToDate) || (C.ToDate >= FromDate && C.ToDate <= ToDate) ||
                       (FromDate >= C.FromDate && FromDate <= C.ToDate) || (ToDate >= C.FromDate && ToDate <= C.ToDate))
                              select C).ToList();
            GetDataIsMonth(FromDate, ToDate);
            decimal? TotalAmount = 0;
            
            foreach (var item in _listStaff)
            {
                TotalAmount = 0;
                int CMonth = DateTimeHelper.MonthDiff(FromDate, ToDate) + 1;
                decimal RateInDate = Filter1.Rate / CMonth;
                decimal _SPrate = Filter1.Rate;
                var Staff = _staff.FirstOrDefault(w => w.EmpCode == item.EmpCode);
                var Parameter = PayPar.FirstOrDefault(w => w.Code == Staff.PayParam);
                DateTime FFromDate = DateTimeHelper.StartDateOfMonth(ToDate);
                DateTime FToDate = DateTimeHelper.EndDateOfMonth(ToDate);
                if (!Parameter.IsPrevoiuseMonth.IsNullOrZero())
                {
                    DateTime tempDate = FFromDate.AddMonths(-1);
                    FromDate = new DateTime(tempDate.Year, tempDate.Month, Parameter.FROMDATE.Value().Day);
                    ToDate = new DateTime(ToDate.Year, ToDate.Month, Parameter.TODATE.Value().Day);
                }

                decimal DayInMonth = Get_WorkingDay_Allw(Parameter, FFromDate, FToDate);

                var EmpSin = new EmpSeniority();
                EmpSin.EmpCode = item.EmpCode;
                EmpSin.AllName = item.AllName;
                EmpSin.Department = item.Department;
                EmpSin.Position = item.Position;
                EmpSin.StartDate = item.StartDate.Value;
                EmpSin.Probation = item.Probation.Value;
                EmpSin.Salary = Staff.Salary;
                EmpSin.Rate = 0;
                EmpSin.Balance = 0;
                EmpSin.Average = 0;
                var ListHis = Emp_Salary.Where(w => w.EmpCode == item.EmpCode).ToList();
                
                foreach (var _emp in ListHis)
                {
                    decimal Salary = 0;
                    if (Filter.SalaryType == "BS")
                    {
                        Salary = _emp.Salary.Value;
                    }
                    else if (Filter.SalaryType == "GP")
                    {
                        Salary = _emp.Salary.Value;
                        //Salary = _emp.GrossPay.Value;
                        if( _emp.AlwBeforTax.HasValue)
                        {
                            Salary += _emp.AlwBeforTax.Value;
                        }
                    }
                    else
                    {
                        Salary = _emp.NetWage.Value + _emp.FirstPaymentAmount;
                    }
                    decimal _Seniority = 0;
                    if (_emp.INYear == ToDate.Year && _emp.INMonth == ToDate.Month && _emp.Seniority.HasValue)
                        _Seniority = _emp.Seniority.Value + _emp.SeniorityTaxable.Value;
                    Salary = Salary - _Seniority;
                    if (Staff.Probation.Value.Year > _emp.ToDate.Value.Year)
                    {
                        Salary = 0;
                        CMonth -= 1;
                    }
                    
                    if (Staff.Probation.Value.Year == ToDate.Year)
                    {
                        if (Staff.Probation.Value.Date <= _emp.FromDate.Value.Date)
                        {
                            if (_emp.INYear == Staff.Probation.Value.Year && _emp.INMonth == Staff.Probation.Value.Month)
                            {
                                DateTime Prob = Staff.Probation.Value;
                                DateTime _ToDate = new DateTime(Prob.Year, Prob.Month, DateTime.DaysInMonth(Prob.Year, Prob.Month));
                                decimal _DayInMonth = Get_WorkingDay(Parameter, Prob, _ToDate);
                                if (_DayInMonth < 21)
                                {
                                    Salary = 0;
                                    CMonth -= 1;
                                }
                            }
                        }
                        else
                        {
                            if (_emp.INYear == Staff.Probation.Value.Year && _emp.INMonth == Staff.Probation.Value.Month)
                            {
                                DateTime Prob = Staff.Probation.Value;
                                DateTime _ToDate = new DateTime(Prob.Year, Prob.Month, DateTime.DaysInMonth(Prob.Year, Prob.Month));
                                decimal _DayInMonth = Get_WorkingDay(Parameter, Prob, _ToDate);
                                if (_DayInMonth < 21)
                                {
                                    Salary = 0;
                                    CMonth -= 1;
                                }
                            }
                            else
                            {
                                Salary = 0;
                                CMonth -= 1;
                            }
                        }
                    }
                    GetDataSenior(EmpSin, _emp.ToDate.Value, Salary);
                    TotalAmount += Salary;
                }
                EmpSin.Rate = CMonth * RateInDate;
                if (TotalAmount > 0)
                    EmpSin.Average = ClsRounding.Rounding((TotalAmount / CMonth / DayInMonth).Value(), SYConstant.DECIMAL_PLACE, "N");
                EmpSin.TotalAmount = ClsRounding.Rounding(EmpSin.Average.Value * EmpSin.Rate.Value, SYConstant.DECIMAL_PLACE, "N");
                EmpSin.Remark = CMonth.ToString();
                _List.Add(EmpSin);
            }
            return _List.OrderBy(w => w.EmpCode).ToList();
        }
        public List<HR_PR_EmpSalary> LoadEmpPaySlip(FTFilterEmployee Filter1, List<HRBranch> _ListBranch)
        {
            var _List = new List<HR_PR_EmpSalary>();
            var _lstEmpPaySlip = DBV.HR_PR_EmpSalary.ToList();
            var _staff = DB.HRStaffProfiles.ToList();
            var _listStaff = _staff.ToList();
            DateTime date = new DateTime(1900, 1, 1);
            _lstEmpPaySlip = _lstEmpPaySlip.Where(w => w.INYear == Filter1.InMonth.Year && w.INMonth == Filter1.InMonth.Month).ToList();
            _listStaff = _listStaff.Where(w => _ListBranch.Where(x => x.Code == w.Branch).Any()).ToList();
            if (Filter1.Branch != null)
                _listStaff = _listStaff.Where(w => w.Branch == Filter1.Branch).ToList();
            if (Filter1.Division != null)
                _listStaff = _listStaff.Where(w => w.Division == Filter1.Division).ToList();
            if (Filter1.Department != null)
                _listStaff = _listStaff.Where(w => w.DEPT == Filter1.Department).ToList();
            if (Filter1.Section != null)
                _listStaff = _listStaff.Where(w => w.SECT == Filter1.Section).ToList();
            if (Filter1.Position != null)
                _listStaff = _listStaff.Where(w => w.JobCode == Filter1.Position).ToList();
            if (Filter1.Level != null)
                _listStaff = _listStaff.Where(w => w.LevelCode == Filter1.Level).ToList();

            _List = _lstEmpPaySlip.Where(w => _listStaff.Where(x => x.EmpCode == w.EmpCode).Any()).ToList();
            return _List.OrderBy(w => w.EmpCode).ToList();
        }

        public string Delete_GenerateAll(DateTime InMonth, List<HRBranch> _lstBranch)
        {
            var DBD = new HumicaDBContext();
            try
            {
                DBD.Configuration.AutoDetectChangesEnabled = false;
                DB.Configuration.AutoDetectChangesEnabled = false;
                var AppSalary = DB.HisPendingAppSalaries.Where(w => w.InMonth == InMonth.Month && w.InYear == InMonth.Year && w.IsLock == true).ToList();
                if (AppSalary.Count() > 0)
                {
                    return "APPROVE_SALARY";
                }
                var ListEmp = DB.HISGenSalaries.Where(w => w.INMonth == InMonth.Month && w.INYear == InMonth.Year).ToList();
                ListEmp = ListEmp.Where(x => _lstBranch.Where(w => w.Code == x.Branch).Any()).ToList();
                try
                {
                    var His_GenD = DB.HISGenSalaryDs.Where(w => w.INYear == InMonth.Year && w.INMonth == InMonth.Month).ToList();
                    His_GenD = His_GenD.Where(w => ListEmp.Where(x => x.EmpCode == w.EmpCode).Any()).ToList();
                    foreach (var item in His_GenD)
                    {
                        DBD.HISGenSalaryDs.Attach(item);
                        DBD.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                    }
                    foreach (var item in ListEmp)
                    {
                        DBD.HISGenSalaries.Attach(item);
                        DBD.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                    }
                    var His_GenOT = DB.HISGenOTHours.Where(w => w.INYear == InMonth.Year && w.INMonth == InMonth.Month).ToList();
                    His_GenOT = His_GenOT.Where(w => ListEmp.Where(x => x.EmpCode == w.EmpCode).Any()).ToList();
                    foreach (var item in His_GenOT)
                    {
                        DBD.HISGenOTHours.Attach(item);
                        DBD.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                    }
                    var His_GenAllw = DB.HISGenAllowances.Where(w => w.INYear == InMonth.Year && w.INMonth == InMonth.Month).ToList();
                    His_GenAllw = His_GenAllw.Where(w => ListEmp.Where(x => x.EmpCode == w.EmpCode).Any()).ToList();
                    foreach (var item in His_GenAllw)
                    {
                        DBD.HISGenAllowances.Attach(item);
                        DBD.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                    }
                    var His_GenDed = DB.HISGenDeductions.Where(w => w.INYear == InMonth.Year && w.INMonth == InMonth.Month).ToList();
                    His_GenDed = His_GenDed.Where(w => ListEmp.Where(x => x.EmpCode == w.EmpCode).Any()).ToList();
                    foreach (var item in His_GenDed)
                    {
                        DBD.HISGenDeductions.Attach(item);
                        DBD.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                    }
                    var His_GenBon = DB.HISGenBonus.Where(w => w.INYear == InMonth.Year && w.INMonth == InMonth.Month).ToList();
                    His_GenBon = His_GenBon.Where(w => ListEmp.Where(x => x.EmpCode == w.EmpCode).Any()).ToList();
                    foreach (var item in His_GenBon)
                    {
                        DBD.HISGenBonus.Attach(item);
                        DBD.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                    }
                    var His_GenLeave = DB.HISGenLeaveDeducts.Where(w => w.INYear == InMonth.Year && w.INMonth == InMonth.Month).ToList();
                    His_GenLeave = His_GenLeave.Where(w => ListEmp.Where(x => x.EmpCode == w.EmpCode).Any()).ToList();
                    foreach (var item in His_GenLeave)
                    {
                        DBD.HISGenLeaveDeducts.Attach(item);
                        DBD.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                    }
                    var His_GenPay = DB.HISPaySlips.Where(w => w.INYear == InMonth.Year && w.INMonth == InMonth.Month).ToList();
                    His_GenPay = His_GenPay.Where(w => ListEmp.Where(x => x.EmpCode == w.EmpCode).Any()).ToList();
                    foreach (var item in His_GenPay)
                    {
                        DBD.HISPaySlips.Attach(item);
                        DBD.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                    }
                    var His_TransferToMgrs = DB.HisTransferToMgrs.Where(w => w.InYear == InMonth.Year && w.InMonth == InMonth.Month).ToList();
                    foreach (var item in His_TransferToMgrs)
                    {
                        DBD.HisTransferToMgrs.Attach(item);
                        DBD.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                    }
                    var EmpLoan = DB.HREmpLoans.Where(w => w.PayMonth.Year == InMonth.Year && w.PayMonth.Month == InMonth.Month).ToList();

                    foreach (var read in EmpLoan)
                    {
                        read.Status = SYDocumentStatus.OPEN.ToString();
                        DBD.HREmpLoans.Attach(read);
                        DBD.Entry(read).Property(w => w.Status).IsModified = true;
                    }
                    var His_Gen = DB.HISGLBenCharges.Where(w => w.InYear == InMonth.Year && w.InMonth == InMonth.Month).ToList();
                    if (His_Gen.Count() > 0)
                    {
                        foreach (var item in His_Gen)
                        {
                            DB.HISGLBenCharges.Attach(item);
                            DB.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                            var rowD = DB.SaveChanges();
                        }
                    }
                    int row = DBD.SaveChanges();

                    return SYConstant.OK;
                }
                catch (DbEntityValidationException e)
                {
                    /*------------------SaveLog----------------------------------*/
                    SYEventLog log = new SYEventLog();
                    log.ScreenId = ScreenId;
                    log.UserId = User.UserName;
                    log.DocurmentAction = InMonth.ToString();
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
                    log.DocurmentAction = InMonth.ToString();
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
                    log.DocurmentAction = InMonth.ToString();
                    log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                    SYEventLogObject.saveEventLog(log, e, true);
                    /*----------------------------------------------------------*/
                    return "EE001";
                }
            }
            finally
            {
                DBD.Configuration.AutoDetectChangesEnabled = true;
                DB.Configuration.AutoDetectChangesEnabled = true;
            }
        }
        public void Delete_Generate(GEN_Filter _filter)
        {
            try
            {
                DB.Configuration.AutoDetectChangesEnabled = false;
                var His_GenD = DB.HISGenSalaryDs.Where(w => w.EmpCode == _filter.EmpCode && w.INYear == _filter.InYear && w.INMonth == _filter.InMonth).ToList();
                foreach (var item in His_GenD)
                {
                    DB.HISGenSalaryDs.Attach(item);
                    DB.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                }
                var HisTrans = DB.PRTransferToMgrItems.Where(w => w.EmpCode == _filter.EmpCode && w.EmpCode == _filter.Staff.EmpCode && _filter.Staff.BankAcc != null).ToList();
                foreach (var read in HisTrans)
                {
                    DB.PRTransferToMgrItems.Attach(read);
                    DB.Entry(read).State = System.Data.Entity.EntityState.Deleted;
                }
                var His_Gen = DB.HISGenSalaries.Where(w => w.EmpCode == _filter.EmpCode && w.INYear == _filter.InYear && w.INMonth == _filter.InMonth).ToList();
                foreach (var item in His_Gen)
                {
                    DB.HISGenSalaries.Attach(item);
                    DB.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                }
                var His_GenOT = DB.HISGenOTHours.Where(w => w.EmpCode == _filter.EmpCode && w.INYear == _filter.InYear && w.INMonth == _filter.InMonth).ToList();
                foreach (var item in His_GenOT)
                {
                    DB.HISGenOTHours.Attach(item);
                    DB.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                }
                var His_GenAllw = DB.HISGenAllowances.Where(w => w.EmpCode == _filter.EmpCode && w.INYear == _filter.InYear && w.INMonth == _filter.InMonth).ToList();
                foreach (var item in His_GenAllw)
                {
                    DB.HISGenAllowances.Attach(item);
                    DB.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                }
                var His_GenDed = DB.HISGenDeductions.Where(w => w.EmpCode == _filter.EmpCode && w.INYear == _filter.InYear && w.INMonth == _filter.InMonth).ToList();
                foreach (var item in His_GenDed)
                {
                    DB.HISGenDeductions.Attach(item);
                    DB.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                }
                var His_GenBon = DB.HISGenBonus.Where(w => w.EmpCode == _filter.EmpCode && w.INYear == _filter.InYear && w.INMonth == _filter.InMonth).ToList();
                foreach (var item in His_GenBon)
                {
                    DB.HISGenBonus.Attach(item);
                    DB.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                }
                var His_GenLeave = DB.HISGenLeaveDeducts.Where(w => w.EmpCode == _filter.EmpCode && w.INYear == _filter.InYear && w.INMonth == _filter.InMonth).ToList();
                foreach (var item in His_GenLeave)
                {
                    DB.HISGenLeaveDeducts.Attach(item);
                    DB.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                }
                var His_GenPay = DB.HISPaySlips.Where(w => w.EmpCode == _filter.EmpCode && w.INYear == _filter.InYear && w.INMonth == _filter.InMonth).ToList();
                foreach (var item in His_GenPay)
                {
                    DB.HISPaySlips.Attach(item);
                    DB.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                }
                var His_TransferToMgrs = DB.HisTransferToMgrs.Where(w => w.EmpCode == _filter.EmpCode && w.InYear == _filter.InYear && w.InMonth == _filter.InMonth).ToList();
                foreach (var item in His_TransferToMgrs)
                {
                    DB.HisTransferToMgrs.Attach(item);
                    DB.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                }

                var His_GenGL = DB.HISGLBenCharges.Where(w => w.EmpCode == _filter.EmpCode && w.InYear == _filter.InYear && w.InMonth == _filter.InMonth).ToList();
                if (His_GenGL.Count() > 0)
                {
                    foreach (var item in His_GenGL)
                    {
                        DB.HISGLBenCharges.Attach(item);
                        DB.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                        var rowD = DB.SaveChanges();
                    }
                }
                int row = DB.SaveChanges();
            }
            finally
            {
                DB.Configuration.AutoDetectChangesEnabled = true;
            }
        }
        public void Generate_Salary(GEN_Filter _filter,
            HRBranch _Branch, List<HRCareerHistory> LstCarcode, ref bool IsGenerate)
        {
            try
            {
                DB.Configuration.AutoDetectChangesEnabled = false;
                var _listGenD = new List<HISGenSalaryD>();
                decimal TempSalary = 0;
                decimal Rate = 0;
                decimal Amount = 0;
                decimal WorkHourPerDay = Convert.ToDecimal(_filter.Parameter.WHOUR);
                decimal PWorkDate = Get_WorkingDay(_filter.Parameter, _filter.FromDate, _filter.ToDate);
                decimal DayInMonth = Get_WorkingDay_Salary(_filter.Parameter, _filter.FromDate, _filter.ToDate);
                var Emp_C = from C in _filter.LstEmpCareer
                            where ((C.FromDate >= _filter.FromDate && C.FromDate <= _filter.ToDate) || (C.ToDate >= _filter.FromDate && C.ToDate <= _filter.ToDate) ||
                     (_filter.FromDate >= C.FromDate && _filter.FromDate <= C.ToDate) || (_filter.ToDate >= C.FromDate && _filter.ToDate <= C.ToDate))
                     && C.EmpCode == _filter.EmpCode
                            select new { C.CompanyCode,C.CareerCode, C.EmpCode, C.EmpType, C.Branch, C.LOCT, C.Division, C.DEPT, C.LINE, C.CATE, C.SECT, C.JobCode, C.LevelCode, C.NewSalary, C.FromDate, C.ToDate, C.resigntype };
                Emp_C = Emp_C.ToList();
                if (LstCarcode.Where(w => w.NotCalSalary == true && Emp_C.Where(x => x.CareerCode == w.Code).Any()).Any())
                {
                    var _lstInAct = Emp_C.Where(w => LstCarcode.Where(x => w.CareerCode == x.Code).Any()).ToList();
                    Emp_C = Emp_C.Where(w => !LstCarcode.Where(x => w.CareerCode == x.Code).Any()).ToList();
                    var ResType = DB.HRTerminTypes.Where(w => w.IsCalSalary == true).ToList();
                    if (!_lstInAct.Where(w => ResType.Where(x => x.Code == w.resigntype).Any()).Any()
                        && Emp_C.Where(w => w.CareerCode != "RENEW").Any())
                    {
                        return;
                    }
                }
                int C_Career = Emp_C.Count();
                foreach (var emp in Emp_C)
                {

                    DateTime PToDate = emp.ToDate.Value;
                    DateTime PFromDate = emp.FromDate.Value;
                    if (PToDate > _filter.ToDate) PToDate = _filter.ToDate;
                    if (PFromDate < _filter.FromDate) PFromDate = _filter.FromDate;
                    Decimal ActualWorkDay = Get_WorkingDay(_filter.Parameter, PFromDate, PToDate);
                    if (PFromDate == _filter.FromDate && PToDate == _filter.ToDate)
                    {
                        TempSalary = emp.NewSalary;
                    }
                    else
                    {
                        decimal TMPD = 0;
                        //if (emp.CareerCode == "NEWJOIN" && C_Career == 1 && _filter.Parameter.SALWKTYPE == 3)
                        //{
                        //    ActualWorkDay = DayInMonth - Get_WorkingDay(_filter.Parameter, FromDate, PFromDate);
                        //}
                        TMPD = Convert.ToDecimal(_listGenD.Sum(x => x.ActWorkDay));
                        if ((TMPD + ActualWorkDay) > DayInMonth) ActualWorkDay = Convert.ToDecimal(DayInMonth) - TMPD;
                        Rate = Convert.ToDecimal(emp.NewSalary / DayInMonth);
                        TempSalary = Math.Round((Rate * ActualWorkDay), 2);
                    }
                    TempSalary = Math.Round(TempSalary, SYConstant.DECIMAL_PLACE);
                    Rate = Convert.ToDecimal(emp.NewSalary / DayInMonth);
                    var _His_GenD = new HISGenSalaryD()
                    {
                        CompanyCode = emp.CompanyCode,
                        INYear = _filter.InYear,
                        INMonth = _filter.InMonth,
                        FromDate = _filter.FromDate,
                        ToDate = _filter.ToDate,
                        CareerCode = emp.CareerCode,
                        WorkDay = DayInMonth,
                        WorkHour = WorkHourPerDay,
                        EmpCode = emp.EmpCode,
                        EmpType = emp.EmpType,
                        Branch = emp.Branch,
                        Location = emp.LOCT,
                        Division = emp.Division,
                        DEPT = emp.DEPT,
                        LINE = emp.LINE,
                        CATE = emp.CATE,
                        Sect = emp.SECT,
                        POST = emp.JobCode,
                        LevelCode = emp.LevelCode,
                        PayFrom = PFromDate,
                        PayTo = PToDate,
                        ActWorkDay = Convert.ToDecimal(ActualWorkDay),
                        BasicSalary = emp.NewSalary,
                        Rate = Rate,
                        Amount = TempSalary,
                        CreateBy = User.UserName,
                        CreateOn = DateTime.Now
                    };
                    _listGenD.Add(_His_GenD);
                    DB.HISGenSalaryDs.Add(_His_GenD);
                }
                //---------------------------------------
                var ExchangeRate = DB.PRExchRates.FirstOrDefault(w => w.InYear == _filter.InYear && w.InMonth == _filter.InMonth);
                var EmpFamaily = DB.HREmpFamilies.Where(w => w.EmpCode == _filter.EmpCode).ToList();
                var Except = DB.SYHRSettings.FirstOrDefault();
                var EMP_GENERATESALARY_C = (from Emp_salary in _listGenD
                                            group Emp_salary by new { Emp_salary.EmpCode }
                                           into myGroup
                                            where myGroup.Count() > 0
                                            select new
                                            {
                                                myGroup.Key.EmpCode,
                                                ActWorkDay = myGroup.Sum(w => w.ActWorkDay),
                                                Amount = myGroup.Sum(w => w.Amount),
                                                ActworkHour = myGroup.Sum(w => w.ActWorkHours)
                                            }).ToList();
                var staff = _filter.Staff;
                var AdvPay = DB.PRADVPays.Where(w => w.EmpCode == _filter.EmpCode && w.TranDate.Value.Year == _filter.InYear && w.TranDate.Value.Month == _filter.InMonth).ToList();
                var _Loan = DB.HREmpLoans.Where(w => w.EmpCode == _filter.EmpCode && w.PayMonth.Year == _filter.InYear && w.PayMonth.Month == _filter.InMonth).ToList();
                var empCardId = DB.HREmpIdentities.FirstOrDefault(w => w.EmpCode == _filter.EmpCode && w.IdentityTye == "IDCard");
                string IDCard = "";
                if (empCardId != null)
                    IDCard = empCardId.IDCardNo;
                //
                decimal? BaseSalary = 0;
                decimal? _Increased = 0;
                if (_listGenD.Count > 0)
                {
                    BaseSalary = _listGenD.OrderBy(x => x.PayTo).FirstOrDefault().BasicSalary;
                }
                foreach (var Emp in EMP_GENERATESALARY_C)
                {
                    int Child = EmpFamaily.Where(w => w.Child == true && w.TaxDeduc == true).ToList().Count;
                    int Spouse = EmpFamaily.Where(w => w.Spouse == true && w.TaxDeduc == true).ToList().Count;
                    decimal? ChildAm = 0;
                    if (Except.Child.HasValue)
                        ChildAm = Convert.ToDecimal(Child * Except.Child.Value);
                    decimal? SpouseAm = 0;
                    if (Except.Spouse.HasValue)
                        SpouseAm = Convert.ToDecimal(Spouse * Except.Spouse.Value);
                    if (Emp.Amount - BaseSalary > 0)
                        _Increased = Emp.Amount - BaseSalary;
                    else if (LstCarcode.Where(x => x.Code == staff.CareerDesc).Any() && (Emp.Amount - BaseSalary) > 0)
                        _Increased = Emp.Amount - BaseSalary;

                    var _GenSala = new HISGenSalary()
                    {
                        CompanyCode = staff.CompanyCode,
                        Status = SYDocumentStatus.OPEN.ToString(),
                        TermStatus = staff.TerminateStatus,
                        TermRemark = staff.TerminateRemark,
                        TermDate = staff.DateTerminate,
                        INYear = _filter.InYear,
                        INMonth = _filter.InMonth,
                        FromDate = _filter.FromDate,
                        ToDate = _filter.ToDate,
                        WorkDay = PWorkDate,
                        WorkHour = WorkHourPerDay,
                        ExchRate = ExchangeRate.Rate,
                        NSSFRate = ExchangeRate.NSSFRate,
                        EmpCode = _filter.EmpCode,
                        EmpName = staff.AllName,
                        EmpType = staff.EmpType,
                        Branch = staff.Branch,
                        Location = staff.LOCT,
                        Division = staff.Division,
                        DEPT = staff.DEPT,
                        LINE = staff.Line,
                        CATE = staff.CATE,
                        Sect = staff.SECT,
                        POST = staff.JobCode,
                        JobGrade = staff.JobGrade,
                        LevelCode = staff.LevelCode,
                        Sex = staff.Sex,
                        ICNO = IDCard,
                        SOCSO = staff.SOCSO,
                        BankName = staff.BankName,
                        BankBranch = staff.BankBranch,
                        BankAcc = staff.BankAcc,
                        Spouse = Spouse,
                        Child = Child,
                        DateJoin = staff.StartDate,
                        ActWorkDay = Convert.ToInt32(Emp.ActWorkDay),
                        Salary = Emp.Amount,
                        ADVPay = Convert.ToDecimal(AdvPay.Sum(w => w.Amount)),
                        UTAXCH = Convert.ToDecimal(ChildAm),
                        UTAXSP = Convert.ToDecimal(SpouseAm),
                        USERGEN = User.UserName,
                        DATEGEN = DateTime.Now,
                        TAXBONAM = 0,
                        CostCenter = staff.Costcent,
                        TXPayType = staff.TXPayType,
                        NWAM = 0,
                        PayBack = _filter.LstEmpHold.Sum(w => w.Salary),
                        ShiftPay = 0,
                        LOAN = _Loan.Sum(w => w.LoanAm),
                        Increased = _Increased,
                        SFT_Salary = staff.SalaryTax,
                        SFT_AmToBeTax = 0,
                        SFT_GrossPay = 0,
                        SFT_Tax = 0,
                        SFT_TaxRate = 0,
                        SFT_NetPay = 0,
                        FirstPaymentAmount = 0
                    };
                    Amount = Convert.ToDecimal(Emp.Amount);
                    _filter.HisGensalarys = _GenSala;
                }
                if (_listGenD.Count == 0)
                {
                    if (_filter.Staff.ReSalary.Year == _filter.InYear && _filter.Staff.ReSalary.Month == _filter.InMonth && _filter.Staff.SalaryFlag == true)
                    {
                        var _EmpCareer = _filter.LstEmpCareer.FirstOrDefault(w => LstCarcode.Where(x => x.Code == w.CareerCode).Any() && w.EmpCode == _filter.EmpCode);
                        var _His_GenD = new HISGenSalaryD()
                        {
                            CompanyCode = _EmpCareer.CompanyCode,
                            INYear = _filter.InYear,
                            INMonth = _filter.InMonth,
                            FromDate = _filter.FromDate,
                            ToDate = _filter.ToDate,
                            CareerCode = _EmpCareer.CareerCode,
                            WorkDay = DayInMonth,
                            WorkHour = WorkHourPerDay,
                            EmpCode = _EmpCareer.EmpCode,
                            EmpType = _EmpCareer.EmpType,
                            Branch = _EmpCareer.Branch,
                            Location = _EmpCareer.LOCT,
                            Division = _EmpCareer.Division,
                            DEPT = _EmpCareer.DEPT,
                            LINE = _EmpCareer.LINE,
                            CATE = _EmpCareer.CATE,
                            Sect = _EmpCareer.SECT,
                            POST = _EmpCareer.JobCode,
                            LevelCode = _EmpCareer.LevelCode,
                            PayFrom = _filter.FromDate,
                            PayTo = _filter.ToDate,
                            ActWorkDay = 0,
                            BasicSalary = _EmpCareer.NewSalary,
                            Rate = Rate,
                            Amount = TempSalary,
                            CreateBy = User.UserName,
                            CreateOn = DateTime.Now
                        };
                        var _GenSala = new HISGenSalary()
                        {
                            CompanyCode = staff.CompanyCode,
                            Status = SYDocumentStatus.OPEN.ToString(),
                            TermStatus = staff.TerminateStatus,
                            TermRemark = staff.TerminateRemark,
                            TermDate = staff.DateTerminate,
                            INYear = _filter.InYear,
                            INMonth = _filter.InMonth,
                            FromDate = _filter.FromDate,
                            ToDate = _filter.ToDate,
                            WorkDay = PWorkDate,
                            WorkHour = WorkHourPerDay,
                            ExchRate = ExchangeRate.Rate,
                            NSSFRate = ExchangeRate.NSSFRate,
                            EmpCode = _filter.EmpCode,
                            EmpName = staff.AllName,
                            EmpType = staff.EmpType,
                            Branch = staff.Branch,
                            Location = staff.LOCT,
                            Division = staff.Division,
                            DEPT = staff.DEPT,
                            LINE = staff.Line,
                            CATE = staff.CATE,
                            Sect = staff.SECT,
                            JobGrade = staff.JobCode,
                            LevelCode = staff.LevelCode,
                            Sex = staff.Sex,
                            ICNO = IDCard,
                            SOCSO = staff.SOCSO,
                            BankName = staff.BankName,
                            BankBranch = staff.BankBranch,
                            BankAcc = staff.BankAcc,
                            Spouse = 0,
                            Child = 0,
                            DateJoin = staff.StartDate,
                            ActWorkDay = 0,
                            Salary = 0,
                            ADVPay = Convert.ToDecimal(AdvPay.Sum(w => w.Amount)),
                            UTAXCH = 0,
                            UTAXSP = 0,
                            USERGEN = User.UserName,
                            DATEGEN = DateTime.Now,
                            TAXBONAM = 0,
                            CostCenter = staff.Costcent,
                            TXPayType = staff.TXPayType,
                            NWAM = 0,
                            PayBack = _filter.LstEmpHold.Sum(w => w.Salary),
                            ShiftPay = 0,
                            LOAN = _Loan.Sum(w => w.LoanAm),
                            FirstPaymentAmount = 0
                        };
                        Amount = 0;
                        DB.HISGenSalaryDs.Add(_His_GenD);
                        _filter.HisGensalarys = _GenSala;
                    }
                }
                _filter.LstPayHis.Add(new ClsPayHis()
                {
                    EmpCode = _filter.EmpCode,
                    SGROUP = "A",
                    PayType = "BS",
                    Code = "BS",
                    Description = "BASIC SALARY",
                    Amount = Amount
                });

                var lstTranMan = DB.PRTransferToMgrItems.Where(w => w.EmpCode == _filter.EmpCode);
                foreach (var item in lstTranMan)
                {
                    var read = new HisTransferToMgr();
                    read.EmpCode = item.EmpCode;
                    read.InMonth = _filter.InMonth;
                    read.InYear = _filter.InYear;
                    read.FromDate = _filter.FromDate;
                    read.ToDate = _filter.ToDate;
                    read.HOD = item.HOD;
                    DB.HisTransferToMgrs.Add(read);
                }
                //Update Status Loan
                foreach (var read in _Loan)
                {
                    read.Status = SYDocumentStatus.CLEARED.ToString();
                    DB.HREmpLoans.Attach(read);
                    DB.Entry(read).Property(w => w.Status).IsModified = true;
                }
                if (_filter.HisAppSalary == null && IsGenerate == false)
                {
                    IsGenerate = true;
                    var _AppSalary = new HisPendingAppSalary();
                    _AppSalary.InYear = _filter.InYear;
                    _AppSalary.InMonth = _filter.InMonth;
                    _AppSalary.FromDate = _filter.FromDate;
                    _AppSalary.ToDate = _filter.ToDate;
                    _AppSalary.IsLock = false;
                    DB.HisPendingAppSalaries.Add(_AppSalary);
                }
                int row = DB.SaveChanges();
            }
            finally
            {
                DB.Configuration.AutoDetectChangesEnabled = true;
            }
        }
        public void Calculate_OT(GEN_Filter _filter, HRBranch _Branch)
        {
            try
            {
                DB.Configuration.AutoDetectChangesEnabled = false;
                var OTRate = DB.PROTRates.ToList();
                decimal? TempRate = 0;
                decimal Rate = 0;
                decimal OTAmount = 0;
                decimal WorkHourPerDay = Convert.ToDecimal(_filter.Parameter.WHOUR);
                var EmpOT = DB.PREmpOverTimes.Where(w => w.EmpCode == _filter.EmpCode && w.PayMonth.Value.Year == _filter.InYear && w.PayMonth.Value.Month == _filter.InMonth).ToList();

                foreach (var OT in EmpOT.OrderBy(w => w.OTDate))
                {
                    DateTime FirstDate = new DateTime(OT.OTDate.Value.Year, OT.OTDate.Value.Month, 1);
                    DateTime LastDate = new DateTime(OT.OTDate.Value.Year, OT.OTDate.Value.Month, DateTime.DaysInMonth(OT.OTDate.Value.Year, OT.OTDate.Value.Month));
                    decimal WorkDayPerMonth = Get_WorkingDay_Salary(_filter.Parameter, FirstDate, LastDate);
                    if (_filter.LstEmpCareer.Where(w => OT.OTDate >= w.FromDate && OT.OTDate <= w.ToDate).Any())
                    {
                        decimal BaseSalary = Convert.ToDecimal(_filter.LstEmpCareer.FirstOrDefault(w => OT.OTDate >= w.FromDate && OT.OTDate <= w.ToDate).NewSalary);
                        var Result = OTRate.FirstOrDefault(w => w.OTCode == OT.OTType);
                        Rate = BaseSalary / WorkDayPerMonth;
                        if (Result.Soperator == "+")
                            Rate = Convert.ToDecimal(Rate + Result.Toperand);
                        else if (Result.Soperator == "-")
                            Rate = Convert.ToDecimal(Rate - Result.Toperand);
                        else if (Result.Soperator == "/")
                            Rate = Convert.ToDecimal(Rate / Result.Toperand);
                        else if (Result.Soperator == "*")
                            Rate = Convert.ToDecimal(Rate * Result.Toperand);
                        if (Result.Measure == "H") TempRate = Convert.ToDecimal(Rate / WorkHourPerDay);
                        else if (Result.Measure == "D") TempRate = Convert.ToDecimal(Rate);

                        OTAmount = OTAmount + (Convert.ToDecimal(OT.OTHour) * TempRate.Value());
                        //OTAmount = Math.Round(OTAmount, SYConstant.DECIMAL_PLACE);
                        var Gen_OT = new HISGenOTHour()
                        {
                            INYear = _filter.InYear,
                            INMonth = _filter.InMonth,
                            EmpCode = _filter.EmpCode,
                            OTDate = OT.OTDate,
                            BaseSalary = BaseSalary,
                            WorkDay = WorkDayPerMonth,
                            WorkHour = WorkHourPerDay,
                            OTHour = OT.OTHour,
                            OTDesc = Result.OTType,
                            OTHDesc = Result.OTHDESC,
                            OTCode = OT.OTType,
                            OTRate = TempRate,
                            OTFormula = "(" + Result.Foperand + ")" + Result.Soperator + Result.Toperand,
                            Measure = Result.Measure,
                            Amount = TempRate * Convert.ToDecimal(OT.OTHour),//Math.Round(TempRate * Convert.ToDecimal(OT.OTHour), SYConstant.DECIMAL_PLACE),
                            CreateBy = User.UserName,
                            CreateOn = DateTime.Now
                        };
                        _filter.ListHisEmpOT.Add(Gen_OT);
                        DB.HISGenOTHours.Add(Gen_OT);

                        if (_filter.LstPayHis.Where(w => w.PayType == "OT" && w.Code == OT.OTType).Any())
                        {
                            _filter.LstPayHis.Where(w => w.PayType == "OT" && w.Code == OT.OTType).ToList().ForEach(x => x.Amount += Convert.ToDecimal(Gen_OT.Amount));
                        }
                        else
                        {
                            _filter.LstPayHis.Add(new ClsPayHis()
                            {
                                EmpCode = _filter.EmpCode,
                                SGROUP = "D",
                                PayType = "OT",
                                Code = OT.OTType,
                                Description = OT.OTDescription.ToUpper(),
                                Amount = Convert.ToDecimal(Gen_OT.Amount)
                            });
                        }
                    }
                }

                _filter.HisGensalarys.OTAM = OTAmount;
            }
            finally
            {
                DB.Configuration.AutoDetectChangesEnabled = true;
            }
        }
        public void Calculate_Allowance(GEN_Filter _filter)
        {
            try
            {
                DB.Configuration.AutoDetectChangesEnabled = false;
                decimal ActualWorkDay = 0;
                decimal TempAmount = 0;
                decimal exchangeRate = Filter.ExchangeRate;
                PRSRewardType rewardType = new PRSRewardType();
                #region ********Allw InMonth********
                var EmpAllw = _filter.LstEmpAllow.Where(w => w.EmpCode == _filter.EmpCode && w.FromDate.Value.Year == _filter.InYear && w.FromDate.Value.Month == _filter.InMonth &&
                   w.ToDate.Value.Year == _filter.InYear && w.ToDate.Value.Month == _filter.InMonth && w.Status == 1).ToList();
                var AllwType = rewardType.GetRewardsType(_filter.LstRewardsType, RewardTypeCode.ALLW.ToString());

                var HRSettingObj = DB.SYHRSettings;
                SYHRSetting syHRSetting = HRSettingObj.FirstOrDefault();
                decimal seniorityAmount = 0;
                string[] allowanceType = { "SP" };
                foreach (var Allw in EmpAllw)
                {
                    ActualWorkDay = _filter.ToDate.Subtract(_filter.FromDate).Days + 1;
                    var _allw = AllwType.FirstOrDefault(w => w.Code == Allw.AllwCode);
                    if (_allw != null)
                    {
                        var _GenAllw = new HISGenAllowance()
                        {
                            INYear = _filter.InYear,
                            INMonth = _filter.InMonth,
                            EmpCode = _filter.EmpCode,
                            FromDate = Allw.FromDate,
                            ToDate = Allw.ToDate,
                            WorkDay = 0,
                            RatePerDay = 0,
                            AllwCode = Allw.AllwCode,
                            AllwDesc = _allw.Description,
                            OthDesc = _allw.OthDesc,
                            TaxAble = _allw.Tax,
                            FringTax = _allw.FTax,
                            AllwAm = Allw.Amount,
                            AllwAmPM = Allw.Amount / ActualWorkDay,
                            CreateBy = User.UserName,
                            CreateOn = DateTime.Now
                        };
                        _filter.LstPayHis.Add(new ClsPayHis()
                        {
                            EmpCode = _filter.EmpCode,
                            SGROUP = "B",
                            PayType = "ALW",
                            Code = _allw.Code,
                            Description = Allw.AllwDescription.ToUpper(),
                            Amount = Convert.ToDecimal(Allw.Amount)
                        });
                        _filter.ListHisEmpAllw.Add(_GenAllw);
                        DB.HISGenAllowances.Add(_GenAllw);
                    }
                }
                //  var WorkDay = DB.HISGenSalaries.FirstOrDefault(w => w.EmpCode == _filter.EmpCode && w.INYear == _filter.InYear && w.INMonth == _filter.InMonth).WorkDay;
                var BaseSalary = _filter.Staff.Salary;
                #endregion

                #region ********Allw Period********
                var EmpAllwP = _filter.LstEmpAllow.Where(w => w.EmpCode == _filter.EmpCode && ((_filter.FromDate >= w.FromDate && _filter.FromDate <= w.ToDate) || (_filter.ToDate >= w.FromDate && _filter.ToDate <= w.ToDate))
                && w.Status == 0).ToList();
                foreach (var Allw in EmpAllwP)
                {
                    if (Allw.AllwCode is "SP") continue;
                    ActualWorkDay = Get_WorkingDay_Allw(_filter.Parameter, _filter.FromDate, _filter.ToDate);
                    decimal? ALWperDay = Allw.Amount / ActualWorkDay;
                    DateTime TempGenFromDate = new DateTime();
                    DateTime TempGenToDate = new DateTime();
                    var _allw = AllwType.FirstOrDefault(w => w.Code == Allw.AllwCode);
                    if (_allw != null)
                    {
                        int HasAllowanceDetail = 0;
                        if (_filter.FromDate.Date >= Allw.FromDate.Value.Date && _filter.ToDate.Date <= Allw.ToDate.Value.Date)
                        {
                            TempGenFromDate = _filter.FromDate;
                            TempGenToDate = _filter.ToDate;
                            HasAllowanceDetail = 1;
                        }
                        else if (_filter.FromDate >= Allw.FromDate && Allw.ToDate <= _filter.ToDate)
                        {
                            TempGenFromDate = _filter.FromDate;
                            TempGenToDate = Allw.ToDate.Value;
                            HasAllowanceDetail = 1;
                        }
                        else if (Allw.FromDate >= _filter.FromDate && Allw.FromDate <= _filter.ToDate && Allw.ToDate >= _filter.ToDate)
                        {
                            TempGenFromDate = Allw.FromDate.Value;
                            TempGenToDate = _filter.ToDate;
                            HasAllowanceDetail = 1;
                        }
                        else if (Allw.FromDate >= _filter.FromDate && Allw.ToDate <= _filter.ToDate)
                        {
                            TempGenFromDate = Allw.FromDate.Value;
                            TempGenToDate = Allw.ToDate.Value;
                            HasAllowanceDetail = 1;
                        }
                        if (HasAllowanceDetail == 1)
                        {
                            if (_filter.Staff.StartDate.Value.Date > TempGenFromDate)
                            {
                                TempGenFromDate = _filter.Staff.StartDate.Value;
                            }

                            decimal TempWorkDay = Get_WorkingDay(_filter.Parameter, TempGenFromDate, TempGenToDate);
                            if (TempGenFromDate == _filter.FromDate && TempGenToDate == _filter.ToDate)
                            {
                                TempAmount = Convert.ToDecimal(Allw.Amount);
                            }
                            else
                            {
                                TempAmount = Convert.ToDecimal((Allw.Amount / ActualWorkDay) * TempWorkDay);
                            }
                            var _GenAllw = new HISGenAllowance()
                            {
                                INYear = _filter.InYear,
                                INMonth = _filter.InMonth,
                                EmpCode = _filter.EmpCode,
                                FromDate = TempGenFromDate,
                                ToDate = TempGenToDate,
                                WorkDay = Convert.ToInt32(TempWorkDay),
                                RatePerDay = ALWperDay,
                                AllwCode = Allw.AllwCode,
                                AllwDesc = _allw.Description,
                                OthDesc = _allw.OthDesc,
                                TaxAble = _allw.Tax,
                                FringTax = _allw.FTax,
                                AllwAm = TempAmount,
                                AllwAmPM = Allw.Amount,
                                CreateBy = User.UserName,
                                CreateOn = DateTime.Now
                            };
                            _filter.ListHisEmpAllw.Add(_GenAllw);
                            DB.HISGenAllowances.Add(_GenAllw);
                            _filter.LstPayHis.Add(new ClsPayHis()
                            {
                                EmpCode = _filter.EmpCode,
                                SGROUP = "B",
                                PayType = "ALW",
                                Code = _allw.Code,
                                Description = Allw.AllwDescription.ToUpper(),
                                Amount = Convert.ToDecimal(TempAmount)
                            });
                        }
                    }
                }
                #endregion
                // seniority allowance
                string empType = string.Empty;
                if (syHRSetting != null && !syHRSetting.IsTax.IsNullOrZero() && _filter.ListHisEmpAllw.Count > 0)
                {
                    empType = syHRSetting.EmpType;
                    List<HISGenAllowance> allowances = _filter.ListHisEmpAllw;
                    decimal amountUS = allowances.Where(x => allowanceType.Contains(x.AllwCode))
                        .Sum(x => x.AllwAm).ToDecimal();
                    decimal amountKH = amountUS * exchangeRate;
                    if (_filter.HisGensalarys.EmpType == empType)
                    {
                        seniorityAmount += amountUS;
                    }
                    else if (amountKH > syHRSetting.SeniorityException.ToDecimal())
                    {
                        seniorityAmount += ((amountKH - syHRSetting.SeniorityException.ToDecimal()) / exchangeRate);
                    }
                }
                _filter.HisGensalarys.SeniorityTaxable = ClsRounding.Rounding(seniorityAmount, SYConstant.DECIMAL_PLACE, "N");


                #region ********Service Charge********
                var _ListSVC = DB.HISSVCMonthlies.Where(w => w.EmpCode == _filter.EmpCode && w.InYear == _filter.InYear && w.InMonth == _filter.InMonth).ToList();
                var SVCAm = _ListSVC.Sum(w => w.Amount);
                if (SVCAm > 0)
                {
                    ActualWorkDay = _filter.ToDate.Subtract(_filter.FromDate).Days + 1;
                    var AllwTypeSVC = _filter.LstRewardsType.Where(w => w.ReCode == "ALLW").ToList();
                    var _allw = AllwType.FirstOrDefault(w => w.Code == "SVC");
                    if (_allw != null)
                    {
                        var _GenAllw = new HISGenAllowance()
                        {
                            INYear = _filter.InYear,
                            INMonth = _filter.InMonth,
                            EmpCode = _filter.EmpCode,
                            FromDate = _filter.FromDate,
                            ToDate = _filter.ToDate,
                            WorkDay = 0,
                            RatePerDay = 0,
                            AllwCode = _allw.Code,
                            AllwDesc = _allw.Description,
                            OthDesc = _allw.OthDesc,
                            TaxAble = _allw.Tax,
                            FringTax = _allw.FTax,
                            AllwAm = SVCAm,
                            AllwAmPM = SVCAm / ActualWorkDay,
                            CreateBy = User.UserName,
                            CreateOn = DateTime.Now
                        };
                        _filter.LstPayHis.Add(new ClsPayHis()
                        {
                            EmpCode = _filter.EmpCode,
                            SGROUP = "B",
                            PayType = "ALW",
                            Code = _allw.Code,
                            Description = _allw.Description.ToUpper(),
                            Amount = Convert.ToDecimal(SVCAm)
                        });
                        _filter.ListHisEmpAllw.Add(_GenAllw);
                        DB.HISGenAllowances.Add(_GenAllw);
                    }
                }
                #endregion

                #region ********Leave Balance********
                var Emp_C = from C in _filter.LstEmpCareer
                            where ((C.FromDate >= _filter.FromDate && C.FromDate <= _filter.ToDate) || (C.ToDate >= _filter.FromDate && C.ToDate <= _filter.ToDate) ||
                     (_filter.FromDate >= C.FromDate && _filter.FromDate <= C.ToDate) || (_filter.ToDate >= C.FromDate && _filter.ToDate <= C.ToDate))
                     && C.EmpCode == _filter.EmpCode && C.resigntype != "RES"
                            select new { C.NewSalary, C.FromDate, C.resigntype };
                decimal DayInMonth = Get_WorkingDay_Salary(_filter.Parameter, _filter.FromDate, _filter.ToDate);
                if (_filter.Staff.TerminateStatus != "")
                {
                    var _allw = AllwType.FirstOrDefault(w => w.Code == "ALPALY");
                    if (_allw != null)
                    {
                        var ALBalance = DB.HREmpLeaveBs.FirstOrDefault(w => w.EmpCode == _filter.EmpCode && w.InYear == _filter.InYear && w.InMonth == _filter.InMonth && w.LeaveCode == "AL");
                        if (ALBalance != null)
                        {
                            decimal Balance = Convert.ToDecimal((BaseSalary / DayInMonth) * ALBalance.ALPayBalance);
                            if (Balance > 0)
                            {
                                var _GenAllw = new HISGenAllowance()
                                {
                                    INYear = _filter.InYear,
                                    INMonth = _filter.InMonth,
                                    EmpCode = _filter.EmpCode,
                                    FromDate = _filter.FromDate,
                                    ToDate = _filter.ToDate,
                                    WorkDay = 0,
                                    RatePerDay = 0,
                                    AllwCode = "ALPALY",
                                    AllwDesc = _allw.Description,
                                    OthDesc = _allw.OthDesc,
                                    TaxAble = _allw.Tax,
                                    FringTax = _allw.FTax,
                                    AllwAm = Balance,
                                    AllwAmPM = Balance / ActualWorkDay,
                                    CreateBy = User.UserName,
                                    CreateOn = DateTime.Now,
                                    Remark = "Generate From System"
                                };
                                _filter.ListHisEmpAllw.Add(_GenAllw);
                                DB.HISGenAllowances.Add(_GenAllw);
                                _filter.LstPayHis.Add(new ClsPayHis()
                                {
                                    EmpCode = _filter.EmpCode,
                                    SGROUP = "B",
                                    PayType = "ALW",
                                    Code = _allw.Code,
                                    Description = _allw.Description.ToUpper(),
                                    Amount = Convert.ToDecimal(Balance)
                                });
                            }
                        }
                    }
                }
                #endregion

                #region ********Maternity Leave********
                //var EmpLeave = DB.HREmpLeaveDs.Where(w => w.EmpCode == _filter.EmpCode && w.LeaveCode == "ML" && w.LeaveDate.Year == _filter.InYear && w.LeaveDate.Month == _filter.InMonth).ToList();
                //if (EmpLeave.Count > 0)
                //{
                //    var _allw = AllwType.FirstOrDefault(w => w.Code == "LB");
                //}
                #endregion
                DB.SaveChanges();
            }
            finally
            {
                DB.Configuration.AutoDetectChangesEnabled = true;
            }
        }
        public void Calculate_Deduction(GEN_Filter _filter, DateTime FromDate, DateTime ToDate)
        {
            try
            {
                DB.Configuration.AutoDetectChangesEnabled = true;

                var _listGenD = new List<HISGenSalaryD>();
                decimal ActualWorkDay = 0;
                decimal TempAmount = 0;
                PRSRewardType rewardType = new PRSRewardType();
                #region ******** InMonth ********
                var EmpDed = DB.PREmpDeducs.Where(w => w.EmpCode == _filter.EmpCode && w.FromDate.Value.Year == _filter.InYear && w.FromDate.Value.Month == _filter.InMonth &&
                   w.ToDate.Value.Year == _filter.InYear && w.ToDate.Value.Month == _filter.InMonth && w.Status == 1).ToList();
                var Reward_Type = rewardType.GetRewardsType(_filter.LstRewardsType, RewardTypeCode.DED.ToString());

                foreach (var Ded in EmpDed)
                {
                    ActualWorkDay = ToDate.Subtract(FromDate).Days + 1;
                    var _Ded = Reward_Type.FirstOrDefault(w => w.Code == Ded.DedCode);
                    if (_Ded != null)
                    {
                        var _GenDed = new HISGenDeduction()
                        {
                            INYear = _filter.InYear,
                            INMonth = _filter.InMonth,
                            EmpCode = _filter.EmpCode,
                            FromDate = Ded.FromDate,
                            ToDate = Ded.ToDate,
                            WorkDay = 0,
                            RatePerDay = 0,
                            DedCode = Ded.DedCode,
                            DedDesc = _Ded.Description,
                            OthDesc = _Ded.OthDesc,
                            TaxAble = _Ded.Tax,
                            DedAm = Ded.Amount,
                            DedAMPM = Ded.Amount / ActualWorkDay,
                            Remark = Ded.Remark,
                            CreateBy = User.UserName,
                            CreateOn = DateTime.Now
                        };
                        _filter.LstPayHis.Add(new ClsPayHis()
                        {
                            EmpCode = _filter.EmpCode,
                            SGROUP = "H",
                            PayType = "DED",
                            Code = _Ded.Code,
                            Description = _Ded.Description.ToUpper(),
                            Amount = -Convert.ToDecimal(Ded.Amount)
                        });
                        _filter.ListHisEmpDed.Add(_GenDed);
                        DB.HISGenDeductions.Add(_GenDed);
                        DB.SaveChanges();
                    }
                }
                #endregion

                #region ******** Period ********
                var EmpDedP = DB.PREmpDeducs.Where(w => w.EmpCode == _filter.EmpCode && ((FromDate >= w.FromDate && FromDate <= w.ToDate) || (ToDate >= w.FromDate && ToDate <= w.ToDate))
                && w.Status == 0).ToList();
                foreach (var Ded in EmpDedP)
                {
                    ActualWorkDay = Get_WorkingDay_Ded(_filter.Parameter, FromDate, ToDate);
                    DateTime TempGenFromDate = new DateTime();
                    DateTime TempGenToDate = new DateTime();
                    var _Ded = Reward_Type.FirstOrDefault(w => w.Code == Ded.DedCode);
                    if (_Ded != null)
                    {
                        int HasAllowanceDetail = 0;
                        if (FromDate >= Ded.FromDate && ToDate <= Ded.ToDate)
                        {
                            TempGenFromDate = FromDate;
                            TempGenToDate = ToDate;
                            HasAllowanceDetail = 1;
                        }
                        else if (FromDate >= Ded.FromDate && Ded.ToDate <= ToDate)
                        {
                            TempGenFromDate = FromDate;
                            TempGenToDate = Ded.ToDate.Value;
                            HasAllowanceDetail = 1;
                        }
                        else if (Ded.FromDate >= FromDate && Ded.FromDate <= ToDate && Ded.ToDate >= ToDate)
                        {
                            TempGenFromDate = Ded.FromDate.Value;
                            TempGenToDate = ToDate;
                            HasAllowanceDetail = 1;
                        }
                        else if (Ded.FromDate >= FromDate && Ded.ToDate <= ToDate)
                        {
                            TempGenFromDate = Ded.FromDate.Value;
                            TempGenToDate = Ded.ToDate.Value;
                            HasAllowanceDetail = 1;
                        }
                        if (HasAllowanceDetail == 1)
                        {
                            decimal TempWorkDay = Get_WorkingDay(_filter.Parameter, TempGenFromDate, TempGenToDate);
                            if (TempGenFromDate == FromDate && TempGenToDate == ToDate)
                            {
                                TempAmount = Convert.ToDecimal(Ded.Amount);
                            }
                            else
                            {
                                TempAmount = Convert.ToDecimal((Ded.Amount / ActualWorkDay) * TempWorkDay);
                            }
                            var _GenDed = new HISGenDeduction()
                            {
                                INYear = _filter.InYear,
                                INMonth = _filter.InMonth,
                                EmpCode = _filter.EmpCode,
                                FromDate = Ded.FromDate,
                                ToDate = Ded.ToDate,
                                WorkDay = 0,
                                RatePerDay = 0,
                                DedCode = Ded.DedCode,
                                DedDesc = _Ded.Description,
                                OthDesc = _Ded.OthDesc,
                                TaxAble = _Ded.Tax,
                                DedAm = TempAmount,
                                DedAMPM = Ded.Amount,
                                CreateBy = User.UserName,
                                CreateOn = DateTime.Now
                            };
                            _filter.ListHisEmpDed.Add(_GenDed);
                            DB.HISGenDeductions.Add(_GenDed);
                            DB.SaveChanges();
                            _filter.LstPayHis.Add(new ClsPayHis()
                            {
                                EmpCode = _filter.EmpCode,
                                SGROUP = "H",
                                PayType = "DED",
                                Code = _Ded.Code,
                                Description = _Ded.Description.ToUpper(),
                                Amount = -Convert.ToDecimal(TempAmount)
                            });
                        }
                    }
                }
                #endregion

                #region ******** Staff Deposit ********
                var EmpStaffDep = DB.HREmpDeposits.Where(w => w.EmpCode == _filter.EmpCode && ((FromDate >= w.FromDate && FromDate <= w.ToDate) || (ToDate >= w.FromDate && ToDate <= w.ToDate))).ToList();
                var EmpStaffDepItem = DB.HREmpDepositItems.Where(w => w.EmpCode == _filter.EmpCode
                && w.PayMonth.Month == _filter.InMonth && w.PayMonth.Year == _filter.InYear).ToList();
                foreach (var Ded in EmpStaffDep)
                {
                    decimal Deposit = 0;
                    Deposit = EmpStaffDepItem.Where(w => w.DepositNum == Ded.DepositNum).Sum(x => x.Deposit);
                    ActualWorkDay = Get_WorkingDay_Ded(_filter.Parameter, FromDate, ToDate);
                    var _Ded = Reward_Type.FirstOrDefault(w => w.Code == Ded.DeductionType);
                    if (_Ded != null)
                    {
                        var _GenDed = new HISGenDeduction()
                        {
                            INYear = _filter.InYear,
                            INMonth = _filter.InMonth,
                            EmpCode = _filter.EmpCode,
                            FromDate = FromDate,
                            ToDate = ToDate,
                            WorkDay = ActualWorkDay,
                            RatePerDay = 0,
                            DedCode = Ded.DeductionType,
                            DedDesc = _Ded.Description,
                            OthDesc = _Ded.OthDesc,
                            TaxAble = _Ded.Tax,
                            DedAm = Deposit,
                            DedAMPM = Deposit / ActualWorkDay,
                            Remark = "Staff Deposit",
                            CreateBy = User.UserName,
                            CreateOn = DateTime.Now
                        };
                        _filter.ListHisEmpDed.Add(_GenDed);
                        DB.HISGenDeductions.Add(_GenDed);
                        DB.SaveChanges();
                        _filter.LstPayHis.Add(new ClsPayHis()
                        {
                            EmpCode = _filter.EmpCode,
                            SGROUP = "H",
                            PayType = "DED",
                            Code = _Ded.Code,
                            Description = _Ded.Description.ToUpper(),
                            Amount = -Convert.ToDecimal(Deposit)
                        });
                    }
                }
                #endregion
            }
            finally
            {
                DB.Configuration.AutoDetectChangesEnabled = true;
            }
        }
        public void Calculate_Misscan_Deduction(GEN_Filter _filter, DateTime FromDate, DateTime ToDate)
        {
            try
            {
                decimal? MisscanDed = 0.0M;
                var dayrate = 0.0M;
                var STAFF = DB.HRStaffProfiles.Where(w => w.EmpCode == _filter.EmpCode).FirstOrDefault();
                //var param = DB.PRParameters.Where(w => w.Code == STAFF.PayParam).FirstOrDefault();
                var MISS = DB.PREmpLateDeducts.Where(w => w.EmpCode == _filter.EmpCode && w.FromDate >= FromDate && w.ToDate <= ToDate && w.DedCode == "MissScan").ToList();
                decimal WorkDayPerMonth = Get_WorkingDay_Salary(_filter.Parameter, FromDate, ToDate);
                if (STAFF.IsAtten != true)
                {
                    dayrate = STAFF.Salary / WorkDayPerMonth;
                }
                else
                {
                    dayrate = 0;
                }
                string Cutday = string.Empty;
                if (MISS.Sum(w => w.Day) > 0)
                {
                    MisscanDed = MISS.Sum(w => w.Day) * dayrate;
                    Cutday = MISS.Sum(w => w.Day) + " Day";
                    var _GenDed = new HISGenDeduction()
                    {
                        INYear = _filter.InYear,
                        INMonth = _filter.InMonth,
                        EmpCode = _filter.EmpCode,
                        FromDate = _filter.FromDate,
                        ToDate = _filter.ToDate,
                        WorkDay = 0,
                        RatePerDay = dayrate,
                        DedCode = "NOSCAN",
                        DedDesc = "Misscan",
                        TaxAble = true,
                        DedAm = Convert.ToDecimal(MisscanDed),
                        Remark = Cutday,
                        CreateBy = User.UserName,
                        CreateOn = DateTime.Now
                    };
                    _filter.ListHisEmpDed.Add(_GenDed);
                    DB.HISGenDeductions.Add(_GenDed);
                    DB.SaveChanges();
                }

            }
            finally
            {
                DB.Configuration.AutoDetectChangesEnabled = true;
            }

        }
        public void Calculate_Late_Early_Deduction(GEN_Filter _filter, DateTime FromDate, DateTime ToDate)
        {
            try
            {
                DB.Configuration.AutoDetectChangesEnabled = false;
                var EmpCareer = _filter.LstEmpCareer.ToList();
                decimal WorkDay = Get_WorkingDay_Ded(_filter.Parameter, FromDate, ToDate);
                PRSRewardType rewardType = new PRSRewardType();
                #region ******** InMonth ********
                //var EmpDed = DB.PREmpLateDeducts.Where(w => w.EmpCode == _filter.EmpCode && w.FromDate.Value.Year == _filter.InYear && w.FromDate.Value.Month == _filter.InMonth &&
                //   w.ToDate.Value.Year == _filter.InYear && w.ToDate.Value.Month == _filter.InMonth && w.Status == 1).ToList();
                var EmpDed = DB.PREmpLateDeducts.Where(w => w.EmpCode == _filter.EmpCode && w.InMonth.Year == _filter.InYear && w.InMonth.Month == _filter.InMonth && w.Status == 1).ToList();
                var List = EmpDed.GroupBy(w => new { w.EmpCode, w.DedCode, w.FromDate, w.ToDate }).
                    Select(x => new PREmpLateDeduct
                    {
                        EmpCode = x.Key.EmpCode,
                        DedCode = x.Key.DedCode,
                        FromDate = x.Key.FromDate,
                        ToDate = x.Key.ToDate,
                        Minute = x.Sum(s => s.Minute),
                        Day = x.Sum(s => s.Day)
                    }).ToList();
                var Reward_Type = rewardType.GetRewardsType(_filter.LstRewardsType, RewardTypeCode.DED.ToString());

                foreach (var Ded in List)
                {
                    var Emp_C = EmpCareer.FirstOrDefault(w => ToDate >= w.FromDate && ToDate <= w.ToDate);
                    // ActualWorkDay = ToDate.Subtract(FromDate).Days + 1;
                    var _Ded = Reward_Type.FirstOrDefault(w => w.Code == Ded.DedCode);
                    if (_Ded != null && Emp_C != null)
                    {
                        decimal Salary = Emp_C.NewSalary;
                        decimal Rate = (Salary / WorkDay);
                        decimal Total = Convert.ToDecimal(Ded.Minute);
                        decimal Hours = Math.Round(Total / 60.00M, 2);
                        decimal RateH = Math.Round(Convert.ToDecimal(Rate / _filter.Parameter.WHOUR), 2);
                        //  decimal Amount = ((Hours + Convert.ToDecimal(Ded.Day * _filter.Parameter.WHOUR)) * RateH);
                        decimal Amount = Convert.ToDecimal((Ded.Day * Rate)) + (Hours * RateH);
                        var _GenDed = new HISGenDeduction()
                        {
                            INYear = _filter.InYear,
                            INMonth = _filter.InMonth,
                            EmpCode = _filter.EmpCode,
                            FromDate = Ded.FromDate,
                            ToDate = Ded.ToDate,
                            WorkDay = 0,
                            RatePerDay = 0,
                            DedCode = Ded.DedCode,
                            DedDesc = _Ded.Description,
                            OthDesc = _Ded.OthDesc,
                            TaxAble = _Ded.Tax,
                            Remark = Ded.Minute.ToString(),
                            DedAm = Convert.ToDecimal(Amount),
                            DedAMPM = Convert.ToDecimal(Amount / WorkDay),
                            CreateBy = User.UserName,
                            CreateOn = DateTime.Now
                        };
                        if (_filter.LstPayHis.Where(w => w.PayType == "DED" && w.Code == _Ded.Code).Any())
                        {
                            _filter.LstPayHis.Where(w => w.PayType == "DED" && w.Code == _Ded.Code).ToList().ForEach(x => x.Amount -= Convert.ToDecimal(Amount));
                        }
                        else
                        {
                            _filter.LstPayHis.Add(new ClsPayHis()
                            {
                                EmpCode = _filter.EmpCode,
                                SGROUP = "H",
                                PayType = "DED",
                                Code = _Ded.Code,
                                Description = _Ded.Description.ToUpper(),
                                Amount = -Convert.ToDecimal(Amount)
                            });
                        }
                        _filter.ListHisEmpDed.Add(_GenDed);
                        DB.HISGenDeductions.Add(_GenDed);
                        DB.SaveChanges();
                    }
                }
                #endregion
            }
            finally
            {
                DB.Configuration.AutoDetectChangesEnabled = true;
            }
        }
        public void Calculate_Bonus(GEN_Filter _filter, DateTime FromDate, DateTime ToDate)
        {
            try
            {
                DB.Configuration.AutoDetectChangesEnabled = false;
                decimal ActualWorkDay = 0;
                decimal TempAmount = 0;
                PRSRewardType rewardType = new PRSRewardType();
                var AllwType = rewardType.GetRewardsType(_filter.LstRewardsType, RewardTypeCode.BON.ToString());

                #region ********Bon InMonth********
                var EmpBon = _filter.LstEmpBon.Where(w => w.EmpCode == _filter.EmpCode && w.FromDate.Value.Year == _filter.InYear && w.FromDate.Value.Month == _filter.InMonth &&
                   w.ToDate.Value.Year == _filter.InYear && w.ToDate.Value.Month == _filter.InMonth && w.Status == 1).ToList();

                foreach (var item in EmpBon)
                {
                    ActualWorkDay = _filter.ToDate.Subtract(_filter.FromDate).Days + 1;
                    var _allw = AllwType.FirstOrDefault(w => w.Code == item.BonCode);
                    if (_allw != null)
                    {
                        var _GenAllw = new HISGenBonu()
                        {
                            INYear = _filter.InYear,
                            INMonth = _filter.InMonth,
                            EmpCode = _filter.EmpCode,
                            FromDate = item.FromDate,
                            ToDate = item.ToDate,
                            BonusCode = item.BonCode,
                            BonusDesc = _allw.Description,
                            OthDesc = _allw.OthDesc,
                            TaxAble = _allw.Tax,
                            FringTax = _allw.FTax,
                            BonusAM = item.Amount,
                            CreateBy = User.UserName,
                            CreateOn = DateTime.Now
                        };
                        _filter.ListHisEmpBonu.Add(_GenAllw);
                        DB.HISGenBonus.Add(_GenAllw);
                        DB.SaveChanges();
                        _filter.LstPayHis.Add(new ClsPayHis()
                        {
                            EmpCode = _filter.EmpCode,
                            SGROUP = "C",
                            PayType = "BON",
                            Code = _allw.Code,
                            Description = _allw.Description.ToUpper(),
                            Amount = Convert.ToDecimal(item.Amount)
                        });
                    }
                }
                var BaseSalary = _filter.Staff.Salary;
                #endregion

                #region ********Bon Period********
                var EmpBonP = _filter.LstEmpBon.Where(w => w.EmpCode == _filter.EmpCode && ((_filter.FromDate >= w.FromDate && _filter.FromDate <= w.ToDate) || (_filter.ToDate >= w.FromDate && _filter.ToDate <= w.ToDate))
                && w.Status == 0).ToList();
                foreach (var Allw in EmpBonP)
                {
                    ActualWorkDay = Get_WorkingDay_Allw(_filter.Parameter, _filter.FromDate, _filter.ToDate);
                    DateTime TempGenFromDate = new DateTime();
                    DateTime TempGenToDate = new DateTime();
                    var _allw = AllwType.FirstOrDefault(w => w.Code == Allw.BonCode);
                    if (_allw != null)
                    {
                        int HasAllowanceDetail = 0;
                        if (_filter.FromDate >= Allw.FromDate && _filter.ToDate <= Allw.ToDate)
                        {
                            TempGenFromDate = _filter.FromDate;
                            TempGenToDate = _filter.ToDate;
                            HasAllowanceDetail = 1;
                        }
                        else if (_filter.FromDate >= Allw.FromDate && Allw.ToDate <= _filter.ToDate)
                        {
                            TempGenFromDate = _filter.FromDate;
                            TempGenToDate = Allw.ToDate.Value;
                            HasAllowanceDetail = 1;
                        }
                        else if (Allw.FromDate >= _filter.FromDate && Allw.FromDate <= _filter.ToDate && Allw.ToDate >= _filter.ToDate)
                        {
                            TempGenFromDate = Allw.FromDate.Value;
                            TempGenToDate = _filter.ToDate;
                            HasAllowanceDetail = 1;
                        }
                        else if (Allw.FromDate >= _filter.FromDate && Allw.ToDate <= _filter.ToDate)
                        {
                            TempGenFromDate = Allw.FromDate.Value;
                            TempGenToDate = Allw.ToDate.Value;
                            HasAllowanceDetail = 1;
                        }
                        if (HasAllowanceDetail == 1)
                        {
                            decimal TempWorkDay = Get_WorkingDay(_filter.Parameter, TempGenFromDate, TempGenToDate);
                            if (TempGenFromDate == _filter.FromDate && TempGenToDate == _filter.ToDate)
                            {
                                TempAmount = Convert.ToDecimal(Allw.Amount);
                            }
                            else
                            {
                                TempAmount = Convert.ToDecimal((Allw.Amount / ActualWorkDay) * TempWorkDay);
                            }
                            var _GenAllw = new HISGenBonu()
                            {
                                INYear = _filter.InYear,
                                INMonth = _filter.InMonth,
                                EmpCode = _filter.EmpCode,
                                FromDate = Allw.FromDate,
                                ToDate = Allw.ToDate,
                                BonusCode = Allw.BonCode,
                                BonusDesc = _allw.Description,
                                OthDesc = _allw.OthDesc,
                                TaxAble = _allw.Tax,
                                FringTax = _allw.FTax,
                                BonusAM = TempAmount,
                                CreateBy = User.UserName,
                                CreateOn = DateTime.Now
                            };
                            _filter.ListHisEmpBonu.Add(_GenAllw);
                            DB.HISGenBonus.Add(_GenAllw);
                            DB.SaveChanges();
                            _filter.LstPayHis.Add(new ClsPayHis()
                            {
                                EmpCode = _filter.EmpCode,
                                SGROUP = "C",
                                PayType = "BON",
                                Code = _allw.Code,
                                Description = _allw.Description.ToUpper(),
                                Amount = Convert.ToDecimal(TempAmount)
                            });
                        }
                    }
                }
                #endregion
            }
            finally
            {
                DB.Configuration.AutoDetectChangesEnabled = true;
            }
        }
        public void Calculate_LeaveDeduct(GEN_Filter _filter, DateTime FromDate, DateTime ToDate)
        {
            try
            {
                DB.Configuration.AutoDetectChangesEnabled = false;
                var LeaveType = DB.HRLeaveTypes.ToList();

                decimal DayRate = 0;
                string Approve = SYDocumentStatus.APPROVED.ToString();
                decimal WorkDay = Get_WorkingDay(_filter.Parameter, FromDate, ToDate);
                decimal NoDayInMonth = 0;

                var LeaveH = DB.HREmpLeaves.Where(w => w.EmpCode == _filter.EmpCode && w.Status == Approve).ToList();
                LeaveH = LeaveH.Where(C => ((C.FromDate >= FromDate && C.FromDate <= ToDate) || (C.ToDate >= FromDate && C.ToDate <= ToDate) ||
                      (FromDate >= C.FromDate && FromDate <= C.ToDate) || (ToDate >= C.FromDate && ToDate <= C.ToDate))).ToList();

                var EmpLeave = DB.HREmpLeaveDs.Where(w => w.EmpCode == _filter.EmpCode && w.CutMonth.Value >= FromDate && w.CutMonth.Value <= ToDate && w.Status == "Leave").ToList();
                EmpLeave = EmpLeave.Where(w => LeaveH.Where(x => x.EmpCode == w.EmpCode).Any()).ToList();
                EmpLeave = EmpLeave.Where(w => LeaveH.Where(x => x.EmpCode == w.EmpCode && x.Increment == w.LeaveTranNo).Any()).ToList();
                EmpLeave = EmpLeave.Where(w => LeaveType.Where(x => x.Code == w.LeaveCode && x.CUT == true).Any()).ToList();
                if (_filter.Staff.Status == "I" && _filter.Staff.DateTerminate.Year == _filter.InYear && _filter.Staff.DateTerminate.Month == _filter.InMonth)
                {
                    EmpLeave = EmpLeave.Where(w => w.LeaveDate < _filter.Staff.DateTerminate).ToList();
                }
                foreach (var Leave in EmpLeave)
                {
                    DayRate = 0;
                    var Emp_C = _filter.LstEmpCareer.FirstOrDefault(C => Leave.LeaveDate >= C.FromDate && Leave.LeaveDate <= C.ToDate);
                    var _Type = LeaveType.FirstOrDefault(w => w.Code == Leave.LeaveCode);
                    decimal Salary = Convert.ToDecimal(Emp_C.NewSalary);
                    NoDayInMonth = Get_WorkingDay_Salary(_filter.Parameter, FromDate, ToDate);
                    if (_Type.Foperand == "B")
                    {
                        if (_Type.Operator == "/")
                            DayRate = Convert.ToDecimal(Salary / _Type.Soperand);
                        else if (_Type.Operator == "-")
                            DayRate = Convert.ToDecimal(Salary - _Type.Soperand);
                        else if (_Type.Operator == "+")
                            DayRate = Convert.ToDecimal(Salary + _Type.Soperand);
                        else if (_Type.Operator == "*")
                            DayRate = Convert.ToDecimal(Salary * _Type.Soperand);
                    }
                    else if (_Type.Foperand == "B/W")
                    {
                        if (_Type.Operator == "/")
                            DayRate = Convert.ToDecimal((Salary / NoDayInMonth) / _Type.Soperand);
                        else if (_Type.Operator == "-")
                            DayRate = Convert.ToDecimal((Salary / NoDayInMonth) - _Type.Soperand);
                        else if (_Type.Operator == "+")
                            DayRate = Convert.ToDecimal((Salary / NoDayInMonth) + _Type.Soperand);
                        else if (_Type.Operator == "*")
                            DayRate = Convert.ToDecimal((Salary / NoDayInMonth) * _Type.Soperand);
                    }
                    else if (_Type.Foperand == "B/D*H")
                    {
                        if (_Type.Operator == "/")
                            DayRate = Convert.ToDecimal((Salary / (NoDayInMonth * _filter.Parameter.WHOUR)) / _Type.Soperand);
                        else if (_Type.Operator == "-")
                            DayRate = Convert.ToDecimal((Salary / (NoDayInMonth - _filter.Parameter.WHOUR)) / _Type.Soperand);
                        else if (_Type.Operator == "+")
                            DayRate = Convert.ToDecimal((Salary / (NoDayInMonth + _filter.Parameter.WHOUR)) / _Type.Soperand);
                        else if (_Type.Operator == "*")
                            DayRate = Convert.ToDecimal((Salary / (NoDayInMonth + _filter.Parameter.WHOUR)) / _Type.Soperand);
                    }
                    decimal LHour = Convert.ToDecimal(Leave.LHour);
                    string Measure = "H";
                    if (_Type.CUTTYPE == 1) Measure = "D";
                    if (Measure == "D") LHour = Convert.ToDecimal(Leave.LHour / _filter.Parameter.WHOUR);
                    decimal? Amount = LHour * DayRate;// Math.Round(DayRate, 2);
                    var _Gen = new HISGenLeaveDeduct()
                    {
                        INYear = _filter.InYear,
                        INMonth = _filter.InMonth,
                        EmpCode = _filter.EmpCode,
                        LeaveCode = Leave.LeaveCode,
                        LeaveDesc = _Type.Description,
                        LeaveOthDesc = _Type.OthDesc,
                        LeaveDate = Leave.LeaveDate,
                        ForMular = "(" + _Type.Foperand + ")" + _Type.Operator + _Type.Soperand,
                        BaseSalary = Salary,
                        WorkDay = NoDayInMonth,
                        WorkHour = _filter.Parameter.WHOUR,
                        Measure = Measure,
                        Qty = LHour,
                        Rate = Math.Round(DayRate, 2),
                        Amount = Amount,
                        CreateBy = User.UserName,
                        CreateOn = DateTime.Now
                    };
                    _filter.ListHisEmpLeave.Add(_Gen);
                    DB.HISGenLeaveDeducts.Add(_Gen);
                    DB.SaveChanges();
                    if (_filter.LstPayHis.Where(w => w.PayType == "LV" && w.Code == _Gen.LeaveCode).Any())
                    {
                        _filter.LstPayHis.Where(w => w.PayType == "LV" && w.Code == _Gen.LeaveCode).ToList().ForEach(x => x.Amount -= Convert.ToDecimal(_Gen.Amount));
                    }
                    else
                    {
                        _filter.LstPayHis.Add(new ClsPayHis()
                        {
                            EmpCode = _filter.EmpCode,
                            SGROUP = "I",
                            PayType = "LV",
                            Code = _Gen.LeaveCode,
                            Description = _Gen.LeaveDesc.ToUpper(),
                            Amount = -Convert.ToDecimal(_Gen.Amount)
                        });
                    }
                }
            }
            finally
            {
                DB.Configuration.AutoDetectChangesEnabled = true;
            }
        }
        public void Calculate_TAX(GEN_Filter _filter, DateTime FromDate, DateTime ToDate, bool TaxType, HRBranch _Branch, bool IsTax)
        {
            try
            {
                DB.Configuration.AutoDetectChangesEnabled = false;
                string CareerCode = _filter.Staff.CareerDesc;
                decimal AmountOT = 0;
                var EmpOT = _filter.ListHisEmpOT;
                foreach (var emp in EmpOT)
                {
                    //AmountOT = Math.Round(AmountOT + (Convert.ToDecimal(emp.OTHour) * Convert.ToDecimal(emp.OTRate)), SYConstant.DECIMAL_PLACE);
                    AmountOT += Convert.ToDecimal(emp.Amount);
                }
                AmountOT = ClsRounding.Rounding(AmountOT, SYConstant.DECIMAL_PLACE, "N");
                //---------Allowance-----
                decimal TaxAmountAllw = 0, UTaxAmountAllw = 0, AmFring = 0;
                var EmpAllw = _filter.ListHisEmpAllw;
                TaxAmountAllw = Convert.ToDecimal(EmpAllw.Where(w => w.TaxAble == true).Sum(x => x.AllwAm));
                UTaxAmountAllw = Convert.ToDecimal(EmpAllw.Where(w => w.TaxAble == false && w.FringTax != true).Sum(x => x.AllwAm));
                AmFring = Convert.ToDecimal(EmpAllw.Where(w => w.FringTax == true).Sum(x => x.AllwAm));
                //--------Deduct----------
                decimal TaxDeduct = 0, UTaxDeduct = 0;
                var EmpDed = _filter.ListHisEmpDed;
                TaxDeduct = Convert.ToDecimal(EmpDed.Where(w => w.TaxAble == true).Sum(x => x.DedAm));
                UTaxDeduct = Convert.ToDecimal(EmpDed.Where(w => w.TaxAble == false).Sum(x => x.DedAm));
                //--------Bonus-----------
                decimal TaxBonus = 0, UTaxBonus = 0;
                var EmpBonus = _filter.ListHisEmpBonu;
                TaxBonus = Convert.ToDecimal(EmpBonus.Where(w => w.TaxAble == true).Sum(x => x.BonusAM));
                UTaxBonus = Convert.ToDecimal(EmpBonus.Where(w => w.TaxAble == false).Sum(x => x.BonusAM));
                AmFring = AmFring + Convert.ToDecimal(EmpBonus.Where(w => w.FringTax == true).Sum(x => x.BonusAM));
                //-------Leave Deduct-----------------
                decimal LeaveDedAm = 0;
                var EmpLeaveDed = _filter.ListHisEmpLeave;
                LeaveDedAm = ClsRounding.Rounding(Convert.ToDecimal(EmpLeaveDed.Sum(x => x.Amount)), SYConstant.DECIMAL_PLACE, "N");
                //------------------------------------
                var ObjSalary = _filter.HisGensalarys;
                ObjSalary.OTAM = AmountOT;
                ObjSalary.TaxALWAM = ClsRounding.Rounding(TaxAmountAllw, SYConstant.DECIMAL_PLACE, "N");
                ObjSalary.UTAXALWAM = ClsRounding.Rounding(UTaxAmountAllw - ObjSalary.SeniorityTaxable.Value, SYConstant.DECIMAL_PLACE, "N");
                ObjSalary.TAXDEDAM = ClsRounding.Rounding(TaxDeduct, SYConstant.DECIMAL_PLACE, "N");
                ObjSalary.UTAXDEDAM = ClsRounding.Rounding(UTaxDeduct, SYConstant.DECIMAL_PLACE, "N");
                ObjSalary.TAXBONAM = ClsRounding.Rounding(TaxBonus, SYConstant.DECIMAL_PLACE, "N");
                ObjSalary.UTAXBONAM = ClsRounding.Rounding(UTaxBonus, SYConstant.DECIMAL_PLACE, "N");
                ObjSalary.LeaveDeduct = ClsRounding.Rounding(LeaveDedAm, SYConstant.DECIMAL_PLACE, "N");
                ObjSalary.AMFRINGTAX = 0;
                ObjSalary.FRINGAM = 0;
                ObjSalary.FRINGRATE = 0;
                if (_filter.Setting != null)
                    ObjSalary.Seniority = _filter.ListHisEmpAllw.Where(w => w.AllwCode == _filter.Setting.SeniorityType).Sum(w => w.AllwAm);
                var AMToBeTax = (ObjSalary.Salary + ObjSalary.OTAM + ObjSalary.NWAM + ObjSalary.PayBack + ObjSalary.ShiftPay + ObjSalary.TaxALWAM -
                    ObjSalary.TAXDEDAM + ObjSalary.TAXBONAM) - ObjSalary.LeaveDeduct;
                var GrossPay = ObjSalary.Salary + ObjSalary.OTAM + ObjSalary.NWAM + ObjSalary.PayBack + ObjSalary.ShiftPay + (ObjSalary.TaxALWAM + ObjSalary.UTAXALWAM) +
                     (ObjSalary.TAXBONAM + ObjSalary.UTAXBONAM) - LeaveDedAm;
                var AlwBeforTax = (ObjSalary.OTAM + ObjSalary.NWAM + ObjSalary.PayBack + ObjSalary.ShiftPay + ObjSalary.TaxALWAM + ObjSalary.SeniorityTaxable.ToDecimal() -
                    ObjSalary.TAXDEDAM + ObjSalary.TAXBONAM) - ObjSalary.LeaveDeduct;
                ObjSalary.AlwBeforTax = AlwBeforTax;
                var SpouseAmount = ObjSalary.UTAXSP;
                var ChildAmount = ObjSalary.UTAXCH;
                decimal? SalaryNSSF = 0;

                if (_filter.NSSFSalaryType == SalaryType.GP.ToString())
                {
                    SalaryNSSF = GrossPay;
                }
                else SalaryNSSF = ObjSalary.Salary;
                CalculatePensionFund(_filter, SalaryNSSF);

                var UnTaxAmount = ObjSalary.UTAXALWAM + ObjSalary.UTAXDEDAM + ObjSalary.UTAXBONAM;
                decimal ExchangeRate = Convert.ToDecimal(ObjSalary.ExchRate);
                var TxPayType = ObjSalary.TXPayType;
                GrossPay = ClsRounding.Rounding(Convert.ToDecimal(GrossPay), SYConstant.DECIMAL_PLACE, "N");
                if (ObjSalary.StaffPensionFundAmount.HasValue)
                {
                    ObjSalary.StaffPensionFundAmount = ClsRounding.Rounding(ObjSalary.StaffPensionFundAmount.Value, SYConstant.DECIMAL_PLACE, "N");
                    ObjSalary.CompanyPensionFundAmount = ClsRounding.Rounding(ObjSalary.CompanyPensionFundAmount.Value, SYConstant.DECIMAL_PLACE, "N");
                }
                ObjSalary.TotalTaxableIncome = AMToBeTax;
                decimal RealAmountTobeTax = Convert.ToDecimal(AMToBeTax) * ExchangeRate - (SpouseAmount.Value + ChildAmount.Value);
                AMToBeTax = RealAmountTobeTax / ExchangeRate;
                AMToBeTax = AMToBeTax.Value();
                if (_filter.NSSFSalaryType == SalaryType.BS.ToString())
                    RealAmountTobeTax = RealAmountTobeTax - ObjSalary.StaffPensionFundAmountKH.Value;
                if (RealAmountTobeTax < 0)
                {
                    RealAmountTobeTax = 0;
                    AMToBeTax = 0;
                }
                if (TaxType == true)
                {
                    decimal? TaxKH = 0;
                    decimal? TaxUSD = 0;
                    decimal? TaxRate = 0;
                    var ListTax = _filter.ListTaxSetting; //DB.PRTaxSettings.ToList();
                    if (_filter.Staff.IsResident == false)
                    {
                        TaxRate = 20;
                        TaxUSD = ((RealAmountTobeTax * TaxRate) / 100) / ExchangeRate;
                    }
                    else
                    {
                        TaxKH = CambodiaTaxDeduction(RealAmountTobeTax, ListTax, ref TaxRate, false);
                        TaxUSD = TaxKH / ExchangeRate;
                        TaxUSD = TaxUSD.Value();
                    }

                    ObjSalary.TAXTYPE = 0;
                    ObjSalary.GrossNoTIP = GrossPay;
                    ObjSalary.GrossPay = GrossPay;
                    ObjSalary.TAXAM = Math.Round(Convert.ToDecimal(TaxUSD), SYConstant.DECIMAL_PLACE);
                    ObjSalary.TAXRATE = TaxRate;
                    ObjSalary.AMTOBETAX = AMToBeTax = ClsRounding.Rounding(Convert.ToDecimal(AMToBeTax), SYConstant.DECIMAL_PLACE, "N");
                    ObjSalary.AMUNTAX = UnTaxAmount;
                    ObjSalary.AmtoBeTaxKH = RealAmountTobeTax;
                    if (SYConstant.DECIMAL_PLACE == 0) ObjSalary.TaxKH = Convert.ToDecimal(ObjSalary.TAXAM * ExchangeRate);
                    else
                        ObjSalary.TaxKH = Convert.ToDecimal(TaxKH);
                }
                decimal AmFringRate = 0.2M;
                var AdvPay = DB.PRADVPays.Where(w => w.EmpCode == _filter.EmpCode && w.TranDate.Value.Year == _filter.InYear && w.TranDate.Value.Month == _filter.InMonth).ToList();
                var AdvpayAm = AdvPay.Sum(w => w.Amount);
                AmFring = Math.Round(AmFring, SYConstant.DECIMAL_PLACE);
                if (TxPayType == "N")
                {
                    ObjSalary.TAXAM = 0;//Math.Round(Convert.ToDecimal(TaxUSD), SYConstant.DECIMAL_PLACE);
                    ObjSalary.TAXRATE = 0;
                    ObjSalary.TaxKH = 0;
                    ObjSalary.NetNoTIP = (ObjSalary.GrossNoTIP - AdvpayAm - ObjSalary.LOAN - ObjSalary.TAXDEDAM - ObjSalary.UTAXDEDAM)+ ObjSalary.UTAXALWAM +ObjSalary.UTAXBONAM;
                    ObjSalary.NetWage = (ObjSalary.GrossPay - AdvpayAm - ObjSalary.LOAN - ObjSalary.TAXDEDAM - ObjSalary.UTAXDEDAM) + ObjSalary.UTAXALWAM + ObjSalary.UTAXBONAM;
                    ObjSalary.AMFRINGTAX = AmFring;
                    ObjSalary.FRINGRATE = 20;
                    ObjSalary.FRINGAM = Math.Round(AmFring * AmFringRate, SYConstant.DECIMAL_PLACE);
                }
                if (TxPayType == "C")
                {
                    ObjSalary.NetNoTIP = (ObjSalary.GrossNoTIP - AdvpayAm - ObjSalary.LOAN - ObjSalary.TAXDEDAM - ObjSalary.UTAXDEDAM);
                    ObjSalary.NetWage = (ObjSalary.GrossPay - AdvpayAm - ObjSalary.LOAN - ObjSalary.TAXDEDAM - ObjSalary.UTAXDEDAM);
                    ObjSalary.AMFRINGTAX = AmFring;
                    ObjSalary.FRINGRATE = 20;
                    ObjSalary.FRINGAM = Math.Round(AmFring * AmFringRate, SYConstant.DECIMAL_PLACE);
                }
                else if (TxPayType == "S")
                {
                    ObjSalary.NetNoTIP = (ObjSalary.GrossNoTIP - AdvpayAm - ObjSalary.LOAN - ObjSalary.TAXDEDAM - ObjSalary.UTAXDEDAM - ObjSalary.TAXAM);
                    ObjSalary.NetWage = (ObjSalary.GrossPay - AdvpayAm - ObjSalary.LOAN - ObjSalary.TAXDEDAM - ObjSalary.UTAXDEDAM - ObjSalary.TAXAM);
                    ObjSalary.AMFRINGTAX = AmFring;
                    ObjSalary.FRINGRATE = 20;
                    ObjSalary.FRINGAM = Math.Round(AmFring * AmFringRate, SYConstant.DECIMAL_PLACE);
                    ObjSalary.NetWage += (AmFring - ObjSalary.FRINGAM);
                }
                if (ObjSalary.StaffPensionFundAmount.HasValue)
                {
                    if (_filter.NSSFSalaryType == SalaryType.BS.ToString())
                    {
                        ObjSalary.GrossNoTIP = GrossPay - ObjSalary.StaffPensionFundAmount;
                        ObjSalary.GrossPay = GrossPay - ObjSalary.StaffPensionFundAmount;
                        ObjSalary.AMTOBETAX = AMToBeTax - ObjSalary.StaffPensionFundAmount;
                    }

                    ObjSalary.NetNoTIP -= ObjSalary.StaffPensionFundAmount;
                    ObjSalary.NetWage -= ObjSalary.StaffPensionFundAmount;
                }
                if (_filter.Staff.ISNSSF == true)
                {
                    ObjSalary.NetNoTIP -= (ObjSalary.StaffRisk + ObjSalary.StaffHealthCareUSD);
                    ObjSalary.NetWage -= (ObjSalary.StaffRisk + ObjSalary.StaffHealthCareUSD);
                }
                ObjSalary.CareerCode = CareerCode;
                if (_filter.LstBankFee.Where(w => w.BrankCode == ObjSalary.BankName).Count() > 0)
                {
                    var _bankFee = _filter.LstBankFee.Where(w => w.BrankCode == ObjSalary.BankName).ToList();
                    var ListBabkFee = _bankFee.Where(w => w.FeeFrom <= ObjSalary.NetWage && w.FeeTo >= ObjSalary.NetWage).ToList();
                    foreach (var item in ListBabkFee)
                    {
                        if (item.Type == "Amount")
                        {
                            ObjSalary.BankFee = item.Rate;
                        }
                    }
                }
                if (ObjSalary.FirstPaymentAmount > 0)
                {
                    ObjSalary.NetNoTIP -= ObjSalary.FirstPaymentAmount;
                    ObjSalary.NetWage -= ObjSalary.FirstPaymentAmount;
                }
                var NetPay = Math.Round(Convert.ToDecimal(ObjSalary.NetWage), SYConstant.DECIMAL_PLACE);

                if (_filter.Default_Curremcy == "KH" && _filter.Round_UP == "YES")
                {
                    if (NetPay > 0)
                        NetPay = Rounding(NetPay);
                }
                ObjSalary.NetWage = NetPay;

                DB.HISGenSalaries.Add(ObjSalary);
                DB.SaveChanges();

                _filter.LstPayHis.Add(new ClsPayHis()
                {
                    EmpCode = _filter.EmpCode,
                    SGROUP = "G",
                    PayType = "GR",
                    Code = "GR",
                    Description = "GROSS PAY",
                    Amount = Convert.ToDecimal(ObjSalary.GrossPay)
                });
                _filter.LstPayHis.Add(new ClsPayHis()
                {
                    EmpCode = _filter.EmpCode,
                    SGROUP = "L",
                    PayType = "IT",
                    Code = "IT",
                    Description = "INCOME TAX",
                    Amount = -Convert.ToDecimal(ObjSalary.TAXAM)
                });
                if (ObjSalary.ADVPay > 0)
                {
                    _filter.LstPayHis.Add(new ClsPayHis()
                    {
                        EmpCode = _filter.EmpCode,
                        SGROUP = "M",
                        PayType = "AV",
                        Code = "AV",
                        Description = "ADVANCE PAY",
                        Amount = -Convert.ToDecimal(ObjSalary.ADVPay)
                    });
                }
                if (ObjSalary.StaffPensionFundAmount > 0)
                {
                    _filter.LstPayHis.Add(new ClsPayHis()
                    {
                        EmpCode = _filter.EmpCode,
                        SGROUP = "E",
                        PayType = "ST_PEN",
                        Code = "ST_PEN",
                        Description = "EMPLOYEE PENSION FUND CONTRIBUTION",
                        Amount = -Convert.ToDecimal(ObjSalary.StaffPensionFundAmount)
                    });
                }
                if (ObjSalary.FirstPaymentAmount > 0)
                {
                    _filter.LstPayHis.Add(new ClsPayHis()
                    {
                        EmpCode = _filter.EmpCode,
                        SGROUP = "M",
                        PayType = "FIRST",
                        Code = "FIRST",
                        Description = "FIRST PAYMENT",
                        Amount = -Convert.ToDecimal(ObjSalary.FirstPaymentAmount)
                    });
                }
                _filter.LstPayHis.Add(new ClsPayHis()
                {
                    EmpCode = _filter.EmpCode,
                    SGROUP = "N",
                    PayType = "NET",
                    Code = "NET",
                    Description = "NETT WAGE",
                    Amount = Convert.ToDecimal(ObjSalary.NetWage)
                });
            }
            finally
            {
                DB.Configuration.AutoDetectChangesEnabled = true;
            }
        }
        public int Rounding(decimal? Salary)
        {
            int _result = 0;
            if (Salary == 0) return 0;
            int _netPay = Convert.ToInt32(Salary);
            if (_netPay.ToString().Length < 2) return _result;
            int Values = Convert.ToInt32(_netPay.ToString().Substring(_netPay.ToString().Length - 2, 2));
            int _rounding = 100;
            if (Values >= 50)
            {
                int result = _rounding - Values;
                _result = _netPay + result;
            }
            else
            {
                _result = _netPay - Values;
            }
            return _result;
        }
        public void Commit_PaySlip(GEN_Filter _filter, HRBranch _Branch)
        {
            try
            {
                DB.Configuration.AutoDetectChangesEnabled = false;
                string SalaryDesc = "";

                ListPaySlip = new List<HISPaySlip>();
                var Gen_salry = _filter.HisGensalarys;
                var _listLeave = _filter.ListHisEmpLeave;
                var _listMaternity = _listLeave.Where(x => x.LeaveCode == "ML").ToList();
                var Allowance_G = _filter.ListHisEmpAllw;
                var OverTime_G = _filter.ListHisEmpOT;
                var Bonus_G = _filter.ListHisEmpBonu;
                var Deduct_G = _filter.ListHisEmpDed;
                var LeaveDed_G = _listLeave.Where(x => x.LeaveCode != "ML").ToList();
                var OTType = DB.PROTRates.ToList();
                int TranLine = 1;
                for (int i = 1; i <= 50; i++)
                {
                    var Gen = new HISPaySlip()
                    {
                        TranLine = i,
                        EmpCode = _filter.EmpCode,
                        INYear = _filter.InYear,
                        INMonth = _filter.InMonth,
                        EarnDesc = "EARNING",
                        EAmount = 0,
                        DeductDesc = "DEDUCTIONS",
                        DeductAmount = 0
                    };
                    ListPaySlip.Add(Gen);
                }
                decimal Maternity = 0;
                if (_listMaternity.Count > 0) Maternity = _listMaternity.Sum(x => x.Amount).Value;
                if (Maternity > 0) SalaryDesc = "Basic Pay - Maternity Leave";
                else SalaryDesc = "Basic Pay";
                var Salary = Gen_salry.Salary - ClsRounding.Rounding(Maternity, SYConstant.DECIMAL_PLACE, _filter.Round_UP);
                TranLine = Get_LineTranNo(_filter.EmpCode, _filter.InYear, _filter.InMonth, 1);
                ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.EAmount = Salary);
                ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.EarnDesc = SalaryDesc);

                //-------Allowance----------
                foreach (var item in Allowance_G)
                {
                    TranLine = Get_LineTranNo(_filter.EmpCode, _filter.InYear, _filter.InMonth, 1);
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.EAmount = Math.Round(Convert.ToDecimal(item.AllwAm), SYConstant.DECIMAL_PLACE));
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.EarnDesc = item.AllwDesc);
                }
                //---------OverTime-------
                var G_Over = from F in OverTime_G
                             group F by new { F.OTCode, F.OTDesc, F.OTRate } into myGroup
                             select new
                             {
                                 myGroup.Key.OTCode,
                                 OTHour = myGroup.Sum(w => w.OTHour),
                                 myGroup.Key.OTRate,
                                 Amount = myGroup.Sum(w => w.Amount)
                             };
                foreach (var item in G_Over)
                {
                    string Desc = "DAYS";
                    string StrDes = "";
                    var Type = OTType.FirstOrDefault(w => w.OTCode == item.OTCode);
                    if (Type != null)
                    {
                        if (Type.Measure == "H") Desc = "HOURS";
                        StrDes = Type.OTType + "(" + item.OTHour + " " + Desc + ") *" + Math.Round(item.OTRate.Value, 2);
                    }
                    else
                    {
                        Desc = "HOURS";
                        StrDes = "OT Claim";
                    }
                    TranLine = Get_LineTranNo(_filter.EmpCode, _filter.InYear, _filter.InMonth, 1);
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.EAmount = Math.Round(Convert.ToDecimal(item.Amount), SYConstant.DECIMAL_PLACE));
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.EarnDesc = StrDes);
                }
                //---------Bonus------------
                foreach (var item in Bonus_G)
                {
                    TranLine = Get_LineTranNo(_filter.EmpCode, _filter.InYear, _filter.InMonth, 1);
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.EAmount = Math.Round(item.BonusAM, SYConstant.DECIMAL_PLACE));
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.EarnDesc = item.BonusDesc);
                }
                //---------PayBack------------
                var Payback = Gen_salry.PayBack;
                if (Payback > 0)
                {
                    TranLine = Get_LineTranNo(_filter.EmpCode, _filter.InYear, _filter.InMonth, 1);
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.EAmount = Math.Round(Convert.ToDecimal(Payback), SYConstant.DECIMAL_PLACE));
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.EarnDesc = "Pay Back");
                }
                //---------Tax------------
                var Tax_Value = Gen_salry.TAXAM.Value;
                //if (_filter.Staff.TXPayType == "S")
                //{
                TranLine = Get_LineTranNo(_filter.EmpCode, _filter.InYear, _filter.InMonth, 2);
                ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.DeductAmount = Tax_Value);
                ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.DeductDesc = "Income Tax");
                // }
                //Fringe Benefit Tax
                var FrinAm = Gen_salry.FRINGAM;
                if (FrinAm > 0)
                {
                    TranLine = Get_LineTranNo(_filter.EmpCode, _filter.InYear, _filter.InMonth, 2);
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.DeductAmount = FrinAm);
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.DeductDesc = "Fringe Benefit Tax");
                }
                //---------Deduction------------
                foreach (var item in Deduct_G)
                {
                    TranLine = Get_LineTranNo(_filter.EmpCode, _filter.InYear, _filter.InMonth, 2);
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.DeductAmount = Math.Round(Convert.ToDecimal(item.DedAm), SYConstant.DECIMAL_PLACE));
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.DeductDesc = item.DedDesc);
                }
                //---------Leave Deduction------------
                var G_Leave = from F in LeaveDed_G
                              group F by new { F.LeaveDesc } into myGroup
                              select new
                              {
                                  myGroup.Key.LeaveDesc,
                                  Amount = myGroup.Sum(w => w.Amount),
                              };
                foreach (var item in G_Leave)
                {
                    var Amount = Math.Round(Convert.ToDecimal(item.Amount), SYConstant.DECIMAL_PLACE);
                    TranLine = Get_LineTranNo(_filter.EmpCode, _filter.InYear, _filter.InMonth, 2);
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.DeductAmount = Amount);
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.DeductDesc = item.LeaveDesc);
                }
                //---------Loan------------
                var Loan_value = Gen_salry.LOAN;
                if (Loan_value > 0)
                {
                    Loan_value = Math.Round(Convert.ToDecimal(Loan_value), SYConstant.DECIMAL_PLACE);
                    TranLine = Get_LineTranNo(_filter.EmpCode, _filter.InYear, _filter.InMonth, 2);
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.DeductAmount = Loan_value);
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.DeductDesc = "Loan");
                }
                //---------Advance------------
                var ADVPay_value = Gen_salry.ADVPay;
                if (ADVPay_value > 0)
                {
                    TranLine = Get_LineTranNo(_filter.EmpCode, _filter.InYear, _filter.InMonth, 2);
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.DeductAmount = ADVPay_value);
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.DeductDesc = "Advance Pay");
                }
                //add it below deduction
                if (_filter.HisGensalarys.StaffPensionFundAmount.HasValue)
                {
                    TranLine = Get_LineTranNo(_filter.EmpCode, _filter.InYear, _filter.InMonth, 2);
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.DeductAmount = _filter.HisGensalarys.StaffPensionFundAmount);
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.DeductDesc = "Employee Pension Fund Contribution");
                }
                //add it below deduction
                if (_filter.HisGensalarys.StaffRisk.Value > 0)
                {
                    TranLine = Get_LineTranNo(_filter.EmpCode, _filter.InYear, _filter.InMonth, 2);
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.DeductAmount = _filter.HisGensalarys.StaffRisk);
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.DeductDesc = "Employee Risk");
                }
                //add it below deduction
                if (_filter.HisGensalarys.StaffHealthCareUSD.Value > 0)
                {
                    TranLine = Get_LineTranNo(_filter.EmpCode, _filter.InYear, _filter.InMonth, 2);
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.DeductAmount = _filter.HisGensalarys.StaffHealthCareUSD);
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.DeductDesc = "Employee Health Care");
                }
                if (_filter.HisGensalarys.FirstPaymentAmount > 0)
                {
                    TranLine = Get_LineTranNo(_filter.EmpCode, _filter.InYear, _filter.InMonth, 2);
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.DeductAmount = _filter.HisGensalarys.FirstPaymentAmount);
                    ListPaySlip.Where(w => w.EmpCode == _filter.EmpCode && w.TranLine == TranLine).ToList().ForEach(w => w.DeductDesc = "First Payment");
                }
                foreach (var item in ListPaySlip.Where(w => w.EAmount > 0 || w.DeductAmount > 0))
                {
                    DB.HISPaySlips.Add(item);
                }
                var row1 = DB.SaveChanges();
            }
            finally
            {
                DB.Configuration.AutoDetectChangesEnabled = true;
            }
        }
        public void Commit_PayHis(GEN_Filter _filter, List<HisPayHi> ListPayH)
        {
            var DBD = new HumicaDBContext();
            try
            {
                DBD.Configuration.AutoDetectChangesEnabled = false;
                DB.Configuration.AutoDetectChangesEnabled = false;
                var _lstPayH = ListPayH.Where(w => w.EmpCode == _filter.EmpCode).ToList();
                var NoResult = _lstPayH.Where(w => !_filter.LstPayHis.Where(x => x.Code == w.Code && w.PayType == x.PayType).Any()).ToList();
                decimal Amount = 0;
                var PayHis = (from PH in _filter.LstPayHis
                              group PH by new { PH.SGROUP, PH.Code, PH.PayType }
                             into g
                              select new
                              {
                                  SGROUP = g.Key.SGROUP,
                                  Code = g.Key.Code,
                                  PayType = g.Key.PayType,
                              }).ToList().OrderBy(w => w.SGROUP);
                foreach (var read in PayHis)
                {
                    var ListHis = _filter.LstPayHis.Where(w => w.Code == read.Code && w.SGROUP == read.SGROUP && w.PayType == read.PayType).ToList();//.OrderBy(w => w.SGROUP);
                    var item = ListHis.FirstOrDefault(w => w.Code == read.Code && w.SGROUP == read.SGROUP && w.PayType == read.PayType);//.OrderBy(w => w.SGROUP);
                                                                                                                                        //foreach (var item in _filter.LstPayHis.Where().OrderBy(w => w.SGROUP))
                                                                                                                                        //{
                    int result = 0;
                    var _payHis = _lstPayH.Where(w => w.PayType == item.PayType && w.Code == item.Code).FirstOrDefault();
                    Amount = ListHis.Sum(w => w.Amount);
                    var PayH = new HisPayHi();

                    if (_payHis != null)
                        PayH = _payHis;
                    else
                    {
                        result = 1;
                        PayH.EmpCode = item.EmpCode;
                        PayH.InYear = _filter.InYear;
                        PayH.SGROUP = item.SGROUP;
                        PayH.PayType = item.PayType;
                        PayH.Code = item.Code;
                        PayH.Description = item.Description;

                    }
                    if (_filter.InMonth == 1)
                        PayH.JAN = Amount;
                    else if (_filter.InMonth == 2)
                        PayH.FEB = Amount;
                    else if (_filter.InMonth == 3)
                        PayH.MAR = Amount;
                    else if (_filter.InMonth == 4)
                        PayH.APR = Amount;
                    else if (_filter.InMonth == 5)
                        PayH.MAY = Amount;
                    else if (_filter.InMonth == 6)
                        PayH.JUN = Amount;
                    else if (_filter.InMonth == 7)
                        PayH.JUL = Amount;
                    else if (_filter.InMonth == 8)
                        PayH.AUG = item.Amount;
                    else if (_filter.InMonth == 9)
                        PayH.SEP = Amount;
                    else if (_filter.InMonth == 10)
                        PayH.OCT = Amount;
                    else if (_filter.InMonth == 11)
                        PayH.NOV = Amount;
                    else if (_filter.InMonth == 12)
                        PayH.DECE = Amount;
                    if (result == 1)
                        DB.HisPayHis.Add(PayH);
                    else
                    {
                        DB.HisPayHis.Attach(PayH);
                        DB.Entry(PayH).State = System.Data.Entity.EntityState.Modified;
                    }

                }
                //foreach(var item in NoResult)
                //{
                //    if (_filter.InMonth == 1)
                //        item.JAN = 0;
                //    else if (_filter.InMonth == 2)
                //        item.FEB = 0;
                //    else if (_filter.InMonth == 3)
                //        item.MAR = 0;
                //    else if (_filter.InMonth == 4)
                //        item.APR = 0;
                //    else if (_filter.InMonth == 5)
                //        item.MAY = 0;
                //    else if (_filter.InMonth == 6)
                //        item.JUN = 0;
                //    else if (_filter.InMonth == 7)
                //        item.JUL = 0;
                //    else if (_filter.InMonth == 8)
                //        item.AUG = 0;
                //    else if (_filter.InMonth == 9)
                //        item.SEP = 0;
                //    else if (_filter.InMonth == 10)
                //        item.OCT = 0;
                //    else if (_filter.InMonth == 11)
                //        item.NOV = 0;
                //    else if (_filter.InMonth == 12)
                //        item.DECE = 0;

                //    var result = item.JAN + item.FEB + item.MAR + item.APR + item.MAY +
                //        item.JUN + item.JUL + item.AUG + item.SEP + item.OCT + item.NOV + item.DECE;
                //    if(result==0)
                //    {
                //        DBD.HisPayHis.Attach(item);
                //        DBD.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                //    }
                //    else
                //    {
                //        DB.HisPayHis.Attach(item);
                //        DB.Entry(item).State = System.Data.Entity.EntityState.Modified;
                //    }
                //}
                //var rowD = DBD.SaveChanges();
                var row1 = DB.SaveChanges();
            }
            finally
            {
                DB.Configuration.AutoDetectChangesEnabled = true;
                DBD.Configuration.AutoDetectChangesEnabled = true;
            }
        }
        public string Commit_PayCostAccount(GEN_Filter _filter)
        {
            try
            {
                DB.Configuration.AutoDetectChangesEnabled = false;

                var HisSal = _filter.HisGensalarys;
                DateTime DateNoW = DateTime.Now;
                var GLmappings = DB.PRGLmappings.ToList();
                int ID = Convert.ToInt32(_filter.Staff.GrpGLAcc);
                var CostCenter = DB.PRCostCenters.FirstOrDefault(w => w.ID == ID);
                int LineItem = 0;
                if (CostCenter != null)
                {
                    GLmappings = GLmappings.Where(w => w.ID == CostCenter.ID).ToList();
                    if (HisSal != null && GLmappings.Count > 0)
                    {
                        LineItem = 0;
                        var GLMap = GLmappings.Where(w => w.BenGrp == "SYS" && w.BenCode != "_TAXC" && !string.IsNullOrEmpty(w.GLCode)).ToList();
                        var ListSyAccount = DB.SYSHRBuiltinAccs.ToList();
                        List<object> ListObjectDictionary = new List<object>();
                        ListObjectDictionary.Add(_filter.HisGensalarys);
                        foreach (var item in GLMap)
                        {
                            if (item.GLCode == null) continue;
                            var objParam = ListSyAccount.FirstOrDefault(w => w.Code == item.BenCode);
                            if (objParam.ObjectName != null && objParam.FieldName != null)
                            {
                                LineItem += 1;
                                var GLBanChar = new HISGLBenCharge();
                                SwapValueBenCharge(GLBanChar, _filter, item, LineItem, objParam.IsPO,objParam.IsCredit);
                                var Amount = ClsInformation.GetFieldValues(objParam.ObjectName, ListObjectDictionary, objParam.FieldName);
                                if (Amount != null)
                                {
                                    if (Convert.ToDecimal(Amount) != 0)
                                    {
                                        GLBanChar.Amount = Convert.ToDecimal(Amount);
                                        if (item.BenCode == "_TAX" || item.BenCode == "_SPF")
                                            GLBanChar.IsDebitNote = true;
                                        DB.HISGLBenCharges.Add(GLBanChar);
                                        if (item.BenCode == "_TAX" && GLBanChar.Amount > 0)
                                        {
                                            DB.HISGLBenCharges.Add(GLBanChar);
                                            if (HisSal.TXPayType == "C")
                                            {
                                                LineItem += 1;
                                                PRGLmapping _TAXC = GLmappings.FirstOrDefault(w => w.BenGrp == "SYS" && w.BenCode == "_TAXC");
                                                var objParam1 = ListSyAccount.FirstOrDefault(w => w.Code == _TAXC.BenCode);
                                                var GLBanChar1 = new HISGLBenCharge();
                                                SwapValueBenCharge(GLBanChar1, _filter, _TAXC, LineItem, objParam.IsPO, objParam1.IsCredit);
                                                if (_TAXC != null)
                                                {
                                                    GLBanChar1.GLCode = _TAXC.GLCode;
                                                    GLBanChar1.Amount = Convert.ToDecimal(Amount);
                                                    DB.HISGLBenCharges.Add(GLBanChar1);
                                                }
                                                else
                                                {
                                                    return "INVALID_TAX_Payroll_Tax";
                                                }
                                            }
                                        }
                                        if(item.BenCode== "_CPF" && GLBanChar.Amount > 0)
                                        {
                                            LineItem += 1;
                                            var GLBanChar1 = new HISGLBenCharge();
                                            SwapValueBenCharge(GLBanChar1, _filter, item, LineItem, false,true);
                                            GLBanChar1.Amount = Convert.ToDecimal(Amount);
                                            DB.HISGLBenCharges.Add(GLBanChar1);
                                        }
                                    }
                                }
                            }
                        }
                        // var row = DB.SaveChanges();
                    }
                    //*************OverTime*************
                    var HisOT = _filter.ListHisEmpOT; //DB.HISGenOTHours.ToList();
                    HisOT = HisOT.Where(w => w.INYear == _filter.InYear && w.INMonth == _filter.InMonth && _filter.Staff.EmpCode == w.EmpCode).ToList();
                    var GLOT = GLmappings.Where(w => w.BenGrp == "OT" && HisOT.Where(x => x.OTCode == w.BenCode).Any()).ToList();
                    if (GLOT.Count > 0)
                    {
                        LineItem = 0;
                        foreach (var item in GLOT)
                        {
                            var OTValue = HisOT.FirstOrDefault(w => w.OTCode == item.BenCode);
                            if (OTValue != null)
                            {
                                LineItem += 1;
                                var GLBanChar1 = new HISGLBenCharge();
                                SwapValueBenCharge(GLBanChar1, _filter, item,LineItem,true);
                                if (GLBanChar1.GLCode == null) continue;
                                GLBanChar1.Amount = OTValue.OTRate * Convert.ToDecimal(OTValue.OTHour);
                                DB.HISGLBenCharges.Add(GLBanChar1);
                            }
                        }
                    }
                    //*************Allowance*************
                    var HisAllow = _filter.ListHisEmpAllw; //DB.HISGenAllowances.ToList();
                    HisAllow = HisAllow.Where(w => w.INYear == _filter.InYear && w.INMonth == _filter.InMonth && _filter.Staff.EmpCode == w.EmpCode).ToList();
                    var GLAllow = GLmappings.Where(w => w.BenGrp == "ALW" && HisAllow.Where(x => x.AllwCode == w.BenCode).Any()).ToList();
                    if (GLAllow.Count > 0)
                    {
                        LineItem = 0;
                        foreach (var item in GLAllow)
                        {
                            var AllwValue = HisAllow.FirstOrDefault(w => w.AllwCode == item.BenCode);
                            if (AllwValue != null)
                            {
                                LineItem += 1;
                                var GLBanChar1 = new HISGLBenCharge();
                                SwapValueBenCharge(GLBanChar1, _filter, item,LineItem,true);
                                GLBanChar1.Amount = AllwValue.AllwAm;
                                GLBanChar1.IsPO = true;
                                if (GLBanChar1.GLCode == null) continue;
                                DB.HISGLBenCharges.Add(GLBanChar1);
                            }
                        }
                    }
                    //*************Bonus*************
                    var HisBon = _filter.ListHisEmpBonu;// DB.HISGenBonus.ToList();
                    HisBon = HisBon.Where(w => w.INYear == _filter.InYear && w.INMonth == _filter.InMonth && _filter.Staff.EmpCode == w.EmpCode).ToList();
                    var GLBon = GLmappings.Where(w => w.BenGrp == "BON" && HisBon.Where(x => x.BonusCode == w.BenCode).Any()).ToList();
                    if (GLBon.Count > 0)
                    {
                        LineItem = 0;
                        foreach (var item in GLBon)
                        {
                            var BonValue = HisBon.FirstOrDefault(w => w.BonusCode == item.BenCode);
                            if (BonValue != null)
                            {
                                LineItem += 1;
                                var GLBanChar1 = new HISGLBenCharge();
                                SwapValueBenCharge(GLBanChar1, _filter, item,LineItem,true);
                                if (GLBanChar1.GLCode == null) continue;
                                GLBanChar1.IsPO = true;
                                GLBanChar1.Amount = Convert.ToDecimal(BonValue.BonusAM);
                                DB.HISGLBenCharges.Add(GLBanChar1);
                            }
                        }
                    }
                    //*************Deduction*************
                    var HisDed = _filter.ListHisEmpDed;// DB.HISGenDeductions.ToList();
                    HisDed = HisDed.Where(w => w.INYear == _filter.InYear && w.INMonth == _filter.InMonth && _filter.Staff.EmpCode == w.EmpCode).ToList();
                    var GLDed = GLmappings.Where(w => w.BenGrp == "DED" && HisDed.Where(x => x.DedCode == w.BenCode).Any()).ToList();
                    if (GLDed.Count > 0)
                    {
                        LineItem = 0;
                        foreach (var item in GLDed)
                        {
                            var DedValue = HisDed.FirstOrDefault(w => w.DedCode == item.BenCode);
                            if (DedValue != null)
                            {
                                LineItem += 1;
                                var GLBanChar1 = new HISGLBenCharge();
                                SwapValueBenCharge(GLBanChar1, _filter, item, LineItem,true, true);
                                if (GLBanChar1.GLCode == null) continue;
                                GLBanChar1.IsPO = true;
                                GLBanChar1.IsDebitNote = true;
                                if (item.BenCode == "LATE" || item.BenCode == "EARLY")
                                    GLBanChar1.IsDebitNote = null;
                                GLBanChar1.Amount = Convert.ToDecimal(DedValue.DedAm);
                                DB.HISGLBenCharges.Add(GLBanChar1);
                            }
                        }
                    }
                    var row1 = DB.SaveChanges();
                }
                return SYConstant.OK;
            }
            finally
            {
                DB.Configuration.AutoDetectChangesEnabled = true;
            }
        }
        public void Get_CostCenter(GEN_Filter _filter)
        {
            try
            {
                DB.Configuration.AutoDetectChangesEnabled = false;
                var His_Gen = DB.HisCostCharges.Where(w => w.EmpCode == _filter.EmpCode && w.InYear == _filter.InYear && w.InMonth == _filter.InMonth).ToList();
                if (His_Gen.Count() > 0)
                {
                    foreach (var item in His_Gen)
                    {
                        DB.HisCostCharges.Attach(item);
                        DB.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                        var rowD = DB.SaveChanges();
                    }
                }
                var CostGroup = DB.PRCostCenterGroups.FirstOrDefault(w => w.Code == _filter.Staff.Costcent);
                if (CostGroup != null)
                {
                    var lstCost = DB.PRCostCenterGroupItems.Where(w => w.CodeCCGoup == _filter.Staff.Costcent).ToList();
                    foreach (var Item in lstCost)
                    {
                        var HisCost = new HisCostCharge();
                        HisCost.CodeCCGoup = CostGroup.Code;
                        HisCost.GroupDescription = CostGroup.Description;
                        HisCost.EmpCode = _filter.EmpCode;
                        HisCost.InYear = _filter.InYear;
                        HisCost.InMonth = _filter.InMonth;
                        HisCost.CostCenter = Item.CostCenterType;
                        HisCost.Description = Item.Description;
                        HisCost.Charge = Convert.ToDecimal(Item.Charge);
                        HisCost.CreatedBy = User.UserName;
                        HisCost.CreatedOn = DateTime.Now;
                        DB.HisCostCharges.Add(HisCost);
                    }
                }
                var row1 = DB.SaveChanges();
            }
            finally
            {
                DB.Configuration.AutoDetectChangesEnabled = true;
            }
        }
        public void CalculatePensionFund(GEN_Filter filter, decimal? Salary)
        {
            filter.HisGensalarys.StaffPensionFundRate = 0;
            filter.HisGensalarys.StaffPensionFundAmount = 0;
            filter.HisGensalarys.StaffPensionFundAmountKH = 0;
            filter.HisGensalarys.CompanyPensionFundRate = 0;
            filter.HisGensalarys.CompanyPensionFundAmount = 0;
            filter.HisGensalarys.CompanyPensionFundAmountKH = 0;
            filter.HisGensalarys.SOSEC = 0;
            filter.HisGensalarys.CompHealth = 0;
            filter.HisGensalarys.StaffRiskKH = 0;
            filter.HisGensalarys.StaffRisk = 0;
            filter.HisGensalarys.TotalRisk = 0;
            filter.HisGensalarys.StaffHealthCareUSD = 0;
            filter.HisGensalarys.TotalHealthCare = 0;
            //Get employee service length
            if (Salary.Value > 0)
            {
                EmploymentInfo empInfo = new EmploymentInfo();
                DateTime FromDate = filter.Staff.StartDate.Value;
                DateTime ToDate = new DateTime(filter.ToDate.Year, filter.ToDate.Month, 1);
                double serviceLength = empInfo.GetEmploymentServiceLength(FromDate, filter.ToDate, ServiceLengthType.Month);
                double EmpLength = empInfo.GetEmploymentServiceLength(filter.Staff.DOB.Value, ToDate, ServiceLengthType.Month);

                decimal? maxContribution = 1200000;
                decimal? minContribution = 400000;
                decimal? basicSalary = 0;
                decimal? ExchangeRate = filter.HisGensalarys.NSSFRate;
                //Get Pension Fund Setting
                List<PRPensionFundSetting> lists = DB.PRPensionFundSettings.ToList();
                var list = lists.Where(x => x.SeviceLenghtFrom <= serviceLength & x.SeviceLenghtTo >= serviceLength).FirstOrDefault();

                var _Setting = DB.SYHRSettings.FirstOrDefault();
                if (_Setting != null)
                {
                    maxContribution = _Setting.MaxContribute;
                    minContribution = _Setting.MinContribue;
                }
                if (filter.Staff.ISNSSF == true)
                {
                    decimal? Rate = 0;
                    decimal? _RateUSD = 0;
                    if (filter.HisGensalarys != null && (filter.Staff != null && filter.Staff.NSSFContributionType == "RHP"))
                    {
                        basicSalary = Salary;

                        //Convert local currency to foreign currency
                        if ((EmpLength / 12.00) < 60)
                        {

                            basicSalary = basicSalary * ExchangeRate;// filter.HisGensalarys.ExchRate;
                            if (basicSalary > maxContribution) basicSalary = maxContribution;
                            else if (basicSalary < minContribution) basicSalary = minContribution;

                            if (list != null && list.StaffPercentage > 0)
                            {
                                Rate = ((basicSalary * list.StaffPercentage) / 100);
                                filter.HisGensalarys.StaffPensionFundRate = list.StaffPercentage;
                                filter.HisGensalarys.StaffPensionFundAmount = (Rate / ExchangeRate).Value();
                                filter.HisGensalarys.StaffPensionFundAmountKH = Rate;
                            }
                            if (list != null && list.ComPercentage > 0)
                            {
                                Rate = (basicSalary * list.ComPercentage) / 100;
                                filter.HisGensalarys.CompanyPensionFundRate = list.ComPercentage;
                                filter.HisGensalarys.CompanyPensionFundAmount = (Rate / ExchangeRate).Value();
                                filter.HisGensalarys.CompanyPensionFundAmountKH = Rate;
                            }
                            filter.HisGensalarys.AVGGrSOSC = basicSalary;
                            filter.HisGensalarys.CompanyPensionFundAmountKH = Math.Round((decimal)filter.HisGensalarys.CompanyPensionFundAmountKH, 0);
                            filter.HisGensalarys.StaffPensionFundAmountKH = Math.Round((decimal)filter.HisGensalarys.StaffPensionFundAmountKH, 0);
                        }
                    }

                    filter.HisGensalarys.AVGGrSOSC = basicSalary;
                    if (_Setting.StaffRisk.HasValue)
                    {
                        Rate = (decimal)(basicSalary * _Setting.StaffRisk);
                        _RateUSD = (Rate.Value / ExchangeRate).Value();
                        filter.HisGensalarys.StaffRiskKH = Math.Round(Rate.Value, 0);
                        filter.HisGensalarys.StaffRisk = ClsRounding.Rounding(_RateUSD.Value, SYConstant.DECIMAL_PLACE, "N");
                        filter.HisGensalarys.TotalRisk = filter.HisGensalarys.StaffRisk;
                    }
                    if (_Setting.StaffHealthCare.HasValue)
                    {
                        Rate = (decimal)(basicSalary * _Setting.StaffHealthCare);
                        _RateUSD = (Rate.Value / ExchangeRate).Value();
                        filter.HisGensalarys.StaffHealth = Math.Round(Rate.Value, 0);
                        filter.HisGensalarys.StaffHealthCareUSD = ClsRounding.Rounding(_RateUSD.Value, SYConstant.DECIMAL_PLACE, "N");
                        filter.HisGensalarys.TotalHealthCare = filter.HisGensalarys.StaffHealthCareUSD;
                    }
                }
            }
        }
        public string GenerateSalarys(string EmpCode)
        {
            DB = new HumicaDBContext();
            string Result = "";
            try
            {
                DateTime FromDate = Filter.InMonth.Date;
                DateTime ToDate = DateTimeHelper.DateInMonth(Filter.InMonth);
                DateTime tempFromDate = FromDate;
                var LstExchangeRate = DB.PRExchRates.ToList();
                var ExchangeRate = LstExchangeRate.FirstOrDefault(w => w.InYear == Filter.InMonth.Year && w.InMonth == Filter.InMonth.Month);
                if (ExchangeRate == null)
                {
                    return "EXCHANGERATE";
                }
                var LstAppSalary = DB.HisPendingAppSalaries.Where(w => w.InMonth == Filter.InMonth.Month && w.InYear == Filter.InMonth.Year).ToList();
                if (LstAppSalary.Where(w => w.IsLock == true).Count() > 0)
                {
                    return "APPROVE_SALARY";
                }
                Filter.ExchangeRate = ExchangeRate.Rate;
                var _lstStaff = DB.HRStaffProfiles.ToList();
                var _lstPara = DB.PRParameters.ToList();
                var _lstAllowance = DB.PREmpAllws.ToList();
                var _lstBon = DB.PREmpBonus.ToList();
                var _lstEmpCareer = DB.HREmpCareers.ToList();
                var _lstRewardType = DB.PR_RewardsType.ToList();
                var _lstBankFee = DB.PRBankFees.ToList();
                var _listPayHis = DB.HisPayHis.Where(w => w.InYear == Filter.InMonth.Year).ToList();
                var ListBranch = DMS.HRBranches.ToList();
                var ListPayFirst = DB.HISGenFirstPays.Where(w => w.INYear == Filter.InMonth.Year && w.INMonth == Filter.InMonth.Month).ToList();
                string CLEARED = SYDocumentStatus.CLEARED.ToString();
                var ListEmpPayBack = DB.PREmpHolds.Where(w => w.Status == CLEARED).ToList();
                ListEmpPayBack = ListEmpPayBack.Where(w => w.PayBack.Value.Year == Filter.InMonth.Year &&
                 w.PayBack.Value.Month == Filter.InMonth.Month).ToList();
                var _lstCaeCode = DB.HRCareerHistories.Where(w => w.NotCalSalary == true).ToList();
                int InYear = Filter.InMonth.Year;
                bool IsTax = false;
                var LstSySetting = DMS.SYSettings.Where(w => w.ObjectReference == "N").ToList();
                var getSetting = LstSySetting.FirstOrDefault(w => w.SettingName == "SALARYTAX");
                if (getSetting != null)
                {
                    var _isTax = getSetting.SettinValue;
                    if (_isTax == "YES") IsTax = true;
                }
                var DEFAULT_CURRENCY = LstSySetting.FirstOrDefault(w => w.SettingName == "DEFAULT_CURRENCY");
                var ROUND_UP = LstSySetting.FirstOrDefault(w => w.SettingName == "ROUND_UP");

                string[] Emp = EmpCode.Split(';');
                Progress = Emp.Count();
                _lstStaff = _lstStaff.ToList();
                _lstPara = _lstPara.ToList();
                _lstAllowance = _lstAllowance.ToList();
                _lstEmpCareer = _lstEmpCareer.ToList();
                _lstRewardType = _lstRewardType.ToList();
                _lstBon = _lstBon.ToList();
                _lstBankFee = _lstBankFee.ToList();
                var ListTaxSetting = DB.PRTaxSettings.ToList();
                int i = 0;
                var SYData = DMS.SYDatas.Where(w => w.DropDownType == "PR_YEAR_SELECT").ToList();
                if (SYData.Where(w => w.SelectValue == InYear.ToString()).Count() == 0)
                {
                    var _sydata = new SYData()
                    {
                        SelectText = InYear.ToString(),
                        SelectValue = InYear.ToString(),
                        DropDownType = "PR_YEAR_SELECT",
                        IsActive = true
                    };
                    DMS.SYDatas.Add(_sydata);
                    DMS.SaveChanges();
                }
                decimal _p = 0;
                var ListCompany = DMS.SYHRCompanies.First();
                if (ListProgress != null)
                    ListProgress.Where(w => w.UserName == User.UserName).ToList().ForEach(x => x.Progress = Progress);
                bool IsGenerate = false;
                var HRSetting = DB.SYHRSettings.First();
                foreach (var Code in Emp)
                {
                    if (Code.Trim() != "")
                    {
                        Result = Code;
                        GEN_Filter _Filter = new GEN_Filter();
                        _Filter.Staff = _lstStaff.FirstOrDefault(w => w.EmpCode == Code);
                        _Filter.Parameter = _lstPara.FirstOrDefault(w => w.Code == _Filter.Staff.PayParam);
                        if (!_Filter.Parameter.IsPrevoiuseMonth.IsNullOrZero())
                        {
                            DateTime tempDate = tempFromDate.AddMonths(-1);
                            FromDate = new DateTime(tempDate.Year, tempDate.Month, _Filter.Parameter.FROMDATE.Value().Day);
                            ToDate = new DateTime(ToDate.Year, ToDate.Month, _Filter.Parameter.TODATE.Value().Day);
                        }
                        _Filter.Default_Curremcy = DEFAULT_CURRENCY.SettinValue;
                        if (ROUND_UP != null) _Filter.Round_UP = ROUND_UP.SettinValue;
                        _Filter.LstEmpAllow = new List<PREmpAllw>();
                        _Filter.LstEmpCareer = new List<HREmpCareer>();
                        _Filter.LstRewardsType = new List<PR_RewardsType>();
                        _Filter.LstPayHis = new List<ClsPayHis>();
                        _Filter.LstEmpHold = new List<PREmpHold>();
                        _Filter.EmpCode = Code;
                        _Filter.InYear = Filter.InMonth.Year;
                        _Filter.InMonth = Filter.InMonth.Month;
                        _Filter.FromDate = FromDate;
                        _Filter.ToDate = ToDate;
                        _Filter.CompanyCode = ListCompany;
                        _Filter.HisGensalarys = new HISGenSalary();
                        _Filter.ListHisEmpBonu = new List<HISGenBonu>();
                        _Filter.ListHisEmpAllw = new List<HISGenAllowance>();
                        _Filter.ListHisEmpDed = new List<HISGenDeduction>();
                        _Filter.ListHisEmpOT = new List<HISGenOTHour>();
                        _Filter.ListHisEmpLeave = new List<HISGenLeaveDeduct>();
                        _Filter.ListTaxSetting = new List<PRTaxSetting>();
                        _Filter.LstEmpAllow = _lstAllowance.Where(w => w.EmpCode == Code).ToList();
                        _Filter.LstEmpBon = _lstBon.Where(w => w.EmpCode == Code).ToList();
                        _Filter.LstEmpCareer = _lstEmpCareer.Where(w => _lstStaff.Where(x => x.EmpCode == w.EmpCode && x.EmpCode == Code).Any()).ToList();
                        _Filter.LstRewardsType = _lstRewardType.ToList();
                        _Filter.LstBankFee = _lstBankFee.ToList();
                        _Filter.ListTaxSetting = ListTaxSetting.ToList();
                        _Filter.LstEmpHold = ListEmpPayBack.Where(w => w.EmpCode == Code).ToList();
                        _Filter.HisAppSalary = LstAppSalary.FirstOrDefault(w => w.IsLock == false);
                        _Filter.NSSFSalaryType = HRSetting.NSSFSalaryType;
                        _Filter.Setting = HRSetting;
                        var Branch = ListBranch.Where(w => w.Code == _Filter.Staff.Branch).FirstOrDefault();
                        Delete_Generate(_Filter);
                        Generate_Salary(_Filter, Branch, _lstCaeCode, ref IsGenerate);
                        if (_Filter.HisGensalarys.EmpCode != null)
                        {
                            Calculate_OT(_Filter, Branch);
                            Calculate_Allowance(_Filter);
                            Calculate_Deduction(_Filter, FromDate, ToDate);
                            Calculate_Late_Early_Deduction(_Filter, FromDate, ToDate);
                            Calculate_Misscan_Deduction(_Filter, FromDate, ToDate);
                            Calculate_Bonus(_Filter, FromDate, ToDate);
                            if (_Filter.Staff.IsAtten != true)
                                Calculate_LeaveDeduct(_Filter, FromDate, ToDate);
                            if (ListPayFirst.Where(w => w.EmpCode == Code).ToList().Count() > 0)
                            {
                                var _netPay = ListPayFirst.Where(w => w.EmpCode == Code).ToList().Sum(w => w.NetWage);
                                _Filter.HisGensalarys.FirstPaymentAmount = _netPay.Value;
                            }

                            Calculate_TAX(_Filter, FromDate, ToDate, true, Branch, IsTax);
                            Commit_PaySlip(_Filter, Branch);
                            Commit_PayHis(_Filter, _listPayHis.Where(w => w.EmpCode == Code).ToList());
                            if (!string.IsNullOrEmpty(_Filter.Staff.GrpGLAcc) && _Filter.Staff.GrpGLAcc != "null")
                                Commit_PayCostAccount(_Filter);
                            Get_CostCenter(_Filter);
                        }

                    }
                    i += 1;
                    _p = i;
                    if (ListProgress != null)
                        ListProgress.Where(w => w.UserName == User.UserName).ToList().ForEach(x => x.Percen = _p / x.Progress * 100);

                }

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Result;
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
                log.DocurmentAction = Result;
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
                log.DocurmentAction = Result;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }

        }
        public string Delete_PayRecord(string EmpCode, DateTime InMonth)
        {
            try
            {
                var AppSalary = DB.HisPendingAppSalaries.Where(w => w.InYear == InMonth.Year && w.InMonth == InMonth.Month && w.IsLock == true).ToList();
                if (AppSalary.Count() > 0)
                {
                    return "APPROVE_SALARY";
                }
                GEN_Filter _filter = new GEN_Filter();
                _filter.Staff = new HRStaffProfile();
                _filter.Staff.EmpCode = EmpCode;
                _filter.EmpCode = EmpCode;
                _filter.InYear = InMonth.Year;
                _filter.InMonth = InMonth.Month;

                Delete_Generate(_filter);
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
        public string Generate_NSSF(DateTime InMonth, List<HRBranch> _listBranch)
        {
            DB = new HumicaDBContext();
            try
            {
                DB.Configuration.AutoDetectChangesEnabled = false;
                var Company = DMS.SYHRCompanies.First();
                var ExchangeRate = DB.PRExchRates.FirstOrDefault(w => w.InYear == InMonth.Year && w.InMonth == InMonth.Month);
                if (ExchangeRate == null)
                {
                    return "EXCHANGERATE";
                }
                Filter.ExchangeRate = ExchangeRate.NSSFRate;
                if (Filter.ExchangeRate > 0)
                {
                    var _listEmp = DB.HRStaffProfiles.Where(w => w.ISNSSF == true).ToList();

                    _listEmp = _listEmp.Where(w => _listBranch.Where(x => x.Code == w.Branch).Any()).ToList();
                    if (Filter.Branch != null)
                    {
                        string[] Branch = Filter.Branch.Split(',');
                        List<string> LstBranch = new List<string>();
                        foreach (var read in Branch)
                        {
                            if (read.Trim() != "")
                            {
                                LstBranch.Add(read.Trim());
                            }
                        }
                        _listEmp = _listEmp.Where(w => LstBranch.Contains(w.Branch)).ToList();
                    }
                    var result = DB.HISGenSalaries.Where(w => w.INYear == InMonth.Year && w.INMonth == InMonth.Month).ToList();
                    result = result.Where(w => _listEmp.Where(x => x.EmpCode == w.EmpCode).Any()).ToList();
                    var _His_Gen = DB.HISGLBenCharges.Where(w => w.InYear == InMonth.Year && w.InMonth == InMonth.Month && (w.BenCode == "_NSSF" || w.BenCode == "_PAYABLE")).ToList();
                    //var GLmappings = DB.PRGLmappings.ToList();
                    var GLmappings = DB.PRGLmappings.Where(w => w.BenGrp == "SYS").ToList();
                    var HRSetting = DB.SYHRSettings.FirstOrDefault();
                    decimal? GrossPay = 0;
                    decimal? maxContribution = 1200000;
                    decimal? minContribution = 400000;
                    if (HRSetting != null)
                    {
                        maxContribution = HRSetting.MaxContribute;
                        minContribution = HRSetting.MinContribue;
                    }
                    foreach (var item in result)
                    {
                        if (HRSetting.NSSFSalaryType == SalaryType.NP.ToString())
                        {
                            GrossPay = item.NetWage;
                        }
                        else if (HRSetting.NSSFSalaryType == SalaryType.GP.ToString())
                        {
                            GrossPay = item.GrossPay;
                        }
                        else GrossPay = item.Salary;

                        var rate = GrossPay * ExchangeRate.NSSFRate;
                        if (rate > maxContribution) rate = maxContribution;
                        else if (rate < minContribution) rate = minContribution;


                        item.SOSEC = Math.Round((decimal)(rate * HRSetting.ComRisk), 0);
                        item.AVGGrSOSC = rate;
                        item.NSSFRate = ExchangeRate.NSSFRate;
                        item.CompHealth = Math.Round((decimal)(rate * HRSetting.ComHealthCare), 0);
                        decimal TotalRisk = ClsRounding.Rounding(item.SOSEC.Value / ExchangeRate.NSSFRate, SYConstant.DECIMAL_PLACE, "N");
                        item.TotalRisk =item.StaffRisk + TotalRisk;
                        decimal TotalHealthCare = ClsRounding.Rounding(item.CompHealth.Value / ExchangeRate.NSSFRate, SYConstant.DECIMAL_PLACE, "N");
                        item.TotalHealthCare = item.StaffHealthCareUSD + TotalHealthCare;
                        DB.HISGenSalaries.Attach(item);
                        DB.Entry(item).Property(w => w.SOSEC).IsModified = true;
                        DB.Entry(item).Property(w => w.AVGGrSOSC).IsModified = true;
                        DB.Entry(item).Property(w => w.NSSFRate).IsModified = true;
                        DB.Entry(item).Property(w => w.CompHealth).IsModified = true;
                        DB.Entry(item).Property(w => w.StaffHealth).IsModified = true;
                        DB.Entry(item).Property(w => w.TotalRisk).IsModified = true;
                        DB.Entry(item).Property(w => w.TotalHealthCare).IsModified = true;

                        //*****************************

                        var His_Gen = _His_Gen.Where(w => w.EmpCode == item.EmpCode).ToList();
                        if (His_Gen.Count() > 0)
                        {
                            foreach (var item1 in His_Gen)
                            {
                                DB.HISGLBenCharges.Attach(item1);
                                DB.Entry(item1).State = System.Data.Entity.EntityState.Deleted;
                            }
                        }

                        var Staff = _listEmp.FirstOrDefault(w => w.EmpCode == item.EmpCode);
                        if (Staff == null) continue;
                        var ListGL = GLmappings.Where(w => Staff.GrpGLAcc == w.Branch).ToList();
                        var GLMap = ListGL.FirstOrDefault(w => w.BenCode == "_NSSF");
                        if (GLMap != null && (item.TotalRisk + item.TotalHealthCare)>0)
                        {
                            int LineItem = 1;
                            var GLBanChar1 = new HISGLBenCharge();
                            GEN_Filter _filter = new GEN_Filter();
                            _filter.Staff = Staff;
                            _filter.CompanyCode = Company;
                            _filter.InYear = InMonth.Year;
                            _filter.InMonth = InMonth.Month;
                            _filter.ToDate = InMonth;
                            SwapValueBenCharge(GLBanChar1, _filter, GLMap, LineItem,true);
                            GLBanChar1.Amount = item.TotalRisk + item.TotalHealthCare;
                            DB.HISGLBenCharges.Add(GLBanChar1);

                            var _GL1 = ListGL.FirstOrDefault(w => w.BenCode == "_PAYABLE");
                            if (_GL1 != null)
                            {
                                var GLBanChar2 = new HISGLBenCharge();
                                SwapValueBenCharge(GLBanChar2, _filter, _GL1, LineItem,false, true);
                                GLBanChar2.Amount = GLBanChar1.Amount;
                                DB.HISGLBenCharges.Add(GLBanChar2);
                            }
                        }
                    }
                    int row = DB.SaveChanges();
                }
                else
                {
                    return "EXCHANGERATE";
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
            finally
            {
                DB.Configuration.AutoDetectChangesEnabled = true;
            }
        }
        public string Transfer_NSSF(DateTime InMonth, List<HRBranch> _listBranch)
        {
            DB = new HumicaDBContext();
            try
            {
                var ExchangeRate = DB.PRExchRates.FirstOrDefault(w => w.InYear == InMonth.Year && w.InMonth == InMonth.Month);
                if (ExchangeRate == null)
                {
                    return "EXCHANGERATE";
                }
                else if (ExchangeRate.NSSFRate == 0)
                {
                    return "EXCHANGERATE";
                }
                string fileName = System.Web.HttpContext.Current.Server.MapPath("~/Content/TEMPLATE/E-Form_v10.accdb");

                OleDbCommand cmd = new OleDbCommand();
                OleDbCommand cmd1 = new OleDbCommand();
                //string Password = "P@$$wdEForm";
                //string myStr = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={fileName};" +
                //       $"Jet OLEDB:Database Password={Password};Persist Security Info=True;";

                OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder
                {
                    Provider = "Microsoft.ACE.OLEDB.12.0",
                    DataSource = fileName,
                    PersistSecurityInfo = true
                };
                builder["Jet OLEDB:Database Password"] = "P@$$wdEForm";
                using (OleDbConnection Con = new OleDbConnection(builder.ConnectionString))
                {
                    Con.Open();
                    cmd.Connection = Con;
                    cmd.CommandText = "DELETE FROM TBL_BENEFICIARY";
                    cmd.ExecuteNonQuery();

                    string str = "";
                    var listBranch = DMS.HRBranches.ToList();
                    var listCompany = DMS.HRCompanies.ToList();
                    var _listNSFF = DBV.HR_NSSF_Transfer.Where(w => w.INYear == InMonth.Year && w.INMonth == InMonth.Month).ToList();
                    _listNSFF = _listNSFF.Where(w => _listBranch.Where(x => x.Code == w.Branch).Any()).ToList();
                    if (Filter.Branch != null)
                    {
                        string[] Branch = Filter.Branch.Split(',');
                        List<string> LstBranch = new List<string>();
                        foreach (var read in Branch)
                        {
                            if (read.Trim() != "")
                            {
                                LstBranch.Add(read.Trim());
                            }
                        }
                        _listNSFF = _listNSFF.Where(w => LstBranch.Contains(w.Branch)).ToList();
                        //_listNSFF = _listNSFF.Where(w => w.Branch == Filter.Branch).ToList();
                    }
                    _listNSFF = _listNSFF.Where(w => w.INYear == InMonth.Year && w.INMonth == InMonth.Month).ToList();
                    foreach (var item in _listNSFF)
                    {
                        int Sex = 0;
                        var DOB = item.DOB.Value.ToString("MM/dd/yyyy");
                        var StartDate = item.StartDate.Value.ToString("MM/dd/yyyy");
                        if (item.Sex == "M" || item.Sex == "Male") Sex = 1;
                        str = @"INSERT INTO TBL_BENEFICIARY(Enter_ID,NSSF_ID,ID_National,Ben_FName_kh,Ben_LName_kh,Ben_FName_Eng,Ben_LName_Eng,Sex,
                         Date_of_Birth, Nationality, Hired_Date, Ben_Group, Ben_Position,Ben_Sector, Ben_Status, Ben_Wage, Ben_WageInDollar,Ben_Assum, Ben_Contribution)
                         VALUES('" + item.EmpCode + "','" + item.SOCSO + "','" + item.IDCard + "','" + item.OthFirstName + "','" + item.OthLastName + "','" +
                             item.FirstName + "','" + item.LastName + "'," + Sex + ",'" + DOB + "','" + item.NATIONID + "','" + StartDate + "','" + item.DeptDesc + "','" +
                             item.PostDesc + "',1,0," + item.NSSFGROSS + "," + item.NSSFGROSSUSD + "," + item.AVGGrSOSC + "," + item.SOSEC + ")";
                        cmd.CommandText = str;
                        cmd.ExecuteNonQuery();
                    }
                    string enterID = "";
                    if (Filter.Branch != null)
                    {
                        var result = listBranch.FirstOrDefault(w => w.Code == Filter.Branch);
                        if (result != null)
                        {
                            var _company = listCompany.FirstOrDefault(w => w.Company == result.CompanyCode);
                            enterID = _company.NSSFNo;
                        }
                    }
                    else enterID = listCompany.FirstOrDefault().NSSFNo;

                    if (enterID == null)
                    {
                        return "INVALID_NSSF_NO";
                    }
                    cmd1 = new OleDbCommand("Select COUNT(*) from tbl_DateContribution", Con);
                    string cmdText = !Microsoft.VisualBasic.CompilerServices.Operators.ConditionalCompareObjectGreater(cmd1.ExecuteScalar(), 0, false) ? "Insert into tbl_DateContribution(Enter_ID,Month_Contribution,Year_Contribution,ExchangeRate) values(@enterID,@month,@year,@exchangerate)" : "Update tbl_DateContribution SET Enter_ID=@enterID, Month_Contribution = @month,Year_Contribution = @year,ExchangeRate=exchangerate";
                    cmd1 = new OleDbCommand(cmdText, Con);
                    cmd1.Parameters.Add("@enterID", OleDbType.VarChar).Value = enterID;
                    cmd1.Parameters.Add("@month", OleDbType.Integer).Value = InMonth.Month;
                    cmd1.Parameters.Add("@year", OleDbType.Integer).Value = InMonth.Year;
                    cmd1.Parameters.Add("@exchangerate", OleDbType.VarChar).Value = ExchangeRate.NSSFRate;
                    cmd1.ExecuteNonQuery();
                    Con.Close();
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
        public string Generate_Senority(string EmpCode, FTFilterEmployee Filter1)
        {
            string ID = "";
            try
            {
                try
                {
                    DB = new HumicaDBContext();
                    DB.Configuration.AutoDetectChangesEnabled = false;
                    DateTime FromDate = new DateTime(Filter1.FromDate.Year, Filter1.FromDate.Month, 1);
                    DateTime ToDate = new DateTime(Filter1.ToDate.Year, Filter1.ToDate.Month, DateTime.DaysInMonth(Filter1.ToDate.Year, Filter1.ToDate.Month));
                    var Temp = DB.PRSincerities.ToList();
                    if (Temp.Count() > 0)
                    {
                        foreach (var item in Temp)
                        {
                            DB.PRSincerities.Attach(item);
                            DB.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                            var rowD = DB.SaveChanges();
                        }
                    }
                    string[] Emp = EmpCode.Split(';');
                    var ListSalary = DB.HISGenSalaries.ToList();
                    var ListStaff = DB.HRStaffProfiles.ToList();
                    var listParameter = DB.PRParameters.ToList();
                    var ListEmpCareer = DB.HREmpCareers.ToList();
                    ListStaff = ListStaff.ToList();
                    listParameter = listParameter.ToList();
                    ListSalary = ListSalary.Where(w => w.ToDate.Value.Date >= FromDate.Date && w.ToDate.Value.Date <= ToDate.Date).ToList();
                    var _StaffInPro = ListStaff.Where(w => w.Probation.Value.Date >= ToDate.Date.AddMonths(-1)).ToList();
                    ListSalary = ListSalary.Where(w => !_StaffInPro.Where(x => x.EmpCode == w.EmpCode).Any()).ToList();
                    var _StaffPro = ListStaff.Where(w => w.Probation.Value.Date >= FromDate.Date && w.Probation.Value.Date <= ToDate.Date).ToList();
                    decimal RateInDate = 1.25M;

                    foreach (var Code in Emp)
                    {
                        decimal _SPrate = Filter1.Rate;
                        int CMonth = GetMonthDifference(FromDate, ToDate) + 1;
                        ID = Code;
                        if (Code.Trim() != "")
                        {
                            var Seniority = new PRSincerity();
                            var Staff = ListStaff.FirstOrDefault(w => w.EmpCode == Code);
                            var Parameter = listParameter.FirstOrDefault(w => w.Code == Staff.PayParam);
                            DateTime FFromDate = GetFromDate(Parameter, Filter1.ToDate);
                            DateTime FToDate = GetToDate(Parameter, Filter1.ToDate);

                            decimal DayInMonth = Get_WorkingDay_Salary(Parameter, FFromDate, FToDate);
                            decimal AVGSalary = 0;
                            decimal Rate = 0;
                            decimal Balance = 0;
                            var result = ListSalary.Where(w => w.EmpCode == Code).ToList();
                            if (result.Count() == 0) continue;
                            //Probation
                            if (_StaffPro.Where(w => w.EmpCode == Staff.EmpCode && w.Probation.Value.Year == FToDate.Year
                            && w.Probation.Value.Month == FToDate.Month).Any())
                            {
                                var _result = result.Where(w => w.INYear == Staff.Probation.Value.Year &&
                                  w.INMonth == Staff.Probation.Value.Month).ToList();
                                DateTime Prob = Staff.Probation.Value;
                                DateTime _ToDate = new DateTime(Prob.Year, Prob.Month, DateTime.DaysInMonth(Prob.Year, Prob.Month));
                                decimal _DayInMonth = Get_WorkingDay(Parameter, Prob, _ToDate);
                                if (_DayInMonth <= 21)
                                {
                                    result = result.Where(w => w.ToDate.Value.Date >= _ToDate.Date).ToList();
                                }
                                //else
                                //{
                                //    result = result.Where(w => w.FromDate.Value.Date >= _ToDate.Date).ToList();
                                //}
                                CMonth = result.Count;
                                _SPrate = RateInDate * CMonth;
                            }

                            #region "Resgin"
                            var LstEmpAllow = DB.PREmpAllws.ToList();
                            var LstEmpBon = DB.PREmpBonus.ToList();
                            decimal TempSalary = 0;
                            decimal Salary = 0;
                            decimal TempAmount = 0;
                            var _listGenD = new List<HISGenSalaryD>();
                            var LstEmpCareer = ListEmpCareer.Where(w => w.EmpCode == Code).ToList();
                            if (Staff.TerminateStatus == "TERMINAT" && Staff.SalaryFlag == false
                                && Staff.DateTerminate.Year == Filter1.ToDate.Year && Staff.DateTerminate.Month == Filter1.ToDate.Month)
                            {
                                //
                                result = result.Where(w => w.ToDate.Value.Date != ToDate.Date).ToList();

                                var Emp_C = from C in LstEmpCareer
                                            where ((C.FromDate >= FFromDate && C.FromDate <= ToDate) || (C.ToDate >= FFromDate && C.ToDate <= ToDate) ||
                                     (FFromDate >= C.FromDate && FFromDate <= C.ToDate) || (ToDate >= C.FromDate && ToDate <= C.ToDate))
                                     && C.EmpCode == Code && C.CareerCode != "TERMINAT"
                                            select new { C.CareerCode, C.EmpCode, C.NewSalary, C.FromDate, C.ToDate };
                                int C_Career = Emp_C.Count();
                                foreach (var emp in Emp_C)
                                {
                                    DateTime PToDate = emp.ToDate.Value;
                                    DateTime PFromDate = emp.FromDate.Value;
                                    if (PToDate > ToDate) PToDate = ToDate;
                                    if (PFromDate < FFromDate) PFromDate = FFromDate;
                                    Decimal ActualWorkDay = Get_WorkingDay(Parameter, PFromDate, PToDate);

                                    if (PFromDate == FFromDate && PToDate == ToDate)
                                    {
                                        TempSalary = emp.NewSalary;
                                    }
                                    else
                                    {
                                        decimal TMPD = 0;
                                        TMPD = Convert.ToDecimal(_listGenD.Sum(x => x.ActWorkDay));
                                        if ((TMPD + ActualWorkDay) > DayInMonth) ActualWorkDay = Convert.ToDecimal(DayInMonth) - TMPD;
                                        Rate = Convert.ToDecimal(emp.NewSalary / DayInMonth);
                                        TempSalary = ClsRounding.Rounding((Rate * ActualWorkDay), SYConstant.DECIMAL_PLACE, "N");
                                    }
                                    Salary += TempSalary;
                                }

                                var EmpAllw = LstEmpAllow.Where(w => w.EmpCode == Code && w.FromDate.Value.Year == ToDate.Year && w.FromDate.Value.Month == ToDate.Month &&
                                   w.ToDate.Value.Year == ToDate.Year && w.ToDate.Value.Month == ToDate.Month && w.Status == 1).ToList();
                                Salary += Convert.ToDecimal(EmpAllw.Where(w => w.AllwCode != "SP").Sum(w => w.Amount));

                                #region ********Allw Period********
                                var EmpAllwP = LstEmpAllow.Where(w => w.EmpCode == Code && ((FFromDate >= w.FromDate && FFromDate <= w.ToDate) || (ToDate >= w.FromDate && ToDate <= w.ToDate))
                                && w.Status == 0).ToList();
                                foreach (var Allw in EmpAllwP.Where(w => w.AllwCode != "SP"))
                                {
                                    var ActualWorkDay = Get_WorkingDay_Allw(Parameter, FFromDate, ToDate);
                                    DateTime TempGenFromDate = new DateTime();
                                    DateTime TempGenToDate = new DateTime();
                                    int HasAllowanceDetail = 0;
                                    if (FFromDate >= Allw.FromDate && ToDate <= Allw.ToDate)
                                    {
                                        TempGenFromDate = FFromDate;
                                        TempGenToDate = ToDate;
                                        HasAllowanceDetail = 1;
                                    }
                                    else if (FFromDate >= Allw.FromDate && Allw.ToDate <= ToDate)
                                    {
                                        TempGenFromDate = FFromDate;
                                        TempGenToDate = Allw.ToDate.Value;
                                        HasAllowanceDetail = 1;
                                    }
                                    else if (Allw.FromDate >= FFromDate && Allw.FromDate <= ToDate && Allw.ToDate >= ToDate)
                                    {
                                        TempGenFromDate = Allw.FromDate.Value;
                                        TempGenToDate = ToDate;
                                        HasAllowanceDetail = 1;
                                    }
                                    else if (Allw.FromDate >= FFromDate && Allw.ToDate <= ToDate)
                                    {
                                        TempGenFromDate = Allw.FromDate.Value;
                                        TempGenToDate = Allw.ToDate.Value;
                                        HasAllowanceDetail = 1;
                                    }
                                    if (HasAllowanceDetail == 1)
                                    {
                                        decimal TempWorkDay = Get_WorkingDay(Parameter, TempGenFromDate, TempGenToDate);
                                        if (TempGenFromDate == FFromDate && TempGenToDate == ToDate)
                                            TempAmount = Convert.ToDecimal(Allw.Amount);
                                        else
                                            TempAmount = Convert.ToDecimal((Allw.Amount / ActualWorkDay) * TempWorkDay);
                                    }
                                    Salary += TempAmount;
                                }
                                #endregion

                                TempAmount = 0;
                                var EmpBon = LstEmpBon.Where(w => w.EmpCode == Code && w.FromDate.Value.Year == ToDate.Year && w.FromDate.Value.Month == ToDate.Month &&
                                   w.ToDate.Value.Year == ToDate.Year && w.ToDate.Value.Month == ToDate.Month && w.Status == 1).ToList();
                                Salary += EmpBon.Sum(w => w.Amount);

                                #region ********Bon Period********
                                var EmpBonP = LstEmpBon.Where(w => w.EmpCode == Code && ((FFromDate >= w.FromDate && FFromDate <= w.ToDate) || (ToDate >= w.FromDate && ToDate <= w.ToDate))
                                && w.Status == 0).ToList();
                                foreach (var Allw in EmpBonP)
                                {
                                    var ActualWorkDay = Get_WorkingDay_Allw(Parameter, FFromDate, ToDate);
                                    DateTime TempGenFromDate = new DateTime();
                                    DateTime TempGenToDate = new DateTime();
                                    int HasAllowanceDetail = 0;
                                    if (FFromDate >= Allw.FromDate && ToDate <= Allw.ToDate)
                                    {
                                        TempGenFromDate = FFromDate;
                                        TempGenToDate = ToDate;
                                        HasAllowanceDetail = 1;
                                    }
                                    else if (FFromDate >= Allw.FromDate && Allw.ToDate <= ToDate)
                                    {
                                        TempGenFromDate = FFromDate;
                                        TempGenToDate = Allw.ToDate.Value;
                                        HasAllowanceDetail = 1;
                                    }
                                    else if (Allw.FromDate >= FFromDate && Allw.FromDate <= ToDate && Allw.ToDate >= ToDate)
                                    {
                                        TempGenFromDate = Allw.FromDate.Value;
                                        TempGenToDate = ToDate;
                                        HasAllowanceDetail = 1;
                                    }
                                    else if (Allw.FromDate >= FFromDate && Allw.ToDate <= ToDate)
                                    {
                                        TempGenFromDate = Allw.FromDate.Value;
                                        TempGenToDate = Allw.ToDate.Value;
                                        HasAllowanceDetail = 1;
                                    }
                                    if (HasAllowanceDetail == 1)
                                    {
                                        decimal TempWorkDay = Get_WorkingDay(Parameter, TempGenFromDate, TempGenToDate);
                                        if (TempGenFromDate == FFromDate && TempGenToDate == ToDate)
                                            TempAmount = Convert.ToDecimal(Allw.Amount);
                                        else
                                            TempAmount = Convert.ToDecimal((Allw.Amount / ActualWorkDay) * TempWorkDay);
                                        Salary += TempAmount;
                                    }
                                }
                                #endregion


                                TempAmount = 0;
                                var EmpDed = DB.PREmpDeducs.Where(w => w.EmpCode == Code && w.FromDate.Value.Year == ToDate.Year && w.FromDate.Value.Month == ToDate.Month &&
                                   w.ToDate.Value.Year == ToDate.Year && w.ToDate.Value.Month == ToDate.Month && w.Status == 1).ToList();

                                Salary += Convert.ToDecimal(EmpDed.Sum(w => w.Amount));

                                #region ********Deducs Period ********
                                var EmpDedP = DB.PREmpDeducs.Where(w => w.EmpCode == Code && ((FFromDate >= w.FromDate && FFromDate <= w.ToDate) || (ToDate >= w.FromDate && ToDate <= w.ToDate))
                                && w.Status == 0).ToList();
                                foreach (var Ded in EmpDedP)
                                {
                                    var ActualWorkDay = Get_WorkingDay_Ded(Parameter, FFromDate, ToDate);
                                    DateTime TempGenFromDate = new DateTime();
                                    DateTime TempGenToDate = new DateTime();
                                    int HasAllowanceDetail = 0;
                                    if (FFromDate >= Ded.FromDate && ToDate <= Ded.ToDate)
                                    {
                                        TempGenFromDate = FFromDate;
                                        TempGenToDate = ToDate;
                                        HasAllowanceDetail = 1;
                                    }
                                    else if (FFromDate >= Ded.FromDate && Ded.ToDate <= ToDate)
                                    {
                                        TempGenFromDate = FFromDate;
                                        TempGenToDate = Ded.ToDate.Value;
                                        HasAllowanceDetail = 1;
                                    }
                                    else if (Ded.FromDate >= FFromDate && Ded.FromDate <= ToDate && Ded.ToDate >= ToDate)
                                    {
                                        TempGenFromDate = Ded.FromDate.Value;
                                        TempGenToDate = ToDate;
                                        HasAllowanceDetail = 1;
                                    }
                                    else if (Ded.FromDate >= FFromDate && Ded.ToDate <= ToDate)
                                    {
                                        TempGenFromDate = Ded.FromDate.Value;
                                        TempGenToDate = Ded.ToDate.Value;
                                        HasAllowanceDetail = 1;
                                    }
                                    if (HasAllowanceDetail == 1)
                                    {
                                        decimal TempWorkDay = Get_WorkingDay(Parameter, TempGenFromDate, TempGenToDate);
                                        if (TempGenFromDate == FFromDate && TempGenToDate == ToDate)
                                            TempAmount = Convert.ToDecimal(Ded.Amount);
                                        else
                                            TempAmount = Convert.ToDecimal((Ded.Amount / ActualWorkDay) * TempWorkDay);
                                        Salary += TempAmount;
                                    }
                                }
                                #endregion

                            }
                            #endregion

                            if (result.Count > 0)
                            {

                                if (Filter.SalaryType == "BS")
                                {
                                    AVGSalary = Convert.ToDecimal(result.Sum(x => x.Salary) / CMonth);
                                }
                                else if (Filter.SalaryType == "GP")
                                {
                                    var _Seniority = result.Where(w => w.INYear == ToDate.Year && w.INMonth == ToDate.Month).Sum(x => x.Seniority);

                                    AVGSalary = (Convert.ToDecimal(result.Sum(x => x.GrossPay) + Salary - _Seniority) / CMonth);
                                }
                                else
                                {
                                    AVGSalary = (Convert.ToDecimal(result.Sum(x => x.NetWage + x.FirstPaymentAmount) + Salary) / CMonth);
                                }
                                AVGSalary = ClsRounding.Rounding(AVGSalary, SYConstant.DECIMAL_PLACE, "N");
                                Rate = AVGSalary / DayInMonth;
                                Rate = ClsRounding.Rounding(Rate, SYConstant.DECIMAL_PLACE, "N");
                                Balance = Rate * _SPrate;
                            }
                            Seniority.InYear = Filter1.ToDate.Year;
                            Seniority.InMonth = Filter1.ToDate.Month;
                            Seniority.EmpCode = Code;
                            Seniority.FromDate = FromDate;
                            Seniority.ToDate = ToDate;
                            Seniority.Salary = AVGSalary;
                            Seniority.Rate = Rate;
                            Seniority.TotalBalance = ClsRounding.Rounding(Balance, SYConstant.DECIMAL_PLACE, "N");
                            DB.PRSincerities.Add(Seniority);
                        }
                    }
                    DB.SaveChanges();
                    return SYConstant.OK;
                }
                finally
                {
                    DB.Configuration.AutoDetectChangesEnabled = true;
                }
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ID;
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
                log.DocurmentAction = ID;
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
                log.DocurmentAction = ID;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string Generate_Severance(string EmpCode, FTFilterEmployee Filter1)
        {
            try
            {
                try
                {
                    DB = new HumicaDBContext();
                    DB.Configuration.AutoDetectChangesEnabled = false;
                    DateTime FromDate = new DateTime(Filter1.FromDate.Year, Filter1.FromDate.Month, 1);
                    DateTime ToDate = new DateTime(Filter1.ToDate.Year, Filter1.ToDate.Month, DateTime.DaysInMonth(Filter1.ToDate.Year, Filter1.ToDate.Month));
                    var Temp = DB.PRSeverancePays.ToList();
                    if (Temp.Count() > 0)
                    {
                        foreach (var item in Temp)
                        {
                            DB.PRSeverancePays.Attach(item);
                            DB.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                            var rowD = DB.SaveChanges();
                        }
                    }
                    string[] Emp = EmpCode.Split(';');
                    var ListSalary = DB.HISGenSalaries.ToList();
                    var ListStaff = DB.HRStaffProfiles.ToList();
                    var listParameter = DB.PRParameters.ToList();
                    var ListEmpCareer = DB.HREmpCareers.ToList();
                    ListStaff = ListStaff.ToList();
                    listParameter = listParameter.ToList();
                    ListSalary = ListSalary.Where(w => w.ToDate.Value.Date >= FromDate.Date && w.ToDate.Value.Date <= ToDate.Date).ToList();
                    int CMonth = GetMonthDifference(FromDate, ToDate) + 1;

                    var _StaffPro = ListStaff.Where(w => w.Probation.Value.Date >= FromDate.Date && w.Probation.Value.Date <= ToDate.Date).ToList();
                    decimal RateInDate = 1.25M;
                    foreach (var Code in Emp)
                    {
                        if (Code.Trim() != "")
                        {
                            var Seniority = new PRSincerity();
                            var Staff = ListStaff.FirstOrDefault(w => w.EmpCode == Code);
                            var Parameter = listParameter.FirstOrDefault(w => w.Code == Staff.PayParam);
                            DateTime FFromDate = GetFromDate(Parameter, Filter1.ToDate);
                            DateTime FToDate = GetToDate(Parameter, Filter1.ToDate);

                            decimal DayInMonth = Get_WorkingDay_Salary(Parameter, FFromDate, FToDate);
                            decimal AVGSalary = 0;
                            decimal Rate = 0;
                            decimal Balance = 0;
                            var result = ListSalary.Where(w => w.EmpCode == Code).ToList();

                            //Probation
                            if (_StaffPro.Where(w => w.EmpCode == Staff.EmpCode).Any())
                            {
                                var _result = result.Where(w => w.INYear == Staff.Probation.Value.Year &&
                                  w.INMonth == Staff.Probation.Value.Month).ToList();
                                DateTime Prob = Staff.Probation.Value;
                                DateTime _ToDate = new DateTime(Prob.Year, Prob.Month, DateTime.DaysInMonth(Prob.Year, Prob.Month));
                                decimal _DayInMonth = Get_WorkingDay(Parameter, Prob, _ToDate);
                                if (_DayInMonth >= 21)
                                {
                                    result = result.Where(w => w.ToDate.Value.Date >= _ToDate.Date).ToList();
                                }
                                else
                                {
                                    result = result.Where(w => w.FromDate.Value.Date >= _ToDate.Date).ToList();
                                }
                                CMonth = result.Count;
                                Filter1.Rate = RateInDate * CMonth;
                            }

                            #region "Resgin"
                            var LstEmpAllow = DB.PREmpAllws.ToList();
                            var LstEmpBon = DB.PREmpBonus.ToList();
                            decimal TempSalary = 0;
                            decimal Salary = 0;
                            decimal TempAmount = 0;
                            var _listGenD = new List<HISGenSalaryD>();
                            var LstEmpCareer = ListEmpCareer.Where(w => w.EmpCode == Code).ToList();
                            if (Staff.TerminateStatus == "TERMINAT" && Staff.SalaryFlag == false
                                && Staff.DateTerminate.Year == Filter1.ToDate.Year && Staff.DateTerminate.Month == Filter1.ToDate.Month)
                            {
                                //
                                result = result.Where(w => w.ToDate.Value.Date != ToDate.Date).ToList();

                                var Emp_C = from C in LstEmpCareer
                                            where ((C.FromDate >= FFromDate && C.FromDate <= ToDate) || (C.ToDate >= FFromDate && C.ToDate <= ToDate) ||
                                     (FFromDate >= C.FromDate && FFromDate <= C.ToDate) || (ToDate >= C.FromDate && ToDate <= C.ToDate))
                                     && C.EmpCode == Code && C.CareerCode != "TERMINAT"
                                            select new { C.CareerCode, C.EmpCode, C.NewSalary, C.FromDate, C.ToDate };
                                int C_Career = Emp_C.Count();
                                foreach (var emp in Emp_C)
                                {
                                    DateTime PToDate = emp.ToDate.Value;
                                    DateTime PFromDate = emp.FromDate.Value;
                                    if (PToDate > ToDate) PToDate = ToDate;
                                    if (PFromDate < FFromDate) PFromDate = FFromDate;
                                    Decimal ActualWorkDay = Get_WorkingDay(Parameter, PFromDate, PToDate);

                                    if (PFromDate == FFromDate && PToDate == ToDate)
                                    {
                                        TempSalary = emp.NewSalary;
                                    }
                                    else
                                    {
                                        decimal TMPD = 0;
                                        TMPD = Convert.ToDecimal(_listGenD.Sum(x => x.ActWorkDay));
                                        if ((TMPD + ActualWorkDay) > DayInMonth) ActualWorkDay = Convert.ToDecimal(DayInMonth) - TMPD;
                                        Rate = Convert.ToDecimal(emp.NewSalary / DayInMonth);
                                        TempSalary = Math.Round((Rate * ActualWorkDay), 2);
                                    }
                                    Salary += TempSalary;
                                }

                                var EmpAllw = LstEmpAllow.Where(w => w.EmpCode == Code && w.FromDate.Value.Year == ToDate.Year && w.FromDate.Value.Month == ToDate.Month &&
                                   w.ToDate.Value.Year == ToDate.Year && w.ToDate.Value.Month == ToDate.Month && w.Status == 1).ToList();
                                Salary += Convert.ToDecimal(EmpAllw.Where(w => w.AllwCode != "SP").Sum(w => w.Amount));

                                #region ********Allw Period********
                                var EmpAllwP = LstEmpAllow.Where(w => w.EmpCode == Code && ((FFromDate >= w.FromDate && FFromDate <= w.ToDate) || (ToDate >= w.FromDate && ToDate <= w.ToDate))
                                && w.Status == 0).ToList();
                                foreach (var Allw in EmpAllwP.Where(w => w.AllwCode != "SP"))
                                {
                                    var ActualWorkDay = Get_WorkingDay_Allw(Parameter, FFromDate, ToDate);
                                    DateTime TempGenFromDate = new DateTime();
                                    DateTime TempGenToDate = new DateTime();
                                    int HasAllowanceDetail = 0;
                                    if (FFromDate >= Allw.FromDate && ToDate <= Allw.ToDate)
                                    {
                                        TempGenFromDate = FFromDate;
                                        TempGenToDate = ToDate;
                                        HasAllowanceDetail = 1;
                                    }
                                    else if (FFromDate >= Allw.FromDate && Allw.ToDate <= ToDate)
                                    {
                                        TempGenFromDate = FFromDate;
                                        TempGenToDate = Allw.ToDate.Value;
                                        HasAllowanceDetail = 1;
                                    }
                                    else if (Allw.FromDate >= FFromDate && Allw.FromDate <= ToDate && Allw.ToDate >= ToDate)
                                    {
                                        TempGenFromDate = Allw.FromDate.Value;
                                        TempGenToDate = ToDate;
                                        HasAllowanceDetail = 1;
                                    }
                                    else if (Allw.FromDate >= FFromDate && Allw.ToDate <= ToDate)
                                    {
                                        TempGenFromDate = Allw.FromDate.Value;
                                        TempGenToDate = Allw.ToDate.Value;
                                        HasAllowanceDetail = 1;
                                    }
                                    if (HasAllowanceDetail == 1)
                                    {
                                        decimal TempWorkDay = Get_WorkingDay(Parameter, TempGenFromDate, TempGenToDate);
                                        if (TempGenFromDate == FFromDate && TempGenToDate == ToDate)
                                            TempAmount = Convert.ToDecimal(Allw.Amount);
                                        else
                                            TempAmount = Convert.ToDecimal((Allw.Amount / ActualWorkDay) * TempWorkDay);
                                    }
                                    Salary += TempAmount;
                                }
                                #endregion

                                TempAmount = 0;
                                var EmpBon = LstEmpBon.Where(w => w.EmpCode == Code && w.FromDate.Value.Year == ToDate.Year && w.FromDate.Value.Month == ToDate.Month &&
                                   w.ToDate.Value.Year == ToDate.Year && w.ToDate.Value.Month == ToDate.Month && w.Status == 1).ToList();
                                Salary += EmpBon.Sum(w => w.Amount);

                                #region ********Bon Period********
                                var EmpBonP = LstEmpBon.Where(w => w.EmpCode == Code && ((FFromDate >= w.FromDate && FFromDate <= w.ToDate) || (ToDate >= w.FromDate && ToDate <= w.ToDate))
                                && w.Status == 0).ToList();
                                foreach (var Allw in EmpBonP)
                                {
                                    var ActualWorkDay = Get_WorkingDay_Allw(Parameter, FFromDate, ToDate);
                                    DateTime TempGenFromDate = new DateTime();
                                    DateTime TempGenToDate = new DateTime();
                                    int HasAllowanceDetail = 0;
                                    if (FFromDate >= Allw.FromDate && ToDate <= Allw.ToDate)
                                    {
                                        TempGenFromDate = FFromDate;
                                        TempGenToDate = ToDate;
                                        HasAllowanceDetail = 1;
                                    }
                                    else if (FFromDate >= Allw.FromDate && Allw.ToDate <= ToDate)
                                    {
                                        TempGenFromDate = FFromDate;
                                        TempGenToDate = Allw.ToDate.Value;
                                        HasAllowanceDetail = 1;
                                    }
                                    else if (Allw.FromDate >= FFromDate && Allw.FromDate <= ToDate && Allw.ToDate >= ToDate)
                                    {
                                        TempGenFromDate = Allw.FromDate.Value;
                                        TempGenToDate = ToDate;
                                        HasAllowanceDetail = 1;
                                    }
                                    else if (Allw.FromDate >= FFromDate && Allw.ToDate <= ToDate)
                                    {
                                        TempGenFromDate = Allw.FromDate.Value;
                                        TempGenToDate = Allw.ToDate.Value;
                                        HasAllowanceDetail = 1;
                                    }
                                    if (HasAllowanceDetail == 1)
                                    {
                                        decimal TempWorkDay = Get_WorkingDay(Parameter, TempGenFromDate, TempGenToDate);
                                        if (TempGenFromDate == FFromDate && TempGenToDate == ToDate)
                                            TempAmount = Convert.ToDecimal(Allw.Amount);
                                        else
                                            TempAmount = Convert.ToDecimal((Allw.Amount / ActualWorkDay) * TempWorkDay);
                                        Salary += TempAmount;
                                    }
                                }
                                #endregion


                                TempAmount = 0;
                                var EmpDed = DB.PREmpDeducs.Where(w => w.EmpCode == Code && w.FromDate.Value.Year == ToDate.Year && w.FromDate.Value.Month == ToDate.Month &&
                                   w.ToDate.Value.Year == ToDate.Year && w.ToDate.Value.Month == ToDate.Month && w.Status == 1).ToList();

                                Salary += Convert.ToDecimal(EmpDed.Sum(w => w.Amount));

                                #region ********Deducs Period ********
                                var EmpDedP = DB.PREmpDeducs.Where(w => w.EmpCode == Code && ((FFromDate >= w.FromDate && FFromDate <= w.ToDate) || (ToDate >= w.FromDate && ToDate <= w.ToDate))
                                && w.Status == 0).ToList();
                                foreach (var Ded in EmpDedP)
                                {
                                    var ActualWorkDay = Get_WorkingDay_Ded(Parameter, FFromDate, ToDate);
                                    DateTime TempGenFromDate = new DateTime();
                                    DateTime TempGenToDate = new DateTime();
                                    int HasAllowanceDetail = 0;
                                    if (FFromDate >= Ded.FromDate && ToDate <= Ded.ToDate)
                                    {
                                        TempGenFromDate = FFromDate;
                                        TempGenToDate = ToDate;
                                        HasAllowanceDetail = 1;
                                    }
                                    else if (FFromDate >= Ded.FromDate && Ded.ToDate <= ToDate)
                                    {
                                        TempGenFromDate = FFromDate;
                                        TempGenToDate = Ded.ToDate.Value;
                                        HasAllowanceDetail = 1;
                                    }
                                    else if (Ded.FromDate >= FFromDate && Ded.FromDate <= ToDate && Ded.ToDate >= ToDate)
                                    {
                                        TempGenFromDate = Ded.FromDate.Value;
                                        TempGenToDate = ToDate;
                                        HasAllowanceDetail = 1;
                                    }
                                    else if (Ded.FromDate >= FFromDate && Ded.ToDate <= ToDate)
                                    {
                                        TempGenFromDate = Ded.FromDate.Value;
                                        TempGenToDate = Ded.ToDate.Value;
                                        HasAllowanceDetail = 1;
                                    }
                                    if (HasAllowanceDetail == 1)
                                    {
                                        decimal TempWorkDay = Get_WorkingDay(Parameter, TempGenFromDate, TempGenToDate);
                                        if (TempGenFromDate == FFromDate && TempGenToDate == ToDate)
                                            TempAmount = Convert.ToDecimal(Ded.Amount);
                                        else
                                            TempAmount = Convert.ToDecimal((Ded.Amount / ActualWorkDay) * TempWorkDay);
                                        Salary += TempAmount;
                                    }
                                }
                                #endregion

                            }
                            #endregion

                            if (result.Count > 0)
                            {

                                if (Filter.SalaryType == "BS")
                                {
                                    AVGSalary = Convert.ToDecimal(result.Sum(x => x.Salary) / CMonth);
                                }
                                else if (Filter.SalaryType == "GP")
                                {
                                    AVGSalary = (Convert.ToDecimal(result.Sum(x => x.GrossPay) + Salary) / CMonth);
                                }
                                else
                                {
                                    AVGSalary = (Convert.ToDecimal(result.Sum(x => x.NetWage) + Salary) / CMonth); ;
                                }
                                AVGSalary = Math.Round(AVGSalary, 2);
                                Rate = AVGSalary / DayInMonth;
                                Rate = Math.Round(Rate, 2);
                                Balance = Rate * Filter1.Rate;
                            }
                            Seniority.InYear = Filter1.ToDate.Year;
                            Seniority.InMonth = Filter1.ToDate.Month;
                            Seniority.EmpCode = Code;
                            Seniority.FromDate = FromDate;
                            Seniority.ToDate = ToDate;
                            Seniority.Salary = AVGSalary;
                            Seniority.Rate = Rate;
                            Seniority.TotalBalance = Balance;
                            DB.PRSincerities.Add(Seniority);
                        }
                    }
                    DB.SaveChanges();
                    return SYConstant.OK;
                }
                finally
                {
                    DB.Configuration.AutoDetectChangesEnabled = true;
                }
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
        public string TransferSenority(string EmpCode, DateTime InMonth)
        {
            try
            {
                SYHRSetting Setting = DB.SYHRSettings.FirstOrDefault();
                if (Setting == null)
                    return "INVALTID_ALLOWANCE";
                var RewardType = DB.PR_RewardsType.Where(w => w.ReCode == "ALLW").ToList();
                var AllwType = RewardType.FirstOrDefault(w => w.Code == Setting.SeniorityType);
                if (AllwType == null)
                {
                    return "INVALTID_ALLOWANCE";
                }

                var DBI = new HumicaDBContext();
                var ListEmpAllw = DB.PREmpAllws.ToList();
                var ID = ListEmpAllw.OrderByDescending(u => u.TranNo).FirstOrDefault();
                string[] _TranNo = EmpCode.Split(';');
                DateTime FromDate = new DateTime(InMonth.Year, InMonth.Month, 1);
                DateTime ToDate = new DateTime(InMonth.Year, InMonth.Month, DateTime.DaysInMonth(InMonth.Year, InMonth.Month));
                //var TempAllw = DB.PRSincerities.ToList();
                //if (TempAllw.Count > 0)
                //{
                foreach (var Code in _TranNo)
                {
                    //var _TBA = TempAllw.Where(w => w.EmpCode == Code).ToList();
                    var ObjUpdate = ListEmpSeniority.FirstOrDefault(w => w.EmpCode == Code);
                    if (ObjUpdate != null)
                    {
                        //var ObjUpdate = _TBA.First();
                        var _empAllw = ListEmpAllw.FirstOrDefault(w => w.EmpCode == Code && w.AllwCode == AllwType.Code &&
           ((FromDate.Date >= w.FromDate.Value.Date && FromDate.Date <= w.ToDate.Value.Date) || (ToDate.Date >= w.FromDate.Value.Date && ToDate.Date <= w.ToDate.Value.Date)));

                        if (_empAllw != null)
                        {
                            DBI.PREmpAllws.Attach(_empAllw);
                            DBI.Entry(_empAllw).State = System.Data.Entity.EntityState.Deleted;
                        }
                        if (ObjUpdate.TotalAmount > 0)
                        {
                            var Header = new PREmpAllw();
                            Header.TranNo = ID.TranNo + 1;
                            Header.EmpCode = Code;
                            Header.AllwCode = AllwType.Code;
                            Header.AllwDescription = AllwType.Description;
                            Header.FromDate = FromDate;
                            Header.ToDate = ToDate;
                            Header.Remark = "Transfer from system";
                            Header.Amount = Convert.ToDecimal(ObjUpdate.TotalAmount);
                            Header.Status = 1;

                            //ObjUpdate.Remark = "Transfer on " + DateTime.Now.ToString("dd.MM.yyyy");

                            DB.PREmpAllws.Add(Header);
                            //DB.PRSincerities.Attach(ObjUpdate);
                            //DB.Entry(ObjUpdate).Property(w => w.Remark).IsModified = true;
                        }
                    }
                }
                //}
                var rowD = DBI.SaveChanges();
                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Header.EmpCode;
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
                log.DocurmentAction = Header.EmpCode;
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
                log.DocurmentAction = Header.EmpCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public decimal Get_WorkingDay(PRParameter PayPram, DateTime FromDate, DateTime ToDate)
        {
            decimal? Result = 0;
            DateTime TempDate = new DateTime();
            if (PayPram != null)
            {
                int Count = 0;
                //for (int i = FromDate.Day; i <= ToDate.Day; i++)
                for (var day = FromDate.Date; day.Date <= ToDate.Date; day = day.AddDays(1))
                {
                    //TempDate = FromDate.AddDays(Count);
                    TempDate = day;
                    if (PayPram.WDMON == true && TempDate.DayOfWeek == DayOfWeek.Monday) Result += PayPram.WDMONDay;
                    else if (PayPram.WDTUE == true && TempDate.DayOfWeek == DayOfWeek.Tuesday) Result += PayPram.WDTUEDay;
                    else if (PayPram.WDWED == true && TempDate.DayOfWeek == DayOfWeek.Wednesday) Result += PayPram.WDWEDDay;
                    else if (PayPram.WDTHU == true && TempDate.DayOfWeek == DayOfWeek.Thursday) Result += PayPram.WDTHUDay;
                    else if (PayPram.WDFRI == true && TempDate.DayOfWeek == DayOfWeek.Friday) Result += PayPram.WDFRIDay;
                    else if (PayPram.WDSAT == true && TempDate.DayOfWeek == DayOfWeek.Saturday) Result += PayPram.WDSATDay;
                    else if (PayPram.WDSUN == true && TempDate.DayOfWeek == DayOfWeek.Sunday) Result += PayPram.WDSUNDay;
                    Count++;
                }
            }
            return Convert.ToDecimal(Result);
        }
        public decimal Get_WorkingDay_Salary(PRParameter PayPram, DateTime FromDate, DateTime ToDate)
        {
            decimal Result = 0;
            if (PayPram.SALWKTYPE == 1)
                Result = Get_WorkingDay(PayPram, FromDate, ToDate);
            else if (PayPram.SALWKTYPE == 2)
                Result = ToDate.Subtract(FromDate).Days + 1;
            else if (PayPram.SALWKTYPE == 3)
                Result = Convert.ToInt32(PayPram.SALWKVAL);
            return Result;
        }
        public decimal Get_WorkingDay_OT(PRParameter PayPram, DateTime FromDate, DateTime ToDate)
        {
            decimal Result = 0;
            if (PayPram.OTWKTYPE == 1)
                Result = Get_WorkingDay(PayPram, FromDate, ToDate);
            else if (PayPram.OTWKTYPE == 2)
                Result = ToDate.Subtract(FromDate).Days + 1;
            else if (PayPram.OTWKTYPE == 3)
                Result = Convert.ToDecimal(PayPram.OTWKVAL);
            return Result;
        }
        public decimal Get_WorkingDay_Allw(PRParameter PayPram, DateTime FromDate, DateTime ToDate)
        {
            decimal Result = 0;
            if (PayPram.ALWTYPE == 1)
                Result = Get_WorkingDay(PayPram, FromDate, ToDate);
            else if (PayPram.ALWTYPE == 2)
                Result = ToDate.Subtract(FromDate).Days + 1;
            else if (PayPram.ALWTYPE == 3)
                Result = Convert.ToDecimal(PayPram.ALWVAL);
            return Result;
        }
        public decimal Get_WorkingDay_Ded(PRParameter PayPram, DateTime FromDate, DateTime ToDate)
        {
            decimal Result = 0;
            if (PayPram.DEDTYPE == 1)
                Result = Get_WorkingDay(PayPram, FromDate, ToDate);
            else if (PayPram.DEDTYPE == 2)
                Result = ToDate.Subtract(FromDate).Days + 1;
            else if (PayPram.DEDTYPE == 3)
                Result = Convert.ToInt32(PayPram.DEDVAL);
            return Result;
        }
        public DateTime GetFromDate(PRParameter Parameter, DateTime InDate)
        {
            DateTime FromDate = InDate.Date;
            DateTime tempFromDate = FromDate;
            DateTime ToDate = DateTimeHelper.DateInMonth(InDate);

            if (!Parameter.IsPrevoiuseMonth.IsNullOrZero())
            {
                DateTime tempDate = tempFromDate.AddMonths(-1);
                FromDate = new DateTime(tempDate.Year, tempDate.Month, Parameter.FROMDATE.Value().Day);
                ToDate = new DateTime(ToDate.Year, ToDate.Month, Parameter.TODATE.Value().Day);
            }

            DateTime Result = new DateTime();
            if (FromDate.Year != ToDate.Year)
            {
                if (FromDate.Month > ToDate.Month)
                {
                    int Month = InDate.Month - 1;
                    int Year = InDate.Year;
                    if (Month == 0)
                    {
                        Month = 12;
                        Year -= 1;
                    }
                    Result = new DateTime(Year, Month, FromDate.Day);
                }
            }
            else if (FromDate.Month < ToDate.Month)
            {
                int Month = InDate.Month - 1;
                int Year = InDate.Year;
                if (Month == 0)
                {
                    Month = 12;
                    Year -= 1;
                }
                Result = new DateTime(Year, Month, FromDate.Day);
            }

            else Result = new DateTime(InDate.Year, InDate.Month, FromDate.Day);
            return Result;
        }
        public DateTime GetToDate(PRParameter Parameter, DateTime InDate)
        {
            DateTime Result = new DateTime();
            DateTime ToDate = DateTimeHelper.DateInMonth(InDate);
            if (!Parameter.IsPrevoiuseMonth.IsNullOrZero())
            {
                ToDate = new DateTime(ToDate.Year, ToDate.Month, Parameter.TODATE.Value().Day);
            }
            Result = ToDate;
            return Result;
        }
        public int Get_LineTranNo(string EmpCode, int InYear, int InMonth, int Status)
        {
            int Result = 0;
            if (Status == 1)
            {
                var PaySlips = ListPaySlip.Where(w => w.EmpCode == EmpCode && w.INYear == InYear && w.INMonth == InMonth && w.EarnDesc == "EARNING").ToList();
                if (PaySlips == null) Result = 0;
                else
                    Result = Convert.ToInt32(PaySlips.Min(w => w.TranLine));
            }
            else if (Status == 2)
            {
                var PaySlips = ListPaySlip.Where(w => w.EmpCode == EmpCode && w.INYear == InYear && w.INMonth == InMonth && w.DeductDesc == "DEDUCTIONS").ToList();
                if (PaySlips == null) Result = 0;
                else
                    Result = Convert.ToInt32(PaySlips.Min(w => w.TranLine));
            }
            return Result;
        }
        public static int GetMonthDifference(DateTime startDate, DateTime endDate)
        {
            int monthsApart = 12 * (startDate.Year - endDate.Year) + startDate.Month - endDate.Month;
            return Math.Abs(monthsApart);
        }
        private decimal CambodiaTaxDeduction(decimal income, List<PRTaxSetting> list, ref decimal? _rate, bool isForeigner = false)
        {
            if (isForeigner) return income * 0.2m;
            decimal total_deduct = 0.0m;
            _rate = 0;
            foreach (var rule in list)
            {
                if (income > rule.TaxFrom && income <= rule.TaxTo)
                {
                    _rate = rule.TaxPercent;
                    total_deduct = (income) * (rule.TaxPercent.Value / 100) - rule.Amdeduct.Value;
                    break;
                }
            }
            return total_deduct;
        }
        #region Approval Salary
        public string CreateAppSalary()
        {
            try
            {
                var DBR = new HumicaDBContext();
                var objCF = DB.ExCfWorkFlowItems.Find(ScreenId, HeaderAppSalary.DocumentType);
                if (objCF == null)
                {
                    return "REQUEST_TYPE_NE";
                }

                var objNumber = new CFNumberRank(HeaderAppSalary.DocumentType, ScreenId);
                if (objNumber == null)
                {
                    return "NUMBER_RANK_NE";
                }
                HeaderAppSalary.ASNumber = objNumber.NextNumberRank.Trim();
                HeaderAppSalary.Status = SYDocumentStatus.OPEN.ToString(); ;
                //Add approver
                foreach (var read in ListApproval)
                {
                    read.ID = 0;
                    read.LastChangedDate = DateTime.Now;
                    read.DocumentNo = HeaderAppSalary.ASNumber;
                    read.DocumentType = HeaderAppSalary.DocumentType;
                    read.Status = SYDocumentStatus.OPEN.ToString();
                    read.WFObject = objCF.ApprovalFlow;
                    read.ApprovedBy = "";
                    read.ApprovedName = "";
                    read.ApproverName = "";
                    var objStaff = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == read.Approver);
                    if (objStaff != null)
                    {
                        read.ApproverName = objStaff.AllName;
                    }
                    DB.ExDocApprovals.Add(read);
                }
                var objUpdate = DBR.HisPendingAppSalaries.FirstOrDefault(w => w.InYear == HeaderAppSalary.PayInMonth.Year && w.InMonth == HeaderAppSalary.PayInMonth.Month);
                if (objUpdate != null)
                {
                    objUpdate.IsLock = true;
                    DB.HisPendingAppSalaries.Attach(objUpdate);
                    DB.Entry(objUpdate).Property(w => w.IsLock).IsModified = true;
                }
                HeaderAppSalary.InYear = objUpdate.InYear;
                HeaderAppSalary.InMonth = objUpdate.InMonth;
                HeaderAppSalary.CreatedOn = DateTime.Now;
                HeaderAppSalary.CreatedBy = User.UserName;

                DB.HISApproveSalaries.Add(HeaderAppSalary);
                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = HeaderAppSalary.ASNumber;
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
                log.DocurmentAction = HeaderAppSalary.ASNumber;
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
                log.DocurmentAction = HeaderAppSalary.ASNumber;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string requestToApprove(string id)
        {
            try
            {
                HumicaDBContext DBX = new HumicaDBContext();
                var objMatch = DB.HISApproveSalaries.Find(id);
                if (objMatch == null)
                {
                    return "REQUEST_NE";
                }
                HeaderAppSalary = objMatch;

                if (objMatch.Status != SYDocumentStatus.OPEN.ToString())
                {
                    return "INV_DOC";
                }

                objMatch.Status = SYDocumentStatus.PENDING.ToString();
                DB.HISApproveSalaries.Attach(objMatch);
                DB.Entry(objMatch).Property(w => w.Status).IsModified = true;
                DB.SaveChanges();

                int row = DBX.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.ScreenId = HeaderAppSalary.ASNumber;
                log.Action = SYActionBehavior.ADD.ToString();

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
                log.DocurmentAction = HeaderAppSalary.ASNumber;
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
                log.DocurmentAction = HeaderAppSalary.ASNumber;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string approveTheDoc(string id)
        {
            try
            {
                HumicaDBContext DBX = new HumicaDBContext();
                var objMatch = DB.HISApproveSalaries.FirstOrDefault(w => w.ASNumber == id);
                if (objMatch == null)
                {
                    return "REQUEST_NE";
                }
                HeaderAppSalary = objMatch;

                if (objMatch.Status != SYDocumentStatus.PENDING.ToString())
                {
                    return "INV_DOC";
                }
                string open = SYDocumentStatus.OPEN.ToString();
                var listApproval = DBX.ExDocApprovals.Where(w => w.DocumentType == objMatch.DocumentType
                                    && w.DocumentNo == objMatch.ASNumber && w.Status == open).OrderBy(w => w.ApproveLevel).ToList();
                var listUser = DB.HRStaffProfiles.Where(w => w.EmpCode == User.UserName).ToList();
                var b = false;
                foreach (var read in listApproval)
                {
                    var checklist = listUser.Where(w => w.EmpCode == read.Approver).ToList();
                    if (checklist.Count > 0)
                    {
                        if (read.Status == SYDocumentStatus.APPROVED.ToString())
                        {
                            return "USER_ALREADY_APP";
                        }

                        if (read.ApproveLevel > listApproval.Min(w => w.ApproveLevel))
                        {
                            return "REQUIRED_PRE_LEVEL";
                        }
                        var objStaff = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == read.Approver);
                        if (objStaff != null)
                        {
                            ////New
                            //if (listApproval.Where(w => w.ApproveLevel <= read.ApproveLevel).Count() >= listApproval.Count())
                            //{
                            //    listApproval.ForEach(w => w.Status = SYDocumentStatus.APPROVED.ToString());
                            //}
                            //StaffProfile = objStaff;
                            read.ApprovedBy = objStaff.EmpCode;
                            read.ApprovedName = objStaff.AllName;
                            read.LastChangedDate = DateTime.Now.Date;
                            read.ApprovedDate = DateTime.Now;
                            read.Status = SYDocumentStatus.APPROVED.ToString();
                            DBX.ExDocApprovals.Attach(read);
                            DBX.Entry(read).State = System.Data.Entity.EntityState.Modified;
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
                    // objMatch.IsApproved = false;
                }

                objMatch.Status = status;

                DB.Entry(objMatch).Property(w => w.Status).IsModified = true;
                DB.SaveChanges();

                int row = DBX.SaveChanges();

                #region Send Email
                var excfObject = DB.ExCfWorkFlowItems.Find(ScreenId, objMatch.DocumentType);
                if (excfObject != null)
                {
                    SYWorkFlowEmailObject wfo =
                           new SYWorkFlowEmailObject(excfObject.ApprovalFlow, WorkFlowType.APPROVER,
                                UserType.N, SYDocumentStatus.APPROVED.ToString());
                    wfo.SelectListItem = new SYSplitItem(Convert.ToString(id));
                    wfo.User = User;
                    wfo.BS = BS;
                    wfo.ScreenId = ScreenId;
                    wfo.Module = "HR";// CModule.PURCHASE.ToString();
                    wfo.ListLineRef = new List<BSWorkAssign>();
                    wfo.Action = SYDocumentStatus.PENDING.ToString();
                    wfo.ObjectDictionary = HeaderAppSalary;
                    wfo.ListObjectDictionary = new List<object>();
                    wfo.ListObjectDictionary.Add(objMatch);
                    HRStaffProfile Staff = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == objMatch.Requestor);
                    wfo.ListObjectDictionary.Add(Staff);
                    WorkFlowResult result1 = wfo.InsertProcessWorkFlow(Staff);
                    MessageCode = wfo.getErrorMessage(result1);
                }
                #endregion
                //#region *****Send to Telegram
                //var _Objmatch = DB.HR_PR_VIEW.FirstOrDefault(w => w.RequestNumber == objMatch.RequestNumber);

                //var excfObject = DB.ExCfWorkFlowItems.Find(ScreenId, objMatch.DocumentType);
                //Humica.Core.SY.SYSendTelegramObject wfo =
                //    new Humica.Core.SY.SYSendTelegramObject(excfObject.ApprovalFlow, WorkFlowType.APPROVER, objMatch.Status);
                //wfo.User = User;
                //wfo.ListObjectDictionary = new List<object>();
                //wfo.ListObjectDictionary.Add(_Objmatch);
                //wfo.ListObjectDictionary.Add(StaffProfile);
                //wfo.Send_SMS_Telegram(excfObject.Telegram, "");
                //#endregion

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.ScreenId = HeaderAppSalary.ASNumber;
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
                log.DocurmentAction = HeaderAppSalary.ASNumber;
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
                log.DocurmentAction = HeaderAppSalary.ASNumber;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string CancelAppSalary(string ASNumber)
        {
            try
            {
                string approved = SYDocumentStatus.CANCELLED.ToString();
                var objmatch = DB.HISApproveSalaries.FirstOrDefault(w => w.ASNumber == ASNumber);
                var _obj = DB.ExDocApprovals.Where(x => x.DocumentNo == ASNumber);
                if (objmatch == null)
                {
                    return "INV_EN";
                }
                if (objmatch.IsPostGL == true)
                {
                    return "DOC_POST_GL";
                }
                var objUpdate = DB.HisPendingAppSalaries.FirstOrDefault(w => w.InYear == objmatch.InYear && w.InMonth == objmatch.InMonth);
                if (objUpdate != null)
                {
                    objUpdate.IsLock = false;
                    DB.HisPendingAppSalaries.Attach(objUpdate);
                    DB.Entry(objUpdate).Property(w => w.IsLock).IsModified = true;
                    foreach (var read in _obj)
                    {
                        read.Status = approved;
                        read.LastChangedDate = DateTime.Now;
                        DB.Entry(read).Property(w => w.Status).IsModified = true;
                        DB.Entry(read).Property(w => w.LastChangedDate).IsModified = true;
                    }
                }
                objmatch.Status = approved;
                objmatch.ChangedOn = DateTime.Now;
                objmatch.ChangedBy = User.UserName;
                DB.HISApproveSalaries.Attach(objmatch);
                DB.Entry(objmatch).Property(w => w.ChangedOn).IsModified = true;
                DB.Entry(objmatch).Property(w => w.ChangedBy).IsModified = true;
                DB.Entry(objmatch).Property(w => w.Status).IsModified = true;
                DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ASNumber;
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
                log.DocurmentAction = ASNumber;
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
                log.DocurmentAction = ASNumber;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        #endregion
        public void SwapValueBenCharge(HISGLBenCharge GLBanChar, GEN_Filter _filter, PRGLmapping item,int LineItem,bool? IsPO ,bool? IsCredit = false)
        {
            DateTime DateNoW = DateTime.Now;
            GLBanChar.CompanyCode = _filter.CompanyCode.CompanyCode;
            GLBanChar.LineItem = LineItem;
            GLBanChar.CurrencyCode = _filter.CompanyCode.BaseCurrency;
            GLBanChar.Branch = _filter.Staff.Branch;
            GLBanChar.CostCenter = item.Branch;
            GLBanChar.Warehouse = item.Warehouse;
            GLBanChar.Project = item.Project;
            GLBanChar.EmpCode = _filter.Staff.EmpCode;
            GLBanChar.InMonth = _filter.InMonth;
            GLBanChar.InYear = _filter.InYear;
            GLBanChar.PaymentDate = _filter.ToDate;
            GLBanChar.PostPeriod = _filter.ToDate.ToString("MM-yyyy");
            GLBanChar.CreateBy = User.UserName;
            GLBanChar.CreateOn = DateNoW;
            GLBanChar.GRPBen = item.BenGrp;
            GLBanChar.BenCode = item.BenCode;
            GLBanChar.GLCode = item.GLCode;
            GLBanChar.MaterialCode = item.MaterialCode;
            GLBanChar.IsPO = IsPO;
            
            if (IsCredit != true) GLBanChar.IsCredit = false;
            else
                GLBanChar.IsCredit = IsCredit;
        }
        public void GetDataSenior(EmpSeniority Senior,DateTime InMonth, decimal Salary)
        {
            if (InMonth.Month == 1)
                Senior.Jan = Salary;
            else if (InMonth.Month == 2)
                Senior.Feb = Salary;
            else if (InMonth.Month == 3)
                Senior.Mar = Salary;
            else if (InMonth.Month == 4)
                Senior.Apr = Salary;
            else if (InMonth.Month == 5)
                Senior.May = Salary;
            else if (InMonth.Month == 6)
                Senior.Jun = Salary;
            else if (InMonth.Month == 7)
                Senior.Jul = Salary;
            else if (InMonth.Month == 8)
                Senior.Aug = Salary;
            else if (InMonth.Month == 9)
                Senior.Sep = Salary;
            else if (InMonth.Month == 10)
                Senior.Oct = Salary;
            else if (InMonth.Month == 11)
                Senior.Nov = Salary;
            else if (InMonth.Month == 12)
                Senior.Dec = Salary;
        }
        public void GetDataIsMonth(DateTime FromDate, DateTime ToDate)
        {
            InYear = ToDate.Year;
            for (DateTime date = FromDate; date <= ToDate; date = date.AddMonths(1))
            {
                if (date.Month == 1)
                    IsJan = true;
                else if (date.Month == 2)
                    IsFeb = true;
                else if (date.Month == 3)
                    IsMar = true;
                else if (date.Month == 4)
                    IsApr = true;
                else if (date.Month == 5)
                    IsMay = true;
                else if (date.Month == 6)
                    IsJun = true;
                else if (date.Month == 7)
                    IsJul = true;
                else if (date.Month == 8)
                    IsAug = true;
                else if (date.Month == 9)
                    IsSep = true;
                else if (date.Month == 10)
                    IsOct = true;
                else if (date.Month == 11)
                    IsNov = true;
                else if(date.Month == 12)
                    IsDec = true;
            }
        }
    }
    public class LeaveDeduction
    {
        public string LeaveCode { get; set; }
        public string LeaveDescription { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal DayLeave { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
    }
    public class EmpSeniority
    {
        public string EmpCode { get; set; }
        public string AllName { get; set; }
        public string Sex { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime Probation { get; set; }
        public decimal Salary { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Average { get; set; }
        public decimal? Balance { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Remark { get; set; }
        public decimal Jan { get; set; }
        public decimal Feb { get; set; }
        public decimal Mar { get; set; }
        public decimal Apr { get; set; }
        public decimal May { get; set; }
        public decimal Jun { get; set; }
        public decimal Jul { get; set; }
        public decimal Aug { get; set; }
        public decimal Sep { get; set; }
        public decimal Oct { get; set; }
        public decimal Nov { get; set; }
        public decimal Dec { get; set; }
    }
    public class GEN_Filter
    {
        public string EmpCode { get; set; }
        public int InYear { get; set; }
        public int InMonth { get; set; }
        public string Status { get; set; }
        public string PayType { get; set; }
        public string Default_Curremcy { get; set; }
        public string Round_UP { get; set; }
        public SYHRSetting Setting { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public SYHRCompany CompanyCode { get; set; }
        public HRStaffProfile Staff { get; set; }
        public PRParameter Parameter { get; set; }
        public HisPendingAppSalary HisAppSalary { get; set; }
        public HISGenSalary HisGensalarys { get; set; }
        public List<PREmpAllw> LstEmpAllow { get; set; }
        public List<PREmpBonu> LstEmpBon { get; set; }
        public List<HREmpCareer> LstEmpCareer { get; set; }
        public List<PR_RewardsType> LstRewardsType { get; set; }
        public List<PRBankFee> LstBankFee { get; set; }
        public List<ClsPayHis> LstPayHis { get; set; }
        public List<HISGenAllowance> ListHisEmpAllw { get; set; }
        public List<HISGenBonu> ListHisEmpBonu { get; set; }
        public List<HISGenDeduction> ListHisEmpDed { get; set; }
        public List<HISGenOTHour> ListHisEmpOT { get; set; }
        public List<HISGenLeaveDeduct> ListHisEmpLeave { get; set; }
        public List<PREmpHold> LstEmpHold { get; set; }
        public List<PRTaxSetting> ListTaxSetting { get; set; }
        public string NSSFSalaryType { get; set; }
    }
    public class ClsSelaryType
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public static List<ClsSelaryType> LoadData()
        {
            List<ClsSelaryType> _lst = new List<ClsSelaryType>();
            _lst.Add(new ClsSelaryType { Code = "BS", Description = "Basic Salary" });
            _lst.Add(new ClsSelaryType { Code = "NP", Description = "Net Pay" });
            _lst.Add(new ClsSelaryType { Code = "GP", Description = "Gross Pay" });
            return _lst;
        }
    }
    public class ClsPayHis
    {
        public string EmpCode { get; set; }
        public string SGROUP { get; set; }
        public string PayType { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }
}