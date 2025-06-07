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
    public class RCMPShortListObject
    {
        public HumicaDBContext DB = new HumicaDBContext();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public string ScreenId { get; set; }
        public string MessageError { get; set; }
        public RCMApplicant Header { get; set; }
        public List<RCMApplicant> ListHeader { get; set; }
        public List<RCMAWorkHistory> ListWorkHistory { get; set; }
        public List<RCMAEdu> ListEdu { get; set; }
        public List<RCMALanguage> ListLang { get; set; }
        public List<RCMAReference> ListRef { get; set; }
        public FilterShortLsit Filtering { get; set; }
        public RCMPShortListObject()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }
        public string Passed(string ApplicantIDs)
        {
            try
            {
                HumicaDBContext DBI = new HumicaDBContext();

                string Reject = SYDocumentStatus.REJECTED.ToString();
                string[] ids = ApplicantIDs.Split(';');
                var applicant = DB.RCMApplicants;
                List<RCMApplicant> items = new List<RCMApplicant>();
                applicant.Where(x => string.IsNullOrEmpty(x.ShortList) && ids.Where(w => w == x.ApplicantID).Any()).ToList().ForEach(x => items.Add(x));
                if (items.Count == 0)
                {
                    return "CANDIDATE_INV";
                }
                foreach (RCMApplicant objmatch in items)
                {
                    if (objmatch == null)
                    {
                       return "CANDIDATE_INV";
                    }
                    if (objmatch.ShortList == Reject) return "SHORTLIST_EEREJ";

                    var chkStatus = DB.RCMPInterviews.FirstOrDefault(w => w.ApplicantID == objmatch.ApplicantID);

                    if (chkStatus != null) return "SHORTLIST_EE";

                    objmatch.ShortList = SYDocumentStatus.PASS.ToString();
                    objmatch.IntvStep = 1;

                    objmatch.CurStage = "ShortListing";
                    objmatch.IntVStatus = SYDocumentStatus.OPEN.ToString();
                    objmatch.ShortListingBy = User.UserName;
                    objmatch.ShortListingDate = DateTime.Now;

                    DB.RCMApplicants.Attach(objmatch);

                    DB.Entry(objmatch).Property(x => x.ShortList).IsModified = true;
                    DB.Entry(objmatch).Property(x => x.IntvStep).IsModified = true;
                    DB.Entry(objmatch).Property(x => x.IntVStatus).IsModified = true;
                    DB.Entry(objmatch).Property(x => x.ShortListingBy).IsModified = true;
                    DB.Entry(objmatch).Property(x => x.ShortListingDate).IsModified = true;
                    DB.Entry(objmatch).Property(x => x.CurStage).IsModified = true;

                    DB.SaveChanges();
                }

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ApplicantIDs;
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e);
                return "EE001";
            }
            catch (DbUpdateException e)
            {
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ApplicantIDs;
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                return "EE001";
            }
            catch (Exception e)
            {
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ApplicantIDs;
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                return "EE001";
            }
            return SYConstant.OK;

        }
        public string Reject(string ApplicantIDs)
        {
            try
            {
                HumicaDBContext DBI = new HumicaDBContext();

                string Reject = SYDocumentStatus.REJECTED.ToString();
                string[] ids = ApplicantIDs.Split(';');
                var applicant = DB.RCMApplicants;
                List<RCMApplicant> items = new List<RCMApplicant>();
                applicant.Where(x => string.IsNullOrEmpty(x.ShortList) && ids.Where(w => w == x.ApplicantID).Any()).ToList().ForEach(x => items.Add(x));
                if (items.Count == 0)
                {
                    return "CANDIDATE_INV";
                }
                foreach (RCMApplicant objmatch in items)
                {

                    if (objmatch == null)
                    {
                       return "CANDIDATE_INV";
                    }

                    if (objmatch.ShortList == Reject)
                    {
                       return "SHORTLIST_EEREJ";
                    }

                    var chkStatus = DB.RCMPInterviews.FirstOrDefault(w => w.ApplicantID == objmatch.ApplicantID);

                    if (chkStatus != null)
                    {
                        return "SHORTLIST_EE";
                    }

                    objmatch.ShortList = Reject;

                    objmatch.IntvStep = 0;

                    objmatch.CurStage = "ShortListing";
                    objmatch.ShortListingBy = User.UserName;
                    objmatch.ShortListingDate = DateTime.Now;

                    DB.RCMApplicants.Attach(objmatch);

                    DB.Entry(objmatch).Property(x => x.ShortList).IsModified = true;
                    DB.Entry(objmatch).Property(x => x.IntvStep).IsModified = true;
                    DB.Entry(objmatch).Property(x => x.ShortListingBy).IsModified = true;
                    DB.Entry(objmatch).Property(x => x.ShortListingDate).IsModified = true;
                    DB.Entry(objmatch).Property(x => x.CurStage).IsModified = true;

                    DB.SaveChanges();
                }
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ApplicantIDs;
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e);
                return "EE001";
            }
            catch (DbUpdateException e)
            {
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ApplicantIDs;
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                return "EE001";
            }
            catch (Exception e)
            {
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ApplicantIDs;
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                return "EE001";
            }
            return SYConstant.OK;

        }


        public string Fail(string ApplicantIDs)
        {

            try
            {
                HumicaDBContext DBI = new HumicaDBContext();

                string Reject = SYDocumentStatus.FAILURED.ToString();
                string[] ids = ApplicantIDs.Split(';');
                var applicant = DB.RCMApplicants;
                List<RCMApplicant> items = new List<RCMApplicant>();
                applicant.Where(x => string.IsNullOrEmpty(x.ShortList) && ids.Where(w => w == x.ApplicantID).Any()).ToList().ForEach(x => items.Add(x));
                if (items.Count == 0)
                {
                    return "CANDIDATE_INV";
                }
                foreach (RCMApplicant objmatch in items)
                {
                    if (objmatch == null)
                    {
                       return "CANDIDATE_INV";
                    }

                    if (objmatch.ShortList == Reject)
                    {
                       return "SHORTLIST_EEREJ";
                    }

                    var chkStatus = DB.RCMPInterviews.FirstOrDefault(w => w.ApplicantID == objmatch.ApplicantID);

                    if (chkStatus != null)
                    {
                        return "SHORTLIST_EE";
                    }
                    objmatch.ShortList = SYDocumentStatus.FAILURED.ToString();

                    objmatch.IntvStep = 0;

                    objmatch.CurStage = "ShortListing";
                    objmatch.ShortListingBy = User.UserName;
                    objmatch.ShortListingDate = DateTime.Now;

                    DB.RCMApplicants.Attach(objmatch);

                    DB.Entry(objmatch).Property(x => x.ShortList).IsModified = true;
                    DB.Entry(objmatch).Property(x => x.IntvStep).IsModified = true;
                    DB.Entry(objmatch).Property(x => x.ShortListingBy).IsModified = true;
                    DB.Entry(objmatch).Property(x => x.ShortListingDate).IsModified = true;
                    DB.Entry(objmatch).Property(x => x.CurStage).IsModified = true;

                    DB.SaveChanges();
                }
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ApplicantIDs;
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e);
                return "EE001";
            }
            catch (DbUpdateException e)
            {
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ApplicantIDs;
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                return "EE001";
            }
            catch (Exception e)
            {
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ApplicantIDs;
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                return "EE001";
            }
            return SYConstant.OK;
        }
        public string Kept(string ApplicantIDs)
        {
            try
            {
                HumicaDBContext DBI = new HumicaDBContext();

                string Reject = SYDocumentStatus.REJECTED.ToString();
                string[] ids = ApplicantIDs.Split(';');
                var applicant = DB.RCMApplicants;
                List<RCMApplicant> items = new List<RCMApplicant>();
                applicant.Where(x => string.IsNullOrEmpty(x.ShortList) && ids.Where(w => w == x.ApplicantID).Any()).ToList().ForEach(x => items.Add(x));
                if (items.Count == 0)
                {
                    return "CANDIDATE_INV";
                }
                foreach (RCMApplicant objmatch in items)
                {

                    if (objmatch == null)
                    {
                        return "CANDIDATE_INV";
                    }

                    if (objmatch.ShortList == Reject)
                    {
                       return "SHORTLIST_EEREJ";
                    }

                    var chkStatus = DB.RCMPInterviews.FirstOrDefault(w => w.ApplicantID == objmatch.ApplicantID);

                    if (chkStatus != null)
                    {
                        return "SHORTLIST_EE";
                    }

                    objmatch.ShortList = "KEEP";

                    objmatch.IntvStep = 0;

                    objmatch.CurStage = "ShortListing";
                    objmatch.ShortListingBy = User.UserName;
                    objmatch.ShortListingDate = DateTime.Now;

                    DB.RCMApplicants.Attach(objmatch);

                    DB.Entry(objmatch).Property(x => x.ShortList).IsModified = true;
                    DB.Entry(objmatch).Property(x => x.IntvStep).IsModified = true;
                    DB.Entry(objmatch).Property(x => x.ShortListingBy).IsModified = true;
                    DB.Entry(objmatch).Property(x => x.ShortListingDate).IsModified = true;
                    DB.Entry(objmatch).Property(x => x.CurStage).IsModified = true;

                    DB.SaveChanges();
                }
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ApplicantIDs;
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e);
                return "EE001";
            }
            catch (DbUpdateException e)
            {
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ApplicantIDs;
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                return "EE001";
            }
            catch (Exception e)
            {
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ApplicantIDs;
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                return "EE001";
            }
            return SYConstant.OK;
        }
    }
    public class FilterShortLsit
    {
        public string Vacancy { get; set; }
        public string ApplyPost { get; set; }
        public string Status { get; set; }
    }
}
