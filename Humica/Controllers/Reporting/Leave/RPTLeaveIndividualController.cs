﻿using Humica.Core.FT;
using Humica.EF.Models.SY;
using DevExpress.Web.Mvc;
using System;
using System.Linq;
using System.Web.Mvc;
using Humica.EF;
using Humica.Models.Report;
using System.Reflection;
using Humica.Models.SY;
using Humica.EF.MD;
using Humica.Core.DB;
using HUMICA.Models.Report.Leave;

namespace Humica.Controllers.Reporting
{
    public class RPTLeaveIndividualController : Humica.EF.Controllers.MasterSaleController

    {
        private const string SCREEN_ID = "RPTLM00003";
        private const string URL_SCREEN = "/Reporting/RPTLeaveIndividual/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName = "";
        HumicaDBContext DB = new HumicaDBContext();

        public RPTLeaveIndividualController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
        }
        public ActionResult Index()
        {
            ActionName = "Index";
            DataList();
            UserSession();
            UserConfListAndForm();
            FTFilterEmployee Filter = new FTFilterEmployee();
            Filter.InMonth = DateTime.Now;
            Filter.Status = SYDocumentStatus.A.ToString();
            return View(Filter);
        }
        [HttpPost]
        public ActionResult Index(FTFilterEmployee Filter)
        {
            ActionName = "Print";
            UserSession();
            DataList();
            UserConfListAndForm();
            Session[Index_Sess_Obj + ActionName] = Filter;
            return View("ReportView", Filter);
        }
        public ActionResult DocumentViewerPartial()
        {
            UserSession();
            DataList();
            UserConf(ActionBehavior.VIEWONLY);
            ActionName = "Print";
        try
        {
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                var RPT = (FTFilterEmployee)Session[Index_Sess_Obj + ActionName];
                RPTLeaveIndividual sa = new RPTLeaveIndividual();
                var objRpt = DB.CFReportObjects.Where(w => w.ScreenID == SCREEN_ID
                && w.IsActive == true).ToList();
                if (objRpt.Count > 0)
                {
                    sa.LoadLayoutFromXml(ClsConstant.DEFAULT_REPORT_PATH + objRpt.First().ReportObject);
                }
                var Dict = RPT.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(RPT, null));

                foreach (var read in sa.Parameters)
                {
                    if (Dict[read.Name] == null)
                    {
                        sa.Parameters[read.Name].Value = "";
                        if (read.Name == "Branch")
                        {
                            sa.Parameters[read.Name].Value = SYConstant.Branch_Condition;
                        }
                    }
                    else
                    {
                        sa.Parameters[read.Name].Value = Dict[read.Name].ToString();
                    }

                    read.Visible = false;
                }
                Session[Index_Sess_Obj] = sa;

                Session[Index_Sess_Obj + ActionName] = RPT;

                return PartialView("PrintForm", sa);
            }
        }
        catch (Exception e)
        {
            /*------------------SaveLog----------------------------------*/
            SYEventLog log = new SYEventLog();
            log.ScreenId = SCREEN_ID;
            log.UserId = user.UserID.ToString();
            log.DocurmentAction = "Print";
            log.Action = SYActionBehavior.ADD.ToString();

            SYEventLogObject.saveEventLog(log, e, true);
            /*----------------------------------------------------------*/
        }
        return null;
        }
        public ActionResult DocumentViewerExportTo()
        {
            ActionName = "Print";

            if (Session[Index_Sess_Obj] != null)
            {
                RPTLeaveIndividual reportModel = (RPTLeaveIndividual)Session[Index_Sess_Obj];
                return ReportViewerExtension.ExportTo(reportModel);
            }
            return null;
        }

        private void DataList()
        {
            SYDataList objList = new SYDataList("STATUS_EMPLOYEE");
            ViewData["STATUS_EMPLOYEE"] = objList.ListData;
            ViewData["DEPARTMENT_SELECT"] = DB.HRDepartments.ToList();
            ViewData["BRANCHES_SELECT"] = SYConstant.getBranchDataAccess();
            ViewData["SECTION_SELECT"] = DB.HRSections.ToList();
            ViewData["POSITION_SELECT"] = DB.HRPositions.ToList();
            ViewData["DIVISION_SELECT"] = DB.HRDivisions.ToList();
            ViewData["LEVEL_SELECT"] = SYConstant.getLevelDataAccess();
            ViewData["LEAVE_TYPE_SELECT"] = DB.HRLeaveTypes.ToList();
            ViewData["STAFF_SELECT"] = DB.HRStaffProfiles.ToList();
        }
    }
}