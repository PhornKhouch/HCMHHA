using DevExpress.Web;
using DevExpress.Web.Mvc;
using Humica.Core.DB;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic.LM;
using Humica.Models.SY;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Humica.Controllers.HRM.LMS
{

    public class HRPubHolidayController : Humica.EF.Controllers.MasterSaleController
    {

        private const string SCREEN_ID = "HRS0000009";
        private const string URL_SCREEN = "/HRM/LMS/HRPubHoliday/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "TranNo";
        HumicaDBContext DB = new HumicaDBContext();

        public HRPubHolidayController()
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

            PubHoliDayaObject BSM = new PubHoliDayaObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                var obj = (PubHoliDayaObject)Session[Index_Sess_Obj + ActionName];
            }
            BSM.ListHeader = DB.HRPubHolidays.OrderByDescending(w => w.PDate).ToList();
            Session[Index_Sess_Obj + ActionName] = BSM;

            return View(BSM);
        }
        public ActionResult PartialList()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            PubHoliDayaObject BSM = new PubHoliDayaObject();
            BSM.ListHeader = new List<HRPubHoliday>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (PubHoliDayaObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView(SYListConfuration.ListDefaultView, BSM.ListHeader);
        }
        #endregion

        public ActionResult GridViews()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm();
            PubHoliDayaObject BSM = new PubHoliDayaObject();
            BSM.ListHeader = DB.HRPubHolidays.OrderByDescending(w => w.PDate).ToList();
            return PartialView("GridViews", BSM.ListHeader);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateItem(HRPubHoliday MModel)
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm();
            PubHoliDayaObject BSM = new PubHoliDayaObject();
            if (ModelState.IsValid)
            {
                try
                {
                    var listPH = DB.HRPubHolidays.ToList();
                    if (listPH.Where(w => w.PDate.Date == MModel.PDate.Date).ToList().Count() == 0)
                    {
                        MModel.CreatedOn = DateTime.Now.Date;
                        MModel.CreatedBy = user.UserName;
                        DB.HRPubHolidays.Add(MModel);
                        int row = DB.SaveChanges();
                        //Session[Index_Sess_Obj + ActionName] = BSM;
                    }
                    else
                    {
                        ViewData["EditError"] = SYMessages.getMessage("PUB_EN");
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
            BSM.ListHeader = DB.HRPubHolidays.OrderByDescending(w => w.PDate).ToList();
            return PartialView("GridViews", BSM.ListHeader);
        }
        //edit
        [HttpPost, ValidateInput(false)]
        public ActionResult EditItem(HRPubHoliday MModel)
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm();
            PubHoliDayaObject BSM = new PubHoliDayaObject();
            if (ModelState.IsValid)
            {
                try
                {
                    var DBU = new HumicaDBContext();
                    var listPH = DB.HRPubHolidays.ToList();
                    var objUpdate = listPH.FirstOrDefault(w => w.PDate.Date == MModel.PDate.Date);
                    objUpdate.Description = MModel.Description;
                    objUpdate.SecDescription = MModel.SecDescription;
                    objUpdate.ChangedOn = DateTime.Now.Date;
                    objUpdate.ChangedBy = user.UserName;
                    DB.HRPubHolidays.Attach(objUpdate);
                    DB.Entry(objUpdate).State = System.Data.Entity.EntityState.Modified;
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
            BSM.ListHeader = DB.HRPubHolidays.OrderByDescending(w => w.PDate).ToList();
            return PartialView("GridViews", BSM.ListHeader);
        }
        //delete
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteItem(string PDate)
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm();
            PubHoliDayaObject BSM = new PubHoliDayaObject();
            if (PDate != null)
            {
                try
                {
                    DateTime _PDate = Convert.ToDateTime(PDate);
                    var listPH = DB.HRPubHolidays.ToList();
                    var objUpdate = listPH.FirstOrDefault(w => w.PDate.Date == _PDate);
                    if (objUpdate != null)
                    {
                        DB.HRPubHolidays.Remove(objUpdate);
                        int row = DB.SaveChanges();
                    }
                    BSM.ListHeader = DB.HRPubHolidays.OrderByDescending(w => w.PDate).ToList();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }

            return PartialView("GridViews", BSM.ListHeader);
        }
        #region Import
        public ActionResult Import()
        {
            UserSession();
            ActionName = "Import";
            UserConfListAndForm(ActionBehavior.IMPORT, "UploadName", "HRPubHoliday", SYSConstant.DEFAULT_UPLOAD_LIST);

            var objHoliday = new PubHoliDayaObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                objHoliday = (PubHoliDayaObject)Session[Index_Sess_Obj + ActionName];
                if (objHoliday.ListHeader != null)
                {
                    objHoliday.ListHeader.Clear();
                }

            }

            objHoliday.ListTemplate = DB.MDUploadTemplates.Where(w => w.ScreenId == SCREEN_ID && w.UploadBy == user.UserName).OrderByDescending(w => w.UploadDate).ToList();

            if (objHoliday.ListTemplate.Count > 0)
            {
                SYExcel excel = new SYExcel();
                foreach (var read in objHoliday.ListTemplate.ToList())
                {
                    excel.FileName = read.UpoadPath;
                }


                DataTable dtHeader = excel.GenerateExcelData();
                objHoliday.ListHeader = new List<HRPubHoliday>();
                if (dtHeader != null)
                {
                    for (int i = 0; i < dtHeader.Rows.Count; i++)
                    {
                        var objHeader = new HRPubHoliday();
                        objHeader.PDate = SYSettings.getDateTimeValue(dtHeader.Rows[i][0].ToString());
                        objHeader.Description = dtHeader.Rows[i][1].ToString();
                        objHeader.SecDescription = dtHeader.Rows[i][2].ToString();

                        objHoliday.ListHeader.Add(objHeader);

                    }
                }


            }


            Session[Index_Sess_Obj + ActionName] = objHoliday;
            return View(objHoliday);
        }

        [HttpPost]
        public ActionResult UploadControlCallbackAction(HttpPostedFileBase file_Uploader)
        {

            UserSession();
            ActionName = "Import";
            this.ScreendIDControl = SYSConstant.DEFAULT_UPLOAD_LIST;
            UserConfListAndForm(ActionBehavior.IMPORT, "UploadName", "HRPubHoliday", SYSConstant.DEFAULT_UPLOAD_LIST);
            SYFileImport sfi = new SYFileImport(DB.CFUploadPaths.Find("PUB"));
            sfi.ObjectTemplate = new MDUploadTemplate();
            sfi.ObjectTemplate.UploadDate = DateTime.Now;
            sfi.ObjectTemplate.ScreenId = SCREEN_ID;
            sfi.ObjectTemplate.UploadBy = user.UserName;
            sfi.ObjectTemplate.Module = "HR";
            sfi.ObjectTemplate.IsGenerate = false;

            UploadedFile[] files = UploadControlExtension.GetUploadedFiles("FileUploadOPB",
                sfi.ValidationSettings,
                sfi.uc_FileUploadComplete);


            var objStaff = new PubHoliDayaObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                objStaff = (PubHoliDayaObject)Session[Index_Sess_Obj + ActionName];
            }


            objStaff.ListTemplate = DB.MDUploadTemplates.Where(w => w.ScreenId == SCREEN_ID && w.UploadBy == user.UserName).OrderByDescending(w => w.UploadDate).ToList();
            objStaff.ListHeader = new List<HRPubHoliday>();


            Session[Index_Sess_Obj + ActionName] = objStaff;
            return Redirect(SYUrl.getBaseUrl() + ScreenUrl + "Import");
            //return null;
        }
        public ActionResult UploadList()
        {
            UserSession();
            ActionName = "Import";
            this.ScreendIDControl = SYSConstant.DEFAULT_UPLOAD_LIST;
            UserConfListAndForm(ActionBehavior.IMPORT, "UploadName", "HRPubHoliday", SYSConstant.DEFAULT_UPLOAD_LIST);

            var objStaff = new PubHoliDayaObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                objStaff = (PubHoliDayaObject)Session[Index_Sess_Obj + ActionName];
            }

            objStaff.ListTemplate = DB.MDUploadTemplates.Where(w => w.ScreenId == SCREEN_ID && w.UploadBy == user.UserName).OrderByDescending(w => w.UploadDate).ToList();
            objStaff.ListHeader = new List<HRPubHoliday>();


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
                var objHoliday = new PubHoliDayaObject();
                objHoliday.Header = new HRPubHoliday();
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

                        DateTime create = DateTime.Now;
                        if (dtHeader.Rows.Count > 0)  //Item
                        {
                            objHoliday.ListHeader = new List<HRPubHoliday>();
                            for (int i = 0; i < dtHeader.Rows.Count; i++)
                            {
                                var objHeader = new HRPubHoliday();
                                objHeader.PDate = SYSettings.getDateTimeValue(dtHeader.Rows[i][0].ToString());
                                objHeader.Description = dtHeader.Rows[i][1].ToString();
                                objHeader.SecDescription = dtHeader.Rows[i][2].ToString();


                                objHoliday.ListHeader.Add(objHeader);

                            }

                            msg = objHoliday.uploadPubHoliday();
                            if (msg == SYConstant.OK)
                            {
                                Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("IMPORTED", user.Lang);

                                if (obj != null)
                                {
                                    DB.MDUploadTemplates.Remove(obj);
                                    DB.SaveChanges();

                                    if (System.IO.File.Exists(obj.UpoadPath))
                                    {
                                        System.IO.File.Delete(obj.UpoadPath);
                                    }
                                }

                            }
                            else
                            {
                                //Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessage(msg, user.Lang) + "(" + objStaff.ErrorMessage + ")";
                                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "/Import");
                            }


                        }
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

        public ActionResult DownloadTemplate()
        {
            string fileName = Server.MapPath("~/Content/TEMPLATE/PublicHolidayTemplate.xlsx");
            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;filename=PubHolidayTemplate.xlsx");
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.WriteFile(fileName);
            Response.End();
            return null;
        }
        public ActionResult GridItems()
        {
            ActionName = "Import";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            PubHoliDayaObject objHoliday = new PubHoliDayaObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                objHoliday = (PubHoliDayaObject)Session[Index_Sess_Obj + ActionName];
                if (objHoliday.ListHeader != null)
                {
                    objHoliday.ListHeader.Clear();
                }
            }
            if (objHoliday.ListTemplate.Count > 0)
            {
                SYExcel excel = new SYExcel();
                foreach (var read in objHoliday.ListTemplate.ToList())
                {
                    excel.FileName = read.UpoadPath;
                }
                DataTable dtHeader = excel.GenerateExcelData();
                objHoliday.ListHeader = new List<HRPubHoliday>();
                //objStaff.ListItem = new List<HRStaffProfile>();
                //objStaff.ListPlanItem = new List<HLContractPlanItem>();

                if (dtHeader != null)
                {
                    for (int i = 0; i < dtHeader.Rows.Count; i++)
                    {
                        var objHeader = new HRPubHoliday();
                        objHeader.PDate = SYSettings.getDateTimeValue(dtHeader.Rows[i][0].ToString());
                        objHeader.Description = dtHeader.Rows[i][1].ToString();
                        objHeader.SecDescription = dtHeader.Rows[i][2].ToString();

                        objHoliday.ListHeader.Add(objHeader);

                    }
                }
            }
            //BSM.ListDepreMethod = DB.ExFADepreciationMethods.ToList();
            Session[Index_Sess_Obj + ActionName] = objHoliday;
            return PartialView("GridItems", objHoliday);
        }
        #endregion
        private void DataSelector()
        {
            //SYDataList objList = new SYDataList("FAAVERAGE_SELECT");
            //ViewData["FAAVERAGE_SELECT"] = objList.ListData;
        }
    }
}
