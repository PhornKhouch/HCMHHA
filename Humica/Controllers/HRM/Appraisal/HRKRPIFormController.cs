using DevExpress.DataProcessing;
using DevExpress.Web;
using DevExpress.Web.Mvc;
using Humica.Core.DB;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic.HR;
using Humica.Models.SY;
using HUMICA.Models.Report.HRM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Humica.Controllers.HRM.Appraisal
{
    public class HRKRPIFormController : Humica.EF.Controllers.MasterSaleController
    {

        private const string SCREEN_ID = "HRF0000001";
        private const string URL_SCREEN = "/HRM/Appraisal/HRKRPIForm/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "LineItem";
        private string DOCTYPE = "KPIF01";
        HumicaDBContext DB = new HumicaDBContext();
        HumicaDBViewContext DBV = new HumicaDBViewContext();
        public HRKRPIFormController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
        }
        #region List
        public ActionResult Index()
        {
            ActionName = "Index";
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);
            HRKPISetupObject BSM = new HRKPISetupObject();
            BSM.Filter = new Humica.Core.FT.FTDGeneralAccount();
            BSM.Filter.FromDate = DateTime.Now.Date;
            BSM.Filter.ToDate = DateTime.Now.Date;
            BSM.Filter.Status = SYDocumentStatus.OPEN.ToString();
            Session["AssignCode"] = "";
            Session["KPICategory"] = "";
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];
                BSM.Filter = BSM.Filter;
            }
            BSM.ListHeaderForm = DB.HRKPIFormHeaders.Where(w => w.DocumentDate >= BSM.Filter.FromDate && w.DocumentDate <= BSM.Filter.ToDate).ToList();
            BSM.ListWorkProcess = DB.HRKPIActivitiesHeaders.Where(w => w.DocumentDate >= BSM.Filter.FromDate && w.DocumentDate <= BSM.Filter.ToDate && w.Status == BSM.Filter.Status).ToList();
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        [HttpPost]
        public ActionResult Index(HRKPISetupObject Collection)
        {
            ActionName = "Index";
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);
            HRKPISetupObject BSM = new HRKPISetupObject();
            BSM.Filter = new Humica.Core.FT.FTDGeneralAccount();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];
                BSM.Filter = Collection.Filter;
            }
            BSM.Filter.Status = SYDocumentStatus.OPEN.ToString();
            BSM.ListHeaderForm = DB.HRKPIFormHeaders.Where(w => w.DocumentDate >= BSM.Filter.FromDate && w.DocumentDate <= BSM.Filter.ToDate).ToList();
            BSM.ListWorkProcess = DB.HRKPIActivitiesHeaders.Where(w => w.DocumentDate >= BSM.Filter.FromDate && w.DocumentDate <= BSM.Filter.ToDate && w.Status == BSM.Filter.Status).ToList();
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        public ActionResult PartialList()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            HRKPISetupObject BSM = new HRKPISetupObject();
            //BSM.ListStaffView = new List<HR_STAFF_VIEW>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView(SYListConfuration.ListDefaultView, BSM.ListWorkProcess);
        }
        public ActionResult PartialListProcess()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            HRKPISetupObject BSM = new HRKPISetupObject();
            //BSM.ListStaffView = new List<HR_STAFF_VIEW>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("PartialListProcess", BSM.ListHeaderForm);
        }
        #endregion
        #region EmployeeConcern
        public ActionResult EmployeeConcern(string id, string id1, string FormType)
        {
            ActionName = "EmployeeConcern";
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);
            HRKPISetupObject BSM = new HRKPISetupObject();
            BSM.HeaderForm = new HRKPIFormHeader();
            BSM.ListItemForm = new List<HRKPIFormItem>();
            BSM.HeaderForm.FormType = ActionName;
            BSM.HeaderForm.DocumentDate = DateTime.Now.Date;
            Session["KPICodeItem"] = "";
            if (id != null)
            {
                BSM.HeaderActivities = DB.HRKPIActivitiesHeaders.FirstOrDefault(w => w.AVTCode == id && w.AssignCode == id1);
                if (BSM.HeaderActivities != null)
                {
                    BSM.AssignHeader = DB.HRKPIAssignHeaders.SingleOrDefault(w => w.AssignCode == BSM.HeaderActivities.AssignCode);
                    Session["AssignCode"] = BSM.AssignHeader.AssignCode;
                    Session["KPICategory"] = BSM.HeaderActivities.KPICategory;
                    BSM.HeaderForm.AVTCode = BSM.HeaderActivities.AVTCode;
                    BSM.HeaderForm.AVTCode = BSM.HeaderActivities.AVTCode;
                    BSM.HeaderForm.EmpName = BSM.AssignHeader.EmpName;
                    BSM.HeaderForm.Department = BSM.HeaderActivities.KPICategory;
                }
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        [HttpPost]
        public ActionResult EmployeeConcern(HRKPISetupObject collection)
        {
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);
            ActionName = "EmployeeConcern";
            if (ModelState.IsValid)
            {
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    var Obj = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];
                    Obj.DocType = DOCTYPE;
                    Obj.ScreenId = SCREEN_ID;
                    Obj.HeaderForm.EmpName = collection.HeaderForm.EmpName;
                    Obj.HeaderForm.Detail = collection.HeaderForm.Detail;
                    string msg = Obj.SaveForm(Obj.HeaderForm.FormType);
                    if (msg == SYConstant.OK)
                    {
                        SYMessages mess_err = SYMessages.getMessageObject("MS001", user.Lang);
                        mess_err.DocumentNumber = collection.HeaderForm.Code.ToString();
                        mess_err.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + mess_err.DocumentNumber + "&FormType=" + Obj.HeaderForm.FormType;
                        Obj.HeaderForm = new HRKPIFormHeader();
                        Obj.HeaderForm.FormType = ActionName;
                        Obj.ListItemForm = new List<HRKPIFormItem>();
                        Session[Index_Sess_Obj + this.ActionName] = Obj;
                        UserConfForm(ActionBehavior.SAVEGRID);
                        Session[SYConstant.MESSAGE_SUBMIT] = mess_err;
                        return View(Obj);
                    }
                    else
                    {
                        ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                        Session[Index_Sess_Obj + ActionName] = Obj;
                        return View(Obj);
                    }
                }
            }
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Create");

        }
        #region GridItemsEmployeeConcern
        public ActionResult GridEmpCC()
        {
            ActionName = "EmployeeConcern";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRKPISetupObject BSM = new HRKPISetupObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("GridItemsEmployeeConcern", BSM);
        }
        public ActionResult CreateEmpCC_(HRKPIFormItem ModelObject)
        {
            ActionName = "EmployeeConcern";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRKPISetupObject BSM = new HRKPISetupObject();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];
                    }
                    if (ModelObject.KPIName == null)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("KPI_ERR", user.Lang);
                    }
                    else
                    {
                        var Item = BSM.ListItemForm.ToList();
                        var check = BSM.ListItemForm.Where(w => w.KPICode == ModelObject.KPICode && w.KPIName == ModelObject.KPIName).ToList();
                        var line = Item.Count + 1;
                        if (check.Count == 0)
                        {
                            ModelObject.LineItem = line;
                            BSM.ListItemForm.Add(ModelObject);
                        }
                        else
                        {
                            ViewData["EditError"] = SYMessages.getMessage("ISDUPLICATED", user.Lang);
                        }
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
            //DataSelector();
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("GridItemsEmployeeConcern", BSM);
        }
        public ActionResult EditEmpCC(HRKPIFormItem obj)
        {
            ActionName = "EmployeeConcern";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRKPISetupObject BSM = new HRKPISetupObject();

            BSM.ListItemForm = new List<HRKPIFormItem>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];
                var objCheck = BSM.ListItemForm.Where(w => w.LineItem == obj.LineItem).ToList();
                if (obj.KPIName == null || obj.KPIName == null)
                {
                    ViewData["EditError"] = SYMessages.getMessage("KPI_ERR");

                }
                else if (objCheck.Count > 0)
                {

                    objCheck.First().KPIName = obj.KPIName;
                    objCheck.First().Actual = obj.Actual;
                    objCheck.First().Target = obj.Target;
                    objCheck.First().Difference = obj.Difference;
                    objCheck.First().Comment = obj.Comment;
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }


            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("GridItemsEmployeeConcern", BSM);
        }
        public ActionResult DeleteEmpCC(int LineItem)
        {
            ActionName = "EmployeeConcern";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRKPISetupObject BSM = new HRKPISetupObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListItemForm.Where(w => w.LineItem == LineItem).ToList();
                if (objCheck.Count > 0)
                {
                    BSM.ListItemForm.Remove(objCheck.First());
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return PartialView("GridItemsEmployeeConcern", BSM);
        }
        #endregion
        #region GridCreate
        public ActionResult JobGridItems()
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRKPISetupObject BSM = new HRKPISetupObject();
            BSM.ListItemForm = new List<HRKPIFormItem>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("GridItems", BSM);
        }
        public ActionResult JobCreate(HRKPIFormItem ModelObject)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            HRKPISetupObject BSM = new HRKPISetupObject();
            if (ModelState.IsValid != true)
            {
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];
                }
                var ListIteme = DB.HRKPIFormItems.Where(w => w.Code == ModelObject.Code && w.FormType == ModelObject.FormType).ToList();
                if (ListIteme.Count > 0)
                {
                    ViewData["Editor"] = SYMessages.getMessage("Erro", user.Lang);
                }
            }
            return PartialView("GridItems", BSM);
        }
        #endregion
        #endregion
        #region EmployeeAction
        public ActionResult EmployeeAction(string id, string id1, string FormType)
        {
            ActionName = "EmployeeAction";
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);
            HRKPISetupObject BSM = new HRKPISetupObject();
            BSM.HeaderForm = new HRKPIFormHeader();
            BSM.ListItemForm = new List<HRKPIFormItem>();
            BSM.HeaderForm.FormType = ActionName;
            BSM.HeaderForm.ExpectedDate = DateTime.Now.Date;
            Session["KPICodeItem"] = "";
            if (id != null)
            {
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];
                }
                BSM.HeaderActivities = DB.HRKPIActivitiesHeaders.FirstOrDefault(w => w.AVTCode == id && w.AssignCode == id1);
                if (BSM.HeaderActivities != null)
                {
                    BSM.AssignHeader = DB.HRKPIAssignHeaders.SingleOrDefault(w => w.AssignCode == BSM.HeaderActivities.AssignCode);
                    Session["AssignCode"] = BSM.AssignHeader.AssignCode;
                    Session["KPICategory"] = BSM.HeaderActivities.KPICategory;
                    BSM.HeaderForm.AVTCode = BSM.HeaderActivities.AVTCode;
                    BSM.HeaderForm.EmpName = BSM.AssignHeader.EmpName;
                    BSM.HeaderForm.Department = BSM.HeaderActivities.KPICategory;
                }
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        [HttpPost]
        public ActionResult EmployeeAction(HRKPISetupObject collection)
        {
            ActionName = "EmployeeAction";
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);
            if (ModelState.IsValid)
            {
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    var Obj = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];
                    Obj.DocType = DOCTYPE;
                    Obj.ScreenId = SCREEN_ID;
                    Obj.HeaderForm.EmpName = collection.HeaderForm.EmpName;
                    Obj.HeaderForm.Status = collection.HeaderForm.Status;
                    string msg = Obj.SaveForm(Obj.HeaderForm.FormType);
                    if (msg == SYConstant.OK)
                    {
                        SYMessages mess_err = SYMessages.getMessageObject("MS001", user.Lang);
                        mess_err.DocumentNumber = Obj.HeaderForm.Code.ToString();
                        mess_err.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + mess_err.DocumentNumber + "&FormType=" + Obj.HeaderForm.FormType;
                        Obj.HeaderForm = new HRKPIFormHeader();
                        Obj.HeaderForm.FormType = ActionName;
                        Obj.ListItemForm = new List<HRKPIFormItem>();
                        Session[Index_Sess_Obj + this.ActionName] = Obj;
                        UserConfForm(ActionBehavior.SAVEGRID);
                        Session[SYConstant.MESSAGE_SUBMIT] = mess_err;
                        return View(Obj);
                    }
                    else
                    {
                        ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                        Session[Index_Sess_Obj + ActionName] = Obj;
                        return View(Obj);
                    }
                }
            }
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Create");

        }
        #region GridItemsEmployeeAction
        public ActionResult GridEmpAc()
        {
            ActionName = "EmployeeAction";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRKPISetupObject BSM = new HRKPISetupObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("GridItemsEmployeeAction", BSM);
        }
        public ActionResult CreateEmpAc(HRKPIFormItem ModelObject)
        {
            ActionName = "EmployeeAction";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRKPISetupObject BSM = new HRKPISetupObject();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];
                    }
                    if (ModelObject.KPIName == null)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("KPI_ERR", user.Lang);
                    }
                    else
                    {
                        var Item = BSM.ListItemForm.ToList();
                        var check = BSM.ListItemForm.Where(w => w.KPICode == ModelObject.KPICode && w.KPIName == ModelObject.KPIName).ToList();
                        var line = Item.Count + 1;
                        if (check.Count == 0)
                        {
                            ModelObject.LineItem = line;
                            BSM.ListItemForm.Add(ModelObject);
                        }
                        else
                        {
                            ViewData["EditError"] = SYMessages.getMessage("ISDUPLICATED", user.Lang);
                        }
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
            //DataSelector();
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("GridItemsEmployeeAction", BSM);
        }
        public ActionResult EditEmpAc(HRKPIFormItem obj)
        {
            ActionName = "EmployeeAction";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRKPISetupObject BSM = new HRKPISetupObject();

            BSM.ListItemForm = new List<HRKPIFormItem>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];
                var objCheck = BSM.ListItemForm.Where(w => w.LineItem == obj.LineItem).ToList();
                if (obj.KPIName == null || obj.KPIName == null)
                {
                    ViewData["EditError"] = SYMessages.getMessage("KPI_ERR");

                }
                else if (objCheck.Count > 0)
                {

                    objCheck.First().KPIName = obj.KPIName;
                    objCheck.First().Actual = obj.Actual;
                    objCheck.First().Target = obj.Target;
                    objCheck.First().Difference = obj.Difference;
                    objCheck.First().Comment = obj.Comment;
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }


            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("GridItemsEmployeeAction", BSM);
        }
        public ActionResult DeleteEmpAc(int LineItem)
        {
            ActionName = "EmployeeAction";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRKPISetupObject BSM = new HRKPISetupObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListItemForm.Where(w => w.LineItem == LineItem).ToList();
                if (objCheck.Count > 0)
                {
                    BSM.ListItemForm.Remove(objCheck.First());
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return PartialView("GridItemsEmployeeAction", BSM);
        }
        #endregion
        #endregion
        #region ManagermentGuideLine
        public ActionResult ManagermentGuideLine(string id, string id1, string FormType)
        {
            ActionName = "ManagermentGuideLine";
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);
            HRKPISetupObject BSM = new HRKPISetupObject();
            BSM.HeaderForm = new HRKPIFormHeader();
            BSM.ListItemForm = new List<HRKPIFormItem>();
            BSM.HeaderForm.FormType = ActionName;
            BSM.HeaderForm.DocumentDate = DateTime.Now.Date;
            Session["KPICodeItem"] = "";
            if (id != null)
            {
                BSM.HeaderActivities = DB.HRKPIActivitiesHeaders.FirstOrDefault(w => w.AVTCode == id && w.AssignCode == id1);
                if (BSM.HeaderActivities != null)
                {
                    BSM.AssignHeader = DB.HRKPIAssignHeaders.SingleOrDefault(w => w.AssignCode == BSM.HeaderActivities.AssignCode);
                    Session["AssignCode"] = BSM.AssignHeader.AssignCode;
                    Session["KPICategory"] = BSM.HeaderActivities.KPICategory;
                    BSM.HeaderForm.AVTCode = BSM.HeaderActivities.AVTCode;
                    BSM.HeaderForm.EmpName = BSM.AssignHeader.EmpName;
                    BSM.HeaderForm.Department = BSM.HeaderActivities.KPICategory;
                }
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        [HttpPost]
        public ActionResult ManagermentGuideLine(HRKPISetupObject collection)
        {
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);
            ActionName = "ManagermentGuideLine";
            if (ModelState.IsValid)
            {
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    var Obj = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];
                    Obj.DocType = DOCTYPE;
                    Obj.ScreenId = SCREEN_ID;
                    Obj.HeaderForm.CoachName = collection.HeaderForm.CoachName;
                    Obj.HeaderForm.EmpName = collection.HeaderForm.CoachName;
                    string msg = Obj.SaveForm(Obj.HeaderForm.FormType);
                    if (msg == SYConstant.OK)
                    {
                        SYMessages mess_err = SYMessages.getMessageObject("MS001", user.Lang);
                        mess_err.DocumentNumber = Obj.HeaderForm.Code.ToString();
                        mess_err.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + mess_err.DocumentNumber + "&FormType=" + Obj.HeaderForm.FormType;
                        Obj.HeaderForm = new HRKPIFormHeader();
                        Obj.HeaderForm.FormType = ActionName;
                        Obj.ListItemForm = new List<HRKPIFormItem>();
                        Session[Index_Sess_Obj + this.ActionName] = Obj;
                        UserConfForm(ActionBehavior.SAVEGRID);
                        Session[SYConstant.MESSAGE_SUBMIT] = mess_err;
                        return View(Obj);
                    }
                    else
                    {
                        ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                        Session[Index_Sess_Obj + ActionName] = Obj;
                        return View(Obj);
                    }
                }
            }
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Create");
        }
        #region GridItemsEmployeeConcern
        public ActionResult GridMG()
        {
            ActionName = "ManagermentGuideLine";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRKPISetupObject BSM = new HRKPISetupObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("ManagermentGuideLineGridItem", BSM);
        }
        public ActionResult CreateMG(HRKPIFormItem ModelObject)
        {
            ActionName = "ManagermentGuideLine";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRKPISetupObject BSM = new HRKPISetupObject();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];
                    }
                    if (ModelObject.KPIName == null)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("KPI_ERR", user.Lang);
                    }
                    else
                    {
                        var Item = BSM.ListItemForm.ToList();
                        var check = BSM.ListItemForm.Where(w => w.KPICode == ModelObject.KPICode && w.KPIName == ModelObject.KPIName).ToList();
                        var line = Item.Count + 1;
                        if (check.Count == 0)
                        {
                            ModelObject.LineItem = line;
                            BSM.ListItemForm.Add(ModelObject);
                        }
                        else
                        {
                            ViewData["EditError"] = SYMessages.getMessage("ISDUPLICATED", user.Lang);
                        }
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
            //DataSelector();
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("ManagermentGuideLineGridItem", BSM);
        }
        public ActionResult EditEmpMG(HRKPIFormItem obj)
        {
            ActionName = "ManagermentGuideLine";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRKPISetupObject BSM = new HRKPISetupObject();

            BSM.ListItemForm = new List<HRKPIFormItem>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];
                var objCheck = BSM.ListItemForm.Where(w => w.LineItem == obj.LineItem).ToList();
                if (obj.KPIName == null || obj.KPIName == null)
                {
                    ViewData["EditError"] = SYMessages.getMessage("KPI_ERR");
                }
                else if (objCheck.Count > 0)
                {
                    objCheck.First().KPIName = obj.KPIName;
                    objCheck.First().Actual = obj.Actual;
                    objCheck.First().Target = obj.Target;
                    objCheck.First().Difference = obj.Difference;
                    objCheck.First().Comment = obj.Comment;
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }

            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("ManagermentGuideLineGridItem", BSM);
        }
        public ActionResult DeleteMG(int LineItem)
        {
            ActionName = "ManagermentGuideLine";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRKPISetupObject BSM = new HRKPISetupObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListItemForm.Where(w => w.LineItem == LineItem).ToList();
                if (objCheck.Count > 0)
                {
                    BSM.ListItemForm.Remove(objCheck.First());
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return PartialView("ManagermentGuideLineGridItem", BSM);
        }
        #endregion
        #endregion
        #region FollowUp
        public ActionResult FollowUp(string id, string id1, string FormType)
        {
            ActionName = "FollowUp";
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);
            HRKPISetupObject BSM = new HRKPISetupObject();
            BSM.HeaderForm = new HRKPIFormHeader();
            BSM.ListItemForm = new List<HRKPIFormItem>();
            BSM.HeaderForm.FormType = ActionName;
            BSM.HeaderForm.DocumentDate = DateTime.Now.Date;
            BSM.HeaderForm.ExpectedDate = DateTime.Now.Date;
            Session["KPICodeItem"] = "";
            if (id != null)
            {
                BSM.HeaderActivities = DB.HRKPIActivitiesHeaders.FirstOrDefault(w => w.AVTCode == id && w.AssignCode == id1);
                if (BSM.HeaderActivities != null)
                {
                    BSM.AssignHeader = DB.HRKPIAssignHeaders.SingleOrDefault(w => w.AssignCode == BSM.HeaderActivities.AssignCode);
                    Session["AssignCode"] = BSM.AssignHeader.AssignCode;
                    Session["KPICategory"] = BSM.HeaderActivities.KPICategory;
                    BSM.HeaderForm.AVTCode = BSM.HeaderActivities.AVTCode;
                    BSM.HeaderForm.EmpName = BSM.AssignHeader.EmpName;
                    BSM.HeaderForm.Department = BSM.HeaderActivities.KPICategory;
                }
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        [HttpPost]
        public ActionResult FollowUp(HRKPISetupObject collection)
        {
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);
            ActionName = "FollowUp";
            if (ModelState.IsValid)
            {
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    var Obj = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];
                    Obj.DocType = DOCTYPE;
                    Obj.ScreenId = SCREEN_ID;
                    Obj.HeaderForm.FormType = collection.HeaderForm.FormType;
                    Obj.HeaderForm.EmpName = collection.HeaderForm.EmpName;
                    string msg = Obj.SaveForm(Obj.HeaderForm.FormType);
                    if (msg == SYConstant.OK)
                    {
                        SYMessages mess_err = SYMessages.getMessageObject("MS001", user.Lang);
                        mess_err.DocumentNumber = Obj.HeaderForm.Code.ToString();
                        mess_err.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + mess_err.DocumentNumber + "&FormType=" + Obj.HeaderForm.FormType;
                        Obj.HeaderForm = new HRKPIFormHeader();
                        Obj.HeaderForm.FormType = ActionName;
                        Obj.ListItemForm = new List<HRKPIFormItem>();
                        Session[Index_Sess_Obj + this.ActionName] = Obj;
                        UserConfForm(ActionBehavior.SAVEGRID);
                        Session[SYConstant.MESSAGE_SUBMIT] = mess_err;
                        return View(Obj);
                    }
                    else
                    {
                        ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                        Session[Index_Sess_Obj + ActionName] = Obj;
                        return View(Obj);
                    }
                }
            }
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Create");

        }
        public ActionResult GridFU()
        {
            ActionName = "FollowUp";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRKPISetupObject BSM = new HRKPISetupObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("FollowUpGridItem", BSM);
        }
        public ActionResult CreateFU(HRKPIFormItem ModelObject)
        {
            ActionName = "FollowUp";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRKPISetupObject BSM = new HRKPISetupObject();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];
                    }
                    if (ModelObject.KPIName == null)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("KPI_ERR", user.Lang);
                    }
                    else
                    {
                        var Item = BSM.ListItemForm.ToList();
                        var check = BSM.ListItemForm.Where(w => w.KPICode == ModelObject.KPICode && w.KPIName == ModelObject.KPIName).ToList();
                        var line = Item.Count + 1;
                        if (check.Count == 0)
                        {
                            ModelObject.LineItem = line;
                            BSM.ListItemForm.Add(ModelObject);
                        }
                        else
                        {
                            ViewData["EditError"] = SYMessages.getMessage("ISDUPLICATED", user.Lang);
                        }
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
            //DataSelector();
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("FollowUpGridItem", BSM);
        }
        public ActionResult EditEmpFU(HRKPIFormItem obj)
        {
            ActionName = "FollowUp";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRKPISetupObject BSM = new HRKPISetupObject();

            BSM.ListItemForm = new List<HRKPIFormItem>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];
                var objCheck = BSM.ListItemForm.Where(w => w.LineItem == obj.LineItem).ToList();
                if (obj.KPIName == null || obj.KPIName == null)
                {
                    ViewData["EditError"] = SYMessages.getMessage("KPI_ERR");
                }
                else if (objCheck.Count > 0)
                {
                    objCheck.First().KPIName = obj.KPIName;
                    objCheck.First().Actual = obj.Actual;
                    objCheck.First().Target = obj.Target;
                    objCheck.First().Difference = obj.Difference;
                    objCheck.First().Comment = obj.Comment;
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }

            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("FollowUpGridItem", BSM);
        }
        public ActionResult DeleteFU(int LineItem)
        {
            ActionName = "FollowUp";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRKPISetupObject BSM = new HRKPISetupObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListItemForm.Where(w => w.LineItem == LineItem).ToList();
                if (objCheck.Count > 0)
                {
                    BSM.ListItemForm.Remove(objCheck.First());
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return PartialView("FollowUpGridItem", BSM);
        }
        #endregion
        #region Print
        public ActionResult Print(string id, string FormType)
        {
            UserSession();
            UserConf(ActionBehavior.VIEWONLY);
            ViewData[Humica.EF.SYSConstant.PARAM_ID] = id;
            ViewData[Humica.EF.SYSConstant.PARAM_ID1] = FormType;
            UserMVCReportView();
            return View("ReportView");
        }
        public ActionResult DocumentViewerPartial(string id, string FormType)
        {
            ActionName = "Print";
            UserSession();
            UserConf(ActionBehavior.VIEWONLY);
            ViewData[Humica.EF.SYSConstant.PARAM_ID] = id;
            //UserMVC();
            var SD = DB.HRKPIFormHeaders.Find(id);
            if (SD != null)
            {
                try
                {
                    if (SD.FormType == "EmployeeConcern")
                    {
                        EmployeeConcern reportModel = new EmployeeConcern();
                        reportModel.Parameters["Code"].Value = SD.Code;
                        reportModel.Parameters["Code"].Visible = false;
                        reportModel.Parameters["FormType"].Value = SD.FormType;
                        reportModel.Parameters["FormType"].Visible = false;

                        Session[Index_Sess_Obj + ActionName] = reportModel;
                        return PartialView("PrintForm", reportModel);
                    }
                    if (SD.FormType == "EmployeeAction")
                    {
                        EmployeeAction reportModel = new EmployeeAction();
                        reportModel.Parameters["Code"].Value = SD.Code;
                        reportModel.Parameters["Code"].Visible = false;
                        reportModel.Parameters["FormType"].Value = SD.FormType;
                        reportModel.Parameters["FormType"].Visible = false;
                        Session[Index_Sess_Obj + ActionName] = reportModel;
                        return PartialView("PrintForm", reportModel);
                    }
                    if (SD.FormType == "FollowUp")
                    {
                        FollowUpForm reportModel = new FollowUpForm();
                        reportModel.Parameters["Code"].Value = SD.Code;
                        reportModel.Parameters["Code"].Visible = false;
                        reportModel.Parameters["FormType"].Value = SD.FormType;
                        reportModel.Parameters["FormType"].Visible = false;

                        Session[Index_Sess_Obj + ActionName] = reportModel;
                        return PartialView("PrintForm", reportModel);
                    }
                    if (SD.FormType == "ManagermentGuideLine")
                    {
                        ManagementGuideLineForm reportModel = new ManagementGuideLineForm();
                        reportModel.Parameters["Code"].Value = SD.Code;
                        reportModel.Parameters["Code"].Visible = false;
                        reportModel.Parameters["FormType"].Value = SD.FormType;
                        reportModel.Parameters["FormType"].Visible = false;

                        Session[Index_Sess_Obj + ActionName] = reportModel;
                        return PartialView("PrintForm", reportModel);
                    }
                }
                catch (Exception e)
                {
                    /*------------------SaveLog----------------------------------*/
                    SYEventLog log = new SYEventLog();
                    log.ScreenId = SCREEN_ID;
                    log.UserId = user.UserID.ToString();
                    log.DocurmentAction = id;
                    log.Action = SYActionBehavior.ADD.ToString();

                    SYEventLogObject.saveEventLog(log, e, true);
                    /*----------------------------------------------------------*/
                }
            }
            return null;
        }
        public ActionResult DocumentViewerExportTo(string id)
        {
            ActionName = "Print";
            ViewData[Humica.EF.SYSConstant.PARAM_ID] = id;
            var SD = DB.HRKPIFormHeaders.Find(id);
            if (SD.FormType == "EmployeeConcern")
            {
                EmployeeConcern reportModel = (EmployeeConcern)Session[Index_Sess_Obj + ActionName];
                return ReportViewerExtension.ExportTo(reportModel);
            }
            if (SD.FormType == "EmployeeAction")
            {
                EmployeeAction reportModel = (EmployeeAction)Session[Index_Sess_Obj + ActionName];
                return ReportViewerExtension.ExportTo(reportModel);
            }
            if (SD.FormType == "FollowUp")
            {
                FollowUpForm reportModel = (FollowUpForm)Session[Index_Sess_Obj + ActionName];
                return ReportViewerExtension.ExportTo(reportModel);
            }
            if (SD.FormType == "ManagermentGuideLine")
            {
                ManagementGuideLineForm reportModel = (ManagementGuideLineForm)Session[Index_Sess_Obj + ActionName];
                return ReportViewerExtension.ExportTo(reportModel);
            }

            return null;
        }
        #endregion
        #region Edit
        public ActionResult Edit(string Code, string FormType)
        {
            ActionName = "Edit";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            HRKPISetupObject BSM = new HRKPISetupObject();
            BSM.HeaderForm = new HRKPIFormHeader();
            BSM.ListItemForm = new List<HRKPIFormItem>();
            BSM.FormType = FormType;
            Session["KPICodeItem"] = "";
            BSM.HeaderForm = DB.HRKPIFormHeaders.FirstOrDefault(w => w.Code == Code && w.FormType == FormType);
            if (BSM.HeaderForm != null)
            {
                var AVTHeader = DB.HRKPIActivitiesHeaders.FirstOrDefault(w => w.AVTCode == BSM.HeaderForm.AVTCode);
                Session["AssignCode"] = AVTHeader.AssignCode;
                Session["KPICategory"] = AVTHeader.KPICategory;
                BSM.ListItemForm = DB.HRKPIFormItems.Where(w => w.Code == BSM.HeaderForm.Code && w.FormType == BSM.HeaderForm.FormType).ToList();
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        [HttpPost]
        public ActionResult Edit(string Code, string FormType, HRKPISetupObject collection)
        {
            ActionName = "Edit";
            UserSession();
            this.ScreendIDControl = SCREEN_ID;
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = Code;
            HRKPISetupObject BSM = new HRKPISetupObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRKPISetupObject)Session[Index_Sess_Obj + ActionName];
            }
            //BSM.Header = collection.Header;
            collection.ListScore = BSM.ListScore;
            BSM.ScreenId = SCREEN_ID;
            string msg = collection.EditForm(Code, FormType);
            if (msg == SYConstant.OK)
            {
                DB = new HumicaDBContext();
                SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                mess.DocumentNumber = Code;
                mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?Code=" + mess.DocumentNumber + "&FormType=" + FormType;
                ViewData[SYConstant.MESSAGE_SUBMIT] = mess;
                return View(collection);
            }
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
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
                //int TranNo = Convert.ToInt32(id);
                HRProAppraiselObject Del = new HRProAppraiselObject();
                string msg = Del.DeleteAppr(id);
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
                Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            }
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

        }
        #endregion
        #region Details
        public ActionResult Details(string id, string FormType)
        {
            ActionName = "Details";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = id;
            ViewData[SYSConstant.PARAM_ID1] = FormType;
            if (id != null)
            {
                HRKPISetupObject BSM = new HRKPISetupObject();
                BSM.DocType = FormType;
                BSM.HeaderForm = DB.HRKPIFormHeaders.FirstOrDefault(w => w.Code == id && w.FormType == FormType);
                if (BSM.HeaderForm != null)
                {
                    BSM.ListItemForm = DB.HRKPIFormItems.Where(w => w.Code == BSM.HeaderForm.Code && w.FormType == BSM.HeaderForm.FormType).ToList();
                    Session[Index_Sess_Obj + ActionName] = BSM;
                    return View(BSM);
                }
            }
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        #endregion Details
        public ActionResult ShowDataEmp(string ID, string EmpCode, string Action)
        {

            ActionName = Action;
            HRProAppraiselObject BSM = new HRProAppraiselObject();
            var EmpStaff = DBV.HR_STAFF_VIEW.FirstOrDefault(w => w.EmpCode == EmpCode);
            BSM.Header = new HREmpAppraisal();
            BSM.ListFactor = DB.HRApprFactors.ToList();
            BSM.ListScore = new List<HREmpAppraisalItem>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRProAppraiselObject)Session[Index_Sess_Obj + ActionName];
            }
            //var list = DB.HREmpAppraisalItems.Where(w => w.EmpID == EmpCode ).ToList();
            //var Info = DB.HREmpAppraisals.FirstOrDefault(w => w.EmpID == EmpCode );
            BSM.ListScore = new List<HREmpAppraisalItem>();


            if (EmpStaff != null)
            {
                var result = new
                {
                    MS = SYConstant.OK,
                    POS = EmpStaff.Position,
                    NAME = EmpStaff.AllName,
                    Division = EmpStaff.Division,
                    DATE = EmpStaff.StartDate,
                    //EVADATE=Info.EvaluteDate,
                    //MANAGER=Info.Manager,
                    //ATYPE = Info.AppraiselType,
                    //RES=Info.Result

                };

                return Json(result, JsonRequestBehavior.DenyGet);

            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        public ActionResult GridItems()
        {
            ActionName = "Import";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRProAppraiselObject objScore = new HRProAppraiselObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                objScore = (HRProAppraiselObject)Session[Index_Sess_Obj + ActionName];
                if (objScore.ListScore != null)
                {
                    objScore.ListScore.Clear();
                }
            }
            if (objScore.ListTemplate.Count > 0)
            {
                SYExcel excel = new SYExcel();
                foreach (var read in objScore.ListTemplate.ToList())
                {
                    excel.FileName = read.UpoadPath;
                }
                DataTable dtHeader = excel.GenerateExcelData();
                objScore.ListScore = new List<HREmpAppraisalItem>();
                //objStaff.ListItem = new List<HRStaffProfile>();
                //objStaff.ListPlanItem = new List<HLContractPlanItem>();

                if (dtHeader != null)
                {
                    for (int i = 6; i < dtHeader.Rows.Count; i++)
                    {
                        var objHeader = new HREmpAppraisalItem();
                        //objHeader.EmpID = dtHeader.Rows[i][0].ToString();
                        objHeader.Code = dtHeader.Rows[i][0].ToString();
                        objHeader.Description = dtHeader.Rows[i][1].ToString();
                        objHeader.SecDescription = dtHeader.Rows[i][2].ToString();
                        objHeader.Score = (int?)SYSettings.getNumberValue(dtHeader.Rows[i][3].ToString());
                        objHeader.Score = (int?)SYSettings.getNumberValue(dtHeader.Rows[i][4].ToString());

                        objScore.ListScore.Add(objHeader);
                    }
                }
            }
            //BSM.ListDepreMethod = DB.ExFADepreciationMethods.ToList();
            Session[Index_Sess_Obj + ActionName] = objScore;
            return PartialView("GridItems", objScore);
        }
        #region "Import"
        public ActionResult Import()
        {
            UserSession();
            ActionName = "Import";
            UserConfListAndForm(ActionBehavior.IMPORT, "UploadName", "HRProAppraisel", SYSConstant.DEFAULT_UPLOAD_LIST);

            var objStaff = new HRProAppraiselObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                objStaff = (HRProAppraiselObject)Session[Index_Sess_Obj + ActionName];
                if (objStaff.ListScore != null)
                {
                    objStaff.ListScore.Clear();
                }
                if (objStaff.ListHeader != null)
                {
                    objStaff.ListHeader.Clear();
                }
            }

            objStaff.ListTemplate = DB.MDUploadTemplates.Where(w => w.ScreenId == SCREEN_ID && w.UploadBy == user.UserName).OrderByDescending(w => w.UploadDate).ToList();

            if (objStaff.ListTemplate.Count > 0)
            {
                SYExcel excel = new SYExcel();
                foreach (var read in objStaff.ListTemplate.ToList())
                {
                    excel.FileName = read.UpoadPath;
                }


                DataTable dtHeader = excel.GenerateExcelData();
                objStaff.ListScore = new List<HREmpAppraisalItem>();
                objStaff.ListHeader = new List<HREmpAppraisal>();
                if (dtHeader != null)
                {
                    for (int i = 0; i < dtHeader.Rows.Count; i++)
                    {
                        var objHeader = new HREmpAppraisalItem();
                        objHeader.Code = dtHeader.Rows[i][1].ToString();
                        objHeader.Description = dtHeader.Rows[i][2].ToString();
                        objHeader.SecDescription = dtHeader.Rows[i][3].ToString();
                        objHeader.Score = (int?)SYSettings.getNumberValue(dtHeader.Rows[i][4].ToString());
                        objHeader.Score = (int?)SYSettings.getNumberValue(dtHeader.Rows[i][5].ToString());

                        objStaff.ListScore.Add(objHeader);

                    }
                }


            }


            Session[Index_Sess_Obj + ActionName] = objStaff;
            return View(objStaff);
        }

        [HttpPost]
        public ActionResult UploadControlCallbackAction(HttpPostedFileBase file_Uploader)
        {

            UserSession();
            ActionName = "Import";
            this.ScreendIDControl = SYSConstant.DEFAULT_UPLOAD_LIST;
            UserConfListAndForm(ActionBehavior.IMPORT, "UploadName", "HRProAppraisel", SYSConstant.DEFAULT_UPLOAD_LIST);
            SYFileImport sfi = new SYFileImport(DBV.CFUploadPaths.Find("EMPLOYEE"));
            sfi.ObjectTemplate = new MDUploadTemplate();
            sfi.ObjectTemplate.UploadDate = DateTime.Now;
            sfi.ObjectTemplate.ScreenId = SCREEN_ID;
            sfi.ObjectTemplate.UploadBy = user.UserName;
            sfi.ObjectTemplate.Module = "HR";
            sfi.ObjectTemplate.IsGenerate = false;

            UploadedFile[] files = UploadControlExtension.GetUploadedFiles("FileUploadOPB",
                sfi.ValidationSettings,
                sfi.uc_FileUploadComplete);


            var objStaff = new HRProAppraiselObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                objStaff = (HRProAppraiselObject)Session[Index_Sess_Obj + ActionName];
            }


            objStaff.ListTemplate = DB.MDUploadTemplates.Where(w => w.ScreenId == SCREEN_ID && w.UploadBy == user.UserName).OrderByDescending(w => w.UploadDate).ToList();
            objStaff.ListScore = new List<HREmpAppraisalItem>();
            objStaff.ListHeader = new List<HREmpAppraisal>();


            Session[Index_Sess_Obj + ActionName] = objStaff;
            return Redirect(SYUrl.getBaseUrl() + ScreenUrl + "Import");
            //return null;
        }
        public ActionResult UploadList()
        {
            UserSession();
            ActionName = "Import";
            this.ScreendIDControl = SYSConstant.DEFAULT_UPLOAD_LIST;
            UserConfListAndForm(ActionBehavior.IMPORT, "UploadName", "HRProAppraisel", SYSConstant.DEFAULT_UPLOAD_LIST);

            var objStaff = new HRProAppraiselObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                objStaff = (HRProAppraiselObject)Session[Index_Sess_Obj + ActionName];
            }


            objStaff.ListTemplate = DB.MDUploadTemplates.Where(w => w.ScreenId == SCREEN_ID && w.UploadBy == user.UserName).OrderByDescending(w => w.UploadDate).ToList();
            objStaff.ListScore = new List<HREmpAppraisalItem>();
            objStaff.ListHeader = new List<HREmpAppraisal>();


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
                var objStaff = new HRProAppraiselObject();
                objStaff.Header = new HREmpAppraisal();
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
                            objStaff.ListHeader = new List<HREmpAppraisal>();

                            var objHeader = new HREmpAppraisal();
                            objHeader.EmpID = dtHeader.Rows[0][1].ToString();
                            //objHeader.Division = dtHeader.Rows[i][2].ToString();
                            objHeader.Positon = dtHeader.Rows[2][1].ToString();
                            //objHeader.DateOfHire = SYSettings.getDateValue(dtHeader.Rows[i][4].ToString());
                            objHeader.AppraisalType = dtHeader.Rows[1][1].ToString();
                            objHeader.AppraiserDate = SYSettings.getDateValue(dtHeader.Rows[3][1].ToString());

                            objStaff.Header.EmpID = objHeader.EmpID;
                            objStaff.ListHeader.Add(objHeader);

                        }

                        DateTime create = DateTime.Now;
                        if (dtHeader.Rows.Count > 0)  //Item
                        {
                            //objStaff.ListHeader = new List<HREmpAppraisal>();
                            objStaff.ListScore = new List<HREmpAppraisalItem>();
                            //objStaff.ListPlanItem = new List<HLContractPlanItem>();
                            for (int i = 6; i < dtHeader.Rows.Count; i++)
                            {
                                var objHeader = new HREmpAppraisalItem();
                                objHeader.Code = dtHeader.Rows[i][0].ToString();
                                objHeader.Description = dtHeader.Rows[i][1].ToString();
                                objHeader.SecDescription = dtHeader.Rows[i][2].ToString();
                                objHeader.Score = (int?)SYSettings.getNumberValue(dtHeader.Rows[i][3].ToString());
                                objHeader.Score = (int?)SYSettings.getNumberValue(dtHeader.Rows[i][4].ToString());

                                objStaff.ListScore.Add(objHeader);

                            }

                            msg = objStaff.uploadStaff();
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
            string fileName = Server.MapPath("~/Content/TEMPLATE/Appraisel_List.xlsx");
            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;filename=EMPLOYEE_TEMPLATE.xlsx");
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.WriteFile(fileName);
            Response.End();
            return null;
        }

        #endregion
        public ActionResult IsCalculate(string ApprID)
        {
            ActionName = "Details";
            UserSession();
            HRProAppraiselObject BSM = new HRProAppraiselObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRProAppraiselObject)Session[Index_Sess_Obj + ActionName];
            }

            var S = DB.HREmpAppraisalItems.Where(w => w.ApprID == ApprID);
            var sum = S.Sum(x => x.Score);
            var Res = DB.HRApprEvaluates.OrderByDescending(s => s.ToRate).ToList();
            foreach (var R in Res.ToList())
            {
                if (R.FromRate <= sum && R.ToRate >= sum)
                {
                    BSM.Header.Result = R.Result;
                }


            }
            var objMatch = DB.HREmpAppraisals.FirstOrDefault(x => x.ApprID == ApprID);
            objMatch.Result = BSM.Header.Result;
            DB.HREmpAppraisals.Attach(objMatch);
            DB.Entry(objMatch).Property(x => x.Result).IsModified = true;
            DB.SaveChanges();
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + ApprID);

        }
        #region "Grid Score"

        public ActionResult GridItemScore()
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();

            HRProAppraiselObject BSM = new HRProAppraiselObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRProAppraiselObject)Session[Index_Sess_Obj + ActionName];
            }

            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("GridItemScore", BSM.ListScore);
        }

        public ActionResult EditScore(HREmpAppraisalItem ModelObject)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();

            HRProAppraiselObject BSM = new HRProAppraiselObject();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (HRProAppraiselObject)Session[Index_Sess_Obj + ActionName];
                    }

                    var objCheck = BSM.ListScore.Where(w => w.Code == ModelObject.Code).ToList();
                    if (objCheck.Count > 0)
                    {
                        if (ModelObject.Score > objCheck.First().Score)
                        {
                            ViewData["EditError"] = SYMessages.getMessage("SCORE_VAL");
                        }
                        else
                        {
                            objCheck.First().Score = ModelObject.Score;
                            objCheck.First().SecDescription = ModelObject.SecDescription;
                            objCheck.First().Description = ModelObject.Description;
                            objCheck.First().Region = ModelObject.Region;
                            objCheck.First().Remark = ModelObject.Remark;
                        }

                    }
                    else
                    {
                        ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                    }

                    Session[Index_Sess_Obj + ActionName] = BSM;
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

            return PartialView("GridItemScore", BSM.ListScore);
        }

        #endregion "Grid Score"
        #region "Grid Score Edit"

        public ActionResult GridItemScoreEdit()
        {
            ActionName = "Edit";
            UserSession();
            UserConfListAndForm();
            DataSelector();

            HRProAppraiselObject BSM = new HRProAppraiselObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRProAppraiselObject)Session[Index_Sess_Obj + ActionName];
            }

            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("GridItemScoreEdit", BSM.ListScore);
        }

        public ActionResult EditScoreEdit(HREmpAppraisalItem ModelObject)
        {
            ActionName = "Edit";
            UserSession();
            DataSelector();
            UserConfListAndForm();

            HRProAppraiselObject BSM = new HRProAppraiselObject();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (HRProAppraiselObject)Session[Index_Sess_Obj + ActionName];
                    }

                    var objCheck = BSM.ListScore.Where(w => w.Code == ModelObject.Code).ToList();
                    if (objCheck.Count > 0)
                    {
                        if (ModelObject.Score > objCheck.First().Score)
                        {
                            ViewData["EditError"] = SYMessages.getMessage("SCORE_VAL");
                        }
                        else
                        {
                            objCheck.First().Score = ModelObject.Score;
                            objCheck.First().SecDescription = ModelObject.SecDescription;
                            objCheck.First().Description = ModelObject.Description;
                            objCheck.First().Region = ModelObject.Region;
                            objCheck.First().Remark = ModelObject.Remark;
                        }

                    }
                    else
                    {
                        ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                    }

                    Session[Index_Sess_Obj + ActionName] = BSM;
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

            return PartialView("GridItemScoreEdit", BSM.ListScore);
        }
        public ActionResult GridItemScoreDetails()
        {
            ActionName = "Details";
            UserSession();
            UserConfListAndForm();
            DataSelector();

            HRProAppraiselObject BSM = new HRProAppraiselObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRProAppraiselObject)Session[Index_Sess_Obj + ActionName];
            }

            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("GridItemScoreDetails", BSM.ListScore);
        }
        #endregion "Grid Score"
        #region SelectKPIElement
        public ActionResult SelectKPIElement(string Actionname, string Code)
        {
            ActionName = Actionname;
            Session["KPICodeItem"] = Code;
            UserSession();
            //HRKPISetupObject BSM = new HRKPISetupObject();
            var listBranch = SYConstant.getBranchDataAccess();
            ViewData[SYSConstant.PARAM_ID] = Code;
            //if (Session[Index_Sess_Obj + this.ActionName] != null)
            //{
            //    BSM = (HRKPISetupObject)Session[Index_Sess_Obj + this.ActionName];
            //}
            var data = new
            {
                MS = SYSConstant.OK
            };
            return Json(data, (JsonRequestBehavior)1);

        }

        #endregion
        [HttpPost]
        public ActionResult CheckValue(string Value, string Action)
        {
            this.ActionName = Action;
            UserSession();
            UserConfListAndForm();

            HRProAppraiselObject BSM = new HRProAppraiselObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRProAppraiselObject)Session[Index_Sess_Obj + ActionName];

                string[] sp = Value.Split('_');
                string Code = sp[0].ToString();
                int EvID = Convert.ToInt32(sp[1]);
                int Score = Convert.ToInt32(sp[2]);

                var checkexist = BSM.ListScore.Where(w => w.Code == Code).ToList();
                if (checkexist.Count == 0)
                {
                    var obj = new HREmpAppraisalItem();
                    obj.Code = Code;
                    obj.RatingID = EvID;
                    obj.Score = Score;
                    BSM.ListScore.Add(obj);
                }
                else
                {
                    checkexist.First().Code = Code;
                    checkexist.First().RatingID = EvID;
                    checkexist.First().Score = Score;
                }
                var result = new
                {
                    MS = SYConstant.OK,
                };
                Session[Index_Sess_Obj + ActionName] = BSM;
                return Json(result, JsonRequestBehavior.DenyGet);
            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        [HttpPost]
        public ActionResult CommentValue(string Code, string Value, string Action)
        {
            this.ActionName = Action;
            UserSession();
            UserConfListAndForm();

            HRProAppraiselObject BSM = new HRProAppraiselObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRProAppraiselObject)Session[Index_Sess_Obj + ActionName];

                var checkexist = BSM.ListScore.Where(w => w.Code == Code).ToList();
                if (checkexist.Count == 0)
                {
                    var obj = new HREmpAppraisalItem();
                    obj.Code = Code;
                    obj.Remark = Value;
                    BSM.ListScore.Add(obj);
                }
                else
                {
                    checkexist.First().Code = Code;
                    checkexist.First().Remark = Value;
                }
                var result = new
                {
                    MS = SYConstant.OK,
                };
                Session[Index_Sess_Obj + ActionName] = BSM;
                return Json(result, JsonRequestBehavior.DenyGet);
            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }

        public ActionResult KPISetupItem()
        {
            UserSession();
            return GridViewExtension.GetComboBoxCallbackResult(cboProperties =>
            {
                cboProperties.CallbackRouteValues = new { Controller = "HRKPIForm", Action = "HRKRPIForm" };
                cboProperties.ValueField = "KPIItemCode";
                cboProperties.TextField = "DescriptionEN";
                cboProperties.TextFormatString = "{0}:{1}";
                cboProperties.Columns.Add("KPIItemCode", Humica.EF.Models.SY.SYSettings.getLabel("Code"));
                cboProperties.Columns.Add("DescriptionEN", Humica.EF.Models.SY.SYSettings.getLabel("Description"));
                cboProperties.BindList(Humica.Logic.HR.HRKPISetupObject.GetAllKPIElement());
            });
        }
        private void DataSelector()
        {
            SYDataList DL = new SYDataList("FORM_TYPE_LIST");
            var _listBranch = SYConstant.getBranchDataAccess();
            var ListEmp = DBV.HR_STAFF_VIEW.ToList();
            ViewData["LIST_STAFF"] = ListEmp.Where(w => _listBranch.Where(x => x.Code == w.BranchID).Any()).ToList();
            //ViewData["APPRTYPE_SELECT"] = DB.HRApprTypes.ToList();
            ViewData["FORM_TYPE_LIST"] = DL.ListData;
            DL = new SYDataList("KPI_STATUS");
            ViewData["KPI_STATUS"] = DL.ListData;
        }
    }
}