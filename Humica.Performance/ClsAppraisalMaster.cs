using Humica.Core.DB;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.EF.Repo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Humica.Performance
{
    public class ClsAppraisalMaster : IClsAppraisalMaster
    {
        protected IUnitOfWork unitOfWork;
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public List<HRApprType> ListApprType { get; set; }
        public List<HRApprRegion> ListApprRegion { get; set; }
        public List<HRApprFactor> ListApprFactor { get; set; }
        public List<HRAppGrade> ListApprResult { get; set; }
        public List<HRApprRating> ListApprRating { get; set; }
        public List<HRApprSelfAssessment> ListSelfAssessment { get; set; }
        public List<HRAppLevelMidPoint> ListLeveMidPoint { get; set; }
        public List<HRAppPerformanceRate> ListPerformanceRate { get; set; }
        public List<HRAppSalaryBudget> ListSalaryBudget { get; set; }
        public List<HRAppCompareRatio> ListCompareRatio { get; set; }
        public ClsAppraisalMaster()
        {
            OnLoad();
        }
        public void OnLoad()
        {
            unitOfWork = new UnitOfWork<HumicaDBContext>();
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }
        public void OnIndexLoading()
        {
            ListApprType = unitOfWork.Set<HRApprType>().ToList();
            ListApprRegion = unitOfWork.Set<HRApprRegion>().ToList();
            ListApprFactor = unitOfWork.Set<HRApprFactor>().ToList();
            ListApprResult = unitOfWork.Set<HRAppGrade>().OrderBy(w => w.Grade).ToList();
            ListApprRating = unitOfWork.Set<HRApprRating>().ToList();
            ListSelfAssessment = unitOfWork.Set<HRApprSelfAssessment>().ToList();
            ListCompareRatio = unitOfWork.Set<HRAppCompareRatio>().ToList();
            ListLeveMidPoint = unitOfWork.Set<HRAppLevelMidPoint>().ToList();
            ListPerformanceRate = unitOfWork.Set<HRAppPerformanceRate>().ToList();
            ListSalaryBudget = unitOfWork.Set<HRAppSalaryBudget>().ToList();
        }
        #region Region
        public void OnIndexLoadingRegion()
        {
            ListApprRegion = unitOfWork.Set<HRApprRegion>().ToList();
        }
        public string OnGridModifyRegion(HRApprRegion MModel, string Action)
        {
            try
            {
                if (string.IsNullOrEmpty(MModel.Code))
                {
                    return "CODE";
                }
                if (!MModel.InOrder.HasValue)
                {
                    MModel.InOrder = 0;
                }
                if (Action == "ADD")
                {
                    MModel.Code = MModel.Code.Trim();
                    unitOfWork.Add(MModel);
                }
                else if (Action == "EDIT")
                {
                    var existingEntity = unitOfWork.Set<HRApprRegion>()
                .FirstOrDefault(w => w.Code == MModel.Code && w.AppraiselType == MModel.AppraiselType);

                    if (existingEntity != null)
                    {
                        existingEntity.Description = MModel.Description;
                        existingEntity.SecDescription = MModel.SecDescription;
                        existingEntity.Remark = MModel.Remark;
                        existingEntity.InOrder = MModel.InOrder;
                        existingEntity.IsKPI = MModel.IsKPI;
                        existingEntity.Rating = MModel.Rating;

                        unitOfWork.Update(existingEntity);
                    }
                }
                else if (Action == "DELETE")
                {
                    var objCheck = unitOfWork.Set<HRApprRegion>().FirstOrDefault(w => w.Code == MModel.Code
                    && w.AppraiselType == MModel.AppraiselType);
                    if (objCheck != null)
                    {
                        unitOfWork.Delete(objCheck);
                    }
                    else
                    {
                        return "INV_DOC";
                    }
                }

                unitOfWork.Save();
                return SYConstant.OK;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        #endregion

        #region Factor
        public void OnIndexLoadingFactor()
        {
            ListApprFactor = unitOfWork.Set<HRApprFactor>().ToList();
        }
        public string OnGridModifyFactor(HRApprFactor MModel, string Action)
        {
            try
            {
                if (string.IsNullOrEmpty(MModel.Code))
                {
                    return "CODE";
                }
                if (Action == "ADD")
                {
                    MModel.Code = MModel.Code.Trim();
                    if (string.IsNullOrEmpty(MModel.Region))
                    {
                        return "INV_REGION";
                    }
                    unitOfWork.Add(MModel);
                }
                else if (Action == "EDIT")
                {
                    unitOfWork.Update(MModel);
                }
                else if (Action == "DELETE")
                {
                    var objCheck = unitOfWork.Set<HRApprFactor>().FirstOrDefault(w => w.Code == MModel.Code
                    && w.AppraiselType == MModel.AppraiselType);
                    if (objCheck != null)
                    {
                        unitOfWork.Delete(objCheck);
                    }
                    else
                    {
                        return "INV_DOC";
                    }
                }

                unitOfWork.Save();
                return SYConstant.OK;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        #endregion

        public void OnIndexLoadingSelfAssessment()
        {
            ListSelfAssessment = unitOfWork.Set<HRApprSelfAssessment>().ToList();
        }
        public string OnGridModifySelfAssessment(HRApprSelfAssessment MModel, string Action)
        {
            try
            {
                if (string.IsNullOrEmpty(MModel.QuestionCode))
                {
                    return "CODE";
                }
                if (Action == "ADD")
                {
                    MModel.QuestionCode = MModel.QuestionCode.Trim();
                    unitOfWork.Add(MModel);
                }
                else if (Action == "EDIT")
                {
                    unitOfWork.Update(MModel);
                }
                else if (Action == "DELETE")
                {
                    var objCheck = unitOfWork.Set<HRApprSelfAssessment>().FirstOrDefault(w => w.QuestionCode == MModel.QuestionCode
                    && w.AppraiselType == MModel.AppraiselType);
                    if (objCheck != null)
                    {
                        unitOfWork.Delete(objCheck);
                    }
                    else
                    {
                        return "INV_DOC";
                    }
                }

                unitOfWork.Save();
                return SYConstant.OK;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        public Dictionary<string, dynamic> OnDataSelectorLoading()
        {
            Dictionary<string, dynamic> keyValues = new Dictionary<string, dynamic>();
            keyValues.Add("REGION_SELECT", unitOfWork.Set<HRApprRegion>().Where(w => w.IsKPI != true).ToList().OrderBy(w => w.Description));
            keyValues.Add("AppraiselType_SELECT", unitOfWork.Set<HRApprType>().ToList());
            keyValues.Add("Level_SELECT", unitOfWork.Set<HRLevel>().ToList());
            return keyValues;
        }
    }
}
