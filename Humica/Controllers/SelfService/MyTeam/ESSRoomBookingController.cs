using Humica.Core.DB;
using Humica.EF;
using Humica.EF.Models.SY;
using Humica.Logic.HR;
using Humica.Models.SY;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.SelfService.MyTeam
{

    public class ESSRoomBookingController : Humica.EF.Controllers.MasterSaleController
    {

        private const string SCREEN_ID = "ESS0000014";
        private const string URL_SCREEN = "/SelfService/MyTeam/ESSRoomBooking/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "BookingNo";
        HumicaDBContext DB = new HumicaDBContext();

        public ESSRoomBookingController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
        }


        #region "List"
        public ActionResult Index()
        {
            ActionName = "Index";
            UserSession();
            UserConfList(this.KeyName);
            DataSelector();

            BookingRoomObject BSM = new BookingRoomObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (BookingRoomObject)Session[Index_Sess_Obj + ActionName];
            }
            BSM.ListHeader = new List<HRBookingRoom>();
            var ListHeader = DB.HRBookingRooms.ToList();
            ListHeader = ListHeader.OrderByDescending(x => x.DocumentDate).ToList();
            BSM.ListHeader = ListHeader.ToList();

            Session[Index_Sess_Obj + ActionName] = BSM;

            return View(BSM);
        }
        [HttpPost]
        public ActionResult Index(BookingRoomObject collection)
        {
            ActionName = "Index";
            UserSession();
            UserConfList(this.KeyName);
            DataSelector();
            BookingRoomObject BSM = new BookingRoomObject();
            BSM.ListHeader = new List<HRBookingRoom>();
            var ListHeader = DB.HRBookingRooms.ToList();
            ListHeader = ListHeader.OrderByDescending(x => x.DocumentDate).ToList();
            BSM.ListHeader = ListHeader.ToList();

            Session[Index_Sess_Obj + ActionName] = BSM;

            return View(BSM);
        }
        public ActionResult PartialList()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            BookingRoomObject BSM = new BookingRoomObject();
            BSM.ListHeader = new List<HRBookingRoom>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (BookingRoomObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView(SYListConfuration.ListDefaultView, BSM.ListHeader);
        }
        #endregion

        #region "Create"
        public ActionResult Create()
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm(this.KeyName);
            DataSelector();
            BookingRoomObject BSM = new BookingRoomObject();
            BSM.ListBookingSchedule = new List<HRBookingSchedule>();
            BSM.Header = new HRBookingRoom();
            BSM.Header.DocumentDate = DateTime.Now;
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        [HttpPost]
        public ActionResult Create(string ID, BookingRoomObject collection)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = ID;
            var BSM = new BookingRoomObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (BookingRoomObject)Session[Index_Sess_Obj + ActionName];
                BSM.Header = collection.Header;
            }
            if (ModelState.IsValid)
            {
                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.createBooking();

                if (msg == SYConstant.OK)
                {
                    SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                    mess.DocumentNumber = BSM.Header.BookingNo.ToString();
                    mess.Description = mess.Description + BSM.MessageError;
                    mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details/" + mess.DocumentNumber;
                    ViewData[SYConstant.MESSAGE_SUBMIT] = mess;
                    BSM.Header = new HRBookingRoom();
                    BSM.Header.DocumentDate = DateTime.Now;
                    BSM.ListBookingSchedule = new List<HRBookingSchedule>();
                    return View(BSM);
                }
                else
                {
                    ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                    Session[Index_Sess_Obj + ActionName] = BSM;
                    return View(BSM);
                }
            }
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            return View(BSM);
        }
        #endregion

        //#region "Edit"
        //public ActionResult Edit(string id)
        //{
        //    ActionName = "Edit";
        //    UserSession();
        //    DataSelector();
        //    UserConfForm(SYActionBehavior.EDIT);
        //    ViewData[SYSConstant.PARAM_ID] = id;
        //    BookingRoomObject BSM = new BookingRoomObject();
        //    BSM.HeaderEmpLeave = new HREmpLeave();
        //    BSM.HeaderStaff = new HR_STAFF_VIEW();
        //    //if (Session[Index_Sess_Obj + ActionName] != null)
        //    //{
        //    //    BSM = (BookingRoomObject)Session[Index_Sess_Obj + ActionName];
        //    //}
        //    string re = id;
        //    if (id == "null") id = null;
        //    if (id != null)
        //    {
        //        BSM.ListApproverLeave = new List<HRApproverLeave>();
        //        int TranNo = Convert.ToInt32(id);
        //        var ListWF = DB.HRWorkFlowLeaves.ToList();
        //        var HeaderEmpLeave = DB.HREmpLeaves;
        //        var ListStaff = DB.HR_STAFF_VIEW.ToList();
        //        BSM.HeaderEmpLeave = HeaderEmpLeave.SingleOrDefault(w => w.TranNo == TranNo);
        //        BSM.HeaderStaff = ListStaff.SingleOrDefault(x => x.EmpCode == BSM.HeaderEmpLeave.EmpCode);
        //        if (ListWF.Count > 0)
        //        {
        //            if (ListWF.Where(w => w.Code == BSM.HeaderStaff.PostFamily).Any())
        //            {
        //                BSM.ApprovalWorkFlow = DB.HRApproverLeaves.SingleOrDefault(w => w.TranNo == TranNo);
        //            }
        //        }

        //        var ListApproverLeave = DB.HRApproverLeaves.Where(w => w.TranNo == TranNo).ToList();
        //        string Status = BSM.HeaderEmpLeave.Status;
        //        if (Status != SYDocumentStatus.APPROVED.ToString() || Status != SYDocumentStatus.REJECTED.ToString())
        //        {
        //            if (ListWF.Count > 0)
        //            {
        //                var result = ListWF.Where(w => w.Code == Status).ToList();
        //                foreach (var item in ListWF.Where(w => result.Where(x => x.Step <= w.Step).Any()).ToList())
        //                {
        //                    foreach (var item1 in ListStaff.Where(w => w.PostFamily == item.Code && w.DEPT == BSM.HeaderStaff.DEPT).ToList())
        //                    {
        //                        HRApproverLeave App = new HRApproverLeave();
        //                        App.EmpCode = item1.EmpCode;
        //                        App.EmpName = item1.AllName;
        //                        App.Status = item.Description;
        //                        ListApproverLeave.Add(App);
        //                    }
        //                }
        //            }
        //        }

        //        BSM.ListApproverLeave = ListApproverLeave.ToList();
        //        if (BSM.HeaderEmpLeave != null)
        //        {
        //            BSM.ListEmpLeaveD = DB.HREmpLeaveDs.Where(x => x.LeaveTranNo == BSM.HeaderEmpLeave.TranNo).ToList();
        //            Session[Index_Sess_Obj + ActionName] = BSM;
        //            return View(BSM);
        //        }
        //    }

        //    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
        //    Session[Index_Sess_Obj + ActionName] = BSM;
        //    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

        //}
        //[HttpPost]
        //public ActionResult Edit(string id, BookingRoomObject collection)
        //{
        //    ActionName = "Edit";
        //    UserSession();
        //    this.ScreendIDControl = SCREEN_ID;
        //    DataSelector();
        //    UserConfForm(SYActionBehavior.EDIT);
        //    ViewData[SYSConstant.PARAM_ID] = id;
        //    BookingRoomObject BSM = new BookingRoomObject();
        //    if (id != null)
        //    {
        //        if (Session[Index_Sess_Obj + ActionName] != null)
        //        {
        //            BSM = (BookingRoomObject)Session[Index_Sess_Obj + ActionName];
        //            BSM.HeaderEmpLeave = collection.HeaderEmpLeave;
        //        }
        //        BSM.ScreenId = SCREEN_ID;
        //        int TranNo = Convert.ToInt32(id);
        //        string msg = BSM.EditLeaveRequest(TranNo);
        //        if (msg == SYConstant.OK)
        //        {
        //            DB = new DBBusinessProcess();
        //            SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
        //            mess.DocumentNumber = id;
        //            mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + mess.DocumentNumber;
        //            ViewData[SYConstant.MESSAGE_SUBMIT] = mess;
        //            Session[Index_Sess_Obj + ActionName] = BSM;
        //            return View(BSM);
        //        }
        //        else
        //        {
        //            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
        //            Session[Index_Sess_Obj + ActionName] = BSM;
        //            return View(BSM);
        //        }
        //    }
        //    ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
        //    return View(BSM);

        //}
        //#endregion
        //#region "Delete"
        //public ActionResult Delete(string id)
        //{
        //    UserSession();
        //    UserConfForm(SYActionBehavior.EDIT);
        //    DataSelector();
        //    if (id == "null") id = null;
        //    if (id != null)
        //    {
        //        BookingRoomObject Del = new BookingRoomObject();
        //        int TranNo = Convert.ToInt32(id);
        //        string msg = Del.DeleteLeave(TranNo);
        //        if (msg == SYConstant.OK)
        //        {
        //            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("LEAVE_COM", user.Lang);
        //        }
        //        else
        //        {
        //            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
        //        }
        //    }
        //    else
        //    {
        //        Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("LEAVE_NE", user.Lang);
        //    }
        //    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

        //}
        //#endregion
        #region "Details"
        public ActionResult Details(string id)
        {
            ActionName = "Details";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = id;

            if (id != null)
            {
                BookingRoomObject BSM = new BookingRoomObject();
                BSM.Header = DB.HRBookingRooms.Find(id);
                if (BSM.Header != null)
                {
                    BSM.ListBookingSchedule = DB.HRBookingSchedules.Where(w => w.BookingNo == id).OrderBy(w => w.LineItem).ToList();
                    Session[Index_Sess_Obj + ActionName] = BSM;
                    return View(BSM);
                }
            }

            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

        }
        #endregion

        #region "Ajax select item for time"
        public ActionResult GridItems()
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            BookingRoomObject BSM = new BookingRoomObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (BookingRoomObject)Session[Index_Sess_Obj + ActionName];
            }
            DataSelector();
            return PartialView("GridItems", BSM.ListBookingSchedule);
        }
        //create
        [HttpPost, ValidateInput(false)]
        public ActionResult CreateItem(HRBookingSchedule ModelObject)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            BookingRoomObject BSM = new BookingRoomObject();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (BookingRoomObject)Session[Index_Sess_Obj + ActionName];
                    }
                    DateTime StartTime = ModelObject.BookingDate + ModelObject.StartTime.TimeOfDay;
                    DateTime EndTime = ModelObject.BookingDate + ModelObject.EndTime.TimeOfDay;
                    ModelObject.StartTime = StartTime;
                    ModelObject.EndTime = EndTime;
                    string msg = BSM.IsValidBookingTime(ModelObject, BSM.ListBookingSchedule);
                    if (msg == SYConstant.OK)
                    {
                        int line = 0;
                        if (BSM.ListBookingSchedule.Count == 0)
                        {
                            line = 1;
                        }
                        else
                        {
                            line = BSM.ListBookingSchedule.Max(w => w.LineItem);
                            line = line + 1;
                        }
                        ModelObject.LineItem = line;

                        //var objEvent = DH.HRRoomTypes.Find(ModelObject.RoomID);
                        //if (objEvent != null)
                        //{
                        //    ModelObject.RoomID = objEvent.RoomCode;
                        //}

                        BSM.ListBookingSchedule.Add(ModelObject);
                        Session[Index_Sess_Obj + ActionName] = BSM;
                    }
                    else
                    {
                        ViewData["EditError"] = SYMessages.getMessage(msg);
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
            {
                ViewData["EditError"] = SYMessages.getMessage("EE001", user.Lang);
            }
            DataSelector();

            return PartialView("GridItems", BSM.ListBookingSchedule);
        }
        //edit
        [HttpPost, ValidateInput(false)]
        public ActionResult EditItem(HRBookingSchedule ModelObject)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            BookingRoomObject BSM = new BookingRoomObject();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (BookingRoomObject)Session[Index_Sess_Obj + ActionName];
                    }
                    var error = 0;
                    DateTime StartTime = ModelObject.BookingDate + ModelObject.StartTime.TimeOfDay;
                    DateTime EndTime = ModelObject.BookingDate + ModelObject.EndTime.TimeOfDay;
                    ModelObject.StartTime = StartTime;
                    ModelObject.EndTime = EndTime;
                    string msg = BSM.IsValidBookingTime(ModelObject, BSM.ListBookingSchedule);
                    if (msg != SYConstant.OK)
                    {
                        error = 1;
                        ViewData["EditError"] = SYMessages.getMessage(msg);
                    }
                    //var checkList = BSM.ListBookingSchedule.Where(w => w.RoomID == ModelObject.RoomID
                    //&& w.BookingDate == ModelObject.BookingDate
                    //&& w.EndTime.TimeOfDay >= ModelObject.StartTime.TimeOfDay).ToList();
                    //if (checkList.Count > 1)
                    //{
                    //    ViewData["EditError"] = SYMessages.getMessage("CON_NOT_ALLOW");
                    //    error = 1;
                    //}
                    //var objCheck = DP.HRBookingSchedules.Where(w=>w.RoomID== ModelObject.RoomID &&
                    //w.BookingDate== ModelObject.BookingDate).ToList();
                    //var _objCheck = objCheck.FirstOrDefault(w => w.EndTime.TimeOfDay >= ModelObject.StartTime.TimeOfDay);
                    //if (_objCheck != null)
                    //{
                    //    ViewData["EditError"] = SYMessages.getMessage("DUP_WITH_OTHER");
                    //    error = 2;
                    //}
                    if (error == 0)
                    {
                        var objUpdate = BSM.ListBookingSchedule.Where(w => w.LineItem == ModelObject.LineItem).First();

                        objUpdate.RoomID = ModelObject.RoomID;
                        objUpdate.BookingDate = ModelObject.BookingDate;
                        objUpdate.StartTime = ModelObject.StartTime;
                        objUpdate.EndTime = ModelObject.EndTime;

                        Session[Index_Sess_Obj + ActionName] = BSM;
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
            {
                ViewData["EditError"] = SYMessages.getMessage("EE001", user.Lang);
            }
            DataSelector();

            return PartialView("GridItems", BSM.ListBookingSchedule);
        }
        //delete
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteItem(int LineItem)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            BookingRoomObject BSM = new BookingRoomObject();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (BookingRoomObject)Session[Index_Sess_Obj + ActionName];
                    }
                    var error = 0;

                    var checkList = BSM.ListBookingSchedule.Where(w => w.LineItem == LineItem).ToList();
                    if (checkList.Count == 0)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("NO_ITEM_DELETE");
                        error = 1;
                    }

                    if (error == 0)
                    {
                        BSM.ListBookingSchedule.Remove(checkList.First());
                        Session[Index_Sess_Obj + ActionName] = BSM;
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
            {
                ViewData["EditError"] = SYMessages.getMessage("EE001", user.Lang);
            }
            DataSelector();

            return PartialView("GridItems", BSM.ListBookingSchedule);
        }

        #endregion

        public ActionResult RefreshTotal(string id)
        {
            ActionName = "Create";
            BookingRoomObject BSM = new BookingRoomObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (BookingRoomObject)Session[Index_Sess_Obj + ActionName];
                decimal TotalHour = 0;
                foreach (var read in BSM.ListBookingSchedule)
                {
                    var interval = read.EndTime.Subtract(read.StartTime);
                    var Hour = interval.TotalHours;
                    TotalHour += Convert.ToDecimal(Math.Round(Hour, 2));
                }
                BSM.Header.TotalHour = TotalHour;
                var result = new
                {
                    MS = SYConstant.OK,
                    Total = BSM.Header.TotalHour
                };

                return Json(result, JsonRequestBehavior.DenyGet);

            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }

        private void DataSelector()
        {
            SYDataList objList = new SYDataList("LEAVE_TIME");

            ViewData["ROOMTypes_SELECT"] = DB.HRRoomTypes.Where(w => w.IsActive == true).ToList();
            ViewData["STAFF_SELECT"] = DB.HRStaffProfiles.ToList();

            objList = new SYDataList("STATUS_LEAVE_APPROVAL");
            ViewData["STATUS_APPROVAL"] = objList.ListData;
        }
    }
}
