using Humica.Core.DB;
using Humica.EF.MD;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers
{
    public class DashboardsController : Humica.EF.Controllers.MasterSaleController
    {
        HumicaDBContext appointmentContext = new HumicaDBContext();
        object resourceContext = null;

        public ActionResult Index()
        {
            UserSession();
            UserConfListAndForm();

            return View();
        }

        public ActionResult LinkTo(int id, string linkType)
        {
            if (linkType == "SCH")
            {
                //var DBB = new HumicaDBContext();
                //var listOp = DBB.OPBookingSchedules.Where(w => w.ID == id).ToList();
                //if(listOp.Count>0)
                //{
                //    return Redirect(Humica.EF.Models.SY.SYUrl.getBaseUrl() + "/WH/OP/Booking/Details?id="+listOp.First().SOReference);
                //}

            }
            return null;
        }


        HumicaDBViewContext appointmentContext1 = new HumicaDBViewContext();
        HumicaDBViewContext resourceContext1 = new HumicaDBViewContext();

        public ActionResult SchedulerPartial()
        {
            var usr = SYSession.getSessionUser();
            var appointments = appointmentContext1.AT_EmpSchedule.Where(w => w.EmpCode == usr.UserName).ToList();
            var resources = resourceContext1.ATEmpSchedules.Where(w => w.EmpCode == usr.UserName).ToList();

            ViewData["Appointments"] = appointments.ToList();
            ViewData["Resources"] = resources.ToList();

            return PartialView("_SchedulerPartial");
        }

        public ActionResult SummarySchedulerPartial()
        {
            var appointments = appointmentContext1.RPT_AttScheduleSummary;
            var resources = resourceContext1.RPT_AttScheduleSummary;

            ViewData["AppointmentsSum"] = appointments.ToList();
            ViewData["ResourcesSum"] = resources.ToList();

            return PartialView("SummarySchedulerPartial");
        }


        // Humica.Core.DB.HumicaDBContext db = new Humica.Core.DB.HumicaDBContext();

        //public ActionResult ChartPartial()
        //{
        //    var model = DB.HRStaffProfiles;
        //    return PartialView("_ChartPartial", model.ToList());
        //}
    }
    public class DashboardsControllerSchedulerSettingsSum
    {
        static DevExpress.Web.Mvc.MVCxAppointmentStorage appointmentStorage;
        public static DevExpress.Web.Mvc.MVCxAppointmentStorage AppointmentStorage
        {
            get
            {
                if (appointmentStorage == null)
                {
                    appointmentStorage = new DevExpress.Web.Mvc.MVCxAppointmentStorage();
                    appointmentStorage.Mappings.AppointmentId = "ID";
                    appointmentStorage.Mappings.Start = "TranDate";
                    appointmentStorage.Mappings.End = "TranDate";
                    appointmentStorage.Mappings.Subject = "Subjects";
                    appointmentStorage.Mappings.Description = "";
                    appointmentStorage.Mappings.Location = "";
                    appointmentStorage.Mappings.AllDay = "AllDay";
                    appointmentStorage.Mappings.Type = "Type";
                    appointmentStorage.Mappings.RecurrenceInfo = "";
                    appointmentStorage.Mappings.ReminderInfo = "";
                    appointmentStorage.Mappings.Label = "Lable";
                    appointmentStorage.Mappings.Status = "Status";
                    appointmentStorage.Mappings.ResourceId = "ID";

                    appointmentStorage.Labels.Clear();
                    appointmentStorage.Labels.Add(appointmentStorage.Labels.CreateNewLabel(0, "Working", "Working", System.Drawing.Color.White));
                    appointmentStorage.Labels.Add(appointmentStorage.Labels.CreateNewLabel(1, "PH", "test", System.Drawing.Color.Red));

                }
                return appointmentStorage;
            }
        }

        static DevExpress.Web.Mvc.MVCxResourceStorage resourceStorage;
        public static DevExpress.Web.Mvc.MVCxResourceStorage ResourceStorage
        {
            get
            {
                if (resourceStorage == null)
                {
                    resourceStorage = new DevExpress.Web.Mvc.MVCxResourceStorage();
                    resourceStorage.Mappings.ResourceId = "ID";
                    resourceStorage.Mappings.Caption = "Subjects";
                }
                return resourceStorage;
            }
        }


    }

    public class DashboardControllerSchedulerSettings
    {
        static DevExpress.Web.Mvc.MVCxAppointmentStorage appointmentStorage;
        public static DevExpress.Web.Mvc.MVCxAppointmentStorage AppointmentStorage
        {
            get
            {
                if (appointmentStorage == null)
                {
                    appointmentStorage = new DevExpress.Web.Mvc.MVCxAppointmentStorage();
                    appointmentStorage.Mappings.AppointmentId = "TranNo";
                    appointmentStorage.Mappings.Start = "TranDate";
                    appointmentStorage.Mappings.End = "TranDate";
                    appointmentStorage.Mappings.Subject = "Subjects";
                    appointmentStorage.Mappings.Description = "";
                    appointmentStorage.Mappings.Location = "";
                    appointmentStorage.Mappings.AllDay = "AllDay";
                    appointmentStorage.Mappings.Type = "Type";
                    appointmentStorage.Mappings.RecurrenceInfo = "";
                    appointmentStorage.Mappings.ReminderInfo = "";
                    appointmentStorage.Mappings.Label = "Lable";
                    appointmentStorage.Mappings.Status = "Status";
                    appointmentStorage.Mappings.ResourceId = "TranNo";
                    appointmentStorage.Labels.Clear();
                    appointmentStorage.Labels.Add(appointmentStorage.Labels.CreateNewLabel(1, "Working", "Working", System.Drawing.Color.White));
                    appointmentStorage.Labels.Add(appointmentStorage.Labels.CreateNewLabel(0, "PH", "test", System.Drawing.Color.Red));
                }
                return appointmentStorage;
            }
        }

        static DevExpress.Web.Mvc.MVCxResourceStorage resourceStorage;
        public static DevExpress.Web.Mvc.MVCxResourceStorage ResourceStorage
        {
            get
            {
                if (resourceStorage == null)
                {
                    resourceStorage = new DevExpress.Web.Mvc.MVCxResourceStorage();
                    resourceStorage.Mappings.ResourceId = "TranNo";
                    resourceStorage.Mappings.Caption = "";
                }
                return resourceStorage;
            }
        }
    }

}
