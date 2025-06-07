using Humica.EF.MD;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using Humica.EF.Models.SY;
using Humica.Core.Process;

namespace DBHumica.Models.TRN
{
    public class TRTrainingObject
    {
        public SMSystemEntity DBI = new SMSystemEntity();
        //public DBBusinessProcess DB = new DBBusinessProcess();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public string ScreenId { get; set; }
        public TrainingRating Header { get; set; }
        public List<TrainingRating> ListTrainingRating { get; set; }
        public List<TRTrainingType> ListTrainingType { get; set; }
        public List<TRCourseCategory> ListCourseCategory { get; set; }
        public List<TRCourseType> ListCourseType { get; set; }
        public HRStaffProfile Staff { get; set; }
        public string CompanyCode { get; set; }
        public string Plant { get; set; }
        public string MessageCode { get; set; }

        public TRTrainingObject()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }

        public string CreateTrainingRating()
        {
            try
            {

                DBSystemHREntities DB = new DBSystemHREntities();
                DB.TrainingRatings.Add(Header);

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
                //log.DocurmentAction = ActionName;
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
                log.DocurmentAction = Header.Code;
                //log.DocurmentAction = ActionName;
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
                log.DocurmentAction = Header.Code;
                //log.DocurmentAction = ActionName;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string EditTraining()
        {
            try
            {
                DBSystemHREntities DB = new DBSystemHREntities();


                var program = DB.TrainingRatings.Find(Header.Code);
                program.Description = Header.Description;
                program.SecDescription = Header.SecDescription;
                program.Remark = Header.Remark;
                DB.TrainingRatings.Attach(program);
                DB.Entry(program).Property(x => x.Description).IsModified = true;
                DB.Entry(program).Property(x => x.SecDescription).IsModified = true;
                DB.Entry(program).Property(x => x.Remark).IsModified = true;
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
                //log.DocurmentAction = ActionName;
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
                log.DocurmentAction = Header.Code;
                //log.DocurmentAction = ActionName;
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

                //log.DocurmentAction = ActionName;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }

        public string DeleteTrainig()
        {
            try
            {
                DBSystemHREntities DB = new DBSystemHREntities();
                var program = DB.TrainingRatings.FirstOrDefault(w => w.Code == Header.Code);

                DB.TrainingRatings.Remove(program);

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
                //log.DocurmentAction = ActionName;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();
                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (DbUpdateException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();

                log.UserId = User.UserName;
                log.DocurmentAction = Header.Code;
                //log.DocurmentAction = ActionName;
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
                log.DocurmentAction = Header.Code;
                //log.DocurmentAction = ActionName;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
    }
}
