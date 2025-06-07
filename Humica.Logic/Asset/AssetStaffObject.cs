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
    public class AssetStaffObject
    {
        public HumicaDBContext DB = new HumicaDBContext();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public string ScreenId { get; set; }
        public string MessageError { get; set; }
        public string DocType { get; set; }
        public HRAssetStaff Header { get; set; }
        public List<HRAssetStaff> ListHeader { get; set; }
        public List<HRAssetRegister> ListAsset { get; set; }
        public HR_STAFF_VIEW HeaderStaff { get; set; }
        public List<MDUploadTemplate> ListTemplate { get; set; }
        public string Code { get; set; }
        public string MessageCode { get; set; }

        public AssetStaffObject()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }

        public string AssignAsset()
        {
            try
            {
                Header.EmpCode = HeaderStaff.EmpCode;
                Header.EmployeName = HeaderStaff.AllName;
                Header.CreatedBy = User.UserName;
                Header.CreatedOn = DateTime.Now;
                Header.Condition = Header.Condition;
                Header.Attachment = Header.Attachment;
                var checkdup = DB.HRAssetRegisters.Where(w => w.AssetCode == Header.AssetCode).ToList();
                foreach (var read in checkdup.ToList())
                {
                    read.StatusUse = Header.Status;
                    DB.HRAssetRegisters.Attach(read);
                    DB.Entry(read).Property(x => x.StatusUse).IsModified = true;
                }
                DB.HRAssetStaffs.Add(Header);

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
        public string updAssign(string id)
        {
            try
            {
                DB = new HumicaDBContext();
                int ID = Convert.ToInt32(id);
                var ObjMatch = DB.HRAssetStaffs.FirstOrDefault(w => w.ID == ID);

                if (ObjMatch == null)
                {
                    return "DOC_INV";
                }

                ObjMatch.ChangedBy = User.UserName;
                ObjMatch.ChangedOn = DateTime.Now;
                ObjMatch.AssignDate = Header.AssignDate;
                ObjMatch.Remark = Header.Remark;
                ObjMatch.Attachment = Header.Attachment;
                DB.HRAssetStaffs.Attach(ObjMatch);

                DB.Entry(ObjMatch).Property(x => x.ChangedBy).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.ChangedOn).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.AssignDate).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Remark).IsModified = true;
                DB.Entry(ObjMatch).Property(x => x.Attachment).IsModified = true;

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
        public string deleteAssign(string id)
        {
            try
            {
                DB = new HumicaDBContext();

                int ID = Convert.ToInt32(id);
                var ObjMatch = DB.HRAssetStaffs.FirstOrDefault(w => w.ID == ID);
                string RETURN = SYDocumentStatus.RETURN.ToString();
                if (ObjMatch == null || ObjMatch.Status != RETURN)
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
                    DB.HRAssetStaffs.Remove(ObjMatch);
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
        public string Transfer(string id)
        {
            try
            {
                HumicaDBContext DBI = new HumicaDBContext();
                int ID = Convert.ToInt32(id);
                HRAssetStaff objmatch = DB.HRAssetStaffs.First(w => w.ID == ID);
                string assign = SYDocumentStatus.ASSIGN.ToString();

                if (objmatch == null || objmatch.Status != assign)
                {
                    return "DOC_INV";
                }
                else
                {
                    var checkdup = DB.HRAssetRegisters.Where(w => w.AssetCode == objmatch.AssetCode).ToList();
                    foreach (var read in checkdup.ToList())
                    {
                        read.StatusUse = SYDocumentStatus.OPEN.ToString();
                        DB.HRAssetRegisters.Attach(read);
                        DB.Entry(read).Property(x => x.StatusUse).IsModified = true;
                    }
                    objmatch.ReturnDate = DateTime.Now;
                    objmatch.Status = SYDocumentStatus.RETURN.ToString();
                    objmatch.ChangedBy = User.UserName;
                    objmatch.ChangedOn = DateTime.Now;
                    DB.HRAssetStaffs.Attach(objmatch);
                    DB.Entry(objmatch).Property(w => w.ReturnDate).IsModified = true;
                    DB.Entry(objmatch).Property(w => w.Status).IsModified = true;
                    DB.Entry(objmatch).Property(w => w.ChangedBy).IsModified = true;
                    DB.Entry(objmatch).Property(w => w.ChangedOn).IsModified = true;
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
                log.DocurmentAction = id;
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
                log.DocurmentAction = id;
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
                log.DocurmentAction = id;
                log.Action = SYActionBehavior.RELEASE.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string Import()
        {
            try
            {
                if (ListHeader.Count == 0)
                {
                    return "NO_DATA";
                }
                try
                {
                    DB.Configuration.AutoDetectChangesEnabled = false;
                    var _list = new List<HREmpFamily>();
                    List<HRStaffProfile> _listStaff = new List<HRStaffProfile>();
                    _list = DB.HREmpFamilies.ToList();
                    _listStaff = DB.HRStaffProfiles.ToList();

                    var date = DateTime.Now;
                    var ListAsscode = DB.HRAssetRegisters.Where(w => w.StatusUse == "OPEN").ToList();
                    foreach (var staffs in ListHeader.ToList())
                    {
                        Header = new HRAssetStaff();
                        var EmpCode = _listStaff.Where(w => w.EmpCode == staffs.EmpCode).ToList();
                        Header.EmpCode = "";
                        if (EmpCode.Count <= 1)
                        {
                            if (EmpCode.Count == 1)
                            {
                                Header.EmpCode = EmpCode.FirstOrDefault().EmpCode;
                            }
                            Header.EmpCode = staffs.EmpCode;
                            Header.EmployeName = staffs.EmployeName;
                            Header.AssetCode = staffs.AssetCode;
                            Header.AssetDescription = staffs.AssetDescription;
                            Header.AssignDate = staffs.AssignDate;
                            Header.ReturnDate = staffs.ReturnDate;
                            Header.Status = staffs.Status;
                            Header.Condition = staffs.Condition;
                            Header.Remark = staffs.Remark;
                            Header.CreatedBy = User.UserName;
                            Header.CreatedOn = DateTime.Now;
                            var Asscode = ListAsscode.Where(w => w.AssetCode == Header.AssetCode).FirstOrDefault();
                            if (Asscode != null)
                            {
                                Asscode.StatusUse = "ASSIGN";
                                DB.HRAssetRegisters.Attach(Asscode);
                                DB.Entry(Asscode).Property(x => x.StatusUse).IsModified = true;
                            }
                            DB.HRAssetStaffs.Add(Header);
                        }
                        else
                        {
                            foreach (var item1 in EmpCode)
                            {
                                if (item1 != null)
                                {
                                    Header.EmpCode = item1.EmpCode;
                                }
                                Header.EmpCode = staffs.EmpCode;
                                Header.EmployeName = staffs.EmployeName;
                                Header.AssetCode = staffs.AssetCode;
                                Header.AssetDescription = staffs.AssetDescription;
                                Header.AssignDate = staffs.AssignDate;
                                Header.ReturnDate = staffs.ReturnDate;
                                Header.Status = staffs.Status;
                                Header.Status = staffs.Status;
                                Header.Remark = staffs.Remark;
                                Header.CreatedBy = User.UserName;
                                Header.CreatedOn = DateTime.Now;
                                var Asscode = ListAsscode.Where(w => w.AssetCode == Header.AssetCode).FirstOrDefault();
                                if (Asscode != null)
                                {
                                    Asscode.StatusUse = "ASSIGN";
                                    DB.HRAssetRegisters.Attach(Asscode);
                                    DB.Entry(Asscode).Property(x => x.StatusUse).IsModified = true;
                                }
                                DB.HRAssetStaffs.Add(Header);
                            }
                        }
                    }

                    DB.SaveChanges();
                    return SYConstant.OK;
                }
                finally
                {
                    DB.Configuration.AutoDetectChangesEnabled = true;
                }
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Header.EmpCode;
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
                log.DocurmentAction = Header.EmpCode;
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
                log.DocurmentAction = Header.EmpCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
    }
}
