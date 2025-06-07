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
    public class RCMVacancyObject
    {
        public HumicaDBContext DB = new HumicaDBContext();
        public SMSystemEntity DBI = new SMSystemEntity();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public string ScreenId { get; set; }
        public string JobResponsibility { get; set; }
        public string JobRequirement { get; set; }
        public string PostOfJD { get; set; }
        public string MessageError { get; set; }
        public RCMVacancy Header { get; set; }
        public RCMRecruitRequest RecruitRequest { get; set; }
        public RCMApplicant Applicants { get; set; }
        public List<RCMVacancy> ListHeader { get; set; }
        public List<RCM_Vacancy_VIEW> ListVacView { get; set; }
        public List<RCMRecruitRequest> ListPending { get; set; }
        public RCMVInterviewer VInt { get; set; }
        public List<RCMVInterviewer> ListInt { get; set; }
        public List<RCMAdvertising> ListAdvertise { get; set; }
        public string Code { get; set; }
        public string MessageCode { get; set; }

        public RCMVacancyObject()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }

        #region 'Create'
        public string createVAC()
        {
            try
            {
                if (Header.VacancyType == null) return "VACTYPE_EN";

                var objCF = DB.ExCfWorkFlowItems.Where(w => w.ScreenID == ScreenId).ToList();

                if (objCF.Count() == 0) return "REQUEST_TYPE_NE";

                var objNumber = new CFNumberRank(objCF.First().NumberRank, ScreenId);

                if (objNumber.NextNumberRank == null) return "NUMBER_RANK_NE";

                Header.Code = objNumber.NextNumberRank;

                var _ReqForm = DB.RCMRecruitRequests.FirstOrDefault(w => w.RequestNo == Header.DocRef);
                if (_ReqForm != null)
                {
                    Header.Position = _ReqForm.POST;
                    Header.Sect = _ReqForm.Sect;
                    Header.Branch = _ReqForm.Branch;
                    Header.Dept = _ReqForm.DEPT;
                    Header.StaffType = _ReqForm.StaffType;
                    Header.WorkingType = _ReqForm.WorkingType;
                    Header.JobLevel = _ReqForm.JobLevel;
                }
                foreach (var read in ListInt.ToList())
                {
                    read.Code = Header.Code;
                    read.Position = Header.Position;
                    DB.RCMVInterviewers.Add(read);
                }
                foreach (var read in ListAdvertise.ToList())
                {
                    read.VacNo = Header.Code;
                    DB.RCMAdvertisings.Add(read);
                }
                Header.AppApplied = 0;
                Header.DocDate = DateTime.Now;
                Header.CreatedBy = User.UserName;
                Header.CreatedOn = DateTime.Now;

                DB.RCMVacancies.Add(Header);

                DB.SaveChanges();

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Header.Code;
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
                log.DocurmentAction = Header.Code;
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
                log.DocurmentAction = Header.Code;
                log.Action = SYActionBehavior.ADD.ToString();
                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        #endregion 
        #region 'Update'
        public string updateVAC(string Code)
        {
            try
            {
                DB = new HumicaDBContext();

                var ObjMatch = DB.RCMVacancies.FirstOrDefault(w => w.Code == Code);

                if (ObjMatch == null) return "DOC_INV";

                var checkdupListIntv = DB.RCMVInterviewers.Where(w => w.Code == Code).ToList();

                foreach (var read in checkdupListIntv.ToList())
                {
                    DB.RCMVInterviewers.Remove(read);
                }
                foreach (var read in ListInt.ToList())
                {
                    read.Code = ObjMatch.Code;
                    read.Position = ObjMatch.Position;
                    DB.RCMVInterviewers.Add(read);
                }

                ObjMatch.ChangedBy = User.UserName;
                ObjMatch.ChangedOn = DateTime.Now;
                ObjMatch.VacancyType = Header.VacancyType;
                ObjMatch.Description = Header.Description;
                ObjMatch.ClosedDate = Header.ClosedDate;

                DB.RCMVacancies.Attach(ObjMatch);
                DB.Entry(ObjMatch).Property(x => x.VacancyType).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Description).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ClosedDate).IsModified = true;
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
                log.DocurmentAction = Code;
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
                log.DocurmentAction = Code;
                log.Action = SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        #endregion
        #region 'Delete'
        public string deleteVAC(string Code)
        {
            try
            {
                DB = new HumicaDBContext();

                var ObjMatch = DB.RCMVacancies.FirstOrDefault(w => w.Code == Code);
                var _chkApplicant = DB.RCMApplicants.FirstOrDefault(w => w.VacNo == Code);

                string Process = SYDocumentStatus.PROCESSING.ToString();

                if (ObjMatch.Status == Process) return "DOC_INV";
                if (_chkApplicant != null) return "EE_APPCHK";

                var checkdupListInt = DB.RCMVInterviewers.Where(w => w.Code == Code).ToList();
                var checkdupListAds = DB.RCMAdvertisings.Where(w => w.VacNo == Code).ToList();

                foreach (var read in checkdupListInt.ToList())
                {
                    DB.RCMVInterviewers.Remove(read);
                }
                foreach (var read in checkdupListAds.ToList())
                {
                    DB.RCMAdvertisings.Remove(read);
                }
                DB.RCMVacancies.Remove(ObjMatch);

                DB.SaveChanges();

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Code;
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
                log.DocurmentAction = Code;
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
                log.DocurmentAction = Code;
                log.Action = SYActionBehavior.DELETE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        #endregion
        #region 'Convert Status'
        public string Processing(string Code)
        {
            try
            {
                HumicaDBContext DBI = new HumicaDBContext();

                RCMVacancy objmatch = DB.RCMVacancies.FirstOrDefault(w => w.Code == Code);

                if (objmatch == null) return "DOC_INV";

                objmatch.Status = SYDocumentStatus.PROCESSING.ToString();
                objmatch.ProcessBy = User.UserName;
                objmatch.ProcessDate = DateTime.Now;

                DB.RCMVacancies.Attach(objmatch);

                DB.Entry(objmatch).Property(w => w.Status).IsModified = true;
                DB.Entry(objmatch).Property(w => w.ProcessDate).IsModified = true;
                DB.Entry(objmatch).Property(w => w.ProcessBy).IsModified = true;

                DB.SaveChanges();

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Code;
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
                log.DocurmentAction = Code;
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
                log.DocurmentAction = Code;
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string Closed(string Code)
        {
            try
            {
                HumicaDBContext DBI = new HumicaDBContext();

                RCMVacancy objmatch = DB.RCMVacancies.FirstOrDefault(w => w.Code == Code);

                if (objmatch == null) return "DOC_INV";

                objmatch.Status = SYDocumentStatus.CLOSED.ToString();
                objmatch.ClosedDate = DateTime.Now;
                objmatch.ClosedBy = User.UserName;

                DB.RCMVacancies.Attach(objmatch);

                DB.Entry(objmatch).Property(w => w.Status).IsModified = true;
                DB.Entry(objmatch).Property(w => w.ClosedDate).IsModified = true;
                DB.Entry(objmatch).Property(w => w.ClosedBy).IsModified = true;

                DB.SaveChanges();

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Code;
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
                log.DocurmentAction = Code;
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
                log.DocurmentAction = Code;
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string Completed(string Code)
        {
            try
            {
                HumicaDBContext DBI = new HumicaDBContext();

                RCMVacancy objmatch = DB.RCMVacancies.FirstOrDefault(w => w.Code == Code);

                if (objmatch == null)
                    return "DOC_INV";

                objmatch.Status = SYDocumentStatus.COMPLETED.ToString();
                objmatch.ChangedBy = User.UserName;
                objmatch.ChangedOn = DateTime.Now;

                DB.RCMVacancies.Attach(objmatch);

                DB.Entry(objmatch).Property(w => w.Status).IsModified = true;
                DB.Entry(objmatch).Property(w => w.ChangedBy).IsModified = true;
                DB.Entry(objmatch).Property(w => w.ChangedOn).IsModified = true;

                DB.SaveChanges();

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Code;
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
                log.DocurmentAction = Code;
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
                log.DocurmentAction = Code;
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        #endregion 
    }
}
