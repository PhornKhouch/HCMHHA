using Humica.Core.DB;
using Humica.EF.MD;
using System.Collections.Generic;

namespace Humica.Logic.PR
{
    public class PRPayPeriodObject
    {
        public HumicaDBContext DB = new HumicaDBContext();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public string ScreenId { get; set; }
        public string CountryCode { get; set; }
        public string MessageCode { get; set; }
        public bool IsInUse { get; set; }
        public bool IsEditable { get; set; }
        public List<PRpayperiod> ListPayPeriod { get; set; }
        public PRpayperiod Header { get; set; }

        public PRPayPeriodObject()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }
    }
}


