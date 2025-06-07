using Humica.Core.Process;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;

namespace Humica.Logic.PR
{
    public class PRRetropayment
    {
        public DBBusinessProcess DB = new DBBusinessProcess();
        public DBSystemHREntities DH = new DBSystemHREntities();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public PREmpRetroPayman Header { get; set; }
        public List<PREmpRetroPayman> ListHeader { get; set; }
        public string ScreenId { get; set; }
        public string MessageCode { get; set; }
        public string MessageError { get; set; }
        public List<MDUploadTemplate> ListTemplate { get; set; }
        public HR_STAFF_VIEW HeaderStaff { get; set; }

        public PRRetropayment()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }

        public string CreateRetro()
        {
            try
            {
                //Header = new PREmpRetroPayman();
                if (HeaderStaff.EmpCode == null)
                {
                    return "EMPCodeEmty";
                }
                if (Header.FromDate > Header.ToDate)
                {
                    return "REQ_DATE_RETRO_PAY";
                }
                if (Header.PayMonth < (Header.ToDate))
                {
                    return "PAYMONTH";
                }
                Header.EmpCode = HeaderStaff.EmpCode;
                Header.CreatedBy = User.UserName;
                Header.CreatedOn = DateTime.Now;
                DH.PREmpRetroPaymen.Add(Header);
                int row = DH.SaveChanges();

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Header.TrainNo.ToString();
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
                log.DocurmentAction = Header.TrainNo.ToString();
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
                log.DocurmentAction = Header.TrainNo.ToString();
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string EditRetro(int id)
        {
            try
            {
                var objMatch = DH.PREmpRetroPaymen.FirstOrDefault(w => w.TrainNo == id);
                if (objMatch == null)
                {
                    return "FAMILY_NE";
                }
                if (objMatch.FromDate > objMatch.ToDate)
                {
                    return "REQ_DATE_RETRO_PAY";
                }
                Header.EmpCode = objMatch.EmpCode;
                objMatch.FromDate = Header.FromDate;
                objMatch.ToDate = Header.ToDate.Value;
                objMatch.PayMonth = Header.PayMonth.Value;
                objMatch.Description = Header.Description;
                objMatch.ChangedBy = User.UserName;
                objMatch.ChangedOn = DateTime.Now;

                DH.PREmpRetroPaymen.Attach(objMatch);
                DH.Entry(objMatch).State = System.Data.Entity.EntityState.Modified;
                int row1 = DH.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Header.TrainNo.ToString();
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
                log.DocurmentAction = Header.TrainNo.ToString();
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
                log.DocurmentAction = Header.TrainNo.ToString();
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string Delete(int id)
        {
            try
            {
                Header = new PREmpRetroPayman();
                var objMatch = DH.PREmpRetroPaymen.FirstOrDefault(w => w.TrainNo == id);
                if (objMatch == null)
                {
                    return "FAMILY_NE";
                }
                Header.TrainNo = id;
                DH.PREmpRetroPaymen.Attach(objMatch);
                DH.Entry(objMatch).State = System.Data.Entity.EntityState.Deleted;
                int row = DH.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserID.ToString();
                log.ScreenId = Header.TrainNo.ToString();
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

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
                log.ScreenId = Header.TrainNo.ToString();
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        //public string Delete(int id)
        //{
        //    try
        //    {
        //        Header = new PREmpDeduc();
        //        var objMatch = DB.PREmpDeducs.FirstOrDefault(u => u.TranNo == id);

        //        if (objMatch == null)
        //        {
        //            return "DEDCUTION_EN";
        //        }
        //        Header.TranNo = id;
        //        DB.PREmpDeducs.Attach(objMatch);
        //        DB.Entry(objMatch).State = System.Data.Entity.EntityState.Deleted;
        //        int row = DB.SaveChanges();
        //        return SYConstant.OK;
        //    }
        //    catch (DbEntityValidationException e)
        //    {
        //        /*------------------SaveLog----------------------------------*/
        //        SYEventLog log = new SYEventLog();
        //        log.ScreenId = ScreenId;
        //        log.UserId = User.UserName;
        //        log.DocurmentAction = Header.TranNo.ToString();
        //        log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

        //        SYEventLogObject.saveEventLog(log, e);
        //        /*----------------------------------------------------------*/
        //        return "EE001";
        //    }
        //    catch (DbUpdateException e)
        //    {
        //        /*------------------SaveLog----------------------------------*/
        //        SYEventLog log = new SYEventLog();
        //        log.ScreenId = ScreenId;
        //        log.UserId = User.UserName;
        //        log.DocurmentAction = Header.TranNo.ToString();
        //        log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

        //        SYEventLogObject.saveEventLog(log, e, true);
        //        /*----------------------------------------------------------*/
        //        return "EE001";
        //    }
        //    catch (Exception e)
        //    {
        //        /*------------------SaveLog----------------------------------*/
        //        SYEventLog log = new SYEventLog();
        //        log.ScreenId = ScreenId;
        //        log.UserId = User.UserName;
        //        log.DocurmentAction = Header.TranNo.ToString();
        //        log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

        //        SYEventLogObject.saveEventLog(log, e, true);
        //        /*----------------------------------------------------------*/
        //        return "EE001";
        //    }
        //}
        //public string CreateLateEarly()
        //{
        //    try
        //    {
        //        if (HeaderStaff.EmpCode == null)
        //        {
        //            return "EMPCodeEmty";
        //        }
        //        if (ListHeaderLE.Count == 0)
        //        {
        //            return "LIST_INV";
        //        }
        //        var TranNo = DB.PREmpLateDeducts.OrderByDescending(u => u.TranNo).FirstOrDefault();
        //        // var RewardType = DH.PR_RewardsType.Where(w => w.ReCode == "DED").ToList();
        //        foreach (var read in ListHeaderLE)
        //        {
        //            var Result = new PREmpLateDeduct();
        //            Result.EmpCode = HeaderStaff.EmpCode;
        //            Result.DedCode = read.DedCode;
        //            Result.Day = read.Day;
        //            Result.Minute = read.Minute;
        //            Result.InMonth = read.InMonth;
        //            Result.FromDate = read.FromDate;
        //            Result.ToDate = read.ToDate;
        //            Result.Remark = read.Remark;
        //            Result.Status = 1;
        //            Result.LCK = 0;
        //            //if (TranNo != null)
        //            //{
        //            //    Result.TranNo = TranNo.TranNo + 1;
        //            //}
        //            //else
        //            //{
        //            //    Result.TranNo = 1;
        //            //}
        //            // HeaderLE.TranNo = Result.TranNo;
        //            DB.PREmpLateDeducts.Add(Result);
        //            HeaderLE.TranNo = Result.TranNo;
        //        }

        //        int row = DB.SaveChanges();

        //        return SYConstant.OK;
        //    }
        //    catch (DbEntityValidationException e)
        //    {
        //        /*------------------SaveLog----------------------------------*/
        //        SYEventLog log = new SYEventLog();
        //        log.ScreenId = ScreenId;
        //        log.UserId = User.UserName;
        //        log.DocurmentAction = HeaderLE.TranNo.ToString(); ;
        //        log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

        //        SYEventLogObject.saveEventLog(log, e);
        //        /*----------------------------------------------------------*/
        //        return "EE001";
        //    }
        //    catch (DbUpdateException e)
        //    {
        //        /*------------------SaveLog----------------------------------*/
        //        SYEventLog log = new SYEventLog();
        //        log.ScreenId = ScreenId;
        //        log.UserId = User.UserName;
        //        log.DocurmentAction = HeaderLE.TranNo.ToString();
        //        log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

        //        SYEventLogObject.saveEventLog(log, e, true);
        //        /*----------------------------------------------------------*/
        //        return "EE001";
        //    }
        //    catch (Exception e)
        //    {
        //        /*------------------SaveLog----------------------------------*/
        //        SYEventLog log = new SYEventLog();
        //        log.ScreenId = ScreenId;
        //        log.UserId = User.UserName;
        //        log.DocurmentAction = HeaderLE.TranNo.ToString();
        //        log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

        //        SYEventLogObject.saveEventLog(log, e, true);
        //        /*----------------------------------------------------------*/
        //        return "EE001";
        //    }
        //}
        //public string EditLateEarly(int id)
        //{
        //    try
        //    {
        //        HeaderLE = new PREmpLateDeduct();
        //        //var check = DB.PREmpDeducs.Where(u => u.TranNo == id && u.LCK == 1).ToList();
        //        //if (check.Count > 0)
        //        //{
        //        //    return "ALLLock";
        //        //}

        //        if (HeaderStaff.EmpCode == null)
        //        {
        //            return "EMPCodeEmty";
        //        }

        //        var List = DB.PREmpLateDeducts.Where(u => u.TranNo == id).ToList();

        //        foreach (var read in List)
        //        {
        //            DB.PREmpLateDeducts.Remove(read);
        //            DB.SaveChanges();
        //        }
        //        foreach (var read in ListHeaderLE)
        //        {
        //            var Result = new PREmpLateDeduct();
        //            Result.EmpCode = HeaderStaff.EmpCode;
        //            Result.DedCode = read.DedCode;
        //            Result.Day = read.Day;
        //            Result.Minute = read.Minute;
        //            Result.InMonth = read.InMonth;
        //            Result.FromDate = read.FromDate;
        //            Result.ToDate = read.ToDate;
        //            Result.Remark = read.Remark;
        //            Result.Status = 1;
        //            Result.LCK = 0;
        //            DB.PREmpLateDeducts.Add(Result);
        //            HeaderLE.TranNo = Result.TranNo;
        //        }

        //        int row = DB.SaveChanges();

        //        return SYConstant.OK;
        //    }
        //    catch (DbEntityValidationException e)
        //    {
        //        /*------------------SaveLog----------------------------------*/
        //        SYEventLog log = new SYEventLog();
        //        log.ScreenId = ScreenId;
        //        log.UserId = User.UserName;
        //        log.DocurmentAction = HeaderLE.TranNo.ToString();
        //        log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

        //        SYEventLogObject.saveEventLog(log, e);
        //        /*----------------------------------------------------------*/
        //        return "EE001";
        //    }
        //    catch (DbUpdateException e)
        //    {
        //        /*------------------SaveLog----------------------------------*/
        //        SYEventLog log = new SYEventLog();
        //        log.ScreenId = ScreenId;
        //        log.UserId = User.UserName;
        //        log.DocurmentAction = HeaderLE.TranNo.ToString();
        //        log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

        //        SYEventLogObject.saveEventLog(log, e, true);
        //        /*----------------------------------------------------------*/
        //        return "EE001";
        //    }
        //    catch (Exception e)
        //    {
        //        /*------------------SaveLog----------------------------------*/
        //        SYEventLog log = new SYEventLog();
        //        log.ScreenId = ScreenId;
        //        log.UserId = User.UserName;
        //        log.DocurmentAction = HeaderLE.TranNo.ToString();
        //        log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

        //        SYEventLogObject.saveEventLog(log, e, true);
        //        /*----------------------------------------------------------*/
        //        return "EE001";
        //    }
        //}
        //public string DeleteLateEarly(int id)
        //{
        //    try
        //    {
        //        HeaderLE = new PREmpLateDeduct();
        //        var objMatch = DB.PREmpLateDeducts.FirstOrDefault(u => u.TranNo == id);

        //        if (objMatch == null)
        //        {
        //            return "DEDCUTION_EN";
        //        }
        //        HeaderLE.TranNo = id;
        //        DB.PREmpLateDeducts.Attach(objMatch);
        //        DB.Entry(objMatch).State = System.Data.Entity.EntityState.Deleted;
        //        int row = DB.SaveChanges();
        //        return SYConstant.OK;
        //    }
        //    catch (DbEntityValidationException e)
        //    {
        //        /*------------------SaveLog----------------------------------*/
        //        SYEventLog log = new SYEventLog();
        //        log.ScreenId = ScreenId;
        //        log.UserId = User.UserName;
        //        log.DocurmentAction = HeaderLE.TranNo.ToString();
        //        log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

        //        SYEventLogObject.saveEventLog(log, e);
        //        /*----------------------------------------------------------*/
        //        return "EE001";
        //    }
        //    catch (DbUpdateException e)
        //    {
        //        /*------------------SaveLog----------------------------------*/
        //        SYEventLog log = new SYEventLog();
        //        log.ScreenId = ScreenId;
        //        log.UserId = User.UserName;
        //        log.DocurmentAction = HeaderLE.TranNo.ToString();
        //        log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

        //        SYEventLogObject.saveEventLog(log, e, true);
        //        /*----------------------------------------------------------*/
        //        return "EE001";
        //    }
        //    catch (Exception e)
        //    {
        //        /*------------------SaveLog----------------------------------*/
        //        SYEventLog log = new SYEventLog();
        //        log.ScreenId = ScreenId;
        //        log.UserId = User.UserName;
        //        log.DocurmentAction = HeaderLE.TranNo.ToString();
        //        log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

        //        SYEventLogObject.saveEventLog(log, e, true);
        //        /*----------------------------------------------------------*/
        //        return "EE001";
        //    }
        //}

        //public int IsExistEmployeeDeduction(string EmpCode, string DedCode, DateTime FromDate, DateTime ToDate)
        //{
        //    int Result = -1;
        //    var _list = DB.PREmpDeducs.Where(w => w.EmpCode == EmpCode).ToList();
        //    _list = _list.Where(w => w.DedCode == DedCode &&
        //        ((FromDate.Date >= w.FromDate.Value.Date && FromDate.Date <= w.ToDate.Value.Date) || (ToDate.Date >= w.FromDate.Value.Date && ToDate.Date <= w.ToDate.Value.Date))).ToList();
        //    if (_list.Count > 0)
        //    {
        //        foreach (var item in _list)
        //        {
        //            Result = Convert.ToInt32(item.Status);
        //        }
        //    }
        //    return Result;
        //}
        //public int IsExistLateEarlyDeduction(string EmpCode, string DedCode, DateTime FromDate, DateTime ToDate)
        //{
        //    int Result = -1;
        //    var _list = DB.PREmpLateDeducts.Where(w => w.EmpCode == EmpCode).ToList();
        //    _list = _list.Where(w => w.DedCode == DedCode &&
        //        ((FromDate.Date >= w.FromDate.Value.Date && FromDate.Date <= w.ToDate.Value.Date) || (ToDate.Date >= w.FromDate.Value.Date && ToDate.Date <= w.ToDate.Value.Date))).ToList();
        //    if (_list.Count > 0)
        //    {
        //        foreach (var item in _list)
        //        {
        //            Result = Convert.ToInt32(item.Status);
        //        }
        //    }
        //    return Result;
        //}
    }
}


