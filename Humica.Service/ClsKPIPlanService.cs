using Humica.Calculate;
using Humica.Core;
using Humica.Core.DB;
using Humica.Core.Helper;
using Humica.EF;
using Humica.EF.Repo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace Humica.Service
{
    public class ClsKPIPlanService : BaseBackgroundService
    {
        private IUnitOfWork unitOfWork;
        public void OnLoad()
        {
            unitOfWork = new UnitOfWork<HumicaDBContext>(new HumicaDBContext());
        }
        public ClsKPIPlanService()
        {
            JobName = "SenKPIPlanJob";
            TriggerName = "SendKPIPlanTrigger";
            JobGroup = "SendKPIPlanGroup";
            CronSchedule = "0 0/5 * * * ?"; // create a trigger that fires every 5 minutes, at 10 seconds after the minute
            Job = GetType();
        }

        public override void DoService()
        {
            try
            {
                OnLoad();
                AutoUpdate();
                string Status = SYDocumentStatus.RELEASED.ToString();
                var ListKPI = unitOfWork.Set<HRKPIAssignHeader>().Where(w => w.Status == Status).ToList();
                foreach (var id in ListKPI)
                {
                    if (id.HandleCode == "TELA100093")
                    {

                    }
                    CalculateKPI(id.AssignCode);
                }
                CalculateKPI_Dept();
                //string PENDING = SYDocumentStatus.PENDING.ToString();
                //var ListAppraisal = unitOfWork.Set<HREmpAppraisal>().Where(w => w.Status == PENDING).ToList();
                //foreach (var id in ListAppraisal)
                //{
                //    if (id.EmpCode == "TELA100096")
                //    {

                //    }
                //    CalculateAppraisal(id.ApprID);
                //}
            }
            catch (Exception ex)
            {
                //logger.LogError(ex, "Error executing DoService");
            }
        }
        public void CalculateKPI_Dept()
        {
            OnLoad();
            try
            {
                string RELEASED = SYDocumentStatus.RELEASED.ToString();
                string Closed = SYDocumentStatus.CLOSED.ToString();
                //var ListPlan = unitOfWork.Set<HRKPIPlanHeader>().Where(w => w.KPICategory == Category && w.CriteriaType == CriteriaType
                //&& w.KPIType == KPIType).ToList();

                var ListPlan = unitOfWork.Set<HRKPIPlanHeader>().Where(w => w.Status == RELEASED || w.Status == Closed).ToList();

                var ListNorm = unitOfWork.Set<HRKPINorm>().ToList();
                //var TempEmpKPI = (from AssignHeader in unitOfWork.Set<HRKPIAssignHeader>()
                //                  join Ind in unitOfWork.Set<HRKPIAssignIndicator>()
                //                  on AssignHeader.AssignCode equals Ind.AssignCode
                //                  where AssignHeader.Status == Status && AssignHeader.KPICategory == Category
                //                  && AssignHeader.CriteriaType == CriteriaType && AssignHeader.KPIType == KPIType
                //                  select Ind
                //                      ).ToList();
                foreach (var Plan in ListPlan)
                {
                    List<HREmpAppraisal> ListStaff = new List<HREmpAppraisal>();
                    string PENDING = SYDocumentStatus.PENDING.ToString();
                    if (Plan.KPICategory == "DPM")
                    {
                        ListStaff = (from _App in unitOfWork.Set<HREmpAppraisal>()
                                     join Staff in unitOfWork.Set<HRStaffProfile>()
                                     on _App.EmpCode equals Staff.EmpCode
                                     where _App.KPICategory == Plan.KPICategory
                                     && Staff.EmpCode == _App.EmpCode
                                     && Staff.DEPT == Plan.CriteriaType
                                     select _App
                                    ).ToList();
                    }
                    else if (Plan.KPICategory == "BSN")
                    {
                        ListStaff = (from _App in unitOfWork.Set<HREmpAppraisal>()
                                     join Staff in unitOfWork.Set<HRStaffProfile>()
                                     on _App.EmpCode equals Staff.EmpCode
                                     where _App.KPICategory == Plan.KPICategory
                                     && Staff.EmpCode == _App.EmpCode
                                     && Staff.GroupDept == Plan.CriteriaType
                                     select _App
                                    ).ToList();
                    }
                    var TempEmpKPIBYTeam = unitOfWork.Set<HRKPIAssignHeader>().Where(w => (w.Status == RELEASED || w.Status == Closed) && w.KPICategory == Plan.KPICategory
                                      && w.CriteriaType == Plan.CriteriaType
                                      ).ToList();
                    var TempKPIBYTeam = (from AssignHeader in TempEmpKPIBYTeam
                                         join Member in unitOfWork.Set<HRKPIAssignMember>()
                                          on AssignHeader.AssignCode equals Member.AssignCode
                                         where AssignHeader.AssignedBy == "BYTEAM"
                                         select Member).ToList();
                    Plan.TotalEmp = ListStaff.Where(w => w.Status != Closed).Count();
                    Plan.TotalByIndividual = TempEmpKPIBYTeam.Where(w => w.AssignedBy != "BYTEAM" && w.Status != Closed).Count();
                    Plan.TotalEmpTeam = TempKPIBYTeam.Count;

                    var TempEmpKPI = (from AssignHeader in unitOfWork.Set<HRKPIAssignHeader>()
                                      join Ind in unitOfWork.Set<HRKPIAssignIndicator>()
                                      on AssignHeader.AssignCode equals Ind.AssignCode
                                      where (AssignHeader.Status == RELEASED || AssignHeader.Status == Closed) && AssignHeader.KPICategory == Plan.KPICategory
                                      && AssignHeader.CriteriaType == Plan.CriteriaType
                                      //&& AssignHeader.KPIType == Plan.KPIType
                                      select Ind
                                     ).ToList();

                    var ListPlanitem = unitOfWork.Set<HRKPIPlanItem>().Where(w => w.KPIPlanCode == Plan.Code).ToList();
                    foreach (var item in ListPlanitem)
                    {
                        item.Actual = 0;
                        item.SubTotalEmp = 0;
                        var Actual = TempEmpKPI.Where(w => w.Indicator == item.Indicator).ToList();
                        if (Actual.Count > 0)
                        {
                            int TotalTeam = Actual.Where(w => TempKPIBYTeam.Where(x => x.AssignCode == w.AssignCode).Any()).ToList().Count();
                            int TotalEmp = Actual.Where(w => !TempKPIBYTeam.Where(x => x.AssignCode == w.AssignCode).Any()).ToList().Count();
                            item.SubTotalEmp = TotalEmp + TotalTeam;
                            item.Actual = Actual.Average(w => w.Acheivement).Value;
                            var Grade = ListNorm.Where(w => w.FromActual <= item.Actual && w.ToActual > item.Actual).ToList();
                            foreach (var G in Grade)
                            {
                                item.Achievement = G.Achievement;
                            }
                        }
                        unitOfWork.Update(item);
                    }
                    unitOfWork.Update(Plan);
                    unitOfWork.Save();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }

        public void AutoUpdate()
        {
            string status = SYDocumentStatus.PENDING.ToString();
            DateTime date = DateTime.Now;
            var ListKPI = unitOfWork.Set<HRKPIAssignHeader>().Where(w => w.Status == status && w.AutoAccept <= date).ToList();
            foreach (var item in ListKPI)
            {
                item.Status = SYDocumentStatus.RELEASED.ToString();
                item.AutoAccept = null;
                unitOfWork.Update(item);
                var EmpAppraisal = unitOfWork.Set<HREmpAppraisal>().FirstOrDefault(w => w.ApprID == item.AssignCode);
                //
                if (EmpAppraisal == null)
                {
                    var AssignMember = unitOfWork.Set<HRKPIAssignMember>().Where(w => w.AssignCode == item.AssignCode);
                    foreach (var member in AssignMember)
                    {
                        EmpAppraisal = unitOfWork.Set<HREmpAppraisal>().FirstOrDefault(w => w.ApprID == member.DocRef);
                        if (EmpAppraisal != null)
                        {
                            EmpAppraisal.KPIStatus = SYDocumentStatus.COMPLETED.ToString();
                            unitOfWork.Update(EmpAppraisal);
                        }
                    }
                }
                else
                {
                    EmpAppraisal.KPIStatus = SYDocumentStatus.COMPLETED.ToString();
                    unitOfWork.Update(EmpAppraisal);
                }
                unitOfWork.Save();
            }
        }
        public void CalculateKPI(string id)
        {
            try
            {
                unitOfWork.BeginTransaction();
                var ASHeader = unitOfWork.Set<HRKPIAssignHeader>().FirstOrDefault(w => w.AssignCode == id);
                List<HRKPITracking> ListKPITracking = new List<HRKPITracking>();
                List<HRKPIList> LIstKPIList = unitOfWork.Set<HRKPIList>().ToList();
                string Status = SYDocumentStatus.APPROVED.ToString();
                var KPIType = unitOfWork.Set<HRKPIType>().FirstOrDefault(w => w.Code == ASHeader.KPIType);
                if (KPIType != null)
                {
                    DateTime StatDate = DateTimeHelper.DateInHourMin(KPIType.StartDate.Value);
                    DateTime EndDate = DateTimeHelper.DateInHourMin(KPIType.EndDate.Value);
                    ListKPITracking = unitOfWork.Set<HRKPITracking>().Where(w => w.AssignCode == id && w.Status == Status
                    && w.DocumentDate >= StatDate && w.DocumentDate <= EndDate).ToList();
                }
                var ListKPITask = unitOfWork.Set<HRKPIAssignItem>().Where(w => w.AssignCode == id).ToList();
                var ListAssignIndicator = unitOfWork.Set<HRKPIAssignIndicator>().Where(w => w.AssignCode == id).ToList();

                decimal Score = 0;
                int Indicator = 0;
                int CountTask = 0;
                decimal TotalAcheivement = 0;
                List<HRKPIAssignItem> ListUpdateItem = new List<HRKPIAssignItem>();
                List<HRKPIAssignIndicator> ListUpdateIndicator = new List<HRKPIAssignIndicator>();
                List<HRKPIAssignHeader> ListUpdateHeader = new List<HRKPIAssignHeader>();
                List<HRKPITracking> ListUpTracking = new List<HRKPITracking>();
                ClsFilterStaff filterStaff = new ClsFilterStaff();
                foreach (var group in ListAssignIndicator)
                {
                    Indicator++;
                    CountTask = 0;
                    Score = 0;
                    group.Weight = 0;
                    foreach (var item in ListKPITask.Where(w => w.Indicator == group.Indicator).ToList())
                    {
                        CountTask++;
                        item.Acheivement = 0;
                        item.Actual = 0;
                        item.Score = 0;
                        group.Weight += item.Weight;
                        var ListTrack = ListKPITracking.Where(w => w.KPI == item.ItemCode).ToList();
                        if (item.Target > 0)
                        {
                            if (ListTrack.Sum(w => w.Actual) > 0)
                            {
                                decimal Amount = 0;
                                if (item.Options == "AVERAGE")
                                {
                                    Amount = ClsRounding.Rounding(ListTrack.Where(w => w.Actual > 0).Average(w => w.Actual), 4, "N");
                                }
                                else
                                {
                                    Amount = ListTrack.Sum(w => w.Actual);
                                }

                                item.Actual = Amount;
                            }
                            ClsAppraiselCalculate KPI = new ClsAppraiselCalculate();
                            KPI.Target = item.Target.Value;
                            KPI.Weight = item.Weight;
                            KPI.Actual = item.Actual.Value;
                            var Sccore = KPI.CalculateScore(item.Symbol);
                            item.Score = Sccore;
                            item.Acheivement = KPI.Achievement;
                        }
                        Score += item.Acheivement.Value;
                        var KPITask = LIstKPIList.FirstOrDefault(w => w.Code == item.ItemCode);
                        if (KPITask != null)
                        {
                            item.KPI = KPITask.Description;
                        }
                        ListUpdateItem.Add(item);
                    }
                    decimal temScore = Score / CountTask;
                    decimal tempTotal = ClsRounding.RoundNormal(temScore, 4);
                    group.Acheivement = tempTotal;
                    //group.Score = tempTotal * group.Weight * 100;
                    group.Score = ListKPITask.Where(w => w.Indicator == group.Indicator).Sum(x => x.Score);
                    //unitOfWork.Update(group);
                    ListUpdateIndicator.Add(group);
                    TotalAcheivement += tempTotal;
                }

                if (ASHeader != null)
                {
                    if (ListAssignIndicator.Count > 0)
                    {
                        if (KPIType != null)
                        {
                            ASHeader.KPIAverage = KPIType.KPIAverage;
                        }
                        ASHeader.TotalAchievement = ClsRounding.Rounding(TotalAcheivement / ListAssignIndicator.Count(), 4, "N");
                        ASHeader.TotalScore = ListAssignIndicator.Sum(w => w.Score);
                        ASHeader.FinalResult = ASHeader.TotalScore * ASHeader.KPIAverage;
                        ASHeader.Grade = "";
                        if (ASHeader.TotalScore > 0)
                        {
                            ClsKPICalculate KPIGrade = new ClsKPICalculate();
                            ASHeader.Grade = KPIGrade.Get_GradeKPI(ASHeader.TotalScore.Value);
                        }
                    }
                    if (ASHeader.AssignedBy == "Individual")
                    {
                        HRStaffProfile Staff = unitOfWork.Set<HRStaffProfile>().FirstOrDefault(w => w.EmpCode == ASHeader.HandleCode);
                        ASHeader.Position = filterStaff.Get_Positon(Staff.JobCode);
                        var Tracking = unitOfWork.Set<HRStaffProfile>().FirstOrDefault(w => w.EmpCode == Staff.APPTracking);
                        if (Tracking != null)
                        {
                            ASHeader.PlanerCode = Tracking.EmpCode;
                            ASHeader.PlanerName = Tracking.AllName;
                            ASHeader.PlanerPosition = filterStaff.Get_Positon(Tracking.JobCode);
                        }
                        var Evaluator = unitOfWork.Set<HRStaffProfile>().FirstOrDefault(w => w.APPEvaluator == Staff.APPEvaluator);
                        if (Evaluator != null)
                        {
                            ASHeader.DirectedByCode = Evaluator.EmpCode;
                            ASHeader.DirectedByName = Evaluator.AllName;
                        }
                    }
                    DateTime DateNow = DateTime.Now.Date;
                    if (ASHeader.ExpectedDate.Value.Date <= DateNow.Date)
                    {
                        ASHeader.ReStatus = SYDocumentStatus.RELEASED.ToString();
                    }
                    //unitOfWork.Update(ASHeader);
                    ListUpdateHeader.Add(ASHeader);
                }
                if (ListUpdateItem.Count > 0)
                {
                    unitOfWork.BulkUpdate(ListUpdateItem);
                }
                if (ListUpdateIndicator.Count > 0)
                {
                    unitOfWork.BulkUpdate(ListUpdateIndicator);
                }
                if (ListUpdateHeader.Count > 0)
                {
                    unitOfWork.BulkUpdate(ListUpdateHeader);
                }
                unitOfWork.BulkCommit();

            }
            catch (Exception ex)
            {
                unitOfWork.Rollback();
            }
        }

        public void CalculateAppraisal()
        {
            OnLoad();
            DateTime date = DateTime.Now;
            var ListAppraisal = unitOfWork.Set<HREmpAppraisal>().Where(w => w.InYear == date.Year).ToList();
            ClsFilterStaff filterStaff = new ClsFilterStaff();
            var ListStaff = unitOfWork.Set<HRStaffProfile>().Where(w => w.Status == "A").ToList();
            foreach (var Appraisal in ListAppraisal)
            {
                HRStaffProfile Staff = ListStaff.FirstOrDefault(w => w.EmpCode == Appraisal.EmpCode);
                if (Staff != null)
                {
                    Appraisal.Position = filterStaff.Get_Positon(Staff.JobCode);
                    var Appraisal1 = unitOfWork.Set<HRStaffProfile>().FirstOrDefault(w => w.EmpCode == Staff.APPAppraisal);
                    if (Appraisal1 != null)
                    {
                        Appraisal.AppraiserCode = Appraisal1.EmpCode;
                        Appraisal.AppraiserName = Appraisal1.AllName;
                        Appraisal.AppraiserPosition = filterStaff.Get_Positon(Appraisal1.JobCode);
                    }
                    var Appraisal2 = unitOfWork.Set<HRStaffProfile>().FirstOrDefault(w => w.APPEvaluator == Staff.APPAppraisal2);
                    if (Appraisal2 != null)
                    {
                        Appraisal.AppraiserCode2 = Appraisal2.EmpCode;
                        Appraisal.AppraiserName2 = Appraisal2.AllName;
                        Appraisal.AppraiserPosition2 = filterStaff.Get_Positon(Appraisal2.JobCode);
                    }
                    if (Appraisal.ApprovedStep == null || Appraisal.ApprovedStep == 1)
                    {
                        Appraisal.AppraiserNext = Appraisal.AppraiserCode;
                    }
                }
                //var _listAppSummary = unitOfWork.Set<HREmpAppraisalSummary>().Where(w => w.AppraisalNo == Appraisal.ApprID).ToList();
                //var KPIMethod = unitOfWork.Set<HRKPIType>().FirstOrDefault(w => w.Code == Appraisal.KPIType);
                //if (KPIMethod == null)
                //{
                //    continue;
                //}
                //decimal FinalScore = ClsRounding.Rounding(EmpKPI.TotalScore.Value * KPIMethod.KPIAverage.Value, 2, "N");
                //bool isAdd = false;
                //if (_listAppSummary.Count() == 0)
                //{
                //    var ListRegion = unitOfWork.Set<HRApprRegion>().Where(w => w.AppraiselType == Appraisal.AppraisalType).ToList();
                //    foreach (var item in ListRegion)
                //    {
                //        isAdd = true;
                //        var AppSum = new HREmpAppraisalSummary();
                //        NewAppraisalSummary(AppSum, item, Appraisal.ApprID, Appraisal.AppraisalType);
                //        _listAppSummary.Add(AppSum);
                //    }
                //}
                //foreach (var item in _listAppSummary)//.Where(w => w.IsKPI == true).ToList())
                //{
                //    if (item.IsKPI == true)
                //    {
                //        Appraisal.KPIScore = FinalScore;
                //        item.Score = EmpKPI.TotalScore;
                //        item.SubTotal = FinalScore;
                //        item.Weight = KPIMethod.KPIAverage;
                //    }

                //    if (isAdd == true)
                //    {
                //        unitOfWork.Add(item);
                //    }
                //    else
                //    {
                //        unitOfWork.Update(item);
                //    }
                //}
                //Appraisal.FinalScore = ClsRounding.Rounding(_listAppSummary.Sum(x => x.SubTotal).Value, 2, "N");
                //if (_listAppSummary.Sum(w => w.SubTotal) > 0)
                //{
                //    var EvaRate = unitOfWork.Set<HRAppGrade>().ToList();
                //    var Rate = EvaRate.Where(w => w.FromScore <= Appraisal.FinalScore && w.ToScore > Appraisal.FinalScore).ToList();
                //    foreach (var item in Rate)
                //    {
                //        Appraisal.Grade = item.Grade;
                //    }
                //}
                //else
                //{
                //    Appraisal.Grade = "";
                //}
                unitOfWork.Update(Appraisal);
                unitOfWork.Save();
            }
        }

        public void NewAppraisalSummary(HREmpAppraisalSummary D, HRApprRegion S, string AppraisalNo, string AppraisalType)
        {
            D.AppraisalNo = AppraisalNo;
            D.TaskID = S.Code;
            D.AppraisalType = AppraisalType;
            D.EvaluationCriteria = S.Description;
            if (!D.Weight.HasValue) D.Weight = 0;
            if (S.IsKPI != true)
            {
                D.Weight = S.Rating;
                D.Score = 0;
            }
            if (D.Weight > 1) D.Weight = D.Weight / 100.00M;
            D.InOrder = S.InOrder;
            D.IsKPI = S.IsKPI;
        }
    }
}