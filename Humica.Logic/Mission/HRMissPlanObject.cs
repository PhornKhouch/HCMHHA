using Humica.Core.DB;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic.CF;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;

namespace Humica.Logic.Mission
{
    public class HRMissPlanObject
    {
        public SMSystemEntity DBI = new SMSystemEntity();
        public HumicaDBContext DB = new HumicaDBContext();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public string ScreenId { get; set; }
        public HRMissionPlan Header { get; set; }
        public List<HRMissionPlan> ListHeader { get; set; }
        public List<HRMissionPlanItem> ListItem { get; set; }
        public List<HRMissionPlanMem> ListMember { get; set; }
        public List<ExDocApproval> ListApproval { get; set; }
        public string MessageCode { get; set; }

        public HRMissPlanObject()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }
        public HRStaffProfile getNextApprover(string id, string pro)
        {
            var objStaff = new HRStaffProfile();
            var DBX = new HumicaDBContext();
            var objHeader = DBX.HRMissionPlans.Find(id);
            if (objHeader == null)
            {
                return new HRStaffProfile();
            }
            string open = SYDocumentStatus.OPEN.ToString();
            var listCanApproval = DBX.ExDocApprovals.Where(w => w.DocumentNo == id && w.Status == open && w.DocumentType == objHeader.MissionType).ToList();

            if (listCanApproval.Count == 0)
            {
                return new HRStaffProfile();
            }

            var min = listCanApproval.Min(w => w.ApproveLevel);
            var NextApp = listCanApproval.Where(w => w.ApproveLevel == min).First();
            objStaff = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == NextApp.Approver);//, objHeader.Property);
            return objStaff;
        }
        public void SetAutoApproval(string DocType, string Branch, string SCREEN_ID)
        {
            ListApproval = new List<ExDocApproval>();
            var DBX = new HumicaDBContext();
            var objDoc = DBX.ExCfWorkFlowItems.Find(SCREEN_ID, DocType);
            if (objDoc != null)
            {
                if (objDoc.IsRequiredApproval == true)
                {
                    var listDefaultApproval = DBX.ExCfWFApprovers.Where(w => w.Branch == Branch && w.WFObject == objDoc.ApprovalFlow && w.IsSelected == true).ToList();
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

        public static IEnumerable<HRMissItem> GetAllMissItem()
        {
            HumicaDBContext DBB = new HumicaDBContext();
            List<HRMissItem> HRList = new List<HRMissItem>();
            string KPICode = HttpContext.Current.Session["Type"].ToString();
            if (KPICode != null && KPICode != "")
            {
                HRList = DBB.HRMissItems.Where(w => w.Type == KPICode).ToList();
            }
            return HRList;
        }
        public string CreateMissPlan()
        {
            try
            {
                DB = new HumicaDBContext();
                if (Header.IsDriver == true && Header.DriverName == null)
                {
                    return "DriverName";
                }
                var objCF = DB.ExCfWorkFlowItems.Find(ScreenId, Header.MissionType);
                if (objCF == null)
                {
                    return "REQUEST_TYPE_NE";
                }
                var objNumber = new CFNumberRank(Header.MissionType, ScreenId);
                if (objNumber.NextNumberRank == null)
                {
                    return "NUMBER_RANK_NE";
                }
                Header.MissionCode = objNumber.NextNumberRank;

                var Staff = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == Header.EmpCode);
                Header.PlannerName = Staff.AllName;
                Header.Status = SYDocumentStatus.OPEN.ToString();
                Header.ReStatus = SYDocumentStatus.OPEN.ToString();
                Header.CreatedBy = User.UserName;
                Header.CreatedOn = DateTime.Now;
                int lineitem = 1;
                Header.TotalAmount = 0;
                foreach (var item in ListItem)
                {
                    var read = new HRMissionPlanItem();
                    read = item;
                    read.LineItem = lineitem;
                    read.MissionCode = Header.MissionCode;
                    DB.HRMissionPlanItems.Add(read);
                    lineitem++;
                    Header.TotalAmount += item.Amount;
                }
                //Member
                foreach (var item in ListMember)
                {
                    var read = new HRMissionPlanMem();
                    read = item;
                    read.MissionCode = Header.MissionCode;
                    DB.HRMissionPlanMems.Add(read);
                }
                Header.Member = ListMember.Count();
                //Add approver
                foreach (var read in ListApproval)
                {
                    read.ID = 0;
                    read.LastChangedDate = DateTime.Now;
                    read.DocumentNo = Header.MissionCode;
                    read.DocumentType = Header.MissionType;
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
                DB.HRMissionPlans.Add(Header);

                DB.SaveChanges();

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Header.MissionCode;
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
                log.DocurmentAction = Header.MissionCode;
                log.Action = SYActionBehavior.ADD.ToString();
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
                log.DocurmentAction = Header.MissionCode;
                log.Action = SYActionBehavior.ADD.ToString();
                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string UpdateMPlan(string id)
        {
            try
            {
                DB = new HumicaDBContext();
                var ObjMatch = DB.HRMissionPlans.FirstOrDefault(w => w.MissionCode == id);

                if (ObjMatch == null)
                {
                    return "DOC_INV";
                }

                var checkdupListDpt = DB.HRMissionPlanItems.Where(w => w.MissionCode == id).ToList();

                foreach (var read in checkdupListDpt.ToList())
                {
                    DB.HRMissionPlanItems.Remove(read);
                }
                //Old Member
                var checkdMem = DB.HRMissionPlanMems.Where(w => w.MissionCode == id).ToList();
                foreach (var read in checkdMem.ToList())
                {
                    DB.HRMissionPlanMems.Remove(read);
                }

                var listApprovalDoc = DB.ExDocApprovals.Where(w => w.DocumentType == ObjMatch.MissionType && w.DocumentNo == ObjMatch.MissionCode).ToList();

                foreach (var read in listApprovalDoc)
                {
                    DB.ExDocApprovals.Attach(read);
                    DB.Entry(read).State = System.Data.Entity.EntityState.Deleted;
                }
                int lineitem = 1;
                ObjMatch.TotalAmount = 0;
                foreach (var item in ListItem)
                {
                    var read = new HRMissionPlanItem();
                    read = item;
                    read.LineItem = lineitem;
                    read.MissionCode = Header.MissionCode;
                    DB.HRMissionPlanItems.Add(read);
                    lineitem++;
                    ObjMatch.TotalAmount += item.Amount;
                }
                //Member
                foreach (var item in ListMember)
                {
                    var read = new HRMissionPlanMem();
                    read = item;
                    read.MissionCode = Header.MissionCode;
                    DB.HRMissionPlanMems.Add(read);
                    lineitem++;
                }
                Header.Member = ListMember.Count();
                var objCF = DB.ExCfWorkFlowItems.Find(ScreenId, Header.MissionType);
                if (objCF == null)
                {
                    return "REQUEST_TYPE_NE";
                }
                foreach (var read in ListApproval)
                {
                    read.ID = 0;
                    read.LastChangedDate = DateTime.Now;
                    read.DocumentNo = Header.MissionCode;
                    read.DocumentType = Header.MissionType;
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

                ObjMatch.ChangedBy = User.UserName;
                ObjMatch.ChangedOn = DateTime.Now;
                ObjMatch.FromDate = Header.FromDate;
                ObjMatch.ToDate = Header.ToDate;
                ObjMatch.Member = Header.Member;
                ObjMatch.TravelBy = Header.TravelBy;
                ObjMatch.Province = Header.Province;
                ObjMatch.IsDriver = Header.IsDriver;
                ObjMatch.Description = Header.Description;
                if (Header.IsDriver == true && Header.DriverName == null)
                {
                    return "DriverName";
                }
                ObjMatch.DriverName = Header.DriverName;
                ObjMatch.WorkingPlan = Header.WorkingPlan;

                DB.HRMissionPlans.Attach(ObjMatch);
                DB.Entry(ObjMatch).Property(x => x.Description).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.TotalAmount).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.FromDate).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ToDate).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Member).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.TravelBy).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Province).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.IsDriver).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.DriverName).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.WorkingPlan).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ChangedBy).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ChangedOn).IsModified = true;

                DB.SaveChanges();

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Header.MissionCode;
                log.Action = SYActionBehavior.EDIT.ToString();

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
                log.DocurmentAction = Header.MissionCode;
                log.Action = SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string deleteMPlan(string MissionCode)
        {
            try
            {
                DB = new HumicaDBContext();

                var ObjMatch = DB.HRMissionPlans.FirstOrDefault(w => w.MissionCode == MissionCode);

                if (ObjMatch == null)
                {
                    return "DOC_INV";
                }
                var ListPlanItem = DB.HRMissionPlanItems.Where(w => w.MissionCode == MissionCode).ToList();

                foreach (var read in ListPlanItem.ToList())
                {
                    DB.HRMissionPlanItems.Remove(read);
                }
                var ListPlanMem = DB.HRMissionPlanMems.Where(w => w.MissionCode == MissionCode).ToList();

                foreach (var read in ListPlanMem.ToList())
                {
                    DB.HRMissionPlanMems.Remove(read);
                }


                DB.HRMissionPlans.Remove(ObjMatch);
                DB.SaveChanges();

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = MissionCode;
                log.Action = SYActionBehavior.DELETE.ToString();

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
                log.DocurmentAction = MissionCode;
                log.Action = SYActionBehavior.DELETE.ToString();

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
                log.DocurmentAction = MissionCode;
                log.Action = SYActionBehavior.DELETE.ToString();

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
                var objMatch = DB.HRMissionPlans.FirstOrDefault(w => w.MissionCode == id);
                if (objMatch == null)
                {
                    return "REQUEST_NE";
                }
                Header = objMatch;

                if (objMatch.Status != SYDocumentStatus.OPEN.ToString())
                {
                    return "INV_DOC";
                }

                objMatch.Status = SYDocumentStatus.PENDING.ToString();
                DB.HRMissionPlans.Attach(objMatch);
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
                log.ScreenId = Header.MissionCode;
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
                log.DocurmentAction = Header.MissionCode;
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
                log.DocurmentAction = Header.MissionCode;
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
                var objMatch = DB.HRMissionPlans.FirstOrDefault(w => w.MissionCode == id);
                if (objMatch == null)
                {
                    return "REQUEST_NE";
                }
                Header = objMatch;

                if (objMatch.Status != SYDocumentStatus.PENDING.ToString())
                {
                    return "INV_DOC";
                }
                string open = SYDocumentStatus.OPEN.ToString();
                var listApproval = DBX.ExDocApprovals.Where(w => w.DocumentType == objMatch.MissionType
                                    && w.DocumentNo == objMatch.MissionCode && w.Status == open).OrderBy(w => w.ApproveLevel).ToList();
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
                DB.HRMissionPlans.Attach(objMatch);
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
                log.ScreenId = Header.MissionCode;
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
                log.DocurmentAction = Header.MissionCode;
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
                log.DocurmentAction = Header.MissionCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string CancelDoc(string ApprovalID)
        {
            try
            {
                string cancelled = SYDocumentStatus.CANCELLED.ToString();
                var objmatch = DB.HRMissionPlans.Find(ApprovalID);
                if (objmatch == null)
                {
                    return "REQUEST_NE";
                }
                if (objmatch.Status != cancelled)
                {
                    objmatch.Status = cancelled;
                    objmatch.ReStatus = cancelled;
                    DB.Entry(objmatch).Property(w => w.Status).IsModified = true;
                    DB.Entry(objmatch).Property(w => w.ReStatus).IsModified = true;
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
    }
}
