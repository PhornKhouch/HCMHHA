using Humica.Core.DB;
using Humica.EF.MD;
using System.Collections.Generic;

namespace Humica.Logic.HRS
{
    public class MDEmpInfor
    {
        public HumicaDBContext DB = new HumicaDBContext();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        //public HRBranch Header { get; set; }
        public string ScreenId { get; set; }
        public string Code { get; set; }
        public string MessageCode { get; set; }
        public bool IsInUse { get; set; }
        public bool IsEditable { get; set; }
        public List<HRCountry> ListCountry { get; set; }
        public List<HRNation> ListNation { get; set; }
        public List<HRProvice> ListProvine { get; set; }
        //public List<HRBank> ListBank { get; set; }
        //  public List<HRBankBranch> ListBankBranch { get; set; }
        public List<HRCareerHistory> ListCareerHistory { get; set; }
        public List<HRRelationshipType> ListRelationshipType { get; set; }
        public List<HREduType> ListEduType { get; set; }
        public List<HRCertificationType> ListCertificationType { get; set; }
        public List<HRContractType> ListContractType { get; set; }
        public List<HRWorkingType> ListWorkingType { get; set; }
        public List<HRDisciplinAction> ListDisciplinAction { get; set; }
        public List<HRDisciplinType> ListDisciplinType { get; set; }
        public List<HRBloodType> ListBloodType { get; set; }
        public List<HRTerminType> ListSeparateType { get; set; }
        public List<HRInsuranceCompany> ListInsuranceCompany { get; set; }
        public List<HRAnnounceType> ListAnnounementType { get; set; }
        public MDEmpInfor()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }
    }
}