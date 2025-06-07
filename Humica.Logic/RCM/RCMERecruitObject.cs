using Humica.EF.MD;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using System;
using System.Linq;
using Humica.Logic.CF;
using Humica.EF;
using Humica.EF.Models.SY;
using Humica.Core.DB;

namespace Humica.Logic.RCM
{
    public class RCMERecruitObject
    {
        public HumicaDBContext DB = new HumicaDBContext();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public string ScreenId { get; set; }
        public string MessageError { get; set; }
        public string DocType { get; set; }
        public RCMERecruit Header { get; set; }

        public SYSocialMedia Headersocial { get; set; }
        public List<RCMERecruit> ListHeader { get; set; }
        public List<RCMVacancy> ListPending { get; set; }
       // public List<RCMVAdvertisePlace> ListAdvertise { get; set; }

        public string Code { get; set; }
        public string MessageCode { get; set; }

        public RCMERecruitObject()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }

        #region 'Create'
        public string createERecruit()
        {
            try
            {
                if (Header.HirePosition == null)
                    return "EEHIREPOST";
                if (Header.PosterNo <= 0)
                    return "EEPOSTERNO";
                if (Header.ReviewBy == null)
                    return "EEREV";
                if (Header.VerifyBy == null)
                    return "EEVERIFY";

                var objCF = DB.ExCfWorkFlowItems.Find(ScreenId, DocType);
                if (objCF == null)
                {
                    return "REQUEST_TYPE_NE";
                }
                var objNumber = new CFNumberRank(objCF.DocType, objCF.ScreenID);
                if (objNumber.NextNumberRank == null)
                {
                    return "NUMBER_RANK_NE";
                }
                Header.JobID = objNumber.NextNumberRank;
                //foreach (var read in ListAdvertise.ToList())
                //{
                //    read.Code = Header.JobID;
                //    DB.RCMVAdvertisePlaces.Add(read);
                //}
                Header.Status = SYDocumentStatus.OPEN.ToString();
                Header.CreatedBy = User.UserName;
                Header.CreatedOn = DateTime.Now;
                Header.Attachfile= Header.Attachfile;
                Header.MediaType = Header.MediaType;
                

               
                DB.RCMERecruits.Add(Header);

                int row = DB.SaveChanges();

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Header.JobID;
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
                log.DocurmentAction = Header.JobID;
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
                log.DocurmentAction = Header.JobID;
                log.Action = SYActionBehavior.ADD.ToString();
                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        #endregion 
        #region 'Update'
        public string updERecruit(string JobID)
        {
            try
            {
                if (Header.HirePosition == null)
                    return "EEHIREPOST";
                if (Header.PosterNo <= 0)
                    return "EEPOSTERNO";
                if (Header.ReviewBy == null)
                    return "EEREV";
                if (Header.VerifyBy == null)
                    return "EEVERIFY";

                DB = new HumicaDBContext();

                var ObjMatch = DB.RCMERecruits.FirstOrDefault(w =>w.JobID == JobID);

                if (ObjMatch == null)
                    return "DOC_INV";

                //var ChkListAds = DB.RCMVAdvertisePlaces.Where(w => w.Code==JobID).ToList();
                //foreach (var read in ChkListAds.ToList())
                //{
                //    DB.RCMVAdvertisePlaces.Remove(read);
                //}
                //foreach (var read in ListAdvertise.ToList())
                //{
                //    read.Code = ObjMatch.JobID;
                //    DB.RCMVAdvertisePlaces.Add(read);
                //}
                ObjMatch.ChangedBy = User.UserName;
                ObjMatch.ChangedOn = DateTime.Now;
                ObjMatch.HirePosition = Header.HirePosition;
                ObjMatch.Status = Header.Status;
                ObjMatch.PosterNo = Header.PosterNo;
                ObjMatch.Location = Header.Location;
                ObjMatch.ContactInfo = Header.ContactInfo;
                ObjMatch.VerifyBy = Header.VerifyBy;
                ObjMatch.ReviewBy = Header.ReviewBy;
                ObjMatch.AckBy = Header.AckBy;
                ObjMatch.DateLine = Header.DateLine;
                ObjMatch.DatePosting = Header.DatePosting;
                ObjMatch.PersonInCharge = Header.PersonInCharge;
                ObjMatch.Attachfile = Header.Attachfile;
                ObjMatch.MediaType = Header.MediaType;


                DB.RCMERecruits.Attach(ObjMatch);

                
                DB.Entry(ObjMatch).Property(x => x.HirePosition).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Status).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.PosterNo).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Location).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ContactInfo).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.VerifyBy).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ReviewBy).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.AckBy).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.DateLine).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.DatePosting).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.PersonInCharge).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ChangedBy).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ChangedOn).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Attachfile).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.MediaType).IsModified = true;






                DB.SaveChanges();

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Header.JobID;
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
                log.DocurmentAction = Header.JobID;
                log.Action = SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }

        #endregion 
        #region 'Delete'
        public string deleteERecruit(string JobID)
        {
            try
            {
                DB = new HumicaDBContext();

                var ObjMatch = DB.RCMERecruits.FirstOrDefault(w => w.JobID == JobID);
                string Approve = SYDocumentStatus.APPROVED.ToString();
                if (ObjMatch.Status == Approve)
                    return "DOC_INV";
                //var checkdupListAds = DB.RCMVAdvertisePlaces.Where(w => w.Code == JobID).ToList();
                //foreach (var read in checkdupListAds.ToList())
                //{
                //    DB.RCMVAdvertisePlaces.Remove(read);
                //}
                DB.RCMERecruits.Remove(ObjMatch);
                DB.SaveChanges();

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = JobID;
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
                log.DocurmentAction = JobID;
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
                log.DocurmentAction = JobID;
                log.Action = SYActionBehavior.DELETE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        #endregion 
        #region 'Convert Status'
        public string Processing(string JobID)
        {
            try
            {
                HumicaDBContext DBI = new HumicaDBContext();
                
                RCMERecruit objmatch = DB.RCMERecruits.First(w => w.JobID == JobID);
                if (objmatch == null)
                {
                    return "DOC_INV";
                }

                objmatch.Status = SYDocumentStatus.PROCESSING.ToString();
                objmatch.ChangedBy = User.UserName;
                objmatch.ChangedOn = DateTime.Now;

                DB.RCMERecruits.Attach(objmatch);

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
                log.DocurmentAction = JobID;
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
                log.DocurmentAction = JobID;
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
                log.DocurmentAction = JobID;
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string Posting(string JobID)
        {
            try
            {
                HumicaDBContext DBI = new HumicaDBContext();

                RCMERecruit objmatch = DB.RCMERecruits.First(w => w.JobID == JobID);
                if (objmatch == null)
                {
                    return "DOC_INV";
                }

                objmatch.Status = SYDocumentStatus.POSTED.ToString();
                objmatch.ChangedBy = User.UserName;
                objmatch.ChangedOn = DateTime.Now;
                
                DB.RCMERecruits.Attach(objmatch);

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
                log.DocurmentAction = JobID;
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
                log.DocurmentAction = JobID;
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
                log.DocurmentAction = JobID;
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string Closed(string JobID)
        {
            try
            {
                HumicaDBContext DBI = new HumicaDBContext();

                RCMERecruit objmatch = DB.RCMERecruits.First(w => w.JobID == JobID);
                if (objmatch == null)
                {
                    return "DOC_INV";
                }
                objmatch.Status = SYDocumentStatus.CLOSED.ToString();
                objmatch.ChangedBy = User.UserName;
                objmatch.ChangedOn = DateTime.Now;

                DB.RCMERecruits.Attach(objmatch);

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
                log.DocurmentAction = JobID;
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
                log.DocurmentAction = JobID;
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
                log.DocurmentAction = JobID;
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        #endregion 
    }
}
