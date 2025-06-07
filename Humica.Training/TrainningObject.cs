using Humica.Core.BS;
using Humica.Core.FT;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic.CF;
using Humica.Training.DB;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;

namespace Humica.Training
{
    public class TrainningObject
    {
        public string ScreenId { get; set; }
        HumicaDBContext DB = new HumicaDBContext();
        Humica.Core.DB.HumicaDBContext DBX = new Core.DB.HumicaDBContext();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public TrainingProgram THeader { get; set; }
        public List<TrainingProgram> ListHeader { get; set; }
        public List<TRTrainingInvitation> ListInvitation { get; set; }
        public List<TRTrainingType> ListTrainingType { get; set; }
        public List<TRTrainingVenue> ListTRTrainingVenues { get; set; }
        //public List<TRTrainingCourseType> ListCourseTypeTypes { get; set; }
        public List<TRCourseCategory> ListTRCourseCategory { get; set; }
        //public List<TRCourseType> ListTRCourseType { get; set; }
        public TRTrainingCourse TRTrainingCourse { get; set; }
        public List<TRTrainingCourse> ListCourseType { get; set; }
        public List<TRTopic> ListTRTopic { get; set; }

        //public List<CFDPTruckAllow> ListTruckMaster { get; set; }

        public TrainingRequestHeader RequestHeader { get; set; }
        public List<TrainingRequestHeader> ListRequestHeader { get; set; }
        public List<TrainingRequestItem> ListRequestItem { get; set; }
        public List<TrainingRequestItem> ListWaitingRequestItem { get; set; }
        public List<TRTrainingEmployee> ListWaitingRItem { get; set; }

        public List<TRELearningQuestion> ListQuestion { get; set; }
        public List<TRELearningAnswer> ListAnswer { get; set; }
        public List<TrainigModule> ListModule { get; set; }
        public TrainingExamHeader ExamHeader { get; set; }
        public List<TrainingExamHeader> ListExamHeader { get; set; }
        public List<TrainingExamItem> ListExamItem { get; set; }
        public List<TrainingCourseItem> ListInvited { get; set; }
        public List<TrainingCourseMaster> ListCourseMaster { get; set; }
        public List<TrainigModuleTemp> TrainingBoard { get; set; }
        public List<TrainingAnouncementItem> TrainingAnounceDetail { get; set; }

        public string InvCode { get; set; }
        public string Program { get; set; }
        public string Course { get; set; }
        public string TrainingType { get; set; }
        public string ModuleDescription { get; set; }
        public string Question { get; set; }
        public string Pic { get; set; }
        public string Dealer { get; set; }
        public string ActionType { get; set; }
        public string AnswerSelected { get; set; }
        public IEnumerable<String> SelectedSport { get; set; }

