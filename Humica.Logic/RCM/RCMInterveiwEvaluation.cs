using Humica.Core.DB;
using Humica.EF.MD;
using System.Collections.Generic;

namespace Humica.Logic.RCM
{
    public class RCMInterveiwEvaluation
    {
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public string ScreenId { get; set; }
        public string Code { get; set; }
        public string MessageCode { get; set; }
        public bool IsInUse { get; set; }
        public bool IsEditable { get; set; }
        public List<RCMInterveiwRegion> ListInterveiwRegion { get; set; }
        public List<RCMInterveiwFactor> ListInterveiwFactor { get; set; }
        public List<RCMInterveiwRating> ListInterveiwRating { get; set; }
        public RCMInterveiwEvaluation()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }
    }
}