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
    public class LeaveTypeObject
    {
        public HumicaDBContext DB = new HumicaDBContext();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public string ScreenId { get; set; }
        public string MessageError { get; set; }
        public HRLeaveType Header { get; set; }
        public List<HRLeaveType> ListHeader { get; set; }
        public List<HRLeaveProRate> ListLeaveProRate { get; set; }
        public LeaveTypeObject()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }
        public string CreateLeaveType()
        {
            try
            {
                if (Header.Code == null)
                {
                    return "CODE_EN";
                }
                var Count = DB.HRLeaveTypes.ToList();
                if (Count.Where(w => w.Code == Header.Code).ToList().Count() > 0)
                {
                    return "DUP_CODE_EN";
                }
                Header.Code = Header.Code.ToUpper().Trim();
                Header.CreatedOn = DateTime.Now.Date;
                Header.CreatedBy = User.UserName;
                Header.CUTTYPE = 1;
                if (Header.IsParent == true && Header.CUT != true)
                    Header.Amount = 0.5M;
                else Header.Amount = 0;
                DB.HRLeaveTypes.Add(Header);

                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Header.Code;
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
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string editLeaveType(string id)
        {
            try
            {
                HRLeaveType objMast = DB.HRLeaveTypes.Find(id);
                //objMast.Description = Header.Description;
                //objMast.OthDesc = Header.OthDesc;
                //objMast.Amount = Header.Amount;
                //objMast.IsParent = Header.IsParent;
                //objMast.Parent = Header.Parent;
                //objMast.Remark = Header.Remark;
                //objMast.Operator = Header.Operator;
                //objMast.Foperand = Header.Foperand;
                //objMast.Soperand = Header.Soperand;
                //objMast.CUT = Header.CUT;
                //objMast.Probation = Header.Probation;
                //objMast.CUTTYPE = 1;
                //objMast.IncPub = Header.IncPub;
                //objMast.InRest = Header.InRest;
                //objMast.SVC = Header.SVC;
                //objMast.IsCurrent = Header.IsCurrent;
                //objMast.BeforeDay = Header.BeforeDay;
                Header.CreatedBy = objMast.CreatedBy;
                Header.CreatedOn = objMast.CreatedOn;
                Header.ChangedOn = DateTime.Now;
                Header.ChangedBy = User.UserName;
                //objMast.ReqDocument = Header.ReqDocument;
                //objMast.NumDay = Header.NumDay;
                //objMast.Allowbackward = Header.Allowbackward;
                //objMast.Beforebackward = Header.Beforebackward;
                //objMast.Gender = Header.Gender;
                if (Header.IsParent == true && Header.CUT != true)
                    Header.Amount = 0.5M;
                else Header.Amount = 0;

                //DB.HRLeaveTypes.Attach(Header);
                HumicaDBContext DBM = new HumicaDBContext();
                DBM.Entry(Header).State = System.Data.Entity.EntityState.Modified;
                int row = DBM.SaveChanges();

                //ClsAuditLog.AuditLog(ScreenId, objMast.Code,"Header", objMast, Header);
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserID.ToString();
                log.ScreenId = Header.Code;
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string deleteLeaveType(string id)
        {
            try
            {

                HRLeaveType objCust = DB.HRLeaveTypes.Find(id);
                if (objCust == null)
                {
                    return "LEAVE_NE";
                }
                var objEmp = DB.HREmpLeaveBs.ToList();
                if (objEmp.Where(w => w.LeaveCode == id).Any())
                {
                    return "DATA_USE";
                }
                HumicaDBContext DBM = new HumicaDBContext();
                DBM.HRLeaveTypes.Attach(objCust);
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
                log.ScreenId = Header.Code;
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
                log.ScreenId = Header.Code;
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public static decimal GetUnitLeaveDeductionAmoun(HRLeaveType leaveType, decimal? salary, decimal numDayInMonth, decimal workingHour)
        {
            if (!salary.HasValue)
            {
                return 0m;
            }

            decimal result = 0;
            if (leaveType.Foperand == "B")
            {
                if (leaveType.Operator == "/")
                {
                    result = Convert.ToDecimal(salary / leaveType.Soperand);
                }
                else if (leaveType.Operator == "-")
                {
                    result = Convert.ToDecimal(salary - leaveType.Soperand);
                }
                else if (leaveType.Operator == "+")
                {
                    result = Convert.ToDecimal(salary + leaveType.Soperand);
                }
                else if (leaveType.Operator == "*")
                {
                    result = Convert.ToDecimal(salary * leaveType.Soperand);
                }
            }
            else if (leaveType.Foperand == "B/W")
            {
                if (leaveType.Operator == "/")
                {
                    result = Convert.ToDecimal(salary / (decimal?)numDayInMonth / leaveType.Soperand);
                }
                else if (leaveType.Operator == "-")
                {
                    result = Convert.ToDecimal(salary / (decimal?)numDayInMonth - leaveType.Soperand);
                }
                else if (leaveType.Operator == "+")
                {
                    result = Convert.ToDecimal(salary / (decimal?)numDayInMonth + leaveType.Soperand);
                }
                else if (leaveType.Operator == "*")
                {
                    result = Convert.ToDecimal(salary / (decimal?)numDayInMonth * leaveType.Soperand);
                }
            }
            else if (leaveType.Foperand == "B/D*H")
            {
                if (leaveType.Operator == "/")
                {
                    result = Convert.ToDecimal(salary / (decimal?)(numDayInMonth * workingHour) / leaveType.Soperand);
                }
                else if (leaveType.Operator == "-")
                {
                    result = Convert.ToDecimal(salary / (decimal?)(numDayInMonth - workingHour) / leaveType.Soperand);
                }
                else if (leaveType.Operator == "+")
                {
                    result = Convert.ToDecimal(salary / (decimal?)(numDayInMonth + workingHour) / leaveType.Soperand);
                }
                else if (leaveType.Operator == "*")
                {
                    result = Convert.ToDecimal(salary / (decimal?)(numDayInMonth + workingHour) / leaveType.Soperand);
                }
            }

            return result;
        }

    }
}