        public List<int> ListDelete { get; set; }
        public string DocumentNo { get; set; }
        public string ShipmentType { get; set; }
        public string WCType = "SPC01";
        public string GIDocumentType = "NGI01";
        public string DeliveryType = "NDO01";
        public string TREXAM = "TREX01";
        public string TRRQ = "TRRQ01";
        public string CompanyCode { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime ShipDate { get; set; }
        public BSDocConfg DocConfigure { get; set; }
        public FTDGeneralPeriod Filter { get; set; }
        public SYSplitItem SelectListItem { get; set; }
        //public BSQuotaConf Configure { get; set; }

        private const string REMAINING = "REMAINING";
        public string DefaultPlant { get; set; }
        public string DefaultStorage { get; set; }
        public int Group { get; set; }
        public int Timer { get; set; }
        public decimal DG1 { get; set; }
        public decimal DG { get; set; }

        //Result Initial
        public decimal DGInitial { get; set; }
        public decimal DGInitial1 { get; set; }
        public int TotalModuleInitial { get; set; }
        public int PassModuleInitial { get; set; }
        public int FailModuleInitial { get; set; }
        public decimal AcheiveInitial { get; set; }
        public string StatusInitial { get; set; }

        //Result Final
        public decimal DGFinal { get; set; }
        public decimal DGFinal1 { get; set; }
        public int TotalModuleFinal { get; set; }
        public int PassModuleFinal { get; set; }
        public int FailModuleFinal { get; set; }
        public decimal AcheiveFinal { get; set; }
        public string StatusFinal { get; set; }
        public bool IsFN { get; set; }

        public List<TRTrainingRequirement> ListTrainingRequirement { get; set; }
        public List<TRTrainingRequirement> ListTrainingGroup { get; set; }
        public List<TRTrainingRequirement> ListTrainingRoom { get; set; }
        public List<TRTrainingRequirement> ListTrainingDays { get; set; }
        public List<TRTrainingRequirement> ListTrainingCategory { get; set; }
        public List<TRTrainingRequirement> ListTrainingSubCategory { get; set; }
        public List<TRTrainingRequirement> ListExamType { get; set; }
        public List<TrainingAnouncement> ListAnouncement { get; set; }
        public List<TrainingAnouncementItem> ListAnouncementItem { get; set; }
        public List<HRLevel> ListLevel { get; set; }
        public List<TrainigModuleTemp> ListModuleTemp { get; set; }
        //public List<SPMDDeliveryLeadTime> ListDealerSelected { get; set; }
        //public List<SPMDDeliveryLeadTime> ListDealer { get; set; }
        public List<Core.DB.HR_STAFF_VIEW> ListStaff { get; set; }

        public TrainningObject()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }
        public string ReleaseAnoun(string Course, string Dealer)
        {
            try
            {
                DB = new HumicaDBContext();
                if (Course == null || Dealer == null)
                {
                    return "COURSE_DLR_EMPTY";
                }
                var trc = DB.TrainingAnouncement.ToList();
                //var trp = DB.TrainingReleasePrograms.ToList();
                string[] c = Course.Split(';');

                foreach (var r in c)
                {
                    int id = Convert.ToInt32(r);
                    string OPEN = SYDocumentStatus.OPEN.ToString();
                    var h = trc.Single(w => w.ID == id && w.Status == OPEN);
                    if (h != null)
                    {
                        h.Status = SYDocumentStatus.RELEASED.ToString();
                        DB.TrainingAnouncement.Attach(h);
                        DB.Entry(h).Property(w => w.Status).IsModified = true;

                        //Add Dealer
                        string[] d = Dealer.Split(';');
                        int l = 1;
                        foreach (var r2 in d)
                        {
                            var obj = new TrainingAnouncementItem();
                            obj.LineItem = l;
                            obj.AnounceCode = h.Code;
                            obj.Status = SYDocumentStatus.RELEASED.ToString();
                            DB.TrainingAnouncementItem.Add(obj);
                            l++;
                        }
                    }
                    else
                    {
                        return "DOC_INV";
                    }

                }
                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = "";
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (DbUpdateException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = "";
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (Exception e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = "";
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string SavePro()
        {
            try
            {
                if (ListHeader.Count == 0)
                {
                    return "RQU_EMPTY";
                }
                DB = new HumicaDBContext();

                var rh = DB.TrainingProgram.ToList();
                var rhc = DB.TrainingCourseMaster.ToList();

                foreach (var r in ListHeader)
                {
                    var c = rh.Where(w => w.ProgramCode == r.ProgramCode).ToList();
                    if (c.Count > 0)
                    {
                        DB.TrainingProgram.Remove(c.First());
                    }

                }
                foreach (var r in ListCourseMaster)
                {
                    var c = rhc.Where(w => w.Program == r.Program && w.Code == r.Code).ToList();
                    if (c.Count > 0)
                    {
                        DB.TrainingCourseMaster.Remove(c.First());
                    }

                }

                var lst = new List<TrainingProgram>();
                foreach (var r in ListHeader)
                {
                    if (
                         r.ProgramCode == "" || r.ProgramName == ""
                        )
                    {
                        return "REQUIRED_FIELD(*)";
                    }
                    if (lst.Where(w => w.ProgramCode == r.ProgramCode).ToList().Count == 0)
                    {
                        DB.TrainingProgram.Add(r);
                        lst.Add(r);
                    }

                }

                var lstcourse = new List<TrainingCourseMaster>();
                foreach (var r in ListCourseMaster)
                {
                    if (
                         r.Program == "" || r.Code == "" || r.Description == ""
                        )
                    {
                        return "REQUIRED_FIELD(*)";
                    }
                    if (lstcourse.Where(w => w.Program == r.Program && w.Code == r.Code).ToList().Count == 0)
                    {
                        DB.TrainingCourseMaster.Add(r);
                        lstcourse.Add(r);
                    }

                }
                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = "";
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (DbUpdateException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = "";
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (Exception e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = "";
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string SAVERequest(string InvCode)
        {
            try
            {
                if (ListRequestItem.Count == 0)
                {
                    return "RQU_EMPTY";
                }
                DB = new HumicaDBContext();

                var rh = DB.TrainingRequestHeader.ToList();

                //int p = rh.Count;
                DocConfigure = new BSDocConfg(TRRQ, BS.CompanyCode, true);
                //Header.InvCode = Configure.NextNumberRank;
                RequestHeader.RequestCode = DocConfigure.NextNumberRank;
                if (RequestHeader.RequestCode == null)
                {
                    return "NUMBERANGE_EMPTY";
                }
                RequestHeader.CreatedBy = User.UserName;
                RequestHeader.CreatedOn = DateTime.Now;
                RequestHeader.ChangedBy = User.UserName;
                RequestHeader.ChangedOn = DateTime.Now;
                RequestHeader.Status = SYDocumentStatus.OPEN.ToString();
                RequestHeader.ReStatus = SYDocumentStatus.OPEN.ToString();
                RequestHeader.InvCode = InvCode;

                int l = 1;
                var st = DBX.HRStaffProfiles.ToList();
                foreach (var r in ListRequestItem)
                {
                    r.RequestCode = RequestHeader.RequestCode;
                    r.InvCode = RequestHeader.InvCode;
                    r.Course = RequestHeader.Course;
                    r.Program = RequestHeader.Program;
                    r.RequestDate = RequestHeader.RequestDate;
                    r.Description = RequestHeader.Description;
                    r.Comment = RequestHeader.Comment;
                    r.LineItem = l;
                    r.IsDLRRead = true;
                    r.IsNCXRead = true;
                    r.Status = SYDocumentStatus.OPEN.ToString();
                    r.ReStatus = SYDocumentStatus.OPEN.ToString();
                    r.MGStatus = SYDocumentStatus.OPEN.ToString();

                    DB.TrainingRequestItem.Add(r);
                    l++;
                }
                DB.TrainingRequestHeader.Add(RequestHeader);
                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = RequestHeader.RequestCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (DbUpdateException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = RequestHeader.RequestCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (Exception e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = RequestHeader.RequestCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }

        public string SAVEExam(string id, string InvCode)
        {
            try
            {
                string[] Answers = id.Split(';');

                if (id == null)
                {
                    return "CHECKED_EMPTY";
                }


                DB = new HumicaDBContext();

                //var chstaff = DB.CFStaffUsers.Where(w => w.Owner == BS.CompanyCode && w.UserID == User.UserID).ToList();
                //if (chstaff.Count == 0)
                //{
                //    return "STAFF_MAPPING";
                //}
                var rh = DB.TrainingExamHeader.ToList();
                var getAnswer = DB.TRELearningAnswer.ToList();
                var getModule = DB.TrainigModule.ToList();

                //string stid = chstaff.First().StaffID.ToString();
                var staff = DBX.HRStaffProfiles.Where(w => w.EmpCode == User.UserName).ToList();
                if (staff.Count == 0)
                {
                    return "STAFF_MAPPING";
                }
                //int p = rh.Count;
                var objCF = DBX.ExCfWorkFlowItems.FirstOrDefault(w => w.ScreenID == ScreenId);
                if (objCF == null)
                {
                    return "REQUEST_TYPE_NE";
                }
                var objNumber = new CFNumberRank(objCF.DocType, objCF.ScreenID);
                if (objNumber.NextNumberRank == null)
                {
                    return "NUMBER_RANK_NE";
                }
                //DocConfigure = new BSDocConfg(TREXAM, BS.CompanyCode, true);
                //Header.InvCode = Configure.NextNumberRank;
                //ExamHeader.ExamCode = DocConfigure.NextNumberRank;
                ExamHeader.ExamCode = objNumber.NextNumberRank;
                if (ExamHeader.ExamCode == null)
                {
                    return "NUMBERANGE_EMPTY";
                }
                ExamHeader.StaffID = staff.First().EmpCode.ToString();
                ExamHeader.CreatedBy = User.UserName;
                ExamHeader.CreatedOn = DateTime.Now;
                ExamHeader.Type = 67;
                ExamHeader.Result = SYDocumentStatus.FAILURED.ToString();
                ExamHeader.IsInitialTest = true;
                ExamHeader.InvCode = InvCode;
                var chectest = rh.Where(w => w.TrainingType == ExamHeader.TrainingType && w.Course == ExamHeader.Course && w.Topic == ExamHeader.Topic && w.InvCode == ExamHeader.InvCode && w.StaffID == ExamHeader.StaffID).ToList();
                if (chectest.Count > 0)
                {
                    ExamHeader.IsFinalTest = true;
                }
                var lst = new List<TrainingExamItem>();

                int l = 1;
                foreach (var r in Answers)
                {

                    if (r != "")
                    {
                        string[] anstr = r.Split(',');
                        string qu = anstr[0].ToString();
                        string ans = anstr[1].ToString();
                        string vl = anstr[2].ToString();

                        var obj = new TrainingExamItem();
                        obj.ExamCode = ExamHeader.ExamCode;
                        obj.ExamDate = ExamHeader.ExamDate;
                        obj.Coursecode = ExamHeader.Course;
                        obj.TrainingType = ExamHeader.TrainingType;
                        obj.StaffID = ExamHeader.StaffID;
                        obj.LineItem = l;
                        obj.Topic = ExamHeader.Topic;
                        obj.InvCode = ExamHeader.InvCode;
                        obj.QuestionCode = qu;
                        obj.AnswerCode = ans;
                        obj.CheckValue = Convert.ToInt32(vl);

                        var gscore = getAnswer.FirstOrDefault(w => w.TrainingType == obj.TrainingType && w.CourseCode == obj.Coursecode && w.Topic == obj.Topic && w.QuestionCode == obj.QuestionCode && w.AnswerCode == obj.AnswerCode);
                        if (gscore != null)
                        {
                            if (obj.CheckValue == gscore.CorrectValue && gscore.CorrectValue > 0)
                            {
                                obj.TotalScore = gscore.TotalScore;
                                ExamHeader.CorrectAnswer += 1;
                            }
                            else
                            {
                                obj.CheckValue = 1;
                                ExamHeader.InCorrectAnswer += 1;
                            }
                        }
                        ExamHeader.TotalScore += obj.TotalScore;

                        lst.Add(obj);
                        DB.TrainingExamItem.Add(obj);

                        l++;
                    }

                }

                var listModule = getAnswer.Where(w => w.TrainingType == ExamHeader.TrainingType && w.CourseCode == ExamHeader.Course && w.Topic == ExamHeader.Topic).ToList();
                foreach (var r in listModule.ToList())
                {
                    var coutnotcheck = lst.Where(w => w.TrainingType == r.TrainingType && w.Coursecode == r.CourseCode && w.Topic == r.Topic && w.QuestionCode == r.QuestionCode && w.AnswerCode == r.AnswerCode).ToList();
                    if (coutnotcheck.Count == 0)
                    {
                        var gscore = getAnswer.FirstOrDefault(w => w.TrainingType == r.TrainingType && w.TrainingType == r.CourseCode && w.Topic == r.Topic && w.QuestionCode == r.QuestionCode && w.AnswerCode == r.AnswerCode);
                        if (gscore != null)
                        {
                            if (gscore.ColumnCheck == 1 && gscore.CorrectValue == 0)
                            {
                                var obj = new TrainingExamItem();
                                obj.ExamCode = ExamHeader.ExamCode;
                                obj.ExamDate = ExamHeader.ExamDate;
                                obj.Coursecode = ExamHeader.Course;
                                obj.TrainingType = ExamHeader.TrainingType;
                                obj.StaffID = ExamHeader.StaffID;
                                obj.LineItem = l;
                                obj.Topic = ExamHeader.Topic;
                                obj.QuestionCode = r.QuestionCode;
                                obj.AnswerCode = r.AnswerCode;
                                obj.InvCode = ExamHeader.InvCode;

                                if (r.CheckValue == gscore.CorrectValue)
                                {
                                    obj.TotalScore = gscore.TotalScore;
                                    obj.CheckValue = Convert.ToInt32(r.CorrectValue);
                                    ExamHeader.TotalScore += obj.TotalScore;
                                    ExamHeader.CorrectAnswer += 1;
                                }
                                else
                                {
                                    obj.CheckValue = 1;
                                    ExamHeader.InCorrectAnswer += 1;
                                }

                                DB.TrainingExamItem.Add(obj);

                                l++;
                            }
                            else
                            {
                                var obj = new TrainingExamItem();
                                obj.ExamCode = ExamHeader.ExamCode;
                                obj.ExamDate = ExamHeader.ExamDate;
                                obj.Coursecode = ExamHeader.Course;
                                obj.TrainingType = ExamHeader.TrainingType;
                                obj.StaffID = ExamHeader.StaffID;
                                obj.LineItem = l;
                                obj.Topic = ExamHeader.Topic;
                                obj.QuestionCode = r.QuestionCode;
                                obj.AnswerCode = r.AnswerCode;
                                obj.InvCode = ExamHeader.InvCode;
                                obj.CheckValue = 0;
                                ExamHeader.InCorrectAnswer += 1;

                                DB.TrainingExamItem.Add(obj);

                                l++;
                            }

                        }
                    }
                }
                var chmodule = getModule.FirstOrDefault(w => w.TrainingType == ExamHeader.TrainingType && w.CourseCode == ExamHeader.Course && w.Topic == ExamHeader.Topic);
                if (chmodule != null)
                {
                    decimal? sc = chmodule.Score;
                    decimal? pc = chmodule.Target;
                    ExamHeader.TotalAchieve = Math.Round(ExamHeader.TotalScore / (decimal)sc, 2) * 100;
                    if (ExamHeader.TotalAchieve >= pc)
                    {
                        ExamHeader.Result = SYDocumentStatus.PASS.ToString();
                    }
                    ExamHeader.TotalModuleScore = (decimal)sc;
                    ExamHeader.TopicDescription = chmodule.Description;
                }

                DB.TrainingExamHeader.Add(ExamHeader);
                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ExamHeader.ExamCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (DbUpdateException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ExamHeader.ExamCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (Exception e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ExamHeader.ExamCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }

        public string RequestTraining(string id)
        {
            try
            {
                DB = new HumicaDBContext();
                //var DBI = new DBBusinessProcess();
                if (id == null)
                {
                    return "CHECKNULL";
                }

                var OPEN = SYDocumentStatus.OPEN.ToString();
                var checkdup = DB.TrainingRequestHeader.FirstOrDefault(w => w.RequestCode == id && w.Status == OPEN);
                if (checkdup != null)
                {
                    var checkItem = DB.TrainingRequestItem.Where(w => w.RequestCode == id).ToList();
                    foreach (var r in checkItem)
                    {
                        r.IsDLRRead = true;
                        r.IsNCXRead = false;
                        r.Status = SYDocumentStatus.PENDING.ToString();
                        r.ReStatus = SYDocumentStatus.PENDING.ToString();
                        DB.TrainingRequestItem.Attach(r);
                        DB.Entry(r).Property(w => w.ReStatus).IsModified = true;
                        DB.Entry(r).Property(w => w.Status).IsModified = true;
                        DB.Entry(r).Property(w => w.IsDLRRead).IsModified = true;
                        DB.Entry(r).Property(w => w.IsNCXRead).IsModified = true;
                    }

                    checkdup.ChangedOn = DateTime.Now;
                    checkdup.ChangedBy = User.UserName;
                    checkdup.ReStatus = SYDocumentStatus.PENDING.ToString();
                    checkdup.Status = SYDocumentStatus.PENDING.ToString();
                    checkdup.ChangedBy = User.UserName;
                    checkdup.ChangedOn = DateTime.Now;
                    DB.TrainingRequestHeader.Attach(checkdup);
                    DB.Entry(checkdup).Property(w => w.RequestDate).IsModified = true;
                    DB.Entry(checkdup).Property(w => w.Status).IsModified = true;
                    DB.Entry(checkdup).Property(w => w.ChangedOn).IsModified = true;
                    DB.Entry(checkdup).Property(w => w.ChangedBy).IsModified = true;
                    DB.Entry(checkdup).Property(w => w.CreatedOn).IsModified = true;
                    DB.Entry(checkdup).Property(w => w.CreatedBy).IsModified = true;
                }
                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = id;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (DbUpdateException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = id;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (Exception e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = id;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        //public string IMPORTExam()
        //{
        //    try
        //    {
        //        ;

        //        if (ListExamHeader.Count == 0)
        //        {
        //            return "CHECKED_EMPTY";
        //        }


        //        DB = new DBBusinessProcess();

        //       // var chstaff = DB.CFStaffUsers.ToList();

        //        var rh = DB.TrainingExamHeaders.ToList();
        //        foreach (var r in ListExamHeader)
        //        {
        //            var check = rh.Where(w =>  w.Program == r.Program && w.Course == r.Course && w.Module == r.Module).ToList();
        //            if (check.Count > 0)
        //            {
        //                DB.TrainingExamHeaders.Remove(check.First());
        //            }

        //        }
        //        int p = rh.Count + 1;
        //        var stafflist = DB.HRStaffProfiles.ToList();
        //        foreach (var r in ListExamHeader)
        //        {
        //            //if (
        //            //    r.DealerCode == "" || r.Program == "" ||
        //            //    r.IsFinalTest == null || r.Type == null ||
        //            //    r.Course == "" || r.Module == "" ||
        //            //    r.StaffID == "" || r.ExamDate == null ||
        //            //    r.Result == "" || r.TotalScore == null ||
        //            //    r.TotalAchieve == null || r.CorrectAnswer == null
        //            //    || r.InCorrectAnswer == null || r.InvCode == ""
        //            //    )
        //            //{
        //            //    return "REQUIRED_FIELD(*)";
        //            //}
        //            ExamHeader = new TrainingExamHeader();
        //            DocConfigure = new BSDocConfg(TREXAM);
        //            //Header.InvCode = Configure.NextNumberRank;
        //            ExamHeader.ExamCode = DocConfigure.NextNumberRank;
        //            ExamHeader.Type = r.Type;
        //            ExamHeader.Program = r.Program;
        //            ExamHeader.Course = r.Course;
        //            ExamHeader.InvCode = r.InvCode;
        //            ExamHeader.Module = r.Module;
        //            var checkst = stafflist.Where(w => w.EmpCode == r.StaffID).ToList();
        //            if (checkst.Count > 0)
        //            {
        //                ExamHeader.StaffID = checkst.First().EmpCode.ToString();
        //            }

        //            ExamHeader.ExamDate = r.ExamDate;
        //            ExamHeader.TotalScore = r.TotalScore;
        //            ExamHeader.TotalAchieve = r.TotalAchieve;
        //            ExamHeader.CorrectAnswer = r.CorrectAnswer;
        //            ExamHeader.InCorrectAnswer = r.InCorrectAnswer;
        //            ExamHeader.Result = r.Result;
        //            ExamHeader.NCXComment1 = r.NCXComment1;
        //            ExamHeader.NCXComment2 = r.NCXComment2;
        //            ExamHeader.NCXComment3 = r.NCXComment3;
        //            ExamHeader.CreatedBy = User.UserName;
        //            ExamHeader.CreatedOn = DateTime.Now;
        //            ExamHeader.IsInitialTest = true;
        //            ExamHeader.IsFinalTest = r.IsFinalTest;

        //            DB.TrainingExamHeader.Add(ExamHeader);
        //            p++;
        //        }


        //        int row = DB.SaveChanges();
        //        return SYConstant.OK;
        //    }
        //    catch (DbEntityValidationException e)
        //    {
        //        /*------------------SaveLog----------------------------------*/
        //        SYEventLog log = new SYEventLog();
        //        log.ScreenId = ScreenId;
        //        log.UserId = User.UserName;
        //        log.DocurmentAction = ExamHeader.ExamCode;
        //        log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

        //        SYEventLogObject.saveEventLog(log, e);
        //        /*----------------------------------------------------------*/
        //        return "EE001";
        //    }
        //    catch (DbUpdateException e)
        //    {
        //        /*------------------SaveLog----------------------------------*/
        //        SYEventLog log = new SYEventLog();
        //        log.ScreenId = ScreenId;
        //        log.UserId = User.UserName;
        //        log.DocurmentAction = ExamHeader.ExamCode;
        //        log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

        //        SYEventLogObject.saveEventLog(log, e, true);
        //        /*----------------------------------------------------------*/
        //        return "EE001";
        //    }
        //    catch (Exception e)
        //    {
        //        /*------------------SaveLog----------------------------------*/
        //        SYEventLog log = new SYEventLog();
        //        log.ScreenId = ScreenId;
        //        log.UserId = User.UserName;
        //        log.DocurmentAction = ExamHeader.ExamCode;
        //        log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

        //        SYEventLogObject.saveEventLog(log, e, true);
        //        /*----------------------------------------------------------*/
        //        return "EE001";
        //    }
        //}

        public string UpdateRequest(string id)
        {
            try
            {
                DB = new HumicaDBContext();
                //var DBI = new DBBusinessProcess();
                if (id == null || ListRequestItem.Count == 0)
                {
                    return "CHECKNULL";
                }

                var OPEN = SYDocumentStatus.OPEN.ToString();
                var checkdup = DB.TrainingRequestHeader.FirstOrDefault(w => w.RequestCode == id && w.Status == OPEN);
                if (checkdup != null)
                {
                    var checkItem = DB.TrainingRequestItem.Where(w => w.RequestCode == id).ToList();
                    foreach (var r in checkItem)
                    {
                        DB.TrainingRequestItem.Remove(r);
                    }
                    int l = 1;
                    foreach (var r in ListRequestItem)
                    {
                        r.RequestCode = checkdup.RequestCode;
                        r.Course = RequestHeader.Course;
                        r.Program = RequestHeader.Program;
                        r.RequestDate = RequestHeader.RequestDate;
                        r.Status = RequestHeader.Status;
                        r.ReStatus = RequestHeader.ReStatus;
                        r.MGStatus = RequestHeader.Status;
                        r.LineItem = l;
                        r.IsNCXRead = true;
                        r.IsDLRRead = true;
                        DB.TrainingRequestItem.Add(r);
                        l++;
                    }

                    checkdup.Program = RequestHeader.Program;
                    checkdup.Course = RequestHeader.Course;
                    checkdup.RequestDate = RequestHeader.RequestDate;
                    checkdup.Description = RequestHeader.Description;
                    checkdup.ChangedOn = DateTime.Now;
                    checkdup.ChangedBy = User.UserName;
                    DB.TrainingRequestHeader.Attach(checkdup);
                    DB.Entry(checkdup).Property(w => w.RequestDate).IsModified = true;
                    DB.Entry(checkdup).Property(w => w.Description).IsModified = true;
                    DB.Entry(checkdup).Property(w => w.ChangedOn).IsModified = true;
                    DB.Entry(checkdup).Property(w => w.ChangedBy).IsModified = true;
                    DB.Entry(checkdup).Property(w => w.Program).IsModified = true;
                    DB.Entry(checkdup).Property(w => w.Course).IsModified = true;
                }
                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = RequestHeader.RequestCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (DbUpdateException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = RequestHeader.RequestCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (Exception e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = RequestHeader.RequestCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }

        public string SaveInfo()
        {
            try
            {
                DB = new HumicaDBContext();
                THeader.Status = SYDocumentStatus.OPEN.ToString();
                DB.TrainingProgram.Add(THeader);
                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = THeader.ProgramCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (DbUpdateException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = THeader.ProgramCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (Exception e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = THeader.ProgramCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string UpdatePO(string id)
        {
            try
            {
                DB = new HumicaDBContext();
                //var DBI = new DBBusinessProcess();
                if (id == null)
                {
                    return "CHECKNULL";
                }

                var status = SYDocumentStatus.OPEN.ToString();
                var checkdup = DB.TrainingProgram.FirstOrDefault(w => w.ProgramCode == id);
                if (checkdup != null)
                {
                    checkdup.TrainingRequirement = THeader.TrainingRequirement;
                    checkdup.Target = THeader.Target;
                    checkdup.Capacity = THeader.Capacity;
                    checkdup.TrainingGroup = THeader.TrainingGroup;
                    checkdup.StartDate = THeader.StartDate;
                    checkdup.EndDate = THeader.EndDate;
                    DB.TrainingProgram.Attach(checkdup);
                    DB.Entry(checkdup).Property(w => w.TrainingRequirement).IsModified = true;
                    DB.Entry(checkdup).Property(w => w.Target).IsModified = true;
                    DB.Entry(checkdup).Property(w => w.Capacity).IsModified = true;
                    DB.Entry(checkdup).Property(w => w.TrainingGroup).IsModified = true;
                    DB.Entry(checkdup).Property(w => w.StartDate).IsModified = true;
                    DB.Entry(checkdup).Property(w => w.EndDate).IsModified = true;
                }
                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = THeader.ProgramCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (DbUpdateException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = THeader.ProgramCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (Exception e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = THeader.ProgramCode;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string ReleaseInfo(string Code)
        {
            try
            {
                string[] Emp = Code.Split(';');
                foreach (var Procode in Emp)
                {
                    if (Procode.Trim() != "")
                    {
                        string status = SYDocumentStatus.OPEN.ToString();
                        var objMatch = DB.TrainingProgram.FirstOrDefault(w => w.ProgramCode == Procode && w.Status == status);
                        if (objMatch == null)
                        {
                            return "EE001";
                        }
                        objMatch.Status = SYDocumentStatus.RELEASED.ToString();
                        DB.TrainingProgram.Attach(objMatch);
                        DB.Entry(objMatch).Property(w => w.Status).IsModified = true;
                        int row = DB.SaveChanges();

                    }
                }
                return SYConstant.OK;
            }
            catch (Exception e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Code;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string InactiveInfo(string Code)
        {
            try
            {
                string[] Emp = Code.Split(';');
                foreach (var Procode in Emp)
                {
                    if (Procode.Trim() != "")
                    {
                        string status = SYDocumentStatus.RELEASED.ToString();
                        var objMatch = DB.TrainingProgram.FirstOrDefault(w => w.ProgramCode == Procode && w.Status == status);
                        if (objMatch == null)
                        {
                            return "EE001";
                        }
                        objMatch.Status = SYDocumentStatus.CANCELLED.ToString();
                        DB.TrainingProgram.Attach(objMatch);
                        DB.Entry(objMatch).Property(w => w.Status).IsModified = true;
                        int row = DB.SaveChanges();
                    }
                }
                return SYConstant.OK;
            }
            catch (Exception e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = Code;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        //Training Course
        public string CreateTrainingCourse()
        {
            try
            {
                if (TRTrainingCourse.Code == null)
                {
                    return "CODE";
                }
                if (string.IsNullOrEmpty(TRTrainingCourse.Description))
                {
                    return "DESCRIPTION";
                }
                TRTrainingCourse.Code = TRTrainingCourse.Code.Trim().ToUpper();
                int count = DB.TRTrainingCourses.Where(x => x.Code == TRTrainingCourse.Code).Count();

                if (count > 0)
                {
                    return "CODE_EXISTS";
                }
                TRTrainingCourse.CreatedBy = User.UserName;
                TRTrainingCourse.CreatedOn = DateTime.Now;

                DB.TRTrainingCourses.Add(TRTrainingCourse);
                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserID.ToString();
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string EditTrainingCourse(int id)
        {
            try
            {
                if (string.IsNullOrEmpty(TRTrainingCourse.Description))
                {
                    return "DESCRIPTION";
                }
                var trainingCourse = DB.TRTrainingCourses.Find(id);
                trainingCourse.Description = TRTrainingCourse.Description;
                trainingCourse.SecondDescription = TRTrainingCourse.SecondDescription;
                trainingCourse.Objective = TRTrainingCourse.Objective;
                trainingCourse.ChangedBy = User.UserName;
                trainingCourse.ChangedOn = DateTime.Now;

                DB.TRTrainingCourses.Attach(trainingCourse);
                DB.Entry(trainingCourse).Property(w => w.Description).IsModified = true;
                DB.Entry(trainingCourse).Property(w => w.SecondDescription).IsModified = true;
                DB.Entry(trainingCourse).Property(w => w.Objective).IsModified = true;
                DB.Entry(trainingCourse).Property(w => w.ChangedBy).IsModified = true;
                DB.Entry(trainingCourse).Property(w => w.ChangedOn).IsModified = true;

                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserID.ToString();
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string DeleteTrainingCourse(int id)
        {
            try
            {

                var objCust = DB.TRTrainingCourses.Find(id);
                if (objCust == null)
                {
                    return "TRAINING_EN";
                }

                DB.TRTrainingCourses.Attach(objCust);
                DB.Entry(objCust).State = System.Data.Entity.EntityState.Deleted;
                int row = DB.SaveChanges();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserID.ToString();
                log.ScreenId = TRTrainingCourse.Code;
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
            catch (Exception e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserID.ToString();
                log.ScreenId = TRTrainingCourse.Code; ;
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
    }

    public partial class TrainigModuleTemp
    {
        public int ID { get; set; }
        public string Invcode { get; set; }
        public string Coursecode { get; set; }
        public string TrainingType { get; set; }
        public string Topic { get; set; }
        public string Coursename { get; set; }
        public string ProgramName { get; set; }
        public string Document { get; set; }
        public string UrlDocument { get; set; }
        public string OtherDocument { get; set; }
        public string UrlOtherDocument { get; set; }
        public DateTime? Startdate { get; set; }
        public DateTime? Enddate { get; set; }
        public string Description { get; set; }
        public string DayTerm { get; set; }
        public decimal Timer1 { get; set; }
        public decimal Timer2 { get; set; }
    }
    public partial class TrainingExamQuizAnswer
    {
        public int ID { get; set; }
        public string ExamCode { get; set; }
        public string StaffID { get; set; }
        public string Program { get; set; }
        public string Course { get; set; }
        public string Module { get; set; }
        public Nullable<System.DateTime> ExamDate { get; set; }
        public decimal TotalScore { get; set; }
        public int CorrectAnswer { get; set; }
        public int InCorrectAnswer { get; set; }
        public bool IsInitialTest { get; set; }
        public bool IsFinalTest { get; set; }
    }
    public enum ExtStatus
    {
        RETURN, TRAINING, INVITED, FAILURED, PROCCESSING, WAITING, PASS, FINISHED, REPAIRING, CONFIRMED, ACKNOWLEDGE, PAID, SHIPPING, SHIPPED, BACKORDER, HOLD, OPEN, PENDING, PARTIAL, RELEASED, COMPLETED, APPROVED, RECEIVED, REJECTED, SOLD,
        PRECANCEL, CANCELLED, PRERELEASE, POSTED, DEPOSITED, REVERSED, CLEARED, NOTCLEAR, CLOSED, SIMULATED, COVERED, RESERVED, CREDIT
    }
}