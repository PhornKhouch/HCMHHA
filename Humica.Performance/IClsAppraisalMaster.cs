using Humica.Core.DB;
using Humica.EF;
using System.Collections.Generic;

namespace Humica.Performance
{
    public interface IClsAppraisalMaster : IClsApplication
    {
        List<HRApprType> ListApprType { get; set; }
        List<HRApprRegion> ListApprRegion { get; set; }
        List<HRApprFactor> ListApprFactor { get; set; }
        List<HRAppGrade> ListApprResult { get; set; }
        List<HRApprRating> ListApprRating { get; set; }
        List<HRApprSelfAssessment> ListSelfAssessment { get; set; }
        List<HRAppLevelMidPoint> ListLeveMidPoint { get; set; }
        List<HRAppPerformanceRate> ListPerformanceRate { get; set; }
        List<HRAppSalaryBudget> ListSalaryBudget { get; set; }
        List<HRAppCompareRatio> ListCompareRatio { get; set; }

        Dictionary<string, dynamic> OnDataSelectorLoading();
        string OnGridModifyFactor(HRApprFactor MModel, string Action);
        string OnGridModifyRegion(HRApprRegion MModel, string Action);
        string OnGridModifySelfAssessment(HRApprSelfAssessment MModel, string Action);
        void OnIndexLoading();
        void OnIndexLoadingFactor();
        void OnIndexLoadingRegion();
        void OnIndexLoadingSelfAssessment();
    }
}
