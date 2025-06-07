using DevExpress.Web.Mvc;
using Humica.Core.DB;
using Humica.EF;
using Humica.EF.Models.SY;
using Humica.Logic.Asset;
using Humica.Models.SY;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.Asset
{

    public class HRAssetTransferController : Humica.EF.Controllers.MasterSaleController
    {

        private const string SCREEN_ID = "AM00000004";
        private const string URL_SCREEN = "/Asset/HRAssetTransfer/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName = "";
        private string KeyName = "ID";
        private string PATH_FILE = "12313123123sadfsdfsdfsdf";

        HumicaDBContext DB = new HumicaDBContext();
        HumicaDBViewContext DBV = new HumicaDBViewContext();

        public HRAssetTransferController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
        }


        #region 'List' 
        public ActionResult Index()
        {
            ActionName = "Index";
            DataSelector();
            UserSession();
            UserConfListAndForm(this.KeyName);
            AssetTransferObject BSM = new AssetTransferObject();
            BSM.ListHeader = new List<HRAssetTransfer>();
            BSM.ListStaffAsset = new List<HRAssetStaff>();
            BSM.ListHeader = DB.HRAssetTransfers.ToList();
            BSM.ListStaffAsset = DB.HRAssetStaffs.Where(w => w.Status == "ASSIGN").ToList();
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        public ActionResult PartialList()
        {
            ActionName = "Index";
            AssetTransferObject BSM = new AssetTransferObject();
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (AssetTransferObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView(SYListConfuration.ListDefaultView, BSM.ListHeader);
        }

        public ActionResult PartialProcess()
        {
            ActionName = "Index";
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);
            AssetTransferObject BSM = new AssetTransferObject();
            BSM.ListHeader = new List<HRAssetTransfer>();
            BSM.ListStaffAsset = new List<HRAssetStaff>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (AssetTransferObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("PartialProcess", BSM.ListStaffAsset);
        }
        #endregion
        [HttpPost]
        public ActionResult GetData(DateTime FromDate, int Period, decimal? DedAmount, decimal Amount, string ActionName = "Create")
        {
            this.ActionName = ActionName;
            Session["Index"] = ActionName;
            UserSession();
            UserConfListAndForm();
            if (Period > 0) Period -= 1;
            DateTime ToDate = FromDate.AddMonths(Period);
            AssetTransferObject BSM = new AssetTransferObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (AssetTransferObject)Session[Index_Sess_Obj + ActionName];
                BSM.ListDed = new List<PREmpDeduc>();
                Decimal _DedAmount = (decimal)DedAmount;
                int CountMount = (((ToDate.Year - FromDate.Year) * 12) + ToDate.Month - FromDate.Month);
                int C_Month = (((ToDate.Year - FromDate.Year) * 12) + ToDate.Month - FromDate.Month) + 1;
                int Line = 0;

                for (var i = 0; i <= CountMount; i++)
                {

                    Line += 1;
                    var Ded = new PREmpDeduc();
                    if (Line == C_Month) Amount = _DedAmount;
                    Ded.FromDate = FromDate.AddMonths(i);
                    Ded.ToDate = FromDate.AddMonths(i);
                    if (C_Month == 0) Ded.Amount = Amount;
                    else Ded.Amount = Amount;
                    Ded.TranNo = Line;
                    Ded.StatusAssetDed = SYDocumentStatus.OPEN.ToString();
                    _DedAmount -= Amount;
                    if (_DedAmount < 0)
                    {
                        var rs1 = new { MS = "Invalid Period" };
                        return Json(rs1, JsonRequestBehavior.DenyGet);
                    }
                    if (Ded.Amount > 0)
                        BSM.ListDed.Add(Ded);
                }
                var result = new
                {
                    MS = SYConstant.OK,
                    ToDate = ToDate
                };
                Session[Index_Sess_Obj + ActionName] = BSM;
                return Json(result, JsonRequestBehavior.DenyGet);
            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        #region 'Create'
        public ActionResult Create(int ID)
        {
            ActionName = "Create";
            Session["Index"] = ActionName;
            UserSession();
            UserConfListAndForm();
            DataSelector();

            AssetTransferObject BSM = new AssetTransferObject();
            BSM.Header = new HRAssetTransfer();
            BSM.ListDed = new List<PREmpDeduc>();
            //if (Session[Index_Sess_Obj + ActionName] != null)
            //{
            //    BSM = (AssetTransferObject)Session[Index_Sess_Obj + ActionName];
            //    BSM.Header = new HRAssetTransfer();
            //}
            var AssetStaff = DB.HRAssetStaffs.Where(w => w.ID == ID).FirstOrDefault();
            BSM.Header.EmpCode = AssetStaff.EmpCode;
            BSM.Header.EmployeName = AssetStaff.EmployeName;
            BSM.Header.AssetCode = AssetStaff.AssetCode;
            BSM.Header.AssetDescription = AssetStaff.AssetDescription;
            BSM.Header.AssignDate = AssetStaff.AssignDate;
            BSM.Header.Status = AssetStaff.Status;

            BSM.Header.FromDate = DateTime.Now;
            BSM.Header.ToDate = DateTime.Now;
            BSM.Header.Amount = 0;
            BSM.Header.Period = 1;
            BSM.Header.DedAmount = 0;
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);

        }
        [HttpPost]
        public ActionResult Create(AssetTransferObject obj)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.ADD);


            AssetTransferObject BSM = new AssetTransferObject();

            BSM.Header = new HRAssetTransfer();
            BSM.HeaderDed = new PREmpDeduc();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (AssetTransferObject)Session[Index_Sess_Obj + ActionName];
                BSM.Header = obj.Header;
            }
            if (Session[PATH_FILE] != null)
            {
                obj.Header.Attachment = Session[PATH_FILE].ToString();
                Session[PATH_FILE] = null;
            }
            BSM.ScreenId = SCREEN_ID;
            string msg = BSM.Create();
            if (msg == SYConstant.OK)
            {
                SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                mess.DocumentNumber = BSM.Header.ID.ToString();
                mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details/" + mess.DocumentNumber;

                Session[Index_Sess_Obj + ActionName] = BSM;
                Session[SYConstant.MESSAGE_SUBMIT] = mess;
                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
            }
            else
            {
                ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                return View(BSM);
            }
            //}
            //ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            return View(BSM);
        }
        #endregion 

        #region 'Edit'
        public ActionResult Edit(string id)
        {
            ActionName = "Edit";
            Session["Index"] = ActionName;
            UserSession();
            DataSelector();
            AssetTransferObject BSM = new AssetTransferObject();
            UserConfListAndForm();

            int ID = Convert.ToInt32(id);
            BSM.Header = DB.HRAssetTransfers.FirstOrDefault(w => w.ID == ID);
            if (BSM.Header != null)
            {
                BSM.ListHeader = DB.HRAssetTransfers.Where(w => w.EmpCode == BSM.Header.EmpCode).ToList();
                BSM.HeaderStaff = DBV.HR_STAFF_VIEW.FirstOrDefault(w => w.EmpCode == BSM.Header.EmpCode);
                BSM.ListDed = DB.PREmpDeducs.Where(w => w.AssetTransferID == BSM.Header.ID).ToList();
                Session[Index_Sess_Obj + ActionName] = BSM;
                return View(BSM);
            }
            Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("MATERIAL_NE");
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        [HttpPost]
        public ActionResult Edit(string id, AssetTransferObject collection)
        {
            ActionName = "Edit";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            AssetTransferObject BSM = new AssetTransferObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (AssetTransferObject)Session[Index_Sess_Obj + ActionName];
            }
            //if (ModelState.IsValid)
            //{

            if (Session[PATH_FILE] != null)
            {
                collection.Header.Attachment = Session[PATH_FILE].ToString();
                Session[PATH_FILE] = null;
            }

            BSM.Header = collection.Header;
            BSM.ScreenId = SCREEN_ID;

            string msg = BSM.Update(id);
            if (msg != SYConstant.OK)
            {
                SYMessages mess_err = SYMessages.getMessageObject(msg, user.Lang);
                Session[Index_Sess_Obj + this.ActionName] = BSM;
                UserConfForm(ActionBehavior.SAVEGRID);
                Session[SYConstant.MESSAGE_SUBMIT] = mess_err;
                return View(collection);

            }

            SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
            mess.DocumentNumber = id;
            mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + mess.DocumentNumber; ;
            Session[Index_Sess_Obj + this.ActionName] = collection;
            UserConfForm(ActionBehavior.SAVEGRID);
            Session[SYConstant.MESSAGE_SUBMIT] = mess;
            return View(collection);

            //}
            //Session[Index_Sess_Obj + this.ActionName] = BSM;
            //ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            //return View(BSM);

        }
        #endregion 
        #region 'Details'
        public ActionResult Details(string id)
        {
            ActionName = "Details";
            UserSession();
            DataSelector();

            AssetTransferObject BSM = new AssetTransferObject();

            ViewData[SYConstant.PARAM_ID] = id;
            ViewData[ClsConstant.IS_READ_ONLY] = true;
            int ID = Convert.ToInt32(id);
            BSM.Header = DB.HRAssetTransfers.FirstOrDefault(w => w.ID == ID);
            BSM.ListDed = DB.PREmpDeducs.Where(w => w.AssetTransferID == BSM.Header.ID).ToList();
            if (BSM.ListDed.Count == 0)
            {
                BSM.Header.FromDate = DateTime.Now;
                BSM.Header.ToDate = DateTime.Now;
                BSM.Header.Amount = 0;
                BSM.Header.Period = 1;
                BSM.Header.DedAmount = 0;
            }
            if (BSM.Header == null)
            {
                Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("Error");
                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
            }
            else
            {
                BSM.HeaderStaff = DBV.HR_STAFF_VIEW.FirstOrDefault(w => w.EmpCode == BSM.Header.EmpCode);
                BSM.ListHeader = DB.HRAssetTransfers.Where(w => w.EmpCode == BSM.Header.EmpCode).ToList();
            }
            UserConfForm(SYActionBehavior.VIEW);
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        #endregion
        #region 'Delete'
        public ActionResult Delete(string id)
        {
            UserSession();
            AssetTransferObject BSM = new AssetTransferObject();
            if (id != null)
            {
                string msg = BSM.Delete(id);

                if (msg == SYConstant.OK)
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("MS001", user.Lang);
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
                }
                else
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                }
            }

            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        #endregion
        public ActionResult GridEdit(PREmpDeduc MModel)
        {
            ActionName = Session["Index"].ToString();
            UserSession();
            UserConfListAndForm();
            AssetTransferObject BSM = new AssetTransferObject();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (AssetTransferObject)Session[Index_Sess_Obj + ActionName];
                    }
                    var DBU = new HumicaDBContext();
                    string Open = SYDocumentStatus.OPEN.ToString();
                    var ListHeaderD = BSM.ListDed.Where(w => w.TranNo == MModel.TranNo).ToList();
                    if (ListHeaderD.Where(w => w.StatusAssetDed == Open).Any())
                    {
                        if (ListHeaderD.Count > 0)
                        {
                            var objUpdate = ListHeaderD.First();
                            objUpdate.Amount = MModel.Amount;
                            objUpdate.Remark = MModel.Remark;
                            Session[Index_Sess_Obj + ActionName] = BSM;
                        }
                    }
                    else
                    {
                        ViewData["EditError"] = SYMessages.getMessage("LOAN_READY");
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
            return PartialView("GridItemDetails", BSM);
        }
        public ActionResult GridItemDetails()
        {
            ActionName = Session["Index"].ToString();
            UserSession();
            UserConfForm(ActionBehavior.ACC_REV);
            AssetTransferObject BSM = new AssetTransferObject();
            BSM.ScreenId = SCREEN_ID;

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (AssetTransferObject)Session[Index_Sess_Obj + ActionName];
            }
            DataSelector();
            Session[SYConstant.CURRENT_URL] = URL_SCREEN + "GridItemDetails";
            return PartialView("GridItemDetails", BSM);
        }
        //#region "Print"
        //public ActionResult Print(string id)
        //{
        //    UserSession();
        //    UserConf(ActionBehavior.VIEWONLY);
        //    ViewData[Humica.EF.SYSConstant.PARAM_ID] = id;
        //    UserMVCReportView();
        //    return View("ReportView");
        //}
        //public ActionResult DocumentViewerPartial(string id)
        //{
        //    UserSession();
        //    UserConf(ActionBehavior.VIEWONLY);
        //    ActionName = "Print";
        //    //UserMVC();
        //    int ID = Convert.ToInt32(id);
        //    var SD = DB.HRAssetStaffs.FirstOrDefault(w => w.ID == ID);
        //    if (SD != null)
        //    {
        //        try
        //        {
        //            ViewData[Humica.EF.SYSConstant.PARAM_ID] = id;
        //            var sa = new RptEmpAsset();
        //            var objRpt = DB.CFReportObjects.Where(w => w.ScreenID == SCREEN_ID
        //           && w.IsActive == true).ToList();
        //            if (objRpt.Count > 0)
        //            {
        //                sa.LoadLayoutFromXml(ClsConstant.DEFAULT_REPORT_PATH + objRpt.First().ReportObject);
        //            }
        //            sa.Parameters["EmpCode"].Value = SD.EmpCode;
        //            sa.Parameters["EmpCode"].Visible = false;

        //            Session[Index_Sess_Obj + ActionName] = sa;
        //            Session[Index_Sess_Obj] = sa;
        //            return PartialView("PrintForm", sa);
        //        }
        //        catch (Exception e)
        //        {
        //            /*------------------SaveLog----------------------------------*/
        //            SYEventLog log = new SYEventLog();
        //            log.ScreenId = SCREEN_ID;
        //            log.UserId = user.UserID.ToString();
        //            log.DocurmentAction = id;
        //            log.Action = SYActionBehavior.ADD.ToString();

        //            SYEventLogObject.saveEventLog(log, e, true);
        //            /*----------------------------------------------------------*/
        //        }
        //    }
        //    return null;
        //}
        //public ActionResult DocumentViewerExportTo(string id)
        //{
        //    ActionName = "Print";
        //    if (Session[Index_Sess_Obj] != null)
        //    {
        //        RptEmpAsset reportModel = (RptEmpAsset)Session[Index_Sess_Obj];
        //        return ReportViewerExtension.ExportTo(reportModel);
        //    }
        //    return null;
        //}
        //#endregion
        //#region 'Transfer'
        //public ActionResult Transfer(string id)
        //{
        //    ActionName = "Create";
        //    UserSession();
        //    UserConfForm(SYActionBehavior.EDIT);
        //    DataSelector();
        //    if (id != null)
        //    {
        //        AssetStaffObject BSM = new AssetStaffObject();
        //        if (Session[Index_Sess_Obj + ActionName] != null)
        //        {
        //            BSM = (AssetStaffObject)Session[Index_Sess_Obj + ActionName];
        //        }
        //        BSM.ScreenId = SCREEN_ID;
        //        string msg = BSM.Transfer(id);
        //        if (msg == SYConstant.OK)
        //        {
        //            var mess = SYMessages.getMessageObject("MS001", user.Lang);
        //            mess.Description = mess.Description + ". " + BSM.MessageError;
        //            Session[SYSConstant.MESSAGE_SUBMIT] = mess;
        //        }
        //        else
        //        {
        //            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
        //        }
        //    }
        //    else
        //    {
        //        Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("INV_DOC", user.Lang);
        //    }
        //    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + id);
        //}
        //#endregion 
        //#region "Import & Upload"
        //public ActionResult GridItemsImport()
        //{
        //    ActionName = "Import";
        //    UserSession();
        //    UserConfListAndForm();
        //    DataSelector();
        //    AssetStaffObject BSM = new AssetStaffObject();
        //    if (Session[Index_Sess_Obj + ActionName] != null)
        //    {
        //        BSM = (AssetStaffObject)Session[Index_Sess_Obj + ActionName];
        //    }
        //    Session[Index_Sess_Obj + ActionName] = BSM;
        //    return PartialView("GridItemsImport", BSM);
        //}
        //public ActionResult Import()
        //{
        //    UserSession();
        //    ActionName = "Import";
        //    UserConfListAndForm(ActionBehavior.IMPORT, "UploadName", "HRAssetStaff", SYSConstant.DEFAULT_UPLOAD_LIST);

        //    var BSM = new AssetStaffObject();
        //    if (Session[Index_Sess_Obj + ActionName] != null)
        //    {
        //        BSM = (AssetStaffObject)Session[Index_Sess_Obj + ActionName];
        //    }

        //    BSM.ListTemplate = DB.MDUploadTemplates.Where(w => w.ScreenId == SCREEN_ID && w.UploadBy == user.UserName).OrderByDescending(w => w.UploadDate).ToList();

        //    if (BSM.ListTemplate.Count > 0)
        //    {
        //        SYExcel excel = new SYExcel();
        //        foreach (var read in BSM.ListTemplate.ToList())
        //        {
        //            excel.FileName = read.UpoadPath;
        //        }
        //        DataTable dtHeader = excel.GenerateExcelData();
        //        BSM.ListHeader = new List<HRAssetStaff>();

        //        if (dtHeader != null)
        //        {
        //            for (int i = 0; i < dtHeader.Rows.Count; i++)
        //            {
        //                var objHeader = new HRAssetStaff();
        //                objHeader.EmpCode = dtHeader.Rows[i][0].ToString();
        //                objHeader.EmployeName = dtHeader.Rows[i][1].ToString();
        //                objHeader.AssetCode = dtHeader.Rows[i][2].ToString();
        //                objHeader.AssetDescription = dtHeader.Rows[i][3].ToString();
        //                objHeader.AssignDate = SYSettings.getDateValue(dtHeader.Rows[i][4].ToString());
        //                objHeader.ReturnDate = SYSettings.getDateValue(dtHeader.Rows[i][5].ToString());
        //                objHeader.Status = dtHeader.Rows[i][6].ToString();
        //                //objHeader.Remark = dtHeader.Rows[i][7].ToString();
        //                BSM.ListHeader.Add(objHeader);
        //            }
        //        }

        //    }
        //    Session[Index_Sess_Obj + ActionName] = BSM;
        //    return View(BSM);
        //}

        //[HttpPost]
        //public ActionResult UploadControlCallbackAction(HttpPostedFileBase file_Uploader)
        //{
        //    UserSession();
        //    ActionName = "Import";
        //    this.ScreendIDControl = SYSConstant.DEFAULT_UPLOAD_LIST;
        //    UserConfListAndForm(ActionBehavior.IMPORT, "UploadName", "HRAssetStaff", SYSConstant.DEFAULT_UPLOAD_LIST);
        //    SYFileImport sfi = new SYFileImport(DB.CFUploadPaths.Find("ASSETSTAFF"));
        //    sfi.ObjectTemplate = new MDUploadTemplate();
        //    sfi.ObjectTemplate.UploadDate = DateTime.Now;
        //    sfi.ObjectTemplate.ScreenId = SCREEN_ID;
        //    sfi.ObjectTemplate.UploadBy = user.UserName;
        //    sfi.ObjectTemplate.Module = "HR";
        //    sfi.ObjectTemplate.IsGenerate = false;

        //    UploadedFile[] files = UploadControlExtension.GetUploadedFiles("FileUploadOPB",
        //        sfi.ValidationSettings,
        //        sfi.uc_FileUploadComplete);

        //    AssetStaffObject BSM = new AssetStaffObject();
        //    if (Session[Index_Sess_Obj + ActionName] != null)
        //    {
        //        BSM = (AssetStaffObject)Session[Index_Sess_Obj + ActionName];
        //    }
        //    BSM.ListTemplate = DB.MDUploadTemplates.Where(w => w.ScreenId == SCREEN_ID && w.UploadBy == user.UserName).OrderByDescending(w => w.UploadDate).ToList();
        //    BSM.ListHeader = new List<HRAssetStaff>();

        //    Session[Index_Sess_Obj + ActionName] = BSM;
        //    return Redirect(SYUrl.getBaseUrl() + ScreenUrl + "Import");
        //}
        //public ActionResult UploadList()
        //{
        //    UserSession();
        //    ActionName = "Import";
        //    this.ScreendIDControl = SYSConstant.DEFAULT_UPLOAD_LIST;
        //    UserConfListAndForm(ActionBehavior.IMPORT, "UploadName", "HRAssetStaff", SYSConstant.DEFAULT_UPLOAD_LIST);

        //    var BSM = new AssetStaffObject();
        //    if (Session[Index_Sess_Obj + ActionName] != null)
        //    {
        //        BSM = (AssetStaffObject)Session[Index_Sess_Obj + ActionName];
        //    }

        //    BSM.ListTemplate = DB.MDUploadTemplates.Where(w => w.ScreenId == SCREEN_ID && w.UploadBy == user.UserName).OrderByDescending(w => w.UploadDate).ToList();
        //    BSM.ListHeader = new List<HRAssetStaff>();

        //    Session[Index_Sess_Obj + ActionName] = BSM;
        //    return PartialView(SYListConfuration.ListDefaultUpload, BSM.ListTemplate);
        //}
        //[HttpGet]
        //public ActionResult GenerateUpload(int id)
        //{
        //    UserSession();
        //    MDUploadTemplate obj = DB.MDUploadTemplates.Find(id);
        //    HumicaDBContext DBB = new HumicaDBContext();
        //    if (obj != null)
        //    {
        //        SYExcel excel = new SYExcel();
        //        excel.FileName = obj.UpoadPath;
        //        DataTable dtHeader = excel.GenerateExcelData();
        //        if (obj.IsGenerate == true)
        //        {
        //            SYMessages mess = SYMessages.getMessageObject("FILE_RG", user.Lang);
        //            Session[SYSConstant.MESSAGE_SUBMIT] = mess;
        //            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Import");
        //        }
        //        if (dtHeader != null)
        //        {
        //            try
        //            {
        //                AssetStaffObject BSM = new AssetStaffObject();
        //                BSDocConfg DocBatch = new BSDocConfg("BATCH_UPLOAD", DocConfType.Normal, true);

        //                string msg = SYConstant.OK;

        //                DateTime create = DateTime.Now;
        //                if (dtHeader.Rows.Count > 0)
        //                {
        //                    BSM.ListHeader = new List<HRAssetStaff>();

        //                    for (int i = 0; i < dtHeader.Rows.Count; i++)
        //                    {
        //                        var objHeader = new HRAssetStaff();
        //                        objHeader.EmpCode = dtHeader.Rows[i][0].ToString();
        //                        objHeader.EmployeName = dtHeader.Rows[i][1].ToString();
        //                        objHeader.AssetCode = dtHeader.Rows[i][2].ToString();
        //                        objHeader.AssetDescription = dtHeader.Rows[i][3].ToString();
        //                        objHeader.AssignDate = SYSettings.getDateValue(dtHeader.Rows[i][4].ToString());
        //                        objHeader.ReturnDate = SYSettings.getDateValue(dtHeader.Rows[i][5].ToString());
        //                        objHeader.Status = dtHeader.Rows[i][6].ToString();
        //                        objHeader.Remark = dtHeader.Rows[i][7].ToString();
        //                        BSM.ListHeader.Add(objHeader);
        //                    }

        //                    msg = BSM.Import();
        //                    if (msg != SYConstant.OK)
        //                    {
        //                        obj.Message = SYMessages.getMessage(msg);
        //                        obj.Message += ":" + BSM.MessageError;
        //                        obj.IsGenerate = false;
        //                        DB.MDUploadTemplates.Attach(obj);
        //                        DB.Entry(obj).Property(w => w.Message).IsModified = true;
        //                        DB.Entry(obj).Property(w => w.IsGenerate).IsModified = true;
        //                        DB.SaveChanges();
        //                        return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "/Import");
        //                    }
        //                    Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("GENERATER_COMPLATED", user.Lang);
        //                    obj.DocumentNo = DocBatch.NextNumberRank;
        //                    obj.IsGenerate = true;
        //                    DBB.MDUploadTemplates.Attach(obj);
        //                    DBB.Entry(obj).Property(w => w.Message).IsModified = true;
        //                    DBB.Entry(obj).Property(w => w.DocumentNo).IsModified = true;
        //                    DBB.Entry(obj).Property(w => w.IsGenerate).IsModified = true;
        //                    DBB.SaveChanges();
        //                }
        //            }
        //            catch (DbUpdateException e)
        //            {
        //                /*------------------SaveLog----------------------------------*/
        //                SYEventLog log = new SYEventLog();
        //                log.ScreenId = SCREEN_ID;
        //                log.UserId = user.UserID.ToString();
        //                log.DocurmentAction = "UPLOAD";
        //                log.Action = SYActionBehavior.ADD.ToString();

        //                SYEventLogObject.saveEventLog(log, e, true);
        //                /*----------------------------------------------------------*/
        //                obj.Message = e.Message;
        //                obj.IsGenerate = false;
        //                DB.MDUploadTemplates.Attach(obj);
        //                DB.Entry(obj).Property(w => w.Message).IsModified = true;
        //                DB.Entry(obj).Property(w => w.IsGenerate).IsModified = true;
        //                DB.SaveChanges();
        //            }
        //            catch (Exception e)
        //            {
        //                /*------------------SaveLog----------------------------------*/
        //                SYEventLog log = new SYEventLog();
        //                log.ScreenId = SCREEN_ID;
        //                log.UserId = user.UserID.ToString();
        //                log.DocurmentAction = "UPLOAD";
        //                log.Action = SYActionBehavior.ADD.ToString();

        //                SYEventLogObject.saveEventLog(log, e, true);
        //                obj.Message = e.Message;
        //                obj.IsGenerate = false;
        //                DB.MDUploadTemplates.Attach(obj);
        //                DB.Entry(obj).Property(w => w.Message).IsModified = true;
        //                DB.Entry(obj).Property(w => w.IsGenerate).IsModified = true;
        //                DB.SaveChanges();
        //            }
        //        }

        //    }

        //    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Import");
        //}
        //public ActionResult DownloadTemplate()
        //{
        //    string fileName = Server.MapPath("~/Content/TEMPLATE/TEMPALTE_ASSETSTAFF.xlsx");
        //    Response.Clear();
        //    Response.Buffer = true;
        //    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //    Response.AddHeader("content-disposition", "attachment;filename=TEMPALTE_ASSETSTAFF.xlsx");
        //    Response.Cache.SetCacheability(HttpCacheability.NoCache);
        //    Response.WriteFile(fileName);
        //    Response.End();
        //    return null;
        //}
        //#endregion
        //#region 'Grid'
        //public ActionResult GridItems()
        //{
        //    ActionName = "Create";
        //    UserSession();
        //    UserConfListAndForm();
        //    DataSelector();
        //    AssetStaffObject BSM = new AssetStaffObject();
        //    BSM.ListHeader = new List<HRAssetStaff>();
        //    if (Session[Index_Sess_Obj + ActionName] != null)
        //    {
        //        BSM = (AssetStaffObject)Session[Index_Sess_Obj + ActionName];
        //    }

        //    return PartialView("GridItems", BSM);
        //}
        //#endregion
        //#region private code
        //public ActionResult ShowDataEmp(string EmpCode, string Action)
        //{
        //    ActionName = Action;
        //    AssetStaffObject BSM = new AssetStaffObject();

        //    if (Session[Index_Sess_Obj + ActionName] != null)
        //    {
        //        BSM = (AssetStaffObject)Session[Index_Sess_Obj + ActionName];
        //    }

        //    var Stafff = DBV.HR_STAFF_VIEW.ToList();
        //    HR_STAFF_VIEW EmpStaff = Stafff.SingleOrDefault(w => w.EmpCode == EmpCode);
        //    var _ListAsset = DB.HRAssetStaffs.Where(w => w.EmpCode == EmpCode).ToList();
        //    BSM.ListHeader = _ListAsset;

        //    if (EmpStaff != null)
        //    {
        //        var result = new
        //        {
        //            MS = SYConstant.OK,
        //            AllName = EmpStaff.AllName,
        //            EmpType = EmpStaff.EmpType,
        //            Division = EmpStaff.DivisionDesc,
        //            DEPT = EmpStaff.Department,
        //            SECT = EmpStaff.Section,
        //            LevelCode = EmpStaff.LevelCode,
        //            Position = EmpStaff.Position,
        //            StartDate = EmpStaff.StartDate
        //        };
        //        return Json(result, JsonRequestBehavior.DenyGet);
        //    }
        //    var rs = new { MS = SYConstant.FAIL };
        //    return Json(rs, JsonRequestBehavior.DenyGet);
        //}
        //private AssetStaffObject NewAssign()
        //{
        //    ActionName = "Create";
        //    UserSession();
        //    DataSelector();

        //    AssetStaffObject BSD = new AssetStaffObject();
        //    BSD.Header = new HRAssetStaff();
        //    BSD.ListHeader = new List<HRAssetStaff>();
        //    BSD.Header.AssignDate = DateTime.Now;
        //    BSD.Header.Status = SYDocumentStatus.ASSIGN.ToString();

        //    UserConfListAndForm();
        //    Session[Index_Sess_Obj + ActionName] = BSD;
        //    return BSD;
        //}

        public ActionResult UploadControlCallbackActionImage()
        {
            UserSession();

            if (Session[SYSConstant.IMG_SESSION_KEY_1] != null)
            {
                //DeleteFile(Session[SYSConstant.IMG_SESSION_KEY_1].ToString());
            }

            var path = DB.CFUploadPaths.Find("IMG_UPLOAD");
            var objFile = new SYFileImportImage(path);
            objFile.TokenKey = ClsCrypo.GetUniqueKey(15);

            objFile.ObjectTemplate = new MDUploadImage();
            objFile.ObjectTemplate.ScreenId = SCREEN_ID;
            objFile.ObjectTemplate.Module = "MASTER";
            objFile.ObjectTemplate.TokenCode = objFile.TokenKey;
            objFile.ObjectTemplate.UploadBy = user.UserName;

            Session[SYSConstant.IMG_SESSION_KEY_1] = objFile.TokenKey;
            UploadControlExtension.GetUploadedFiles("UploadControl", objFile.ValidationSettings, objFile.uc_FileUploadComplete);
            Session[PATH_FILE] = objFile.ObjectTemplate.UpoadPath;
            return null;
        }
        private void DataSelector()
        {
            string DEDType = "Ded";
            string status_Open = SYDocumentStatus.OPEN.ToString();
            ViewData["ASSETCODE_SELECT"] = DB.HRAssetRegisters.Where(w => w.StatusUse == status_Open).ToList();
            ViewData["EMP_SELECT"] = DBV.HR_STAFF_VIEW.ToList();
            var ObjAsset = new SYDataList("ASSET_SELECT");
            ViewData["ASSET_SELECT"] = ObjAsset.ListData;
            ViewData["DED_SELECT"] = DB.PR_RewardsType.Where(w => w.ReCode == DEDType).ToList();

        }
        //#endregion
    }
}
