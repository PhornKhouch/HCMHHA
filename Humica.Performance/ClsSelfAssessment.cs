using Humica.Core.DB;
using Humica.Core.SY;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.EF.Repo;
using Humica.Logic.CF;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;

namespace Humica.Performance
{
    public class ClsSelfAssessment : IClsSelfAssessment
    {
        protected IUnitOfWork unitOfWork;
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public string ScreenId { get; set; }
        public string MessageCode { get; set; }
        public HREmpSelfAssessment Header { get; set; }
        public List<HREmpSelfAssessment> ListHeader { get; set; }
        public List<HREmpSelfAssessment> ListAssessmentPending { get; set; }
        public List<HREmpSelfAssessmentItem> ListItem { get; set; }
        public List<HRApprSelfAssessment> ListSelfAssItem { get; set; }

        public ClsSelfAssessment()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
            OnLoad();
        }

        public void OnLoad()
        {
            unitOfWork = new UnitOfWork<HumicaDBViewContext>(new HumicaDBViewContext());
        }
        public void OnIndexLoading(bool IsESS = false)
        {
            if (IsESS)
            {
                string Open = SYDocumentStatus.OPEN.ToString();
                ListHeader = unitOfWork.Set<HREmpSelfAssessment>().Where(w => w.EmpCode == User.UserName
                && w.Status != Open).ToList();
            }
            else
            {
                ListHeader = unitOfWork.Set<HREmpSelfAssessment>().ToList();
            }
        }
        public void OnIndexLoadingPending()
        {
            string pending = SYDocumentStatus.PENDING.ToString();
            string userName = User.UserName;
            ListAssessmentPending = unitOfWork.Set<HREmpSelfAssessment>().Where(x => x.EmpCode == userName && x.Status == pending
            && x.IsRead != true).ToList();
        }
        public void OnCreatingLoading(params object[] keys)
        {
            string AppType = (string)keys[1];
            string EmpCode = (string)keys[0];
            Header = new HREmpSelfAssessment();
            Header.AssessmentDate = DateTime.Now;
            Header.AppraiselType = AppType;
            var Staff = unitOfWork.Set<HR_STAFF_VIEW>().FirstOrDefault(w => w.EmpCode == EmpCode);
            if (Staff != null)
            {
                Header.EmpCode = EmpCode;
                Header.EmpName = Staff.AllName;
                Header.Department = Staff.Department;
                Header.Position = Staff.Position;
            }
            ListItem = new List<HREmpSelfAssessmentItem>();
            ListSelfAssItem = new List<HRApprSelfAssessment>();
            var objAppType = unitOfWork.Set<HRApprType>().FirstOrDefault(w => w.Code == AppType);
            if (objAppType != null)
            {
                ListSelfAssItem = unitOfWork.Set<HRApprSelfAssessment>().Where(w => w.AppraiselType == AppType).ToList();
            }

        }
        public string Create()
        {
            OnLoad();
            try
            {
                var lstAssass = unitOfWork.Set<HRApprSelfAssessment>().Where(w => w.AppraiselType == Header.AppraiselType).ToList();
                var objCF = unitOfWork.Set<ExCfWorkFlowItem>().FirstOrDefault(w => w.ScreenID == ScreenId);
                if (objCF == null)
                {
                    return "REQUEST_TYPE_NE";
                }
                var AppType = unitOfWork.Set<HRApprType>().FirstOrDefault(w => w.Code == Header.AppraiselType);
                if (AppType != null)
                {
                    Header.AppraiselName = AppType.Description;
                }
                var objNumber = new CFNumberRank(objCF.DocType, objCF.ScreenID);
                if (objNumber.NextNumberRank == null)
                {
                    return "NUMBER_RANK_NE";
                }
                Header.AssessmentCode = objNumber.NextNumberRank;

                foreach (var item in lstAssass)
                {
                    var obj = new HREmpSelfAssessmentItem();
                    obj.AssessmentCode = Header.AssessmentCode;
                    obj.QuestionCode = item.QuestionCode;
                    obj.IsQCM = item.IsQCM;
                    obj.Description1 = item.Description1;
                    obj.Description2 = item.Description2;
                    foreach (var read in ListItem.Where(w => w.QuestionCode == item.QuestionCode).ToList())
                    {
                        obj.Comment = read.Comment;
                        obj.CorrectValue = read.CorrectValue;
                    }
                    unitOfWork.Add(obj);
                }
                Header.Status = SYDocumentStatus.OPEN.ToString();
                Header.CreatedBy = User.UserName;
                Header.CreatedOn = DateTime.Now;
                unitOfWork.Add(Header);
                unitOfWork.Save();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, Header.EmpCode, SYActionBehavior.ADD.ToString(), e, true);
            }
            catch (DbUpdateException e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, Header.EmpCode, SYActionBehavior.ADD.ToString(), e, true);
            }
        }
        public virtual string OnEditLoading(params object[] keys)
        {
            string AppType = (string)keys[0];
            Header = unitOfWork.Set<HREmpSelfAssessment>().FirstOrDefault(w => w.AssessmentCode == AppType);
            ListItem = new List<HREmpSelfAssessmentItem>();
            if (Header != null)
            {
                ListSelfAssItem = unitOfWork.Set<HRApprSelfAssessment>().Where(w => w.AppraiselType == Header.AppraiselType).ToList();
                ListItem = unitOfWork.Set<HREmpSelfAssessmentItem>().Where(w => w.AssessmentCode == AppType).ToList();
            }
            return SYConstant.OK;
        }
        public string Update(string id, bool IsESS = false)
        {
            OnLoad();
            try
            {
                var objMatch = unitOfWork.Set<HREmpSelfAssessment>().FirstOrDefault(w => w.AssessmentCode == id);
                if (objMatch == null)
                {
                    return "DOC_INV";
                }
                var lstAssass = unitOfWork.Set<HRApprSelfAssessment>().Where(w => w.AppraiselType == objMatch.AppraiselType).ToList();
                var ObjMatchItem = unitOfWork.Set<HREmpSelfAssessmentItem>().Where(w => w.AssessmentCode == objMatch.AssessmentCode).ToList();
                foreach (var read in ObjMatchItem)
                {
                    unitOfWork.Delete(read);
                }

                foreach (var item in lstAssass)
                {
                    var obj = new HREmpSelfAssessmentItem();
                    obj.AssessmentCode = Header.AssessmentCode;
                    obj.QuestionCode = item.QuestionCode;
                    obj.IsQCM = item.IsQCM;
                    obj.Description1 = item.Description1;
                    obj.Description2 = item.Description2;
                    foreach (var read in ListItem.Where(w => w.QuestionCode == item.QuestionCode).ToList())
                    {
                        obj.Comment = read.Comment;
                        obj.CorrectValue = read.CorrectValue;
                    }
                    unitOfWork.Add(obj);
                }
                objMatch.ChangedBy = User.UserName;
                objMatch.ChangedOn = DateTime.Now;
                if (IsESS)
                {
                    objMatch.IsRead = true;
                }
                unitOfWork.Update(objMatch);
                unitOfWork.Save();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, Header.EmpCode, SYActionBehavior.ADD.ToString(), e, true);
            }
            catch (DbUpdateException e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, Header.EmpCode, SYActionBehavior.ADD.ToString(), e, true);
            }
        }
        public string Delete(string id)
        {
            OnLoad();
            try
            {
                var objMatch = unitOfWork.Set<HREmpSelfAssessment>().FirstOrDefault(w => w.AssessmentCode == id);
                if (objMatch == null)
                {
                    return "DOC_EN";
                }
                if (objMatch.Status != SYDocumentStatus.OPEN.ToString())
                {
                    return "DELETE_CAN_N";
                }
                var ObjMatchItem = unitOfWork.Set<HREmpSelfAssessmentItem>().Where(w => w.AssessmentCode == objMatch.AssessmentCode).ToList();
                foreach (var read in ObjMatchItem)
                {
                    unitOfWork.Delete(read);
                }
                unitOfWork.Delete(objMatch);
                unitOfWork.Save();
                return SYConstant.OK;
            }
            catch (Exception e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, Header.EmpCode, SYActionBehavior.ADD.ToString(), e, true);
            }
        }
        public string RequestToApprove(string id)
        {
            try
            {
                var objMatch = unitOfWork.Set<HREmpSelfAssessment>().FirstOrDefault(w => w.AssessmentCode == id);
                if (objMatch == null)
                {
                    return "REQUEST_NE";
                }
                Header = objMatch;
                if (objMatch.Status != SYDocumentStatus.OPEN.ToString())
                {
                    return "INV_DOC";
                }

                objMatch.Status = SYDocumentStatus.PENDING.ToString();
                unitOfWork.Update(objMatch);
                unitOfWork.Save();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                ClsEventLog.SaveEventLogs(ScreenId, User.UserName, id, SYActionBehavior.ADD.ToString(), e, true);
                return "EE001";
            }
            catch (DbUpdateException e)
            {
                ClsEventLog.SaveEventLogs(ScreenId, User.UserName, id, SYActionBehavior.ADD.ToString(), e, true);
                return "EE001";
            }
            catch (Exception e)
            {
                ClsEventLog.SaveEventLogs(ScreenId, User.UserName, id, SYActionBehavior.ADD.ToString(), e, true);
                return "EE001";
            }
        }
        public string CancelDoc(string id)
        {
            try
            {
                var objMatch = unitOfWork.Set<HREmpSelfAssessment>().FirstOrDefault(w => w.AssessmentCode == id);
                if (objMatch == null)
                {
                    return "REQUEST_NE";
                }
                Header = objMatch;

                objMatch.Status = SYDocumentStatus.CANCELLED.ToString();

                unitOfWork.Update(objMatch);
                unitOfWork.Save();

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                ClsEventLog.SaveEventLogs(ScreenId, User.UserName, id, SYActionBehavior.ADD.ToString(), e, true);
                return "EE001";
            }
            catch (DbUpdateException e)
            {
                ClsEventLog.SaveEventLogs(ScreenId, User.UserName, id, SYActionBehavior.ADD.ToString(), e, true);
                return "EE001";
            }
            catch (Exception e)
            {
                ClsEventLog.SaveEventLogs(ScreenId, User.UserName, id, SYActionBehavior.ADD.ToString(), e, true);
                return "EE001";
            }
        }
        public string ApproveDoc(string id)
        {
            try
            {
                var objMatch = unitOfWork.Set<HREmpSelfAssessment>().FirstOrDefault(w => w.AssessmentCode == id);
                if (objMatch == null) return "DOC_NE";
                Header = objMatch;
                if (objMatch.Status != SYDocumentStatus.PENDING.ToString())
                {
                    return "INV_DOC";
                }

                objMatch.Status = SYDocumentStatus.APPROVED.ToString();

                unitOfWork.Update(objMatch);
                unitOfWork.Save();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, id, SYActionBehavior.ADD.ToString(), e, true);
            }
            catch (DbUpdateException e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, id, SYActionBehavior.ADD.ToString(), e, true);
            }
            catch (Exception e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, id, SYActionBehavior.ADD.ToString(), e, true);
            }
        }
        public HR_STAFF_VIEW GetStaffFilter(string id)
        {
            HR_STAFF_VIEW Staff = unitOfWork.Set<HR_STAFF_VIEW>().Where(w => w.EmpCode == id).FirstOrDefault();
            if (Staff == null)
            {
                Staff = new HR_STAFF_VIEW();
            }
            return Staff;
        }
        public Dictionary<string, dynamic> OnDataSelectorLoading(params object[] keys)
        {
            Dictionary<string, dynamic> keyValues = new Dictionary<string, dynamic>();

            keyValues.Add("STAFF_SELECT", unitOfWork.Set<HR_STAFF_VIEW>().Where(w => w.StatusCode == SYDocumentStatus.A.ToString()).ToList());
            keyValues.Add("APPRTYPE_SELECT", unitOfWork.Set<HRApprType>().ToList());

            return keyValues;
        }
    }

}