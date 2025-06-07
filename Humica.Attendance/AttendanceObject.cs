using Humica.Core;
using Humica.Core.DB;
using Humica.Core.FT;
using Humica.Core.SY;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.EF.Repo;
using Humica.Logic;
using Humica.Logic.Att;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;

namespace Humica.Attendance
{
    public class AttendanceObject : IAttendanceObject
    {
        protected IUnitOfWork unitOfWork;
        public string ScreenId { get; set; }
        public string EmpID { get; set; }
        public int Progress { get; set; }
        public SYUser User { get; set; }
        public FTFilterAttenadance Attenadance { get; set; }
        public HRStaffProfile StaffProfile { get; set; }
        public ATEmpSchedule Header { get; set; }
        public List<VIEW_ATEmpSchedule> ListEmpSchdule { get; set; }
        public List<ATEmpSchedule> ListHeader { get; set; }
        public void OnLoad()
        {
            unitOfWork = new UnitOfWork<HumicaDBViewContext>(new HumicaDBViewContext());
        }

        public AttendanceObject()
        {
            User = SYSession.getSessionUser();
            OnLoad();
        }

        public void OnIndexLoading()
        {
            var ListAttendance = unitOfWork.Set<VIEW_ATEmpSchedule>().Where(w => DbFunctions.TruncateTime(w.TranDate) >= Attenadance.FromDate.Date &&
            DbFunctions.TruncateTime(w.TranDate) <= Attenadance.ToDate.Date &&
             (string.IsNullOrEmpty(Attenadance.EmpCode) || w.EmpCode == Attenadance.EmpCode)
             ).ToList();
            var ListBranch = SYConstant.getBranchDataAccess();
            ListEmpSchdule = ListAttendance.Where(w => ListBranch.Where(x => x.Code == w.Branch).Any()).ToList();
            ListEmpSchdule = ListEmpSchdule.Where(w => w.DateTerminate.Value.Year == 1900 || w.DateTerminate.Value.Date > Attenadance.FromDate.Date).ToList();
            if (Attenadance.Branch != null)
            {
                string[] Branch = Attenadance.Branch.Split(',');
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
            if (Attenadance.Locations != null)
                ListEmpSchdule = ListEmpSchdule.Where(w => w.LOCT == Attenadance.Locations).ToList();
            if (Attenadance.Department != null)
                ListEmpSchdule = ListEmpSchdule.Where(w => w.DEPT == Attenadance.Department).ToList();
            if (Attenadance.Division != null)
                ListEmpSchdule = ListEmpSchdule.Where(w => w.DivisionCode == Attenadance.Division).ToList();
            if (Attenadance.Shift != null)
                ListEmpSchdule = ListEmpSchdule.Where(w => w.SHIFT == Attenadance.Shift).ToList();
            ListEmpSchdule = ListEmpSchdule.OrderBy(x => x.TranDate).ToList();
            Progress = ListEmpSchdule.Count();
        }
        public void OnFilterStaff(string EmpCode)
        {
            StaffProfile = unitOfWork.Set<HRStaffProfile>().FirstOrDefault(w => w.EmpCode == EmpCode);
            if (StaffProfile == null)
            {
                StaffProfile = new HRStaffProfile();
            }
        }
        public string GenrateAttendance(string ID)
        {
            string Error_Status = "";
            try
            {
                unitOfWork.BeginTransaction();
                Header = new ATEmpSchedule();
                ListHeader = new List<ATEmpSchedule>();
                var ListInOut = new List<ATInOut>();
                var ListSHift = new List<ATShift>();
                var listLeaveH = new List<HREmpLeave>();
                var ListLeaveD = new List<HREmpLeaveD>();
                string Approval = SYDocumentStatus.APPROVED.ToString();
                var _LeaveType = unitOfWork.Set<HRLeaveType>().ToList();
                List<HRStaffProfile> ListStaff = unitOfWork.Set<HRStaffProfile>().ToList();
                if (Attenadance.Branch != null)
                    ListStaff = ListStaff.Where(w => w.Branch == Attenadance.Branch).ToList();
                if (Attenadance.Locations != null)
                    ListStaff = ListStaff.Where(w => w.LOCT == Attenadance.Locations).ToList();
                if (Attenadance.Department != null)
                    ListStaff = ListStaff.Where(w => w.DEPT == Attenadance.Department).ToList();
                if (Attenadance.Division != null)
                    ListStaff = ListStaff.Where(w => w.Division == Attenadance.Division).ToList();
                if (Attenadance.EmpCode != null)
                    ListStaff = ListStaff.Where(w => w.EmpCode == Attenadance.EmpCode).ToList();

                var _list = unitOfWork.Set<ATEmpSchedule>().Where(w =>
                DbFunctions.TruncateTime(w.TranDate) >= Attenadance.FromDate.Date && DbFunctions.TruncateTime(w.TranDate) <= Attenadance.ToDate.Date).ToList();
                _list = _list.Where(w => ListStaff.Where(x => x.EmpCode == w.EmpCode).Any()).ToList();
                var FromDate = Attenadance.FromDate.AddDays(-1);
                var ToDate = Attenadance.ToDate.AddDays(1);
                var _listINOut = (from s in unitOfWork.Set<ATInOut>()
                                  where DbFunctions.TruncateTime(s.FullDate) >= FromDate.Date
                                  && DbFunctions.TruncateTime(s.FullDate) <= ToDate.Date
                                  && s.EmpCode.Trim() != ""
                                  select s).ToList();


                var _listShift = unitOfWork.Set<ATShift>().ToList();
                var _listLeaveh = unitOfWork.Set<HREmpLeave>().Where(w => w.Status == Approval && w.TranType == false).ToList();
                _listLeaveh = _listLeaveh.Where(w => ((w.FromDate.Date >= FromDate.Date && w.FromDate.Date <= ToDate.Date) ||
                (w.ToDate.Date >= FromDate.Date && w.ToDate.Date <= ToDate.Date) ||
                (FromDate.Date >= w.FromDate.Date && FromDate.Date <= w.ToDate.Date) || (ToDate.Date >= w.FromDate.Date && ToDate.Date <= w.ToDate.Date))).ToList();

                var _listLeaveD = unitOfWork.Set<HREmpLeaveD>().Where(w => DbFunctions.TruncateTime(w.LeaveDate) >= FromDate.Date
                && DbFunctions.TruncateTime(w.LeaveDate) <= ToDate.Date).ToList();
                var ListPub = unitOfWork.Set<HRPubHoliday>().ToList();
                var ListLa_Ea = unitOfWork.Set<ATPolicyLaEa>().ToList();
                ListInOut = _listINOut.ToList();
                ListHeader = _list.ToList();
                ListSHift = _listShift.ToList();
                ListSHift = ListSHift.ToList();
                listLeaveH = _listLeaveh.ToList();
                ListLeaveD = _listLeaveD.ToList();
                _LeaveType = _LeaveType.ToList();
                ListLeaveD = ListLeaveD.Where(w => listLeaveH.Where(x => x.Increment == w.LeaveTranNo && x.Status == Approval).Any()).ToList();
                ATPolicy NWPolicy = unitOfWork.Set<ATPolicy>().FirstOrDefault();
                string[] _TranNo = ID.Split(';');
                int i = 0;
                List<ATEmpSchedule> _listHeader = new List<ATEmpSchedule>();
                var listMaternity = unitOfWork.Set<HRReqMaternity>().ToList();
                var listEmpRework = unitOfWork.Set<ATEmpRelWork>().Where(w => w.InDate >= Attenadance.FromDate.Date && w.InDate <= Attenadance.ToDate.Date).ToList();
                var listReqLateEarly = unitOfWork.Set<HRReqLateEarly>().ToList();
                var ListPeriod = unitOfWork.Set<PRpayperiod>().Where(w => DbFunctions.TruncateTime(w.AttendanceStartDate) <= ToDate
                && DbFunctions.TruncateTime(w.AttendanceEndDate) >= FromDate).ToList();
                Progress = _TranNo.Count();
                var ListPara = unitOfWork.Set<PRParameter>().ToList();

                var ListEmpOTReq = unitOfWork.Set<HRRequestOT>().Where(w => w.Status == Approval && DbFunctions.TruncateTime(w.OTEndTime) >= Attenadance.FromDate.Date && DbFunctions.TruncateTime(w.OTEndTime) <= Attenadance.ToDate.Date).ToList();
                var ListOTPolic = unitOfWork.Set<ATOTPolicy>().ToList();
                DateTime FromDateMin = new DateTime(FromDate.Year, FromDate.Month, 1);
                DateTime ToDateMax = new DateTime(ToDate.Year, ToDate.Month, DateTime.DaysInMonth(ToDate.Year, ToDate.Month));

                var IS_PERIORD = unitOfWork.Set<SYSetting>().FirstOrDefault(w => w.SettingName == "IS_PERIORD");

                if (ListPeriod.Count > 0)
                {
                    FromDateMin = ListPeriod.Min(w => w.AttendanceStartDate);
                    ToDateMax = ListPeriod.Min(w => w.AttendanceEndDate);
                }
                var ListEmpLateEarly = (from s in unitOfWork.Set<ATLateEarlyDeduct>()
                                        where DbFunctions.TruncateTime(s.DocumentDate) >= FromDateMin.Date
                                        && DbFunctions.TruncateTime(s.DocumentDate) <= ToDateMax.Date
                                        select s).ToList();
                ListEmpLateEarly = ListEmpLateEarly.Where(w => ListStaff.Where(x => x.EmpCode == w.EmpCode).Any()).ToList();
                if (ListEmpLateEarly.Count() > 0)
                {
                    ListEmpLateEarly.ForEach(w => w.IsLate_Early = 0);
                    ListEmpLateEarly.ForEach(w => w.IsMissScan = 0);
                    ListEmpLateEarly = ListEmpLateEarly.OrderBy(w => w.DocumentDate).ToList();
                }
                List<HRStaffProfile> ListStaffRes = ListStaff.Where(w => w.Status == "I").ToList();
                var ListAtTRes = ListHeader.Where(w => ListStaffRes.Where(x => x.EmpCode == w.EmpCode && (w.TranDate.Date >= x.DateTerminate.Date || x.DateTerminate.Year == 1900)).Any()).ToList();
                //foreach (var item in ListAtTRes)
                //{
                if (ListAtTRes.Count() > 0)
                {
                    unitOfWork.BulkDelete(ListAtTRes);
                }
                //}

                List<ATEmpSchedule> ListUpdateDataATT = new List<ATEmpSchedule>();
                List<ATLateEarlyDeduct> ListDeleteLaEa = new List<ATLateEarlyDeduct>();
                List<ATLateEarlyDeduct> ListAddLaEa = new List<ATLateEarlyDeduct>();
                List<HRRequestOT> ListUpdateOT = new List<HRRequestOT>();
                foreach (var TranNo in _TranNo)
                {
                    if (TranNo.Trim() != "")
                    {
                        i++;
                        int No = Convert.ToInt32(TranNo);

                        var _Empsch = ListHeader.Where(w => w.TranNo == No).ToList();
                        foreach (var item in _Empsch)
                        {
                            Error_Status = item.EmpCode + ":" + item.TranDate.ToString("dd-MMM-yyyy");
                            bool IsPeriod = false;
                            if (IS_PERIORD != null)
                            {
                                if (IS_PERIORD.SettinValue == "YES") IsPeriod = true;
                            }
                            DateTime PFromDate = item.TranDate;
                            DateTime PToDate = item.TranDate;
                            if (IsPeriod == false)
                            {
                                IsPeriod = true;
                                PFromDate = new DateTime(item.TranDate.Year, item.TranDate.Month, 1);
                                PToDate = new DateTime(item.TranDate.Year, item.TranDate.Month, DateTime.DaysInMonth(item.TranDate.Year, item.TranDate.Month));
                            }
                            else
                            {
                                var _Period = ListPeriod.FirstOrDefault(w => w.AttendanceStartDate.Date <= item.TranDate.Date && w.AttendanceEndDate.Date >= item.TranDate);
                                if (_Period != null)
                                {
                                    PFromDate = _Period.AttendanceStartDate;
                                    PToDate = _Period.AttendanceEndDate;
                                }
                                else IsPeriod = false;
                            }
                            var Staff = ListStaff.FirstOrDefault(w => w.EmpCode == item.EmpCode);
                            var _para = ListPara.FirstOrDefault(w => w.Code == Staff.PayParam);
                            var _Inout = ListInOut.Where(w => w.EmpCode == item.EmpCode).ToList();
                            var _EmpResMater = listMaternity.Where(w => w.EmpCode == item.EmpCode
                            && w.FromDate.Date <= item.TranDate.Date && w.ToDate.Date >= item.TranDate.Date).ToList();
                            var _EmpResLateEalry = listReqLateEarly.Where(w => w.EmpCode == item.EmpCode && w.LeaveDate.Date == item.TranDate.Date && w.Status == "APPROVED").ToList();
                            if (listEmpRework.Where(w => w.FromEmpCode == item.EmpCode && w.InDate.Date == item.TranDate.Date).Any())
                            {
                                var _EmpRework = listEmpRework.FirstOrDefault(w => w.FromEmpCode == item.EmpCode && w.InDate.Date == item.TranDate.Date);
                                _Inout = ListInOut.Where(w => w.EmpCode == _EmpRework.ToEmpCode).ToList();
                            }

                            Header.EmpCode = item.EmpCode;
                            item.OTStart = null;
                            item.OTEnd = null;

                            var EmpOTReq = ListEmpOTReq.Where(w => w.EmpCode == item.EmpCode && w.OTEndTime.Date == item.TranDate.Date).ToList();
                            bool IsOT = false;
                            if (EmpOTReq.Count() > 0)
                                IsOT = true;
                            ATEmpSchedule result = ClsAttendance.GetResult(item, ListSHift, _Inout, IsOT,NWPolicy);
                            DateTime NWFromDate = result.TranDate + NWPolicy.NWFROM.Value.TimeOfDay;
                            DateTime NWToDate = result.TranDate + NWPolicy.NWTO.Value.TimeOfDay;
                            NWToDate = NWToDate.AddDays(1);

                            result = ClsAttendance.GenrateAttendance(result, item.EmpCode, NWPolicy,
                            item.LeaveDesc, ListPub.ToList(),
                            ListSHift, ListLeaveD, _LeaveType.ToList());

                            if (ListPub.Where(w => w.PDate.Date == result.TranDate.Date).Any())
                            {
                                result.Remark2 = "PH";
                            }
                            if (_LeaveType.Where(w => w.Code == result.SHIFT).Any())
                            {
                                result.LeaveDesc = result.SHIFT;
                            }
                            if (result.LeaveDesc == "" || result.LeaveDesc == null)
                                result.LeaveDesc = item.LeaveDesc;

                            //NWPolicy
                            string LeaveLa = "";
                            string LeaveEa = "";
                            ClsMissScan _MissScan = new ClsMissScan();
                            if (ListLeaveD.Where(w => w.LeaveDate.Date == result.TranDate.Date && w.EmpCode == item.EmpCode).Any())
                            {
                                var _Shift = ListSHift.FirstOrDefault(w => w.Code == result.SHIFT);
                                var _Leave = ListLeaveD.Where(w => w.LeaveDate == result.TranDate && w.EmpCode == item.EmpCode).ToList();
                                foreach (var Leave in _Leave)
                                {
                                    var _LeaveCode = _LeaveType.FirstOrDefault(w => w.Code == Leave.LeaveCode);
                                    if (Leave.Remark == "Morning")
                                    {
                                        LeaveLa = Leave.LeaveCode;
                                        if (_Shift != null && result.Late1 > 0)
                                        {
                                            if (result.Start1.Value.TimeOfDay < _Shift.BreakEnd.Value.TimeOfDay)
                                            {
                                                result.Late1 = 0;
                                            }
                                        }

                                        ClsAttendance.IsMissScan(_para, _LeaveCode, item, _MissScan, Leave.Remark);
                                    }
                                    else if (Leave.Remark == "Afternoon")
                                    {
                                        LeaveEa = Leave.LeaveCode;
                                        if (_Shift != null && result.Early1 > 0)
                                        {
                                            if (result.End1.Value.TimeOfDay > _Shift.BreakStart.Value.TimeOfDay)
                                            {
                                                result.Early1 = 0;
                                            }
                                        }
                                        ClsAttendance.IsMissScan(_para, _LeaveCode, item, _MissScan, Leave.Remark);
                                    }
                                    else if (Leave.Remark == "Hours")
                                    {
                                        int TotalMin = (int)Leave.EndTime.Value().Subtract(Leave.StartTime.Value()).TotalMinutes;
                                        if (result.IN1 >= Leave.StartTime && result.IN1 <= Leave.EndTime)
                                        {
                                            result.Late1 -= TotalMin; _MissScan.IsMissIN = true;
                                        }
                                        if (result.OUT1 >= Leave.StartTime && result.OUT1 <= Leave.EndTime)
                                        {
                                            result.Early1 -= TotalMin; _MissScan.IsMissOut = true;
                                        }
                                        if (result.Flag2 == 1)
                                        {
                                            if (result.IN2 >= Leave.StartTime && result.IN2 <= Leave.EndTime)
                                            {
                                                result.Late2 -= TotalMin;
                                                _MissScan.IsMissIN2 = true;
                                            }
                                            if (result.OUT2 >= Leave.StartTime && result.OUT2 <= Leave.EndTime)
                                            {
                                                result.Early2 -= TotalMin;
                                                _MissScan.IsMissOut2 = true;
                                            }
                                        }
                                        if (result.Late1 < 0) result.Late1 = 0;
                                        if (result.Early1 < 0) result.Early1 = 0;
                                        if (result.Late2 < 0) result.Late2 = 0;
                                        if (result.Early2 < 0) result.Early2 = 0;
                                    }
                                    else
                                    {
                                        result.Late1 = 0; result.Early1 = 0;
                                        result.Late2 = 0; result.Early2 = 0;
                                        _MissScan.IsMissIN = true; _MissScan.IsMissOut = true;
                                        _MissScan.IsMissIN2 = true; _MissScan.IsMissOut2 = true;
                                    }
                                }
                            }

                            //int CountLa = ListEmpLateEarly.Where(w => w.DocumentDate.Date >= PFromDate.Date && w.DocumentDate.Date <= PToDate.Date
                            //&& w.EmpCode == item.EmpCode && w.DeductType == "LATE").Sum(x => x.IsLate_Early);
                            //int CountEa = ListEmpLateEarly.Where(w => w.DocumentDate.Date >= PFromDate.Date && w.DocumentDate.Date <= PToDate.Date
                            //&& w.EmpCode == item.EmpCode && w.DeductType == "EARLY").Sum(x => x.IsLate_Early);

                            //Request Maternity Late/Early
                            if (_EmpResMater.Sum(x => x.LateEarly) > 0)
                            {
                                result = ClsAttendance.Cal_Maternity_LateEarly(result, _EmpResMater);
                            }

                            ClsAttendance ObjAttendance = new ClsAttendance();
                            //Validate Late Early
                            ObjAttendance.Validate_Late_Early(result, Staff, ListLa_Ea);

                            if (IsPeriod == true)
                            {
                                var _empLaEa = ListEmpLateEarly.Where(w => w.EmpCode == item.EmpCode && w.DocumentDate.Date == item.TranDate.Date).ToList();
                                //Late & Early
                                var LstlateEarly = _empLaEa.Where(w => w.DeductType == "LATE" || w.DeductType == "EARLY").ToList();
                                ListDeleteLaEa.AddRange(LstlateEarly);
                                ObjAttendance.Insert_LateEarly(unitOfWork, result, _MissScan, ListAddLaEa);

                                //MissScan
                                var _empMissScanIN = _empLaEa.Where(w => w.DeductType == "MissScan" || w.DeductType == "ABS").ToList();
                                ListDeleteLaEa.AddRange(_empMissScanIN);
                                ObjAttendance.Insert_MissScan(unitOfWork, result, _MissScan, ListAddLaEa);
                            }

                            

                            //"Req Late/Early"
                            if (_EmpResLateEalry.Sum(x => x.Qty) > 0)
                            {
                                result = ClsAttendance.Cal_Req_Late_Early(result, _EmpResLateEalry);
                            }

                            //Validate_OT
                            result = ClsAttendance.Cal_OT(result, EmpOTReq, ListOTPolic);
                            foreach (var _OT in EmpOTReq)
                            {
                                ListUpdateOT.Add(_OT);
                            }

                            if (result.OTApproval < 0) result.OTApproval = 0;
                            if (result.Start1.Value.Year == 1900 && result.End1.Value.Year == 1900
                            && result.Start2.Value.Year == 1900 && result.End2.Value.Year == 1900
                            && (result.LeaveCode == "" || result.LeaveCode == null || result.LeaveCode == "ABS") && (result.LeaveDesc == "" || result.LeaveDesc == null))
                            {
                                result.LeaveDesc = "ABS";
                            }
                            else if (result.Start1.Value.Year != 1900 || result.End1.Value.Year != 1900
                            || result.Start2.Value.Year != 1900 || result.End2.Value.Year != 1900)
                            {
                                result.LeaveDesc = "";
                            }
                            if (result.LeaveDesc == "ABS")
                            {
                                if (result.SHIFT == "OFF" || result.SHIFT == "PH") result.LeaveDesc = "";
                            }
                            if (result.WHOUR > 0)
                            {
                                if (result.TranDate.DayOfWeek == DayOfWeek.Saturday && _para.WDSAT == true && _para.WDSATDay == 0.5M)
                                {
                                    result.ActWHour = (_para.WHOUR / 2) - (Convert.ToDecimal(result.Late1 + result.Late2 + result.Early1 + result.Early2) / 60.00M);
                                }
                                else
                                    result.ActWHour = _para.WHOUR - (Convert.ToDecimal(result.Late1 + result.Late2 + result.Early1 + result.Early2) / 60.00M);
                                result.WokingHour = "";

                                string[] Hour = result.WHOUR.ToString().Split('.');
                                if (Convert.ToInt32(Hour[0]) > 0)
                                    result.WokingHour = Hour[0] + "h";
                                if (Hour.Length > 1 && Convert.ToInt64(Hour[1]) > 0)
                                    result.WokingHour += Convert.ToInt64((result.WHOUR - Convert.ToDecimal(Hour[0])) * 60) + "min";
                            }
                            result.ChangedBy = User.UserName;
                            result.ChangedOn = DateTime.Now;
                            //unitOfWork.Update(result);
                            ListUpdateDataATT.Add(result);
                        }
                    }
                }
                if (ListUpdateDataATT.Count > 0)
                {
                    unitOfWork.BulkDelete(ListDeleteLaEa);
                    unitOfWork.BulkInsert(ListAddLaEa);
                    if (ListUpdateOT.Count > 0)
                    {
                        unitOfWork.BulkUpdate(ListUpdateOT);
                    }
                    unitOfWork.BulkUpdate(ListUpdateDataATT);
                }
                //unitOfWork.Save();

                unitOfWork.BulkCommit();

                return SYConstant.OK;

            }
            catch (DbEntityValidationException e)
            {
                unitOfWork.Rollback();
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, Error_Status, SYActionBehavior.ADD.ToString(), e, true);
            }
            catch (DbUpdateException e)
            {
                unitOfWork.Rollback();
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, Error_Status, SYActionBehavior.ADD.ToString(), e, true);
            }
            catch (Exception e)
            {
                unitOfWork.Rollback();
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, Error_Status, SYActionBehavior.ADD.ToString(), e, true);
            }
        }
        public string Set_DefaultShift(DateTime FromDate, DateTime ToDate, List<HRBranch> ListBranch)
        {
            string Status_Error = "";
            try
            {
                var _listShift = unitOfWork.Set<ATShift>().ToList();
                var ListPub = unitOfWork.Set<HRPubHoliday>().ToList();
                var ListEmpSch = unitOfWork.Set<ATEmpSchedule>().Where(w => DbFunctions.TruncateTime(w.TranDate) >= FromDate.Date &&
                DbFunctions.TruncateTime(w.TranDate) <= ToDate.Date).ToList();
                var branchCodes = ListBranch.Select(b => b.Code).ToList();
                var ListStaff = (from staff in unitOfWork.Set<HRStaffProfile>()
                                 join branchCode in branchCodes on staff.Branch equals branchCode
                                 where !string.IsNullOrEmpty(staff.ROSTER)
                                 select staff).ToList();

                var ListBatch = unitOfWork.Set<ATBatch>().ToList();
                var ListBatchItem = unitOfWork.Set<ATBatchItem>().ToList();
                ListBatchItem = ListBatchItem.Where(w => ListBatch.Where(x => x.Code == w.BatchNo).Any()).ToList();
                ListPub = ListPub.ToList();
                List<ATEmpSchedule> ListAddSchedule = new List<ATEmpSchedule>();
                foreach (DateTime InDate in DateTimeHelper.EachDay(FromDate, ToDate))
                {
                    var _Shift = new ATShift();
                    var Staff = ListStaff.Where(w => w.StartDate.Value.Date <= InDate.Date &&
                           (w.TerminateStatus == "" || w.TerminateStatus == null || w.DateTerminate > InDate.Date)
                           && (w.ROSTER != "" && w.ROSTER != null)).ToList();

                    foreach (var st in Staff)
                    {
                        if (ListEmpSch.Where(x => x.EmpCode == st.EmpCode && x.TranDate.Date == InDate.Date).ToList().Any())
                            continue;
                        Status_Error = st.EmpCode + ":" + InDate.ToString();

                        var att = new ATEmpSchedule();
                        Shift_Default(att, InDate, st, _listShift, ListPub, ListBatchItem);
                        ListAddSchedule.Add(att);
                    }
                }
                if (ListAddSchedule.Count > 0)
                {
                    unitOfWork.BulkInsert(ListAddSchedule);
                }
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, Status_Error, SYActionBehavior.ADD.ToString(), e, true);
            }
            catch (DbUpdateException e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, Status_Error, SYActionBehavior.ADD.ToString(), e, true);
            }
            catch (Exception e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, Status_Error, SYActionBehavior.ADD.ToString(), e, true);
            }
        }
        public ATEmpSchedule Shift_Default(ATEmpSchedule att, DateTime InDate, HRStaffProfile Staff,
           List<ATShift> ListShift, List<HRPubHoliday> ListHoliday, List<ATBatchItem> ListBatchItem)
        {
            string ShiftCode = "";
            var _Shift = new ATShift();
            att.Flag1 = 2;
            att.Flag2 = 2;
            DateTime CheckIN1 = new DateTime(1900, 1, 1);
            DateTime CheckOut1 = new DateTime(1900, 1, 1);
            DateTime CheckIN2 = new DateTime(1900, 1, 1);
            DateTime CheckOut2 = new DateTime(1900, 1, 1);
            DateTime CheckDate = new DateTime(1900, 1, 1);
            bool Result = false;
            if (ListHoliday.Where(w => w.PDate.Date == InDate.Date).Any()) { Result = true; ShiftCode = "PH"; }
            var Batch = ListBatchItem.Where(w => w.BatchNo == Staff.ROSTER).ToList();
            if (ListShift.Where(w => Batch.Where(x => x.ShiftCode == w.Code).Any()).Count() > 0)
            {
                if (Batch.Count() > 0 && Result == false)
                {
                    var _Batch = new ATBatchItem();
                    if (InDate.DayOfWeek.ToString() == "Monday")
                    {
                        Batch = Batch.Where(w => w.Mon == true).ToList();
                        if (Batch.Count() > 0)
                        {
                            _Batch = Batch.First();
                        }
                    }
                    else if (InDate.DayOfWeek.ToString() == "Tuesday")
                    {
                        Batch = Batch.Where(w => w.Tue == true).ToList();
                        if (Batch.Count() > 0)
                            _Batch = Batch.First();
                    }
                    else if (InDate.DayOfWeek.ToString() == "Wednesday")
                    {
                        Batch = Batch.Where(w => w.Wed == true).ToList();
                        if (Batch.Count() > 0)
                            _Batch = Batch.First();
                    }
                    else if (InDate.DayOfWeek.ToString() == "Thursday")
                    {
                        Batch = Batch.Where(w => w.Thu == true).ToList();
                        if (Batch.Count() > 0)
                            _Batch = Batch.First();
                    }
                    else if (InDate.DayOfWeek.ToString() == "Friday")
                    {
                        Batch = Batch.Where(w => w.Fri == true).ToList();
                        if (Batch.Count() > 0)
                            _Batch = Batch.First();
                    }
                    else if (InDate.DayOfWeek.ToString() == "Saturday")
                    {
                        Batch = Batch.Where(w => w.Sat == true).ToList();
                        if (Batch.Count() > 0)
                            _Batch = Batch.First();
                    }
                    else if (InDate.DayOfWeek.ToString() == "Sunday")
                    {
                        Batch = Batch.Where(w => w.Sun == true).ToList();
                        if (Batch.Count() > 0)
                            _Batch = Batch.First();
                    }

                    if (_Batch.BatchNo != null)
                    {
                        _Shift = ListShift.Where(w => w.Code == _Batch.ShiftCode).FirstOrDefault();
                        if (_Shift == null)
                            return null;
                        else ShiftCode = _Shift.Code;
                    }
                    else
                    {
                        ShiftCode = "OFF";
                        Result = true;
                    }
                }
                if (ShiftCode == "")
                    return null;
                if (Result == false)
                {
                    CheckIN1 = new DateTime(InDate.Year, InDate.Month, InDate.Day, _Shift.CheckIn1.Value.Hour, _Shift.CheckIn1.Value.Minute, 0);
                    CheckOut1 = new DateTime(InDate.Year, InDate.Month, InDate.Day, _Shift.CheckOut1.Value.Hour, _Shift.CheckOut1.Value.Minute, 0);
                    if (_Shift.OverNight1 == true) CheckOut1 = CheckOut1.AddDays(1);
                    if (_Shift.SplitShift == true)
                    {
                        att.Flag2 = 1;
                        CheckIN2 = new DateTime(InDate.Year, InDate.Month, InDate.Day, _Shift.CheckIn2.Value.Hour, _Shift.CheckIn2.Value.Minute, 0);
                        CheckOut2 = new DateTime(InDate.Year, InDate.Month, InDate.Day, _Shift.CheckOut2.Value.Hour, _Shift.CheckOut2.Value.Minute, 0);
                    }
                    att.Flag1 = 1;
                }
                att.EmpCode = Staff.EmpCode;
                att.TranDate = InDate;
                att.IN1 = CheckIN1;
                att.OUT1 = CheckOut1;
                att.Start1 = CheckDate;
                att.End1 = CheckDate;
                att.Late1 = 0;
                att.LateDesc1 = "";
                att.LateVal1 = 0;
                att.LateFlag1 = "";
                att.LateAm1 = 0;
                att.Early1 = 0;
                att.DepDesc1 = "";
                att.DepVal1 = 0;
                att.DepFlag1 = "";
                att.DepAm1 = 0;
                att.IN2 = CheckIN2;
                att.OUT2 = CheckOut2;
                att.Start2 = CheckDate;
                att.End2 = CheckDate;
                att.Late2 = 0;
                att.LateDesc2 = "";
                att.LateVal2 = 0;
                att.LateFlag2 = "";
                att.LateAm2 = 0;
                att.Early2 = 0;
                att.DepDesc2 = "";
                att.DepVal2 = 0;
                att.DEPFLAG2 = "";
                att.DEPAM2 = 0;
                att.OTTYPE = "-1";
                att.LeaveNo = -1;
                att.SHIFT = ShiftCode;
                att.STATUS = 0;
                att.REMARK = "";
                att.CreateBy = User.CreatedBy;
                att.CreateOn = DateTime.Now;
                att.LeaveCode = "";
            }

            return att;
        }
        public string GenrateAttendance_(string TranNo)
        {
            List<string> ListTranNo = TranNo.Split(';').Where(id => !string.IsNullOrWhiteSpace(id)).ToList();
            var FromDate = Attenadance.FromDate.AddDays(-1);
            var ToDate = Attenadance.ToDate.AddDays(1);
            string Approval = SYDocumentStatus.APPROVED.ToString();

            //var _list = unitOfWork.Set<ATEmpSchedule>().Where(w => ListTranNo.Contains(w.TranNo.ToString()) &&
            //        w.TranDate >= Attenadance.FromDate.Date && w.TranDate <= Attenadance.ToDate.Date).ToList();

            var _listAttendance = unitOfWork.Set<ATEmpSchedule>().Where(w => w.TranDate >= Attenadance.FromDate.Date && w.TranDate <= Attenadance.ToDate.Date).ToList();

            List<string> ListEmpCode = _listAttendance.Where(w => ListTranNo.Contains(w.TranNo.ToString()))
                .GroupBy(y => y.EmpCode).Select(a => a.Key).ToList();

            var _LeaveType = unitOfWork.Set<HRLeaveType>().ToList();
            var _listShift = unitOfWork.Set<ATShift>().ToList();
            List<HRStaffProfile> ListStaff = unitOfWork.Set<HRStaffProfile>()
                .Where(w => ListEmpCode.Contains(w.EmpCode) &&
                    (string.IsNullOrEmpty(Attenadance.Branch) || w.Branch == Attenadance.Branch) &&
                    (string.IsNullOrEmpty(Attenadance.Locations) || w.LOCT == Attenadance.Locations) &&
                    (string.IsNullOrEmpty(Attenadance.Department) || w.DEPT == Attenadance.Department) &&
                    (string.IsNullOrEmpty(Attenadance.Division) || w.Division == Attenadance.Division) &&
                    (string.IsNullOrEmpty(Attenadance.EmpCode) || w.EmpCode == Attenadance.EmpCode)
                ).ToList();
            var ListInOut = unitOfWork.Set<ATInOut>().Where(w => ListEmpCode.Contains(w.EmpCode) &&
          DbFunctions.TruncateTime(w.FullDate) >= FromDate.Date.AddDays(-1) && DbFunctions.TruncateTime(w.FullDate) <= ToDate).ToList();


            var ListLeaveD = (from leaveD in unitOfWork.Set<HREmpLeaveD>()
                              join leaveh in unitOfWork.Set<HREmpLeave>()
                              on leaveD.LeaveTranNo equals leaveh.Increment
                              where leaveh.Status == Approval
                              && leaveD.LeaveDate >= FromDate && leaveD.LeaveDate <= ToDate
                              && leaveh.EmpCode == leaveh.EmpCode
                              && leaveD.EmpCode == User.UserName
                              select leaveD
                                        ).ToListAsync();

            List<ATShift> ListShift = unitOfWork.Set<ATShift>().ToList();

            foreach (var item in _listAttendance)
            {
                var _Inout = ListInOut.Where(w => w.EmpCode == item.EmpCode).ToList();
                //ATEmpSchedule result = ClsAttendance.GetAtten_InOut(item, ListShift, _Inout, false);

            }
            return SYConstant.OK;
        }
        public Dictionary<string, dynamic> OnDataSelector(params object[] keys)
        {
            Dictionary<string, dynamic> keyValues = new Dictionary<string, dynamic>();

            var ListBranch = SYConstant.getBranchDataAccess();
            var ListBranchCodes = ListBranch.Select(b => b.Code).ToList();
            var ListStaff = (from staff in unitOfWork.Set<HRStaffProfile>()
                             join branchCode in ListBranchCodes on staff.Branch equals branchCode
                             select staff).ToList();

            keyValues.Add("STAFF_SELECT", ListStaff);
            keyValues.Add("SHIFT_SELECT", unitOfWork.Set<ATShift>().ToList());
            keyValues.Add("BRANCHES_SELECT", ListBranch);
            keyValues.Add("DIVISION_SELECT", ClsFilter.LoadDivision());
            keyValues.Add("DEPARTMENT_SELECT", ClsFilter.LoadDepartment());
            keyValues.Add("LOCATION_SELECT", unitOfWork.Set<HRLocation>().ToList());

            return keyValues;
        }
    }
    public class ClsAtt
    {
        public string Leave { get; set; }
        public bool? GEN { get; set; }
        public decimal WHour { get; set; }
        public decimal NWH { get; set; }
        public decimal WOT { get; set; }
        public decimal OTMin { get; set; }
        public int Late { get; set; }
        public int Early { get; set; }
    }
}
