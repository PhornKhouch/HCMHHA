using Humica.Core.FT;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Training;
using Humica.Training.DB;
using Humica.Training.Setup;
using System;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.Training.Process
{
    public class NTrainingRequestController : Humica.EF.Controllers.MasterSaleController
    {
        const string SCREENID = "TRA0000001";
        const string DOC_TYPE = "NSV03";
        private const string URL_SCREEN = "/Training/Process/NTrainingRequest/";
        private string status = SYDocumentStatus.PENDING.ToString();
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREENID;
        private string ActionName = "";
        private string KeyName = "ID";
        HumicaDBContext DBX = new HumicaDBContext();
        Humica.Core.DB.HumicaDBContext DB = new Humica.Core.DB.HumicaDBContext();
        SMSystemEntity SMS = new SMSystemEntity();
        public NTrainingRequestController()
            : base()
        {
            this.ScreendIDControl = SCREENID;
            this.ScreenUrl = URL_SCREEN;
        }
        #region "List"
        public ActionResult Index()
        {
            ActionName = "Index";
            TrainningObject BSM = new TrainningObject();
            BSM.Filter = new FTDGeneralPeriod();
            UserSession();
            DataListSelector();
            UserConfList(ActionBehavior.LISTR, KeyName, "NTrainingRequest");

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (TrainningObject)Session[Index_Sess_Obj + ActionName];
                BSM.Filter = BSM.Filter;
            }

            string pending = SYDocumentStatus.PENDING.ToString();
            string received = SYDocumentStatus.RECEIVED.ToString();
            string open = SYDocumentStatus.OPEN.ToString();

            BSM.ListWaitingRItem = DBX.TRTrainingEmployees.ToList();

            BSM.ListRequestItem = DBX.TrainingRequestItem.Where(w => !(w.ReStatus == pending || w.ReStatus == open || w.ReStatus == received) || (w.Status != open && w.IsNCXRead == false)).ToList();
            if (BSM.Filter.Status != null)
            {
                BSM.ListRequestItem = DBX.TrainingRequestItem.Where(w => w.ReStatus == BSM.Filter.Status).ToList();
            }
            foreach (var r in BSM.ListRequestItem)
            {
                r.IsNCXRead = true;
                DBX.TrainingRequestItem.Attach(r);
                DBX.Entry(r).Property(w => w.IsNCXRead).IsModified = true;
            }
            DBX.SaveChanges();
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        [HttpPost]
        public ActionResult Index(TrainningObject BSM)
        {
            ActionName = "Index";
            UserSession();
            DataListSelector();
            //BSM.Filter.Status = SYDocumentStatus.PENDING.ToString();
            UserConfList(ActionBehavior.LISTR, "ID", "NTrainingRequest");

            string pending = SYDocumentStatus.PENDING.ToString();
            string received = SYDocumentStatus.RECEIVED.ToString();
            string open = SYDocumentStatus.OPEN.ToString();

            BSM.ListWaitingRequestItem = DBX.TrainingRequestItem.Where(w => (w.ReStatus == pending || w.ReStatus == received)).ToList();

            BSM.ListRequestItem = DBX.TrainingRequestItem.Where(w => !(w.ReStatus == pending || w.ReStatus == open || w.ReStatus == received) || (w.Status != open && w.IsNCXRead == false)).ToList();
            if (BSM.Filter.Status != null)
            {
                BSM.ListRequestItem = DBX.TrainingRequestItem.Where(w => w.ReStatus == BSM.Filter.Status).ToList();
            }
            foreach (var r in BSM.ListRequestItem)
            {
                r.IsNCXRead = true;
                DBX.TrainingRequestItem.Attach(r);
                DBX.Entry(r).Property(w => w.IsNCXRead).IsModified = true;
            }
            DBX.SaveChanges();
            return View(BSM);
        }
        public ActionResult PartialList()
        {
            ActionName = "Index";
            TrainningObject BSM = new TrainningObject();
            UserSession();
            DataListSelector();
            UserConfList(ActionBehavior.LISTR, "ID", "NTrainingRequest");
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (TrainningObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("PartialList", BSM.ListWaitingRequestItem);
        }
        public ActionResult PartialList1()
        {
            ActionName = "Index";
            TrainningObject BSM = new TrainningObject();
            UserSession();
            DataListSelector();
            UserConfList(ActionBehavior.LISTR, "ID", "NTrainingRequest");
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (TrainningObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("PartialList1", BSM.ListRequestItem);
        }
        #endregion

        #region Detials
        public ActionResult Details(string id, string staffid)
        {
            ActionName = "Details";
            this.UserSession();
            UserConfList(ActionBehavior.LIST_ADD, "ID", "NTrainingRequest");
            DataListSelector();
            TrainningObject BSD = new TrainningObject();
            string OPEN = SYDocumentStatus.OPEN.ToString();
            BSD.RequestHeader = DBX.TrainingRequestHeader.SingleOrDefault(w => w.RequestCode == id);
            if (BSD.RequestHeader != null)
            {
                BSD.ListRequestItem = DBX.TrainingRequestItem.Where(w => w.RequestCode == BSD.RequestHeader.RequestCode && w.StaffID == staffid).ToList();
                ViewData[SYConstant.PARAM_ID] = id + "," + staffid;
                var Mont = DateTime.Now.Month;
                Session[Index_Sess_Obj + ActionName] = BSD;
                return View(BSD);
            }
            else
            {
                Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            }
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        [HttpPost]
        public ActionResult Details(TrainningObject collection, string id, string staffid)
        {
            ActionName = "Details";
            this.UserSession();
            UserConfList(ActionBehavior.LIST_ADD, "ID", "NTrainingRequest");
            DataListSelector();
            TrainningObject BSD = new TrainningObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSD = (TrainningObject)Session[Index_Sess_Obj + ActionName];
            }
            ViewData[SYConstant.PARAM_ID] = id + "," + staffid;
            var Mont = DateTime.Now.Month;
            BSD.RequestHeader = collection.RequestHeader;
            BSD.Group = collection.Group;
            string received = SYDocumentStatus.RECEIVED.ToString();
            string pending = SYDocumentStatus.PENDING.ToString();

            string sms = "";
            BSD.RequestHeader = DBX.TrainingRequestHeader.FirstOrDefault(w => w.RequestCode == id);
            if (BSD.RequestHeader != null)
            {
                var lst = DBX.TrainingRequestItem.Where(w => w.RequestCode == BSD.RequestHeader.RequestCode && w.StaffID == staffid && w.Status == received && w.ReStatus == pending).ToList();
                if (lst.Count == 0)
                {
                    Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
                    return View(BSD);
                }
                foreach (var r1 in BSD.ListRequestItem)
                {
                    lst.First().Group = BSD.Group;
                    //lst.First().Status = SYDocumentStatus.APPROVED.ToString();
                    lst.First().ReStatus = SYDocumentStatus.APPROVED.ToString();
                    lst.First().MGStatus = SYDocumentStatus.PENDING.ToString();
                    lst.First().Comment = r1.Comment;
                    DBX.TrainingRequestItem.Attach(lst.First());
                    DBX.Entry(lst.First()).Property(w => w.ReStatus).IsModified = true;
                    DBX.Entry(lst.First()).Property(w => w.MGStatus).IsModified = true;
                    DBX.Entry(lst.First()).Property(w => w.Group).IsModified = true;
                    DBX.Entry(lst.First()).Property(w => w.Comment).IsModified = true;
                }
                //BSD.RequestHeader.Status = SYDocumentStatus.APPROVED.ToString();
                DBX.TrainingRequestHeader.Attach(BSD.RequestHeader);
                DBX.Entry(BSD.RequestHeader).Property(w => w.Status).IsModified = true;
                DBX.SaveChanges();
                sms = SYSConstant.OK;
            }


            if (sms == "OK")
            {
                SYMessages mess_err = SYMessages.getMessageObject("APPROVED_D", user.Lang);
                Session[Index_Sess_Obj + this.ActionName] = BSD;
                UserConfForm(ActionBehavior.RELEASE);
                Session[SYConstant.MESSAGE_SUBMIT] = mess_err;
                return View(BSD);
            }
            else
            {
                Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(sms, user.Lang);
            }
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        public ActionResult GridItemDetails()
        {
            ActionName = "Details";
            UserSession();
            DataListSelector();
            UserConfListAndForm();
            TrainningObject BSM = new TrainningObject();
            //BSM.ListItems = new List<SPPOItem>();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (TrainningObject)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("GridItemDetails", BSM.ListRequestItem);
        }
        public ActionResult EditItems(TrainingRequestItem obj)
        {
            ActionName = "Details";
            UserSession();
            DataSelectorForFilter();
            UserConfListAndForm();
            TrainningObject BSM = new TrainningObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (TrainningObject)Session[Index_Sess_Obj + ActionName];
            }
            var objCheck = BSM.ListRequestItem.Where(w => w.LineItem == obj.LineItem).ToList();

            if (objCheck.Count > 0)
            {
                objCheck.First().Comment = obj.Comment;

            }
            else
            {
                ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("GridItemDetails", BSM.ListRequestItem);
        }
        #endregion

        #region Receive
        public ActionResult Receive(string id)
        {
            ActionName = "Details";
            UserSession();
            UserConfListAndForm();
            TrainningObject BSD = new TrainningObject();
            if (id != null)
            {
                string sms = "";
                string received = SYDocumentStatus.RECEIVED.ToString();
                string pending = SYDocumentStatus.PENDING.ToString();

                string[] code = id.Split(';');
                foreach (var r in code)
                {
                    if (r != "")
                    {
                        string[] datas = r.Split(',');
                        string rqc = datas[0].ToString();
                        string staff = datas[1].ToString();
                        //int gp = Convert.ToInt32(Session["Group"].ToString());

                        BSD.RequestHeader = DBX.TrainingRequestHeader.FirstOrDefault(w => w.RequestCode == rqc);
                        if (BSD.RequestHeader != null)
                        {
                            BSD.ListRequestItem = DBX.TrainingRequestItem.Where(w => w.RequestCode == BSD.RequestHeader.RequestCode && w.StaffID == staff && w.Status == pending && w.ReStatus == pending).ToList();
                            foreach (var r1 in BSD.ListRequestItem)
                            {
                                r1.IsDLRRead = false;
                                r1.Status = SYDocumentStatus.RECEIVED.ToString();
                                r1.ReStatus = SYDocumentStatus.RECEIVED.ToString();
                                DBX.TrainingRequestItem.Attach(r1);
                                DBX.Entry(r1).Property(w => w.ReStatus).IsModified = true;
                                DBX.Entry(r1).Property(w => w.Status).IsModified = true;
                                DBX.Entry(r1).Property(w => w.IsDLRRead).IsModified = true;
                            }
                            BSD.RequestHeader.Status = SYDocumentStatus.RECEIVED.ToString();
                            DBX.TrainingRequestHeader.Attach(BSD.RequestHeader);
                            DBX.Entry(BSD.RequestHeader).Property(w => w.Status).IsModified = true;

                        }
                    }

                }

                DBX.SaveChanges();
                sms = SYSConstant.OK;
                if (sms == "OK")
                {
                    SYMessages mess_err = SYMessages.getMessageObject("RECEIVE_D", user.Lang);
                    Session[Index_Sess_Obj + this.ActionName] = BSD;
                    UserConfForm(ActionBehavior.RELEASE);
                    Session[SYConstant.MESSAGE_SUBMIT] = mess_err;
                }
                else
                {
                    Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(sms, user.Lang);
                }
            }
            else
            {
                Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            }
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }

        #endregion

        #region RequestToRelease
        public ActionResult RequestTrain(string id)
        {
            ActionName = "Details";
            UserSession();
            UserConfListAndForm();
            string sms = "";
            string received = SYDocumentStatus.RECEIVED.ToString();
            TrainningObject BSD = new TrainningObject();
            if (id != null)
            {
                string[] code = id.Split(';');
                foreach (var r in code)
                {
                    if (r != "")
                    {
                        string[] datas = r.Split(',');
                        string rqc = datas[0].ToString();
                        string staff = datas[1].ToString();
                        //int gp = Convert.ToInt32(Session["Group"].ToString());

                        BSD.RequestHeader = DBX.TrainingRequestHeader.FirstOrDefault(w => w.RequestCode == rqc);
                        if (BSD.RequestHeader != null)
                        {
                            BSD.ListRequestItem = DBX.TrainingRequestItem.Where(w => w.RequestCode == BSD.RequestHeader.RequestCode && w.StaffID == staff && w.Status == received).ToList();
                            foreach (var r1 in BSD.ListRequestItem)
                            {
                                //r1.Group = gp;
                                r1.ReStatus = SYDocumentStatus.PENDING.ToString();
                                DBX.TrainingRequestItem.Attach(r1);
                                DBX.Entry(r1).Property(w => w.ReStatus).IsModified = true;
                                //DBX.Entry(r1).Property(w => w.Group).IsModified = true;
                            }
                            BSD.RequestHeader.ReStatus = SYDocumentStatus.PENDING.ToString();
                            BSD.RequestHeader.ChangedBy = user.UserName;
                            BSD.RequestHeader.ChangedOn = DateTime.Now;
                            DBX.TrainingRequestHeader.Attach(BSD.RequestHeader);
                            DBX.Entry(BSD.RequestHeader).Property(w => w.ReStatus).IsModified = true;
                            DBX.Entry(BSD.RequestHeader).Property(w => w.ChangedBy).IsModified = true;
                            DBX.Entry(BSD.RequestHeader).Property(w => w.ChangedOn).IsModified = true;

                        }
                    }

                }
                DBX.SaveChanges();
                sms = SYSConstant.OK;
                if (sms == "OK")
                {
                    SYMessages mess_err = SYMessages.getMessageObject("PENDING", user.Lang);
                    Session[Index_Sess_Obj + this.ActionName] = BSD;
                    UserConfForm(ActionBehavior.RELEASE);
                    Session[SYConstant.MESSAGE_SUBMIT] = mess_err;
                }
                else
                {
                    Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(sms, user.Lang);
                }
            }
            else
            {
                Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            }
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }

        #endregion

        #region Approve
        public ActionResult Approve(string id)
        {
            ActionName = "Details";
            UserSession();
            UserConfListAndForm();
            TrainningObject BSD = new TrainningObject();
            if (id != null)
            {
                string sms = "";
                string received = SYDocumentStatus.RECEIVED.ToString();
                string pending = SYDocumentStatus.PENDING.ToString();
                string[] code = id.Split(';');
                foreach (var r in code)
                {
                    if (r != "")
                    {
                        string[] datas = r.Split(',');
                        string rqc = datas[0].ToString();
                        string staff = datas[1].ToString();

                        int gp = 0;
                        if (Session["Group"] != null)
                        {
                            gp = Convert.ToInt32(Session["Group"].ToString());
                        }

                        var liststaff = DB.HRStaffProfiles.ToList();

                        BSD.RequestHeader = DBX.TrainingRequestHeader.FirstOrDefault(w => w.RequestCode == rqc);
                        if (BSD.RequestHeader != null)
                        {
                            BSD.ListRequestItem = DBX.TrainingRequestItem.Where(w => w.RequestCode == BSD.RequestHeader.RequestCode && w.StaffID == staff && w.Status == received && w.ReStatus == received).ToList();
                            foreach (var r1 in BSD.ListRequestItem)
                            {
                                r1.Group = gp;
                                //r1.Status = SYDocumentStatus.APPROVED.ToString();
                                r1.ReStatus = SYDocumentStatus.APPROVED.ToString();
                                r1.MGStatus = SYDocumentStatus.PENDING.ToString();
                                r1.IsDLRRead = false;
                                DBX.TrainingRequestItem.Attach(r1);
                                DBX.Entry(r1).Property(w => w.ReStatus).IsModified = true;
                                DBX.Entry(r1).Property(w => w.MGStatus).IsModified = true;
                                DBX.Entry(r1).Property(w => w.Group).IsModified = true;
                                DBX.Entry(r1).Property(w => w.IsDLRRead).IsModified = true;
                            }

                            var citem = DBX.TrainingCourseItem.ToList();

                            int l = 1;
                            l = citem.Where(w => w.Program == BSD.RequestHeader.Program && w.Course == BSD.RequestHeader.Course && w.InvCode == BSD.RequestHeader.InvCode).ToList().Count + 1;
                            foreach (var r2 in BSD.ListRequestItem)
                            {
                                var chstafff = liststaff.Where(w => w.EmpCode == r2.StaffID).ToList();
                                if (chstafff.Count > 0)
                                {
                                    string sfID = chstafff.First().EmpCode.ToString();
                                    var ch = citem.Where(w => w.Program == r2.Program && w.InvCode == r2.InvCode && w.Course == r2.Course && w.StaffID == sfID).ToList();
                                    if (ch.Count > 0)
                                    {

                                        if (r2.ChangeStaffID != null)
                                        {
                                            var chhstaff = liststaff.Where(w => w.EmpCode == r2.ChangeStaffID).ToList();
                                            sfID = chhstaff.First().EmpCode.ToString();
                                            ch.First().StaffID = sfID;
                                            ch.First().StaffLevel = r2.StaffLevel;
                                            ch.First().StaffName = r2.StaffName;
                                            ch.First().Gender = r2.Gender;
                                            ch.First().Position = r2.Position;
                                            ch.First().NatID = r2.NatID;
                                            ch.First().PhoneNo = r2.PhoneNo;
                                            ch.First().WorkExperience = r2.WorkExperience;
                                            ch.First().Description = r2.Description;
                                            ch.First().Comment = r2.Comment;
                                            DBX.TrainingCourseItem.Attach(ch.First());
                                            DBX.Entry(ch.First()).Property(w => w.StaffID).IsModified = true;
                                            DBX.Entry(ch.First()).Property(w => w.StaffLevel).IsModified = true;
                                            DBX.Entry(ch.First()).Property(w => w.StaffName).IsModified = true;
                                            DBX.Entry(ch.First()).Property(w => w.Gender).IsModified = true;
                                            DBX.Entry(ch.First()).Property(w => w.Position).IsModified = true;
                                            DBX.Entry(ch.First()).Property(w => w.NatID).IsModified = true;
                                            DBX.Entry(ch.First()).Property(w => w.PhoneNo).IsModified = true;
                                            DBX.Entry(ch.First()).Property(w => w.WorkExperience).IsModified = true;
                                            DBX.Entry(ch.First()).Property(w => w.Description).IsModified = true;
                                            DBX.Entry(ch.First()).Property(w => w.Comment).IsModified = true;
                                        }
                                        else
                                        {
                                            var course = citem.Where(w => w.Program == r2.Program && w.InvCode == r2.InvCode && w.Course == r2.Course).ToList();
                                            if (course.Count > 0)
                                            {
                                                var obj = new TrainingCourseItem();
                                                obj.LineItem = l;
                                                obj.Course = course.First().Course;
                                                obj.Program = course.First().Program;
                                                obj.Status = course.First().Status;
                                                obj.InvCode = course.First().InvCode;
                                                obj.Type = course.First().Type;
                                                obj.Group = course.First().Group;
                                                if (gp > 0)
                                                {
                                                    obj.Group = gp;
                                                }

                                                obj.StaffLevel = r2.StaffLevel;
                                                obj.StaffID = sfID;
                                                obj.StaffName = r2.StaffName;
                                                obj.Gender = r2.Gender;
                                                obj.Position = r2.Position;
                                                obj.NatID = r2.NatID;
                                                obj.PhoneNo = r2.PhoneNo;
                                                obj.WorkExperience = r2.WorkExperience;
                                                obj.Description = r2.Description;
                                                obj.Comment = r2.Comment;
                                                DBX.TrainingCourseItem.Add(obj);
                                                l++;
                                            }
                                        }

                                    }
                                    else
                                    {
                                        var course = citem.Where(w => w.Program == r2.Program && w.InvCode == r2.InvCode && w.Course == r2.Course).ToList();
                                        if (course.Count > 0)
                                        {
                                            var obj = new TrainingCourseItem();
                                            obj.LineItem = l;
                                            obj.Course = course.First().Course;
                                            obj.Program = course.First().Program;
                                            obj.Status = course.First().Status;
                                            obj.InvCode = course.First().InvCode;
                                            obj.Type = course.First().Type;
                                            obj.Group = course.First().Group;
                                            if (gp > 0)
                                            {
                                                obj.Group = gp;
                                            }
                                            obj.StaffLevel = r2.StaffLevel;
                                            obj.StaffID = sfID;
                                            obj.StaffName = r2.StaffName;
                                            obj.Gender = r2.Gender;
                                            obj.Position = r2.Position;
                                            obj.NatID = r2.NatID;
                                            obj.PhoneNo = r2.PhoneNo;
                                            obj.WorkExperience = r2.WorkExperience;
                                            obj.Description = r2.Description;
                                            obj.Comment = r2.Comment;
                                            DBX.TrainingCourseItem.Add(obj);
                                            l++;
                                        }

                                    }
                                }


                            }
                            //BSD.RequestHeader.Status = SYDocumentStatus.APPROVED.ToString();
                            BSD.RequestHeader.ChangedBy = user.UserName;
                            BSD.RequestHeader.ChangedOn = DateTime.Now;
                            DBX.TrainingRequestHeader.Attach(BSD.RequestHeader);
                            DBX.Entry(BSD.RequestHeader).Property(w => w.Status).IsModified = true;
                            DBX.Entry(BSD.RequestHeader).Property(w => w.ChangedBy).IsModified = true;
                            DBX.Entry(BSD.RequestHeader).Property(w => w.ChangedOn).IsModified = true;
                            DBX.SaveChanges();
                            sms = SYSConstant.OK;
                        }

                    }

                }


                if (sms == "OK")
                {
                    SYMessages mess_err = SYMessages.getMessageObject("APPROVED_D", user.Lang);
                    Session[Index_Sess_Obj + this.ActionName] = BSD;
                    UserConfForm(ActionBehavior.RELEASE);
                    Session[SYConstant.MESSAGE_SUBMIT] = mess_err;
                }
                else
                {
                    Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(sms, user.Lang);
                }
            }
            else
            {
                Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            }
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }

        #endregion

        #region "Reject"
        public ActionResult Reject(string id)
        {

            UserSession();
            TrainningObject BSM = new TrainningObject();
            if (id != null)
            {
                string received = SYDocumentStatus.RECEIVED.ToString();

                var reason = ""; var comment = ""; DateTime CancelDate = DateTime.Now.Date;
                if (Session[Index_Sess_Obj + id] != null)
                {
                    var objReason = (ClsReason)Session[Index_Sess_Obj + id];
                    reason = objReason.ReasonCode;
                    comment = objReason.Comment;
                    CancelDate = objReason.ReasonDate;
                    objReason = null;
                }
                string msg = "";
                string[] code = id.Split(';');
                foreach (var r in code)
                {
                    if (r != "")
                    {
                        string[] datas = r.Split(',');
                        string rqc = datas[0].ToString();
                        string staff = datas[1].ToString();

                        BSM.RequestHeader = DBX.TrainingRequestHeader.FirstOrDefault(w => w.RequestCode == rqc);
                        if (BSM.RequestHeader != null)
                        {
                            var liststaff = DB.HRStaffProfiles.ToList();

                            BSM.ListRequestItem = DBX.TrainingRequestItem.Where(w => w.RequestCode == BSM.RequestHeader.RequestCode && w.Status == received || w.Status == received && w.StaffID == staff).ToList();
                            foreach (var r1 in BSM.ListRequestItem)
                            {
                                r1.Status = SYDocumentStatus.REJECTED.ToString();
                                r1.ReStatus = SYDocumentStatus.REJECTED.ToString();
                                r1.Comment = comment;
                                r1.IsDLRRead = false;
                                DBX.TrainingRequestItem.Attach(r1);
                                DBX.Entry(r1).Property(w => w.Status).IsModified = true;
                                DBX.Entry(r1).Property(w => w.ReStatus).IsModified = true;
                                DBX.Entry(r1).Property(w => w.Comment).IsModified = true;
                                DBX.Entry(r1).Property(w => w.IsDLRRead).IsModified = true;
                            }


                            var citem = DBX.TrainingCourseItem.ToList();

                            int l = 1;
                            l = citem.Where(w => w.Program == BSM.RequestHeader.Program && w.Course == BSM.RequestHeader.Course && w.InvCode == BSM.RequestHeader.InvCode).ToList().Count + 1;
                            foreach (var r2 in BSM.ListRequestItem)
                            {
                                var course = citem.Where(w => w.Program == r2.Program && w.InvCode == r2.InvCode && w.Course == r2.Course).ToList();
                                if (course.Count > 0)
                                {
                                    if (r2.ChangeStaffID != null)
                                    {

                                        var chstafff = liststaff.Where(w => w.EmpCode == r2.ChangeStaffID).ToList();
                                        if (chstafff.Count > 0)
                                        {
                                            string sfID = chstafff.First().EmpCode.ToString();
                                            var obj = new TrainingCourseItem();
                                            obj.LineItem = l;
                                            obj.Course = course.First().Course;
                                            obj.Program = course.First().Program;
                                            obj.Status = SYDocumentStatus.REJECTED.ToString();
                                            obj.InvCode = course.First().InvCode;
                                            obj.Type = course.First().Type;
                                            obj.Group = course.First().Group;
                                            obj.StaffLevel = r2.StaffLevel;
                                            obj.StaffID = sfID;
                                            obj.StaffName = r2.StaffName;
                                            obj.Gender = r2.Gender;
                                            obj.Position = r2.Position;
                                            obj.NatID = r2.NatID;
                                            obj.PhoneNo = r2.PhoneNo;
                                            obj.WorkExperience = r2.WorkExperience;
                                            obj.Description = r2.Description;
                                            obj.Comment = r2.Comment;
                                            DBX.TrainingCourseItem.Add(obj);
                                            l++;

                                        }
                                    }
                                    else
                                    {
                                        var chstafff = liststaff.Where(w => w.EmpCode == r2.StaffID).ToList();
                                        if (chstafff.Count > 0)
                                        {
                                            string sfID = chstafff.First().EmpCode.ToString();
                                            var obj = new TrainingCourseItem();
                                            obj.LineItem = l;
                                            obj.Course = course.First().Course;
                                            obj.Program = course.First().Program;
                                            obj.Status = SYDocumentStatus.REJECTED.ToString();
                                            obj.InvCode = course.First().InvCode;
                                            obj.Type = course.First().Type;
                                            obj.Group = course.First().Group;
                                            obj.StaffLevel = r2.StaffLevel;
                                            obj.StaffID = sfID;
                                            obj.StaffName = r2.StaffName;
                                            obj.Gender = r2.Gender;
                                            obj.Position = r2.Position;
                                            obj.NatID = r2.NatID;
                                            obj.PhoneNo = r2.PhoneNo;
                                            obj.WorkExperience = r2.WorkExperience;
                                            obj.Description = r2.Description;
                                            obj.Comment = r2.Comment;
                                            DBX.TrainingCourseItem.Add(obj);
                                            l++;

                                        }
                                    }
                                }




                            }
                            BSM.RequestHeader.Status = SYDocumentStatus.REJECTED.ToString();
                            BSM.RequestHeader.Reason = reason;
                            BSM.RequestHeader.Comment = comment;
                            BSM.RequestHeader.ChangedBy = user.UserName;
                            BSM.RequestHeader.ChangedOn = DateTime.Now;
                            DBX.TrainingRequestHeader.Attach(BSM.RequestHeader);
                            DBX.Entry(BSM.RequestHeader).Property(w => w.Status).IsModified = true;
                            DBX.Entry(BSM.RequestHeader).Property(w => w.Reason).IsModified = true;
                            DBX.Entry(BSM.RequestHeader).Property(w => w.Comment).IsModified = true;
                            DBX.Entry(BSM.RequestHeader).Property(w => w.ChangedBy).IsModified = true;
                            DBX.Entry(BSM.RequestHeader).Property(w => w.ChangedOn).IsModified = true;
                            DBX.SaveChanges();
                            msg = SYSConstant.OK;
                        }
                    }

                }

                if (msg == SYConstant.OK)
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_CANCELLED", user.Lang);
                }
                else
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                }
                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

            }
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        #endregion

        #region "Reason"
        [HttpPost]
        public ActionResult ReasonCancel(string id)
        {
            var obj = new ClsReason();
            if (Request.Form["Reason"] != null)
            {
                obj.ReasonCode = Request.Form["Reason"].ToString();
            }

            if (Request.Form["Comment"] != null)
            {
                obj.Comment = Request.Form["Comment"].ToString();
            }

            var objReason = DB.CFReasonCodes.Find(obj.ReasonCode);
            if (objReason == null)
            {
                Session[Index_Sess_Obj + id] = null;
                var result = new
                {
                    MS = SYMessages.getMessage("REASON_NE"),
                };
                return Json(result, JsonRequestBehavior.DenyGet);
            }
            else
            {
                Session[Index_Sess_Obj + id] = obj;
                var result = new
                {
                    MS = SYConstant.OK,
                };
                return Json(result, JsonRequestBehavior.DenyGet);
            }
        }
        #endregion

        #region "Private Code"
        private void DataSelectorForFilter()
        {

        }
        private void DataListSelector()
        {
            SYDataList DL = new SYDataList("YEAR_SELECT");
            ViewData["YEAR_SELECT"] = DL.ListData;

            DL = new SYDataList("STATUS_APPROVAL");
            ViewData["STATUS_APPROVAL"] = DL.ListData;

            //ViewData["DEALER_LIST"] = MDDMaterialObject.DealerList().ToList();
            DL = new SYDataList("QT_RELEASE_TYPE");
            ViewData["QT_RELEASE_TYPE"] = DL.ListData;
            DL = new SYDataList("CALENDAR_TYPE");
            ViewData["CALENDAR_TYPE"] = DL.ListData;
            DL = new SYDataList("MONTH_SELECT");
            ViewData["MONTH_SELECT"] = DL.ListData;
            DL = new SYDataList("STATUS_APPROVAL");
            ViewData["STATUS_APPROVAL"] = DL.ListData;
            DL = new SYDataList("REPORT_TYPE");
            ViewData["REPORT_TYPE"] = DL.ListData;
            DL = new SYDataList("KI");
            ViewData["YEAR_KI_SELECT"] = DL.ListData;
            DL = new SYDataList("KI_QUATER");
            ViewData["QUATER_KI_SELECT"] = DL.ListData;

            ViewData["REQUEST_SELECT"] = new SYDataList("REQUEST_SELECT").ListData;
            DL = new SYDataList("STAFF_POSITION");
            ViewData["STAFF_POSITION"] = DL.ListData;
            var listReason = DB.CFReasonCodes.Where(w => w.Indicator == "H").ToList();
            foreach (var read in listReason)
            {
                read.Description1 = read.Description2 + "-" + read.Description1;
            }
            ViewData["REASON_CANCEL"] = listReason;
            //ViewData["Course_List"] = DBX.TrainingCourse.ToList();
            ViewData["Program_List"] = DBX.TrainingProgram.ToList();
            ViewData["StaffLevel_List"] = SMS.HRLevels.ToList();
            ViewData["Target_List"] = DBX.TRTrainingRequirements.Where(w => w.Category == "T").ToList();
            ViewData["Venue_List"] = DBX.TRTrainingRequirements.Where(w => w.Category == "V").ToList();
            ViewData["Dayterm_List"] = DBX.TRTrainingRequirements.Where(w => w.Category == "D").ToList();
            ViewData["Group_List"] = DBX.TRTrainingRequirements.Where(w => w.Category == "G").ToList();
            ViewData["Requirement_List"] = DBX.TRTrainingRequirements.Where(w => w.Category == "R").ToList();
            ViewData["Staff_List"] = DB.HRStaffProfiles.ToList();

            ViewData["Course_SELECT"] = DBX.TRTrainingCourses.ToList();
            ViewData["TRAINING_TYPE"] = DBX.TRTrainingTypes.ToList();
        }
        #endregion

        #region "Report"
        //public ActionResult ReportSummary()
        //{
        //    UserSession();
        //    DataListSelector();
        //    ActionName = "Rpt";
        //    UserConfListAndForm();
        //    ServiceDemandObject BSM = new ServiceDemandObject();
        //    BSM.Filter = new Models.FT.FTDGeneralPeriod();
        //    BSM.Filter.Year = DateTime.Now.Year;
        //    BSM.Filter.Status = Humica.EF.SYDocumentStatus.OPEN.ToString();
        //    BSM.Filter.Period = PeriodBy.J.ToString();
        //    BSM.Filter.ReportType = SYReportType.PIVOT.ToString();
        //    var yearki = new SYDataList("KI").ListData;
        //    string year = BSM.Filter.Year.ToString();
        //    var ki = "";

        //    UserConfReportPartial(BSM.Filter.ReportType,
        //    this.ControllerContext.RouteData.Values["action"].ToString() + "Partail");

        //    if (yearki.Where(w => w.SelectValue == year).ToList().Count > 0)
        //    {
        //        ki = yearki.Where(w => w.SelectValue == year).First().SelectText;
        //    }
        //    BSM.Filter.Ki = ki;
        //    BSM.ListItems = DBX.SVServiceDemandItems.Where(w => w.Ki == ki
        //       && w.Status == BSM.Filter.Status
        //       ).ToList();
        //    Session[Index_Sess_Obj + ActionName] = BSM;
        //    return View(BSM);
        //}
        //[HttpPost]
        //public ActionResult ReportSummary(ServiceDemandObject BSM)
        //{
        //    UserSession();
        //    DataListSelector();
        //    ActionName = "Rpt";
        //    UserConfListAndForm();
        //    BSM.DocumentType = DOC_TYPE;
        //    UserConfListAndForm();
        //    UserConfReportPartial(BSM.Filter.ReportType,
        //    this.ControllerContext.RouteData.Values["action"].ToString() + "Partail");
        //    if (BSM.Filter.Period == PeriodBy.J.ToString())
        //    {
        //        if (BSM.Filter.Status == null)
        //        {
        //            BSM.ListItems = DBX.SVServiceDemandItems.Where(w => w.Ki == BSM.Filter.Ki
        //        ).ToList();
        //        }
        //        else
        //        {
        //            BSM.ListItems = DBX.SVServiceDemandItems.Where(w => w.Ki == BSM.Filter.Ki
        //                && w.Status == BSM.Filter.Status
        //                ).ToList();
        //        }
        //    }
        //    else
        //    {
        //        if (BSM.Filter.Status == null)
        //        {
        //            BSM.ListItems = DBX.SVServiceDemandItems.Where(w => w.Year == BSM.Filter.Year

        //       ).ToList();
        //        }
        //        else
        //        {
        //            BSM.ListItems = DBX.SVServiceDemandItems.Where(w => w.Year == BSM.Filter.Year
        //       && w.Status == BSM.Filter.Status
        //       ).ToList();
        //        }
        //    }
        //    Session[Index_Sess_Obj + ActionName] = BSM;
        //    return View(BSM);
        //}

        //public ActionResult ReportSummaryPartail()
        //{
        //    UserSession();
        //    this.ActionName = "Rpt";
        //    ServiceDemandObject BSM = new ServiceDemandObject();
        //    UserSession();
        //    UserConfListAndForm();

        //    if (Session[Index_Sess_Obj + ActionName] != null)
        //    {
        //        BSM = (ServiceDemandObject)Session[Index_Sess_Obj + ActionName];
        //    }
        //    UserConfReportPartial(BSM.Filter.ReportType,
        //    this.ControllerContext.RouteData.Values["action"].ToString() + "Partail");
        //    if (BSM.Filter.ReportType == SYReportType.PIVOT.ToString())
        //    {
        //        return PartialView(Humica.EF.Models.SY.SYListConfuration.ListPivotSelectView, BSM.ListItems);
        //    }
        //    else
        //    {
        //        return PartialView(Humica.EF.Models.SY.SYListConfuration.ListRowSelectView, BSM.ListItems);
        //    }
        //}
        #endregion


    }
}