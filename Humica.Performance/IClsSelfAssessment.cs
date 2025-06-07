using Humica.Core.DB;
using Humica.EF;
using System.Collections.Generic;

namespace Humica.Performance
{
    public interface IClsSelfAssessment : IClsApplication
    {
        List<HREmpSelfAssessment> ListHeader { get; set; }
        HREmpSelfAssessment Header { get; set; }
        List<HREmpSelfAssessmentItem> ListItem { get; set; }
        List<HRApprSelfAssessment> ListSelfAssItem { get; set; }
        string ScreenId { get; set; }
        List<HREmpSelfAssessment> ListAssessmentPending { get; set; }

        string ApproveDoc(string id);
        string CancelDoc(string id);
        string Create();
        string Delete(string id);
        HR_STAFF_VIEW GetStaffFilter(string id);
        void OnCreatingLoading(params object[] keys);
        Dictionary<string, dynamic> OnDataSelectorLoading(params object[] keys);
        string OnEditLoading(params object[] keys);
        void OnIndexLoading(bool IsESS = false);
        void OnIndexLoadingPending();
        string RequestToApprove(string id);
        string Update(string id, bool IsESS = false);
    }
}
