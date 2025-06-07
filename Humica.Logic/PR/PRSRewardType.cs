using Humica.Core;
using Humica.Core.DB;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;

namespace Humica.Logic.PR
{
    public class PRSRewardType
    {
        public HumicaDBContext DB = new HumicaDBContext();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public PR_RewardsType Header { get; set; }
        public string ScreenId { get; set; }
        public string CompanyCode { get; set; }
        public string MessageCode { get; set; }
        public bool IsInUse { get; set; }
        public bool IsEditable { get; set; }
        public List<PR_RewardsType> ListHeader { get; set; }
        public RawardType HeaderType { get; set; }
        public PRSRewardType()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }
        public string CreateRawardType()
        {
            try
            {
                if (Header.Code == null)
                {
                    return "CODE_EN";
                }
                Header.Code = Header.Code.ToUpper().Trim();
                var Count = DB.PR_RewardsType.ToList();
                HeaderType = new RawardType();
                if (Count.Where(w => w.Code == Header.Code && w.ReCode == Header.ReCode).ToList().Count() > 0)
                {
                    return "INVALID_CODE";
                }
                var listType = HeaderType.ListRawardType().ToList();
                Header.RewardType = listType.Where(w => w.Code == Header.ReCode).FirstOrDefault().Description;
                Header.CreateOn = DateTime.Now;
                Header.CreateBy = User.UserName;

                DB.PR_RewardsType.Add(Header);
                int row = DB.SaveChanges();
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
        public string EditRawardType(string Code)
        {
            try
            {
                var ObjMatch = DB.PR_RewardsType.FirstOrDefault(w => w.ReCode == Header.ReCode && w.Code == Code);

                if (ObjMatch == null)
                {
                    return "RawardType_NE";
                }
                //var ObjMatch = objCust.FirstOrDefault();
                //HeaderType = new RawardType();
                //var listType = HeaderType.ListRawardType().ToList();
                ObjMatch.Description = Header.Description;
                //  ObjMatch.RewardType = //listType.Where(w => w.Code == Header.ReCode).FirstOrDefault().Description;
                ObjMatch.Tax = Header.Tax;
                ObjMatch.FTax = Header.FTax;
                ObjMatch.Amount = Header.Amount;
                ObjMatch.IsBIMonthly = Header.IsBIMonthly;
                ObjMatch.BIPercentageAm = Header.BIPercentageAm;
                ObjMatch.ChangedOn = DateTime.Now;
                ObjMatch.ChangedBy = User.UserName;

                HumicaDBContext DBM = new HumicaDBContext();
                DBM.PR_RewardsType.Attach(ObjMatch);
                DBM.Entry(ObjMatch).State = System.Data.Entity.EntityState.Modified;
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
        }
        public string DeleteRawardType(string Code)
        {
            try
            {
                Header = new PR_RewardsType();
                Header.Code = Code;
                var objCust = DB.PR_RewardsType.FirstOrDefault(w => w.Code == Code);
                if (objCust == null)
                {
                    return "RawardType_NE";
                }

                HumicaDBContext DBM = new HumicaDBContext();
                DBM.PR_RewardsType.Attach(objCust);
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
        public List<PR_RewardsType> GetRewardsType(List<PR_RewardsType> LstRewardsType, string ReCode, string Payterm = "")
        {
            List<PR_RewardsType> items = new List<PR_RewardsType>(LstRewardsType);
            if (Payterm == TermCalculation.FIRST.ToString())
                items = items.Where(w => w.ReCode == ReCode && (!w.IsBIMonthly.IsNullOrZero() && w.BIPercentageAm > 0)).ToList();

            return items;
        }

    }
    public enum TermCalculation
    {
        FIRST,
        SECOND
    }
    public enum RewardTypeCode
    {
        ALLW,
        BON,
        DED
    }
    public class RawardType
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public List<RawardType> ListRawardType()
        {
            List<RawardType> _list = new List<RawardType>();
            _list.Add(new RawardType { Code = "ALLW", Description = "Allowance" });
            _list.Add(new RawardType { Code = "DED", Description = "Deduction" });
            _list.Add(new RawardType { Code = "BON", Description = "Bonus" });
            return _list;
        }

    }
}