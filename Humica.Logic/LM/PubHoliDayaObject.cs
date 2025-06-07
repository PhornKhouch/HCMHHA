using Humica.Core.DB;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;

namespace Humica.Logic.LM
{
    public class PubHoliDayaObject
    {
        public HumicaDBContext DB = new HumicaDBContext();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public string ScreenId { get; set; }
        public string MessageError { get; set; }
        public HRPubHoliday Header { get; set; }
        public List<HRPubHoliday> ListHeader { get; set; }
        public List<MDUploadTemplate> ListTemplate { get; set; }

        public PubHoliDayaObject()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }
        public string creatPubHoliDay()
        {
            try
            {
                Header.CreatedOn = DateTime.Now.Date;
                Header.CreatedBy = User.UserName;
                DB.HRPubHolidays.Add(Header);

                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Header.TranNo.ToString();
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
                log.DocurmentAction = Header.TranNo.ToString();
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
                log.DocurmentAction = Header.TranNo.ToString();
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string editPubHoliDayTran(int id)
        {
            try
            {
                HRPubHoliday objPuh = DB.HRPubHolidays.Find(id);
                objPuh.Description = Header.Description;
                objPuh.SecDescription = Header.SecDescription;
                objPuh.ChangedOn = DateTime.Now.Date;
                objPuh.ChangedBy = User.UserName;

                DB.HRPubHolidays.Attach(objPuh);

                DB.Entry(objPuh).State = System.Data.Entity.EntityState.Modified;
                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserID.ToString();
                log.ScreenId = Header.TranNo.ToString();
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string deletePubH(int id)
        {
            try
            {

                HRPubHoliday objCust = DB.HRPubHolidays.Find(id);
                if (objCust == null)
                {
                    return "CUST_NE";
                }

                HumicaDBContext DBM = new HumicaDBContext();
                DBM.HRPubHolidays.Attach(objCust);
                DBM.Entry(objCust).State = System.Data.Entity.EntityState.Deleted;
                int row = DBM.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserID.ToString();
                log.ScreenId = Header.TranNo.ToString();
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
                log.ScreenId = Header.TranNo.ToString();
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string uploadPubHoliday()
        {
            try
            {
                if (ListHeader.Count == 0)
                {
                    return "NO_DATA";
                }
                //  DocType = "OTR01";
                var date = DateTime.Now;
                var DBI = new HumicaDBContext();
                var ListPub = DB.HRPubHolidays.ToList();
                foreach (var holiday in ListHeader.ToList())
                {
                    //var checkEmp = DB.HRRequestOTs.Where(w => w.EmpCode == staffs.EmpCode).ToList();
                    //var checkDate = ListPub.Where(w => w.PDate.Date == holiday.PDate.Date).ToList();
                    var checkDate = ListPub.Where(w => w.PDate.Date.Year == holiday.PDate.Date.Year).ToList();

                    // var Staff = DB.HR_STAFF_VIEW.FirstOrDefault(w => w.EmpCode == staffs.EmpCode);
                    HRPubHoliday pubHoliday = new HRPubHoliday();
                    foreach (var item in checkDate)
                    {
                        ListPub.Remove(item);
                        DBI.HRPubHolidays.Attach(item);
                        DBI.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                        DBI.SaveChanges();

                    }
                    pubHoliday.PDate = holiday.PDate;
                    pubHoliday.Description = holiday.Description;
                    pubHoliday.SecDescription = holiday.SecDescription;
                    pubHoliday.CreatedBy = User.UserName;
                    pubHoliday.CreatedOn = DateTime.Now;

                    DBI.HRPubHolidays.Add(pubHoliday);
                }

                var row = DBI.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Convert.ToString(Header.TranNo);
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
                log.DocurmentAction = Convert.ToString(Header.TranNo);
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
                log.DocurmentAction = Convert.ToString(Header.TranNo);
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
    }
}