using DevExpress.Web.Mvc;
using Humica.Core.DB;
using Humica.Core.SY;
using Humica.EF;
using Humica.EF.Models.SY;
using Humica.EF.Repo;
using Humica.Logic.SY;
using Humica.Models.SY;
using Humica.Performance;
using HUMICA.Models.Report.KPI;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.SelfService.Performance
{
    public class ESSKPIAssignController : Humica.EF.Controllers.MasterSaleController
    {
        private const string SCREEN_ID = "ESSA000002";
        private const string URL_SCREEN = "/SelfService/Performance/ESSKPIAssign/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "AssignCode;Status";
        protected IClsKPIAssign BSM;
        private IUnitOfWork unitOfWork;
        public ESSKPIAssignController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
            BSM = new CLsKPIAssign();
            BSM.OnLoad();
            unitOfWork = new UnitOfWork<HumicaDBContext>();
        }
        #region List
        public ActionResult Index()
        {
            UserSession();
            ActionName = "Index";
            UserConfListAndForm(this.KeyName);
            //BSM.Filter = new Humica.Core.FT.FTDGeneralAccount();
            //BSM.Filter.FromDate = DateTime.Now.Date;
            //BSM.Filter.ToDate = DateTime.Now.Date;
            //if (Session[Index_Sess_Obj + ActionName] != null)
            //{
            //    var obj = (CLsKPIAssign)Session[Index_Sess_Obj + ActionName];
            //}
            BSM.OnIndexPending(true);
            BSM.OnIndexLoading(SYDocumentASSIGN.Individual.ToString(), true);
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        [HttpPost]
        public ActionResult Index(IClsKPIAssign BSM)
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            BSM.OnIndexPending(true);
            BSM.OnIndexLoading(SYDocumentASSIGN.Individual.ToString(), true);
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }

        public ActionResult PartialListProcess()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (IClsKPIAssign)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("PartialListProcess", BSM.listAssignHeader);
        }
        public ActionResult PartialListPending()
        {
            ActionName = "Index";
            UserSession();
            UserConfList(KeyName);
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (IClsKPIAssign)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("PartialListPending", BSM.ListPending);
        }
        #endregion

        #region Create
        public ActionResult Create(string id)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm(this.KeyName);
            DataSelector();
            BSM.OnCreatingLoading(id);
            Session[Index_Sess_Obj + ActionName] = BSM;
            EmployeeKPI(BSM.AssignHeader.CriteriaType, BSM.AssignHeader.PeriodTo.Value.Year);
            return View(BSM);
        }
        [HttpPost]
        public ActionResult Create(string id, CLsKPIAssign collection)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm(this.KeyName);
            string CriteriaType = "";
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (IClsKPIAssign)Session[Index_Sess_Obj + ActionName];
                CriteriaType = BSM.AssignHeader.CriteriaType;
            }
            BSM.ScreenId = SCREEN_ID;
            BSM.AssignHeader = collection.AssignHeader;
            BSM.AssignHeader.CriteriaType = CriteriaType;
            BSM.AssignHeader.AssignedBy = SYDocumentASSIGN.Individual.ToString();
            string msg = BSM.Create();
            if (msg == SYConstant.OK)
            {
                SYMessages mess_err = SYMessages.getMessageObject("MS001", user.Lang);
                mess_err.DocumentNumber = BSM.AssignHeader.AssignCode.ToString();
                mess_err.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + mess_err.DocumentNumber;
                Session[Index_Sess_Obj + ActionName] = BSM;
                Session[SYConstant.MESSAGE_SUBMIT] = mess_err;
            }
            else
            {
                ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                Session[Index_Sess_Obj + ActionName] = BSM;
                return View(BSM);
            }
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        #endregion

        #region Edit
        public ActionResult Edit(string id)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = id;
            if (id != null)
            {
                BSM.OnEditLoading(id);
                if (BSM.AssignHeader != null)
                {
                    EmployeeKPI(BSM.AssignHeader.CriteriaType, BSM.AssignHeader.PeriodTo.Value.Year);
                    Session[Index_Sess_Obj + ActionName] = BSM;
                    return View(BSM);
                }
            }
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_ED", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }

        [HttpPost]
        public ActionResult Edit(string id, CLsKPIAssign collection)
        {
            ActionName = "Create";
            UserSession();
            this.ScreendIDControl = SCREEN_ID;
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = id;
            if (id != null)
            {
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (IClsKPIAssign)Session[Index_Sess_Obj + ActionName];
                }
                BSM.AssignHeader = collection.AssignHeader;
                BSM.ScreenId = SCREEN_ID;
                BSM.AssignHeader.AssignedBy = SYDocumentASSIGN.Individual.ToString();
                string msg = BSM.Update(id);
                if (msg == SYConstant.OK)
                {
                    SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                    mess.DocumentNumber = id;
                    mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + mess.DocumentNumber;
                    ViewData[SYConstant.MESSAGE_SUBMIT] = mess;
                    return View(BSM);
                }
                else
                {
                    ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                    return View(BSM);
                }
            }
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_ED", user.Lang);
            return View(BSM);

        }
        #endregion

        #region Details
        public ActionResult Details(string id)
        {
            ActionName = "Details";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = id;

            if (id != null)
            {
                BSM.OnDetailLoading(id);
                if (BSM.AssignHeader != null)
                {
                    Session[Index_Sess_Obj + ActionName] = BSM;
                     EmployeeKPI(BSM.AssignHeader.CriteriaType, BSM.AssignHeader.PeriodTo.Value.Year);
                    return View(BSM);
                }
            }
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        public ActionResult GridItemsDetails()
        {
            ActionName = "Details";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (IClsKPIAssign)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("GridItemsDetails", BSM.listAssignInsertItem);
        }
        #endregion

        #region Grid Create
        public ActionResult GridItems()
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm(this.KeyName);
            DataSelector();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (IClsKPIAssign)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            EmployeeKPI(BSM.AssignHeader.CriteriaType, BSM.AssignHeader.PeriodTo.Value.Year);
            return PartialView("GridItems", BSM.listAssignInsertItem);
        }
        public ActionResult CreateItems(HRKPIAssignItem ModelObject)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (IClsKPIAssign)Session[Index_Sess_Obj + ActionName];
                    }
                    var msg = BSM.OnGridModify(ModelObject, SYActionBehavior.ADD.ToString());
                    if (msg == SYConstant.OK)
                    {
                        Session[Index_Sess_Obj + ActionName] = BSM;
                    }
                    else
                    {
                        ViewData["EditError"] = SYMessages.getMessage(msg, user.Lang);
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
            Session[Index_Sess_Obj + ActionName] = BSM;
            EmployeeKPI(BSM.AssignHeader.CriteriaType, BSM.AssignHeader.PeriodTo.Value.Year);
            return PartialView("GridItems", BSM.listAssignInsertItem);

        }
        public ActionResult EditItems(HRKPIAssignItem ModelObject)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (IClsKPIAssign)Session[Index_Sess_Obj + ActionName];
                var msg = BSM.OnGridModify(ModelObject, SYActionBehavior.EDIT.ToString());
                if (msg == SYConstant.OK)
                {
                    Session[Index_Sess_Obj + ActionName] = BSM;
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage(msg, user.Lang);
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
                EmployeeKPI(BSM.AssignHeader.CriteriaType, BSM.AssignHeader.PeriodTo.Value.Year);
            }
            return PartialView("GridItems", BSM.listAssignInsertItem);
        }
        public ActionResult DeleteItems(HRKPIAssignItem ModelObject)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (IClsKPIAssign)Session[Index_Sess_Obj + ActionName];
                var msg = BSM.OnGridModify(ModelObject, SYActionBehavior.DELETE.ToString());
                if (msg == SYConstant.OK)
                {
                    Session[Index_Sess_Obj + ActionName] = BSM;
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage(msg, user.Lang);
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return PartialView("GridItems", BSM.listAssignInsertItem);
        }
        #endregion

        #region ReleaseTheDoc
        public ActionResult ReleaseDoc(string id)
        {
            ActionName = "Details";
            UserSession();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (IClsKPIAssign)Session[Index_Sess_Obj + ActionName];
            }
            if (id != null)
            {
                string resutl = BSM.ReleaseDoc(id);
                if (resutl == SYConstant.OK)
                {
                    SYMessages mess = SYMessages.getMessageObject("DOC_RELEASED", user.Lang);
                    mess.DocumentNumber = id;
                    Session[SYSConstant.MESSAGE_SUBMIT] = mess;
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
                }
                else
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
                }
            }
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        #endregion
        #region AcceptTheDoc

        public ActionResult Accept(string id)
        {
            ActionName = "Details";
            UserSession();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (IClsKPIAssign)Session[Index_Sess_Obj + ActionName];
            }
            if (id != null)
            {
                string resutl = BSM.Accept(id, true);
                if (resutl == SYConstant.OK)
                {
                    SYMessages mess = SYMessages.getMessageObject("DOC_ACCEPT", user.Lang);
                    mess.DocumentNumber = id;
                    Session[SYSConstant.MESSAGE_SUBMIT] = mess;
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
                }
                else
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
                }
            }
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        #endregion

        #region "Print"
        public ActionResult Print(string id)
        {
            ActionName = "Print";
            UserSession();
            UserConf(ActionBehavior.VIEWONLY);
            ViewData[Humica.EF.SYSConstant.PARAM_ID] = id;
            UserMVCReportView();
            return View("ReportView");
        }
        public ActionResult DocumentViewerPartial(string id)
        {
            UserSession();
            UserConf(ActionBehavior.VIEWONLY);
            ActionName = "Print";
            var SD = unitOfWork.Set<HRKPIAssignHeader>().FirstOrDefault(w => w.AssignCode == id);
            if (SD != null)
            {
                try
                {
                    ViewData[Humica.EF.SYSConstant.PARAM_ID] = id;
                    var sa = new RptKPIAssign();
                    var objRpt = unitOfWork.Set<CFReportObject>().Where(w => w.ScreenID == SCREEN_ID
                   && w.IsActive == true).ToList();
                    if (objRpt.Count > 0)
                    {
                        sa.LoadLayoutFromXml(ClsConstant.DEFAULT_REPORT_PATH + objRpt.First().ReportObject);
                    }
                    sa.Parameters["AssignCode"].Value = SD.AssignCode;
                    sa.Parameters["AssignCode"].Visible = false;

                    Session[Index_Sess_Obj + ActionName] = sa;
                    Session[Index_Sess_Obj] = sa;
                    return PartialView("PrintForm", sa);
                }
                catch (Exception e)
                {
                   ClsEventLog.SaveEventLogs(SCREEN_ID, user.UserName, id, SYActionBehavior.ADD.ToString(), e, true);
                }
            }
            return null;
        }
        public ActionResult DocumentViewerExportTo(string id)
        {
            ActionName = "Print";

            if (Session[Index_Sess_Obj] != null)
            {
                RptKPIAssign reportModel = (RptKPIAssign)Session[Index_Sess_Obj];
                return ReportViewerExtension.ExportTo(reportModel);
            }
            return null;
        }
        #endregion

        [HttpPost]
        public ActionResult GetTotalWeight(string actionname)
        {
            ActionName = actionname;
            UserSession();
            UserConfListAndForm();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (IClsKPIAssign)Session[Index_Sess_Obj + ActionName];
                var ListItem = BSM.listAssignInsertItem.ToList();
                decimal? TotalWeight = 0;
                if (BSM.listAssignInsertItem.ToList().Count() > 0)
                {
                    TotalWeight = BSM.listAssignInsertItem.Sum(w => w.Weight);
                }

                Session[Index_Sess_Obj + ActionName] = BSM;
                var data = new
                {
                    TotalWeight = TotalWeight,
                    MS = SYSConstant.OK
                };
                return Json(data, JsonRequestBehavior.AllowGet);

            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        protected void EmployeeKPI(string CriteriaType, int InYear)
        {
            foreach (var data in BSM.OnDataSelectorKPI(CriteriaType, InYear))
            {
                ViewData[data.Key] = data.Value;
            }
        }

        protected void DataSelector()
        {
            foreach (var data in BSM.OnDataSelectorLoading())
            {
                ViewData[data.Key] = data.Value;
            }
        }
    }
}