using Humica.Core.DB;
using Humica.Core.SY;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic.CF;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;

namespace Humica.Logic.RCM
{
    public class RCMRecruitRequestObject
    {
        public HumicaDBContext DB = new HumicaDBContext();
        public SMSystemEntity SMS = new SMSystemEntity();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public string MessageError { get; set; }
        public string ScreenId { get; set; }
        public string JDDescription { get; set; }
        public RCMRecruitRequest Header { get; set; }
        public ClsEmail EmailObject { get; set; }
        public ExDocApproval HeaderAPP { get; set; }
        public List<RCMRecruitRequest> ListHeader { get; set; }
        public List<ExDocApproval> ListApproval { get; set; }
        public List<RCM_RecruitRequest_VIEW> ListRecruitRequest { get; set; }
        public RCM_RecruitRequest_VIEW RCM_RecruitRequest_VIEW { get; set; }
        public string Code { get; set; }
        public string MessageCode { get; set; }
        public HRStaffProfile StaffProfile { get; set; }
        public ExCfWFApprover ExCfWFApprover { get; set; }
        public Filter Filters { get; set; }

        public RCMRecruitRequestObject()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }

        public class Filter
        {
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            public string Status { get; set; }
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

        #region 'Create'
        public string createRRequest(out string RequestNo)
        {
            RequestNo = string.Empty;
            try
            {
                if (Header.Branch == null) return "BRANCH_EN";
                if (Header.DEPT == null) return "DEPARTMENT_EN";
                if (Header.POST == null) return "POSITION_EN";
                if (Header.NoOfRecruit <= 0) return "EENO";
                if (Header.RequestedBy == null) return "EEREQ";
                if (Header.JDCode == null) return "EEJD";

                var objCF = DB.ExCfWorkFlowItems.Where(w => w.ScreenID == ScreenId).ToList();
                if (objCF.Count() == 0)
                {
                    return "REQUEST_TYPE_NE";
                }
                var objNumber = new CFNumberRank(objCF.First().DocType, ScreenId);
                if (objNumber.NextNumberRank == null)
                {
                    return "NUMBER_RANK_NE";
                }
                Header.RequestNo = objNumber.NextNumberRank;
                Header.DocType = objCF.First().DocType;
                Header.DocDate = DateTime.Now;
                Header.CreatedBy = User.UserName;
                Header.CreatedOn = DateTime.Now;
                // Add Approver
                foreach (var read in ListApproval)
                {
                    read.ID = 0;
                    read.LastChangedDate = DateTime.Now;
                    read.DocumentNo = Header.RequestNo;
                    read.DocumentType = Header.DocType;
                    read.Status = SYDocumentStatus.OPEN.ToString();
                    read.ApprovedBy = "";
                    read.ApprovedName = "";
                    read.ApproverName = "";
                    read.WFObject = "";
                    var objStaff = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == read.Approver);
                    if (objStaff != null)
                    {
                        read.ApproverName = objStaff.AllName;
                    }
                    DB.ExDocApprovals.Add(read);
                }

                DB.RCMRecruitRequests.Add(Header);
                DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Header.RequestNo;
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
                log.DocurmentAction = Header.RequestNo;
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
                log.DocurmentAction = Header.RequestNo;
                log.Action = SYActionBehavior.ADD.ToString();
                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        #endregion
        public string UpdRRequest(string RequestNo)
        {
            try
            {

                DB = new HumicaDBContext();

                var ObjMatch = DB.RCMRecruitRequests.FirstOrDefault(w => w.RequestNo == RequestNo);

                if (ObjMatch == null) return "DOC_INV";

                ObjMatch.ChangedBy = User.UserName;
                ObjMatch.ChangedOn = DateTime.Now;
                ObjMatch.Branch = Header.Branch;
                ObjMatch.DEPT = Header.DEPT;
                ObjMatch.Status = Header.Status;
                ObjMatch.POST = Header.POST;
                ObjMatch.RecruitType = Header.RecruitType;
                ObjMatch.NoOfRecruit = Header.NoOfRecruit;
                ObjMatch.WorkingType = Header.WorkingType;
                ObjMatch.ProposedSalaryTo = Header.ProposedSalaryTo;
                ObjMatch.ProposedSalaryFrom = Header.ProposedSalaryFrom;
                ObjMatch.RecruitFor = Header.RecruitFor;
                ObjMatch.JobLevel = Header.JobLevel;
                ObjMatch.Gender = Header.Gender;
                ObjMatch.TermEmp = Header.TermEmp;
                ObjMatch.ExpectedDate = Header.ExpectedDate;
                ObjMatch.RequestedBy = Header.RequestedBy;
                ObjMatch.RequestedDate = Header.RequestedDate;
                ObjMatch.AckedBy = Header.AckedBy;
                ObjMatch.JobResponsibility = Header.JobResponsibility;
                ObjMatch.JobRequirement = Header.JobRequirement;
                ObjMatch.ContFromDate = Header.ContFromDate;
                ObjMatch.ContToDate = Header.ContToDate;
                ObjMatch.Attachment = Header.Attachment;
                ObjMatch.StaffType = Header.StaffType;
                DB.RCMRecruitRequests.Attach(ObjMatch);

                DB.Entry(ObjMatch).Property(x => x.ChangedBy).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ChangedOn).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Branch).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.DEPT).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Status).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.POST).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.RecruitType).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.NoOfRecruit).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.WorkingType).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ProposedSalaryFrom).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ProposedSalaryTo).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.RecruitFor).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.JobLevel).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Gender).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.TermEmp).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ExpectedDate).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.RequestedBy).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.RequestedDate).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.AckedBy).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.JobResponsibility).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.JobRequirement).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ContFromDate).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ContToDate).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Attachment).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.StaffType).IsModified = true;

                DB.SaveChanges();

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = RequestNo;
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

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
                log.DocurmentAction = RequestNo;
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string deleteRRequest(string RequestNo)
        {
            try
            {
                DB = new HumicaDBContext();

                var ObjMatch = DB.RCMRecruitRequests.FirstOrDefault(w => w.RequestNo == RequestNo);
                string Approve = SYDocumentStatus.APPROVED.ToString();

                if (ObjMatch == null) return "DOC_INV";
                if (ObjMatch.Status == Approve) return "DOC_INV";

                var _chkVac = DB.RCMVacancies.FirstOrDefault(w => w.DocRef == RequestNo);
                if (_chkVac != null)
                    return "EE_APPCHK";

                DB.RCMRecruitRequests.Remove(ObjMatch);

                DB.SaveChanges();

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = RequestNo;
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
                log.DocurmentAction = RequestNo;
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
                log.DocurmentAction = RequestNo;
                log.Action = SYActionBehavior.DELETE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        #region 'Convert Status'
        public string Cancel(string RequestNo)
        {
            try
            {
                HumicaDBContext DBI = new HumicaDBContext();

                RCMRecruitRequest objmatch = DB.RCMRecruitRequests.FirstOrDefault(w => w.RequestNo == RequestNo);

                if (objmatch == null) return "DOC_INV";
                string Open = SYDocumentStatus.OPEN.ToString();
                var listApproval = DB.ExDocApprovals.Where(w => w.DocumentType == objmatch.DocType
                                    && w.DocumentNo == objmatch.RequestNo && w.Status == Open).OrderBy(w => w.ApproveLevel).ToList();
                var listUser = DB.HRStaffProfiles.Where(w => w.EmpCode == User.UserName).ToList();
                var b = false;
                int approverLevel = 0;
                foreach (var read in listApproval)
                {
                    approverLevel = read.ApproveLevel;
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
                            StaffProfile = objStaff;
                            read.ApprovedBy = objStaff.EmpCode;
                            read.ApprovedName = objStaff.AllName;
                            read.LastChangedDate = DateTime.Now.Date;
                            read.ApprovedDate = DateTime.Now;
                            read.Status = SYDocumentStatus.REJECTED.ToString();
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
                objmatch.Status = SYDocumentStatus.CANCELLED.ToString();
                objmatch.ChangedBy = User.UserName;
                objmatch.ChangedOn = DateTime.Now;

                DB.RCMRecruitRequests.Attach(objmatch);

                DB.Entry(objmatch).Property(w => w.Status).IsModified = true;
                DB.Entry(objmatch).Property(w => w.ChangedBy).IsModified = true;
                DB.Entry(objmatch).Property(w => w.ChangedOn).IsModified = true;
                #region Email
                var EmailConf = SMS.CFEmailAccounts.FirstOrDefault();
                var Reject = DB.ExDocApprovals.FirstOrDefault(w => w.DocumentNo == objmatch.RequestNo);
                if (Reject == null) return "INV_DOC";
                if (EmailConf != null)
                {
                    var Rejecter = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == User.UserName);
                    var staff_ = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == objmatch.RequestedBy);
                    var _company = SMS.SYHRCompanies.FirstOrDefault();
                    if (Rejecter != null && staff_ != null)
                    {
                        string str = "Dear " + staff_.Title + "<b> " + staff_.AllName + "</b>" + " ,";
                        str += "TEM Trading Co.,Ltd. is pleased to inform you that you have Reject your Recruitment  Request Form  in our company.";
                        str += "<br /><br /> Your truly";
                        str += "<br /> <br /> " + staff_.Title + "<b> " + Rejecter.AllName;
                        CFEmailAccount emailAccount = EmailConf;
                        string subject = "Reject Recruitment Request Form" /*+ Position + " at " + _company.CompENG*/;
                        string body = str;
                        string filePath = "";// Upload;
                        string fileName = "";// Path.GetFileName(filePath);
                        ClsEmail EmailObject = new ClsEmail();
                        int rs = EmailObject.SendMail(emailAccount, staff_.Email, User.Email,
                            subject, body, filePath, fileName);
                    }
                }

                #endregion

                DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = RequestNo;
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
                log.DocurmentAction = RequestNo;
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
                log.DocurmentAction = RequestNo;
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string Request(string RequestNo)
        {
            try
            {
                HumicaDBContext DBI = new HumicaDBContext();

                RCMRecruitRequest objmatch = DB.RCMRecruitRequests.FirstOrDefault(w => w.RequestNo == RequestNo);

                if (objmatch == null) return "DOC_INV";

                objmatch.Status = SYDocumentStatus.PENDING.ToString();
                objmatch.ChangedBy = User.UserName;
                objmatch.ChangedOn = DateTime.Now;

                DB.RCMRecruitRequests.Attach(objmatch);

                DB.Entry(objmatch).Property(w => w.Status).IsModified = true;
                DB.Entry(objmatch).Property(w => w.ChangedBy).IsModified = true;
                DB.Entry(objmatch).Property(w => w.ChangedOn).IsModified = true;

                DB.SaveChanges();

                #region 'Telegram'
                var AnnounceEmp = DB.RCMRecruitRequests.FirstOrDefault(w => w.RequestNo == objmatch.RequestNo);
                var excfObject = DB.ExCfWorkFlowItems.Find(ScreenId, objmatch.DocType);

                var Telegram = DB.TelegramBots.FirstOrDefault(w => w.ChatID == excfObject.Telegram);
                var EmailTemplate = SMS.TPEmailTemplates.Find("RCMREQUESTFORM");
                //var staff = DB.HRStaffProfiles.Where(w => w.EmpCode == EmpCode).ToList();
                //int Chart_ID = Convert.ToInt32(staff.FirstOrDefault().TeleChartID);

                if (Telegram != null)
                {
                    int Chart_ID = Convert.ToInt32(Telegram.ChatID);
                    List<object> ListObjectDictionary = new List<object>();
                    ListObjectDictionary.Add(AnnounceEmp);
                    //SendTelegramObject.Send_Announce_Telegram(EmailTemplate.RequestContent, Chart_ID, fileName, Telegram.TokenID, EmailTemplate.EMTemplateObject, ListObjectDictionary);
                    //SendTelegramObject.Send_Announce_Telegram(EmailTemplate.RequestContent, Chart_ID, Telegram.TokenID, EmailTemplate.EMTemplateObject, ListObjectDictionary);
                }
                #endregion

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = RequestNo;
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
                log.DocurmentAction = RequestNo;
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
                log.DocurmentAction = RequestNo;
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public void SendEmail(string Upload, string Rerceiver)
        {
            try
            {
                #region Email
                var AlertTO = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == Rerceiver);

                var EmailConf = SMS.CFEmailAccounts.FirstOrDefault();


                if (EmailConf != null && AlertTO != null)
                {
                    CFEmailAccount emailAccount = EmailConf;
                    string subject = string.Format("Recruitment Request Form {0:dd-MMM-yyyy}", DateTime.Today);
                    string filePath = Upload;
                    string fileName = Path.GetFileName(filePath);
                    EmailObject = new ClsEmail();
                    int rs = EmailObject.SendMail(emailAccount, "", AlertTO.Email,
                        subject, "", filePath, fileName);
                }

                #endregion
            }
            catch
            {
                throw new Exception("FAIL_TO_SEND_MAIL");
            }
        }
        private static List<T> GetNextItemFromList<T>(List<T> list, T currentObj)
        {
            return list.SkipWhile(obj => obj.Equals(currentObj)).Take(1).ToList();
        }
        public string Approved(string RequestNo, string Upload)
        {
            try
            {
                HumicaDBContext DBI = new HumicaDBContext();

                RCMRecruitRequest objmatch = DB.RCMRecruitRequests.FirstOrDefault(w => w.RequestNo == RequestNo);

                if (objmatch == null) return "DOC_INV";
                //if (objmatch.Status != SYDocumentStatus.PENDING.ToString()) return "INV_DOC";

                string Open = SYDocumentStatus.OPEN.ToString();
                var listApproval = DB.ExDocApprovals.Where(w => w.DocumentType == objmatch.DocType
                                    && w.DocumentNo == objmatch.RequestNo && w.Status == Open).OrderBy(w => w.ApproveLevel).ToList();
                var listUser = DB.HRStaffProfiles.Where(w => w.EmpCode == User.UserName).ToList();
                var b = false;
                int approverLevel = 0;
                foreach (var read in listApproval)
                {
                    approverLevel = read.ApproveLevel;
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
                            StaffProfile = objStaff;
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
                var Appro_ = objmatch.RequestedBy;
                // Approver
                if ((listApproval.Where(w => w.ApproveLevel > approverLevel && w.Status == SYDocumentStatus.OPEN.ToString()).Count() > 0))
                {
                    Status = SYDocumentStatus.PENDING.ToString();
                    Appro_ = listApproval.Where(w => w.ApproveLevel == approverLevel).Select(w => w.Approver)?.FirstOrDefault();

                }

                objmatch.Status = Status;
                objmatch.ApprovedDate = DateTime.Now;

                DB.RCMRecruitRequests.Attach(objmatch);

                DB.Entry(objmatch).Property(w => w.Status).IsModified = true;
                DB.Entry(objmatch).Property(w => w.ApprovedDate).IsModified = true;
                #region Email
                //var AlertTO = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == Appro_);
                ExDocApproval currenctApproval = listApproval.Where(w => w.Approver == Appro_).FirstOrDefault();
                ExDocApproval exDocApproval = GetNextItemFromList(listApproval, currenctApproval).ToList().FirstOrDefault();
                var AlertTO = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == exDocApproval.Approver);
                //var _interview = DB.RCMPInterviews.FirstOrDefault(w => w.ApplicantID == ID);
                var EmailConf = SMS.CFEmailAccounts.FirstOrDefault();
                //var _Position = DB.RCMApplicants.FirstOrDefault(w => w.ApplicantID == ID);
                //if (_Position == null) return "INV_DOC";
                //var _chkpostdesc = DB.HRPositions.FirstOrDefault(w => w.Code == _Position.ApplyPosition);

                if (EmailConf != null && AlertTO != null)
                {
                    CFEmailAccount emailAccount = EmailConf;
                    string subject = string.Format("Recruitment Request Form {0:dd-MMM-yyyy}", DateTime.Today);
                    //string body = str;
                    string filePath = Upload;
                    string fileName = Path.GetFileName(filePath);
                    EmailObject = new ClsEmail();
                    int rs = EmailObject.SendMail(emailAccount, "", AlertTO.Email,
                        subject, "", filePath, fileName);
                    //}
                }

                #endregion
                DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = RequestNo;
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
                log.DocurmentAction = RequestNo;
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
                log.DocurmentAction = RequestNo;
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public HRStaffProfile getNextApprover(string RequestNo)
        {
            var objStaff = new HRStaffProfile();
            var DB = new HumicaDBContext();
            var objHeader = DB.RCMRecruitRequests.FirstOrDefault(w => w.RequestNo == RequestNo);
            if (objHeader == null)
            {
                return new HRStaffProfile();
            }
            string open = SYDocumentStatus.OPEN.ToString();
            var listCanApproval = DB.ExDocApprovals.Where(w => w.DocumentNo == RequestNo && w.Status == open && w.DocumentType == objHeader.DocType).ToList();

            if (listCanApproval.Count == 0)
            {
                return new HRStaffProfile();
            }

            var min = listCanApproval.Min(w => w.ApproveLevel);
            var NextApp = listCanApproval.Where(w => w.ApproveLevel == min).First();
            objStaff = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == NextApp.Approver);
            return objStaff;
        }

        #endregion 
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
        public static IEnumerable<HRPosition> GetPosition()
        {
            HumicaDBContext DBB = new HumicaDBContext();
            List<HRPosition> _List = new List<HRPosition>();
            var WorkGroup = GetDataCompanyGroup("Position");
            if (!string.IsNullOrEmpty(WorkGroup))
            {
                if (HttpContext.Current.Session["Position"] != null)
                {
                    string CompanyGroup = HttpContext.Current.Session["Position"].ToString();
                    if (!string.IsNullOrEmpty(CompanyGroup))
                    {
                        var _HRList = (from Group in DBB.HRCompanyTrees
                                       join JobType in DBB.HRPositions on Group.CompanyMember equals JobType.Code
                                       where Group.ParentWorkGroupID == CompanyGroup
                                       select JobType);
                        _List = _HRList.ToList();
                    }
                }
            }
            else
            {
                _List = DBB.HRPositions.ToList();
            }
            return _List;
        }
        public static IEnumerable<RCMSJobDesc> GetJD()
        {
            HumicaDBContext DBB = new HumicaDBContext();
            List<RCMSJobDesc> _List = new List<RCMSJobDesc>();
            if (HttpContext.Current.Session["JD"] != null)
            {
                string Position = HttpContext.Current.Session["JD"].ToString();
                if (!string.IsNullOrEmpty(Position))
                {
                    var _HRList = DBB.RCMSJobDescs.Where(w => w.Position == Position).ToList();
                    _List = _HRList.ToList();
                }
            }
            return _List;
        }
        public static string GetDataCompanyGroup(string WorkGroup)
        {
            string Result = "";
            HumicaDBContext DBB = new HumicaDBContext();
            var _listCom = DBB.HRCompanyGroups.Where(w => w.WorkGroup == WorkGroup).ToList();
            if (_listCom.Count() > 0)
            {
                var obj = _listCom.FirstOrDefault();
                Result = obj.WorkGroup;
            }
            return Result;
        }
    }

}
