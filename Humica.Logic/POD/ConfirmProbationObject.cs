using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.EF;
using Humica.Logic.CF;
using Humica.Core.Process;

namespace Humica.Logic.POD
{
    public class ConfirmProbationObject
    {
        public DBBusinessProcess DB = new DBBusinessProcess();
        DBSystemHREntities DH = new DBSystemHREntities();
        public SYUser User { get; set; }
        public string ScreenId { get; set; }
        public SYUserBusiness BS { get; set; }
        public HRComfirmProbation Header { get; set; }
        public HRStaffProfile Staff { get; set; } 
        public List<ExDocApproval> ListApprove { get; set; }
        //public List<ExCfWFSalaryApprover> ListApproval { get; set; }
        public List<HRComfirmProbation> ListHeader { get; set; }
       public List<HRStaffProfile> ListStaff { get; set; }

        public HR_STAFF_VIEW HeaderStaff { get; set; }
        public ConfirmProbationObject()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }
        //public string CreateComProbation()
        //{
        //    try
        //    {
        //        Header.EmpCode = Staff.EmpCode;
        //        Header.CreateBy = User.UserName;
        //        Header.CreateOn = DateTime.Now;
        //        DB.HRComfirmProbations.Add(Header);
        //        int row = DB.SaveChanges();
        //        return SYConstant.OK;
        //    }
        //    catch (DbEntityValidationException e)
        //    {
        //        /*------------------SaveLog----------------------------------*/
        //        SYEventLog log = new SYEventLog();
        //        log.ScreenId = ScreenId;
        //        log.UserId = User.UserName;
        //        log.DocurmentAction = Header.EmpCode;
        //        log.Action = SystemAdmin.SYActionBehavior.ADD.ToString();

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
        //        log.DocurmentAction = Header.EmpCode;
        //        log.Action = SystemAdmin.SYActionBehavior.ADD.ToString();

        //        SYEventLogObject.saveEventLog(log, e, true);
        //        /*----------------------------------------------------------*/
        //        return "EE001";
        //    }
        //}
        public string CreateProbation(string id)
        {
            try
            {

                //var DBR = new DBBusinessProcess();
                var objCF = DH.ExCfWorkFlowItems.Find(ScreenId, Header.DocType);
                if (objCF == null)
                {
                    return "REQUEST_TYPE_NE";
                }

                var objNumber = new CFNumberRank(Header.DocType, ScreenId);
                if (objNumber == null)
                {
                    return "NUMBER_RANK_NE";
                }
               
                Header.CPID = objNumber.NextNumberRank.Trim();
                Header.Status = SYDocumentStatus.OPEN.ToString();
                foreach (var read in ListApprove)
                {
                    read.DocumentNo = Header.CPID;
                    read.ID = 0;
                    read.Status = SYDocumentStatus.OPEN.ToString();
                    read.WFObject = objCF.ApprovalFlow;
                    read.ApprovedName = "";
                    read.ApproverName = "";
                    read.ApprovedBy = "";
                    var objStaff = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == read.Approver);
                    if (objStaff != null)
                    {
                        read.ApproverName = objStaff.AllName;
                    }
                    DB.ExDocApprovals.Add(read);
                }
                //var obj_ = DB.HRComfirmProbations.FirstOrDefault(w => w.EmpCode == id && w.CPID==Header.CPID);
                //Header.EmpCode = id;
                Header.CreateBy = User.UserName;
                Header.CreateOn = DateTime.Now;
                DB.HRComfirmProbations.Add(Header);
                int row1 = DB.SaveChanges();
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
        }

