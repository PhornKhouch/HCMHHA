using Humica.Core.DB;
using Humica.Core.FT;
using Humica.EF;
using Humica.EF.MD;
using System.Collections.Generic;

namespace Humica.Performance
{
    public interface IClsKPITracking : IClsApplication
    {
        FTINYear FInYear { get; set; }
        SYUser User { get; set; }
        string ScreenId { get; set; }
        HRKPITracking HeaderKPITracking { get; set; }
        List<HRKPITracking> listKPITracking { get; set; }
        List<ListAssign> ListKPIEmpPending { get; set; }
        List<HRKPITimeSheet> ListTimeSheet { get; set; }
        List<HRKPITracking> OnIndexLoading(string Status, bool ESS = false);
        List<ListAssign> OnIndexLoadingAssign(bool ESS = false);
        void OnCreatingLoading(string ID, string EmpCode);
        void OnDetailLoading(params object[] keys);
        string Create();
        string Update(int id);
        string Delete(params object[] keys);
        string Approved(int id, bool ESS = false);
        string Reject(int id, bool ESS = false);
        string OnGridModify(HRKPITimeSheet MModel, string Action);
        Dictionary<string, dynamic> OnDataSelectorLoading(params object[] keys);
        Dictionary<string, dynamic> OnDataStatusLoading(params object[] keys);
    }
}
