using DevExpress.Spreadsheet;
using DevExpress.Web;
using DevExpress.Web.Mvc;
using Humica.Core.DB;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic;
using Humica.Logic.HR;
using Humica.Models.SY;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Humica.Controllers.HRM.EmployeeInfo
{
    public class CareerHistoryController : Humica.EF.Controllers.MasterSaleController
    {
        private const string SCREEN_ID = "HRE0000002";
        private const string URL_SCREEN = "/HRM/EmployeeInfo/CareerHistory/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string PATH_FILE = "12313123123sadfsdfsdfsdf";
        private string PATH_FILE2 = "12313123123sadfsdfsdfs12fxdf";
        private string KeyName = "TranNo";
        HumicaDBContext DB = new HumicaDBContext();
        HumicaDBViewContext DBV = new HumicaDBViewContext();

        public CareerHistoryController()
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
            DataList();
            DataSelector();
            HRStaffProfileObject BSM = new HRStaffProfileObject();
            BSM.Filter = new Humica.Core.FT.FTFilterEmployee();
            BSM.Filter.Status = SYDocumentStatus.A.ToString();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                var obj = (HRStaffProfileObject)Session[Index_Sess_Obj + ActionName];
                BSM.Filter = obj.Filter;
            }
            BSM.ListEmpCareer = BSM.LoadDataCareer(BSM.Filter.Status);
            BSM.ListEmpCareer = BSM.ListEmpCareer.OrderBy(w => w.FromDate).ToList();
            Session[Index_Sess_Obj + ActionName] = BSM;

            return View(BSM);
        }
        [HttpPost]
        public ActionResult Index(HRStaffProfileObject collection)
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            DataList();
            DataSelector();
            HRStaffProfileObject BSM = new HRStaffProfileObject();
            BSM.Filter = new Humica.Core.FT.FTFilterEmployee();
            BSM.Filter.Status = collection.Filter.Status;
            BSM.ListEmpCareer = BSM.LoadDataCareer(BSM.Filter.Status, collection.Filter.Department, collection.Filter.Office, collection.Filter.Team, collection.Filter.Branch);
            BSM.ListEmpCareer = BSM.ListEmpCareer.OrderBy(w => w.FromDate).ToList();
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        public ActionResult PartialList()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            HRStaffProfileObject BSM = new HRStaffProfileObject();
            BSM.ListEmpCareer = new List<HR_EmpCareer_View>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRStaffProfileObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView(SYListConfuration.ListDefaultView, BSM.ListEmpCareer.OrderBy(x => x.FromDate).ToList());
        }
        #endregion
        #region "Create"
        public ActionResult Create()
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            HRStaffProfileObject BSM = new HRStaffProfileObject();
            BSM.HeaderCareer = new HREmpCareer();
            BSM.ListCareerHis = new List<HR_Career>();
            BSM.HeaderStaff = new HR_STAFF_VIEW();
            BSM.ToDate = "NOW";
            //var IsSalary = BSM.IsHideSalary(BSM.HeaderCareer.LevelCode);
            //if (IsSalary == true)
            //{
            //    BSM.NewSalary = "0";
            //    BSM.OldSalary = "0";
            BSM.Increase = "0";
            //}
            //else
            //{
            //    ViewData[ClsConstant.IS_SALARY] = true;
            //    BSM.NewSalary = "#####";
            //    BSM.OldSalary = "#####";
            //    BSM.Increase = "#####";
            //}
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        [HttpPost]
        public ActionResult Create(HRStaffProfileObject collection)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            HRStaffProfileObject BSM = new HRStaffProfileObject();
            if (ModelState.IsValid)
            {

                //if (Session[PATH_FILE2] != null)
                //{
                //    collection.HeaderCareer.AttachJD = Session[PATH_FILE2].ToString();
                //    Session[PATH_FILE2] = null;
                //}
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (HRStaffProfileObject)Session[Index_Sess_Obj + ActionName];
                }
            }
            if (ModelState.IsValid)
            {

                if (Session[PATH_FILE] != null)
                {
                    collection.HeaderCareer.AttachFile = Session[PATH_FILE].ToString();
                    Session[PATH_FILE] = null;
                }
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (HRStaffProfileObject)Session[Index_Sess_Obj + ActionName];
                }
                BSM.NewSalary = collection.NewSalary;
                BSM.Increase = collection.Increase;
                BSM.HeaderStaff = collection.HeaderStaff;
                BSM.HeaderCareer = collection.HeaderCareer;

                string msg = BSM.CreateCareerStaff();

                if (msg == SYConstant.OK)
                {
                    BSM.ScreenId = SCREEN_ID;
                    SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                    mess.DocumentNumber = BSM.HeaderCareer.TranNo.ToString();
                    mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details/" + mess.DocumentNumber;
                    ViewData[SYConstant.MESSAGE_SUBMIT] = mess;
                    BSM.HeaderCareer = new HREmpCareer();
                    BSM.HeaderStaff = new HR_STAFF_VIEW();
                    BSM.ListCareerHis = new List<HR_Career>();
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
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);

        }
        #endregion
        #region "Edit"
        public ActionResult Edit(int ID)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            HRStaffProfileObject BSM = new HRStaffProfileObject();
            BSM.HeaderCareer = new HREmpCareer();
            if (ID > 0)
            {
                var resualt = DB.HREmpCareers.ToList();
                BSM.IsInAtive = false;
                BSM.HeaderCareer = resualt.FirstOrDefault(w => w.TranNo == ID);
                BSM.ListCareerHis = new List<HR_Career>();
                if (BSM.HeaderCareer != null)
                {
                    var CarCode = DB.HRCareerHistories.Find(BSM.HeaderCareer.CareerCode);
                    if (CarCode != null) BSM.IsInAtive = Convert.ToBoolean(CarCode.NotCalSalary);
                    var IsSalary = BSM.IsHideSalary(BSM.HeaderCareer.LevelCode);
                    if (IsSalary == true)
                    {
                        BSM.NewSalary = BSM.HeaderCareer.NewSalary.ToString();
                        BSM.OldSalary = BSM.HeaderCareer.OldSalary.ToString();
                        BSM.Increase = BSM.HeaderCareer.Increase.ToString();
                    }
                    else
                    {
                        ViewData[ClsConstant.IS_SALARY] = true;
                        BSM.NewSalary = "#####";
                        BSM.OldSalary = "#####";
                        BSM.Increase = "#####";
                    }

                    List<HREmpCareer> listEmpCareer = resualt.Where(x => x.EmpCode == BSM.HeaderCareer.EmpCode).ToList();

                    foreach (var read in listEmpCareer.OrderByDescending(w => w.FromDate).ToList())
                    {
                        var obj = new HR_Career();
                        obj.FromDate = read.FromDate.Value;
                        obj.ToDate = read.ToDate.Value;
                        obj.Department = read.DEPT;
                        obj.Position = read.JobCode;
                        obj.Career = read.CareerCode;
                        obj.Level = read.LevelCode;
                        if (IsSalary == true)
                        {
                            obj.NewSalary = read.NewSalary.ToString();
                            obj.OldSalary = read.OldSalary.ToString();
                            obj.Increase = read.Increase.ToString();
                        }
                        else
                        {
                            obj.NewSalary = "#####";
                            obj.OldSalary = "#####";
                            obj.Increase = "#####";
                        }
                        obj.CreatedBy = read.CreateBy;
                        obj.ChangedBy = read.ChangedBy;
                        obj.CreatedOn = read.CreateOn;
                        obj.ChangedOn = read.ChangedOn;
                        BSM.ListCareerHis.Add(obj);
                    }
                    //var LastCareer = new HREmpCareer();
                    //LastCareer = listEmpCareer.LastOrDefault();
                    //BSM.HeaderCareer = LastCareer;
                    BSM.HeaderStaff = DBV.HR_STAFF_VIEW.FirstOrDefault(x => x.EmpCode == BSM.HeaderCareer.EmpCode);
                    // var resualt = DB.HREmpCareers;
                    Session[Index_Sess_Obj + ActionName] = BSM;
                    return View(BSM);
                }
            }
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("CAREER_NE", user.Lang);
            Session[Index_Sess_Obj + ActionName] = BSM;
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }

        [HttpPost]
        public ActionResult Edit(string ID, HRStaffProfileObject collection)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = ID;
            HRStaffProfileObject BSM = new HRStaffProfileObject();
            Int64 TranNo = 0;
            if (ID != null)
            {


                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (HRStaffProfileObject)Session[Index_Sess_Obj + ActionName];
                }
                //if (Session[PATH_FILE2] != null)
                //{
                //    collection.HeaderCareer.AttachJD = Session[PATH_FILE2].ToString();
                //    Session[PATH_FILE2] = null;
                //}
                //else
                //{
                //    collection.HeaderCareer.AttachJD = BSM.HeaderCareer.AttachJD;
                //}
                if (Session[PATH_FILE2] != null)
                {
                    collection.HeaderCareer.AttachFile = Session[PATH_FILE].ToString();
                    Session[PATH_FILE] = null;
                }
                else
                {
                    collection.HeaderCareer.AttachFile = BSM.HeaderCareer.AttachFile;
                }
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (HRStaffProfileObject)Session[Index_Sess_Obj + ActionName];
                    TranNo = BSM.HeaderCareer.TranNo;
                    if (collection.HeaderCareer.CareerCode == null)
                        collection.HeaderCareer.CareerCode = BSM.HeaderCareer.CareerCode;
                    BSM.HeaderCareer = collection.HeaderCareer;
                    BSM.HeaderCareer.TranNo = TranNo;
                    BSM.HeaderCareer.EmpCode = collection.HeaderStaff.EmpCode;
                    BSM.NewSalary = collection.NewSalary;
                    BSM.Increase = collection.Increase;
                }
                try
                {
                    string msg = "";

                    //if (collection.HeaderCareer.CareerCode == "TERMINAT")
                    //{
                    //    msg = BSM.editCareeUser(BSM.HeaderStaff.EmpCode);

                    //    //if (msg != SYConstant.OK)
                    //    //{
                    //    //    Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
                    //    //    Session[Index_Sess_Obj + ActionName] = BSM;
                    //    //    return View(BSM);
                    //    //}
                    //}

                    msg = BSM.EditCareerStaff(ID);

                    if (msg == SYConstant.OK)
                    {
                        BSM.ListCareerHis = new List<HR_Career>();
                        BSM.ScreenId = SCREEN_ID;
                        SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                        mess.DocumentNumber = ID;
                        mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details/" + mess.DocumentNumber;
                        ViewData[SYConstant.MESSAGE_SUBMIT] = mess;
                        var listEmpCareer = DB.HREmpCareers.Where(w => w.EmpCode == collection.HeaderStaff.EmpCode).ToList();
                        foreach (var read in listEmpCareer.OrderByDescending(w => w.FromDate).ToList())
                        {
                            var obj = new HR_Career();
                            obj.FromDate = read.FromDate.Value;
                            obj.ToDate = read.ToDate.Value;
                            obj.Department = read.DEPT;
                            obj.Position = read.JobCode;
                            obj.Career = read.CareerCode;
                            obj.Level = read.LevelCode;
                            obj.AttachFile = read.AttachFile;

                            var IsSalary = BSM.IsHideSalary(BSM.HeaderCareer.LevelCode);
                            if (IsSalary == true)
                            {
                                obj.NewSalary = read.NewSalary.ToString();
                                obj.OldSalary = read.OldSalary.ToString();
                                obj.Increase = read.Increase.ToString();
                            }
                            else
                            {
                                obj.NewSalary = "#####";
                                obj.OldSalary = "#####";
                                obj.Increase = "#####";
                            }
                            obj.CreatedBy = read.CreateBy;
                            obj.ChangedBy = read.ChangedBy;
                            BSM.ListCareerHis.Add(obj);
                        }
                        Session[Index_Sess_Obj + ActionName] = BSM;
                        return View(BSM);

                    }
                    else
                    {
                        ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                        Session[Index_Sess_Obj + ActionName] = BSM;
                        return View(BSM);
                    }

                }
                catch (Exception e)
                {
                    /*------------------SaveLog----------------------------------*/
                    SYEventLog log = new SYEventLog();
                    log.ScreenId = SCREEN_ID;
                    log.UserId = user.UserID.ToString();
                    log.DocurmentAction = ID;
                    log.Action = SYActionBehavior.ADD.ToString();
                    ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
                    SYEventLogObject.saveEventLog(log, e, true);
                    /*----------------------------------------------------------*/
                }

            }
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);

        }
        #endregion
        #region "Detail"
        public ActionResult Details(string id)
        {
            ActionName = "Details";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            SetSessionNULL();
            ViewData[SYSConstant.PARAM_ID] = id;
            ViewData[ClsConstant.IS_READ_ONLY] = true;
            if (id.ToString() != "null")
            {
                HRStaffProfileObject BSM = new HRStaffProfileObject();
                BSM.ListCareerHis = new List<HR_Career>();
                BSM.HeaderCareer = new HREmpCareer();
                int TranNo = Convert.ToInt32(id);
                BSM.HeaderCareer = DB.HREmpCareers.FirstOrDefault(x => x.TranNo == TranNo);
                if (BSM.HeaderCareer != null)
                {
                    var result = DB.HREmpCareers.Where(w => w.EmpCode == BSM.HeaderCareer.EmpCode).ToList();
                    List<HREmpCareer> listEmpCareer = result.ToList();
                    BSM.HeaderStaff = DBV.HR_STAFF_VIEW.FirstOrDefault(x => x.EmpCode == BSM.HeaderCareer.EmpCode);
                    var IsSalary = BSM.IsHideSalary(BSM.HeaderCareer.LevelCode);

                    if (IsSalary == true)
                    {
                        BSM.NewSalary = BSM.HeaderCareer.NewSalary.ToString();
                        BSM.OldSalary = BSM.HeaderCareer.OldSalary.ToString();
                        BSM.Increase = BSM.HeaderCareer.Increase.ToString();
                    }
                    else
                    {
                        BSM.NewSalary = "#####";
                        BSM.OldSalary = "#####";
                        BSM.Increase = "#####";
                    }
                    var objCar = BSM.HeaderCareer;
                    FitlertDataSession(objCar.Branch, objCar.Division, objCar.GroupDept, objCar.DEPT, objCar.SECT,
                        objCar.JobCode, objCar.LevelCode, objCar.JobGrade);
                    foreach (var read in listEmpCareer.OrderByDescending(w => w.FromDate).ToList())
                    {
                        var obj = new HR_Career();
                        obj.FromDate = read.FromDate.Value;
                        obj.ToDate = read.ToDate.Value;
                        obj.Department = read.DEPT;
                        obj.Position = read.JobCode;
                        obj.Career = read.CareerCode;
                        obj.Level = read.LevelCode;
                        if (IsSalary == true)
                        {
                            obj.NewSalary = read.NewSalary.ToString();
                            obj.OldSalary = read.OldSalary.ToString();
                            obj.Increase = read.Increase.ToString();
                        }
                        else
                        {
                            obj.NewSalary = "#####";
                            obj.OldSalary = "#####";
                            obj.Increase = "#####";
                        }
                        obj.CreatedBy = read.CreateBy;
                        obj.ChangedBy = read.ChangedBy;
                        obj.CreatedOn = read.CreateOn;
                        obj.ChangedOn = read.ChangedOn;
                        BSM.ListCareerHis.Add(obj);
                    }
                    Session[Index_Sess_Obj + ActionName] = BSM;
                    return View(BSM);
                }
            }
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("CAREER_NE", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

        }
        #endregion
        #region "Delete"
        public ActionResult Delete(string id)
        {
            UserSession();
            UserConfForm(SYActionBehavior.EDIT);
            // DataSelector();
            if (id != null)
            {
                HRStaffProfileObject GLA = new HRStaffProfileObject();
                string msg = GLA.Delete_EmployeeCareerHistory(id);
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
                Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("CAREER_NE", user.Lang);
            }
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

        }

        #endregion

        #region Download Document
        public ActionResult DownloadENG(string id, string SignEmpCode)
        {
            ActionName = "Index";
            UserSession();
            DataSelector();
            var BSM = new HRStaffProfileObject();
            if (id == "null") id = null;
            if (id != null)
            {
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (HRStaffProfileObject)Session[Index_Sess_Obj + ActionName];
                }
                string fileName = "";

                int TranNo = Convert.ToInt32(id);
                var empCareer = DB.HREmpCareers.FirstOrDefault(w => w.TranNo == TranNo);
                if (empCareer != null)
                {
                    var CareerC = DB.HRCareerHistories.FirstOrDefault(w => w.Code == empCareer.CareerCode);
                    if (CareerC != null)
                    {
                        if (CareerC.TemplatePath != "" && CareerC.TemplatePath != null)
                        {
                            fileName = CareerC.TemplatePath;
                        }
                        else
                        {
                            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("TEMPLATE", user.Lang);

                            //ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("TEMPLATE", user.Lang);
                            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
                        }
                    }
                    SYExecuteFindAndReplace Para = new SYExecuteFindAndReplace();
                    string TfileName = Server.MapPath(fileName);
                    var STAFFP = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == empCareer.EmpCode);
                    var POS = DB.HRPositions.FirstOrDefault(w => w.Code == empCareer.JobCode);
                    var NATION = DB.HRNations.FirstOrDefault(w => w.Code == STAFFP.Nation);


                    string FileSource = Server.MapPath("~/Content/TEMPLATE/" + empCareer.EmpCode + CareerC.Description + ".docx");

                    Para.ListObjectDictionary = new List<object>();
                    if (POS != null)
                    {
                        Para.ListObjectDictionary.Add(POS);
                    }
                    Para.ListObjectDictionary.Add(empCareer);
                    Para.ListObjectDictionary.Add(STAFFP);

                    var SIGNEmp = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == SignEmpCode);
                    Para.HDHeader = new ClsHeadDepartment();
                    if (SIGNEmp != null)
                    {
                        Para.HDHeader.HDName = SIGNEmp.AllName;
                        Para.HDHeader.HDNameKH = SIGNEmp.OthAllName;
                        var HDPOS = DB.HRPositions.FirstOrDefault(w => w.Code == SIGNEmp.JobCode);
                        if (HDPOS != null)
                        {
                            Para.HDHeader.HDPosition = HDPOS.Description;
                            Para.HDHeader.HDPositionKH = HDPOS.SecDescription;
                        }
                    }

                    var msg = Para.ExecuteFindAndReplaceDOC(TfileName, FileSource, "EmpCareer");
                    if (msg == SYConstant.OK)
                    {

                        Response.Clear();
                        Response.Buffer = true;
                        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        Response.AddHeader("content-disposition", "attachment;filename=" + empCareer.EmpCode + CareerC.Description + ".docx");
                        Response.Cache.SetCacheability(HttpCacheability.NoCache);
                        Response.WriteFile(FileSource);
                        Response.End();

                    }
                    else
                    {
                        Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                    }
                    //return null;
                    //return RedirectToAction("Index");
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

                }
            }
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

        }
        public ActionResult DownloadKH(string id, string SignEmpCode)
        {
            ActionName = "Index";
            UserSession();
            //UserConfForm(SYActionBehavior.EDIT);
            DataSelector();
            var BSM = new HRStaffProfileObject();
            if (id == "null") id = null;
            if (id != null)
            {
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (HRStaffProfileObject)Session[Index_Sess_Obj + ActionName];
                    //BSM.Filter.InMonth = Temp;
                }
                string fileName = "";// Server.MapPath("~/Content/UPLOAD/Contract.docx");

                int TranNo = Convert.ToInt32(id);
                var empCareer = DB.HREmpCareers.FirstOrDefault(w => w.TranNo == TranNo);
                if (empCareer != null)
                {
                    var CareerC = DB.HRCareerHistories.FirstOrDefault(w => w.Code == empCareer.CareerCode);
                    if (CareerC != null)
                    {
                        if (CareerC.TemplatePathKH != "" && CareerC.TemplatePathKH != null)
                        {
                            fileName = CareerC.TemplatePathKH;
                        }
                        else
                        {
                            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("TEMPLATE", user.Lang);
                            //ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("TEMPLATE", user.Lang);
                            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
                        }
                    }
                    SYExecuteFindAndReplace Para = new SYExecuteFindAndReplace();
                    string TfileName = Server.MapPath(fileName);
                    var STAFFP = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == empCareer.EmpCode);
                    //var STAFF = DB.HR_STAFF_VIEW.FirstOrDefault(w => w.EmpCode == CareerC.EmpCode);
                    var POS = DB.HRPositions.FirstOrDefault(w => w.Code == empCareer.JobCode);
                    //var EffDate = DB.HREmpCareerEffectDates.FirstOrDefault(w => w.EmpCode == empCareer.EmpCode);
                    //var NATION = DB.HRNations.FirstOrDefault(w => w.Code == STAFFP.Nation);
                    if (POS.SecDescription == null)
                    {
                        POS.SecDescription = POS.Description;
                    }

                    var SIGNEmp = DB.HRStaffProfiles.FirstOrDefault(w => w.EmpCode == SignEmpCode);
                    Para.HDHeader = new ClsHeadDepartment();
                    if (SIGNEmp != null)
                    {
                        Para.HDHeader.HDName = SIGNEmp.AllName;
                        Para.HDHeader.HDNameKH = SIGNEmp.OthAllName;
                        var HDPOS = DB.HRPositions.FirstOrDefault(w => w.Code == SIGNEmp.JobCode);
                        if (HDPOS != null)
                        {
                            Para.HDHeader.HDPosition = HDPOS.Description;
                            Para.HDHeader.HDPositionKH = HDPOS.SecDescription;
                        }
                    }

                    string FileSource = Server.MapPath("~/Content/TEMPLATE/" + empCareer.EmpCode + CareerC.Description + "KH.docx");

                    Para.ListObjectDictionary = new List<object>();
                    // var contract = DB.HREmpContracts.FirstOrDefault(w => w.TranNo == TranNo);
                    //Para.ListObjectDictionary.Add(contract);
                    //Para.ListObjectDictionary.Add(STAFF);
                    Para.ListObjectDictionary.Add(POS);
                    //Para.ListObjectDictionary.Add(Company);
                    Para.ListObjectDictionary.Add(empCareer);
                    // Para.ListObjectDictionary.Add(EffDate);
                    Para.ListObjectDictionary.Add(STAFFP);
                    //if (Province != null)
                    //{
                    //    Para.ListObjectDictionary.Add(Province);
                    //}
                    //if (NID != null)
                    //{
                    //    Para.ListObjectDictionary.Add(NID);
                    //}
                    //if (NATION != null)
                    //{
                    //    Para.ListObjectDictionary.Add(NATION);
                    //}   
                    // Para.ListObjectDictionary.Add(StartDate);
                    var msg = Para.ExecuteFindAndReplaceDOC(TfileName, FileSource, "EmpCareer");
                    if (msg == SYConstant.OK)
                    {

                        Response.Clear();
                        Response.Buffer = true;
                        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        Response.AddHeader("content-disposition", "attachment;filename=" + empCareer.EmpCode + CareerC.Description + "KH.docx");
                        Response.Cache.SetCacheability(HttpCacheability.NoCache);
                        Response.WriteFile(FileSource);
                        Response.End();

                    }
                    else
                    {
                        Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                    }
                    //return null;
                    //return RedirectToAction("Index");
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

                }
            }
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

        }
        #endregion
        public ActionResult UploadControlCallbackActionJobDis()
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
            UploadControlExtension.GetUploadedFiles("Uploadjobdisc", objFile.ValidationSettings, objFile.uc_FileUploadComplete);
            Session[PATH_FILE2] = objFile.ObjectTemplate.UpoadPath;
            return null;
        }
        #region "Import"
        public ActionResult GridItemHis()
        {
            ActionName = "Import";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRStaffProfileObject objStaff = new HRStaffProfileObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                objStaff = (HRStaffProfileObject)Session[Index_Sess_Obj + ActionName];
                if (objStaff.ListEmp != null)
                {
                    objStaff.ListEmp.Clear();
                }
            }
            if (objStaff.ListTemplate.Count > 0)
            {
                SYExcel excel = new SYExcel();
                foreach (var read in objStaff.ListTemplate.ToList())
                {
                    excel.FileName = read.UpoadPath;
                }
            }
            Session[Index_Sess_Obj + ActionName] = objStaff;
            return PartialView("GridItemHis", objStaff);
        }
        public ActionResult Import()
        {
            UserSession();
            ActionName = "Import";
            UserConfListAndForm(ActionBehavior.IMPORT, "UploadName", "CareerHistory", SYSConstant.DEFAULT_UPLOAD_LIST);

            var BSM = new HRStaffProfileObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRStaffProfileObject)Session[Index_Sess_Obj + ActionName];
                if (BSM.ListEmp != null)
                {
                    BSM.ListEmp.Clear();
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
            UserConfListAndForm(ActionBehavior.IMPORT, "UploadName", "CareerHistory", SYSConstant.DEFAULT_UPLOAD_LIST);
            SYFileImport sfi = new SYFileImport(DB.CFUploadPaths.Find("STAFF_CAREER"));
            sfi.ObjectTemplate = new MDUploadTemplate();
            sfi.ObjectTemplate.UploadDate = DateTime.Now;
            sfi.ObjectTemplate.ScreenId = SCREEN_ID;
            sfi.ObjectTemplate.UploadBy = user.UserName;
            sfi.ObjectTemplate.Module = "STAFF";
            sfi.ObjectTemplate.IsGenerate = false;

            UploadedFile[] files = UploadControlExtension.GetUploadedFiles("FileUploadOPB",
                sfi.ValidationSettings,
                sfi.uc_FileUploadComplete);

            return Redirect(SYUrl.getBaseUrl() + ScreenUrl + "Import");
        }

        public ActionResult UploadList()
        {
            UserSession();
            ActionName = "Import";
            this.ScreendIDControl = SYSConstant.DEFAULT_UPLOAD_LIST;
            UserConfListAndForm(ActionBehavior.IMPORT, "UploadName", "CareerHistory", SYSConstant.DEFAULT_UPLOAD_LIST);
            IEnumerable<MDUploadTemplate> listu = DB.MDUploadTemplates.Where(w => w.ScreenId == SCREEN_ID).OrderByDescending(w => w.UploadDate);
            return PartialView(SYListConfuration.ListDefaultUpload, listu.ToList());
        }

        public ActionResult GenerateUpload(int id)
        {
            UserSession();
            MDUploadTemplate obj = DB.MDUploadTemplates.Find(id);
            HumicaDBContext DBB = new HumicaDBContext();
            if (obj != null)
            {
                SYExcel excel = new SYExcel();
                excel.FileName = obj.UpoadPath;
                DataTable dt = excel.GenerateExcelData();

                if (obj.IsGenerate == true)
                {
                    SYMessages mess = SYMessages.getMessageObject("FILE_RG", user.Lang);
                    Session[SYSConstant.MESSAGE_SUBMIT] = mess;
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Import");
                }
                if (dt != null)
                {
                    var BSM = new HRStaffProfileObject();

                    BSM.ScreenId = ScreendIDControl;
                    BSM.ListCareer = new List<HREmpCareer>();
                    try
                    {

                        DateTime create = DateTime.Now;
                        if (dt.Rows.Count > 0)
                        {

                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                var objHeader = new HREmpCareer();
                                objHeader.EmpCode = dt.Rows[i][0].ToString();
                                objHeader.CareerCode = dt.Rows[i][2].ToString();
                                objHeader.Increase = (decimal)SYSettings.getNumberValue(dt.Rows[i][5].ToString());
                                objHeader.EffectDate = SYSettings.getDateValue(dt.Rows[i][4].ToString());
                                objHeader.resigntype = dt.Rows[i][3].ToString();
                                objHeader.Branch = dt.Rows[i][6].ToString();
                                objHeader.DEPT = dt.Rows[i][7].ToString();
                                objHeader.Office = dt.Rows[i][8].ToString();
                                objHeader.SECT = dt.Rows[i][9].ToString();
                                objHeader.Groups = dt.Rows[i][10].ToString();
                                objHeader.JobCode = dt.Rows[i][11].ToString();
                                objHeader.LevelCode = dt.Rows[i][12].ToString();
                                objHeader.Remark = dt.Rows[i][13].ToString();
                                BSM.ListCareer.Add(objHeader);
                            }

                            var msg = BSM.uploadCareer();
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
                        //obj.DocumentNo = DocBatch.NextNumberRank;
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
                        log.UserId = user.UserName.ToString();
                        log.DocurmentAction = "UPLOAD";
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
                        log.ScreenId = "UPLOAD";
                        log.Action = SYActionBehavior.ADD.ToString();

                        SYEventLogObject.saveEventLog(log, e, true);
                        /*----------------------------------------------------------*/

                    }
                    catch (Exception e)
                    {
                        /*------------------SaveLog----------------------------------*/
                        SYEventLog log = new SYEventLog();
                        log.ScreenId = SCREEN_ID;
                        log.UserId = user.UserName.ToString();
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
           
            var RelCode = DB.HRCareerHistories.ToList();
            var SepCode = DB.HRTerminTypes.ToList();
            using (var workbook = new DevExpress.Spreadsheet.Workbook())
            {
                workbook.Worksheets[0].Name = "Master";
                List<ExCFUploadMapping> _ListMaster = new List<ExCFUploadMapping>();
                _ListMaster.Add(new ExCFUploadMapping { FieldName = "Employee ID" });
                _ListMaster.Add(new ExCFUploadMapping { FieldName = "Employee Name" });
                _ListMaster.Add(new ExCFUploadMapping { FieldName = "Career Code" });
                _ListMaster.Add(new ExCFUploadMapping { FieldName = "Separate Type" });
                _ListMaster.Add(new ExCFUploadMapping { FieldName = "EffectDate\n(date)" });
                _ListMaster.Add(new ExCFUploadMapping { FieldName = "Increase\n(number)" });
                _ListMaster.Add(new ExCFUploadMapping { FieldName ="Branch" });
                _ListMaster.Add(new ExCFUploadMapping { FieldName ="Department" });
                _ListMaster.Add(new ExCFUploadMapping { FieldName ="Office" });
                _ListMaster.Add(new ExCFUploadMapping { FieldName ="Section" });
                _ListMaster.Add(new ExCFUploadMapping { FieldName ="Team" });
                _ListMaster.Add(new ExCFUploadMapping { FieldName ="Position" });
                _ListMaster.Add(new ExCFUploadMapping { FieldName = "Level" });
                _ListMaster.Add(new ExCFUploadMapping { FieldName = "Remark" });
                Worksheet worksheet = workbook.Worksheets[0];
                var sheet2 = workbook.Worksheets.Add("Career-Code");
                List<ExCFUploadMapping> _ListMaster1 = new List<ExCFUploadMapping>();
                _ListMaster1.Add(new ExCFUploadMapping { FieldName = "Code" });
                _ListMaster1.Add(new ExCFUploadMapping { FieldName= "Description" });
                List<ClsUploadMapping> _ListData = new List<ClsUploadMapping>();
                foreach (var read in RelCode)
                {
                    _ListData.Add(new ClsUploadMapping
                    {
                        FieldName = read.Code,
                        FieldName1 = read.Description,
                        FieldName2 = read.SecDescription
                    });
                }
                var sheet3 = workbook.Worksheets.Add("Separate-Type");
                List<ExCFUploadMapping> _ListMaster2 = new List<ExCFUploadMapping>();
                _ListMaster2.Add(new ExCFUploadMapping { FieldName = "Code" });
                _ListMaster2.Add(new ExCFUploadMapping { FieldName = "Description" });
                List<ClsUploadMapping> _ListData2 = new List<ClsUploadMapping>();
                foreach (var read in SepCode)
                {
                    _ListData2.Add(new ClsUploadMapping
                    {
                        FieldName = read.Code,
                        FieldName1 = read.Description,
                        FieldName2 = read.OthDesc
                    });
                }
                ClsConstant.ExportDataToWorksheet(worksheet, _ListMaster);
                ClsConstant.ExportDataToWorksheet(sheet2, _ListMaster1);
                ClsConstant.ExportDataToWorksheetRow(sheet2, _ListData);
                //
                ClsConstant.ExportDataToWorksheet(sheet3, _ListMaster2);
                ClsConstant.ExportDataToWorksheetRow(sheet3, _ListData2);
                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveDocument(stream, DevExpress.Spreadsheet.DocumentFormat.Xlsx);
                    stream.Seek(0, System.IO.SeekOrigin.Begin);

                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CAREER_TEMPLATE.xlsx");
                }
            }
            return null;
        }

        #endregion
        public ActionResult GridItems()
        {
            ActionName = "Create";
            UserSession();
            UserConfForm(ActionBehavior.ACC_REV);
            DataSelector();
            HRStaffProfileObject BSM = new HRStaffProfileObject();
            BSM.ScreenId = SCREEN_ID;

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRStaffProfileObject)Session[Index_Sess_Obj + ActionName];
            }
            Session[SYConstant.CURRENT_URL] = URL_SCREEN + "GridItems";
            return PartialView("GridItems", BSM.ListCareerHis);
        }
        public ActionResult ShowData(string ID, string Tran)
        {
            ActionName = "Details";
            HRStaffProfileObject BSM = new HRStaffProfileObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRStaffProfileObject)Session[Index_Sess_Obj + ActionName];
                int TranNo = Convert.ToInt32(Tran);
                var Carceer = BSM.ListCareer.FirstOrDefault(x => x.TranNo == TranNo);
                BSM.HeaderCareer = Carceer;
                var IsSalary = BSM.IsHideSalary(BSM.HeaderCareer.LevelCode);
                if (IsSalary == true)
                {
                    ViewData[ClsConstant.IS_SALARY] = false;
                    BSM.NewSalary = BSM.HeaderCareer.NewSalary.ToString();
                    BSM.OldSalary = BSM.HeaderCareer.OldSalary.ToString();
                    BSM.Increase = BSM.HeaderCareer.Increase.ToString();
                }
                else
                {
                    ViewData[ClsConstant.IS_SALARY] = true;
                    BSM.NewSalary = "#####";
                    BSM.OldSalary = "#####";
                    BSM.Increase = "#####";
                }
                var result = new
                {
                    MS = SYConstant.OK,
                    CompanyCode = BSM.HeaderCareer.CompanyCode,
                    EmpType = BSM.HeaderCareer.EmpType,
                    Branch = BSM.HeaderCareer.Branch,
                    Division = BSM.HeaderCareer.Division,
                    DEPT = BSM.HeaderCareer.DEPT,
                    SECT = BSM.HeaderCareer.SECT,
                    JobCode = BSM.HeaderCareer.JobCode,
                    LevelCode = BSM.HeaderCareer.LevelCode,
                    CareerCode = BSM.HeaderCareer.CareerCode,
                    EffectDate = BSM.HeaderCareer.EffectDate,
                    OldSalary = BSM.OldSalary,
                    Increase = BSM.Increase,
                    NewSalary = BSM.NewSalary,
                    ToDate = BSM.HeaderCareer.ToDate,
                    Cate = BSM.HeaderCareer.CATE,
                    Remark = BSM.HeaderCareer.Remark
                };

                return Json(result, JsonRequestBehavior.DenyGet);

            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        public ActionResult Refreshvalue(string id, string Increase)
        {
            ActionName = "Create";
            HRStaffProfileObject BSM = new HRStaffProfileObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                decimal Inc = Convert.ToDecimal(Increase);
                BSM = (HRStaffProfileObject)Session[Index_Sess_Obj + ActionName];
                BSM.HeaderCareer.Increase = Inc;
                if (BSM.Increase != "#####")
                    BSM.HeaderCareer.NewSalary = BSM.HeaderCareer.OldSalary + Inc;
                else
                    BSM.HeaderCareer.NewSalary = 0;
                var result = new
                {
                    MS = SYConstant.OK,
                    NewSalary = BSM.HeaderCareer.NewSalary
                };

                return Json(result, JsonRequestBehavior.DenyGet);

            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        public ActionResult ShowDataEmp(string ID, string EmpCode)
        {
            UserSession();
            DataSelector();
            ActionName = "Details";
            HR_STAFF_VIEW EmpStaff = DBV.HR_STAFF_VIEW.FirstOrDefault(w => w.EmpCode == EmpCode);
            var resualt = DB.HREmpCareers;
            List<HREmpCareer> listEmpCareer = resualt.Where(x => x.EmpCode == EmpCode).ToList();
            int tranNo = Convert.ToInt32(listEmpCareer.Max(w => w.TranNo));
            HRStaffProfileObject BSM = new HRStaffProfileObject();
            if (EmpStaff != null)
            {
                var EmpCareer = listEmpCareer.FirstOrDefault(w => w.TranNo == tranNo);
                if (EmpCareer != null)
                {
                    var salary = "";
                    var IsSalary = BSM.IsHideSalary(EmpCareer.LevelCode);
                    if (IsSalary == true)
                    {
                        salary = EmpCareer.NewSalary.ToString();
                    }
                    else
                    {
                        ViewData[ClsConstant.IS_SALARY] = true;
                        salary = "#####";
                    }
                    JobType(EmpCareer.Branch, "Branch");
                    var result = new
                    {
                        MS = SYConstant.OK,
                        AllName = EmpStaff.AllName,
                        EmpType = EmpStaff.EmployeeType,
                        Division = EmpStaff.Division,
                        DEPT = EmpStaff.Department,
                        SECT = EmpStaff.Section,
                        LevelCode = EmpStaff.Level,
                        Position = EmpStaff.Position,
                        StartDate = EmpStaff.StartDate,

                        CCompany = EmpCareer.CompanyCode,
                        CEmpType = EmpCareer.EmpType,
                        CBranch = EmpCareer.Branch,
                        CLocation = EmpCareer.LOCT,
                        CDivi = EmpCareer.Division,
                        CGDept = EmpCareer.GroupDept,
                        COffice = EmpCareer.Office,
                        CGroups = EmpCareer.Groups,
                        CDept = EmpCareer.DEPT,
                        CLine = EmpCareer.LINE,
                        CSect = EmpCareer.SECT,
                        CPost = EmpCareer.JobCode,
                        CJopGrade = EmpCareer.JobGrade,
                        CLevel = EmpCareer.LevelCode,
                        CEff = DateTime.Now,
                        StaffType = EmpCareer.StaffType,
                        CSalary = salary,
                        Cate = EmpCareer.CATE
                    };
                    GetData(EmpCode, "Create");
                    return Json(result, JsonRequestBehavior.DenyGet);
                }
                else
                {
                    var result = new
                    {
                        MS = SYConstant.OK,
                        AllName = EmpStaff.AllName,
                        EmpType = EmpStaff.EmployeeType,
                        Division = EmpStaff.Division,
                        DEPT = EmpStaff.Department,
                        SECT = EmpStaff.Section,
                        LevelCode = EmpStaff.Level,
                        Position = EmpStaff.Position,
                        StartDate = EmpStaff.StartDate
                    };
                    GetData(EmpCode, "Create");
                    return Json(result, JsonRequestBehavior.DenyGet);
                }
            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        public string GetData(string EmpCode, string Action)
        {
            this.ActionName = Action;
            UserSession();
            UserConfListAndForm();

            HRStaffProfileObject BSM = new HRStaffProfileObject();
            BSM.ListCareerHis = new List<HR_Career>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                var resualt = DB.HREmpCareers;
                List<HREmpCareer> listEmpCareer = resualt.Where(x => x.EmpCode == EmpCode).ToList();
                int tranNo = Convert.ToInt32(listEmpCareer.Max(w => w.TranNo));
                BSM.IsValidChecking(tranNo);
                var IsSalary = BSM.IsHideSalary(BSM.HeaderCareer.LevelCode);
                if (IsSalary == true)
                {
                    BSM.NewSalary = BSM.HeaderCareer.NewSalary.ToString();
                    BSM.OldSalary = BSM.HeaderCareer.OldSalary.ToString();
                    BSM.Increase = BSM.HeaderCareer.Increase.ToString();
                }
                else
                {
                    ViewData[ClsConstant.IS_SALARY] = true;
                    BSM.NewSalary = "#####";
                    BSM.OldSalary = "#####";
                    BSM.Increase = "#####";
                }
                foreach (var read in listEmpCareer.OrderByDescending(w => w.FromDate).ToList())
                {
                    var obj = new HR_Career();
                    obj.FromDate = read.FromDate.Value;
                    obj.ToDate = read.ToDate.Value;
                    obj.Department = read.DEPT;
                    obj.Position = read.JobCode;
                    obj.Career = read.CareerCode;
                    obj.Level = read.LevelCode;
                    if (IsSalary == true)
                    {
                        obj.NewSalary = read.NewSalary.ToString();
                        obj.OldSalary = read.OldSalary.ToString();
                        obj.Increase = read.Increase.ToString();
                    }
                    else
                    {
                        obj.NewSalary = "#####";
                        obj.OldSalary = "#####";
                        obj.Increase = "#####";
                    }
                    obj.CreatedBy = read.CreateBy;
                    obj.ChangedBy = read.ChangedBy;
                    BSM.ListCareerHis.Add(obj);
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
                return SYConstant.OK;
            }
            else
            {
                return SYMessages.getMessage("PLEASE_SEARCH_Employee");
            }
        }

        #region JobType

        [HttpPost]
        public ActionResult JobType(string code, string addType)
        {
            UserSession();
            UserConfListAndForm();
            if (!string.IsNullOrEmpty(code))
            {
                var _listCom = DB.HRCompanyGroups.Where(w => w.ParentWorkGroupID == addType).ToList();
                string Res = "";
                if (_listCom.Count() > 0)
                {
                    var obj = _listCom.FirstOrDefault();
                    Res = obj.WorkGroup;
                    if (obj.WorkGroup == "Division")
                        Session["Division"] = code;
                    else if (obj.WorkGroup == "GroupDepartment")
                        Session["GroupDepartment"] = code;
                    else if (obj.WorkGroup == "Department")
                        Session["Department"] = code;
                    else if (obj.WorkGroup == "Position")
                        Session["Position"] = code;
                    else if (obj.WorkGroup == "Section")
                        Session["Section"] = code;
                    else if (obj.WorkGroup == "Level")
                        Session["Level"] = code;
                    else if (obj.WorkGroup == "JobGrade")
                        Session["JobGrade"] = code;
                }
                var result = new
                {
                    MS = Res,
                };
                return Json(result, JsonRequestBehavior.DenyGet);
            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        public ActionResult GetDivision()
        {
            UserSession();
            return GridViewExtension.GetComboBoxCallbackResult(cboProperties =>
            {
                cboProperties.CallbackRouteValues = new { Controller = "CareerHistory", Action = "GetDivision" };
                cboProperties.Width = Unit.Percentage(100);
                cboProperties.ValueField = "Code";
                cboProperties.TextField = "Description";
                cboProperties.TextFormatString = "{0}:{1}";
                cboProperties.Columns.Add("Code", SYSettings.getLabel("Code"), 70);
                cboProperties.Columns.Add("Description", SYSettings.getLabel("Description"), 250);
                cboProperties.Columns.Add("SecDescription", SYSettings.getLabel("Second Description"), 250);
                cboProperties.BindList(Humica.Logic.HR.HRStaffProfileObject.GetDivision());
            });
        }
        public ActionResult GetGroupDepartment()
        {
            UserSession();
            return GridViewExtension.GetComboBoxCallbackResult(cboProperties =>
            {
                cboProperties.CallbackRouteValues = new { Controller = "CareerHistory", Action = "GetGroupDepartment" };
                cboProperties.Width = Unit.Percentage(100);
                cboProperties.ValueField = "Code";
                cboProperties.TextField = "Description";
                cboProperties.TextFormatString = "{1}";
                cboProperties.Columns.Add("Code", SYSettings.getLabel("Code"), 70);
                cboProperties.Columns.Add("Description", SYSettings.getLabel("Description"), 250);
                cboProperties.Columns.Add("SecDescription", SYSettings.getLabel("Second Description"), 250);
                cboProperties.BindList(Humica.Logic.HR.HRStaffProfileObject.GetGroupDepartment());
            });
        }
        public ActionResult GetDepartment()
        {
            UserSession();
            return GridViewExtension.GetComboBoxCallbackResult(cboProperties =>
            {
                cboProperties.CallbackRouteValues = new { Controller = "CareerHistory", Action = "GetDepartment" };
                cboProperties.Width = Unit.Percentage(100);
                cboProperties.ValueField = "Code";
                cboProperties.TextField = "Description";
                cboProperties.TextFormatString = "{1}";
                cboProperties.Columns.Add("Code", SYSettings.getLabel("Code"), 70);
                cboProperties.Columns.Add("Description", SYSettings.getLabel("Description"), 250);
                cboProperties.Columns.Add("SecDescription", SYSettings.getLabel("Second Description"), 250);
                cboProperties.BindList(Humica.Logic.HR.HRStaffProfileObject.GetDepartment());
            });
        }
        public ActionResult GetPosition()
        {
            UserSession();
            return GridViewExtension.GetComboBoxCallbackResult(cboProperties =>
            {
                cboProperties.CallbackRouteValues = new { Controller = "CareerHistory", Action = "GetPosition" };
                cboProperties.Width = Unit.Percentage(100);
                cboProperties.ValueField = "Code";
                cboProperties.TextField = "Description";
                cboProperties.TextFormatString = "{1}";
                cboProperties.Columns.Add("Code", SYSettings.getLabel("Code"), 70);
                cboProperties.Columns.Add("Description", SYSettings.getLabel("Description"), 250);
                cboProperties.Columns.Add("SecDescription", SYSettings.getLabel("Second Description"), 250);
                cboProperties.BindList(HRStaffProfileObject.GetPosition());
            });
        }
        public ActionResult GetSection()
        {
            UserSession();
            return GridViewExtension.GetComboBoxCallbackResult(cboProperties =>
            {
                cboProperties.CallbackRouteValues = new { Controller = "CareerHistory", Action = "GetSection" };
                cboProperties.Width = Unit.Percentage(100);
                cboProperties.ValueField = "Code";
                cboProperties.TextField = "Description";
                cboProperties.TextFormatString = "{1}";
                cboProperties.Columns.Add("Code", SYSettings.getLabel("Code"), 70);
                cboProperties.Columns.Add("Description", SYSettings.getLabel("Description"), 250);
                cboProperties.Columns.Add("SecDescription", SYSettings.getLabel("Second Description"), 250);
                cboProperties.BindList(HRStaffProfileObject.GetSection());
            });
        }
        public ActionResult GetLevel()
        {
            UserSession();
            return GridViewExtension.GetComboBoxCallbackResult(cboProperties =>
            {
                cboProperties.CallbackRouteValues = new { Controller = "CareerHistory", Action = "GetLevel" };
                cboProperties.Width = Unit.Percentage(100);
                cboProperties.ValueField = "Code";
                cboProperties.TextField = "Description";
                cboProperties.TextFormatString = "{1}";
                cboProperties.Columns.Add("Code", SYSettings.getLabel("Code"), 70);
                cboProperties.Columns.Add("Description", SYSettings.getLabel("Description"), 250);
                cboProperties.Columns.Add("SecDescription", SYSettings.getLabel("Second Description"), 250);
                cboProperties.BindList(HRStaffProfileObject.GetLevel());
            });
        }
        public ActionResult GetJobGrade()
        {
            UserSession();
            return GridViewExtension.GetComboBoxCallbackResult(cboProperties =>
            {
                cboProperties.CallbackRouteValues = new { Controller = "CareerHistory", Action = "GetJobGrade" };
                cboProperties.Width = Unit.Percentage(100);
                cboProperties.ValueField = "Code";
                cboProperties.TextField = "Description";
                cboProperties.TextFormatString = "{1}";
                cboProperties.Columns.Add("Code", SYSettings.getLabel("Code"), 70);
                cboProperties.Columns.Add("Description", SYSettings.getLabel("Description"), 250);
                cboProperties.Columns.Add("SecDescription", SYSettings.getLabel("Second Description"), 250);
                cboProperties.BindList(HRStaffProfileObject.GetJobGrade());
            });
        }
        #endregion
        private void DataSelector()
        {
            var ListBranch = SYConstant.getBranchDataAccess();
            ViewData["Company_SELECT"] = SYConstant.getCompanyDataAccess();
            ViewData["BRANCHES_SELECT"] = ListBranch;
            ViewData["EMPTYPE_SELECT"] = DB.HREmpTypes.ToList();
            ViewData["SECTION_SELECT"] = DB.HRSections.ToList();
            ViewData["LEVEL_SELECT"] = SYConstant.getLevelDataAccess();
            ViewData["JOBGRADE_SELECT"] = DB.HRJobGrades.ToList().OrderBy(w => w.Description);
            ViewData["POSITION_SELECT"] = ClsFilter.LoadPosition();
            ViewData["GROUPDEPARTMENT_SELECT"] = DB.HRGroupDepartments.ToList().OrderBy(w => w.Description);
            ViewData["DEPARTMENT_SELECT"] = ClsFilter.LoadDepartment();
            ViewData["CareerHistories_SELECT"] = DB.HRCareerHistories.ToList();
            ViewData["LOCATION_SELECT"] = DB.HRLocations.ToList().OrderBy(w => w.Description);
            ViewData["LINE_SELECT"] = DB.HRLines.ToList().OrderBy(w => w.Description);
            ViewData["WORKINGTYPE_SELECT"] = DB.HRWorkingTypes.ToList();
            ViewData["CATEGORY_SELECT"] = DB.HRCategories.ToList();
            ViewData["SEPARATE_SELECT"] = DB.HRTerminTypes.ToList();
            ViewData["FUNCTION_SELECT"] = DB.HRFunctions.ToList().OrderBy(w => w.Description);

            ViewData["OFFICE_SELECT"] = DB.HROffices.ToList();
            ViewData["GROUP_SELECT"] = DB.HRGroups.ToList();
        }
        private void DataList()
        {
            var objStatus = new SYDataList("STATUS_EMPLOYEE");
            ViewData["STATUS_EMPLOYEE"] = objStatus.ListData;
            var ListBranch = SYConstant.getBranchDataAccess();
            var ListStaff = DBV.HR_STAFF_VIEW.Where(w => w.StatusCode == "A").ToList();
            ListStaff = ListStaff.Where(w => ListBranch.Where(x => x.Code == w.BranchID).Any()).ToList();
            ViewData["STAFF_SELECT"] = ListStaff;
        }
        private void SetSessionNULL()
        {
            Session["Division"] = null;
            Session["GroupDepartment"] = null;
            Session["Department"] = null;
            Session["Position"] = null;
            Session["Section"] = null;
            Session["JobGrade"] = null;
            Session["Level"] = null;
            Session["ObjValue"] = null;
        }

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

        public void FitlertDataSession(string Branch, string Division, string GDept, string Dept,
           string Section, string Position, string Level, string Grade)
        {
            JobType(Branch, "Branch");
            JobType(Division, "Division");
            JobType(GDept, "GroupDepartment");
            JobType(Dept, "Department");
            JobType(Section, "Section");
            JobType(Position, "Position");
            JobType(Level, "Level");
            JobType(Grade, "JobGrade");
        }
    }
}
