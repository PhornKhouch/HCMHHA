using Humica.Core.CF;
using Humica.Core.DB;
using Humica.Core.FT;
using Humica.Core.SY;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;

namespace Humica.Performance
{
    public class ClsKPIAchieve
    {
        public HumicaDBContext DB = new HumicaDBContext();
        public string ScreenId { get; set; }
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public string ErrorMessage { get; set; }
        public FTDGeneralAccount Filter { get; set; }
        public HRKPIAchieveReview HeaderAchieve { get; set; }
        public HRKPIAssignItem AssignItem { get; set; }
        public List<HRKPIAchieveReview> ListAchieveHeader { get; set; }
        public List<HRKPIAchieveItem> listAchieveItem { get; set; }
        public List<HRKPIAssignHeader> ListAssignPending { get; set; }
        public List<HRKPIAssignMember> ListKPIMember { get; set; }
        public List<ListAssignItem> ListTask { get; set; }
        public List<ListAssignItem> ListTaskPending { get; set; }
        public List<ListAssignItem> ListTaskCompleted { get; set; }
        public List<HRKPITracking> ListKPITracking { get; set; }
        public List<ClsTaskSummary> ListtaskSummary { get; set; }

        public ClsKPIAchieve()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }
        public async Task LoadDataEmpKPI(bool ESS = false)
        {
            string RELEASED = SYDocumentStatus.RELEASED.ToString();
            ListAssignPending = await DB.HRKPIAssignHeaders.Where(w => w.Status == RELEASED).ToListAsync();
            if (ESS == true)
            {
                string UserName = User.UserName;
                ListAssignPending = ListAssignPending.Where(w => w.DirectedByCode == UserName).ToList();
            }
        }
        public async Task<List<HRKPIAchieveReview>> LoadDataAssign(string AssignedBy, bool ESS = false)
        {
            List<HRKPIAchieveReview> _List = await DB.HRKPIAchieveReviews.Where(w => w.AssignedBy == AssignedBy).ToListAsync();
            if (ESS == true)
            {
                string UserName = User.UserName;
                _List = _List.Where(w => w.EmpCode == UserName || w.HandleCode == UserName
                || w.DirectedByCode == UserName).ToList();
            }
            return _List;
        }
        public async Task<List<ListAssign>> LoadData()
        {
            using (var context = new HumicaDBContext())
            {
                // Retrieve all members
                var members = await context.HRKPIAssignMembers.ToListAsync();
                string Approve = SYDocumentStatus.APPROVED.ToString();
                // Retrieve assignments
                var assignments = await (from assign in context.HRKPIAssignHeaders
                                         where assign.Status == Approve
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
                                             Status = "Individual",
                                             HandlePerson = assign.HandleCode + " : " + assign.HandleName
                                         }).ToListAsync();

                var result = new List<ListAssign>();

                foreach (var assignment in assignments)
                {
                    result.Add(assignment);
                    var relatedMembers = members.Where(m => m.AssignCode == assignment.AssignCode).ToList();

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
                            Status = "ByTeam"
                        };

                        result.Add(memberAssignment);
                    }
                }

                return result;
            }
        }
        public async Task<List<ListAssignItem>> LoadDataTask()
        {
            try
            {
                using (var context = new HumicaDBContext())
                {
                    // Retrieve all members
                    string approveStatus = SYDocumentStatus.APPROVED.ToString();
                    // Retrieve assignments
                    var assignments = await (from assign in context.HRKPIAssignHeaders
                                             join item in context.HRKPIAssignItems on assign.AssignCode equals item.AssignCode
                                             where assign.Status == approveStatus
                                             select new ListAssignItem
                                             {
                                                 AssignCode = assign.AssignCode,
                                                 EmpCode = assign.HandleCode,
                                                 EmployeeName = assign.HandleName,
                                                 Position = assign.Position,
                                                 Department = assign.Department,
                                                 KPIType = assign.KPIType,
                                                 PeriodFrom = assign.PeriodFrom.HasValue ? assign.PeriodFrom.Value : DateTime.MinValue,
                                                 PeriodTo = assign.PeriodTo.HasValue ? assign.PeriodTo.Value : DateTime.MinValue,
                                                 HandlePerson = assign.HandleCode + " : " + assign.HandleName,
                                                 ItemCode = item.ItemCode,
                                                 KPI = item.KPI,
                                                 Measure = item.Measure,
                                                 Weight = item.Weight,
                                                 Traget = item.Target.HasValue ? item.Target.Value : 0,
                                                 Actual = item.Actual.HasValue ? item.Actual.Value : 0,
                                                 Achievement = item.Acheivement.HasValue ? item.Acheivement.Value : 0,
                                                 StartDate = item.StartDate.HasValue ? item.StartDate.Value : DateTime.MinValue,
                                                 EndDate = item.EndDate.HasValue ? item.EndDate.Value : DateTime.MinValue,
                                                 Status = item.Status
                                             }).ToListAsync();

                    return assignments;
                }
            }
            catch (Exception ex)
            {
                // Log the exception (you can use your logging framework here)
                Console.WriteLine($"An error occurred: {ex.Message}");
                // Consider how you want to handle errors, for now, we'll return an empty list
                return new List<ListAssignItem>();
            }
        }
        public async Task<List<ClsTaskSummary>> LoadDataTaskSummary(string Id, string ItemCode, decimal Target)
        {
            try
            {
                using (var context = new HumicaDBContext())
                {
                    var _ListKPITracking = await DB.HRKPITrackings.Where(w => w.AssignCode == Id && w.KPI == ItemCode).ToListAsync();
                    List<ClsTaskSummary> assignments = new List<ClsTaskSummary>();
                    foreach (var item in _ListKPITracking)
                    {
                        if (assignments.Where(w => w.EmpCode == item.EmpCode).Count() > 0)
                        {
                            assignments.Where(w => w.EmpCode == item.EmpCode).ToList().ForEach(w => w.Actual += item.Actual);
                            assignments.Where(w => w.EmpCode == item.EmpCode).ToList().ForEach(w => w.Achievement = w.Actual / Target);
                        }
                        else
                        {
                            var obj = new ClsTaskSummary();
                            obj.EmpCode = item.EmpCode;
                            obj.EmployeeName = item.EmpName;
                            obj.Position = item.Position;
                            obj.Actual = item.Actual;
                            obj.Achievement = item.Actual / Target;
                            assignments.Add(obj);
                        }
                    }
                    return assignments;
                }
            }
            catch (Exception ex)
            {
                return new List<ClsTaskSummary>();
            }
        }
        public async Task<List<HRKPIList>> LoadDataHRKPIList(string Department)
        {
            List<HRKPIList> _list = new List<HRKPIList>();
            _list = await DB.HRKPILists.Where(w => string.IsNullOrEmpty(w.Department)
            || w.Department == Department).ToListAsync();
            return _list;
        }

        public async Task<HRKPIAchieveReview> Get_EmpKPI(string KPICode)
        {
            HeaderAchieve = new HRKPIAchieveReview();
            var Plan = await DB.HRKPIAssignHeaders.FirstOrDefaultAsync(w => w.AssignCode == KPICode);
            if (Plan != null)
            {
                HeaderAchieve.DocRef = Plan.AssignCode;
                HeaderAchieve.DocumentDate = DateTime.Now;
                HeaderAchieve.PeriodFrom = Plan.PeriodFrom;
                HeaderAchieve.PeriodTo = Plan.PeriodTo;
                HeaderAchieve.TotalWeight = Plan.TotalWeight;
                HeaderAchieve.KPIAverage = Plan.KPIAverage;
                HeaderAchieve.CriteriaType = Plan.CriteriaType;
                HeaderAchieve.KPIType = Plan.KPIType;
                HeaderAchieve.Department = Plan.Department;
                HeaderAchieve.HandleCode = Plan.HandleCode;
                HeaderAchieve.HandleName = Plan.HandleName;
                HeaderAchieve.Position = Plan.Position;
                HeaderAchieve.TotalScore = Plan.TotalScore;

                var ListPlanItem = await DB.HRKPIAssignItems.Where(w => w.AssignCode == KPICode).ToListAsync();
                listAchieveItem = new List<HRKPIAchieveItem>();
                foreach (var item in ListPlanItem)
                {
                    var obj = new HRKPIAchieveItem();
                    obj.Indicator = item.Indicator;
                    obj.Measure = item.Measure;
                    obj.ItemCode = item.ItemCode;
                    obj.KPI = item.KPI;
                    obj.ActionPlan = item.ActionPlan;
                    obj.Remark = item.Remark;
                    obj.Target = item.Target;
                    obj.Weight = item.Weight;
                    obj.StartDate = item.StartDate;
                    obj.EndDate = item.EndDate;
                    listAchieveItem.Add(obj);
                }
            }
            HeaderAchieve.Status = SYDocumentStatus.OPEN.ToString();

            return HeaderAchieve;
        }
        public async Task<HRKPIAchieveReview> getSetupHeader(string id)
        {
            listAchieveItem = new List<HRKPIAchieveItem>();
            ListKPIMember = new List<HRKPIAssignMember>();
            HeaderAchieve = await DB.HRKPIAchieveReviews.FirstOrDefaultAsync(w => w.Code == id);
            if (HeaderAchieve != null)
            {
                listAchieveItem = await DB.HRKPIAchieveItems.Where(w => w.Code == id).ToListAsync();
                ListKPIMember = await DB.HRKPIAssignMembers.Where(w => w.AssignCode == id).ToListAsync();
                HeaderAchieve.TotalScore = 0;
                HeaderAchieve.EmpCode = "";
                HeaderAchieve.HandleCode = "";
                HeaderAchieve.EmpName = "";
                HeaderAchieve.PICPosition = "";
                return HeaderAchieve;
            }
            return null;
        }
        public async Task<string> CreateAssign(string Screen_Id)
        {
            DB = new HumicaDBContext();
            try
            {
                DB.Configuration.AutoDetectChangesEnabled = false;
                var ObjPlan = await DB.HRKPIPlanIndividuals.FirstOrDefaultAsync(w => w.Code == HeaderAchieve.DocRef);
                var objCF = await DB.ExCfWorkFlowItems.FirstOrDefaultAsync(w => w.ScreenID == Screen_Id);
                var Staff = await DB.HRStaffProfiles.FirstOrDefaultAsync(w => w.EmpCode == HeaderAchieve.HandleCode);
                if (objCF == null)
                {
                    return "REQUEST_TYPE_NE";
                }
                if (HeaderAchieve.HandleCode == null)
                {
                    return "DOC_INV";
                }
                if (string.IsNullOrEmpty(HeaderAchieve.KPIType))
                {
                    return "DOC_INV";
                }
                if (listAchieveItem.Count() == 0)
                {
                    return "DOC_INV";
                }
                if (string.IsNullOrEmpty(HeaderAchieve.EmpCode) && HeaderAchieve.AssignedBy != SYDocumentASSIGN.Individual.ToString())
                {
                    return "EMPCODE_EN";
                }
                var objNumber = new CFNumberRank(objCF.DocType, objCF.ScreenID);
                if (objNumber == null)
                {
                    return "NUMBER_RANK_NE";
                }
                HeaderAchieve.Code = objNumber.NextNumberRank;
                foreach (var read in listAchieveItem.ToList())
                {
                    read.Code = HeaderAchieve.Code;
                    read.EmpCode = HeaderAchieve.EmpCode;
                    read.Status = HeaderAchieve.Status;
                    DB.HRKPIAchieveItems.Add(read);
                }
                HeaderAchieve.ScreenID = ScreenId;
                HeaderAchieve.DirectedByCode = ObjPlan.PlanerCode;
                HeaderAchieve.DirectedByName = ObjPlan.PlanerName;
                HeaderAchieve.CreatedBy = User.UserName;
                HeaderAchieve.CreatedOn = DateTime.Now;

                if (ListKPIMember != null)
                {
                    foreach (var read in ListKPIMember.ToList())
                    {
                        read.AssignCode = HeaderAchieve.Code;
                        DB.HRKPIAssignMembers.Add(read);
                    }
                }
                ObjPlan.Status = SYDocumentStatus.COMPLETED.ToString();
                DB.Entry(ObjPlan).Property(w => w.Status).IsModified = true;

                if (Staff != null)
                {
                    HeaderAchieve.HandleName = Staff.AllName;
                }
                DB.HRKPIAchieveReviews.Add(HeaderAchieve);
                int row = await DB.SaveChangesAsync();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                //log.DocurmentAction = Header.EmpID;
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
                //log.DocurmentAction = Header.EmpID;
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

        public async Task<string> EditAssignKPI(string id)
        {
            DB = new HumicaDBContext();
            using (var DBM = new HumicaDBContext())
            {
                using (var transaction = DBM.Database.BeginTransaction())
                {
                    try
                    {
                        DBM.Configuration.AutoDetectChangesEnabled = false;
                        var objMatch = await DB.HRKPIAssignHeaders.FirstOrDefaultAsync(w => w.AssignCode == id);
                        var listAssignItem = await DB.HRKPIAssignItems.Where(w => w.AssignCode == id).ToListAsync();
                        var listAssignMember = await DB.HRKPIAssignMembers.Where(w => w.AssignCode == id).ToListAsync();

                        if (objMatch == null)
                        {
                            return "INV_DOC";
                        }

                        foreach (var read in listAssignItem)
                        {
                            DBM.HRKPIAssignItems.Attach(read);
                            DBM.HRKPIAssignItems.Remove(read);
                        }
                        foreach (var read in listAssignMember)
                        {
                            DBM.HRKPIAssignMembers.Attach(read);
                            DBM.HRKPIAssignMembers.Remove(read);
                        }
                        foreach (var read in listAchieveItem.ToList())
                        {
                            read.Status = HeaderAchieve.Status;
                            read.Code = HeaderAchieve.Code;
                            DBM.HRKPIAchieveItems.Add(read);
                        }
                        if (ListKPIMember != null)
                        {
                            foreach (var read in ListKPIMember.ToList())
                            {
                                read.AssignCode = HeaderAchieve.Code;
                                DBM.HRKPIAssignMembers.Add(read);
                            }
                        }
                        var Staff = await DB.HRStaffProfiles.FirstOrDefaultAsync(w => w.EmpCode == HeaderAchieve.HandleCode);
                        if (Staff != null)
                        {
                            HeaderAchieve.HandleName = Staff.AllName;
                        }
                        HeaderAchieve.ScreenID = objMatch.ScreenID;
                        HeaderAchieve.DirectedByCode = objMatch.DirectedByCode;
                        HeaderAchieve.DirectedByName = objMatch.DirectedByName;
                        HeaderAchieve.CriteriaType = objMatch.CriteriaType;
                        HeaderAchieve.CreatedOn = objMatch.CreatedOn;
                        HeaderAchieve.CreatedBy = objMatch.CreatedBy;
                        HeaderAchieve.ChangedBy = User.UserName;
                        HeaderAchieve.ChangedOn = DateTime.Now;
                        DBM.Entry(HeaderAchieve).State = System.Data.Entity.EntityState.Modified;
                        int row = await DBM.SaveChangesAsync();
                        transaction.Commit();
                        return SYConstant.OK;
                    }
                    catch (DbEntityValidationException e)
                    {
                        /*------------------SaveLog----------------------------------*/
                        SYEventLog log = new SYEventLog();
                        log.ScreenId = ScreenId;
                        log.UserId = User.UserName;
                        log.DocurmentAction = HeaderAchieve.Code;
                        log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                        SYEventLogObject.saveEventLog(log, e);
                        /*----------------------------------------------------------*/
                        return "EE001";
                    }
                    finally
                    {
                        DBM.Configuration.AutoDetectChangesEnabled = true;
                    }
                }
            }
        }
        public string RequestRelease(string id)
        {
            try
            {
                HumicaDBContext DBM = new HumicaDBContext();
                var objMatch = DBM.HRKPIAchieveReviews.FirstOrDefault(w => w.Code == id);
                if (objMatch == null)
                {
                    return "REQUEST_NE";
                }
                HeaderAchieve = objMatch;

                if (objMatch.Status != SYDocumentStatus.OPEN.ToString())
                {
                    return "INV_DOC";
                }
                objMatch.Status = SYDocumentStatus.PENDING.ToString();
                DBM.HRKPIAchieveReviews.Attach(objMatch);
                DBM.Entry(objMatch).Property(w => w.Status).IsModified = true;
                int row = DBM.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = HeaderAchieve.Code;
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
                log.DocurmentAction = HeaderAchieve.Code;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string Approved(string id)
        {
            try
            {
                HumicaDBContext DBI = new HumicaDBContext();

                var objmatch = DB.HRKPIAssignHeaders.FirstOrDefault(w => w.AssignCode == id);

                if (objmatch == null) return "DOC_INV";
                if (objmatch.Status != SYDocumentStatus.PENDING.ToString())
                {
                    return "INV_DOC";
                }
                string Open = SYDocumentStatus.OPEN.ToString();
                var listApproval = DB.ExDocApprovals.Where(w => w.DocumentType == objmatch.CriteriaType
                                    && w.DocumentNo == objmatch.AssignCode && w.Status == Open).OrderBy(w => w.ApproveLevel).ToList();
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
                        var objStaff = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == read.Approver);
                        if (objStaff != null)
                        {
                            if (listApproval.Where(w => w.ApproveLevel <= read.ApproveLevel).Count() >= listApproval.Count())
                            {
                                listApproval.ForEach(w => w.Status = SYDocumentStatus.APPROVED.ToString());
                            }
                            //StaffProfile = objStaff;
                            read.ApprovedBy = objStaff.EmpCode;
                            read.ApprovedName = objStaff.AllName;
                            read.LastChangedDate = DateTime.Now.Date;
                            read.ApprovedDate = DateTime.Now;
                            read.Status = SYDocumentStatus.APPROVED.ToString();
                            DB.ExDocApprovals.Attach(read);
                            DB.Entry(read).State = System.Data.Entity.EntityState.Modified;
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

                var Status = SYDocumentStatus.APPROVED.ToString();

                if (listApproval.Where(w => w.Status == Open).ToList().Count > 0)
                {
                    Status = SYDocumentStatus.PENDING.ToString();
                }

                objmatch.Status = Status;
                objmatch.ApprovedBy = User.UserName;

                DB.HRKPIAssignHeaders.Attach(objmatch);

                DB.Entry(objmatch).Property(w => w.Status).IsModified = true;
                DB.Entry(objmatch).Property(w => w.ApprovedBy).IsModified = true;

                DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = id;
                log.Action = SYActionBehavior.RELEASE.ToString();

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
                log.Action = SYActionBehavior.RELEASE.ToString();

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
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public async Task<string> ReleaseTheDoc(string id)
        {
            string ID = id;
            try
            {
                var objMatch = await DB.HRKPIAchieveReviews.FirstOrDefaultAsync(w => w.Code == id);
                if (objMatch == null)
                {
                    return "REQUEST_NE";
                }
                HeaderAchieve = objMatch;

                if (objMatch.Status != SYDocumentStatus.OPEN.ToString())
                {
                    return "INV_DOC";
                }
                objMatch.Status = SYDocumentStatus.RELEASED.ToString();

                DB.Entry(objMatch).Property(w => w.Status).IsModified = true;
                DB.SaveChanges();

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                await ClsEventLog.SaveEventLog(ScreenId, User.UserName, ID, SYActionBehavior.ADD.ToString(), e, true);
                return "EE001";
            }
            catch (DbUpdateException e)
            {
                await ClsEventLog.SaveEventLog(ScreenId, User.UserName, ID, SYActionBehavior.ADD.ToString(), e, true);
                return "EE001";
            }
            catch (Exception e)
            {
                await ClsEventLog.SaveEventLog(ScreenId, User.UserName, ID, SYActionBehavior.ADD.ToString(), e, true);
                return "EE001";
            }
        }
        public async Task<string> CompeledDoc(string id, string KPI)
        {
            try
            {
                HumicaDBContext DBI = new HumicaDBContext();

                var objmatch = await DB.HRKPIAssignItems.FirstOrDefaultAsync(w => w.AssignCode == id && w.ItemCode == KPI);

                if (objmatch == null) return "DOC_INV";
                if (objmatch.Status != SYDocumentStatus.OPEN.ToString())
                {
                    return "INV_DOC";
                }

                var Status = SYDocumentStatus.COMPLETED.ToString();

                objmatch.Status = Status;
                DB.HRKPIAssignItems.Attach(objmatch);
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
                log.DocurmentAction = id;
                log.Action = SYActionBehavior.RELEASE.ToString();

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
                log.Action = SYActionBehavior.RELEASE.ToString();

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
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }

        public string CalulateKPI(string id)
        {
            DB = new HumicaDBContext();
            try
            {
                string Status = SYDocumentStatus.APPROVED.ToString();
                var ListKPITracking = DB.HRKPITrackings.Where(w => w.AssignCode == id && w.Status == Status).ToList();
                var ListKPITask = DB.HRKPIAssignItems.Where(w => w.AssignCode == id).ToList();
                foreach (var item in ListKPITask)
                {
                    item.Acheivement = 0;
                    item.Actual = 0;
                    item.Score = 0;
                    var ListTrack = ListKPITracking.Where(w => w.KPI == item.ItemCode).ToList();
                    if (ListTrack.Count > 0)
                    {
                        var Amount = ListTrack.Sum(w => w.Actual);
                        item.Actual += Amount;
                        item.Acheivement = item.Actual / item.Target;
                    }
                    DB.HRKPIAssignItems.Attach(item);
                    DB.Entry(item).Property(w => w.Acheivement).IsModified = true;
                    DB.Entry(item).Property(w => w.Actual).IsModified = true;
                    DB.Entry(item).Property(w => w.Score).IsModified = true;
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
                log.DocurmentAction = id;
                log.Action = SYActionBehavior.RELEASE.ToString();

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
                log.Action = SYActionBehavior.RELEASE.ToString();

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
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }

        }
        public static string Calculate(string id)
        {
            var DBU = new HumicaDBContext();
            string Status = SYDocumentStatus.APPROVED.ToString();
            var ListKPITracking = new HumicaDBContext().HRKPITrackings.Where(w => w.AssignCode == id && w.Status == Status).ToList();
            var ListKPITask = new HumicaDBContext().HRKPIAssignItems.Where(w => w.AssignCode == id).ToList();
            var ASHeader = new HumicaDBContext().HRKPIAssignHeaders.FirstOrDefault(w => w.AssignCode == id);
            decimal Score = 0;
            foreach (var item in ListKPITask)
            {
                item.Acheivement = 0;
                item.Actual = 0;
                item.Score = 0;
                var ListTrack = ListKPITracking.Where(w => w.KPI == item.ItemCode).ToList();
                if (ListTrack.Count > 0)
                {
                    var Amount = ListTrack.Sum(w => w.Actual);
                    //var rating = KPIRating.FirstOrDefault(w => w.KPITask == item.ItemCode && Convert.ToDecimal(w.Minimum) <= Amount && Convert.ToDecimal(w.Maximum) >= Amount);
                    //if (rating != null)
                    //{
                    //    //item.Actual = rating.Rating;
                    item.Score = item.Weight * Amount;
                    //}
                    item.Actual += Amount;
                    if (item.Target > 0)
                        item.Acheivement = item.Actual / item.Target;
                }
                Score += item.Score.Value;

                DBU.HRKPIAssignItems.Attach(item);
                DBU.Entry(item).Property(w => w.Acheivement).IsModified = true;
                DBU.Entry(item).Property(w => w.Actual).IsModified = true;
                DBU.Entry(item).Property(w => w.Score).IsModified = true;
            }
            if (ASHeader != null)
            {
                ASHeader.TotalScore = Score;
                DBU.HRKPIAssignHeaders.Attach(ASHeader);
                DBU.Entry(ASHeader).Property(w => w.TotalScore).IsModified = true;
            }
            DBU.SaveChanges();
            return SYConstant.OK;
        }

        public async Task<string> ModifyItem(HRKPIAchieveItem MModel, string Action)
        {
            decimal? TotalWeight = 0;

            if (Action == "ADD")
            {
                TotalWeight = listAchieveItem.Sum(w => w.Weight) + MModel.Weight;
            }
            else if (Action == "EDIT")
            {
                var objCheck = listAchieveItem.Where(w => w.ItemCode == MModel.ItemCode).FirstOrDefault();
                if (objCheck != null)
                {
                    listAchieveItem.Remove(objCheck);
                }
                else
                {
                    return "INV_DOC";
                }
                TotalWeight = listAchieveItem.Sum(w => w.Weight) + MModel.Weight;
            }
            else if (Action == "DELETE")
            {
                var objCheck = listAchieveItem.Where(w => w.ItemCode == MModel.ItemCode).FirstOrDefault();
                if (objCheck != null)
                {
                    listAchieveItem.Remove(objCheck);
                    return SYConstant.OK;
                }
                else
                {
                    return "INV_DOC";
                }
            }
            if (MModel.Weight == 0)
            {
                return "WEIGHT_NOT_0";
            }
            else if (MModel.Weight <= 0 || MModel.Weight > 1)
            {
                return "INV_AMOUNT";
            }
            else if (TotalWeight > 1)
            {
                return "KPI_Percent";
            }
            var check = listAchieveItem.Where(w => w.ItemCode == MModel.ItemCode).ToList();
            if (check.Count == 0)
            {
                listAchieveItem.Add(MModel);
            }
            else
            {
                return "ISDUPLICATED";
            }
            return SYConstant.OK;
        }
        public async Task<string> ModifyMember(HRKPIAssignMember MModel, string Action)
        {
            if (Action == "EDIT")
            {
                var objCheck = ListKPIMember.Where(w => w.EmpCode == MModel.EmpCode).FirstOrDefault();
                if (objCheck != null)
                {
                    ListKPIMember.Remove(objCheck);
                }
                else
                {
                    return "INV_DOC";
                }
            }
            else if (Action == "DELETE")
            {
                var objCheck = ListKPIMember.Where(w => w.EmpCode == MModel.EmpCode).FirstOrDefault();
                if (objCheck != null)
                {
                    ListKPIMember.Remove(objCheck);
                    return SYConstant.OK;
                }
                else
                {
                    return "INV_DOC";
                }
            }

            var check = ListKPIMember.Where(w => w.EmpCode == MModel.EmpCode).ToList();
            if (check.Count == 0)
            {
                ListKPIMember.Add(MModel);
            }
            else
            {
                return "ISDUPLICATED";
            }
            return SYConstant.OK;
        }
    }

}
