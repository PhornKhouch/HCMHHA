using DevExpress.Spreadsheet;
using DevExpress.Web.Mvc;
using Humica.Core.BS;
using Humica.Core.DB;
using Humica.Core.FT;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic;
using Humica.Logic.Att;
using Humica.Models.SY;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Humica.Controllers.Attendance.Attendance
{

    public class TemplateSchController : Humica.EF.Controllers.MasterSaleController
    {

        private const string SCREEN_ID = "ATM0000003";
        private const string URL_SCREEN = "/Attendance/Attendance/TemplateSch/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName = "";
        private string KeyName = "EmpCode";
        HumicaDBContext DB = new HumicaDBContext();
        HumicaDBViewContext DBV = new HumicaDBViewContext();

        public TemplateSchController()
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

            ATEmpSchObject BSM = new ATEmpSchObject();
            BSM.FMonthly = new Humica.Core.FT.FTMonthlySum();
            BSM.LIstEmplSch = new List<ListEmpSch>();
            BSM.FMonthly.InMonth = DateTime.Now;
            BSM.LIstEmplSch = BSM.LoadEmpSch(BSM.FMonthly.InMonth, SYConstant.getBranchDataAccess());
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                var obj = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            var Approve = SYDocumentStatus.APPROVED.ToString();
            BSM.ListImpRoster = DB.ATImpRosterHeaders
                                .Where(w => w.Status != Approve )
                                .OrderByDescending(w => w.DocumentNo)
                                .ToList();

            return View(BSM);
        }
        [HttpPost]
        public ActionResult Index(ATEmpSchObject BSM)
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            DataSelector();
            BSM.LIstEmplSch = new List<ListEmpSch>();
            BSM.LIstEmplSch = BSM.LoadEmpSch(BSM.FMonthly.InMonth, SYConstant.getBranchDataAccess());
            var Approve = SYDocumentStatus.APPROVED.ToString();
            BSM.ListImpRoster = DB.ATImpRosterHeaders
                               .Where(w => w.Status != Approve )
                               .OrderByDescending(w => w.DocumentNo)
                               .ToList();
            Session[Index_Sess_Obj + ActionName] = BSM;

            return View(BSM);
        }
        public ActionResult PartialProcess()
        {
            ActionName = "Index";
            UserSession();
            UserConfList(this.KeyName);
            ATEmpSchObject BSM = new ATEmpSchObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("PartialProcess", BSM.ListImpRoster);
        }
        public ActionResult PartialList()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            ATEmpSchObject BSM = new ATEmpSchObject();
            //   BSM.ListEmpSchdule = new List<VIEW_ATEmpSchedule>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView(SYListConfuration.ListDefaultView, BSM.LIstEmplSch);
        }

        #endregion
        #region "Details"
        public ActionResult Details(string DocumentNo)
        {
            ActionName = "Details";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = DocumentNo;
            ATEmpSchObject BSM = new ATEmpSchObject();
            BSM.FMonthly = new FTMonthlySum();
            BSM.LIstEmplSch = new List<ListEmpSch>();
            BSM.RosterHeader = new ATImpRosterHeader();
            if (!string.IsNullOrEmpty(DocumentNo))
            {
                BSM.RosterHeader = DB.ATImpRosterHeaders.FirstOrDefault(w => w.DocumentNo == DocumentNo);
                if (BSM.RosterHeader != null)
                {
                    BSM.ListApproval = DB.ExDocApprovals.Where(w => w.DocumentNo == DocumentNo && w.DocumentType == "ROSTER").ToList();
                    BSM.LIstEmplSch = BSM.LoadEmpImport(DocumentNo);
                    Session[Index_Sess_Obj + ActionName] = BSM;
                    return View(BSM);
                }
            }

            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            Session[Index_Sess_Obj + ActionName] = BSM;
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

        }
        #endregion
        #region "Import"
        #region Delete
        public ActionResult Delete(string DocumentNo)
        {
            ActionName = "Details";
            UserSession();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = DocumentNo;
            ATEmpSchObject BSM = new ATEmpSchObject();
            BSM.RosterHeader = new ATImpRosterHeader();
            if (!string.IsNullOrEmpty(DocumentNo))
            {
               
                string msg = BSM.DeleteDoc(DocumentNo);
                if (msg == SYConstant.OK)
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_RM", user.Lang);
                }
                else
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
               
            }

            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            Session[Index_Sess_Obj + ActionName] = BSM;
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

        }
        #endregion
        public ActionResult GridItems()
        {
            ActionName = "Import";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            ATEmpSchObject BSM = new ATEmpSchObject();
            BSM.LIstEmplSch = new List<ListEmpSch>();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("GridItems", BSM);
        }

        public ActionResult Import()
        {
            UserSession();
            DataSelector();
            ActionName = "Import";
            UserConfListAndForm(ActionBehavior.IMPORT, "UploadName", "TemplateSch", SYSConstant.DEFAULT_UPLOAD_LIST);

            var objATEmpSch = new ATEmpSchObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                objATEmpSch = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
            }
            objATEmpSch.Filter = new FTFilterEmployee();
            objATEmpSch.Period = DateTime.Now;
            var screenIds = new List<string> { SCREEN_ID, "ESS0000012" };
            objATEmpSch.ListTemplate = DB.MDUploadTemplates
                                        .Where(w => screenIds.Contains(w.ScreenId))
                                        .OrderByDescending(w => w.UploadDate)
                                        .ToList();
            objATEmpSch.LIstEmplSch = new List<ListEmpSch>();
            Session[Index_Sess_Obj + ActionName] = objATEmpSch;
            return View(objATEmpSch);
        }

        [HttpPost]
        public ActionResult UploadControlCallbackAction(HttpPostedFileBase file_Uploader)
        {
            UserSession();
            ActionName = "Import";
            this.ScreendIDControl = SYSConstant.DEFAULT_UPLOAD_LIST;
            UserConfListAndForm(ActionBehavior.IMPORT, "UploadName", "TemplateSch", SYSConstant.DEFAULT_UPLOAD_LIST);
            SYFileImport sfi = new SYFileImport(DB.CFUploadPaths.Find("SCHEDULE"));
            sfi.ObjectTemplate = new MDUploadTemplate();
            sfi.ObjectTemplate.UploadDate = DateTime.Now;
            sfi.ObjectTemplate.ScreenId = SCREEN_ID;
            sfi.ObjectTemplate.UploadBy = user.UserName;
            sfi.ObjectTemplate.Module = "ATTENDANCE";
            sfi.ObjectTemplate.IsGenerate = false;

            DevExpress.Web.UploadedFile[] files = UploadControlExtension.GetUploadedFiles("FileUploadOPB",
                sfi.ValidationSettings,
                sfi.uc_FileUploadComplete);


            var BSM = new ATEmpSchObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
            }
            var screenIds = new List<string> { SCREEN_ID, "ESS0000012" };
            BSM.ListTemplate = DB.MDUploadTemplates
                                        .Where(w => screenIds.Contains(w.ScreenId))
                                        .OrderByDescending(w => w.UploadDate)
                                        .ToList();
            BSM.LIstEmplSch = new List<ListEmpSch>();

            Session[Index_Sess_Obj + ActionName] = BSM;
            return Redirect(SYUrl.getBaseUrl() + ScreenUrl + "Import");
        }

        public ActionResult UploadList()
        {
            UserSession();
            ActionName = "Import";
            this.ScreendIDControl = SYSConstant.DEFAULT_UPLOAD_LIST;
            UserConfListAndForm(ActionBehavior.IMPORT, "UploadName", "TemplateSch", SYSConstant.DEFAULT_UPLOAD_LIST);

            var objStaff = new ATEmpSchObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                objStaff = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
            }

            var screenIds = new List<string> { SCREEN_ID, "ESS0000012" };
            objStaff.ListTemplate = DB.MDUploadTemplates
                                        .Where(w => screenIds.Contains(w.ScreenId))
                                        .OrderByDescending(w => w.UploadDate)
                                        .ToList();

            Session[Index_Sess_Obj + ActionName] = objStaff;
            return PartialView(SYListConfuration.ListDefaultUpload, objStaff.ListTemplate);
        }

        [HttpGet]
        public ActionResult GenerateUpload(int id)
        {
            UserSession();
            ActionName = "Import";
            MDUploadTemplate obj = DB.MDUploadTemplates.Find(id);
            HumicaDBContext DBB = new HumicaDBContext();
            if (obj != null)
            {
                SYExcel excel = new SYExcel();
                excel.FileName = obj.UpoadPath;
                DataTable dtHeader = excel.GenerateExcelData();
                if (obj.IsGenerate == true)
                {
                    SYMessages mess = SYMessages.getMessageObject("FILE_RG", user.Lang);
                    Session[SYSConstant.MESSAGE_SUBMIT] = mess;
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Import");
                }
                if (dtHeader != null)
                {
                    ATEmpSchObject BSM = new ATEmpSchObject();
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
                    }
                    BSM.ScreenId = ScreendIDControl;
                    BSM.LIstEmplSch = new List<ListEmpSch>();
                    BSDocConfg DocBatch = new BSDocConfg("BATCH_UPLOAD", DocConfType.Normal, true);
                    try
                    {
                        DateTime create = DateTime.Now;
                        if (dtHeader.Rows.Count > 0)
                        {
                            var shift = DBV.HLCheckShiftImports.ToList();
                            shift = shift.ToList();
                            DateTime InDate = BSM.Period;
                            DateTime FromDate = new DateTime(InDate.Year, InDate.Month, 1);
                            DateTime ToDate = new DateTime(InDate.Year, InDate.Month, DateTime.DaysInMonth(InDate.Year, InDate.Month));
                            var ListTemp_Roster = new List<Temp_Roster>();
                            for (int i = 0; i < dtHeader.Rows.Count; i++)
                            {
                                var objHeader = new ListEmpSch();
                                for (var day = FromDate.Date; day.Date <= ToDate.Date; day = day.AddDays(1))
                                {
                                    int Count = day.Day;
                                    var _Temp_Roster = new Temp_Roster();
                                    _Temp_Roster.EmpCode = dtHeader.Rows[i][0].ToString();
                                    _Temp_Roster.Shift = dtHeader.Rows[i][2 + Count].ToString().Trim().ToUpper();
                                    _Temp_Roster.InDate = day;
                                    if (_Temp_Roster.Shift.Trim() != "")
                                    {
                                        var getShift = shift.Where(w => w.Code == _Temp_Roster.Shift).ToList();
                                        if (getShift.Count == 0)
                                        {
                                            obj.Message = "";
                                            obj.Message += "ID: " + _Temp_Roster.EmpCode + " :day " + _Temp_Roster.InDate.Day.ToString() + ": Shift:" + _Temp_Roster.Shift;
                                            obj.IsGenerate = false;
                                            DB.MDUploadTemplates.Attach(obj);
                                            DB.Entry(obj).Property(w => w.Message).IsModified = true;
                                            DB.Entry(obj).Property(w => w.IsGenerate).IsModified = true;
                                            DB.SaveChanges();
                                            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "/Import");
                                            //Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
                                            //return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "/Import");
                                        }
                                    }
                                    ListTemp_Roster.Add(_Temp_Roster);
                                }
                            }
                            var msg = BSM.uploadRoster(ListTemp_Roster, BSM.Period);
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
                        }
                        obj.Message = SYConstant.OK;
                        obj.DocumentNo = DocBatch.NextNumberRank;
                        obj.IsGenerate = true;
                        DBB.MDUploadTemplates.Attach(obj);
                        DBB.Entry(obj).Property(w => w.Message).IsModified = true;
                        DBB.Entry(obj).Property(w => w.DocumentNo).IsModified = true;
                        DBB.Entry(obj).Property(w => w.IsGenerate).IsModified = true;
                        DBB.SaveChanges();
                    }
                    catch (DbEntityValidationException e)
                    {
                        /*------------------SaveLog----------------------------------*/
                        SYEventLog log = new SYEventLog();
                        log.ScreenId = SCREEN_ID;
                        log.UserId = user.UserID.ToString();
                        log.DocurmentAction = "OT_UPLOAD";
                        log.Action = SYActionBehavior.ADD.ToString();

                        SYEventLogObject.saveEventLog(log, e);
                        /*----------------------------------------------------------*/
                        obj.Message = e.Message;
                        obj.IsGenerate = false;
                        DB.MDUploadTemplates.Attach(obj);
                        DB.Entry(obj).Property(w => w.Message).IsModified = true;
                        DB.Entry(obj).Property(w => w.IsGenerate).IsModified = true;
                        DB.SaveChanges();
                    }
                    catch (DbUpdateException e)
                    {
                        obj.Message = e.Message;
                        obj.IsGenerate = false;
                        DB.MDUploadTemplates.Attach(obj);
                        DB.Entry(obj).Property(w => w.Message).IsModified = true;
                        DB.Entry(obj).Property(w => w.IsGenerate).IsModified = true;
                        DB.SaveChanges();
                        /*------------------SaveLog----------------------------------*/
                        SYEventLog log = new SYEventLog();
                        log.ScreenId = SCREEN_ID;
                        log.UserId = user.UserName.ToString();
                        log.ScreenId = "OT_UPLOAD";
                        log.Action = SYActionBehavior.ADD.ToString();

                        SYEventLogObject.saveEventLog(log, e, true);
                        /*----------------------------------------------------------*/

                    }
                    catch (Exception e)
                    {
                        obj.Message = e.Message;
                        obj.IsGenerate = false;
                        DB.MDUploadTemplates.Attach(obj);
                        DB.Entry(obj).Property(w => w.Message).IsModified = true;
                        DB.Entry(obj).Property(w => w.IsGenerate).IsModified = true;
                        DB.SaveChanges();

                        /*------------------SaveLog----------------------------------*/
                        SYEventLog log = new SYEventLog();
                        log.ScreenId = SCREEN_ID;
                        log.UserId = user.UserName.ToString();
                        log.DocurmentAction = "OT_UPLOAD";
                        log.Action = SYActionBehavior.ADD.ToString();

                        SYEventLogObject.saveEventLog(log, e, true);
                        /*----------------------------------------------------------*/
                    }
                }
                else
                {
                    obj.Message = SYMessages.getMessage("EX_DT");
                    obj.IsGenerate = false;
                    DB.MDUploadTemplates.Attach(obj);
                    DB.Entry(obj).Property(w => w.Message).IsModified = true;
                    DB.Entry(obj).Property(w => w.IsGenerate).IsModified = true;
                    DB.SaveChanges();

                }
            }
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "/Import");
        }

        public ActionResult DownloadTemplate()
        {
            UserSession();
            ActionName = "Import";
            var objATEmpSch = new ATEmpSchObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                objATEmpSch = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
                if (objATEmpSch.LIstEmplSch != null)
                {
                    objATEmpSch.LIstEmplSch.Clear();
                }
            }
            DateTime FromDate = new DateTime(objATEmpSch.Period.Year, objATEmpSch.Period.Month, 1);
            DateTime ToDate = new DateTime(objATEmpSch.Period.Year, objATEmpSch.Period.Month,
                DateTime.DaysInMonth(objATEmpSch.Period.Year, objATEmpSch.Period.Month));

            List<ExCFUploadMapping> _List = new List<ExCFUploadMapping>();
            _List.Add(new ExCFUploadMapping { Caption = "Employee Code", FieldName = "Employee Code" });
            _List.Add(new ExCFUploadMapping { Caption = "Employee Name", FieldName = "Employee Name" });
            _List.Add(new ExCFUploadMapping { Caption = "Position", FieldName = "Position" });
            for (var day = FromDate.Date; day.Date <= ToDate.Date; day = day.AddDays(1))
            {
                string Name = day.Day.ToString() + "\r\n" + string.Format("{0:ddd}", day);
                _List.Add(new ExCFUploadMapping { Caption = Name, FieldName = Name });
            }

            var ListBranch = SYConstant.getBranchDataAccess().Select(x => x.Code).ToList();
            var dtHeaderQuery = DB.HRStaffProfiles.AsQueryable()
                .Where(w => ListBranch.Contains(w.Branch))  // Apply branch filter
                .Where(w => string.IsNullOrEmpty(objATEmpSch.Filter.Branch) || w.Branch == objATEmpSch.Filter.Branch)
                .Where(w => string.IsNullOrEmpty(objATEmpSch.Filter.Locations) || w.LOCT == objATEmpSch.Filter.Locations)
                .Where(w => string.IsNullOrEmpty(objATEmpSch.Filter.Division) || w.Division == objATEmpSch.Filter.Division)
                .Where(w => string.IsNullOrEmpty(objATEmpSch.Filter.Department) || w.DEPT == objATEmpSch.Filter.Department)
                .Where(w => objATEmpSch.Filter.IsIncludeBatch || string.IsNullOrEmpty(w.ROSTER))
                .Where(w=> DbFunctions.TruncateTime(w.StartDate.Value) <= ToDate.Date && (w.TerminateStatus == "" 
                || w.TerminateStatus == null || DbFunctions.TruncateTime(w.DateTerminate) >= FromDate.Date))
                .Select(w => new
                {
                    w.EmpCode,
                    w.AllName,
                    w.JobCode,
                    JobDescription = DB.HRPositions.Where(p => p.Code == w.JobCode).Select(p => p.Description).FirstOrDefault(),
                    w.ROSTER
                })
                .ToList();

            var ShiftData = DBV.HLCheckShiftImports.ToList();
            
            if (dtHeaderQuery.Count == 0)
            {
                SYMessages mess = SYMessages.getMessageObject("NOEMP", user.Lang);
                Session[SYSConstant.MESSAGE_SUBMIT] = mess;
                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Import");
            }

            using (var workbook = new DevExpress.Spreadsheet.Workbook())
            {
                // Ensure sheet names are unique
                workbook.Worksheets[0].Name = "Master";
                Worksheet worksheet = workbook.Worksheets[0];
                var sheet2 = workbook.Worksheets.Add("Other_Information");

                List<ExCFUploadMapping> _ListShift = new List<ExCFUploadMapping>();
                _ListShift.Add(new ExCFUploadMapping { Caption = "Code", FieldName = "Code" });
                _ListShift.Add(new ExCFUploadMapping { Caption = "Description", FieldName = "Description" });
                _ListShift.Add(new ExCFUploadMapping { Caption = "Remark", FieldName = "Remark" });

                List<ClsUploadMapping> _ListMaster = new List<ClsUploadMapping>();
                foreach (var read in dtHeaderQuery)
                {
                    _ListMaster.Add(new ClsUploadMapping
                    {
                        FieldName = read.EmpCode,
                        FieldName1 = read.AllName,
                        FieldName2 = read.JobDescription
                    });
                }
                List<ClsUploadMapping> _ListMaster1 = new List<ClsUploadMapping>();
                foreach (var read in ShiftData)
                {
                    _ListMaster1.Add(new ClsUploadMapping
                    {
                        FieldName = read.Code,
                        FieldName1 = read.Description,
                        FieldName2 = read.Remark
                    });
                }
                // Get data for each sheet
                // Export data to each sheet with header formatting
                ClsConstant.ExportDataToWorksheet(worksheet, _List);
                ClsConstant.ExportDataToWorksheet(sheet2, _ListShift);
                ClsConstant.ExportDataToWorksheetRow(worksheet, _ListMaster);
                ClsConstant.ExportDataToWorksheetRow(sheet2, _ListMaster1);

                // Save the workbook to a memory stream
                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveDocument(stream, DevExpress.Spreadsheet.DocumentFormat.Xlsx);
                    stream.Seek(0, System.IO.SeekOrigin.Begin);
                    DateTime DateNow = DateTime.Now;
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ROUSTER_TEMPLATE_" + DateNow.ToString("yyyy_MM_dd_hhmmss") + ".xlsx");
                }
            }
            return null;
        }

        #endregion

        [HttpPost]
        public string getPeriod(DateTime Date, string Action)
        {
            this.ActionName = Action;
            UserSession();
            UserConfListAndForm();

            ATEmpSchObject BSM = new ATEmpSchObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
                DateTime date = new DateTime(Date.Year, Date.Month, 1);
                BSM.Period = date;
                Session[Index_Sess_Obj + ActionName] = BSM;
                return SYConstant.OK;
            }
            else
            {
                return SYMessages.getMessage("PLEASE_SEARCH_ALLOWANCE");
            }
        }
        [HttpPost]
        public string getBranch(string Branch, string Action)
        {
            this.ActionName = Action;
            UserSession();
            UserConfListAndForm();

            ATEmpSchObject BSM = new ATEmpSchObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
                BSM.Filter.Branch = Branch;
                BSM.LIstEmplSch = new List<ListEmpSch>();
                var ListBranch = SYConstant.getBranchDataAccess().Select(x => x.Code).ToList();
                var dtHeaderQuery = DB.HRStaffProfiles.AsQueryable()
                    .Where(w => ListBranch.Contains(w.Branch))  // Apply branch filter
                    .Where(w => string.IsNullOrEmpty(BSM.Filter.Branch) || w.Branch == BSM.Filter.Branch)
                    .Where(w => string.IsNullOrEmpty(BSM.Filter.Locations) || w.LOCT == BSM.Filter.Locations)
                    .Where(w => string.IsNullOrEmpty(BSM.Filter.Division) || w.Division == BSM.Filter.Division)
                    .Where(w => string.IsNullOrEmpty(BSM.Filter.Department) || w.DEPT == BSM.Filter.Department)
                    .Where(w => BSM.Filter.IsIncludeBatch || string.IsNullOrEmpty(w.ROSTER))
                    .Select(w => new
                    {
                        w.EmpCode,
                        w.AllName,
                        w.JobCode,
                        JobDescription = DB.HRPositions.Where(p => p.Code == w.JobCode).Select(p => p.Description).FirstOrDefault()
                    })
                    .ToList();

                var Pos = DB.HRPositions.ToList();

                foreach (var read in dtHeaderQuery)
                {
                    string poss = "";
                    if (read.JobCode != null)
                    {
                        if (Pos.Where(w => w.Code == read.JobCode).Any())
                            poss = Pos.Where(w => w.Code == read.JobCode).First().Description;
                    }

                    var objHeader = new ListEmpSch();

                    objHeader.EmpCode = read.EmpCode;
                    objHeader.AllName = read.AllName;
                    objHeader.Position = poss;
                    objHeader.D_1 = "";
                    objHeader.D_2 = "";
                    objHeader.D_3 = "";
                    objHeader.D_4 = "";
                    objHeader.D_5 = "";
                    objHeader.D_6 = "";
                    objHeader.D_7 = "";
                    objHeader.D_8 = "";
                    objHeader.D_9 = "";
                    objHeader.D_10 = "";
                    objHeader.D_11 = "";
                    objHeader.D_12 = "";
                    objHeader.D_13 = "";
                    objHeader.D_14 = "";
                    objHeader.D_15 = "";
                    objHeader.D_16 = "";
                    objHeader.D_17 = "";
                    objHeader.D_18 = "";
                    objHeader.D_19 = "";
                    objHeader.D_20 = "";
                    objHeader.D_21 = "";
                    objHeader.D_22 = "";
                    objHeader.D_23 = "";
                    objHeader.D_24 = "";
                    objHeader.D_25 = "";
                    objHeader.D_26 = "";
                    objHeader.D_27 = "";
                    objHeader.D_28 = "";
                    objHeader.D_29 = "";
                    objHeader.D_30 = "";
                    objHeader.D_31 = "";

                    BSM.LIstEmplSch.Add(objHeader);
                }

                Session[Index_Sess_Obj + ActionName] = BSM;
                return SYConstant.OK;
            }
            else
            {
                return SYMessages.getMessage("PLEASE_SEARCH_ALLOWANCE");
            }
        }
        [HttpPost]
        public string getLocation(string Location, string Action)
        {
            this.ActionName = Action;
            UserSession();
            UserConfListAndForm();

            ATEmpSchObject BSM = new ATEmpSchObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
                BSM.Filter.Locations = Location;
                BSM.LIstEmplSch = new List<ListEmpSch>();

                var ListBranch = SYConstant.getBranchDataAccess().Select(x => x.Code).ToList();
                var dtHeaderQuery = DB.HRStaffProfiles.AsQueryable()
                    .Where(w => ListBranch.Contains(w.Branch))  // Apply branch filter
                    .Where(w => string.IsNullOrEmpty(BSM.Filter.Branch) || w.Branch == BSM.Filter.Branch)
                    .Where(w => string.IsNullOrEmpty(BSM.Filter.Locations) || w.LOCT == BSM.Filter.Locations)
                    .Where(w => string.IsNullOrEmpty(BSM.Filter.Division) || w.Division == BSM.Filter.Division)
                    .Where(w => string.IsNullOrEmpty(BSM.Filter.Department) || w.DEPT == BSM.Filter.Department)
                    .Where(w => BSM.Filter.IsIncludeBatch || string.IsNullOrEmpty(w.ROSTER))
                    .Select(w => new
                    {
                        w.EmpCode,
                        w.AllName,
                        w.JobCode,
                        JobDescription = DB.HRPositions.Where(p => p.Code == w.JobCode).Select(p => p.Description).FirstOrDefault()
                    })
                    .ToList();

                var Pos = DB.HRPositions.ToList();
                foreach (var read in dtHeaderQuery)
                {
                    string poss = "";
                    if (read.JobCode != null)
                    {
                        if (Pos.Where(w => w.Code == read.JobCode).Any())
                            poss = Pos.Where(w => w.Code == read.JobCode).First().Description;
                    }

                    var objHeader = new ListEmpSch();

                    objHeader.EmpCode = read.EmpCode;
                    objHeader.AllName = read.AllName;
                    objHeader.Position = poss;
                    objHeader.D_1 = "";
                    objHeader.D_2 = "";
                    objHeader.D_3 = "";
                    objHeader.D_4 = "";
                    objHeader.D_5 = "";
                    objHeader.D_6 = "";
                    objHeader.D_7 = "";
                    objHeader.D_8 = "";
                    objHeader.D_9 = "";
                    objHeader.D_10 = "";
                    objHeader.D_11 = "";
                    objHeader.D_12 = "";
                    objHeader.D_13 = "";
                    objHeader.D_14 = "";
                    objHeader.D_15 = "";
                    objHeader.D_16 = "";
                    objHeader.D_17 = "";
                    objHeader.D_18 = "";
                    objHeader.D_19 = "";
                    objHeader.D_20 = "";
                    objHeader.D_21 = "";
                    objHeader.D_22 = "";
                    objHeader.D_23 = "";
                    objHeader.D_24 = "";
                    objHeader.D_25 = "";
                    objHeader.D_26 = "";
                    objHeader.D_27 = "";
                    objHeader.D_28 = "";
                    objHeader.D_29 = "";
                    objHeader.D_30 = "";
                    objHeader.D_31 = "";

                    BSM.LIstEmplSch.Add(objHeader);
                }

                Session[Index_Sess_Obj + ActionName] = BSM;
                return SYConstant.OK;
            }
            else
            {
                return SYMessages.getMessage("PLEASE_SEARCH_ALLOWANCE");
            }
        }

        [HttpPost]
        public string getDivision(string Division, string Action)
        {
            this.ActionName = Action;
            UserSession();
            UserConfListAndForm();

            ATEmpSchObject BSM = new ATEmpSchObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
                BSM.Filter.Division = Division;

                BSM.LIstEmplSch = new List<ListEmpSch>();

                var ListBranch = SYConstant.getBranchDataAccess().Select(x => x.Code).ToList();
                var dtHeaderQuery = DB.HRStaffProfiles.AsQueryable()
                    .Where(w => ListBranch.Contains(w.Branch))  // Apply branch filter
                    .Where(w => string.IsNullOrEmpty(BSM.Filter.Branch) || w.Branch == BSM.Filter.Branch)
                    .Where(w => string.IsNullOrEmpty(BSM.Filter.Locations) || w.LOCT == BSM.Filter.Locations)
                    .Where(w => string.IsNullOrEmpty(BSM.Filter.Division) || w.Division == BSM.Filter.Division)
                    .Where(w => string.IsNullOrEmpty(BSM.Filter.Department) || w.DEPT == BSM.Filter.Department)
                    .Where(w => BSM.Filter.IsIncludeBatch || string.IsNullOrEmpty(w.ROSTER))
                    .Select(w => new
                    {
                        w.EmpCode,
                        w.AllName,
                        w.JobCode,
                        JobDescription = DB.HRPositions.Where(p => p.Code == w.JobCode).Select(p => p.Description).FirstOrDefault()
                    })
                    .ToList();

                var Pos = DB.HRPositions.ToList();


                foreach (var read in dtHeaderQuery)
                {
                    string poss = "";
                    if (read.JobCode != null)
                    {
                        if (Pos.Where(w => w.Code == read.JobCode).Any())
                            poss = Pos.Where(w => w.Code == read.JobCode).First().Description;
                    }

                    var objHeader = new ListEmpSch();

                    objHeader.EmpCode = read.EmpCode;
                    objHeader.AllName = read.AllName;
                    objHeader.Position = poss;
                    objHeader.D_1 = "";
                    objHeader.D_2 = "";
                    objHeader.D_3 = "";
                    objHeader.D_4 = "";
                    objHeader.D_5 = "";
                    objHeader.D_6 = "";
                    objHeader.D_7 = "";
                    objHeader.D_8 = "";
                    objHeader.D_9 = "";
                    objHeader.D_10 = "";
                    objHeader.D_11 = "";
                    objHeader.D_12 = "";
                    objHeader.D_13 = "";
                    objHeader.D_14 = "";
                    objHeader.D_15 = "";
                    objHeader.D_16 = "";
                    objHeader.D_17 = "";
                    objHeader.D_18 = "";
                    objHeader.D_19 = "";
                    objHeader.D_20 = "";
                    objHeader.D_21 = "";
                    objHeader.D_22 = "";
                    objHeader.D_23 = "";
                    objHeader.D_24 = "";
                    objHeader.D_25 = "";
                    objHeader.D_26 = "";
                    objHeader.D_27 = "";
                    objHeader.D_28 = "";
                    objHeader.D_29 = "";
                    objHeader.D_30 = "";
                    objHeader.D_31 = "";

                    BSM.LIstEmplSch.Add(objHeader);
                }


                Session[Index_Sess_Obj + ActionName] = BSM;
                return SYConstant.OK;
            }
            else
            {
                return SYMessages.getMessage("PLEASE_SEARCH_ALLOWANCE");
            }
        }

        [HttpPost]
        public string getDepartment(string Department, string Action)
        {
            this.ActionName = Action;
            UserSession();
            UserConfListAndForm();

            ATEmpSchObject BSM = new ATEmpSchObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
                BSM.Filter.Department = Department;

                BSM.LIstEmplSch = new List<ListEmpSch>();

                var ListBranch = SYConstant.getBranchDataAccess().Select(x => x.Code).ToList();
                var dtHeaderQuery = DB.HRStaffProfiles.AsQueryable()
                    .Where(w => ListBranch.Contains(w.Branch))  // Apply branch filter
                    .Where(w => string.IsNullOrEmpty(BSM.Filter.Branch) || w.Branch == BSM.Filter.Branch)
                    .Where(w => string.IsNullOrEmpty(BSM.Filter.Locations) || w.LOCT == BSM.Filter.Locations)
                    .Where(w => string.IsNullOrEmpty(BSM.Filter.Division) || w.Division == BSM.Filter.Division)
                    .Where(w => string.IsNullOrEmpty(BSM.Filter.Department) || w.DEPT == BSM.Filter.Department)
                    .Where(w => BSM.Filter.IsIncludeBatch || string.IsNullOrEmpty(w.ROSTER))
                    .Select(w => new
                    {
                        w.EmpCode,
                        w.AllName,
                        w.JobCode,
                        JobDescription = DB.HRPositions.Where(p => p.Code == w.JobCode).Select(p => p.Description).FirstOrDefault()
                    })
                    .ToList();
                var Pos = DB.HRPositions.ToList();

                foreach (var read in dtHeaderQuery)
                {
                    string poss = "";
                    if (read.JobCode != null)
                    {
                        if (Pos.Where(w => w.Code == read.JobCode).Any())
                            poss = Pos.Where(w => w.Code == read.JobCode).First().Description;
                    }

                    var objHeader = new ListEmpSch();

                    objHeader.EmpCode = read.EmpCode;
                    objHeader.AllName = read.AllName;
                    objHeader.Position = poss;
                    objHeader.D_1 = "";
                    objHeader.D_2 = "";
                    objHeader.D_3 = "";
                    objHeader.D_4 = "";
                    objHeader.D_5 = "";
                    objHeader.D_6 = "";
                    objHeader.D_7 = "";
                    objHeader.D_8 = "";
                    objHeader.D_9 = "";
                    objHeader.D_10 = "";
                    objHeader.D_11 = "";
                    objHeader.D_12 = "";
                    objHeader.D_13 = "";
                    objHeader.D_14 = "";
                    objHeader.D_15 = "";
                    objHeader.D_16 = "";
                    objHeader.D_17 = "";
                    objHeader.D_18 = "";
                    objHeader.D_19 = "";
                    objHeader.D_20 = "";
                    objHeader.D_21 = "";
                    objHeader.D_22 = "";
                    objHeader.D_23 = "";
                    objHeader.D_24 = "";
                    objHeader.D_25 = "";
                    objHeader.D_26 = "";
                    objHeader.D_27 = "";
                    objHeader.D_28 = "";
                    objHeader.D_29 = "";
                    objHeader.D_30 = "";
                    objHeader.D_31 = "";

                    BSM.LIstEmplSch.Add(objHeader);
                }


                Session[Index_Sess_Obj + ActionName] = BSM;
                return SYConstant.OK;
            }
            else
            {
                return SYMessages.getMessage("PLEASE_SEARCH_ALLOWANCE");
            }
        }

        [HttpPost]
        public string getIncludeBatch(bool IsIncludeBatch, string Action)
        {
            this.ActionName = Action;
            UserSession();
            UserConfListAndForm();

            ATEmpSchObject BSM = new ATEmpSchObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (ATEmpSchObject)Session[Index_Sess_Obj + ActionName];
                BSM.Filter.IsIncludeBatch = IsIncludeBatch;

                BSM.LIstEmplSch = new List<ListEmpSch>();

                var dtHeader = DB.HRStaffProfiles.ToList();

                var Pos = DB.HRPositions.ToList();
                var ListBranch = SYConstant.getBranchDataAccess();
                dtHeader = dtHeader.Where(w => ListBranch.Where(x => x.Code == w.Branch).Any()).ToList();

                if (BSM.Filter.Branch != null && BSM.Filter.Branch != "")
                {
                    dtHeader = dtHeader.Where(w => w.Branch == BSM.Filter.Branch).ToList();
                }
                if (BSM.Filter.Locations != null && BSM.Filter.Locations != "")
                {
                    dtHeader = dtHeader.Where(w => w.LOCT == BSM.Filter.Locations).ToList();
                }
                if (BSM.Filter.Division != null && BSM.Filter.Division != "")
                {
                    dtHeader = dtHeader.Where(w => w.Division == BSM.Filter.Division).ToList();
                }
                if (BSM.Filter.Department != null && BSM.Filter.Department != "")
                {
                    dtHeader = dtHeader.Where(w => w.DEPT == BSM.Filter.Department).ToList();
                }
                if (BSM.Filter.IsIncludeBatch != true)
                {
                    dtHeader = dtHeader.Where(w => string.IsNullOrEmpty(w.ROSTER)).ToList();
                }

                foreach (var read in dtHeader)
                {
                    string poss = "";
                    if (read.JobCode != null)
                    {
                        if (Pos.Where(w => w.Code == read.JobCode).Any())
                            poss = Pos.Where(w => w.Code == read.JobCode).First().Description;
                    }

                    var objHeader = new ListEmpSch();

                    objHeader.EmpCode = read.EmpCode;
                    objHeader.AllName = read.AllName;
                    objHeader.Position = poss;
                    objHeader.D_1 = "";
                    objHeader.D_2 = "";
                    objHeader.D_3 = "";
                    objHeader.D_4 = "";
                    objHeader.D_5 = "";
                    objHeader.D_6 = "";
                    objHeader.D_7 = "";
                    objHeader.D_8 = "";
                    objHeader.D_9 = "";
                    objHeader.D_10 = "";
                    objHeader.D_11 = "";
                    objHeader.D_12 = "";
                    objHeader.D_13 = "";
                    objHeader.D_14 = "";
                    objHeader.D_15 = "";
                    objHeader.D_16 = "";
                    objHeader.D_17 = "";
                    objHeader.D_18 = "";
                    objHeader.D_19 = "";
                    objHeader.D_20 = "";
                    objHeader.D_21 = "";
                    objHeader.D_22 = "";
                    objHeader.D_23 = "";
                    objHeader.D_24 = "";
                    objHeader.D_25 = "";
                    objHeader.D_26 = "";
                    objHeader.D_27 = "";
                    objHeader.D_28 = "";
                    objHeader.D_29 = "";
                    objHeader.D_30 = "";
                    objHeader.D_31 = "";

                    BSM.LIstEmplSch.Add(objHeader);
                }


                Session[Index_Sess_Obj + ActionName] = BSM;
                return SYConstant.OK;
            }
            else
            {
                return SYMessages.getMessage("PLEASE_SEARCH_ALLOWANCE");
            }
        }
        private void DataSelector()
        {
            ViewData["BRANCHES_SELECT"] = SYConstant.getBranchDataAccess();// DB.HRBranches.ToList();
            ViewData["DIVISION_SELECT"] = ClsFilter.LoadDivision();
            ViewData["DEPARTMENT_SELECT"] = ClsFilter.LoadDepartment();
            ViewData["LOCATION_SELECT"] = DB.HRLocations.ToList();
        }
    }
}
