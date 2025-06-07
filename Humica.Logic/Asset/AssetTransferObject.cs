using Humica.Core.DB;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;

namespace Humica.Logic.Asset
{
    public class AssetTransferObject
    {
        public HumicaDBContext DB = new HumicaDBContext();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public string ScreenId { get; set; }
        public string MessageError { get; set; }
        public string DocType { get; set; }
        public HRAssetTransfer Header { get; set; }
        public PREmpDeduc HeaderDed { get; set; }
        public List<HRAssetTransfer> ListHeader { get; set; }
        public List<PREmpDeduc> ListDed { get; set; }
        public List<HRAssetStaff> ListStaffAsset { get; set; }
        public List<HRAssetRegister> ListAsset { get; set; }
        public HR_STAFF_VIEW HeaderStaff { get; set; }
        public List<MDUploadTemplate> ListTemplate { get; set; }
        public string Code { get; set; }
        public string MessageCode { get; set; }

        public AssetTransferObject()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }

        public string Create()
        {
            try
            {
                Header.EmpCode = Header.EmpCode;
                Header.EmployeName = Header.EmployeName;
                Header.ReturnDate = Header.ReturnDate;
                Header.IsDedSalary = Header.IsDedSalary;
                Header.AssignDate = Header.AssignDate;
                Header.Condition = Header.Condition;
                Header.AssignDate = Header.AssignDate;
                Header.AssetCode = Header.AssetCode;
                Header.Remark = Header.Remark;
                Header.AssetDescription = Header.AssetDescription;
                Header.Status = SYDocumentStatus.RETURN.ToString();
                var checkdup = DB.HRAssetRegisters.Where(w => w.AssetCode == Header.AssetCode).ToList();
                foreach (var read in checkdup.ToList())
                {
                    if (Header.Condition == "DAMAGE")
                    {
                        read.StatusUse = "DAMAGE";
                    }
                    else
                    {
                        read.StatusUse = SYDocumentStatus.OPEN.ToString(); 
                    }
                    DB.HRAssetRegisters.Attach(read);
                    DB.Entry(read).Property(x => x.StatusUse).IsModified = true;
                }

                var checkdAssetstaff = DB.HRAssetStaffs.Where(w => w.AssetCode == Header.AssetCode && w.EmpCode== Header.EmpCode).ToList();
                foreach (var read in checkdAssetstaff.ToList())
                {
                    read.Status = SYDocumentStatus.RETURN.ToString();
                    read.Condition = Header.Condition;
                    read.ReturnDate = Header.ReturnDate;
                    DB.HRAssetStaffs.Attach(read);
                    DB.Entry(read).Property(x => x.Status).IsModified = true;
                    DB.Entry(read).Property(x => x.Condition).IsModified = true;
                    DB.Entry(read).Property(x => x.ReturnDate).IsModified = true;
                }
                Header.CreatedBy = User.UserName;
                Header.CreatedOn = DateTime.Now;
                DB.HRAssetTransfers.Add(Header);

                if (Header.IsDedSalary == true)
                {
                    var RewardType = DB.PR_RewardsType.Where(w => w.ReCode == "Ded").ToList();
                    var HeaderID = DB.HRAssetTransfers.OrderByDescending(u => u.ID).FirstOrDefault();
                    if (ListDed != null)
                    {
                        foreach (var row in ListDed)
                        {
                            var TranNo = DB.PREmpDeducs.OrderByDescending(u => u.TranNo).FirstOrDefault();
                            var ded = new PREmpDeduc();
                            if (TranNo != null)
                            {
                                ded.TranNo = TranNo.TranNo + 1;
                            }
                            else
                            {
                                ded.TranNo = 1;
                            }
                            ded.EmpCode = Header.EmpCode;
                            ded.EmpName = Header.EmployeName;
                            ded.DedCode = Header.DedType;
                            var DedDes = RewardType.Where(w => w.Code == Header.DedType).FirstOrDefault();
                            ded.DedDescription = DedDes.Description;
                            ded.FromDate = row.FromDate;
                            ded.ToDate = row.ToDate;
                            ded.Amount = row.Amount;
                            ded.Status = 1;
                            if (HeaderID != null)
                            {
                                ded.AssetTransferID = HeaderID.ID + 1;
                            }
                            else
                            {
                                ded.AssetTransferID = 1;
                            }
                            ded.StatusAssetDed = SYDocumentStatus.OPEN.ToString();
                            ded.CreateBy = User.UserName;
                            ded.CreateOn = DateTime.Now;
                            DB.PREmpDeducs.Add(ded);
                            DB.SaveChanges();
                        }
                    }
                }
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
                log.DocurmentAction = Header.ID.ToString();
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
                log.DocurmentAction = Header.ID.ToString();
                log.Action = SYActionBehavior.ADD.ToString();
                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string Update(string id)
        {
            try
            {
                DB = new HumicaDBContext();
                int ID = Convert.ToInt32(id);
                var ObjMatch = DB.HRAssetTransfers.FirstOrDefault(w => w.ID == ID);

                if (ObjMatch == null)
                {
                    return "DOC_INV";
                }

                ObjMatch.ChangedBy = User.UserName;
                ObjMatch.ChangedOn = DateTime.Now;
                ObjMatch.ReturnDate = Header.ReturnDate;
                ObjMatch.Remark = Header.Remark;
                ObjMatch.Condition = Header.Condition;
                ObjMatch.Attachment = Header.Attachment;
                ObjMatch.IsDedSalary = Header.IsDedSalary;
                ObjMatch.DedType = Header.DedType;
                ObjMatch.FromDate = Header.FromDate;
                ObjMatch.ToDate = Header.ToDate;
                ObjMatch.Period = Header.Period;
                ObjMatch.DedAmount = Header.DedAmount;
                ObjMatch.Amount = Header.Amount;
                var checkdAssetstaff = DB.HRAssetStaffs.Where(w => w.AssetCode == Header.AssetCode && w.EmpCode == Header.EmpCode).ToList();
                var RewardType = DB.PR_RewardsType.Where(w => w.ReCode == "Ded").ToList();
                foreach (var read in checkdAssetstaff.ToList())
                {
                    
                    read.Status = SYDocumentStatus.RETURN.ToString();
                    read.Condition = Header.Condition;
                    read.ReturnDate = Header.ReturnDate;
                    DB.HRAssetStaffs.Attach(read);
                    DB.Entry(read).Property(x => x.Status).IsModified = true;
                    DB.Entry(read).Property(x => x.Condition).IsModified = true;
                    DB.Entry(read).Property(x => x.ReturnDate).IsModified = true;
                }

                var checkdup = DB.HRAssetRegisters.Where(w => w.AssetCode == Header.AssetCode).ToList();
                foreach (var read in checkdup.ToList())
                {
                    if (Header.Condition == "DAMAGE" || Header.Condition=="LOST")
                    {
                        read.Condition = Header.Condition;
                    }
                    else
                    {
                        read.Condition = "G";
                        read.StatusUse = SYDocumentStatus.OPEN.ToString();
                    }
                    DB.HRAssetRegisters.Attach(read);
                    DB.Entry(read).Property(x => x.StatusUse).IsModified = true;
                    DB.Entry(read).Property(x => x.Condition).IsModified = true;
                }
                DB.HRAssetTransfers.Attach(ObjMatch);

                if (Header.IsDedSalary == true)
                {
                    if (ListDed != null)
                    {
                        foreach (var row in ListDed)
                        {
                            var ded = DB.PREmpDeducs.Where(u => u.TranNo == row.TranNo).FirstOrDefault();
                            if (ded != null)
                            {
                                ded.Amount = row.Amount;
                                ded.Remark = row.Remark;
                                ded.ChangedBy = User.UserName;
                                ded.ChangedOn = DateTime.Now;
                                DB.PREmpDeducs.Attach(ded);
                                DB.Entry(ded).Property(x => x.Amount).IsModified = true;
                                DB.Entry(ded).Property(x => x.Remark).IsModified = true;
                                DB.Entry(ded).Property(x => x.ChangedBy).IsModified = true;
                                DB.Entry(ded).Property(x => x.ChangedOn).IsModified = true;
                            }
                            else
                            {
                                var TranNo = DB.PREmpDeducs.OrderByDescending(u => u.TranNo).FirstOrDefault();
                                var _ded = new PREmpDeduc();
                                if (TranNo != null)
                                {
                                    _ded.TranNo = TranNo.TranNo + 1;
                                }
                                else
                                {
                                    _ded.TranNo = 1;
                                }
                                _ded.EmpCode = Header.EmpCode;
                                _ded.EmpName = Header.EmployeName;
                                _ded.DedCode = Header.DedType;
                                var DedDes = RewardType.Where(w => w.Code == Header.DedType).First();
                                _ded.DedDescription = DedDes.Description;
                                _ded.FromDate = row.FromDate;
                                _ded.ToDate = row.ToDate;
                                _ded.Amount = row.Amount;
                                _ded.Status = 1;
                                _ded.AssetTransferID = Header.ID;
                                _ded.StatusAssetDed = SYDocumentStatus.OPEN.ToString();
                                _ded.CreateBy = User.UserName;
                                _ded.CreateOn = DateTime.Now;
                                DB.PREmpDeducs.Add(ded);
                            }

                        }
                    }
                }

                DB.Entry(ObjMatch).Property(x => x.ChangedBy).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ChangedOn).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ReturnDate).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Remark).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Attachment).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.IsDedSalary).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Condition).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.FromDate).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ToDate).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.DedType).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Period).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Amount).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.DedAmount).IsModified = true;

                DB.SaveChanges();

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = id;
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
                log.DocurmentAction = id;
                log.Action = SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string Delete(string id)
        {
            try
            {
                DB = new HumicaDBContext();

                int ID = Convert.ToInt32(id);
                var ObjMatch = DB.HRAssetTransfers.FirstOrDefault(w => w.ID == ID);
                string RETURN = SYDocumentStatus.RETURN.ToString();
                if (ObjMatch == null)
                {
                    return "DOC_INV";
                }
                if (ObjMatch.Status == RETURN)
                {
                    var checkdup = DB.HRAssetRegisters.Where(w => w.AssetCode == ObjMatch.AssetCode).ToList();
                    foreach (var read in checkdup.ToList())
                    {
                        read.StatusUse = SYDocumentStatus.OPEN.ToString();
                        DB.HRAssetRegisters.Attach(read);
                        DB.Entry(read).Property(x => x.StatusUse).IsModified = true;
                    }
                    var AssetStaff = DB.HRAssetStaffs.Where(w => w.AssetCode == ObjMatch.AssetCode && w.EmpCode==ObjMatch.EmpCode).ToList();
                    foreach (var read in AssetStaff.ToList())
                    {
                        read.Status = SYDocumentStatus.ASSIGN.ToString();
                        DB.HRAssetStaffs.Attach(read);
                        DB.Entry(read).Property(x => x.Status).IsModified = true;
                    }
                    int _ID = Convert.ToInt32(id);
                    var ItemDed = DB.PREmpDeducs.Where(w => w.AssetTransferID == _ID).ToList();
                    if (ItemDed != null)
                    {
                        foreach(var obj in ItemDed)
                        {
                            DB.PREmpDeducs.Attach(obj);
                            DB.Entry(obj).State = System.Data.Entity.EntityState.Deleted;
                        }
                    }

                    DB.HRAssetTransfers.Remove(ObjMatch);
                    DB.SaveChanges();
                }

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = id;
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
                log.DocurmentAction = id;
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
                log.DocurmentAction = id;
                log.Action = SYActionBehavior.DELETE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        //public string Transfer(string id)
        //{
        //    try
        //    {
        //        HumicaDBContext DBI = new HumicaDBContext();
        //        int ID = Convert.ToInt32(id);
        //        HRAssetStaff objmatch = DB.HRAssetStaffs.First(w => w.ID == ID);
        //        string assign = SYDocumentStatus.ASSIGN.ToString();

        //        if (objmatch == null || objmatch.Status != assign)
        //        {
        //            return "DOC_INV";
        //        }
        //        else
        //        {
        //            var checkdup = DB.HRAssetRegisters.Where(w => w.AssetCode == objmatch.AssetCode).ToList();
        //            foreach (var read in checkdup.ToList())
        //            {
        //                read.StatusUse = SYDocumentStatus.OPEN.ToString();
        //                DB.HRAssetRegisters.Attach(read);
        //                DB.Entry(read).Property(x => x.StatusUse).IsModified = true;
        //            }
        //            objmatch.ReturnDate = Header.ReturnDate;
        //            objmatch.Status = SYDocumentStatus.RETURN.ToString();
        //            objmatch.ChangedBy = User.UserName;
        //            objmatch.ChangedOn = DateTime.Now;
        //            DB.HRAssetStaffs.Attach(objmatch);
        //            DB.Entry(objmatch).Property(w => w.ReturnDate).IsModified = true;
        //            DB.Entry(objmatch).Property(w => w.Status).IsModified = true;
        //            DB.Entry(objmatch).Property(w => w.ChangedBy).IsModified = true;
        //            DB.Entry(objmatch).Property(w => w.ChangedOn).IsModified = true;
        //        }

        //        DB.SaveChanges();

        //        return SYConstant.OK;

        //    }
        //    catch (DbEntityValidationException e)
        //    {
        //        /*------------------SaveLog----------------------------------*/
        //        SYEventLog log = new SYEventLog();
        //        log.ScreenId = ScreenId;
        //        log.UserId = User.UserName;
        //        log.DocurmentAction = id;
        //        log.Action = SYActionBehavior.RELEASE.ToString();

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
        //        log.DocurmentAction = id;
        //        log.Action = SYActionBehavior.RELEASE.ToString();

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
        //        log.DocurmentAction = id;
        //        log.Action = SYActionBehavior.RELEASE.ToString();

        //        SYEventLogObject.saveEventLog(log, e, true);
        //        /*----------------------------------------------------------*/
        //        return "EE001";
        //    }
        //}
        //public string Import()
        //{
        //    try
        //    {
        //        if (ListHeader.Count == 0)
        //        {
        //            return "NO_DATA";
        //        }
        //        try
        //        {
        //            DB.Configuration.AutoDetectChangesEnabled = false;
        //            var _list = new List<HREmpFamily>();
        //            List<HRStaffProfile> _listStaff = new List<HRStaffProfile>();
        //            _list = DB.HREmpFamilies.ToList();
        //            _listStaff = DB.HRStaffProfiles.ToList();

        //            var date = DateTime.Now;
        //            var ListAsscode = DB.HRAssetRegisters.Where(w => w.StatusUse == "OPEN").ToList();
        //            foreach (var staffs in ListHeader.ToList())
        //            {
        //                Header = new HRAssetStaff();
        //                var EmpCode = _listStaff.Where(w => w.EmpCode == staffs.EmpCode).ToList();
        //                Header.EmpCode = "";
        //                if (EmpCode.Count <= 1)
        //                {
        //                    if (EmpCode.Count == 1)
        //                    {
        //                        Header.EmpCode = EmpCode.FirstOrDefault().EmpCode;
        //                    }
        //                    Header.EmpCode = staffs.EmpCode;
        //                    Header.EmployeName = staffs.EmployeName;
        //                    Header.AssetCode = staffs.AssetCode;
        //                    Header.AssetDescription = staffs.AssetDescription;
        //                    Header.AssignDate = staffs.AssignDate;
        //                    Header.ReturnDate = staffs.ReturnDate;
        //                    Header.Status = staffs.Status;
        //                    Header.Remark = staffs.Remark;
        //                    Header.CreatedBy = User.UserName;
        //                    Header.CreatedOn = DateTime.Now;
        //                    var Asscode = ListAsscode.Where(w => w.AssetCode == Header.AssetCode).FirstOrDefault();
        //                    if (Asscode != null)
        //                    {
        //                        Asscode.StatusUse = "ASSIGN";
        //                        DB.HRAssetRegisters.Attach(Asscode);
        //                        DB.Entry(Asscode).Property(x => x.StatusUse).IsModified = true;
        //                    }
        //                    DB.HRAssetStaffs.Add(Header);
        //                }
        //                else
        //                {
        //                    foreach (var item1 in EmpCode)
        //                    {
        //                        if (item1 != null)
        //                        {
        //                            Header.EmpCode = item1.EmpCode;
        //                        }
        //                        Header.EmpCode = staffs.EmpCode;
        //                        Header.EmployeName = staffs.EmployeName;
        //                        Header.AssetCode = staffs.AssetCode;
        //                        Header.AssetDescription = staffs.AssetDescription;
        //                        Header.AssignDate = staffs.AssignDate;
        //                        Header.ReturnDate = staffs.ReturnDate;
        //                        Header.Status = staffs.Status;
        //                        Header.Remark = staffs.Remark;
        //                        Header.CreatedBy = User.UserName;
        //                        Header.CreatedOn = DateTime.Now;
        //                        var Asscode = ListAsscode.Where(w => w.AssetCode == Header.AssetCode).FirstOrDefault();
        //                        if (Asscode != null)
        //                        {
        //                            Asscode.StatusUse = "ASSIGN";
        //                            DB.HRAssetRegisters.Attach(Asscode);
        //                            DB.Entry(Asscode).Property(x => x.StatusUse).IsModified = true;
        //                        }
        //                        DB.HRAssetStaffs.Add(Header);
        //                    }
        //                }
        //            }

        //            DB.SaveChanges();
        //            return SYConstant.OK;
        //        }
        //        finally
        //        {
        //            DB.Configuration.AutoDetectChangesEnabled = true;
        //        }
        //    }
        //    catch (DbEntityValidationException e)
        //    {
        //        /*------------------SaveLog----------------------------------*/
        //        SYEventLog log = new SYEventLog();
        //        log.ScreenId = ScreenId;
        //        log.UserId = User.UserName;
        //        log.DocurmentAction = Header.EmpCode;
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
        //        log.DocurmentAction = Header.EmpCode;
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
        //        log.DocurmentAction = Header.EmpCode;
        //        log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

        //        SYEventLogObject.saveEventLog(log, e, true);
        //        /*----------------------------------------------------------*/
        //        return "EE001";
        //    }
        //}
    }
}