        public string EditProbation(string id)
        {
            try
            {

                var objMatch = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == id.ToString());
                if (objMatch == null)
                {
                    return "EE001";
                }
                Header.EmpCode = objMatch.EmpCode;
                Header.EmpName = objMatch.AllName;
                Header.Branch = objMatch.Branch;
                Header.Position = objMatch.JobCode;
                Header.Department = objMatch.DEPT;
                Header.LevelCode = objMatch.LevelCode;
                Header.StartDate = objMatch.StartDate;
                Header.Probation = objMatch.Probation;
                Header.Salary = objMatch.Salary;
                Header.CPID = "CP01";
                Header.DocType = "SALARY";
                Header.Status = objMatch.Status;
                Header.CreateBy = User.UserName;
                Header.CreateOn = DateTime.Now;

                DB.HRComfirmProbations.Attach(Header);
                int row1 = DB.SaveChanges();
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
        }

        public string DeleteProbation(string id)
        {
            try
            {
                Header = new HRComfirmProbation();
                var objMatch = DB.HRComfirmProbations.FirstOrDefault(w => w.EmpCode == id);
                var _obj = DB.ExDocApprovals.Where(w => w.DocumentNo == objMatch.CPID).ToList();
                foreach(var read in _obj)
                {
                    DB.ExDocApprovals.Attach(read);
                    DB.Entry(read).State = System.Data.Entity.EntityState.Deleted;
                    DB.SaveChanges();
                }
                if (objMatch == null)
                {
                    return "FAMILY_NE";
                }
                Header.EmpCode = id;
                DB.HRComfirmProbations.Attach(objMatch);
                DB.Entry(objMatch).State = System.Data.Entity.EntityState.Deleted;
                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserID.ToString();
                log.ScreenId = Header.EmpCode.ToString();
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
                log.ScreenId = Header.EmpCode.ToString();
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }

        }
        public string requestToApprove(string id)
        {
            try
            {
                PODEnity DBX = new PODEnity();
                var objMatch = DB.HRComfirmProbations.FirstOrDefault(w=>w.EmpCode==id);
                if (objMatch == null)
                {
                    return "REQUEST_NE";
                }
                Header = objMatch;

                if (objMatch.Status != SYDocumentStatus.OPEN.ToString())
                {
                    return "DOC_EXIST";
                }

                objMatch.Status = SYDocumentStatus.PENDING.ToString();
                DB.HRComfirmProbations.Attach(objMatch);
                DB.Entry(objMatch).Property(w => w.Status).IsModified = true;
                DB.SaveChanges();

                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.ScreenId = ScreenId;
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

        public string CancelAppSalary(string ASNumber)
        {
            try
            {
                string approved = SYDocumentStatus.CANCELLED.ToString();
                var objmatch = DB.HRComfirmProbations.FirstOrDefault(w => w.CPID == ASNumber);
                if (objmatch == null)
                {
                    return "INV_EN";
                }
                //var objUpdate = DB.HRComfirmProbations.FirstOrDefault(w => w.InYear == objmatch.InYear && w.InMonth == objmatch.InMonth);
                //if (objUpdate != null)
                //{
                //    objUpdate.IsLock = false;
                //    DB.HRComfirmProbations.Attach(objUpdate);
                //    DB.Entry(objUpdate).Property(w => w.IsLock).IsModified = true;
                //}
                objmatch.Status = approved;
                objmatch.ChangeOn = DateTime.Now;
                objmatch.ChangeBy = User.UserName;
                DB.HRComfirmProbations.Attach(objmatch);
                DB.Entry(objmatch).Property(w => w.ChangeOn).IsModified = true;
                DB.Entry(objmatch).Property(w => w.ChangeBy).IsModified = true;
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
        public void SetAutoApproval(string DocType, string Branch, string SCREEN_ID)
        {
            ListApprove = new List<ExDocApproval>();
           
            var objDoc = DH.ExCfWorkFlowItems.Find(SCREEN_ID, DocType);
            if (objDoc != null)
            {
                if (objDoc.IsRequiredApproval == true)
                {
                    var listDefaultApproval = DH.ExCfWFApprovers.Where(w => w.Branch == Branch && w.WFObject == objDoc.ApprovalFlow && w.IsSelected == true).ToList();
                    foreach (var read in listDefaultApproval)
                    {
                        var objApp = new ExDocApproval();
                        objApp.Approver = read.Employee;
                        objApp.ApproverName = read.EmployeeName;
                        objApp.DocumentType = DocType;
                        objApp.ApproveLevel = read.ApproveLevel;
                        objApp.WFObject = objDoc.ApprovalFlow;
                        ListApprove.Add(objApp);
                    }
                }
            }
        }
        //public void SetAutoApproval(string SCREEN_ID, string DocType)
        //{
        //    ListApprove = new List<ExDocApproval>();
        //    var DBX = new PODEnity();
        //    var objDoc = DB.ExCfWorkFlowItems.Find(SCREEN_ID, DocType);
        //    if (objDoc != null)
        //    {
        //        if (objDoc.IsRequiredApproval == true)
        //        {
        //            var listDefaultApproval = DB.ExCfWFSalaryApprovers.Where(w => w.WFObject == objDoc.ApprovalFlow && w.IsSelected == true).ToList();
        //            foreach (var read in listDefaultApproval)
        //            {
        //                var objApp = new ExDocApproval();
        //                objApp.Approver = read.Employee;
        //                objApp.ApproverName = read.EmployeeName;
        //                objApp.AppLevel = read.ApproveLevel;
        //                //objApp.App = objDoc.ApprovalFlow;
        //                ListApprove.Add(objApp);
        //            }
        //        }
        //    }
        //}

        public HRStaffProfile getNextApprover(string id, string pro)
        {
            var objStaff = new HRStaffProfile();
            var objHeader = DB.HRComfirmProbations.FirstOrDefault(w=>w.EmpCode==id);
            if (objHeader == null)
            {
                return new HRStaffProfile();
            }
            string open = SYDocumentStatus.OPEN.ToString();
            var listCanApproval = DB.ExDocApprovals.Where(w => w.DocumentNo == id && w.Status == open && w.DocumentType == objHeader.DocType).ToList();

            if (listCanApproval.Count == 0)
            {
                return new HRStaffProfile();
            }

            var min = listCanApproval.Min(w => w.ApproveLevel);
            var NextApp = listCanApproval.Where(w => w.ApproveLevel == min).First();
            objStaff = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == NextApp.Approver);//, objHeader.Property);
            return objStaff;
        }

        public string ApproveCP(string DOCNO, string URL)
        {
            //PODEnity DBI = new PODEnity();
            try
            {
                DB.Configuration.AutoDetectChangesEnabled = false;
                try
                {
                    //___________________________________________
                    //string Approved = SYDocumentStatus.APPROVED.ToString();
                    //string Open = SYDocumentStatus.OPEN.ToString();
                    //var _UpdComfirmed = DBI.HRComfirmProbations.FirstOrDefault(w => w.CPID == DOCNO);
                    //if (_UpdComfirmed == null) return "DOC_NE";
                    //if (_UpdComfirmed.Status == Approved) return "REQUEST_TYPE_NE";
                    //var objmatch = DBI.ExDocApprovals.FirstOrDefault(w => w.DocNo == DOCNO);
                    //if (objmatch == null) return "INV_EN";
                    //var _ListApprove = DBI.ExDocApprovals.Where(w => w.DocNo == DOCNO && w.Status == Open).OrderBy(w => w.AppLevel).ToList();
                    //var listUser = DBI.HRStaffProfiles.Where(w => w.EmpCode == User.UserName).ToList();
                    //var b = false;

                    var _UpdComfirmed = DB.HRComfirmProbations.FirstOrDefault(w => w.CPID == DOCNO);
                    if (_UpdComfirmed == null)
                    {
                        return "REQUEST_NE";
                    }
                    Header = _UpdComfirmed;

                    if (_UpdComfirmed.Status != SYDocumentStatus.PENDING.ToString())
                    {
                        return "INV_DOC";
                    }
                    string open = SYDocumentStatus.OPEN.ToString();
                    var _ListApprove = DB.ExDocApprovals.Where(w => //w.do == objMatch.DocType && 
                                        w.DocumentNo == _UpdComfirmed.CPID && w.Status == open).OrderBy(w => w.ApproveLevel).ToList();
                    var listUser = DB.HRStaffProfiles.Where(w => w.EmpCode == User.UserName).ToList();
                    var b = false;
                    foreach (var read in _ListApprove)
                    {
                        var checklist = listUser.Where(w => w.EmpCode == read.Approver).ToList();
                        if (checklist.Count > 0)
                        {
                            if (read.Status == SYDocumentStatus.APPROVED.ToString())
                            {
                                return "USER_ALREADY_APP";
                            }

                            if (read.ApproveLevel > _ListApprove.Min(w => w.ApproveLevel))
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
                                read.Approver = objStaff.EmpCode;
                                read.ApproverName = objStaff.AllName;
                                //read.ApprovedDate = DateTime.Now;
                                read.Status = SYDocumentStatus.APPROVED.ToString();
                                DB.ExDocApprovals.Attach(read);
                                DB.Entry(read).State = System.Data.Entity.EntityState.Modified;

                                b = true;
                                break;
                            }
                        }

                    }
                    if (_ListApprove.Count > 0)
                    {
                        if (b == false)
                        {
                            return "USER_NOT_APPROVOR";
                        }
                    }
                    #region Close
                    //foreach (var read in _ListApprove)
                    //{
                    //    var chkUser = listUser.Where(w => w.EmpCode == read.Approver).ToList();
                    //    if(chkUser.Count >0)
                    //    {
                    //        if (read.Status == Approved) return "USER_ALREADY_APP";
                    //        if (read.AppLevel > _ListApprove.Min(w => w.AppLevel)) return "REQUIRED_PRE_LEVEL";
                    //        var _UpdateApprover = DBI.ExDocApprovals.FirstOrDefault(w => w.DocNo == DOCNO && w.Approver == read.Approver);
                    //        if (_UpdateApprover != null)
                    //        {
                    //            _UpdateApprover.Status = Approved;
                    //            DBI.ExDocApprovals.Attach(objmatch);
                    //            DBI.Entry(_UpdateApprover).Property(w => w.Status).IsModified = true;
                    //            DBI.SaveChanges();
                    //            b = true;
                    //            break;
                    //        }
                    //    }
                    //}
                    //if(_ListApprove.Count > 0)
                    //{
                    //    if (b == false) return "USER_NOT_APPROVOR";
                    //}
                    #endregion
                    var status = SYDocumentStatus.APPROVED.ToString();
                    //var open = SYDocumentStatus.OPEN.ToString();
                    // objMatch.IsApproved = true;
                    if (_ListApprove.Where(w => w.Status == open).ToList().Count > 0)

                    {
                        status = SYDocumentStatus.PENDING.ToString();
                        // objMatch.IsApproved = false;
                    }

                    _UpdComfirmed.Status = status;

                    DB.Entry(_UpdComfirmed).Property(w => w.Status).IsModified = true;
                    DB.SaveChanges();

                    int row = DB.SaveChanges();
                    return SYConstant.OK;
                }
                catch (DbEntityValidationException e)
                {
                    /*------------------SaveLog----------------------------------*/
                    SYEventLog log = new SYEventLog();
                    log.ScreenId = ScreenId;
                    log.UserId = User.UserName;
                    log.DocurmentAction = DOCNO;
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
                    log.DocurmentAction = DOCNO;
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
                    log.DocurmentAction = DOCNO;
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
        public string isValidApproval(ExDocApproval Approver, EnumActionGridLine Action)
        {
            if (Action == EnumActionGridLine.Add)
            {
                if (ListApprove.Where(w => w.Approver == Approver.Approver).ToList().Count > 0)
                {
                    return "ISDUPLICATED";
                }
            }

            return SYConstant.OK;
        }
    }
}
