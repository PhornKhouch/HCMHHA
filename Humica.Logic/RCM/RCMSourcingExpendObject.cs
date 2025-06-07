using Humica.Core.DB;
using Humica.Core.SY;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;

namespace Humica.Logic.RCM
{
    public class RCMSourcingExpendObject
    {
        HumicaDBContext DB = new HumicaDBContext();
        public SYUser User { get; set; }

        public string MessageError { get; set; }

        public SYUserBusiness BS { get; set; }

        public string SaleOrderNo { get; set; }

        public string ScreenId { get; set; }

        public string DocType { get; set; }

        public RCMSourcingExpend Header { get; set; }

        public List<RCMSourcingExpend> ListHeader { get; set; }

        public CFDocType DocTypeObject { get; set; }

        public decimal VATRate { get; set; }
        public string PLANT { get; set; }

        public string Token { get; set; }

        public string PenaltyNo { get; set; }

        public bool IsSave { get; set; }

        public RCMSourcingExpendObject()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }

        public string createRCMSourcingExpend()
        {
            try
            {
                HumicaDBContext DB = new HumicaDBContext();
                if (string.IsNullOrEmpty(Header.ExpendType)) { return "ET"; }
                else
                if (string.IsNullOrEmpty(Header.Remark)) { return "RM"; }
                Header.CreatedDate = DateTime.Now.Date;
                Header.CreatedBy = User.UserName;
                DB.RCMSourcingExpends.Add(Header);

                int row = DB.SaveChanges();

                return "OK";
            }
            catch (DbEntityValidationException e)
            {
                SYEventLog sYEventLog = new SYEventLog();
                sYEventLog.ScreenId = ScreenId;
                sYEventLog.UserId = User.UserName;
                sYEventLog.Action = SYActionBehavior.ADD.ToString();
                SYEventLogObject.saveEventLog(sYEventLog, e);
                return "EE001";
            }
            catch (DbUpdateException e2)
            {
                SYEventLog sYEventLog2 = new SYEventLog();
                sYEventLog2.ScreenId = ScreenId;
                sYEventLog2.UserId = User.UserName;
                sYEventLog2.Action = SYActionBehavior.ADD.ToString();
                SYEventLogObject.saveEventLog(sYEventLog2, e2, catchError: true);
                return "EE001";
            }
            catch (Exception e3)
            {
                SYEventLog sYEventLog3 = new SYEventLog();
                sYEventLog3.ScreenId = ScreenId;
                sYEventLog3.UserId = User.UserName;
                sYEventLog3.Action = SYActionBehavior.ADD.ToString();
                SYEventLogObject.saveEventLog(sYEventLog3, e3, b: true);
                return "EE001";
            }
        }

        public string EditRCMSourcingExpend(int ID)
        {
            try
            {
                RCMSourcingExpend RCMSourcingExpend = DB.RCMSourcingExpends.Find(ID);
                if (string.IsNullOrEmpty(Header.ExpendType)) { return "ET"; }
                else if (string.IsNullOrEmpty(Header.Remark)) { return "RM"; }
                if (RCMSourcingExpend == null)
                {
                    return "INVALID_DATA";
                }

                RCMSourcingExpend.DocumentDate = DateTime.Now;
                RCMSourcingExpend.Amount = Header.Amount;
                RCMSourcingExpend.Remark = Header.Remark;
                RCMSourcingExpend.ExpendType = Header.ExpendType;
                RCMSourcingExpend.DocumentReference = Header.DocumentReference;
                RCMSourcingExpend.AttachFile = Header.AttachFile;
                RCMSourcingExpend.VacancyNumber = Header.VacancyNumber;
                RCMSourcingExpend.ChangeDate = DateTime.Now;
                RCMSourcingExpend.ChangedBy = User.UserName;
                DB.RCMSourcingExpends.Attach(RCMSourcingExpend);
                DB.Entry(RCMSourcingExpend).State = System.Data.Entity.EntityState.Modified;
                int num = DB.SaveChanges();
                return "OK";
            }
            catch (DbEntityValidationException e)
            {
                SYEventLog sYEventLog = new SYEventLog();
                sYEventLog.ScreenId = ScreenId;
                sYEventLog.UserId = User.UserID.ToString();
                sYEventLog.DocurmentAction = "RCMSourcingExpend";
                sYEventLog.Action = SYActionBehavior.EDIT.ToString();
                SYEventLogObject.saveEventLog(sYEventLog, e);
                return "EE001";
            }
            catch (DbUpdateException e2)
            {
                SYEventLog sYEventLog2 = new SYEventLog();
                sYEventLog2.ScreenId = ScreenId;
                sYEventLog2.UserId = User.UserID.ToString();
                sYEventLog2.DocurmentAction = "RCMSourcingExpend";
                sYEventLog2.Action = SYActionBehavior.EDIT.ToString();
                SYEventLogObject.saveEventLog(sYEventLog2, e2, catchError: true);
                return "EE001";
            }
        }

        public string DeleteRCMSourcingExpend(int ID)
        {
            try
            {
                Header = new RCMSourcingExpend();
                var objMatch = DB.RCMSourcingExpends.FirstOrDefault(w => w.ID == ID);
                if (objMatch == null)
                {
                    return "EDCUATION _NE";
                }
                Header.ID = ID;
                DB.RCMSourcingExpends.Attach(objMatch);
                DB.Entry(objMatch).State = System.Data.Entity.EntityState.Deleted;
                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserID.ToString();
                log.ScreenId = Header.ID.ToString();
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (Exception e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserID.ToString();
                log.ScreenId = Header.ID.ToString();
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
    }
}