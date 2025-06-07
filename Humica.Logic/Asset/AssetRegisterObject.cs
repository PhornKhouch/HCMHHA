using Humica.Core.DB;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic.CF;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;

namespace Humica.Logic.Asset
{
    public class AssetRegisterObject
    {
        public HumicaDBContext DB = new HumicaDBContext();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public string ScreenId { get; set; }
        public string Code { get; set; }
        public HRAssetRegister Header { get; set; }
        public List<HRAssetRegister> ListHeader { get; set; }
        public List<HRAssetStaff> ListAssetStaff { get; set; }

        public AssetRegisterObject()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }
        public string CreateFixAsset()
        {
            try
            {
                DB = new HumicaDBContext();
                var objMatch = DB.HRAssetRegisters.FirstOrDefault(w => w.AssetCode == Header.AssetCode);

                if (objMatch != null)
                {
                    return "ASSET_EX";
                }

                if (Header.ParentAsset == Header.AssetClassCode)
                {
                    return "INV_PARENT_CODE";
                }

                if (Header.BranchCode == "" || Header.BranchCode == null)
                {
                    return "BRANCH_NE";
                }

                var objClass = DB.HRAssetClasses.Find(Header.AssetClassCode);
                if (objClass == null)
                {
                    return "FA_CLS_NE";
                }
                Header.AssetTypeID = objClass.AssetTypeCode;
                Header.CreatedBy = User.UserName;
                Header.CreatedOn = DateTime.Now;
                Header.StatusUse = SYDocumentStatus.OPEN.ToString();
                if (Header.Status == "A")
                {
                    Header.IsActive = true;
                }


                if (!(objClass.NumberRank == null || objClass.NumberRank == ""))
                {
                    var objNumber = new CFNumberRank(objClass.NumberRank, DocConfType.FixedAsset, Header.CreatedOn.Year, true);
                    if (objNumber == null)
                    {
                        return "NUMBER_RANK_NE";
                    }
                    if (objNumber.NextNumberRank == null)
                    {
                        return "NUMBER_RANK_NE";
                    }
                    Header.AssetCode = objNumber.NextNumberRank;
                }

                if (Header.AssetCode == null)
                {
                    return "CODE_REQUIRED";
                }

                DB.HRAssetRegisters.Add(Header);
                var row = DB.SaveChanges();

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Header.AssetCode;
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
                log.DocurmentAction = Header.AssetCode;
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
                log.DocurmentAction = Header.AssetCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();
                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string EditFixAsset(string id)
        {
            try
            {
                DB = new HumicaDBContext();
                var obj = DB.HRAssetRegisters.SingleOrDefault(w => w.AssetCode == id);

                if (Header.BranchCode == null)
                {
                    return "BRANCH_REQUIRED";
                }

                if (Header.ParentAsset == Header.AssetClassCode)
                {
                    return "INV_PARENT_CODE";
                }

                var objClass = DB.HRAssetClasses.Find(Header.AssetClassCode);
                if (objClass == null)
                {
                    return "FA_CLS_NE";
                }

                if (obj != null)
                {
                    //Asset Summary
                    obj.BranchCode = Header.BranchCode;
                    obj.Description = Header.Description;
                    //obj.AssetClassCode = Header.AssetClassCode;
                    obj.PropertyType = Header.PropertyType;

                    obj.UsefulLifeYear = Header.UsefulLifeYear;
                    obj.Qty = Header.Qty;
                    obj.ReceiptDate = Header.ReceiptDate;
                    obj.AcquisitionCost = Header.AcquisitionCost;

                    //TrackingInfo
                    obj.BuildingCD = Header.BuildingCD;
                    obj.Floor = Header.Floor;
                    obj.Room = Header.Room;
                    obj.DepartmentCD = Header.DepartmentCD;
                    obj.Reason = Header.Reason;
                    obj.TagNbr = Header.TagNbr;

                    //Purchase Info
                    obj.Model = Header.Model;
                    obj.SerialNumber = Header.SerialNumber;
                    obj.WarrantyExpirationDate = Header.WarrantyExpirationDate;
                    obj.Condition = Header.Condition;

                    //Photo
                    obj.Images = Header.Images;

                    obj.ChangedBy = User.UserName;
                    obj.ChangedOn = DateTime.Now;

                    if (obj.Status == "A")
                    {
                        obj.IsActive = true;
                    }

                    DB.Entry(obj).State = EntityState.Modified;

                }

                int row1 = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Header.AssetCode;
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();
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
                log.DocurmentAction = Header.AssetCode;
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();
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
                log.DocurmentAction = Header.AssetCode;
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();
                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }

        public string DeleteFixAsset(string ID)
        {
            try
            {
                Header = new HRAssetRegister();
                var objMatch = DB.HRAssetRegisters.FirstOrDefault(w => w.AssetCode == ID);
                if (objMatch == null)
                {
                    return "ASSET_EX";
                }
                Header = objMatch;
                //objMatch.IsDeleted = true;
                DB.HRAssetRegisters.Attach(objMatch);
                //DB.Entry(objMatch).Property(w => w.IsDeleted).IsModified = true;
                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Header.AssetCode;
                log.Action = Humica.EF.SYActionBehavior.DELETE.ToString();
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
                log.DocurmentAction = Header.AssetCode;
                log.Action = Humica.EF.SYActionBehavior.DELETE.ToString();
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
                log.DocurmentAction = Header.AssetCode;
                log.Action = Humica.EF.SYActionBehavior.DELETE.ToString();
                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
    }
}