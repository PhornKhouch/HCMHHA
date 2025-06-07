using Humica.Core.DB;
using Humica.EF;
using Humica.EF.Models.SY;
using Humica.Logic.LM;
using Humica.Models.SY;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.HRM.LMS
{

    public class LeaveTypeController : Humica.EF.Controllers.MasterSaleController
    {

        private const string SCREEN_ID = "HRS0000008";
        private const string URL_SCREEN = "/HRM/LMS/LeaveType/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "Code";
        HumicaDBContext DB = new HumicaDBContext();

        public LeaveTypeController()
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
            UserConfListAndForm(this.KeyName);
            DataSelector();

            LeaveTypeObject BSM = new LeaveTypeObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                var obj = (LeaveTypeObject)Session[Index_Sess_Obj + ActionName];
            }
            BSM.ListHeader = DB.HRLeaveTypes.ToList();
            Session[Index_Sess_Obj + ActionName] = BSM;

            return View(BSM);
        }
        public ActionResult PartialList()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            LeaveTypeObject BSM = new LeaveTypeObject();
            BSM.ListHeader = new List<HRLeaveType>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (LeaveTypeObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView(SYListConfuration.ListDefaultView, BSM.ListHeader);
        }
        #endregion
        #region "Create"
        public ActionResult Create()
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);
            LeaveTypeObject BSD = new LeaveTypeObject();
            BSD.Header = new HRLeaveType();
            BSD.Header.Soperand = 1;
            BSD.Header.BeforeDay = 0;
            BSD.Header.Operator = "*";
            BSD.Header.SVC = false;
            BSD.Header.IncPub = true;
            BSD.Header.InRest = true;
            BSD.Header.Probation = false;
            BSD.Header.CUT = false;
            BSD.Header.IsCurrent = true;
            BSD.Header.IsOverEntitle = true;
            BSD.Header.NumDay = 0;
            BSD.Header.Gender = "B";
            return View(BSD);
        }
        [HttpPost]
        public ActionResult Create(LeaveTypeObject BSM)
        {
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);

            if (ModelState.IsValid)
            {
                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.CreateLeaveType();

                if (msg == SYConstant.OK)
                {

                    SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                    mess.DocumentNumber = BSM.Header.Code;
                    mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Edit/" + mess.DocumentNumber;

                    Session[Index_Sess_Obj + ActionName] = BSM;
                    Session[SYConstant.MESSAGE_SUBMIT] = mess;
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Create");
                }
                else
                {
                    ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                    return View(BSM);
                }
            }
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            return View(BSM);

        }
        #endregion
        #region "Edit"
        public ActionResult Edit(string id)
        {
            ActionName = "Edit";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = id;

            if (id != null)
            {
                LeaveTypeObject BSM = new LeaveTypeObject();
                BSM.Header = DB.HRLeaveTypes.Find(id);
                if (BSM.Header != null)
                {
                    return View(BSM);
                }
            }

            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

        }
        [HttpPost]
        public ActionResult Edit(string id, LeaveTypeObject BSM)
        {
            UserSession();
            this.ScreendIDControl = SCREEN_ID;
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = id;
            if (id != null)
            {
                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.editLeaveType(id);
                if (msg == SYConstant.OK)
                {

                    SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                    mess.DocumentNumber = BSM.Header.Code.ToString();
                    mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Edit?id=" + mess.DocumentNumber;
                    ViewData[SYConstant.MESSAGE_SUBMIT] = mess;
                    return View(BSM);
                }
                else
                {
                    ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                    return View(BSM);
                }
            }
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            return View(BSM);

        }
        #endregion
        #region "Delete"
        public ActionResult Delete(string id)
        {
            UserSession();
            UserConfForm(SYActionBehavior.EDIT);
            DataSelector();
            if (id != null)
            {
                LeaveTypeObject Del = new LeaveTypeObject();
                string msg = Del.deleteLeaveType(id);
                if (msg == SYConstant.OK)
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_RM", user.Lang);
                }
                else
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                }
            }
            else
            {
                Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE", user.Lang);
            }
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

        }
        #endregion

        private void DataSelector()
        {
            SYDataList objList = new SYDataList("GENDER_SELECT");
            ViewData["GENDER_SELECT"] = objList.ListData;
            objList = new SYDataList("LEAVETYPE");
            ViewData["LEAVETYPE_SELECT"] = objList.ListData;
            objList = new SYDataList("Operator");
            ViewData["Operator_SELECT"] = objList.ListData;
            ViewData["Leave_SELECT"] = DB.HRLeaveTypes.ToList();

        }
    }
}
