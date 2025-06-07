using Humica.Core.DB;
using Humica.Core.SY;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;

namespace Humica.Logic.EOB
{

    public class ConfirmAlertObject
    {
        public HumicaDBContext DB = new HumicaDBContext();
        public SMSystemEntity DP = new SMSystemEntity();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public string ScreenId { get; set; }
        public string DocType { get; set; }
        public EOBConfirmAlert Header { get; set; }
        public RCMApplicant Applicant { get; set; }
        public List<EOBConfirmAlert> ListHeader { get; set; }
        public List<RCMApplicant> ListApplicant { get; set; }
        public ClsEmail EmailObject { get; set; }
        public TPEmailTemplate EmailTemplate { get; set; }
        public string Action { get; set; }
        public SYSplitItem SelectListItem { get; set; }
        public List<object> ListObjectDictionary { get; set; }
        public string MessageError { get; set; }
        public ConfirmAlertObject()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }
        public string ConfAlert(string ID)
        {
            try
            {
                var _interview = DB.RCMPInterviews.FirstOrDefault(w => w.ApplicantID == ID);
                Header.ID = _interview.ApplicantID;
                Header.Name = _interview.CandidateName;
                Header.Status = SYDocumentStatus.OPEN.ToString();
                Header.Confirmed = false;
                Header.CreatedBy = User.UserName;
                Header.CreatedOn = DateTime.Now;

                DB.EOBConfirmAlerts.Add(Header);

                DB.SaveChanges();

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Header.ID;
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
                log.DocurmentAction = Header.ID;
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
                log.DocurmentAction = Header.ID;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();
                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string updConfirm(string ID)
        {
            try
            {
                DB = new HumicaDBContext();

                var ObjMatch = DB.EOBConfirmAlerts.FirstOrDefault(w => w.ID == ID);

                if (ObjMatch == null)
                    return "DOC_INV";
                if (Header.JoinDate == null)
                    return "Join_Date_is_require";

                ObjMatch.ChangedBy = User.UserName;
                ObjMatch.ChangedOn = DateTime.Now;
                ObjMatch.AttachForm = Header.AttachForm;
                ObjMatch.JoinDate = Header.JoinDate;
                ObjMatch.Confirmed = Header.Confirmed;

                DB.EOBConfirmAlerts.Attach(ObjMatch);

                DB.Entry(ObjMatch).Property(x => x.AttachForm).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.JoinDate).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Confirmed).IsModified = true;
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
                log.DocurmentAction = Header.ID;
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
                log.DocurmentAction = Header.ID;
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string ApproveTheDoc(string ID)
        {
            try
            {
                var objMatch = DB.EOBConfirmAlerts.FirstOrDefault(w => w.ID == ID);
                if (objMatch == null)
                {
                    return "INV_DOC";
                }
                if (objMatch.Status != SYDocumentStatus.PENDING.ToString() && objMatch.Status != "CONSIDER")
                {
                    return "INV_DOC";
                }
                var objMatchApp = DB.RCMApplicants.FirstOrDefault(w => w.ApplicantID == ID);
                if (objMatchApp == null)
                {
                    return "INV_DOC";
                }
                objMatchApp.IsConfirm = true;
                DB.RCMApplicants.Attach(objMatchApp);
                DB.Entry(objMatchApp).Property(w => w.IsConfirm).IsModified = true;
                string open = SYDocumentStatus.OPEN.ToString();
                var status = SYDocumentStatus.CONFIRMED.ToString();
                objMatch.Status = status;
                objMatch.Confirmed = true;
                DB.Entry(objMatch).Property(w => w.Status).IsModified = true;
                DB.Entry(objMatch).Property(w => w.Confirmed).IsModified = true;
                DB.Entry(objMatch).Property(w => w.ChangedOn).IsModified = true;
                DB.Entry(objMatch).Property(w => w.ChangedBy).IsModified = true;
                int row = DB.SaveChanges();
                return SYConstant.OK;
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
        public string requestApprove(string ID, string Upload)
        {
            try
            {
                HumicaDBContext DBX = new HumicaDBContext();
                var objMatch = DB.EOBConfirmAlerts.FirstOrDefault(w => w.ID == ID);
                if (objMatch == null)
                {
                    return "REQUEST_NE";
                }
                if (objMatch.Status != SYDocumentStatus.OPEN.ToString() && objMatch.Status != "CONSIDER")
                {
                    return "INV_DOC";
                }
                Header = objMatch;
                objMatch.Status = SYDocumentStatus.PENDING.ToString();
                DB.EOBConfirmAlerts.Attach(objMatch);
                DB.Entry(objMatch).Property(w => w.Status).IsModified = true;
                DB.SaveChanges();
                #region Email
                var _interview = DB.RCMPInterviews.FirstOrDefault(w => w.ApplicantID == ID);
                var EmailConf = DP.CFEmailAccounts.FirstOrDefault();
                var _Position = DB.RCMApplicants.FirstOrDefault(w => w.ApplicantID == ID);
                if (_Position == null) return "INV_DOC";
                var _chkpostdesc = DB.HRPositions.FirstOrDefault(w => w.Code == _Position.ApplyPosition);
                if (EmailConf != null)
                {
                    if (objMatch.SendingSelected == "EM" || objMatch.SendingSelected == "Email")
                    {
                        string Position = "";
                        if (_chkpostdesc == null) Position = "";
                        else Position = _chkpostdesc.Description;
                        string str = "Dear Mr./Mrs. " + "<b>" + _interview.CandidateName + "</b>" + " ,";
                        str += "<br /> <p>Congratulations, you have successfully appointed for the role of <b>" + Position + "</b>";
                        str += "<br /> We have attached a file below for you to fullfill your information.";
                        str += "<br /> We look forward to seeing you soon." + "</p>";
                        str += "<br /><br />" + "Thank you!";

                        CFEmailAccount emailAccount = EmailConf;
                        string subject = "Congratulation Letter";
                        string body = str;
                        string filePath = Upload;
                        string fileName = Path.GetFileName(filePath);
                        EmailObject = new ClsEmail();
                        int rs = EmailObject.SendMail(emailAccount, objMatch.Remark, User.Email,
                            subject, body, filePath, fileName);

                        //EmailObject.SmtpHost = EmailConf.First().SmtpAddress;
                        //EmailObject.SmtpName = EmailConf.First().SmtpUserName;
                        //EmailObject.SmtpPort = EmailConf.First().SmtpPort.ToString();
                        //EmailObject.SmtpPassword = EmailConf.First().SmtpPassword;
                        //EmailObject.MailFrom = EmailConf.First().EmailAddress;
                        //EmailObject.MailCC = User.Email;
                        //EmailObject.MailTo = Header.Remark;
                        //EmailObject.IsEnableSSL = false;
                        //EmailObject.Attach = Upload;
                        //EmailObject.Subject = "Congratulation Letter";
                        //EmailObject.Body = str;
                        //int rs = EmailObject.SendEmail();
                    }
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
                log.ScreenId = ID;
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
        public string RejectTheDoc(string ID)
        {
            try
            {
                var objMatch = DB.EOBConfirmAlerts.FirstOrDefault(w => w.ID == ID);
                if (objMatch == null)
                {
                    return "INV_DOC";
                }
                objMatch.Status = SYDocumentStatus.REJECTED.ToString();
                objMatch.ChangedBy = User.UserName;
                objMatch.ChangedOn = DateTime.Now;


                DB.Entry(objMatch).Property(w => w.Status).IsModified = true;
                DB.Entry(objMatch).Property(w => w.ChangedOn).IsModified = true;
                DB.Entry(objMatch).Property(w => w.ChangedBy).IsModified = true;
                int row = DB.SaveChanges();

                return SYConstant.OK;
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
        public string Consider(string ID)
        {
            try
            {
                HumicaDBContext DBX = new HumicaDBContext();
                var ObjMatch = DB.EOBConfirmAlerts.FirstOrDefault(w => w.ID == ID);
                if (ObjMatch == null)
                {
                    return "REQUEST_NE";
                }
                if (ObjMatch.Status != SYDocumentStatus.OPEN.ToString())
                {
                    return "INV_DOC";
                }
                Header = ObjMatch;
                ObjMatch.Status = "CONSIDER";
                DB.EOBConfirmAlerts.Attach(ObjMatch);
                DB.Entry(ObjMatch).Property(w => w.Status).IsModified = true;
                DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.ScreenId = Header.ID;
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
                log.DocurmentAction = Header.ID;
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
                log.DocurmentAction = Header.ID;
                log.Action = SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
    }

}