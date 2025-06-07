using Humica.Core.DB;
using Humica.EF.MD;
using Humica.EF.Repo;
using System.Collections.Generic;
using System.Linq;

namespace Humica.Performance
{
    public class ClsKPITransfer : IClsKPITransfer
    {
        protected IUnitOfWork unitOfWork;
        public SYUser User { get; set; }
        public HRKPITransfer Header { get; set; }
        public List<HRKPITransfer> ListHeader { get; set; }
        public void OnLoad()
        {
            unitOfWork = new UnitOfWork<HumicaDBContext>(new HumicaDBContext());
        }
        public ClsKPITransfer()
        {
            User = SYSession.getSessionUser();
            OnLoad();
        }
        public void OnIndexLoading()
        {
            OnLoad();
            ListHeader = unitOfWork.Set<HRKPITransfer>().ToList();
        }
        public Dictionary<string, dynamic> OnDataSelectorLoading(params object[] keys)
        {
            Dictionary<string, dynamic> keyValues = new Dictionary<string, dynamic>();

            keyValues.Add("KPITASK_LIST", ClsMeasure.LoadDataTrasfer());

            return keyValues;
        }
    }
}
