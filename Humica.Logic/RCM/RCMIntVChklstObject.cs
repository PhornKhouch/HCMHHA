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

namespace Humica.Logic.RCM
{

    public class RCMIntVChklstObject
    {
        public HumicaDBContext DB = new HumicaDBContext();
        public SMSystemEntity DP = new SMSystemEntity();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public string ApplyPosition { get; set; }
        public string DocType { get; set; }
        public string ApplicantName { get; set; }
        public string ApplyBranch { get; set; }
        public string Gender { get; set; }
        public string ApplyDept { get; set; }
        public string WorkType { get; set; }
        public string VacNo { get; set; }
        public int? IntVStep { get; set; }
        public decimal ExpectSalary { get; set; }
        public RCMApplicant App { get; set; }
        public RCMPInterview Header { get; set; }
        public RCMIntVQuestionnaire IntVQ { get; set; }
        public List<RCMApplicant> ListCandidate { get; set; }
        public List<RCMPInterview> ListInt { get; set; }
        public List<RCMIntVQuestionnaire> ListQuestionnair { get; set; }
        public List<RCMVInterviewer> ListInterViewer { get; set; }
        public FilterCandidate Filtering { get; set; }
        public string ScreenId { get; set; }
        public string ErrorMessage { get; set; }
        public RCMIntVChklstObject()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }

