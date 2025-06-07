using Humica.Core.DB;
using Humica.Core.FT;
using Humica.Core.SY;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.EF.Repo;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;

namespace Humica.Performance
{
    public class ClsKPITracking : IClsKPITracking
    {
        public FTINYear FInYear { get; set; }
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public string ScreenId { get; set; }
        public string MessageError { get; set; }
        public DateTime DocumentDate { get; set; }
        public HRKPITracking HeaderKPITracking { get; set; }
        public List<HRKPITracking> listKPITracking { get; set; }
        public List<MDUploadTemplate> ListTemplate { get; set; }
        public List<ListKPI> LIstEmplSch { get; set; }
        public List<ListAssign> ListKPIEmpPending { get; set; }
        public List<HRKPITimeSheet> ListTimeSheet { get; set; }
        protected IUnitOfWork unitOfWork;
        public ClsKPITracking()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }
        public void OnLoad()
        {
            unitOfWork = new UnitOfWork<HumicaDBContext>();
        }
        public virtual List<HRKPITracking> OnIndexLoading(string Status, bool ESS = false)
        {
            OnLoad();
            List<HRKPITracking> TempList = new List<HRKPITracking>();
            if (ESS)
            {
                string UserName = User.UserName;
                TempList = unitOfWork.Set<HRKPITracking>().Where(w => w.EmpCode == UserName || w.DirectedByCode == UserName).ToList();
            }
            else
                TempList = unitOfWork.Set<HRKPITracking>().ToList();
            if (!string.IsNullOrEmpty(Status))
            {
                TempList = TempList.Where(w => w.Status == Status).ToList();
            }
            return TempList.OrderByDescending(w => w.DocumentDate).ToList();
        }

        public virtual List<ListAssign> OnIndexLoadingAssign(bool ESS = false)
        {
            List<HRKPIAssignHeader> TempList = new List<HRKPIAssignHeader>();
            List<HRKPIAssignMember> TempListMember = new List<HRKPIAssignMember>();
            string Approved = SYDocumentStatus.APPROVED.ToString();
            DateTime Deadline = DateTime.Now;
            if (ESS)
            {
                string UserName = User.UserName;
                TempList = unitOfWork.Set<HRKPIAssignHeader>().Where(w => w.ReStatus == Approved && w.AssignedBy != "BYTEAM"
                && (w.HandleCode == UserName)).ToList();
                var Staff = unitOfWork.Set<HRStaffProfile>().FirstOrDefault(w => w.DEPT == "PLT" && w.EmpCode == UserName);
                if (Staff != null)
                {
                    var TempListTe = (from KPI in unitOfWork.Set<HRKPIAssignHeader>()
                                      join _Staff in unitOfWork.Set<HRStaffProfile>()
                                      on KPI.HandleCode equals _Staff.EmpCode
                                      where _Staff.DEPT == "PLT"
                                      && KPI.ReStatus == Approved && KPI.AssignedBy != "BYTEAM"
                           && _Staff.APPTracking == Staff.EmpCode
                                      select KPI
                     ).ToList();
                    TempList.AddRange(TempListTe);
                }

            }
            else
            {
                TempList = unitOfWork.Set<HRKPIAssignHeader>().Where(w => w.ReStatus == Approved).ToList();
            }
            TempListMember = unitOfWork.Set<HRKPIAssignMember>().ToList();
            // Retrieve assignments
            var assignments = (from assign in TempList
                               select new ListAssign
                               {
                                   AssignCode = assign.AssignCode,
                                   EmpCode = assign.HandleCode,
                                   EmployeeName = assign.HandleName,
                                   Position = assign.Position,
                                   Department = assign.Department,
                                   KPIType = assign.KPIType,
                                   PeriodFrom = assign.PeriodFrom.Value,
                                   PeriodTo = assign.PeriodTo.Value,
                                   Status = assign.AssignedBy,
                                   HandlePerson = assign.HandleCode + " : " + assign.HandleName
                               }).ToList();

            var result = new List<ListAssign>();
            if (ESS == true)// && TempList.Count() == 0)
            {
                string UserName = User.UserName;
                var TempListTe = unitOfWork.Set<HRKPIAssignHeader>().Where(w => w.ReStatus == Approved
                 && w.AssignedBy == "BYTEAM" && (w.PlanerCode == UserName || w.HandleCode == UserName)).ToList();
                if (TempListTe.Count() == 0)
                {
                    TempListMember = TempListMember.Where(x => x.EmpCode == UserName).ToList();
                    var departmentList = TempListMember.Select(w => w.AssignCode).ToList();
                    TempListTe = unitOfWork.Set<HRKPIAssignHeader>().Where(w => departmentList.Contains(w.AssignCode)).ToList();
                }
                var assignmentTem = (from assign in TempListTe
                                     select new ListAssign
                                     {
                                         AssignCode = assign.AssignCode,
                                         EmpCode = assign.HandleCode,
                                         EmployeeName = assign.HandleName,
                                         Position = assign.Position,
                                         Department = assign.Department,
                                         KPIType = assign.KPIType,
                                         PeriodFrom = assign.PeriodFrom.Value,
                                         PeriodTo = assign.PeriodTo.Value,
                                         Status = assign.AssignedBy,
                                         HandlePerson = assign.HandleCode + " : " + assign.HandleName
                                     }).ToList();
                assignments.AddRange(assignmentTem);
            }
            foreach (var assignment in assignments)
            {
                if (assignment.Status != SYDocumentASSIGN.BYTEAM.ToString())
                {
                    result.Add(assignment);
                }
                var relatedMembers = TempListMember.Where(m => m.AssignCode == assignment.AssignCode).ToList();
                foreach (var member in relatedMembers)
                {
                    var memberAssignment = new ListAssign
                    {
                        AssignCode = assignment.AssignCode,
                        EmpCode = member.EmpCode,
                        EmployeeName = member.EmployeeName,
                        Position = member.Position,
                        Department = member.Department,
                        KPIType = assignment.KPIType,
                        PeriodFrom = assignment.PeriodFrom,
                        PeriodTo = assignment.PeriodTo,
                        HandlePerson = assignment.HandlePerson,
                        Status = assignment.Status
                    };

                    result.Add(memberAssignment);
                }
            }
            return result;
        }

