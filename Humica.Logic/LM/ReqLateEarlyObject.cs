using Humica.Core.DB;
using Humica.Core.FT;
using Humica.Core.SY;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic.CF;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Humica.Logic.LM
{
    public class ReqLateEarlyObject
    {
        public HumicaDBContext DB = new HumicaDBContext();
        public SMSystemEntity DP = new SMSystemEntity();
        public HumicaDBViewContext DBV = new HumicaDBViewContext();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public string ScreenId { get; set; }
        public string MessageError { get; set; }
        public string DocType { get; set; }
        public HRReqLateEarly Header { get; set; }
        public List<HRReqLateEarly> ListHeader { get; set; }
        public FTINYear FInYear { get; set; }
        public HR_STAFF_VIEW HeaderStaff { get; set; }
        public List<ExDocApproval> ListApproval { get; set; }
        public List<ClsReuestLaEa> ListReqPending { get; set; }
        public ReqLateEarlyObject()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }
        public void SetAutoApproval(string EmpCode, string Branch, string DocType)
        {
            ListApproval = new List<ExDocApproval>();
            var DBX = new HumicaDBContext();
            var LstStaff = DB.HRStaffProfiles.ToList();
            LstStaff = LstStaff.Where(w => w.DateTerminate.Year == 1900).ToList();
            var ListWorkFlow = DB.HRWorkFlowLeaves.ToList();
            var _staffApp = new HRStaffProfile();
            foreach (var item in ListWorkFlow)
            {
                var Staff = LstStaff.FirstOrDefault(w => w.EmpCode == EmpCode);
                if (item.ApproveBy == "1st")
                {
                    var Read = LstStaff.Where(w => w.EmpCode == Staff.FirstLine).ToList();
                    _staffApp = Read.FirstOrDefault();
                }
                else if (item.ApproveBy == "2nd")
                {
                    if (Staff.SecondLine != null)
                    {
                        var Read = LstStaff.Where(w => w.EmpCode == Staff.SecondLine).ToList();
                        _staffApp = Read.FirstOrDefault();
                    }
                }
                else
                {
                    _staffApp = LstStaff.FirstOrDefault(w => w.JobCode == item.ApproveBy && w.Branch == Branch);
                }
                if (_staffApp == null)
                    _staffApp = LstStaff.FirstOrDefault(w => w.JobCode == item.ApproveBy);
                if (_staffApp == null) continue;
                if (ListApproval.Where(w => w.Approver == _staffApp.EmpCode).Count() > 0) continue;
                var objApp = new ExDocApproval();
                objApp.Approver = _staffApp.EmpCode;
                objApp.ApproverName = _staffApp.AllName;
                objApp.DocumentType = DocType;
                objApp.ApproveLevel = item.Step;
                objApp.WFObject = "WF02";
                ListApproval.Add(objApp);
            }
        }
        public HRStaffProfile getNextApprover(string id, string DocType)
        {
            var objStaff = new HRStaffProfile();
            var DBX = new HumicaDBContext();
            string open = SYDocumentStatus.OPEN.ToString();
            var listCanApproval = DBX.ExDocApprovals.Where(w => w.DocumentNo == id && w.Status == open && w.DocumentType == DocType).ToList();

            if (listCanApproval.Count == 0)
            {
                return new HRStaffProfile();
            }

            var min = listCanApproval.Min(w => w.ApproveLevel);
            var NextApp = listCanApproval.Where(w => w.ApproveLevel == min).First();
            objStaff = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == NextApp.Approver);
            return objStaff;
        }
        public string CreateReqLateEarly()
        {
            try
            {
                if (Header.RequestType == null)
                {
                    return "INV_DOC";
                }
                if (Header.Qty <= 0)
                {
                    return "INV_BALANCE";
                }

                string Reject = SYDocumentStatus.REJECTED.ToString();
                string Cancel = SYDocumentStatus.CANCELLED.ToString();
                var lstempLeave = DB.HRReqLateEarlies.Where(w => w.EmpCode == HeaderStaff.EmpCode).ToList();
                var leaveH = lstempLeave.Where(w => w.EmpCode == HeaderStaff.EmpCode && w.Status != Cancel && w.Status != Reject).ToList();
                if (leaveH.Where(w => w.LeaveDate.Date == Header.LeaveDate.Date).Any())
                {
                    return "INV_DATE";
                }
                if (Header.Reason == "" || Header.Reason == null)
                    return "REASON_EN";
                var objNumber = new CFNumberRank(DocType);
                Header.ReqLaEaNo = objNumber.NextNumberRank.Trim();
                Header.EmpCode = HeaderStaff.EmpCode;
                Header.Status = SYDocumentStatus.APPROVED.ToString();
                Header.CreatedOn = DateTime.Now;
                Header.CreatedBy = User.UserName;
                Header.EmpName = HeaderStaff.AllName;

                DB.HRReqLateEarlies.Add(Header);

                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Header.ReqLaEaNo;
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
                log.DocurmentAction = Header.ReqLaEaNo;
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
                log.DocurmentAction = Header.ReqLaEaNo;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string editReqLateEarly(string id)
        {
            try
            {
                HRReqLateEarly objMast = DB.HRReqLateEarlies.Find(id);
                objMast.Qty = Header.Qty;
                objMast.Reason = Header.Reason;
                objMast.ChangedOn = DateTime.Now.Date;
                objMast.ChangedBy = User.UserName;
                DB.HRReqLateEarlies.Attach(objMast);
                DB.Entry(objMast).Property(w => w.Qty).IsModified = true;
                DB.Entry(objMast).Property(w => w.Reason).IsModified = true;
                DB.Entry(objMast).Property(w => w.ChangedOn).IsModified = true;
                DB.Entry(objMast).Property(w => w.ChangedBy).IsModified = true;
                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserID.ToString();
                log.ScreenId = Header.ReqLaEaNo;
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string deleteReqLateEarly(string ReqLaEaNo)
        {
            try
            {

                HRReqLateEarly objMater = DB.HRReqLateEarlies.Find(ReqLaEaNo);

                DB.HRReqLateEarlies.Attach(objMater);
                DB.Entry(objMater).State = System.Data.Entity.EntityState.Deleted;
                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserID.ToString();
                log.ScreenId = Header.ReqLaEaNo;
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
                log.ScreenId = Header.ReqLaEaNo;
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string ESSRequestLaEa(string URL)
        {
            try
            {
                DB = new HumicaDBContext();
                var LeaveD = new List<HREmpLeaveD>();
                if (Header.RequestType == null)
                {
                    return "INV_DOC";
                }
                if (Header.Qty <= 0)
                {
                    return "INV_BALANCE";
                }

                string Reject = SYDocumentStatus.REJECTED.ToString();
                string Cancel = SYDocumentStatus.CANCELLED.ToString();
                var lstempLeave = DB.HRReqLateEarlies.Where(w => w.EmpCode == HeaderStaff.EmpCode).ToList();
                var leaveH = lstempLeave.Where(w => w.EmpCode == HeaderStaff.EmpCode && w.Status != Cancel && w.Status != Reject).ToList();
                if (leaveH.Where(w => w.LeaveDate.Date == Header.LeaveDate.Date).Any())
                {
                    return "INV_DATE";
                }
                 var Result = leaveH.Where(w => w.LeaveDate.Date == Header.LeaveDate.Date ).ToList();
                //var LateEarly = leaveH.Where(w => w.LeaveDate.Month == Header.LeaveDate.Month && w.LeaveDate.Year == Header.LeaveDate.Year && (w.RequestType == "LATE1" || w.RequestType == "EARLY2")).ToList();
                //var Misscan = leaveH.Where(w => w.LeaveDate.Month == Header.LeaveDate.Month && w.LeaveDate.Year == Header.LeaveDate.Year && (w.Remark == "MISSSCAN1" || w.Remark == "MISSSCAN4")).ToList();
                //if (LateEarly.Count >= 6)
                //{
                //    return "LATE_EARLY";
                //}
                //if (Misscan.Count() >= 6)
                //{
                //    return "LATE_EARLY";
                //}
                if (Header.Reason == "" || Header.Reason == null)
                    return "REASON_EN";
                var Policy = DB.ATPolicies.First();
                if (Policy != null)
                {
                    if (Policy.IsLate_Early > 0)
                    {
                        var count = leaveH.Where(w => w.LeaveDate.Year == Header.LeaveDate.Year && w.LeaveDate.Month == Header.LeaveDate.Month).Count();
                        if (Policy.IsLate_Early < count)
                        {
                            MessageError = Policy.IsLate_Early.ToString();
                            return "INV_BALANCE";
                        }
                    }
                }
                var StaffList = DB.HRStaffProfiles.ToList();
                StaffList = StaffList.Where(x => x.DateTerminate.Year == 1900).ToList();
                var staff = StaffList.FirstOrDefault(x => x.EmpCode == HeaderStaff.EmpCode && x.DateTerminate.Year == 1900);
                //SetAutoApproval(staff.EmpCode, staff.Branch, Header.RequestType);
                //var objCF = DB.ExCfWorkFlowItems.Find(ScreenId, "RLE01");//, Header.RequestType
                var objCF = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == HeaderStaff.EmpCode);
                if (objCF == null)
                {
                    return "REQUEST_TYPE_NE";
                }
               
                SetAutoApproval_(staff.EmpCode, StaffList);
                var ListWorkFlow = DB.HRWorkFlowLeaves.ToList();
                string Status = SYDocumentStatus.PENDING.ToString();
                var objNumber = new CFNumberRank(DocType);
                Header.ReqLaEaNo = objNumber.NextNumberRank.Trim();
                Header.EmpCode = HeaderStaff.EmpCode;
                Header.EmpName = HeaderStaff.AllName;
                Header.Status = Status;
                Header.CreatedBy = User.UserName;
                Header.CreatedOn = DateTime.Now;
                DB.HRReqLateEarlies.Add(Header);
                if (ListApproval.Count() <= 0)
                {
                    return "NO_LINE_MN";
                }
                //Add approver
                foreach (var read in ListApproval)
                {
                    read.ID = 0;
                    read.LastChangedDate = DateTime.Now;
                    read.DocumentNo = Header.ReqLaEaNo;
                    read.Status = SYDocumentStatus.OPEN.ToString();
                    read.ApprovedBy = "";
                    read.ApprovedName = "";
                    DB.ExDocApprovals.Add(read);
                }

                int row = DB.SaveChanges();
                URL += Header.ReqLaEaNo;
                #region ---Send To Telegram---
                var EmpBooking = DB.HRReqLateEarlies.Where(w => w.ReqLaEaNo == Header.ReqLaEaNo).ToList();
                var HOD = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == staff.FirstLine);
                var _Type = DB.HRReqLateEarlies.FirstOrDefault(w => w.ReqLaEaNo == Header.ReqLaEaNo);
                var Post = DB.HRPositions.FirstOrDefault(w => w.Code == staff.JobCode);
                var QTY = "";
                if (_Type.Qty > 0)
                { 
                    QTY = _Type.Qty.ToString(); 
                }
                else
                {
                    QTY = _Type.Remark;
                }
                var Link = URL;
                if (objCF != null && HOD != null)
                {
                    string str = "Dear <b>"+HOD.Title+" . " + HOD.AllName + "</b>, I would like to request " + _Type.RequestType + " for my team as below:";
                    foreach (var read in EmpBooking)
                    {
                        str += @"%0A- <b>" + read.EmpName + "</b> on " + read.LeaveDate.ToString("dd.MMM.yyyy");
                    }
                    str += "<b> Total: </b>" + QTY+ "%0A<b> Position: </b>" + Post.Description;
                    str += "%0A*<b>" + EmpBooking.First().Reason + "</b> Thanks you.";
                    str += "%0A%0AYours sincerely,%0A%0A<b>" + EmpBooking.First().EmpName +" </b>";//+ "%0A%0APlease login at: " + Link;
                    SYSendTelegramObject Tel = new SYSendTelegramObject();
                    Tel.User = User;
                    Tel.BS = BS;
                    List<object> ListObjectDictionary = new List<object>();
                    WorkFlowResult result1 = Tel.Send_SMS_Telegram(str, objCF.TeleGroup, false);
                    MessageError = Tel.getErrorMessage(result1);
                }
                #endregion

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserID.ToString();
                log.ScreenId = Header.EmpCode;
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string isValidApproval(ExDocApproval Approver, EnumActionGridLine Action)
        {
            if (Action == EnumActionGridLine.Add)
            {
                if (ListApproval.Where(w => w.Approver == Approver.Approver).ToList().Count > 0)
                {
                    return "DUPLICATED_ITEM";
                }
            }

            return SYConstant.OK;
        }
        public string ApproveOTReq(string ReqLaEaNo, string URL)
        {
            try
            {
                string[] c = ReqLaEaNo.Split(';');
                foreach (var ID in c)
                {
                    var objMatch = DB.HRReqLateEarlies.FirstOrDefault(w => w.ReqLaEaNo == ID);
                    if (objMatch == null)
                    {
                        return "EE001";
                    }
                    if (objMatch.Status != SYDocumentStatus.PENDING.ToString())
                    {
                        return "INV_DOC";
                    }

                    string open = SYDocumentStatus.OPEN.ToString();
                    string DocNo = objMatch.ReqLaEaNo;
                    var listApproval = DB.ExDocApprovals.Where(w => w.DocumentType == objMatch.RequestType
                                        && w.DocumentNo == DocNo && w.Status == open).OrderBy(w => w.ApproveLevel).ToList();
                    var listUser = DB.HRStaffProfiles.Where(w => w.EmpCode == User.UserName).ToList();
                    var b = false;
                    if (listApproval.Count == 0)
                    {
                        return "RESTRICT_ACCESS";
                    }
                    foreach (var read in listApproval)
                    {
                        var checklist = listUser.Where(w => w.EmpCode == read.Approver).ToList();
                        if (checklist.Count > 0)
                        {
                            if (read.Status == SYDocumentStatus.APPROVED.ToString())
                            {
                                return "USER_ALREADY_APP";
                            }
                            var objStaff = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == read.Approver);
                            if (objStaff != null)
                            {
                                //New
                                if (listApproval.Where(w => w.ApproveLevel <= read.ApproveLevel).Count() >= listApproval.Count())
                                {
                                    listApproval.ForEach(w => w.Status = SYDocumentStatus.APPROVED.ToString());
                                }
                                read.ApprovedBy = objStaff.EmpCode;
                                read.ApprovedName = objStaff.AllName;
                                read.LastChangedDate = DateTime.Now.Date;
                                read.ApprovedDate = DateTime.Now;
                                read.Status = SYDocumentStatus.APPROVED.ToString();
                                DB.ExDocApprovals.Attach(read);
                                DB.Entry(read).State = System.Data.Entity.EntityState.Modified;
                                b = true;
                                break;
                            }
                        }

                    }
                    if (listApproval.Count > 0)
                    {
                        if (b == false)
                        {
                            return "USER_NOT_APPROVOR";
                        }
                    }

                    var status = SYDocumentStatus.APPROVED.ToString();
                    if (listApproval.Where(w => w.Status == open).ToList().Count > 0)
                    {
                        status = SYDocumentStatus.PENDING.ToString();
                    }
                    objMatch.Status = status;

                    DB.Entry(objMatch).Property(w => w.Status).IsModified = true;
                    DB.Entry(objMatch).Property(w => w.ChangedOn).IsModified = true;
                    DB.Entry(objMatch).Property(w => w.ChangedBy).IsModified = true;
                    int row = DB.SaveChanges();

                    var StaffApp = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == objMatch.EmpCode);
                    if (StaffApp != null)
                    {
                        // string WFObject = "ESSOT";
                        string Email_Template = "ESSLAEA";
                        if (objMatch.Status == SYDocumentStatus.APPROVED.ToString())
                        {
                            //  WFObject = "ESSOT_APP";
                            Email_Template = "ESSLAEA_APP";
                        }
                        else
                        {
                            StaffApp = getNextApprover(objMatch.ReqLaEaNo, objMatch.RequestType);
                        }
                        if (StaffApp.Email != "" && StaffApp.Email != null)
                        {
                            #region Send Email
                            //SYWorkFlowEmailObject wfo =
                            //           new SYWorkFlowEmailObject(WFObject, WorkFlowType.REQUESTER,
                            //                UserType.N, SYDocumentStatus.PENDING.ToString());
                            //wfo.SelectListItem = new SYSplitItem(objMatch.ReqLaEaNo);
                            //wfo.User = User;
                            //wfo.BS = BS;
                            //wfo.UrlView = SYUrl.getBaseUrl();
                            //wfo.ScreenId = ScreenId;
                            //wfo.Module = "HR";// CModule.PURCHASE.ToString();
                            //wfo.ListLineRef = new List<BSWorkAssign>();
                            //wfo.DocNo = objMatch.ReqLaEaNo;
                            //wfo.Action = SYDocumentStatus.PENDING.ToString();
                            //wfo.ListObjectDictionary = new List<object>();
                            //wfo.ListObjectDictionary.Add(objMatch);
                            //HRStaffProfile Staff = StaffApp;
                            //wfo.ListObjectDictionary.Add(Staff);
                            //WorkFlowResult result1 = wfo.InsertProcessWorkFlowLeave(Staff);
                            //MessageError = wfo.getErrorMessage(result1);
                            #endregion
                        }
                        #region ---Send To Telegram---
                        var EmailTemplate = DP.TPEmailTemplates.Find(Email_Template);
                        if (EmailTemplate != null)
                        {
                            SYSendTelegramObject Tel = new SYSendTelegramObject();
                            Tel.User = User;
                            Tel.BS = BS;
                            List<object> ListObjectDictionary = new List<object>();
                            ListObjectDictionary.Add(objMatch);
                            ListObjectDictionary.Add(StaffApp);
                            WorkFlowResult result1 = Tel.Send_SMS_Telegram(Email_Template, EmailTemplate.RequestContent, StaffApp.TeleChartID, ListObjectDictionary, URL);
                            MessageError = Tel.getErrorMessage(result1);
                        }
                        #endregion
                        if (objMatch.Status == SYDocumentStatus.APPROVED.ToString())
                        {
                            var Sett = DB.SYHRSettings.First();
                            var EmailTemplateCC = DP.TPEmailTemplates.Find("ESSLAEA_APP_CC");
                            if (EmailTemplateCC != null)
                            {
                                SYSendTelegramObject Tel = new SYSendTelegramObject();
                                Tel.User = User;
                                Tel.BS = BS;
                                List<object> ListObjectDictionary = new List<object>();
                                ListObjectDictionary.Add(objMatch);
                                ListObjectDictionary.Add(StaffApp);
                                WorkFlowResult result1 = Tel.Send_SMS_Telegram(EmailTemplateCC.EMTemplateObject, EmailTemplateCC.RequestContent, Sett.TelegLeave, ListObjectDictionary, URL);
                                MessageError = Tel.getErrorMessage(result1);
                            }
                        }
                    }

                }
                return SYConstant.OK;
            }
            catch (Exception e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ReqLaEaNo;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string requestToApprove(string ReqLaEaNo, string URL)
        {
            try
            {
                string[] c = ReqLaEaNo.Split(';');
                foreach (var id in c)
                {
                    HumicaDBContext DBX = new HumicaDBContext();
                    var objMatch = DB.HRReqLateEarlies.FirstOrDefault(w => w.ReqLaEaNo == id);
                    if (objMatch == null)
                    {
                        return "REQUEST_NE";
                    }
                    Header = objMatch;
                    var LstStaffProfiles = DB.HRStaffProfiles.Where(w => w.Status == "A").ToList();
                    var StaffReq = LstStaffProfiles.FirstOrDefault(w => w.EmpCode == Header.EmpCode);
                    HRStaffProfile StaffApp = new HRStaffProfile();
                    var chkFLName = LstStaffProfiles.Where(w => w.JobCode == StaffReq.FirstLine).ToList();
                    if (chkFLName.Count() > 0)
                    {
                        StaffApp = chkFLName.FirstOrDefault(w => w.DEPT == StaffReq.DEPT);
                    }
                    else if (chkFLName.Count() == 0)
                    {
                        var chkSLName = LstStaffProfiles.Where(w => w.JobCode == StaffReq.SecondLine).ToList();
                        if (chkSLName.Count() > 0)
                        {
                            StaffApp = chkSLName.First();
                        }
                    }

                    if (objMatch.Status != SYDocumentStatus.OPEN.ToString())
                    {
                        return "INV_DOC";
                    }

                    objMatch.Status = SYDocumentStatus.PENDING.ToString();
                    if (StaffApp != null)
                    {
                        var DocApproval = new ExDocApproval();
                        DocApproval.Approver = StaffApp.EmpCode;
                        DocApproval.ApproverName = StaffApp.AllName;
                        DocApproval.ApprovedBy = "";
                        DocApproval.ApprovedName = "";
                        DocApproval.ApproveLevel = 1;
                        DocApproval.DocumentType = "REQ_OT";
                        DocApproval.DocumentNo = objMatch.ReqLaEaNo.ToString();
                        DocApproval.Status = SYDocumentStatus.OPEN.ToString();
                        DocApproval.WFObject = "WF02";
                        DB.ExDocApprovals.Add(DocApproval);
                    }
                    DB.HRReqLateEarlies.Attach(objMatch);
                    DB.Entry(objMatch).Property(w => w.Status).IsModified = true;
                    DB.SaveChanges();

                    // int row = DBX.SaveChanges();

                    if (StaffApp != null)
                    {
                        if (StaffApp.Email != "" && StaffApp.Email != null)
                        {
                            #region Send Email
                            SYWorkFlowEmailObject wfo =
                                       new SYWorkFlowEmailObject("ESSOT", WorkFlowType.REQUESTER,
                                            UserType.N, SYDocumentStatus.PENDING.ToString());
                            wfo.SelectListItem = new SYSplitItem(objMatch.ReqLaEaNo);
                            wfo.User = User;
                            wfo.BS = BS;
                            wfo.UrlView = SYUrl.getBaseUrl();
                            wfo.ScreenId = ScreenId;
                            wfo.Module = "HR";// CModule.PURCHASE.ToString();
                            wfo.ListLineRef = new List<BSWorkAssign>();
                            wfo.DocNo = objMatch.ReqLaEaNo;
                            wfo.Action = SYDocumentStatus.PENDING.ToString();
                            wfo.ListObjectDictionary = new List<object>();
                            wfo.ListObjectDictionary.Add(objMatch);
                            HRStaffProfile Staff = StaffApp;
                            wfo.ListObjectDictionary.Add(Staff);
                            WorkFlowResult result1 = wfo.InsertProcessWorkFlowLeave(Staff);
                            MessageError = wfo.getErrorMessage(result1);
                            #endregion
                        }
                        #region ---Send To Telegram---
                        var EmailTemplate = DP.TPEmailTemplates.Find("ESSOT_TELEGRAM");
                        if (EmailTemplate != null)
                        {
                            SYSendTelegramObject Tel = new SYSendTelegramObject();
                            Tel.User = User;
                            Tel.BS = BS;
                            List<object> ListObjectDictionary = new List<object>();
                            ListObjectDictionary.Add(objMatch);
                            ListObjectDictionary.Add(StaffApp);
                            Tel.Send_SMS_Telegram("ESSOT_TELEGRAM", EmailTemplate.RequestContent, StaffApp.TeleChartID, ListObjectDictionary, URL);
                        }
                        #endregion
                    }
                }
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.ScreenId = Header.ReqLaEaNo;
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
                log.DocurmentAction = Header.ReqLaEaNo;
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
                log.DocurmentAction = Header.ReqLaEaNo;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string RejectOTReq(string ID)
        {
            try
            {
                var objMatch = DB.HRReqLateEarlies.FirstOrDefault(w => w.ReqLaEaNo == ID);
                if (objMatch == null)
                {
                    return "EE001";
                }
                objMatch.Status = SYDocumentStatus.REJECTED.ToString();
                objMatch.ChangedBy = User.UserName;
                objMatch.ChangedOn = DateTime.Now;
                var listApproval = DB.ExDocApprovals.Where(w => w.DocumentType == "REQ_OT"
                                        && w.DocumentNo == ID).OrderBy(w => w.ApproveLevel).ToList();
                var listUser = DB.HRStaffProfiles.Where(w => w.EmpCode == User.UserName).ToList();
                var b = false;
                if (listApproval.Count == 0)
                {
                    return "RESTRICT_ACCESS";
                }
                foreach (var read in listApproval)
                {
                    var checklist = listUser.Where(w => w.EmpCode == read.Approver).ToList();
                    if (checklist.Count > 0)
                    {
                        if (read.Status == SYDocumentStatus.REJECTED.ToString())
                        {
                            return "USER_ALREADY_APP";
                        }
                        var objStaff = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == read.Approver);
                        if (objStaff != null)
                        {
                            //New
                            if (listApproval.Where(w => w.ApproveLevel <= read.ApproveLevel).Count() >= listApproval.Count())
                            {
                                listApproval.ForEach(w => w.Status = SYDocumentStatus.APPROVED.ToString());
                            }
                            read.ApprovedBy = objStaff.EmpCode;
                            read.ApprovedName = objStaff.AllName;
                            read.LastChangedDate = DateTime.Now.Date;
                            read.ApprovedDate = DateTime.Now;
                            read.Status = SYDocumentStatus.REJECTED.ToString();
                            DB.ExDocApprovals.Attach(read);
                            DB.Entry(read).State = System.Data.Entity.EntityState.Modified;
                            b = true;
                            break;
                        }
                    }

                }
                if (listApproval.Count > 0)
                {
                    if (b == false)
                    {
                        return "USER_NOT_REJECT";
                    }
                }
                DB.Entry(objMatch).Property(w => w.Status).IsModified = true;
                DB.Entry(objMatch).Property(w => w.ChangedOn).IsModified = true;
                DB.Entry(objMatch).Property(w => w.ChangedBy).IsModified = true;
                DB.Entry(objMatch).Property(w => w.Status).IsModified = true;
                DB.Entry(objMatch).Property(w => w.ChangedOn).IsModified = true;
                DB.Entry(objMatch).Property(w => w.ChangedBy).IsModified = true;
                int row = DB.SaveChanges();
                var StaffApp = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == objMatch.EmpCode);
                if (StaffApp != null)
                {
                    #region ---Send To Telegram---
                    var EmailTemplate = DP.TPEmailTemplates.Find("ESSLAEA_REJ");
                    if (EmailTemplate != null)
                    {
                        SYSendTelegramObject Tel = new SYSendTelegramObject();
                        Tel.User = User;
                        Tel.BS = BS;
                        List<object> ListObjectDictionary = new List<object>();
                        ListObjectDictionary.Add(objMatch);
                        ListObjectDictionary.Add(StaffApp);
                        var URL = "";
                        WorkFlowResult result1 = Tel.Send_SMS_Telegram("ESSLAEA_REJ", EmailTemplate.RequestContent, StaffApp.TeleChartID, ListObjectDictionary, URL);
                        MessageError = Tel.getErrorMessage(result1);
                    }
                    #endregion
                }
                return SYConstant.OK;
            }

            catch (Exception e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ID;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string CancelOTReq(string ReqLaEaNo)
        {
            try
            {
                string[] c = ReqLaEaNo.Split(';');
                foreach (var ID in c)
                {
                    var objMatch = DB.HRReqLateEarlies.FirstOrDefault(w => w.ReqLaEaNo == ID);
                    if (objMatch == null)
                    {
                        return "EE001";
                    }
                    var listApproval = DB.ExDocApprovals.Where(w => w.DocumentType == "REQ_OT"
                                        && w.DocumentNo == ID).OrderBy(w => w.ApproveLevel).ToList();
                    var listUser = DB.HRStaffProfiles.Where(w => w.EmpCode == User.UserName).ToList();
                    var b = false;
                    if (listApproval.Count == 0)
                    {
                        return "RESTRICT_ACCESS";
                    }
                    foreach (var read in listApproval)
                    {
                        var checklist = listUser.Where(w => w.EmpCode == read.Approver).ToList();
                        if (checklist.Count > 0)
                        {
                            if (read.Status == SYDocumentStatus.CANCELLED.ToString())
                            {
                                return "USER_ALREADY_APP";
                            }
                            var objStaff = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == read.Approver);
                            if (objStaff != null)
                            {
                                //New
                                if (listApproval.Where(w => w.ApproveLevel <= read.ApproveLevel).Count() >= listApproval.Count())
                                {
                                    listApproval.ForEach(w => w.Status = SYDocumentStatus.APPROVED.ToString());
                                }
                                read.ApprovedBy = objStaff.EmpCode;
                                read.ApprovedName = objStaff.AllName;
                                read.LastChangedDate = DateTime.Now.Date;
                                read.ApprovedDate = DateTime.Now;
                                read.Status = SYDocumentStatus.CANCELLED.ToString();
                                DB.ExDocApprovals.Attach(read);
                                DB.Entry(read).State = System.Data.Entity.EntityState.Modified;
                                b = true;
                                break;
                            }
                        }

                    }
                    if (listApproval.Count > 0)
                    {
                        if (b == false)
                        {
                            return "USER_NOT_CANCELLED";
                        }
                    }
                    objMatch.Status = SYDocumentStatus.CANCELLED.ToString();
                    objMatch.ChangedBy = User.UserName;
                    objMatch.ChangedOn = DateTime.Now;


                    DB.Entry(objMatch).Property(w => w.Status).IsModified = true;
                    DB.Entry(objMatch).Property(w => w.ChangedOn).IsModified = true;
                    DB.Entry(objMatch).Property(w => w.ChangedBy).IsModified = true;
                    int row = DB.SaveChanges();
                }
                return SYConstant.OK;
            }

            catch (Exception e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ReqLaEaNo;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public void SetAutoApproval_(string EmpCode, List<HRStaffProfile> LstStaffProfiles)
        {
            ListApproval = new List<ExDocApproval>();
            var StaffReq = LstStaffProfiles.FirstOrDefault(w => w.EmpCode == EmpCode);
            HRStaffProfile StaffApp = new HRStaffProfile();
            HRStaffProfile StaffHOD = new HRStaffProfile();
            var chkFLName = LstStaffProfiles.Where(w => w.EmpCode == StaffReq.FirstLine).ToList();
            if (chkFLName.Count() > 0)
            {
                //StaffApp = chkFLName.FirstOrDefault(w => w.DEPT == StaffReq.DEPT);
                StaffApp = chkFLName.First();
            }
            else if (chkFLName.Count() == 0)
            {
                var chkSLName = LstStaffProfiles.Where(w => w.EmpCode == StaffReq.SecondLine).ToList();
                if (chkSLName.Count() > 0)
                {
                    StaffApp = chkSLName.First();
                }
            }
            if (LstStaffProfiles.Where(w => w.EmpCode == StaffReq.HODCode).Any())
            {
                StaffHOD = LstStaffProfiles.FirstOrDefault(w => w.EmpCode == StaffReq.HODCode);
            }
            var LstStaff = DB.HRStaffProfiles.ToList();
            LstStaff = LstStaff.Where(w => w.DateTerminate.Year == 1900).ToList();
            var ListWorkFlow = DB.HRWorkFlowLeaves.ToList();
            var _staffApp = new HRStaffProfile();
            int line = 0;
            if (StaffApp != null)
            {
                if (StaffApp.EmpCode != null)
                {
                    line += 1;
                    var DocApproval = new ExDocApproval();
                    DocApproval.Approver = StaffApp.EmpCode;
                    DocApproval.ApproverName = StaffApp.AllName;
                    DocApproval.ApprovedBy = "";
                    DocApproval.ApprovedName = "";
                    DocApproval.ApproveLevel = line;
                    DocApproval.DocumentType = "REQ_OT";
                    //DocApproval.DocumentNo = HeaderOT.OTRNo;
                    DocApproval.Status = SYDocumentStatus.OPEN.ToString();
                    DocApproval.WFObject = "WF02";
                    ListApproval.Add(DocApproval);
                }
            }
            if (StaffHOD != null)
            {
                if (StaffHOD.EmpCode != null)
                {
                    if (ListApproval.Where(w => w.Approver != StaffHOD.EmpCode).Any())
                    {
                        line += 1;
                        var DocApproval = new ExDocApproval();
                        DocApproval.Approver = StaffHOD.EmpCode;
                        DocApproval.ApproverName = StaffHOD.AllName;
                        DocApproval.ApprovedBy = "";
                        DocApproval.ApprovedName = "";
                        DocApproval.ApproveLevel = line;
                        DocApproval.DocumentType = "REQ_OT";
                        DocApproval.DocumentNo = Header.ReqLaEaNo;
                        DocApproval.Status = SYDocumentStatus.OPEN.ToString();
                        DocApproval.WFObject = "WF02";
                        ListApproval.Add(DocApproval);
                    }
                }
            }
        }
        public static class BranchDataAccess
        {
            public static async Task<List<HRBranch>> GetBranchDataAccessAsync()
            {
                // Directly return the cached list if available
                return await GetBranchDataAsync();
            }

            private static async Task<List<HRBranch>> GetBranchDataAsync()
            {
                if (HttpContext.Current != null && HttpContext.Current.Session != null && HttpContext.Current.Session["PLANT_BRANCH_LIST"] != null)
                {
                    // Cast and return the cached list directly without creating a new list
                    var source = HttpContext.Current.Session["PLANT_BRANCH_LIST"] as List<HRBranch>;
                    return await Task.FromResult(source);
                }

                return await Task.FromResult(new List<HRBranch>());
            }

            // Method to set the branch data into the session (if needed)
            public static void SetBranchData(List<HRBranch> branchData)
            {
                if (HttpContext.Current != null && HttpContext.Current.Session != null)
                {
                    HttpContext.Current.Session["PLANT_BRANCH_LIST"] = branchData;
                }
            }
        }

    }

    public class ClsReuestLaEa
    {
        public string ReqLaEaNo { get; set; }
        public string Remark { get; set; }
        public string EmpCode { get; set; }
        public string EmpName { get; set; }
        public DateTime LeaveDate { get; set; }
        public string Reason { get; set; }
        public int? Qty { get; set; }
        public string Status { get; set; }
    }
}