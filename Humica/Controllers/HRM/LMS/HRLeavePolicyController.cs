using DevExpress.DataProcessing;
using Humica.Core.DB;
using Humica.EF;
using Humica.EF.Models.SY;
using Humica.Logic.LM;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.HRM.LMS
{
    public class HRLeavePolicyController : EF.Controllers.MasterSaleController

    {
        private const string SCREEN_ID = "HRS0000012";
        private const string URL_SCREEN = "/HRM/LMS/HRLeavePolicy/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        HumicaDBContext DB = new HumicaDBContext();

        public HRLeavePolicyController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
        }
        // GET: Branch
        public ActionResult Index()
        {
            UserSession();
            UserConfListAndForm();
            DataSelector();
            LeaveTypeObject BSM = new LeaveTypeObject();
            BSM.ListLeaveProRate = DB.HRLeaveProRates.OrderByDescending(w => w.Status).ThenByDescending(w => w.ActWorkDayFrom).ToList();
            return View(BSM);
        }
        #region LeaveProRates

        public ActionResult GridviewProrate()
        {
            UserConf(ActionBehavior.EDIT);
            DataSelector();
            LeaveTypeObject BSM = new LeaveTypeObject();
            BSM.ListLeaveProRate = DB.HRLeaveProRates.OrderByDescending(w => w.Status).ThenBy(w => w.ActWorkDayFrom).ToList();
            return PartialView("GridviewProrate", BSM);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Create(HRLeaveProRate MModel)
        {
            UserSession();
            DataSelector();

            LeaveTypeObject BSM = new LeaveTypeObject();
            if (MModel.LeaveType != null)
            {
                try
                {
                    int Item = 0;
                    var obj = DB.HRLeaveProRates.ToList();
                    if (obj.Count > 0)
                    {
                        Item = obj.Max(w => w.LinItem);
                    }
                    MModel.LinItem = Item + 1;
                    MModel.Rate = MModel.Rate;
                    MModel.LeaveType = MModel.LeaveType;
                    MModel.Status = MModel.Status;
                    MModel.ActWorkDayFrom = MModel.ActWorkDayFrom;
                    MModel.ActWorkDayTo = MModel.ActWorkDayTo;
                    DB.HRLeaveProRates.Add(MModel);
                    int row = DB.SaveChanges();
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
            BSM.ListLeaveProRate = DB.HRLeaveProRates.OrderByDescending(w => w.Status).ThenBy(w => w.ActWorkDayFrom).ToList();
            return PartialView("GridviewProrate", BSM);
        }

        //edit
        [HttpPost, ValidateInput(false)]
        public ActionResult Edit(HRLeaveProRate MModel)
        {
            UserSession();
            DataSelector();
            LeaveTypeObject BSM = new LeaveTypeObject();

            if (ModelState.IsValid)
            {
                try
                {
                    var DHU = new HumicaDBContext();
                    var objUpdate = DB.HRLeaveProRates.FirstOrDefault(w => w.LinItem == MModel.LinItem && w.Status == MModel.Status && w.LeaveType == MModel.LeaveType);
                    if (objUpdate != null)
                    {
                        objUpdate.ActWorkDayFrom = MModel.ActWorkDayFrom;
                        objUpdate.ActWorkDayTo = MModel.ActWorkDayTo;
                        objUpdate.Rate = MModel.Rate;
                        DB.HRLeaveProRates.Attach(objUpdate);
                        DB.Entry(objUpdate).State = System.Data.Entity.EntityState.Modified;
                        int row = DB.SaveChanges();
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
            BSM.ListLeaveProRate = DB.HRLeaveProRates.OrderByDescending(w => w.Status).ThenBy(w => w.ActWorkDayFrom).ToList();
            return PartialView("GridviewProrate", BSM);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Delete(int LinItem, string LeaveType, string Status)
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            LeaveTypeObject BSM = new LeaveTypeObject();
            if (LinItem != null)
            {
                try
                {

                    var listPH = DB.HRLeaveProRates.ToList();
                    var objUpdate = DB.HRLeaveProRates.FirstOrDefault(w => w.LinItem == LinItem && w.Status == Status && w.LeaveType == LeaveType);
                    if (objUpdate != null)
                    {
                        DB.HRLeaveProRates.Remove(objUpdate);
                        int row = DB.SaveChanges();
                    }

                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }

            BSM.ListLeaveProRate = DB.HRLeaveProRates.OrderByDescending(w => w.Status).ThenBy(w => w.ActWorkDayFrom).ToList();
            return PartialView("GridviewProrate", BSM);
        }

        #endregion



        private void DataSelector()
        {
            ViewData["LEAVE_SELECT"] = DB.HRLeaveTypes.ToList();
            SYDataList objList = new SYDataList("STAFF_STATUS");
            ViewData["RESIGN_SELECT"] = objList.ListData;
        }
    }
}
