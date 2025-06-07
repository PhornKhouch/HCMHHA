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

namespace Humica.Logic.RCM
{

    public class RCMRefChkPersonObject
    {

        HumicaDBContext DB = new HumicaDBContext();
        SMSystemEntity SMS = new SMSystemEntity();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public string ScreenId { get; set; }
        public RCMApplicant Filter { get; set; }
        public string DocType { get; set; }
        public RCMHire Hire { get; set; }
        public RCMRefCheckPerson Header { get; set; }
        public RCMApplicant App { get; set; }
        public RCMRefQuestionnaire RefQuestion { get; set; }
        public RCMAReference RefPerson { get; set; }
        public HRStaffProfile StaffPf { get; set; }
        public HREmpCareer HeaderCareer { get; set; }
        public List<RCMHire> ListHire { get; set; }
        public List<RCMPInterview> ListIntV { get; set; }
        public List<HREmpIdentity> ListEmpIdentity { get; set; }
        public List<RCMRefCheckPerson> ListRefPerson { get; set; }
        public List<MDUploadTemplate> ListTemplate { get; set; }
        public List<RCMApplicant> ListApplicant { get; set; }
        public List<RCMRefQuestionnaire> ListRefQuestion { get; set; }
        public List<RCMSRefQuestion> ListSRefQues { get; set; }
        public string ErrorMessage { get; set; }
        public RCMRefChkPersonObject()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }
        #region'hire'
        public string saveHire(string ApplicantID)
        {
            try
            {
                if (Hire.EmployeeType == null)
                    return "EMPTYPE_EN";
                if (Hire.Branch == null)
                    return "BRANCH_EN";
                if (Hire.Department == null)
                    return "DEPARTMENT_EN";
                if (Hire.Position == null)
                    return "POSITION_EN";
                if (Hire.PayParameter == null)
                    return "PAYPARAM_EN";
                if (Hire.TXPayType == null)
                    return "SALRY_PAID_EN";
                if (Hire.Level == null)
                    return "LEVEL_EN";
                if (Hire.ProbationType == null)
                    return "PROBATION_TYPE_EN";

                HumicaDBContext DB = new HumicaDBContext();

                var chkHire = DB.RCMHires.FirstOrDefault(w => w.ApplicantID == ApplicantID);
                if (chkHire != null)
                {
                    return "Candidate already hire";
                }
                StaffPf = new HRStaffProfile();
                var _Company = SMS.SYHRCompanies.FirstOrDefault();
                string Company = _Company.CompENG;
                var objNumber = new CFNumberRank(Hire.Branch, DocConfType.EmpCode, true);
                if (objNumber.NextNumberRank == null)
                {
                    return "NUMBER_RANK_NE";
                }
                StaffPf.EmpCode = objNumber.NextNumberRank;
                App = DB.RCMApplicants.FirstOrDefault(x => x.ApplicantID == ApplicantID);

                //Probation = DateNow.AddMonths(ProMonth);
                //var Probation = DateNow.AddMonths(3);
                StaffPf.CreatedOn = DateTime.Now.Date;
                StaffPf.CreatedBy = User.UserName;
                StaffPf.DateTerminate = new DateTime(1900, 1, 1);
                StaffPf.ReSalary = DateTime.Now;
                //StaffPf.LeaveConf = DateNow;
                StaffPf.CareerDesc = "NEWJOIN";
                StaffPf.BankName = "CC";
                StaffPf.IsCalSalary = true;
                StaffPf.IsResident = true;
                StaffPf.IsOTApproval = false;
                StaffPf.EmpType = "LOCAL";
                StaffPf.IsHold = false;
                StaffPf.Status = SYDocumentStatus.A.ToString();
                StaffPf.BankFee = 0;

                StaffPf.FirstName = App.FirstName;
                StaffPf.LastName = App.LastName;
                StaffPf.AllName = App.LastName + " " + App.FirstName;
                StaffPf.OthFirstName = App.OthFirstName;
                StaffPf.OthLastName = App.OthLastName;
                StaffPf.OthAllName = App.OthLastName + " " + App.OthFirstName;
                StaffPf.Sex = App.Gender;
                StaffPf.Title = App.Title;
                StaffPf.Email = App.Email;
                StaffPf.Nation = App.Nationality;
                StaffPf.Marital = App.Marital;
                StaffPf.Country = App.Country;
                StaffPf.POB = App.POB;
                StaffPf.Phone1 = App.Phone1;
                StaffPf.Phone2 = App.Phone2;
                StaffPf.ConAddress = App.CurAddr;
                StaffPf.Peraddress = App.PermanentAddr;
                StaffPf.DOB = App.DOB;
                StaffPf.CreatedOn = DateTime.Now;
                StaffPf.CreatedBy = User.UserName;
                StaffPf.CreatedBy = User.UserName;

                StaffPf.Branch = Hire.Branch;
                StaffPf.Division = Hire.Division;
                StaffPf.EmpType = Hire.EmployeeType;
                StaffPf.DEPT = Hire.Department;
                StaffPf.LOCT = Hire.Location;
                StaffPf.CATE = Hire.Category;
                StaffPf.LevelCode = Hire.Level;
                StaffPf.JobCode = Hire.Position;
                StaffPf.Salary = (decimal)Hire.Salary;
                StaffPf.StartDate = Hire.StartDate;
                StaffPf.Probation = Hire.ProbationEndDate;
                StaffPf.PayParam = Hire.PayParameter;
                StaffPf.JobGrade = Hire.JobGrade;
                StaffPf.TXPayType = Hire.TXPayType;
                StaffPf.Probation = Hire.ProbationEndDate;
                StaffPf.ProbationType = Hire.ProbationType;
                StaffPf.ROSTER = Hire.ROSTER;
                StaffPf.LeaveConf = Hire.LeaveConf;
                StaffPf.Line = Hire.Line;
                StaffPf.StaffType = Hire.StaffType;
                StaffPf.Images = Hire.Images;
                StaffPf.HODCode = Hire.HODCode;
                StaffPf.SalaryType = Hire.SalaryType;

                Hire.ApplicantID = ApplicantID;
                Hire.EmpCode = StaffPf.EmpCode;

                HeaderCareer = new HREmpCareer();
                HeaderCareer.EmpCode = Hire.EmpCode;
                HeaderCareer.CareerCode = "NEWJOIN";
                HeaderCareer.EmpType = Hire.EmployeeType;
                HeaderCareer.Branch = Hire.Branch;
                HeaderCareer.DEPT = Hire.Department;
                HeaderCareer.LOCT = Hire.Location;
                HeaderCareer.Division = Hire.Division;
                HeaderCareer.LINE = Hire.Line;
                HeaderCareer.SECT = Hire.Section;
                HeaderCareer.CATE = Hire.Category;
                HeaderCareer.LevelCode = Hire.Level;
                HeaderCareer.JobCode = Hire.Position;
                //HeaderCareer.JobDesc = Hire.POSTDESC;
                //HeaderCareer.JobSpec = Header.JOBSPEC;
                HeaderCareer.EstartSAL = Hire.Salary.ToString();
                //HeaderCareer.EIncrease = Hire.Salary.ToString();
                //HeaderCareer.ESalary = Header.ESalary;
                //HeaderCareer.SupCode = Header.HODCode.ToString();
                HeaderCareer.FromDate = Hire.StartDate;
                HeaderCareer.ToDate = Convert.ToDateTime("01/01/5000");
                HeaderCareer.EffectDate = Hire.StartDate;
                HeaderCareer.ProDate = Hire.StartDate;
                HeaderCareer.Reason = "New Join";
                HeaderCareer.Remark = "Welcome to " + Company;
                HeaderCareer.Appby = "";
                HeaderCareer.AppDate = Hire.StartDate.ToString("dd-MM-yyyy");
                HeaderCareer.VeriFyBy = "";
                HeaderCareer.VeriFYDate = Hire.StartDate.ToString("dd-MM-yyyy");
                HeaderCareer.LCK = 1;
                HeaderCareer.OldSalary = Hire.Salary.Value;
                HeaderCareer.Increase = 0;
                HeaderCareer.NewSalary = Hire.Salary.Value;
                HeaderCareer.JobGrade = Hire.JobGrade;
                //HeaderCareer.PersGrade = Hire.PersGrade;
                //HeaderCareer.HomeFunction = Header.HomeFunction;
                //HeaderCareer.Functions = Header.Functions;
                //HeaderCareer.SubFunction = Header.SubFunction;
                HeaderCareer.CreateBy = User.UserName;
                HeaderCareer.CreateOn = DateTime.Now;


                App.Status = SYDocumentStatus.HIRED.ToString();
                App.IsHired = true;
                DB.RCMApplicants.Attach(App);
                DB.Entry(App).Property(x => x.Status).IsModified = true;
                DB.Entry(App).Property(x => x.IsHired).IsModified = true;

                DB.HREmpCareers.Add(HeaderCareer);
                DB.HRStaffProfiles.Add(StaffPf);
                DB.RCMHires.Add(Hire);

                DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ApplicantID;
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
                log.DocurmentAction = ApplicantID;
                log.Action = SYActionBehavior.ADD.ToString();
                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        #endregion 
        public string RefCheck()
        {
            try
            {
                if (Header.NameOfRef == null) return "EEREFPERSON";

                DB = new HumicaDBContext();

                var chkRef = DB.RCMRefCheckPersons.FirstOrDefault(w => w.ApplicantID == Header.ApplicantID);

                if (chkRef != null) return "Candidate already check reference!";
                if (ListRefQuestion != null)
                {
                    foreach (var item in ListRefQuestion)
                    {
                        var Obj = new RCMRefQuestionnaire();
                        Obj.ApplicantID = Header.ApplicantID;
                        Obj.Question = item.Question;
                        Obj.Answer = item.Answer;
                        DB.RCMRefQuestionnaires.Add(Obj);
                    }
                }
                var _App = DB.RCMApplicants.FirstOrDefault(w => w.ApplicantID == Header.ApplicantID);
                if (_App != null)
                {
                    _App.RefCHK = true;
                    DB.RCMApplicants.Attach(_App);
                    DB.Entry(_App).Property(x => x.RefCHK).IsModified = true;
                }
                DB.RCMRefCheckPersons.Add(Header);

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
                log.Action = SYActionBehavior.ADD.ToString();
                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string UpdateRefCheck(string ApplicantID)
        {
            try
            {
                DB = new HumicaDBContext();

                var _chkRef = DB.RCMRefCheckPersons.FirstOrDefault(w => w.ApplicantID == ApplicantID);

                if (_chkRef == null) return "DOC_NE";
                var objMatch = DB.RCMRefQuestionnaires.Where(w => w.ApplicantID == Header.ApplicantID).ToList();
                foreach (var item in objMatch)
                {
                    DB.RCMRefQuestionnaires.Attach(item);
                    DB.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                }
                if (ListRefQuestion != null)
                {
                    foreach (var item in ListRefQuestion)
                    {
                        var Obj = new RCMRefQuestionnaire();
                        Obj.ApplicantID = Header.ApplicantID;
                        Obj.Question = item.Question;
                        Obj.Answer = item.Answer;
                        DB.RCMRefQuestionnaires.Add(Obj);
                    }
                }
                _chkRef.NameOfRef = Header.NameOfRef;
                _chkRef.OccupationOfRef = Header.OccupationOfRef;
                _chkRef.CompanyOfRef = Header.CompanyOfRef;
                _chkRef.PhoneNo = Header.PhoneNo;
                _chkRef.ReasonForLeaving = Header.ReasonForLeaving;
                _chkRef.Attachment = Header.Attachment;
                _chkRef.Description = Header.Description;
                _chkRef.Relationship = Header.Relationship;
                _chkRef.RefChkDate = Header.RefChkDate;
                _chkRef.CompanyCan = Header.CompanyCan;
                _chkRef.PositionCan = Header.PositionCan;

                DB.RCMRefCheckPersons.Attach(_chkRef);

                DB.Entry(_chkRef).Property(x => x.NameOfRef).IsModified = true;
                DB.Entry(_chkRef).Property(x => x.OccupationOfRef).IsModified = true;
                DB.Entry(_chkRef).Property(x => x.CompanyOfRef).IsModified = true;
                DB.Entry(_chkRef).Property(x => x.PhoneNo).IsModified = true;
                DB.Entry(_chkRef).Property(x => x.ReasonForLeaving).IsModified = true;
                DB.Entry(_chkRef).Property(x => x.Attachment).IsModified = true;
                DB.Entry(_chkRef).Property(x => x.Description).IsModified = true;
                DB.Entry(_chkRef).Property(x => x.Relationship).IsModified = true;
                DB.Entry(_chkRef).Property(x => x.RefChkDate).IsModified = true;
                DB.Entry(_chkRef).Property(x => x.CompanyCan).IsModified = true;
                DB.Entry(_chkRef).Property(x => x.PositionCan).IsModified = true;

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
                log.Action = SYActionBehavior.ADD.ToString();
                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
    }
}