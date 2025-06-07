using Humica.Core.DB;
using Humica.EF;
using Humica.EF.Models.SY;
using Humica.Logic.PR;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.PR.PRS
{
    public class PRPayperiodController : Humica.EF.Controllers.MasterSaleController

    {
        private const string SCREEN_ID = "PRS0000010";
        private const string URL_SCREEN = "/PR/PRS/PRPayperiod";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "TranNo";
        HumicaDBContext DB = new HumicaDBContext();

        public PRPayperiodController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
        }
        public ActionResult Index()
        {
            UserSession();
            UserConfListAndForm();
            DataList();
            PRPayPeriodObject BSM = new PRPayPeriodObject();
            BSM.ListPayPeriod = DB.PRpayperiods.ToList().OrderByDescending(w => w.AttendanceStartDate).ToList();
            return View(BSM);
        }
        public ActionResult GridItems()
        {
            UserConf(ActionBehavior.EDIT);
            DataList();
            PRPayPeriodObject BSM = new PRPayPeriodObject();
            BSM.ListPayPeriod = DB.PRpayperiods.ToList().OrderByDescending(w => w.AttendanceStartDate).ToList();
            return PartialView("GridItems", BSM.ListPayPeriod);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Create(PRpayperiod MModel)
        {
            UserSession();
            DataList();
            PRPayPeriodObject BSM = new PRPayPeriodObject();
            if (ModelState.IsValid)
            {
                try
                {
                    BSM.ListPayPeriod = DB.PRpayperiods.Where(w => w.AttendanceStartDate == MModel.AttendanceStartDate).ToList();
                    if (BSM.ListPayPeriod.Count > 0)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("DOC_INV", user.Lang);
                    }
                    else if (MModel.AttendanceStartDate > MModel.AttendanceEndDate)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("DOC_INV", user.Lang);
                    }
                    else
                    {
                        DB.PRpayperiods.Add(MModel);
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
            BSM.ListPayPeriod = DB.PRpayperiods.ToList();
            return PartialView("GridItems", BSM.ListPayPeriod.ToList().OrderByDescending(w => w.AttendanceStartDate));
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult Edit(PRpayperiod MModel)
        {
            UserSession();
            DataList();
            PRPayPeriodObject BSM = new PRPayPeriodObject();
            if (ModelState.IsValid)
            {
                try
                {
                    var DBU = new HumicaDBContext();
                    var objUpdate = DBU.PRpayperiods.Find(MModel.PayPeriodId);
                    objUpdate.AttendanceDesc = MModel.AttendanceDesc;
                    objUpdate.AttendanceStartDate = MModel.AttendanceStartDate;
                    objUpdate.AttendanceEndDate = MModel.AttendanceEndDate;
                    objUpdate.SalaryDesc = MModel.SalaryDesc;
                    objUpdate.SalaryStartDate = MModel.SalaryStartDate;
                    objUpdate.SalaryEndDate = MModel.SalaryEndDate;
                    DB.PRpayperiods.Attach(MModel);
                    DB.Entry(MModel).Property(w => w.AttendanceDesc).IsModified = true;
                    DB.Entry(MModel).Property(w => w.AttendanceStartDate).IsModified = true;
                    DB.Entry(MModel).Property(w => w.AttendanceEndDate).IsModified = true;
                    DB.Entry(MModel).Property(w => w.SalaryDesc).IsModified = true;
                    DB.Entry(MModel).Property(w => w.SalaryStartDate).IsModified = true;
                    DB.Entry(MModel).Property(w => w.SalaryEndDate).IsModified = true;

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
            BSM.ListPayPeriod = DB.PRpayperiods.ToList();
            return PartialView("GridItems", BSM.ListPayPeriod.ToList().OrderByDescending(w => w.AttendanceStartDate));
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult Delete(int PayPeriodId)
        {
            UserSession();
            PRPayPeriodObject BSM = new PRPayPeriodObject();
            if (PayPeriodId != 0)
            {
                try
                {
                    var obj = DB.PRpayperiods.Find(PayPeriodId);
                    if (obj != null)
                    {
                        DB.PRpayperiods.Remove(obj);
                        int row = DB.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            BSM.ListPayPeriod = DB.PRpayperiods.OrderByDescending(w => w.AttendanceStartDate).ToList();
            return PartialView("GridItems", BSM.ListPayPeriod.ToList());
        }
        private void DataList()
        {
        }
    }
}
