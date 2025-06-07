using Humica.Core.FT;
using Humica.Core.SY;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Training.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;

namespace Humica.Training
{
    public class TrainingInvitationObject
    {
        HumicaDBContext DB = new HumicaDBContext();
        Core.DB.HumicaDBContext DBStaff = new Core.DB.HumicaDBContext();
       
        public SMSystemEntity DP = new SMSystemEntity();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public string ScreenId { get; set; }
        public string MessageError { get; set; }
        public FTTraining FTTraining { get; set; }
        public TRTrainingInvitation Header { get; set; }
        public TRTrainingCalender HeaderCalender { get; set; }
        public List<Core.DB.HRStaffProfile> ListStaff { get; set; }
        public List<TRTrainingEmployee> ListTrainee { get; set; }
        public List<TRTrainingInvitation> ListInvitation { get; set; }
        public TRTrainingEmployee HeaderTrainee { get; set; }
        public List<string> ListEmpCode { get; set; }
        public List<TRTrainingCalender> ListCalender { get; set; }
        public TrainingInvitationObject()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }
        public async Task<string> GetCalender(int ID)
        {
            Header = new TRTrainingInvitation();
            ListEmpCode = new List<string>();
            HeaderCalender = await DB.TRTrainingCalenders.FirstOrDefaultAsync(w => w.TrainNo == ID);
            if (HeaderCalender == null)
            {
                return "DOC_INV";
            }
            var _ListTempTrainee = await DB.TRTrainingEmployees.Where(x => x.CalendarID == ID).ToListAsync();
            if (_ListTempTrainee.Count() > 0)
                _ListTempTrainee.ToList().ForEach(w => ListEmpCode.Add(w.EmpCode));

            Header.InYear = HeaderCalender.InYear.Value;
            Header.CalendarID = HeaderCalender.TrainNo;
            Header.CourseID = HeaderCalender.CourseID;
            Header.CourseName = HeaderCalender.CourseName;
            Header.TrainingTypeID = HeaderCalender.TrainingTypeID;
            Header.TrainingType = HeaderCalender.TrainingType;
            Header.CourseCategoryID = HeaderCalender.CourseCategoryID;
            Header.CourseCategory = HeaderCalender.CourseCategory;
            Header.ScheduleFrom = HeaderCalender.StartDate.Value;
            Header.ScheduleTo = HeaderCalender.EndDate.Value;
            Header.ScoreTheory = 0;
            Header.ScorePractice = 0;
            Header.Target = 0;
            Header.Capacity = 0;
            Header.RequestDate = DateTime.Now;
            Header.Status = SYDocumentStatus.OPEN.ToString();

            return SYConstant.OK;
        }
        public async Task GetAllStaff()
        {

            var tblStaffProfile =await DBStaff.HRStaffProfiles.Where(w => w.Status == "A").ToListAsync();
            this.ListStaff = tblStaffProfile.Where(w => !this.ListEmpCode.Contains(w.EmpCode)).ToList();
        }
        public string CreateTrainee()
        {
            try
            {
                int i = 0;
                List<Core.DB.HRStaffProfile> ListStaff = DBStaff.HRStaffProfiles.Where(w => w.Status == "A").ToList();
                List<Core.DB.HRDepartment> ListDepartment = DBStaff.HRDepartments.ToList();
                List<Core.DB.HRPosition> ListPosition = DBStaff.HRPositions.ToList();
                List<TRTrainingRequirement> ListGroup = DB.TRTrainingRequirements.Where(w => w.Category == "G").ToList();
                using (var context = new HumicaDBContext())
                {
                    using (var dbContextTransaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            Header.CalendarID = HeaderCalender.TrainNo;
                            Header.InYear = HeaderCalender.InYear.Value;
                            Header.CourseName = HeaderCalender.CourseName;
                            Header.TrainingType = HeaderCalender.TrainingType;
                            Header.CourseCategory = HeaderCalender.CourseCategory;
                            Header.CreatedBy = User.UserName;
                            Header.CreatedOn = DateTime.Now;

                            context.TRTrainingInvitations.Add(Header);

                            int row = context.SaveChanges();
                            i = (int)Header.TrainNo;
                            int LineItem = 0;
                            foreach (var item in ListTrainee)
                            {
                                LineItem += 1;
                                HeaderTrainee = new TRTrainingEmployee();
                                HeaderTrainee.TrainNo = Header.TrainNo;
                                HeaderTrainee.LineItem = LineItem;
                                //HeaderTrainee.RequestDate = DateTime.Now;
                                HeaderTrainee.ScoreTheory = item.ScoreTheory;
                                HeaderTrainee.ScorePractice = item.ScorePractice;
                                HeaderTrainee.CalendarID = HeaderCalender.TrainNo;
                                HeaderTrainee.InYear = HeaderCalender.InYear;
                                HeaderTrainee.CourseID = HeaderCalender.CourseID;
                                HeaderTrainee.CourseName = HeaderCalender.CourseName;
                                HeaderTrainee.CourseCategoryID = HeaderCalender.CourseCategoryID;
                                HeaderTrainee.CourseCategory = HeaderCalender.CourseCategory;
                                HeaderTrainee.TrainingType = HeaderCalender.TrainingTypeID;
                                HeaderTrainee.EmpCode = item.EmpCode;
                                Core.DB.HRStaffProfile Staff = ListStaff.FirstOrDefault(w => w.EmpCode == item.EmpCode);
                                if (Staff != null)
                                {
                                    HeaderTrainee.EmpName = Staff.AllName;
                                    if (!string.IsNullOrEmpty(Staff.DEPT))
                                    {
                                        var Dept = ListDepartment.FirstOrDefault(w => w.Code == Staff.DEPT);
                                        if (Dept != null) HeaderTrainee.Department = Dept.Description;
                                    }
                                    if (!string.IsNullOrEmpty(Staff.JobCode))
                                    {
                                        var Post = ListPosition.FirstOrDefault(w => w.Code == Staff.JobCode);
                                        if (Post != null) HeaderTrainee.Position = Post.Description;
                                    }
                                }
                                var _GP = ListGroup.FirstOrDefault(w => w.ID == Convert.ToInt32(Header.TrainingGroup));
                                if (_GP != null) HeaderTrainee.GroupDescription = _GP.Description;
                                HeaderTrainee.CreatedBy = User.UserName;
                                HeaderTrainee.CreatedOn = DateTime.Today;
                                HeaderTrainee.Status = SYDocumentStatus.OPEN.ToString();
                                HeaderTrainee.ReStatus = SYDocumentStatus.OPEN.ToString();
                                context.TRTrainingEmployees.Add(HeaderTrainee);
                                context.SaveChanges();
                            }

                            dbContextTransaction.Commit();
                        }
                        catch (DbEntityValidationException e)
                        {
                            dbContextTransaction.Rollback();
                        }
                    }
                }
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = HeaderCalender.TrainNo.ToString();
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
                log.DocurmentAction = HeaderCalender.TrainNo.ToString();
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
                //log.DocurmentAction = Header.TranNo.ToString();
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string EditTrainee(int id)
        {
            List<Core.DB.HRStaffProfile> ListStaff = DBStaff.HRStaffProfiles.Where(w => w.Status == "A").ToList();
            List<Core.DB.HRDepartment> ListDepartment = DBStaff.HRDepartments.ToList();
            List<Core.DB.HRPosition> ListPosition = DBStaff.HRPositions.ToList();
            List<TRTrainingRequirement> ListGroup = DB.TRTrainingRequirements.Where(w => w.Category == "G").ToList();
            var DBM = new HumicaDBContext();
            try
            {
                DBM.Configuration.AutoDetectChangesEnabled = false;
                try
                {
                    var ObjMatch = DB.TRTrainingInvitations.FirstOrDefault(w => w.TrainNo == id);
                    if (ObjMatch == null)
                        return "INV_DOC";
                    var _ListAgenda = DB.TRTrainingEmployees.Where(w => w.TrainNo == ObjMatch.TrainNo).ToList();
                    foreach (var item in _ListAgenda)
                    {
                        DBM.TRTrainingEmployees.Attach(item);
                        DBM.Entry(item).State = EntityState.Deleted;
                    }
                    ObjMatch.ChangedBy = User.UserName;
                    ObjMatch.ChangedOn = DateTime.Now;
                    ObjMatch.RequirementCode = Header.RequirementCode;
                    ObjMatch.ScorePractice = Header.ScorePractice;
                    ObjMatch.TrainingGroup = Header.TrainingGroup;
                    ObjMatch.Target = Header.Target;
                    ObjMatch.TrainingGroup = Header.TrainingGroup;
                    ObjMatch.Venue = Header.Venue;
                    DBM.Entry(ObjMatch).State = EntityState.Modified;
                    int LineItem = 0;
                    foreach (var item in ListTrainee)
                    {
                        LineItem += 1;
                        HeaderTrainee = new TRTrainingEmployee();
                        HeaderTrainee.TrainNo = ObjMatch.TrainNo;
                        HeaderTrainee.LineItem = LineItem;
                        HeaderTrainee.ScoreTheory = item.ScoreTheory;
                        HeaderTrainee.ScorePractice = item.ScorePractice;
                        HeaderTrainee.CalendarID = (int)ObjMatch.CalendarID;
                        HeaderTrainee.InYear = ObjMatch.InYear;
                        HeaderTrainee.CourseID = ObjMatch.CourseID;
                        HeaderTrainee.CourseName = ObjMatch.CourseName;
                        HeaderTrainee.CourseCategoryID = ObjMatch.CourseCategoryID;
                        HeaderTrainee.CourseCategory = ObjMatch.CourseCategory;
                        HeaderTrainee.TrainingType = ObjMatch.TrainingTypeID;
                        HeaderTrainee.EmpCode = item.EmpCode;
                        Core.DB.HRStaffProfile Staff = ListStaff.FirstOrDefault(w => w.EmpCode == item.EmpCode);
                        if (Staff != null)
                        {
                            HeaderTrainee.EmpName = Staff.AllName;
                            if (!string.IsNullOrEmpty(Staff.DEPT))
                            {
                                var Dept = ListDepartment.FirstOrDefault(w => w.Code == Staff.DEPT);
                                if (Dept != null) HeaderTrainee.Department = Dept.Description;
                            }
                            if (!string.IsNullOrEmpty(Staff.JobCode))
                            {
                                var Post = ListPosition.FirstOrDefault(w => w.Code == Staff.JobCode);
                                if (Post != null) HeaderTrainee.Position = Post.Description;
                            }
                        }
                        var _GP = ListGroup.FirstOrDefault(w => w.ID ==Convert.ToInt32( Header.TrainingGroup));
                        if (_GP != null) HeaderTrainee.GroupDescription = _GP.Description;
                        HeaderTrainee.CreatedBy = User.UserName;
                        HeaderTrainee.CreatedOn = DateTime.Today;
                        HeaderTrainee.Status = SYDocumentStatus.OPEN.ToString();
                        HeaderTrainee.ReStatus = SYDocumentStatus.OPEN.ToString();
                        DBM.TRTrainingEmployees.Add(HeaderTrainee);
                    }
                    DBM.SaveChanges();
                    return SYConstant.OK;
                }
                catch (DbEntityValidationException e)
                {
                    /*------------------SaveLog----------------------------------*/
                    SYEventLog log = new SYEventLog();
                    log.ScreenId = ScreenId;
                    log.UserId = User.UserName;
                    log.DocurmentAction = id.ToString();
                    log.Action = SYActionBehavior.EDIT.ToString();

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
                    log.DocurmentAction = id.ToString();
                    log.Action = SYActionBehavior.EDIT.ToString();

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
                    log.DocurmentAction = id.ToString();
                    log.Action = SYActionBehavior.EDIT.ToString();

                    SYEventLogObject.saveEventLog(log, e, true);
                    /*----------------------------------------------------------*/
                    return "EE001";
                }
            }
            finally
            {
                DB.Configuration.AutoDetectChangesEnabled = true;
            }
        }
        public string InviteTheDoc(string IDs, string URL, string fileName)
        {
            try
            {
                if (String.IsNullOrEmpty(IDs))
                {
                    return "INV_TRAINEE";
                }

                var objStaff = DBStaff.HRStaffProfiles;
                var objTrainee = DB.TRTrainingEmployees;
                var objcal = DB.TRTrainingPlans;
                var TRTrainC = DB.TRTrainingCourses;
                List<string> ids = new List<string>(IDs.Split(','));

                List<TRTrainingEmployee> trainees = new List<TRTrainingEmployee>(objTrainee.ToList());
                trainees = trainees.Where(w => w.IsInvite != true && ids.Contains(w.TrainNo.ToString())).ToList();

                #region ---Telegram alert to Line Manager---

                var EmailTemplate = DP.TPEmailTemplates.Find("TRTraining");

                foreach (var trainee in trainees)
                {
                    List<object> ListObjectDictionary = new List<object>();
                    Core.DB.HRStaffProfile staff = new Core.DB.HRStaffProfile();

                    staff = objStaff.Find(trainee.EmpCode);
                    SYSendTelegramObject Tel = new SYSendTelegramObject();
                    Tel.User = User;
                    Tel.BS = BS;
                    TRTrainingCourse course = new TRTrainingCourse();
                    TRTrainingPlan calendar = new TRTrainingPlan();
                    calendar = objcal.Find(trainee.CalendarID);
                    course = TRTrainC.FirstOrDefault(w => w.Code == calendar.CourseID);
                    ListObjectDictionary.Add(calendar);
                    ListObjectDictionary.Add(staff);
                    ListObjectDictionary.Add(course);

                    WorkFlowResult result2 = Tel.Send_SMS_Telegram(EmailTemplate.EMTemplateObject, EmailTemplate.RequestContent, staff.TeleGroup, ListObjectDictionary, URL);
                    MessageError = Tel.getErrorMessage(result2);
                    if (String.IsNullOrEmpty(MessageError))
                    {
                        HeaderTrainee = new TRTrainingEmployee();
                        HeaderTrainee = objTrainee.FirstOrDefault(w => w.TrainNo == trainee.TrainNo);
                        HeaderTrainee.Status = "INVITED";
                        HeaderTrainee.CreatedBy = User.UserName;
                        HeaderTrainee.CreatedOn = DateTime.Now;
                        HeaderTrainee.IsInvite = true;
                        DB.Entry(HeaderTrainee).State = EntityState.Modified;
                        int row = DB.SaveChanges();
                    }
                }
                #endregion

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserID.ToString();
                log.ScreenId = HeaderTrainee.EmpCode;
                log.Action = Humica.EF.SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string requestToApprove(int id)
        {
            try
            {
                var objMatch = DB.TRTrainingInvitations.FirstOrDefault(w => w.TrainNo == id);
                if (objMatch == null)
                {
                    return "REQUEST_NE";
                }
                Header = objMatch;

                if (objMatch.Status != SYDocumentStatus.OPEN.ToString())
                {
                    return "INV_DOC";
                }

                objMatch.Status = SYDocumentStatus.PENDING.ToString();

                DB.Entry(objMatch).Property(w => w.Status).IsModified = true;
                DB.SaveChanges();

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.ScreenId = Header.TrainNo.ToString();
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
                log.DocurmentAction = Header.TrainNo.ToString();
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
                log.DocurmentAction = Header.TrainNo.ToString();
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string approveTheDoc(int id)
        {
            try
            {
                HumicaDBContext DBX = new HumicaDBContext();
                var objMatch = DB.TRTrainingInvitations.FirstOrDefault(w => w.TrainNo == id);
                if (objMatch == null)
                {
                    return "REQUEST_NE";
                }
                Header = objMatch;

                if (objMatch.Status != SYDocumentStatus.PENDING.ToString())
                {
                    return "INV_DOC";
                }
               
                var status = SYDocumentStatus.APPROVED.ToString();

                objMatch.Status = status;

                DB.Entry(objMatch).Property(w => w.Status).IsModified = true;
                DB.SaveChanges();

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.ScreenId = Header.TrainNo.ToString();
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
                log.DocurmentAction = Header.TrainNo.ToString();
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
                log.DocurmentAction = Header.TrainNo.ToString();
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string ReleaseTheDoc(int id)
        {
            HumicaDBContext DBX = new HumicaDBContext();
            try
            {
               
                var objMatch = DB.TRTrainingInvitations.FirstOrDefault(w => w.TrainNo == id);
                if (objMatch == null)
                {
                    return "REQUEST_NE";
                }
                Header = objMatch;

                if (objMatch.Status != SYDocumentStatus.APPROVED.ToString())
                {
                    return "INV_DOC";
                }
                objMatch.Status = SYDocumentStatus.RELEASED.ToString();
                var ListStaff = DB.TRTrainingEmployees.Where(w => w.TrainNo == objMatch.TrainNo).ToList();
                foreach(var item in ListStaff)
                {
                    item.Status= objMatch.Status;
                    item.ReStatus = objMatch.Status;
                    item.IsInvite = true;
                    DBX.TRTrainingEmployees.Attach(item);
                    DBX.Entry(item).Property(w => w.Status).IsModified = true;
                    DBX.Entry(item).Property(w => w.ReStatus).IsModified = true;
                    DBX.Entry(item).Property(w => w.IsInvite).IsModified = true;
                }
                DBX.TRTrainingInvitations.Attach(objMatch);
                DBX.Entry(objMatch).Property(w => w.Status).IsModified = true;
                DBX.SaveChanges();

                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.ScreenId = Header.TrainNo.ToString();
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
                log.DocurmentAction = Header.TrainNo.ToString();
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
                log.DocurmentAction = Header.TrainNo.ToString();
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
        public string CancelTheDoc(string ApprovalID)
        {
            try
            {
                string cancelled = SYDocumentStatus.CANCELLED.ToString();
                string PONumber = ApprovalID;
                var objmatch = DB.TRTrainingInvitations.Find(PONumber);
                if (objmatch.Status != cancelled)
                {
                    objmatch.Status = cancelled;
                    DB.Entry(objmatch).Property(w => w.Status).IsModified = true;
                }
                DB.SaveChanges();
               
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreenId;
                log.UserId = User.UserName;
                log.DocurmentAction = ApprovalID;
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
                log.DocurmentAction = ApprovalID;
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
                log.DocurmentAction = ApprovalID;
                log.Action = Humica.EF.SYActionBehavior.ADD.ToString();

                SYEventLogObject.saveEventLog(log, e, true);
                /*----------------------------------------------------------*/
                return "EE001";
            }
        }
    }
}