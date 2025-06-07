using Humica.Core.CF;
using Humica.Core.DB;
using Humica.Core.Helper;
using Humica.Core.SY;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.EF.Repo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Humica.Performance
{
    public class ClsKPIEvaluation : IClsKPIEvaluation
    {
        protected IUnitOfWork unitOfWork;
        public string ScreenId { get; set; }
        public SYUser User { get; set; }
        public HRKPIEvaluation Header { get; set; }
        public List<HRKPIEvaluation> ListHeander { get; set; }
        public List<HRKPIEvaItem> ListItem { get; set; }
        public List<HRKPIAssignHeader> ListKPIPending { get; set; }
        public List<HRKPIIndicator> ListKPIIndicator { get; set; }
        public void OnLoad()
        {
            unitOfWork = new UnitOfWork<HumicaDBViewContext>(new HumicaDBViewContext());
        }
        public ClsKPIEvaluation()
        {
            User = SYSession.getSessionUser();
            OnLoad();
        }
        public void OnIndexLoading(bool IsESS = false)
        {
            string Release = SYDocumentStatus.RELEASED.ToString();
            if (IsESS)
            {
                string UserName = User.UserName;
                DateTime DateNow = DateTime.Now.Date;
                ListHeander = unitOfWork.Set<HRKPIEvaluation>().Where(w => w.DirectedByCode == UserName).ToList();
                ListKPIPending = unitOfWork.Set<HRKPIAssignHeader>().Where(w => w.ReStatus == Release
                && w.DirectedByCode == UserName).ToList();
                // && w.ExpectedDate < DateNow
            }
            else
            {
                ListHeander = unitOfWork.Set<HRKPIEvaluation>().ToList();
                ListKPIPending = unitOfWork.Set<HRKPIAssignHeader>().Where(w => w.Status == Release).ToList();
            }
        }
        public virtual void OnCreatingLoading(string ID)
        {
            Header = new HRKPIEvaluation();
            ListItem = new List<HRKPIEvaItem>();
            var KPIHeader = unitOfWork.Set<HRKPIAssignHeader>().FirstOrDefault(w => w.AssignCode == ID);
            if (KPIHeader != null)
            {
                var EmpStaff = unitOfWork.Set<HR_STAFF_VIEW>().FirstOrDefault(w => w.EmpCode == KPIHeader.DirectedByCode);
                Header.HandleCode = KPIHeader.HandleCode;
                Header.HandleName = KPIHeader.HandleName;
                Header.Department = KPIHeader.Department;
                Header.Position = KPIHeader.Position;
                Header.KPIAverage = KPIHeader.KPIAverage;
                Header.TotalWeight = KPIHeader.TotalWeight;
                Header.TotalScore = KPIHeader.TotalScore;
                Header.DocumentDate = DateTime.Now;
                Header.Status = SYDocumentStatus.OPEN.ToString();
                Header.KPIType = KPIHeader.KPIType;
                Header.PeriodFrom = KPIHeader.PeriodFrom;
                Header.PeriodTo = KPIHeader.PeriodTo;
                Header.DirectedByCode = KPIHeader.DirectedByCode;
                Header.DirectedByName = KPIHeader.DirectedByName;
                if (EmpStaff != null)
                {
                    Header.DirectedPosition = EmpStaff.Position;
                }
                Header.TotalScoreEval = 0;
            }
            var ListKPIItem = unitOfWork.Set<HRKPIAssignItem>().Where(w => w.AssignCode == ID).ToList();
            foreach (var item in ListKPIItem)
            {
                string Status = Get_Status(item.Acheivement.Value);
                string Color = Get_Color(item.Acheivement.Value);
                decimal Variance = item.Actual.Value - item.Target.Value;
                decimal TargetPer = item.Acheivement.Value - 1;
                if (item.Symbol == "<")
                {
                    if (item.Actual.Value > item.Target.Value)
                    {
                        Variance = item.Actual.Value - item.Target.Value;
                        TargetPer = item.Acheivement.Value - 1;
                    }
                    else
                    {
                        Variance = Math.Abs(item.Actual.Value - item.Target.Value);
                        TargetPer = Math.Abs(item.Acheivement.Value - 1);
                    }
                }
                ListItem.Add(new HRKPIEvaItem
                {
                    Indicator = item.Indicator,
                    ItemCode = item.ItemCode,
                    KPI = item.KPI,
                    Target = item.Target,
                    Actual = item.Actual,
                    Weight = item.Weight,
                    Score = item.Score,
                    Variance = Variance,
                    TargetPer = TargetPer,
                    SubScore = item.Score / 2.00M,
                    Status = Status,
                    Symbol = item.Symbol,
                    Color = Color
                });
            }
            var indicatorCodes = new HashSet<string>(ListItem.GroupBy(w => w.Indicator).Select(x => x.Key));
            ListKPIIndicator = unitOfWork.Set<HRKPIIndicator>().Where(w => indicatorCodes.Contains(w.Code)).ToList();
        }
        public string Create(string Screen_ID)
        {
            OnLoad();
            try
            {
                var EmpStaff = unitOfWork.Set<HR_STAFF_VIEW>().FirstOrDefault(w => w.EmpCode == Header.HandleCode);
                var objCF = unitOfWork.Set<ExCfWorkFlowItem>().FirstOrDefault(w => w.ScreenID == Screen_ID);
                if (objCF == null)
                {
                    return "REQUEST_TYPE_NE";
                }
                var objNumber = new CFNumberRank(objCF.NumberRank, EmpStaff.CompanyCode, Header.DocumentDate.Value.Year, true);
                if (objNumber.NextNumberRank == null)
                {
                    return "NUMBER_RANK_NE";
                }
                var KPIHeader = unitOfWork.Set<HRKPIAssignHeader>().FirstOrDefault(w => w.AssignCode == Header.DocRef);
                if (KPIHeader == null)
                {
                    return "INV_DOC";
                }
                KPIHeader.Status = SYDocumentStatus.COMPLETED.ToString();
                KPIHeader.ReStatus = SYDocumentStatus.COMPLETED.ToString();
                unitOfWork.Update(KPIHeader);

                Header.KPIEvaCode = objNumber.NextNumberRank;
                if (EmpStaff != null)
                {
                    Header.Position = EmpStaff.Position;
                    Header.HandleName = EmpStaff.AllName;
                    Header.Department = EmpStaff.Department;
                }
                decimal TotalScoreEval = 0;
                foreach (var item in ListItem)
                {
                    var obj = new HRKPIEvaItem();
                    obj.KPIEvaCode = Header.KPIEvaCode;
                    obj.Indicator = item.Indicator;
                    obj.ItemCode = item.ItemCode;
                    obj.KPI = item.KPI;
                    obj.Target = item.Target;
                    obj.Weight = item.Weight;
                    obj.Score = item.Score;
                    obj.Actual = item.Actual;
                    obj.Symbol = item.Symbol;
                    obj.TargetPer = ClsRounding.Rounding(item.TargetPer, 4, "N");
                    obj.SubScore = item.SubScore;
                    obj.ScoreEval = item.ScoreEval;
                    obj.Status = item.Status;
                    obj.Variance = item.Variance;
                    obj.Color = item.Color;
                    unitOfWork.Add(obj);
                    if (item.ScoreEval.HasValue)
                    {
                        TotalScoreEval += item.ScoreEval.Value;
                    }
                }
                var KPIIndicator = unitOfWork.Set<HRKPIAssignIndicator>().Where(w => w.AssignCode == KPIHeader.AssignCode).ToList();
                foreach (var item in KPIIndicator)
                {
                    var obj = new HRKPIEvalIndicator();
                    NewIndicator(obj, item);
                    obj.KPIEvaCode = Header.KPIEvaCode;
                    var ScoreEval = ListItem.Where(w => w.Indicator == item.Indicator).Sum(x => x.SubScore);
                    if (ScoreEval > 0)
                    {
                        obj.Score = ScoreEval;
                        unitOfWork.Add(obj);
                    }
                }               

                Header.TotalScoreEval = TotalScoreEval;
                Header.FinalResult = Calculate_Score(Header);
                Header.FinalResultPA = Calculate_ScorePA(Header);
                Header.CreatedBy = User.UserName;
                Header.CreatedOn = DateTime.Now;
                unitOfWork.Add(Header);
                unitOfWork.Save();
                return SYConstant.OK;
            }
            catch (Exception e)
            {
                return ClsEventLog.Save_EventLogs(ScreenId, User.UserName, "", SYActionBehavior.ADD.ToString(), e, true);
            }
        }

        public virtual void OnDetailLoading(params object[] keys)
        {
            string KPIEvaCode = (string)keys[0];
            Header = unitOfWork.Set<HRKPIEvaluation>().FirstOrDefault(w => w.KPIEvaCode == KPIEvaCode);
            ListItem = new List<HRKPIEvaItem>();
            if (Header != null)
            {
                ListItem = unitOfWork.Set<HRKPIEvaItem>().Where(w => w.KPIEvaCode == KPIEvaCode).ToList();
            }
            var indicatorCodes = new HashSet<string>(ListItem.GroupBy(w => w.Indicator).Select(x => x.Key));
            ListKPIIndicator = unitOfWork.Set<HRKPIIndicator>().Where(w => indicatorCodes.Contains(w.Code)).ToList();
        }
        public string Update(string ID, bool IS_ESS = false)
        {
            OnLoad();
            try
            {
                decimal TotalScoreEval = 0;
                var objMatch = unitOfWork.Set<HRKPIEvaluation>().FirstOrDefault(w => w.KPIEvaCode == ID);
                if (objMatch == null)
                {
                    return "DOC_INV";
                }
                var objMatchItem = unitOfWork.Set<HRKPIEvaItem>().Where(w => w.KPIEvaCode == ID);
                foreach (var item in objMatchItem)
                {
                    var obj = ListItem.FirstOrDefault(w => w.ItemCode == item.ItemCode);
                    if (obj != null)
                    {
                        item.ScoreEval = obj.ScoreEval;
                        item.SubScore = obj.SubScore;
                        unitOfWork.Update(item);
                        if (item.ScoreEval.HasValue)
                        {
                            TotalScoreEval += item.ScoreEval.Value;
                        }
                    }
                }
                var KPIIndicator = unitOfWork.Set<HRKPIEvalIndicator>().Where(w => w.KPIEvaCode == ID).ToList();
                foreach (var item in KPIIndicator)
                {
                    var ScoreEval = ListItem.Where(w => w.Indicator == item.Indicator).Sum(x => x.SubScore);
                    if (ScoreEval > 0)
                    {
                        item.Score = ScoreEval;
                        unitOfWork.Update(item);
                    }
                }

                objMatch.TotalScoreEval = TotalScoreEval;
                objMatch.FinalResult = Calculate_Score(objMatch);
                objMatch.FinalResultPA = Calculate_ScorePA(objMatch);
                objMatch.ChangedOn = DateTime.Now;
                objMatch.ChangedBy = User.UserName;
                unitOfWork.Update(objMatch);
                unitOfWork.Save();
                return SYConstant.OK;
            }
            catch (Exception e)
            {
                return ClsEventLog.Save_EventLogs(ScreenId, User.UserName, ID, SYActionBehavior.ADD.ToString(), e, true);
            }
        }
        public string ApproveDoc(string id)
        {
            try
            {
                var objMatch = unitOfWork.Set<HRKPIEvaluation>().FirstOrDefault(w => w.KPIEvaCode == id);
                if (objMatch == null) return "DOC_NE";
                Header = objMatch;
                if (objMatch.Status != SYDocumentStatus.OPEN.ToString())
                {
                    return "INV_DOC";
                }
                objMatch.Status = SYDocumentStatus.APPROVED.ToString();
                unitOfWork.Update(objMatch);
                unitOfWork.Save();
                return SYConstant.OK;
            }
            catch (Exception e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, id, SYActionBehavior.ADD.ToString(), e, true);
            }
        }
        public decimal Calculate_ScorePA(HRKPIEvaluation Eval)
        {
            decimal Final_score = 0;
            Final_score = ((Eval.TotalScoreEval.Value + Eval.TotalScore.Value) / 2.00M) * Eval.KPIAverage.Value;
            return Final_score;
        }
        public decimal Calculate_Score(HRKPIEvaluation Eval)
        {
            decimal Final_score = 0;
            Final_score = ((Eval.TotalScoreEval.Value + Eval.TotalScore.Value) / 2.00M);
            return Final_score;
        }
        public string Get_Color(decimal Achievement)
        {
            string Color = "Red";
            if (Achievement < 0.5M)
            {
                Color = "Red";
            }
            else if (Achievement >= 0.5M && Achievement < 0.6M)
            {
                Color = "Yellow";
            }
            else if (Achievement >= 0.6M && Achievement < 0.9M)
            {
                Color = "Orange";
            }
            else if (Achievement >= 0.9M && Achievement <= 1.1M)
            {
                Color = "Green";
            }
            else if (Achievement > 1.1M)
            {
                Color = "Blue";
            }
            return Color;
        }
        public string Get_Status(decimal Achievement)
        {
            string Status = "Critical";
            if (Achievement < 0.5M)
            {
                Status = "Critical";
            }
            else if (Achievement >= 0.5M && Achievement < 0.6M)
            {
                Status = "Warning";
            }
            else if (Achievement >= 0.6M && Achievement < 0.9M)
            {
                Status = "Acceptable";
            }
            else if (Achievement >= 0.9M && Achievement <= 1.1M)
            {
                Status = "Good";
            }
            else if (Achievement > 1.1M)
            {
                Status = "Excellent";
            }
            return Status;
        }
        public void NewIndicator(HRKPIEvalIndicator D, HRKPIAssignIndicator S)
        {
            D.Indicator = S.Indicator;
            D.Weight = S.Weight;
            D.Acheivement = S.Acheivement;
            D.IndicatorType = S.IndicatorType;
        }
    }
}