        public virtual void OnCreatingLoading(string ID, string EmpCode)
        {
            this.HeaderKPITracking = new HRKPITracking();
            ListTimeSheet = new List<HRKPITimeSheet>();
            var Plan = unitOfWork.Set<HRKPIAssignHeader>().FirstOrDefault(w => w.AssignCode == ID);
            if (Plan != null)
            {
                HeaderKPITracking.EmpCode = Plan.HandleCode;
                HeaderKPITracking.EmpName = Plan.HandleName;
                HeaderKPITracking.Department = Plan.Department;
                HeaderKPITracking.Position = Plan.Position;
                HeaderKPITracking.DirectedByCode = Plan.PlanerCode;
                if (Plan.HandleCode != EmpCode)
                {
                    var members = unitOfWork.Set<HRKPIAssignMember>().FirstOrDefault(w => w.EmpCode == EmpCode);
                    if (members != null)
                    {
                        HeaderKPITracking.EmpCode = members.EmpCode;
                        HeaderKPITracking.EmpName = members.EmployeeName;
                        HeaderKPITracking.Department = members.Department;
                        HeaderKPITracking.Position = members.Position;
                    }
                }
                HeaderKPITracking.AssignCode = Plan.AssignCode;
                HeaderKPITracking.KPIType = Plan.KPIType;
                HeaderKPITracking.DocumentDate = DateTime.Now;
                HeaderKPITracking.Actual = 0;
            }
        }
        public virtual void OnDetailLoading(params object[] keys)
        {
            int TranNo = (int)keys[0];
            this.HeaderKPITracking = unitOfWork.Set<HRKPITracking>().FirstOrDefault(w => w.TranNo == TranNo);
            if (this.HeaderKPITracking != null)
            {
                this.ListTimeSheet = unitOfWork.Set<HRKPITimeSheet>().Where(w => w.ID == TranNo).ToList();
            }
        }
        public string Create()
        {
            OnLoad();
            try
            {
                try
                {
                    unitOfWork.BeginTransaction();
                    HeaderKPITracking.Status = SYDocumentStatus.PENDING.ToString();
                    HeaderKPITracking.CreatedBy = User.UserName;
                    HeaderKPITracking.CreatedOn = DateTime.Now;
                    var Assign = unitOfWork.Set<HRKPIAssignHeader>().FirstOrDefault(w => w.AssignCode == HeaderKPITracking.AssignCode);
                    if (Assign != null)
                    {
                        if (Assign.AssignedBy == "BYTEAM")
                        {
                            if (Assign.HandleCode == User.UserName)
                            {
                                HeaderKPITracking.DirectedByCode = Assign.DirectedByCode;
                            }
                            else if (Assign.PlanerCode == User.UserName)
                            {
                                HeaderKPITracking.DirectedByCode = Assign.HandleCode;
                            }
                            else
                            {
                                HeaderKPITracking.DirectedByCode = Assign.PlanerCode;
                            }
                            if (Assign.HandleCode == User.UserName || Assign.PlanerCode == User.UserName)
                            {
                                if (HeaderKPITracking.EmpCode != User.UserName)
                                {
                                    HeaderKPITracking.Status = SYDocumentStatus.APPROVED.ToString();
                                }
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(HeaderKPITracking.EmpCode))
                    {
                        var StaffPro = unitOfWork.Set<HRStaffProfile>().FirstOrDefault(w => w.EmpCode == HeaderKPITracking.EmpCode);
                        if (StaffPro != null && StaffPro.IsAutoAppKPITraing == true)
                        {
                            HeaderKPITracking.Status = SYDocumentStatus.APPROVED.ToString();
                        }
                    }
                    unitOfWork.Add(HeaderKPITracking);
                    unitOfWork.Save();
                    int i = HeaderKPITracking.TranNo;
                    int lineItem = 0;
                    foreach (var item in ListTimeSheet)
                    {
                        if (item.FromTime.HasValue)
                        {
                            item.FromTime = HeaderKPITracking.DocumentDate.Date + item.FromTime.Value.TimeOfDay;
                        }
                        if (item.ToTime.HasValue)
                        {
                            item.ToTime = HeaderKPITracking.DocumentDate.Date + item.ToTime.Value.TimeOfDay;
                        }
                        lineItem += 1;
                        item.ID = i;
                        item.LineItem = lineItem;
                        if (item.FromTime.HasValue && item.ToTime.HasValue)
                        {
                            item.Hours = (decimal)item.ToTime.Value.Subtract(item.FromTime.Value).TotalHours;
                            if (item.Hours > 0)
                            {
                                TimeSpan time = item.ToTime.Value.Subtract(item.FromTime.Value);
                                if (time.Hours > 0)
                                    item.TotalHours = time.Hours + "h ";
                                if (time.Minutes > 0)
                                    item.TotalHours += time.Minutes + "min";
                            }
                        }
                        unitOfWork.Add(item);
                    }
                    unitOfWork.Save();
                    unitOfWork.Commit();
                }
                catch (Exception e)
                {
                    unitOfWork.Rollback();
                }
                //CLsKPIAssign assign = new CLsKPIAssign();
                //assign.Calculate(HeaderKPITracking.AssignCode);
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, "", SYActionBehavior.ADD.ToString(), e, true);
            }
            catch (DbUpdateException e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, "", SYActionBehavior.ADD.ToString(), e, true);
            }
            catch (Exception e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, "", SYActionBehavior.ADD.ToString(), e, true);
            }
        }
        public string Update(int id)
        {
            OnLoad();
            try
            {
                var objMatch = unitOfWork.Set<HRKPITracking>().FirstOrDefault(w => w.TranNo == id);
                if (objMatch == null)
                {
                    return "INV_DOC";
                }
                var _ListAgenda = unitOfWork.Set<HRKPITimeSheet>().Where(w => w.ID == id).ToList();
                foreach (var item in _ListAgenda)
                {
                    unitOfWork.Delete(item);
                }

                int lineItem = 0;
                foreach (var item in ListTimeSheet)
                {
                    lineItem += 1;
                    item.ID = id;
                    if (item.FromTime.HasValue)
                    {
                        item.FromTime = HeaderKPITracking.DocumentDate.Date + item.FromTime.Value.TimeOfDay;
                    }
                    if (item.ToTime.HasValue)
                    {
                        item.ToTime = HeaderKPITracking.DocumentDate.Date + item.ToTime.Value.TimeOfDay;
                    }
                    item.LineItem = lineItem;
                    if (item.FromTime.HasValue && item.ToTime.HasValue)
                    {
                        item.Hours = (decimal)item.ToTime.Value.Subtract(item.FromTime.Value).TotalHours;
                        item.TotalHours = "";
                        if (item.Hours > 0)
                        {
                            TimeSpan time = item.ToTime.Value.Subtract(item.FromTime.Value);
                            if (time.Hours > 0)
                                item.TotalHours = time.Hours + "h ";
                            if (time.Minutes > 0)
                                item.TotalHours += time.Minutes + "min";
                        }
                    }
                    unitOfWork.Add(item);
                }
                objMatch.Actual = HeaderKPITracking.Actual;
                objMatch.Remark = HeaderKPITracking.Remark;
                objMatch.ChangedBy = User.UserName;
                objMatch.ChangedOn = DateTime.Now;

                unitOfWork.Update(objMatch);
                unitOfWork.Save();
                CLsKPIAssign assign = new CLsKPIAssign();
                assign.Calculate(objMatch.AssignCode);
                return SYConstant.OK;
            }
            catch (Exception e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, this.HeaderKPITracking.TranNo.ToString(), SYActionBehavior.ADD.ToString(), e, true);
            }
        }
        public string Delete(params object[] keys)
        {
            try
            {
                OnLoad();
                int id = (int)keys[0];
                this.HeaderKPITracking = unitOfWork.Set<HRKPITracking>().FirstOrDefault(w => w.TranNo == id);

                if (HeaderKPITracking == null)
                {
                    return "INV_DOC";
                }
                this.ListTimeSheet = unitOfWork.Set<HRKPITimeSheet>().Where(w => w.ID == id).ToList();
                foreach (var read in ListTimeSheet)
                {
                    unitOfWork.Delete(read);
                }
                unitOfWork.Delete(HeaderKPITracking);
                unitOfWork.Save();
                if (HeaderKPITracking.Status == SYDocumentStatus.APPROVED.ToString())
                {
                    CLsKPIAssign assign = new CLsKPIAssign();
                    assign.Calculate(HeaderKPITracking.AssignCode);
                }
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, HeaderKPITracking.TranNo.ToString(), SYActionBehavior.ADD.ToString(), e, true);
            }
            catch (DbUpdateException e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, HeaderKPITracking.TranNo.ToString(), SYActionBehavior.ADD.ToString(), e, true);
            }
            catch (Exception e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, HeaderKPITracking.TranNo.ToString(), SYActionBehavior.ADD.ToString(), e, true);
            }
        }
        public string Approved(int id, bool ESS = false)
        {
            try
            {
                var ObjMatch = unitOfWork.Set<HRKPITracking>().Where(w => w.TranNo == id).FirstOrDefault();
                if (ObjMatch != null)
                {
                    if (ESS)
                    {
                        if (ObjMatch.DirectedByCode != User.UserName)
                        {
                            return "USER_NOT_APPROVOR";
                        }
                    }
                    string Approved = SYDocumentStatus.APPROVED.ToString();
                    ObjMatch.Status = Approved;
                    unitOfWork.Update(ObjMatch);
                    unitOfWork.Save();
                    CLsKPIAssign assign = new CLsKPIAssign();
                    assign.Calculate(ObjMatch.AssignCode);
                }
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, id.ToString(), SYActionBehavior.ADD.ToString(), e, true);
            }
            catch (DbUpdateException e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, id.ToString(), SYActionBehavior.ADD.ToString(), e, true);
            }
            catch (Exception e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, id.ToString(), SYActionBehavior.ADD.ToString(), e, true);
            }
        }
        public string Reject(int id, bool ESS = false)
        {
            try
            {
                var ObjMatch = unitOfWork.Set<HRKPITracking>().Where(w => w.TranNo == id).FirstOrDefault();
                if (ObjMatch != null)
                {
                    if (ESS)
                    {
                        if (ObjMatch.DirectedByCode != User.UserName)
                        {
                            return "USER_NOT_APPROVOR";
                        }
                    }
                    string Rejected = SYDocumentStatus.REJECTED.ToString();
                    ObjMatch.Status = Rejected;
                    unitOfWork.Update(ObjMatch);
                    unitOfWork.Save();
                }
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, id.ToString(), SYActionBehavior.ADD.ToString(), e, true);
            }
            catch (DbUpdateException e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, id.ToString(), SYActionBehavior.ADD.ToString(), e, true);
            }
            catch (Exception e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, id.ToString(), SYActionBehavior.ADD.ToString(), e, true);
            }
        }
        public virtual string OnGridModify(HRKPITimeSheet MModel, string Action)
        {
            if (MModel.FromTime.HasValue && MModel.ToTime.HasValue)
            {
                MModel.Hours = (decimal)MModel.ToTime.Value.Subtract(MModel.FromTime.Value).TotalHours;
            }
            if (Action == "ADD")
            {
                if (ListTimeSheet.Count == 0)
                {
                    MModel.LineItem = 1;
                }
                else
                {
                    int LineItem = ListTimeSheet.Max(w => w.LineItem);
                    MModel.LineItem = LineItem + 1;
                }
            }
            else if (Action == "EDIT")
            {
                var objCheck = ListTimeSheet.Where(w => w.LineItem == MModel.LineItem).FirstOrDefault();
                if (objCheck != null)
                {
                    ListTimeSheet.Remove(objCheck);
                }
                else
                {
                    return "INV_DOC";
                }
            }
            else if (Action == "DELETE")
            {
                var objCheck = ListTimeSheet.Where(w => w.LineItem == MModel.LineItem).FirstOrDefault();
                if (objCheck != null)
                {
                    ListTimeSheet.Remove(objCheck);
                    return SYConstant.OK;
                }
                else
                {
                    return "INV_DOC";
                }
            }
            var check = ListTimeSheet.Where(w => w.LineItem == MModel.LineItem).ToList();
            if (check.Count == 0)
            {
                ListTimeSheet.Add(MModel);
            }

            return SYConstant.OK;
        }

        public Dictionary<string, dynamic> OnDataSelectorLoading(params object[] keys)
        {
            string AssignCode = (string)keys[0];
            Dictionary<string, dynamic> keyValues = new Dictionary<string, dynamic>();

            keyValues.Add("KPITASK_LIST", unitOfWork.Set<HRKPIAssignItem>().Where(w => w.AssignCode == AssignCode).ToList());

            return keyValues;
        }
        public Dictionary<string, dynamic> OnDataStatusLoading(params object[] keys)
        {
            Dictionary<string, dynamic> keyValues = new Dictionary<string, dynamic>();

            keyValues.Add("STATUS_APPROVAL", new SYDataList("STATUS_LEAVE_APPROVAL").ListData.Where(w => w.SelectValue != "CANCELLED"));

            return keyValues;
        }
        //public string ImportKPITracking(List<Temp_Amount> ListTemp_Amount)
        //{
        //    try
        //    {
        //        var emp = DB.HRStaffProfiles.ToList();
        //        //var TranNo = DB.HRKPITrackings.OrderByDescending(u => u.TranNo).FirstOrDefault();

        //        foreach (var read in ListTemp_Amount)
        //        {
        //            var empcheck = emp.Where(w => w.EmpCode == read.EmpCode).ToList();

        //            if (empcheck.Count() == 0) return "Invalid EmpCode : " + read.EmpCode;

        //            if (read.DocumentDate > read.DocumentDate) return read.DocumentDate + " has invalid DocumentDate";
        //            //if (read.ToDate < read.FromDate) return read.EmpCode + " has invalid Todate";
        //            if (empcheck.Count > 0)
        //            {
        //                var obj = new HRKPITracking();
        //                obj.KPI = read.KPI;
        //                obj.KPIDescription = read.KPIDescription;
        //                obj.EmpCode = read.EmpCode;

        //                obj.CreatedOn = DateTime.Now;
        //                obj.CreatedBy = User.UserName;
        //                obj.DocumentDate = read.DocumentDate;
        //                obj.Actual = read.Amount;
        //                DB.HRKPITrackings.Add(obj);

        //            }
        //            else
        //            {
        //                MessageError = "EmpCode :" + read.EmpCode + " INVALID CODE: ";
        //                return "INV_DOC";
        //            }
        //        }

        //        int row = DB.SaveChanges();

        //        return SYConstant.OK;
        //    }
        //    catch (DbEntityValidationException e)
        //    {
        //        /*------------------SaveLog----------------------------------*/
        //        SYEventLog log = new SYEventLog();
        //        log.ScreenId = ScreenId;
        //        log.UserId = User.UserName;
        //        //log.DocurmentAction = HeaderKPITracking.TranNo.ToString();
        //        log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

        //        SYEventLogObject.saveEventLog(log, e);
        //        /*----------------------------------------------------------*/
        //        return "EE001";
        //    }
        //    catch (DbUpdateException e)
        //    {
        //        /*------------------SaveLog----------------------------------*/
        //        SYEventLog log = new SYEventLog();
        //        log.ScreenId = ScreenId;
        //        log.UserId = User.UserName;
        //        //log.DocurmentAction = HeaderKPITracking.TranNo.ToString();
        //        log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

        //        SYEventLogObject.saveEventLog(log, e, true);
        //        /*----------------------------------------------------------*/
        //        return "EE001";
        //    }
        //    catch (Exception e)
        //    {
        //        /*------------------SaveLog----------------------------------*/
        //        SYEventLog log = new SYEventLog();
        //        log.ScreenId = ScreenId;
        //        log.UserId = User.UserName;
        //        //log.DocurmentAction = HeaderKPITracking.TranNo.ToString();
        //        log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

        //        SYEventLogObject.saveEventLog(log, e, true);
        //        /*----------------------------------------------------------*/
        //        return "EE001";
        //    }
        //}
    }
    public class Temp_Amount
    {
        public string EmpCode { get; set; }
        public string KPI { get; set; }
        public string KPIDescription { get; set; }
        public DateTime DocumentDate { get; set; }
        public decimal Amount { get; set; }
    }
    public class ListKPI
    {
        public string Error { get; set; }
        public string EmpCode { get; set; }
        public string AllName { get; set; }
        public string Position { get; set; }
        public string Er_D_1 { get; set; }
        public string Er_D_2 { get; set; }
        public string Er_D_3 { get; set; }
        public string Er_D_4 { get; set; }
        public string Er_D_5 { get; set; }
        public string Er_D_6 { get; set; }
        public string Er_D_7 { get; set; }
        public string Er_D_8 { get; set; }
        public string Er_D_9 { get; set; }
        public string Er_D_10 { get; set; }
        public string Er_D_11 { get; set; }
        public string Er_D_12 { get; set; }
        public string Er_D_13 { get; set; }
        public string Er_D_14 { get; set; }
        public string Er_D_15 { get; set; }
        public string Er_D_16 { get; set; }
        public string Er_D_17 { get; set; }
        public string Er_D_18 { get; set; }
        public string Er_D_19 { get; set; }
        public string Er_D_20 { get; set; }
        public string Er_D_21 { get; set; }
        public string Er_D_22 { get; set; }
        public string Er_D_23 { get; set; }
        public string Er_D_24 { get; set; }
        public string Er_D_25 { get; set; }
        public string Er_D_26 { get; set; }
        public string Er_D_27 { get; set; }
        public string Er_D_28 { get; set; }
        public string Er_D_29 { get; set; }
        public string Er_D_30 { get; set; }
        public string Er_D_31 { get; set; }
        public string D_1 { get; set; }
        public string D_2 { get; set; }
        public string D_3 { get; set; }
        public string D_4 { get; set; }
        public string D_5 { get; set; }
        public string D_6 { get; set; }
        public string D_7 { get; set; }
        public string D_8 { get; set; }
        public string D_9 { get; set; }
        public string D_10 { get; set; }
        public string D_11 { get; set; }
        public string D_12 { get; set; }
        public string D_13 { get; set; }
        public string D_14 { get; set; }
        public string D_15 { get; set; }
        public string D_16 { get; set; }
        public string D_17 { get; set; }
        public string D_18 { get; set; }
        public string D_19 { get; set; }
        public string D_20 { get; set; }
        public string D_21 { get; set; }
        public string D_22 { get; set; }
        public string D_23 { get; set; }
        public string D_24 { get; set; }
        public string D_25 { get; set; }
        public string D_26 { get; set; }
        public string D_27 { get; set; }
        public string D_28 { get; set; }
        public string D_29 { get; set; }
        public string D_30 { get; set; }
        public string D_31 { get; set; }

    }
    public class ListAssign
    {
        public string AssignCode { get; set; }
        public string KPIType { get; set; }
        public string EmpCode { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public string HandlePerson { get; set; }
        public string Status { get; set; }
    }
}
