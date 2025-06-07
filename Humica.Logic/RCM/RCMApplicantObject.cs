using Humica.Core.DB;
using Humica.Core.FT;
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
    public class RCMApplicantObject
    {
        public HumicaDBContext DB = new HumicaDBContext();

        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public string ScreenId { get; set; }
        public RCMApplicant Header { get; set; }
        public RCMApplicant PersonalDate { get; set; }
        public RCMPInterview PIntV { get; set; }
        public List<RCMApplicant> ListHeader { get; set; }
        public List<RCMADependent> ListDependent { get; set; }
        public List<RCMAEdu> ListEdu { get; set; }
        public List<RCMALanguage> ListLang { get; set; }
        public List<RCMATraining> ListTraining { get; set; }
        public List<RCMAWorkHistory> ListWorkHistory { get; set; }
        public List<RCMAReference> ListRef { get; set; }
        public List<RCMAIdentity> ListIdentity { get; set; }
        public FTINYear Filter { get; set; }
        public Filters Filters { get; set; }
        public List<MDUploadTemplate> ListTemplate { get; set; }
        public Filtering Filtering { get; set; }
        public string Code { get; set; }
        public string MessageCode { get; set; }

        public RCMApplicantObject()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }

        public string createApplicant()
        {
            DB = new HumicaDBContext();
            try
            {
                if (Header.VacNo == null) return "EE_VAC";
                if (Header.FirstName == null) return "EE_FNAME";
                if (Header.LastName == null) return "EE_LNAME";

                var objCF = DB.ExCfWorkFlowItems.Where(w => w.ScreenID == ScreenId).ToList();

                if (objCF.Count() == 0) return "REQUEST_TYPE_NE";

                var objNumber = new CFNumberRank(objCF.First().NumberRank, ScreenId);

                if (objNumber.NextNumberRank == null) return "NUMBER_RANK_NE";

                Header.ApplicantID = objNumber.NextNumberRank;

                foreach (var read in ListDependent.ToList())
                {
                    read.ApplicantID = Header.ApplicantID;
                    DB.RCMADependents.Add(read);
                }
                foreach (var read in ListEdu.ToList())
                {
                    read.ApplicantID = Header.ApplicantID;
                    DB.RCMAEdus.Add(read);
                }
                foreach (var read in ListLang.ToList())
                {
                    read.ApplicantID = Header.ApplicantID;
                    DB.RCMALanguages.Add(read);
                }
                foreach (var read in ListTraining.ToList())
                {
                    read.ApplicantID = Header.ApplicantID;
                    DB.RCMATrainings.Add(read);
                }
                foreach (var read in ListWorkHistory.ToList())
                {
                    read.ApplicantID = Header.ApplicantID;
                    DB.RCMAWorkHistories.Add(read);
                }
                foreach (var read in ListRef.ToList())
                {
                    read.ApplicantID = Header.ApplicantID;
                    DB.RCMAReferences.Add(read);
                }
                foreach (var read in ListIdentity.ToList())
                {
                    read.ApplicantID = Header.ApplicantID;
                    DB.RCMAIdentities.Add(read);
                }
                Header.AllName = Header.LastName + " " + Header.FirstName;
                Header.OthAllName = Header.OthLastName + " " + Header.OthFirstName;
                var FIRST_LASTNAME = SYSettings.getSetting("FIRST_LASTNAME");
                var FIRST_LASTNAME_KH = SYSettings.getSetting("FIRST_LASTNAME_KH");
                if (FIRST_LASTNAME != null)
                {
                    if (FIRST_LASTNAME.SettinValue == "TRUE")
                    {
                        Header.AllName = Header.FirstName + " " + Header.LastName;
                    }
                }
                if (FIRST_LASTNAME_KH != null)
                {
                    if (FIRST_LASTNAME_KH.SettinValue == "TRUE")
                        Header.OthAllName = Header.OthFirstName + " " + Header.OthLastName;
                }
                Header.Status = SYDocumentStatus.APPLY.ToString();
                Header.DocDate = DateTime.Now;
                Header.Salary = 0;
                Header.SalaryAfterProb = 0;
                Header.ProposedSalary = 0;
                Header.CreatedBy = User.UserName;
                Header.CreatedOn = DateTime.Now;
                Header.IsHired = false;

                var updVac = DB.RCMVacancies.FirstOrDefault(w => w.Code == Header.VacNo);

                if (updVac != null)
                {
                    updVac.AppApplied += 1;
                    DB.RCMVacancies.Attach(updVac);
                    DB.Entry(updVac).Property(x => x.AppApplied).IsModified = true;

                    Header.Sect = updVac.Sect;
                    Header.StaffType = updVac.StaffType;
                    Header.JobLevel = updVac.JobLevel;
                }

                DB.RCMApplicants.Add(Header);

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
                log.DocurmentAction = Header.ApplicantID;
                log.Action = SYActionBehavior.ADD.ToString();
                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string updateApplicant(string ApplicantID)
        {
            try
            {
                DB = new HumicaDBContext();

                var ObjMatch = DB.RCMApplicants.FirstOrDefault(w => w.ApplicantID == ApplicantID);

                if (ObjMatch == null) return "DOC_INV";

                var chkLstDep = DB.RCMADependents.Where(w => w.ApplicantID == ApplicantID).ToList();
                var chkLstEdu = DB.RCMAEdus.Where(w => w.ApplicantID == ApplicantID).ToList();
                var chkLstLang = DB.RCMALanguages.Where(w => w.ApplicantID == ApplicantID).ToList();
                var chkLstTrain = DB.RCMATrainings.Where(w => w.ApplicantID == ApplicantID).ToList();
                var chkLstWorkHistory = DB.RCMAWorkHistories.Where(w => w.ApplicantID == ApplicantID).ToList();
                var chkLstRef = DB.RCMAReferences.Where(w => w.ApplicantID == ApplicantID).ToList();
                var chkdupLstIdent = DB.RCMAIdentities.Where(w => w.ApplicantID == ApplicantID).ToList();

                foreach (var read in chkLstDep.ToList())
                {
                    DB.RCMADependents.Remove(read);
                }
                foreach (var read in chkLstEdu.ToList())
                {
                    DB.RCMAEdus.Remove(read);
                }
                foreach (var read in chkLstLang.ToList())
                {
                    DB.RCMALanguages.Remove(read);
                }
                foreach (var read in chkLstTrain.ToList())
                {
                    DB.RCMATrainings.Remove(read);
                }
                foreach (var read in chkLstWorkHistory.ToList())
                {
                    DB.RCMAWorkHistories.Remove(read);
                }
                foreach (var read in chkLstRef.ToList())
                {
                    DB.RCMAReferences.Remove(read);
                }
                foreach (var read in chkdupLstIdent.ToList())
                {
                    DB.RCMAIdentities.Remove(read);
                }

                foreach (var read in ListDependent.ToList())
                {
                    read.ApplicantID = ObjMatch.ApplicantID;
                    DB.RCMADependents.Add(read);
                }
                foreach (var read in ListEdu.ToList())
                {
                    read.ApplicantID = ObjMatch.ApplicantID;
                    DB.RCMAEdus.Add(read);
                }
                foreach (var read in ListLang.ToList())
                {
                    read.ApplicantID = ObjMatch.ApplicantID;
                    DB.RCMALanguages.Add(read);
                }
                foreach (var read in ListTraining.ToList())
                {
                    read.ApplicantID = ObjMatch.ApplicantID;
                    DB.RCMATrainings.Add(read);
                }
                foreach (var read in ListWorkHistory.ToList())
                {
                    read.ApplicantID = ObjMatch.ApplicantID;
                    DB.RCMAWorkHistories.Add(read);
                }
                foreach (var read in ListRef.ToList())
                {
                    read.ApplicantID = ObjMatch.ApplicantID;
                    DB.RCMAReferences.Add(read);
                }
                foreach (var read in ListIdentity.ToList())
                {
                    read.ApplicantID = ObjMatch.ApplicantID;
                    DB.RCMAIdentities.Add(read);
                }

                if (ObjMatch.VacNo != Header.VacNo)
                {
                    var updVac = DB.RCMVacancies.FirstOrDefault(w => w.Code == ObjMatch.VacNo);
                    if (updVac != null)
                    {
                        updVac.AppApplied -= 1;
                        DB.RCMVacancies.Attach(updVac);
                        DB.Entry(updVac).Property(x => x.AppApplied).IsModified = true;
                    }
                    var updCurVac = DB.RCMVacancies.FirstOrDefault(w => w.Code == Header.VacNo);
                    if (updCurVac != null)
                    {
                        updCurVac.AppApplied += 1;
                        DB.RCMVacancies.Attach(updCurVac);
                        DB.Entry(updCurVac).Property(x => x.AppApplied).IsModified = true;
                    }
                }
                ObjMatch.AllName = Header.LastName + " " + Header.FirstName;
                ObjMatch.OthAllName = Header.OthLastName + " " + Header.OthFirstName;
                var FIRST_LASTNAME = SYSettings.getSetting("FIRST_LASTNAME");
                var FIRST_LASTNAME_KH = SYSettings.getSetting("FIRST_LASTNAME_KH");
                if (FIRST_LASTNAME != null)
                {
                    if (FIRST_LASTNAME.SettinValue == "TRUE")
                    {
                        ObjMatch.AllName = Header.FirstName + " " + Header.LastName;
                    }
                }
                if (FIRST_LASTNAME_KH != null)
                {
                    if (FIRST_LASTNAME_KH.SettinValue == "TRUE")
                        ObjMatch.OthAllName = Header.OthFirstName + " " + Header.OthLastName;
                }

                ObjMatch.CurStage = "APPLY";
                ObjMatch.ChangedBy = User.UserName;
                ObjMatch.ChangedOn = DateTime.Now;
                ObjMatch.VacNo = Header.VacNo;
                ObjMatch.ApplyDate = Header.ApplyDate;
                ObjMatch.ApplyBranch = Header.ApplyBranch;
                ObjMatch.FirstName = Header.FirstName;
                ObjMatch.LastName = Header.LastName;
                ObjMatch.ApplyPosition = Header.ApplyPosition;
                ObjMatch.ApplyDept = Header.ApplyDept;
                ObjMatch.OthFirstName = Header.OthFirstName;
                ObjMatch.OthLastName = Header.OthLastName;
               
                ObjMatch.ExpectSalary = Header.ExpectSalary;
                ObjMatch.Gender = Header.Gender;
                ObjMatch.DOB = Header.DOB;
                ObjMatch.Email = Header.Email;
                ObjMatch.Phone1 = Header.Phone1;
                ObjMatch.Source = Header.Source;
                ObjMatch.CurAddr = Header.CurAddr;
                ObjMatch.PermanentAddr = Header.PermanentAddr;
                ObjMatch.ResumeFile = Header.ResumeFile;
                ObjMatch.Nationality = Header.Nationality;

                DB.RCMApplicants.Attach(ObjMatch);

                DB.Entry(ObjMatch).Property(x => x.VacNo).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ApplyDate).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ApplyBranch).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ApplyDept).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.FirstName).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.LastName).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ApplyPosition).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.OthLastName).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.OthFirstName).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.AllName).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.OthAllName).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ExpectSalary).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Gender).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.DOB).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Email).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Phone1).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Source).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.CurAddr).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.PermanentAddr).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ResumeFile).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ChangedBy).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ChangedOn).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Nationality).IsModified = true;

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
        public string deleteApplicant(string ApplicantID)
        {
            try
            {
                DB = new HumicaDBContext();

                var ObjMatch = DB.RCMApplicants.FirstOrDefault(w => w.ApplicantID == ApplicantID);
                var PInterview = DB.RCMPInterviews.FirstOrDefault(w => w.ApplicantID == ApplicantID);
                string Pass = SYDocumentStatus.PASS.ToString();

                if (ObjMatch == null) return "DOC_INV";
                if (ObjMatch.Status == Pass) return "EE_APPCHK";
                if (PInterview != null) return "EE_APPCHK";

                var checkdupListDpt = DB.RCMADependents.Where(w => w.ApplicantID == ApplicantID).ToList();
                var checkdupListEdu = DB.RCMAEdus.Where(w => w.ApplicantID == ApplicantID).ToList();
                var checkdupListlang = DB.RCMALanguages.Where(w => w.ApplicantID == ApplicantID).ToList();
                var checkdupListtraining = DB.RCMATrainings.Where(w => w.ApplicantID == ApplicantID).ToList();
                var checkdupListWorkHistory = DB.RCMAWorkHistories.Where(w => w.ApplicantID == ApplicantID).ToList();
                var checkdupListRef = DB.RCMAReferences.Where(w => w.ApplicantID == ApplicantID).ToList();
                var checkdupListIdent = DB.RCMAIdentities.Where(w => w.ApplicantID == ApplicantID).ToList();

                foreach (var read in checkdupListDpt.ToList())
                {
                    DB.RCMADependents.Remove(read);
                }
                foreach (var read in checkdupListEdu.ToList())
                {
                    DB.RCMAEdus.Remove(read);
                }
                foreach (var read in checkdupListlang.ToList())
                {
                    DB.RCMALanguages.Remove(read);
                }
                foreach (var read in checkdupListtraining.ToList())
                {
                    DB.RCMATrainings.Remove(read);
                }
                foreach (var read in checkdupListWorkHistory.ToList())
                {
                    DB.RCMAWorkHistories.Remove(read);
                }
                foreach (var read in checkdupListRef.ToList())
                {
                    DB.RCMAReferences.Remove(read);
                }
                var updVac = DB.RCMVacancies.FirstOrDefault(w => w.Code == Header.VacNo);
                if (updVac != null)
                {
                    updVac.AppApplied -= 1;
                    DB.RCMVacancies.Attach(updVac);
                    DB.Entry(updVac).Property(x => x.AppApplied).IsModified = true;
                }
                foreach (var read in checkdupListIdent.ToList())
                {
                    DB.RCMAIdentities.Remove(read);
                }
                DB.RCMApplicants.Remove(ObjMatch);
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
                log.DocurmentAction = ApplicantID;
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
                log.DocurmentAction = ApplicantID;
                log.Action = SYActionBehavior.DELETE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string upload()
        {
            try
            {
                if (ListHeader.Count == 0)
                {
                    return "NO_DATA";
                }
                try
                {
                    DB.Configuration.AutoDetectChangesEnabled = false;
                    var _list = new List<RCMApplicant>();
                    List<RCMApplicant> _listStaff = new List<RCMApplicant>();

                    var Employee = DB.RCMApplicants;
                    _listStaff = Employee.ToList();

                    var date = DateTime.Now;
                    foreach (var staffs in ListHeader.ToList())
                    {
                        Header = new RCMApplicant();
                        var EmpCode = _listStaff.Where(w => w.ApplicantID == staffs.ApplicantID).ToList();
                        Header.ApplicantID = "";
                        if (EmpCode.Count <= 1)
                        {
                            if (EmpCode.Count == 1)
                            {
                                Header.ApplicantID = EmpCode.FirstOrDefault().ApplicantID;
                            }
                            Header.VacNo = staffs.VacNo;
                            Header.ApplicantID = staffs.ApplicantID;
                            Header.FirstName = staffs.FirstName;
                            Header.LastName = staffs.LastName;
                            Header.OthFirstName = staffs.OthFirstName;
                            Header.OthLastName = staffs.OthLastName;

                            Header.AllName = Header.LastName + " " + Header.FirstName;
                            Header.OthAllName = Header.OthLastName + " " + Header.OthFirstName;
                            var FIRST_LASTNAME = SYSettings.getSetting("FIRST_LASTNAME");
                            var FIRST_LASTNAME_KH = SYSettings.getSetting("FIRST_LASTNAME_KH");
                            if (FIRST_LASTNAME != null)
                            {
                                if (FIRST_LASTNAME.SettinValue == "TRUE")
                                {
                                    Header.AllName = Header.FirstName + " " + Header.LastName;
                                }
                            }
                            if (FIRST_LASTNAME_KH != null)
                            {
                                if (FIRST_LASTNAME_KH.SettinValue == "TRUE")
                                    Header.OthAllName = Header.OthFirstName + " " + Header.OthLastName;
                            }
                            
                            Header.Gender = staffs.Gender;
                            Header.Title = staffs.Title;
                            Header.ShortList = SYDocumentStatus.OPEN.ToString();
                            Header.Marital = staffs.Marital;
                            Header.DOB = staffs.DOB;
                            Header.ApplyBranch = staffs.ApplyBranch;
                            Header.ApplyPosition = staffs.ApplyPosition;
                            Header.WorkingType = staffs.WorkingType;
                            Header.ApplyDate = staffs.ApplyDate;
                            Header.ExpectSalary = staffs.ExpectSalary;
                            Header.Phone1 = staffs.Phone1;
                            Header.Email = staffs.Email;
                            Header.ResumeFile = null;//staffs.ResumeFile;
                            Header.CreatedBy = User.UserName;
                            Header.CreatedOn = DateTime.Now;
                            DB.RCMApplicants.Add(Header);
                        }

                    }

                    DB.SaveChanges();
                    return SYConstant.OK;
                }
                finally
                {
                    DB.Configuration.AutoDetectChangesEnabled = true;
                }
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Header.ApplicantID;
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
                log.DocurmentAction = Header.ApplicantID;
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
                log.DocurmentAction = Header.ApplicantID;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
    }

    public class Filtering
    {
        public string Vacancy { get; set; }
        public string Branch { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
