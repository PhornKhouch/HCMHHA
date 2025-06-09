using DevExpress.Spreadsheet;
using DevExpress.Web;
using DevExpress.Web.Mvc;
using Humica.Core.DB;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic.HR;
using Humica.Models.SY;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Humica.Controllers.HRM.EmployeeInfo
{
    public class HRImportSalaryController : Humica.EF.Controllers.MasterSaleController
    {
        private const string SCREEN_ID = "HRE0000011";
        private const string URL_SCREEN = "/HRM/EmployeeInfo/HRImportSalary/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "EmpCode";
        HumicaDBContext DB = new HumicaDBContext();
        HumicaDBViewContext DBV = new HumicaDBViewContext();

        public HRImportSalaryController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
        }
        public ActionResult Index()
        {
            ActionName = "Import";
            UserSession();
            UserConfListAndForm(this.KeyName);
            HRStaffProfileObject BSM = new HRStaffProfileObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                var obj = (HRStaffProfileObject)Session[Index_Sess_Obj + ActionName];
            }
            //BSM.ListHeader = DB.HRImportSalaries.ToList();
            Session[Index_Sess_Obj + ActionName] = BSM;
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Import");
        }
        public ActionResult PartialList()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            HRStaffProfileObject BSM = new HRStaffProfileObject();
            //BSM.ListStaffView = new List<HR_STAFF_VIEW>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRStaffProfileObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView(SYListConfuration.ListDefaultView, BSM.ListHeader);
        }



        public ActionResult GridItems()
        {
            ActionName = "Import";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRStaffProfileObject BSM = new HRStaffProfileObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRStaffProfileObject)Session[Index_Sess_Obj + ActionName];
                if (BSM.ListHeader != null)
                {
                    BSM.ListHeader.Clear();
                }
            }
            if (BSM.ListTemplate.Count > 0)
            {
                SYExcel excel = new SYExcel();
                foreach (var read in BSM.ListTemplate.ToList())
                {
                    excel.FileName = read.UpoadPath;
                }
                DataTable dtHeader = excel.GenerateExcelData();
                BSM.ListHeader = new List<HRStaffProfile>();
                //objStaff.ListItem = new List<HRStaffProfile>();
                //objStaff.ListPlanItem = new List<HLContractPlanItem>();

                if (dtHeader != null)
                {
                    for (int i = 0; i < dtHeader.Rows.Count; i++)
                    {
                        var objHeader = new HRStaffProfile();
                        objHeader.EmpCode = dtHeader.Rows[i][0].ToString();
                        objHeader.AllName = dtHeader.Rows[i][1].ToString();
                        objHeader.Salary = (decimal)(int?)SYSettings.getNumberValue(dtHeader.Rows[i][2].ToString());


                        BSM.ListHeader.Add(objHeader);
                    }
                }
            }
            //BSM.ListDepreMethod = DB.ExFADepreciationMethods.ToList();
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("GridItems", BSM);
        }
        #region "Import"
        public ActionResult Import()
        {
            UserSession();
            ActionName = "Import";
            UserConfListAndForm(ActionBehavior.IMPORT, "UploadName", "HRImportSalary", SYSConstant.DEFAULT_UPLOAD_LIST);

            var BSM = new HRStaffProfileObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRStaffProfileObject)Session[Index_Sess_Obj + ActionName];
                if (BSM.ListHeader != null)
                {
                    BSM.ListHeader.Clear();
                }

            }
            BSM.ListTemplate = DB.MDUploadTemplates.Where(w => w.ScreenId == SCREEN_ID && w.UploadBy == user.UserName).OrderByDescending(w => w.UploadDate).ToList();

            if (BSM.ListTemplate.Count > 0)
            {
                SYExcel excel = new SYExcel();
                foreach (var read in BSM.ListTemplate.ToList())
                {
                    excel.FileName = read.UpoadPath;
                }

                DataTable dtHeader = excel.GenerateExcelData();

                BSM.ListHeader = new List<HRStaffProfile>();
                if (dtHeader != null)
                {
                    for (int i = 0; i < dtHeader.Rows.Count; i++)
                    {
                        var objHeader = new HRStaffProfile();
                        objHeader.EmpCode = dtHeader.Rows[i][0].ToString();
                        objHeader.AllName = dtHeader.Rows[i][1].ToString();
                        objHeader.Salary = SYSettings.getNumberValue(dtHeader.Rows[i][2].ToString());

                        BSM.ListHeader.Add(objHeader);

                    }
                }

            }

            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }

        [HttpPost]
        public ActionResult UploadControlCallbackAction(HttpPostedFileBase file_Uploader)
        {

            UserSession();
            ActionName = "Import";
            this.ScreendIDControl = SYSConstant.DEFAULT_UPLOAD_LIST;
            UserConfListAndForm(ActionBehavior.IMPORT, "UploadName", "HRImportSalary", SYSConstant.DEFAULT_UPLOAD_LIST);
            SYFileImport sfi = new SYFileImport(DB.CFUploadPaths.Find("IMP_SALARY"));
            sfi.ObjectTemplate = new MDUploadTemplate();
            sfi.ObjectTemplate.UploadDate = DateTime.Now;
            sfi.ObjectTemplate.ScreenId = SCREEN_ID;
            sfi.ObjectTemplate.UploadBy = user.UserName;
            sfi.ObjectTemplate.Module = "HR";
            sfi.ObjectTemplate.IsGenerate = false;

            UploadedFile[] files = UploadControlExtension.GetUploadedFiles("FileUploadOPB",
                sfi.ValidationSettings,
                sfi.uc_FileUploadComplete);


            var objStaff = new HRStaffProfileObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                objStaff = (HRStaffProfileObject)Session[Index_Sess_Obj + ActionName];
            }
            objStaff.ListTemplate = DB.MDUploadTemplates.Where(w => w.ScreenId == SCREEN_ID && w.UploadBy == user.UserName).OrderByDescending(w => w.UploadDate).ToList();
            objStaff.ListHeader = new List<HRStaffProfile>();
            //  objStaff.ListHeader = new List<HRProApprEmpInfor>();


            Session[Index_Sess_Obj + ActionName] = objStaff;
            return Redirect(SYUrl.getBaseUrl() + ScreenUrl + "Import");
            //return null;
        }
        public ActionResult UploadList()
        {
            UserSession();
            ActionName = "Import";
            this.ScreendIDControl = SYSConstant.DEFAULT_UPLOAD_LIST;
            UserConfListAndForm(ActionBehavior.IMPORT, "UploadName", "ImportSalary", SYSConstant.DEFAULT_UPLOAD_LIST);

            var objStaff = new HRStaffProfileObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                objStaff = (HRStaffProfileObject)Session[Index_Sess_Obj + ActionName];
            }
            objStaff.ListTemplate = DB.MDUploadTemplates.Where(w => w.ScreenId == SCREEN_ID && w.UploadBy == user.UserName).OrderByDescending(w => w.UploadDate).ToList();
            objStaff.ListHeader = new List<HRStaffProfile>();


            Session[Index_Sess_Obj + ActionName] = objStaff;
            return PartialView(SYListConfuration.ListDefaultUpload, objStaff.ListTemplate);
        }

        [HttpGet]
        public ActionResult GenerateUpload(int id)
        {
            UserSession();
            MDUploadTemplate obj = DB.MDUploadTemplates.Find(id);
            if (obj != null)
            {
                var DBB = new HumicaDBContext();
                SYExcel excel = new SYExcel();
                excel.FileName = obj.UpoadPath;
                DataTable dtHeader = excel.GenerateExcelData();
                //DataTable dtItem = excel.GenerateExcelData(2);
                var BSM = new HRStaffProfileObject();
                //BSM.Header = new HRStaffProfile();
                //BSM.StCare = new HREmpCareer();

                //objStaff.c = CompanyCode;
                if (obj.IsGenerate == true)
                {
                    SYMessages mess = SYMessages.getMessageObject("FILE_RG", user.Lang);
                    Session[SYSConstant.MESSAGE_SUBMIT] = mess;
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Import");
                }
                if (dtHeader != null)
                {
                    try
                    {
                        string msg = SYConstant.OK;
                        if (dtHeader.Rows.Count > 0) // Header
                        {
                            BSM.ListHeader = new List<HRStaffProfile>();
                            if (dtHeader != null)
                            {
                                for (int i = 0; i < dtHeader.Rows.Count; i++)
                                {
                                    var objHeader = new HRStaffProfile();
                                    objHeader.EmpCode = dtHeader.Rows[i][0].ToString();
                                    objHeader.AllName = dtHeader.Rows[i][1].ToString();
                                    objHeader.Salary = SYSettings.getNumberValue(dtHeader.Rows[i][2].ToString());

                                    BSM.ListHeader.Add(objHeader);

                                }
                            }


                        }

                        DateTime create = DateTime.Now;

                        msg = BSM.uploadSalary();
                        if (msg != SYConstant.OK)
                        {
                            obj.Message = SYMessages.getMessage(msg);
                            obj.Message += ":" + BSM.MessageError;
                            obj.IsGenerate = false;
                            DB.MDUploadTemplates.Attach(obj);
                            DB.Entry(obj).Property(w => w.Message).IsModified = true;
                            DB.Entry(obj).Property(w => w.IsGenerate).IsModified = true;
                            DB.SaveChanges();
                            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "/Import");
                        }
                        obj.Message = SYConstant.OK;
                        obj.IsGenerate = true;
                        DBB.MDUploadTemplates.Attach(obj);
                        DBB.Entry(obj).Property(w => w.Message).IsModified = true;
                        DBB.Entry(obj).Property(w => w.DocumentNo).IsModified = true;
                        DBB.Entry(obj).Property(w => w.IsGenerate).IsModified = true;
                        DBB.SaveChanges();

                    }
                    catch (DbUpdateException e)
                    {
                        /*------------------SaveLog----------------------------------*/
                        SYEventLog log = new SYEventLog();
                        log.ScreenId = SCREEN_ID;
                        log.UserId = user.UserID.ToString();
                        log.DocurmentAction = "UPLOAD";
                        log.Action = SYActionBehavior.ADD.ToString();

                        SYEventLogObject.saveEventLog(log, e, true);
                        /*----------------------------------------------------------*/
                        obj.Message = e.Message;
                        obj.IsGenerate = false;
                        DB.MDUploadTemplates.Attach(obj);
                        DB.Entry(obj).Property(w => w.Message).IsModified = true;
                        DB.Entry(obj).Property(w => w.IsGenerate).IsModified = true;
                        DB.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        /*------------------SaveLog----------------------------------*/
                        SYEventLog log = new SYEventLog();
                        log.ScreenId = SCREEN_ID;
                        log.UserId = user.UserID.ToString();
                        log.DocurmentAction = "UPLOAD";
                        log.Action = SYActionBehavior.ADD.ToString();

                        SYEventLogObject.saveEventLog(log, e, true);
                        obj.Message = e.Message;
                        obj.IsGenerate = false;
                        DB.MDUploadTemplates.Attach(obj);
                        DB.Entry(obj).Property(w => w.Message).IsModified = true;
                        DB.Entry(obj).Property(w => w.IsGenerate).IsModified = true;
                        DB.SaveChanges();
                    }
                }

            }

            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Import");
        }

        //public ActionResult DownloadTemplate()
        //{
        //    string fileName = Server.MapPath("~/Content/TEMPLATE/TemplateSalary.xlsx");
        //    Response.Clear();
        //    Response.Buffer = true;
        //    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //    Response.AddHeader("content-disposition", "attachment;filename=TemplateSalary.xlsx");
        //    Response.Cache.SetCacheability(HttpCacheability.NoCache);
        //    Response.WriteFile(fileName);
        //    Response.End();
        //    return null;
        //}
		public ActionResult DownloadTemplate()
		{

			// Get all active employees
			var allEmployees = DB.HRStaffProfiles
				.Where(e => e.Status == "A")
				.Select(e => new
				{
					e.EmpCode,
					e.AllName
				}).ToList();

			using (var workbook = new DevExpress.Spreadsheet.Workbook())
			{
				workbook.Worksheets[0].Name = "Salary";
				List<ExCFUploadMapping> _ListMaster = new List<ExCFUploadMapping>();
				_ListMaster.Add(new ExCFUploadMapping { FieldName = "EmpCode" });
				_ListMaster.Add(new ExCFUploadMapping { FieldName = "Employee Name" });
				_ListMaster.Add(new ExCFUploadMapping { FieldName = "Salary" });
				Worksheet worksheet = workbook.Worksheets[0];

				List<ClsUploadMapping> _EmployeeData = new List<ClsUploadMapping>();
				foreach (var emp in allEmployees)
				{
					_EmployeeData.Add(new ClsUploadMapping
					{
						FieldName = emp.EmpCode,
						FieldName1 = emp.AllName,
					});
				}
				// Export data to each sheet with header formatting
				ClsConstant.ExportDataToWorksheet(worksheet, _ListMaster);
				ClsConstant.ExportDataToWorksheetRow(worksheet, _EmployeeData);

				// Save the workbook to a memory stream
				using (var stream = new System.IO.MemoryStream())
				{
					workbook.SaveDocument(stream, DevExpress.Spreadsheet.DocumentFormat.Xlsx);
					stream.Seek(0, System.IO.SeekOrigin.Begin);

					return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TemplateSalary.xlsx");
				}
			}
			return null;
		}
		#endregion

		private void DataSelector()
        {
            ViewData["STAFF_SELECT"] = DBV.HR_STAFF_VIEW.ToList();
            //ViewData["APPRTYPE_SELECT"] = DH.HRApprTypes.ToList();
            //ViewData["HREmpEduType_LIST"] = DH.HREduTypes.ToList();
        }
    }
}