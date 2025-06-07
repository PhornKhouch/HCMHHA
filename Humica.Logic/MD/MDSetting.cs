using Humica.Core.DB;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using Humica.Core;
namespace Humica.Logic.MD
{
    public class MDSetting
    {
        private HumicaDBContext DB = new HumicaDBContext();

        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public string ScreenId { get; set; }
        public string ActionName { get; set; }
        public static string PARAM_BRANCH = "PARAM_BRANCH";
        public string hide { get; set; }
        public bool isvisible { get; set; }
        public SYHRSetting Header { get; set; }
        public HR_STAFF_VIEW staff { get; set; }
        public List<SYHRSetting> ListHRSetting { get; set; }
        public List<PRPensionFundSetting> ListPensionFundSetting { get; set; }
        public MDSetting()
        {
            this.User = SYSession.getSessionUser();
            this.BS = SYSession.getSessionUserBS();
        }
        public List<HREmpType> EmpTypes()
        {
            var t = DB.HREmpTypes;
            return t.ToList();
        }
        public string UpdateSetting()
        {
            try
            {
                DB = new HumicaDBContext();
                var ObjMatch = DB.SYHRSettings.First();
                if (Header == null)
                {
                    return "DOC_INV";
                }
                ObjMatch.NSSFSalaryType = Header.NSSFSalaryType;
                ObjMatch.MinContribue = Header.MinContribue;
                ObjMatch.MaxContribute = Header.MaxContribute;
                ObjMatch.SeniorityException = Header.SeniorityException;
                ObjMatch.IsTax = Header.IsTax;
                ObjMatch.EmpType = Header.EmpType;
                ObjMatch.SeniorityType = Header.SeniorityType;
                ObjMatch.ComRisk = Header.ComRisk;
                ObjMatch.StaffRisk = Header.StaffRisk;
                ObjMatch.ComHealthCare = Header.ComHealthCare;
                ObjMatch.StaffHealthCare = Header.StaffHealthCare;
                ObjMatch.Child = Header.Child;
                ObjMatch.Spouse = Header.Spouse;
                DB.SYHRSettings.Attach(ObjMatch);
                DB.Entry(ObjMatch).Property(x => x.NSSFSalaryType).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.MinContribue).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.MaxContribute).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.SeniorityException).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.IsTax).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.EmpType).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.SeniorityType).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ComRisk).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.StaffRisk).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ComHealthCare).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.StaffHealthCare).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Child).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Spouse).IsModified = true;
                DB.SaveChanges();

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Header.ID.ToString();
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
                log.DocurmentAction = Header.ID.ToString();
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string UpdateSettingS()
        {
            try
            {
                DB = new HumicaDBContext();
                var ObjMatch = DB.SYHRSettings.First();
                if (Header == null)
                {
                    return "DOC_INV";
                }
                ObjMatch.TelegOT = Header.TelegOT;
                ObjMatch.TelegLeave = Header.TelegLeave;
                ObjMatch.DeductEalary = Header.DeductEalary;
                ObjMatch.DeductLate = Header.DeductLate;
                ObjMatch.MisScanUP = Header.MisScanUP;
                ObjMatch.MisScanAL = Header.MisScanAL;
                ObjMatch.CountMisscan = Header.CountMisscan;
                DB.SYHRSettings.Attach(ObjMatch);
                DB.Entry(ObjMatch).Property(x => x.TelegOT).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.TelegLeave).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.DeductEalary).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.DeductLate).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.MisScanAL).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.MisScanUP).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.CountMisscan).IsModified = true;
                DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Header.ID.ToString();
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
                log.DocurmentAction = Header.ID.ToString();
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
    }
}