        #region 'Create'
        public string createJobIntV()
        {
            try
            {
                string Open = SYDocumentStatus.OPEN.ToString();
                var _Interview = DB.RCMPInterviews.ToList();
                var _chkdup = _Interview.FirstOrDefault(w => w.IntVStep == IntVStep && w.ApplicantID == Header.ApplicantID);

                if (_chkdup != null)
                {
                    if (_chkdup.Status == Open) return "Candidate have been checklist!";
                }
                foreach (var read in ListQuestionnair.ToList())
                {
                    read.ApplicantID = Header.ApplicantID;
                    read.IntVStep = Convert.ToInt32(IntVStep);
                    DB.RCMIntVQuestionnaires.Add(read);
                }
                var _Interviewer = DB.RCMVInterviewers.Where(w => w.IntStep == IntVStep && w.Code == Header.ApplicantID).ToList();
                foreach (var read in _Interviewer.ToList())
                {
                    DB.RCMVInterviewers.Remove(read);
                    DB.Entry(read).State = System.Data.Entity.EntityState.Deleted;
                }
                foreach (var read in ListInterViewer.ToList())
                {
                    read.Code = Header.ApplicantID;
                    read.Position = Header.ApplyPost;
                    DB.RCMVInterviewers.Add(read);
                }
                var _App = DB.RCMApplicants.FirstOrDefault(w => w.ApplicantID == Header.ApplicantID);
                if (_App != null)
                {
                    Header.DocType = _App.StaffType;
                    _App.IntVStatus = "Interview";
                    DB.RCMApplicants.Attach(_App);
                    DB.Entry(_App).Property(x => x.IntVStatus).IsModified = true;
                }
                Header.VacNo = VacNo;
                Header.DocDate = DateTime.Now;
                Header.Status = SYDocumentStatus.OPEN.ToString();
                Header.ReStatus = SYDocumentStatus.OPEN.ToString();
                Header.IntVStep = Convert.ToInt32(IntVStep);
                DB.RCMPInterviews.Add(Header);

                DB.SaveChanges();

                #region Email
                SYEmail SE = new SYEmail();
                var EmailConf = DP.SYEmailConfs.Where(w => w.ProjectID == "1").ToList();
                //var StageAssigntoEmail = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == Header.StageAssignTo);
                //string Title = "";
                //if (EmailConf.Count > 0)
                //{
                //    if (Header.AlertToInterviewer == "EM" && StageAssigntoEmail!=null)
                //    {
                //        var dt = Header.AppointmentDate;
                //        SE.MailFrom = EmailConf.First().EmailAddress;
                //        SE.SmtpPassword = Convert.ToString(EmailConf.First().SmtpPassword);
                //        SE.SmtpPort = Convert.ToString(EmailConf.First().SmtpPort);
                //        SE.SmtpHost = Convert.ToString(EmailConf.First().SmtpAddress);
                //        SE.Subject = "Interview Candidate";
                //        Title += "<b>" + StageAssigntoEmail.Title + "</b>";
                //        string str = "Dear " + Title + "<b>"+ StageAssigntoEmail.AllName+"</b>";
                //        str += "<br /> <p>I would like to inform you that you have to interview a candidate on " +"<b>" + Header.AppointmentDate.ToString() + " " + Header.SetTime + "</b>";
                //        str += "<br /> At " + Header.Location ;
                //        str += "<br /><br />";
                //        str += "Thank you !";
                //        SE.Body = str;

                //        var MailTo = StageAssigntoEmail;

                //        if (MailTo != null)
                //        {
                //            SE.MailTo = MailTo.Email;

                //            string MailCC = "";
                //            if (MailTo.Email != null)
                //            {
                //                MailCC += User.Email + ";";
                //            }
                //            SE.MailCC = MailCC;
                //            int re = SE.SendEmail();
                //        }
                //    }
                //}

                #endregion

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Header.ApplicantID;
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
                log.DocurmentAction = Header.ApplicantID;
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
                log.DocurmentAction = Header.ApplicantID;
                log.Action = SYActionBehavior.ADD.ToString();
                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        #endregion 
        public string updateCheckList(string TranNo)
        {
            try
            {
                DB = new HumicaDBContext();

                int Tran = Convert.ToInt32(TranNo);
                var ObjMatch = DB.RCMPInterviews.FirstOrDefault(w => w.TranNo == Tran);

                if (ObjMatch == null) return "DOC_INV";
                if (ObjMatch.Status == "PASS" || ObjMatch.Status == "FAIL") return "DOC_INV";
                if (ObjMatch.Status == "NEXTSTEP") return "DOC_INV";

                var _ListQuest = DB.RCMIntVQuestionnaires.Where(w => w.ApplicantID == ObjMatch.ApplicantID && w.IntVStep == ObjMatch.IntVStep).ToList();

                foreach (var read in _ListQuest.ToList())
                {
                    DB.RCMIntVQuestionnaires.Remove(read);
                }
                foreach (var read in ListQuestionnair.ToList())
                {
                    read.ApplicantID = ObjMatch.ApplicantID;
                    read.IntVStep = ObjMatch.IntVStep;
                    DB.RCMIntVQuestionnaires.Add(read);
                }
                var Interviewer = DB.RCMVInterviewers.Where(w => w.IntStep == ObjMatch.IntVStep && w.Code == ObjMatch.VacNo).ToList();

                foreach (var read in Interviewer.ToList())
                {
                    DB.RCMVInterviewers.Remove(read);
                }
                int Line = 0;
                var Interviewer_ = DB.RCMVInterviewers.Where(w => w.Code == ObjMatch.VacNo).OrderByDescending(w => w.LineItem).First();
                if (Interviewer_ != null)
                {
                    Line = Interviewer_.LineItem;
                }
                foreach (var read in ListInterViewer.ToList())
                {
                    Line = Line + 1;
                    var objNew = new RCMVInterviewer();
                    objNew.Code = ObjMatch.VacNo;
                    objNew.LineItem = Line;
                    objNew.IntStep = ObjMatch.IntVStep;
                    objNew.EmpCode = read.EmpCode;
                    objNew.EmpName = read.EmpName;
                    objNew.Remark = read.Remark;
                    objNew.Position = Header.ApplyPost;
                    DB.RCMVInterviewers.Add(objNew);
                }

                ObjMatch.Remark = Header.Remark;
                ObjMatch.AppointmentDate = Header.AppointmentDate;
                ObjMatch.StartTime = Header.StartTime;
                ObjMatch.EndTime = Header.EndTime;
                ObjMatch.Location = Header.Location;
                ObjMatch.AlertToInterviewer = Header.AlertToInterviewer;

                DB.RCMPInterviews.Attach(ObjMatch);

                DB.Entry(ObjMatch).Property(x => x.Remark).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.AppointmentDate).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.StartTime).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.EndTime).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Location).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.AlertToInterviewer).IsModified = true;

                DB.SaveChanges();

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Header.ApplicantID;
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
                log.DocurmentAction = Header.ApplicantID;
                log.Action = SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }

        public string ReleaseDoc(int ID)
        {
            try
            {
                HumicaDBContext DBX = new HumicaDBContext();
                var objMatch = DB.RCMPInterviews.FirstOrDefault(w => w.TranNo == ID);
                if (objMatch == null)
                {
                    return "REQUEST_NE";
                }
                if (objMatch.ReStatus != SYDocumentStatus.OPEN.ToString())
                {
                    return "INV_DOC";
                }
                Header = objMatch;
                objMatch.ReStatus = SYDocumentStatus.PENDING.ToString();
                DB.RCMPInterviews.Attach(objMatch);
                DB.Entry(objMatch).Property(w => w.ReStatus).IsModified = true;
                DB.SaveChanges();
                #region Email
                var EmailConf = DP.CFEmailAccounts.FirstOrDefault();
                var _Position = DB.RCMApplicants.FirstOrDefault(w => w.ApplicantID == objMatch.ApplicantID);
                if (_Position == null) return "INV_DOC";
                var _chkpostdesc = DB.HRPositions.FirstOrDefault(w => w.Code == _Position.ApplyPosition);
                if (EmailConf != null)
                {
                    var CCEmail = "";
                    var Interviewer = DB.RCMVInterviewers.Where(w => w.Code == objMatch.VacNo && w.IntStep == objMatch.IntVStep).ToList();
                    if (Interviewer.Count > 0)
                    {
                        CCEmail = User.Email + ";";
                    }
                    else CCEmail = User.Email;
                    foreach ( var Read in Interviewer)
                    {
                        var Staff_ = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == Read.EmpCode);
                        CCEmail += Staff_.Email + ";";
                    }
                    var _company = DP.SYHRCompanies.FirstOrDefault();
                    //if (objMatch.SendingSelected == "EM" || objMatch.SendingSelected == "Email")
                    //{
                    string Position = "";
                    if (_chkpostdesc == null) Position = "";
                    else Position = _chkpostdesc.Description;
                    string str = "Dear " + _Position.Title + "<b> " + Header.CandidateName + "</b>" + " ,";
                    //str += @"<br /> <p>Thank you for applying for the position of <b>" + Position + " with us. " +
                    //    "We are glad to inform you that your interview has been scheduled for " + Header.SetTime.Value.ToString("h:mm tt") +" on " + Header.AppointmentDate.Value.ToString("dd-MM-yyyy") ;
                    str += "<br /><br /> Kindly note the interview details:";
                    str += "<br /><br /> " + Header.Location;
                    str += "<br /> Please reply to this email if you have any questions or need to reschedule. We look forward to speaking with you.";
                    str += "<br /> Sincerely," + "</p>";
                    str += "<br />HR Department";

                    CFEmailAccount emailAccount = EmailConf;
                    string subject = "Interview Invitation for the position of " + Position + " at " + _company.CompENG;
                    string body = str;
                    string filePath = "";// Upload;
                    string fileName = "";// Path.GetFileName(filePath);
                    ClsEmail EmailObject = new ClsEmail();
                    int rs = EmailObject.SendMail(emailAccount, _Position.Email, User.Email,
                        subject, body, filePath, fileName);

                    //}
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
                log.ScreenId = Header.ApplicantID;
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
                log.DocurmentAction = Header.ApplicantID;
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
                log.DocurmentAction = Header.ApplicantID;
                log.Action = SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
    }
    public class FilterCandidate
    {
        public string Vacancy { get; set; }
        public string ApplyPost { get; set; }
        public int IntVStep { get; set; }
    }
}