using Humica.Core.DB;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;

namespace Humica.Logic.RCM
{
    public class RCMPInterviewObject
    {
        public HumicaDBContext DB = new HumicaDBContext();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public RCMApplicant Filter { get; set; }
        public string ApplyPosition { get; set; }
        public string DocType { get; set; }
        public string Vacancy { get; set; }
        public string ChkData { get; set; }
        public int IntvStep { get; set; }
        public RCMPInterview Header { get; set; }
        public RCMApplicant App { get; set; }
        public string ScreenId { get; set; }
        public List<MDUploadTemplate> ListTemplate { get; set; }
        public List<RCMPInterview> ListInterview { get; set; }
        public List<RCMPInterview> ListWaiting { get; set; }
        public List<RCMAEdu> ListEdu { get; set; }
        public List<RCMAWorkHistory> ListWorkHistory { get; set; }
        public List<RCMALanguage> ListLanguage { get; set; }
        public List<RCMInterveiwFactor> ListFactor { get; set; }
        public List<RCMEmpEvaluateScore> ListScore { get; set; }
        public List<RCMInterveiwRating> ListInterviewRating { get; set; }
        public List<RCMInterveiwRegion> ListRegion { get; set; }
        public string ErrorMessage { get; set; }

        public RCMPInterviewObject()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }
        #region 'CreateInterview'
        public string createIntV(string TranNo)
        {
            try
            {
                //if (Header.Status == null) return "EESTATUS";
                int Tran = Convert.ToInt32(TranNo);
                DB = new HumicaDBContext();
                var lstRating = DB.RCMInterveiwRatings.ToList();
                var Open = SYDocumentStatus.OPEN.ToString();
                var Pass = SYDocumentStatus.PASS.ToString();
                var ObjMatch = DB.RCMPInterviews.FirstOrDefault(w => w.TranNo == Tran);

                if (ObjMatch != null)
                {
                    if (ObjMatch.Status != Open)
                        return "Candidate interview already!";
                }

                var chkApplicant = DB.RCMApplicants.FirstOrDefault(w => w.ApplicantID == ObjMatch.ApplicantID);
                ObjMatch.TotalScore = 0;
                foreach (var read in ListScore)//.Where(w => w.Code == item.Code).ToList()
                {
                    var _Factor = DB.RCMInterveiwFactors.FirstOrDefault(w => w.Code == read.Code);
                    var obj = new RCMEmpEvaluateScore();
                    obj.Applicant = ObjMatch.ApplicantID;
                    obj.Region = _Factor.Region;
                    obj.Description = _Factor.Description;
                    obj.SecDescription = _Factor.SecDescription;
                    obj.Code = read.Code;
                    obj.Score = read.Score;
                    obj.InVStep = ObjMatch.IntVStep;
                    ObjMatch.TotalScore += (int)read.Score;
                    obj.Remark = read.Remark;
                    DB.RCMEmpEvaluateScores.Add(obj);
                }
                var Rating = DB.RCMInterveiwRegions.ToList().Sum(w => w.Rating);
                var Resul = ListScore.ToList().Sum(w => w.Score);
                if (Resul >= Rating)
                {
                    ObjMatch.Result = Pass;
                    if (Header.Status != "REJECT" && Header.Status != "Consider") ObjMatch.Status = Pass;
                    else ObjMatch.Status = Header.Status;
                }
                else
                {
                    ObjMatch.Result = "FAIL";
                    if (Header.Status != "REJECT" && Header.Status != "Consider") ObjMatch.Status = "FAIL";
                    else ObjMatch.Status = Header.Status;
                }
                if (chkApplicant != null)
                {
                    if (Header.Status == "NEXTSTEP")
                    {
                        chkApplicant.IntvStep = ObjMatch.IntVStep + 1;
                        chkApplicant.IntVStatus = Open;
                        chkApplicant.CurStage = "Interview Step " + ObjMatch.IntVStep;
                    }
                    else
                    {
                        chkApplicant.IntVStatus = Header.Status;
                        chkApplicant.CurStage = Header.Status + " in Interview";
                    }
                    chkApplicant.PostOffer = Header.PositionOffer;
                    chkApplicant.SalaryAfterProb = Header.SalaryAfterProb;
                    chkApplicant.Salary = Header.ProposedSalary;
                    DB.RCMApplicants.Attach(chkApplicant);
                    DB.Entry(chkApplicant).Property(x => x.CurStage).IsModified = true;
                    DB.Entry(chkApplicant).Property(x => x.IntVStatus).IsModified = true;
                    DB.Entry(chkApplicant).Property(x => x.IntvStep).IsModified = true;
                    DB.Entry(chkApplicant).Property(x => x.SalaryAfterProb).IsModified = true;
                    DB.Entry(chkApplicant).Property(x => x.Salary).IsModified = true;
                }

                ObjMatch.ApplyDate = Header.ApplyDate;
                ObjMatch.ReStatus = Header.ReStatus;
                ObjMatch.IntVDate = Header.IntVDate;
                ObjMatch.Strength = Header.Strength;
                ObjMatch.Status = Header.Status;
                ObjMatch.Weakness = Header.Weakness;
                ObjMatch.IntCmt = Header.IntCmt;
                ObjMatch.ProposedSalary = Header.ProposedSalary;
                ObjMatch.PositionOffer = Header.PositionOffer;
                ObjMatch.AttachFile = Header.AttachFile;
                ObjMatch.SalaryAfterProb = Header.SalaryAfterProb;

                DB.RCMPInterviews.Attach(ObjMatch);

                DB.Entry(ObjMatch).Property(x => x.VacNo).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Result).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.TotalScore).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Status).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ReStatus).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ApplyDate).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.IntVDate).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Strength).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Weakness).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.IntCmt).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ProposedSalary).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.PositionOffer).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.AttachFile).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.SalaryAfterProb).IsModified = true;

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
        #region 'Cancel'
        public string Cancel(string TranNo)
        {
            try
            {
                HumicaDBContext DBI = new HumicaDBContext();
                int Tran = Convert.ToInt32(TranNo);
                RCMPInterview objmatch = DB.RCMPInterviews.First(w => w.TranNo == Tran);

                if (objmatch == null) return "DOC_INV";

                var chkApplicant = DB.RCMApplicants.FirstOrDefault(w => w.ApplicantID == objmatch.ApplicantID);
                if (chkApplicant != null)
                {
                    chkApplicant.IntVStatus = SYDocumentStatus.CANCELLED.ToString();
                    DB.RCMApplicants.Attach(chkApplicant);
                    DB.Entry(chkApplicant).Property(x => x.IntVStatus).IsModified = true;
                }
                objmatch.Status = SYDocumentStatus.CANCELLED.ToString();
                DB.RCMPInterviews.Attach(objmatch);
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
                log.DocurmentAction = TranNo;
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
                log.DocurmentAction = TranNo;
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
                log.DocurmentAction = TranNo;
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        #endregion
        public string NextStep(string Code)
        {
            try
            {
                string Pending = SYDocumentStatus.PENDING.ToString();
                string Open = SYDocumentStatus.OPEN.ToString();
                var ObjMatch = DB.RCMPInterviews.Where(w => w.ApplicantID == Code).ToList().OrderBy(w => w.IntVStep).Last();
                var DBU = new HumicaDBContext();
                if (ObjMatch != null)
                {
                    var _interview = new RCMPInterview();
                    _interview.ApplyDate = ObjMatch.ApplyDate;
                    _interview.ApplicantID = ObjMatch.ApplicantID;
                    _interview.Status = Open;
                    _interview.ReStatus = Open;
                    _interview.IntVDate = ObjMatch.IntVDate;
                    _interview.IntVStep = ObjMatch.IntVStep + 1;
                    _interview.Strength = ObjMatch.Strength;
                    _interview.Weakness = ObjMatch.Weakness;
                    _interview.IntCmt = ObjMatch.IntCmt;
                    _interview.ProposedSalary = ObjMatch.ProposedSalary;
                    _interview.PositionOffer = ObjMatch.PositionOffer;
                    _interview.AttachFile = ObjMatch.AttachFile;
                    _interview.SalaryAfterProb = ObjMatch.SalaryAfterProb;
                    _interview.CandidateName = ObjMatch.CandidateName;
                    _interview.VacNo = ObjMatch.VacNo;
                    _interview.ApplyPost = ObjMatch.ApplyPost;
                    _interview.AppointmentDate = ObjMatch.AppointmentDate;
                    _interview.DocType = ObjMatch.DocType;
                    _interview.StartTime = ObjMatch.StartTime;
                    _interview.EndTime = ObjMatch.EndTime;
                    _interview.Location = ObjMatch.Location;
                    _interview.Remark = ObjMatch.Remark;
                    _interview.DocDate = ObjMatch.DocDate;
                    DB.RCMPInterviews.Add(_interview);
                    DB.SaveChanges();
                }
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
}