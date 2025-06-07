using DevExpress.Web.Mvc;
using Humica.Core.DB;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic.MD;
using Humica.Logic.Mission;
using Humica.Models.SY;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.HRM.RCM
{
    public class MISPlanController : Humica.EF.Controllers.MasterSaleController
    {
        private const string SCREEN_ID = "MIS0000001";
        private const string URL_SCREEN = "/HRM/Mission/MISPlan/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "MissionCode;Status";
        private string _DOCTYPE_ = "_DOCTYPE_";
        private string _LOCATION_ = "_LOCATION_";
        private string PATH_FILE = "12313123123sadfsdfsdfsdf";

        SMSystemEntity SMS = new SMSystemEntity();
        HumicaDBContext DB = new HumicaDBContext();
        HumicaDBViewContext DBV = new HumicaDBViewContext();
        public MISPlanController()
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
            HRMissPlanObject BSM = new HRMissPlanObject();
            BSM.ListHeader = new List<HRMissionPlan>();
            BSM.ListHeader = DB.HRMissionPlans.ToList();
            Session[_DOCTYPE_] = "";
            Session[_LOCATION_] = "";
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        public ActionResult PartialList()
        {
            ActionName = "Index";
            HRMissPlanObject BSM = new HRMissPlanObject();
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissPlanObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView(SYListConfuration.ListDefaultView, BSM.ListHeader);
        }
        #endregion
        #region 'Create'
        public ActionResult Create()
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            HRMissPlanObject BSM = new HRMissPlanObject();
            BSM.Header = new HRMissionPlan();
            BSM.ListItem = new List<HRMissionPlanItem>();
            BSM.ListMember = new List<HRMissionPlanMem>();
            BSM.ListApproval = new List<ExDocApproval>();
            BSM.Header.PlanDate = DateTime.Now;
            BSM.Header.FromDate = DateTime.Now;
            BSM.Header.ToDate = DateTime.Now;
            BSM.Header.Status = SYDocumentStatus.OPEN.ToString();
            BSM.Header.Member = 0;
            BSM.Header.IsDriver = false;
            BSM.Header.MissionType = "LOC";
            BSM.Header.TotalAmount = 0;
            Session["Type"] = "";
            if (Session[PATH_FILE] != null)
            {
                BSM.Header.WorkingPlan = Session[PATH_FILE].ToString();
                Session[PATH_FILE] = null;
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            LoadSession(BSM.Header.MissionType, BSM.Header.Branch);
            return View(BSM);
        }
        [HttpPost]
        public ActionResult Create(HRMissPlanObject collection)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.ADD);

            HRMissPlanObject BSM = new HRMissPlanObject();
            if (ModelState.IsValid)
            {
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (HRMissPlanObject)Session[Index_Sess_Obj + ActionName];
                    BSM.Header = collection.Header;
                }

                if (Session[PATH_FILE] != null)
                {
                    BSM.Header.WorkingPlan = Session[PATH_FILE].ToString();
                    Session[PATH_FILE] = null;
                }
                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.CreateMissPlan();

                if (msg == SYConstant.OK)
                {
                    SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                    mess.DocumentNumber = BSM.Header.MissionCode;
                    mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + mess.DocumentNumber;

                    Session[Index_Sess_Obj + ActionName] = BSM;
                    Session[SYConstant.MESSAGE_SUBMIT] = mess;
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Create");
                }
                else
                {
                    Session[Index_Sess_Obj + this.ActionName] = BSM;
                    ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                    return View(BSM);
                }
            }
            Session[Index_Sess_Obj + this.ActionName] = BSM;
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            return View(BSM);
        }
        #endregion 
        #region 'Edit'
        public ActionResult Edit(string id)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            HRMissPlanObject BSM = new HRMissPlanObject();
            UserConfListAndForm();
            Session["Type"] = "";

            BSM.Header = DB.HRMissionPlans.FirstOrDefault(w => w.MissionCode == id);
            if (BSM.Header != null)
            {
                if (Session[PATH_FILE] != null)
                {
                    BSM.Header.WorkingPlan = Session[PATH_FILE].ToString();
                    Session[PATH_FILE] = null;
                }
                BSM.ListItem = DB.HRMissionPlanItems.Where(w => w.MissionCode == id).ToList();
                BSM.ListMember = DB.HRMissionPlanMems.Where(w => w.MissionCode == id).ToList();
                BSM.ListApproval = DB.ExDocApprovals.Where(w => w.DocumentNo == id && w.DocumentType == BSM.Header.MissionType).ToList();
                Session[Index_Sess_Obj + ActionName] = BSM;
                LoadSession(BSM.Header.MissionType, BSM.Header.Branch);
                return View(BSM);
            }
            Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE");
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        [HttpPost]
        public ActionResult Edit(string id, HRMissPlanObject collection)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();

            HRMissPlanObject BSM = new HRMissPlanObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissPlanObject)Session[Index_Sess_Obj + ActionName];
                BSM.Header = collection.Header;
            }

            if (Session[PATH_FILE] != null)
            {
                BSM.Header.WorkingPlan = Session[PATH_FILE].ToString();
                Session[PATH_FILE] = null;
            }
            else
            {
                BSM.Header.WorkingPlan = collection.Header.WorkingPlan;
            }
            BSM.ScreenId = SCREEN_ID;

            if (ModelState.IsValid)
            {
                string msg = BSM.UpdateMPlan(id);

                if (msg != SYConstant.OK)
                {
                    SYMessages mess_err = SYMessages.getMessageObject(msg, user.Lang);
                    Session[Index_Sess_Obj + this.ActionName] = BSM;
                    UserConfForm(ActionBehavior.SAVEGRID);
                    Session[SYConstant.MESSAGE_SUBMIT] = mess_err;
                    return View(BSM);
                }

                SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                mess.DocumentNumber = id;
                mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + mess.DocumentNumber; ;
                Session[Index_Sess_Obj + this.ActionName] = BSM;

                Session[SYConstant.MESSAGE_SUBMIT] = mess;
                LoadSession(BSM.Header.MissionType, BSM.Header.Branch);
                return View(BSM);
            }
            Session[Index_Sess_Obj + this.ActionName] = BSM;
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            return View(BSM);
        }
        #endregion 
        #region 'Details'
        public ActionResult Details(string id)
        {
            ActionName = "Details";
            UserSession();
            UserConfForm(SYActionBehavior.VIEW);
            DataSelector();

            HRMissPlanObject BSM = new HRMissPlanObject();
            ViewData[SYConstant.PARAM_ID] = id;
            ViewData[ClsConstant.IS_READ_ONLY] = true;

            BSM.Header = DB.HRMissionPlans.FirstOrDefault(w => w.MissionCode == id);
            if (BSM.Header == null)
            {
                Session[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE");
                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
            }
            if (Session[PATH_FILE] != null)
            {
                BSM.Header.WorkingPlan = Session[PATH_FILE].ToString();
                Session[PATH_FILE] = null;
            }
            BSM.ListItem = DB.HRMissionPlanItems.Where(w => w.MissionCode == id).ToList();
            BSM.ListMember = DB.HRMissionPlanMems.Where(w => w.MissionCode == id).ToList();
            BSM.ListApproval = DB.ExDocApprovals.Where(w => w.DocumentNo == id && w.DocumentType == BSM.Header.MissionType).ToList();
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        #endregion
        #region 'Delete'  
        public ActionResult Delete(string id)
        {
            UserSession();
            HRMissPlanObject BSM = new HRMissPlanObject();
            if (id != null)
            {
                string msg = BSM.deleteMPlan(id);

                if (msg == SYConstant.OK)
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DELETE_M", user.Lang);
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
                }
                else
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
                }
            }
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        #endregion
        #region 'Grid Item'
        public ActionResult GridItem()
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRMissPlanObject BSM = new HRMissPlanObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissPlanObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("GridItem", BSM.ListItem);
        }
        public ActionResult GridItemDetails()
        {
            ActionName = "Details";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRMissPlanObject BSM = new HRMissPlanObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissPlanObject)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("GridItemDetails", BSM.ListItem);
        }
        public ActionResult CreatePItem(HRMissionPlanItem ModelObject)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRMissPlanObject BSM = new HRMissPlanObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissPlanObject)Session[Index_Sess_Obj + ActionName];
            }
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (HRMissPlanObject)Session[Index_Sess_Obj + ActionName];
                    }
                    if (BSM.ListItem.Count == 0)
                    {
                        ModelObject.LineItem = 1;
                    }
                    else
                    {
                        ModelObject.LineItem = BSM.ListItem.Max(w => w.LineItem) + 1;
                    }

                    BSM.ListItem.Add(ModelObject);
                    Session[Index_Sess_Obj + ActionName] = BSM;

                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                ViewData["EditError"] = SYMessages.getMessage("EE001", user.Lang);
            }
            Session["Type"] = ModelObject.Type;
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("GridItem", BSM.ListItem);
        }
        public ActionResult EditPItem(HRMissionPlanItem ModelObject)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            HRMissPlanObject BSM = new HRMissPlanObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissPlanObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListItem.Where(w => w.LineItem == ModelObject.LineItem).ToList();

                if (objCheck.Count > 0)
                {
                    objCheck.First().Amount = ModelObject.Amount;
                    objCheck.First().Remark = ModelObject.Remark;
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return PartialView("GridItem", BSM.ListItem);
        }
        public ActionResult DeletePItem(int LineItem)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            HRMissPlanObject BSM = new HRMissPlanObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissPlanObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListItem.Where(w => w.LineItem == LineItem).ToList();

                if (objCheck.Count > 0)
                {
                    BSM.ListItem.Remove(objCheck.First());
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return PartialView("GridItem", BSM.ListItem);
        }
        #endregion 
        #region 'Grid Member'
        public ActionResult GridMember()
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRMissPlanObject BSM = new HRMissPlanObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissPlanObject)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("GridMember", BSM.ListMember);
        }
        public ActionResult GridMemberDetails()
        {
            ActionName = "Details";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRMissPlanObject BSM = new HRMissPlanObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissPlanObject)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("GridMemberDetails", BSM.ListMember);
        }
        public ActionResult CreatePMember(HRMissionPlanMem ModelObject)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRMissPlanObject BSM = new HRMissPlanObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissPlanObject)Session[Index_Sess_Obj + ActionName];
            }
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (HRMissPlanObject)Session[Index_Sess_Obj + ActionName];
                    }
                    if (!BSM.ListMember.Where(w => w.EmpCode == ModelObject.EmpCode).Any())
                    {
                        BSM.ListMember.Add(ModelObject);
                    }
                    else
                    {
                        ViewData["EditError"] = SYMessages.getMessage("DUPLICATED_ITEM");
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
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                ViewData["EditError"] = SYMessages.getMessage("EE001", user.Lang);
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            return PartialView("GridMember", BSM.ListMember);
        }
        public ActionResult EditPMember(HRMissionPlanMem ModelObject)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            HRMissPlanObject BSM = new HRMissPlanObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissPlanObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListMember.Where(w => w.EmpCode == ModelObject.EmpCode).ToList();

                if (objCheck.Count > 0)
                {
                    objCheck.First().EmpName = ModelObject.EmpName;
                    objCheck.First().Position = ModelObject.Position;
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return PartialView("GridMember", BSM.ListMember);
        }
        public ActionResult DeletePMember(string EmpCode)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm();
            HRMissPlanObject BSM = new HRMissPlanObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissPlanObject)Session[Index_Sess_Obj + ActionName];

                var objCheck = BSM.ListMember.Where(w => w.EmpCode == EmpCode).ToList();

                if (objCheck.Count > 0)
                {
                    BSM.ListMember.Remove(objCheck.First());
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage("HOUSE_NE");
                }
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return PartialView("GridMember", BSM.ListMember);
        }
        #endregion 

        public ActionResult RequestForApproval(string id)
        {
            UserSession();
            HRMissPlanObject BSM = new HRMissPlanObject();
            if (id != null)
            {
                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.requestToApprove(id);
                if (msg == SYConstant.OK)
                {
                    var mess = SYMessages.getMessageObject("DOC_RQ", user.Lang);
                    mess.DocumentNumber = id;
                    mess.Description = mess.Description;
                    mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + id;
                    Session[Index_Sess_Obj + ActionName] = null;
                    Session[SYConstant.MESSAGE_SUBMIT] = mess;
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
                }
            }
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_EV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        public ActionResult Approve(string id)
        {
            UserSession();
            HRMissPlanObject BSM = new HRMissPlanObject();
            if (id != null)
            {

                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.approveTheDoc(id);
                if (msg == SYConstant.OK)
                {
                    var mess = SYMessages.getMessageObject("DOC_APPROVED", user.Lang);
                    mess.DocumentNumber = id;
                    mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + id;
                    Session[Index_Sess_Obj + ActionName] = null;
                    Session[SYConstant.MESSAGE_SUBMIT] = mess;
                }
                else
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                }
                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
            }
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_EV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        #region "Cancel"
        public ActionResult Cancel(string id)
        {
            this.UserSession();
            ViewData[SYSConstant.PARAM_ID] = id;
            HRMissPlanObject BSM = new HRMissPlanObject();
            if (id.ToString() != "null")
            {
                string msg = BSM.CancelDoc(id);
                if (msg == SYConstant.OK)
                {
                    SYMessages messageObject = SYMessages.getMessageObject(msg, user.Lang);
                    messageObject.DocumentNumber = id;
                    messageObject.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + id;
                    SYMessages mess = SYMessages.getMessageObject("DOC_CANCEL", user.Lang);
                    Session[SYConstant.MESSAGE_SUBMIT] = mess;
                }
                else
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                }
                return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + id);
            }
            else
            {
                Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("INV_DOC", user.Lang);
            }
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + id);

        }
        #endregion
        public ActionResult Refreshvalue(string id, string action)
        {
            ActionName = action;
            HRMissPlanObject BSM = new HRMissPlanObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissPlanObject)Session[Index_Sess_Obj + ActionName];
                if (BSM.Header == null)
                {
                    BSM.Header = new HRMissionPlan();
                }

                BSM.Header.TotalAmount = 0;
                foreach (var item in BSM.ListItem)
                {
                    BSM.Header.TotalAmount += Convert.ToDecimal(item.Amount);
                }

                var result = new
                {
                    MS = SYConstant.OK,
                    TotalAmount = BSM.Header.TotalAmount,
                };

                return Json(result, JsonRequestBehavior.DenyGet);

            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        public ActionResult RefreshTotal(string action)
        {
            ActionName = action;
            HRMissPlanObject BSM = new HRMissPlanObject();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissPlanObject)Session[Index_Sess_Obj + ActionName];
                if (BSM.Header == null)
                {
                    BSM.Header = new HRMissionPlan();
                }

                BSM.Header.Member = BSM.ListMember.Count();

                var result = new
                {
                    MS = SYConstant.OK,
                    TotalAmount = BSM.Header.Member,
                };

                return Json(result, JsonRequestBehavior.DenyGet);

            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        #region 'Upload'
        [HttpPost]
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
        #endregion
        #region "Ajax Approval"
        public ActionResult SelectParam(string docType, string location)
        {
            UserSession();
            Session[_DOCTYPE_] = docType;
            Session[_LOCATION_] = location;
            var rs = new { MS = SYConstant.OK };
            //Auto Approval
            ActionName = "Create";
            HRMissPlanObject BSM = new HRMissPlanObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissPlanObject)Session[Index_Sess_Obj + ActionName];
                BSM.SetAutoApproval(docType, location, SCREEN_ID);
                Session[Index_Sess_Obj + ActionName] = BSM;
            }
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        public ActionResult GridApproval()
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            HRMissPlanObject BSM = new HRMissPlanObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissPlanObject)Session[Index_Sess_Obj + ActionName];
            }
            DataSelector();
            return PartialView("GridApproval", BSM.ListApproval);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateApproval(ExDocApproval ModelObject)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            HRMissPlanObject BSM = new HRMissPlanObject();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (HRMissPlanObject)Session[Index_Sess_Obj + ActionName];
                    }

                    var msg = BSM.isValidApproval(ModelObject, EnumActionGridLine.Add);
                    if (msg == SYConstant.OK)
                    {

                        if (BSM.ListApproval.Count == 0)
                        {
                            ModelObject.ID = 1;
                        }
                        else
                        {
                            ModelObject.ID = BSM.ListApproval.Max(w => w.ID) + 1;
                        }
                        //  ModelObject.DocumentType = Session[_DOCTYPE_].ToString();
                        ModelObject.DocumentNo = "N/A";
                        BSM.ListApproval.Add(ModelObject);
                    }
                    else
                    {
                        ViewData["EditError"] = SYMessages.getMessage(msg);
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
            DataSelector();

            return PartialView("GridApproval", BSM.ListApproval);
        }

        public ActionResult DeleteApproval(string Approver)
        {
            ActionName = "Create";
            UserSession();
            UserConfListAndForm();
            HRMissPlanObject BSM = new HRMissPlanObject();
            if (ModelState.IsValid)
            {
                try
                {
                    if (Session[Index_Sess_Obj + ActionName] != null)
                    {
                        BSM = (HRMissPlanObject)Session[Index_Sess_Obj + ActionName];
                    }

                    BSM.ListApproval.Where(w => w.Approver == Approver).ToList();
                    if (BSM.ListApproval.Count > 0)
                    {
                        var objDel = BSM.ListApproval.Where(w => w.Approver == Approver).First();
                        BSM.ListApproval.Remove(objDel);
                    }
                    else
                    {
                        ViewData["EditError"] = SYMessages.getMessage("APPROVER_NE");
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
            DataSelector();

            return PartialView("GridApproval", BSM.ListApproval);
        }
        #endregion

        #region 'private code'
        public ActionResult SelectMItemElement(string Actionname, string Code)
        {
            ActionName = Actionname;
            Session["Type"] = Code;
            UserSession();
            HRMissPlanObject BSM = new HRMissPlanObject();
            var listBranch = SYConstant.getBranchDataAccess();
            ViewData[SYSConstant.PARAM_ID] = Code;
            if (Session[Index_Sess_Obj + this.ActionName] != null)
            {
                BSM = (HRMissPlanObject)Session[Index_Sess_Obj + this.ActionName];
            }
            var data = new
            {
                MS = SYSConstant.OK
            };
            return Json(data, (JsonRequestBehavior)1);

        }

        public ActionResult MissSetupItem()
        {
            UserSession();
            return GridViewExtension.GetComboBoxCallbackResult(cboProperties =>
            {
                cboProperties.CallbackRouteValues = new { Controller = "MISPlan", Action = "MissSetupItem" };
                cboProperties.ValueField = "Code";
                cboProperties.TextField = "Description";
                cboProperties.TextFormatString = "{0}:{1}";
                cboProperties.Columns.Add("Code", Humica.EF.Models.SY.SYSettings.getLabel("Code"));
                cboProperties.Columns.Add("Description", Humica.EF.Models.SY.SYSettings.getLabel("Description"));
                cboProperties.BindList(Humica.Logic.Mission.HRMissPlanObject.GetAllMissItem());
            });
        }
        public ActionResult District()
        {
            UserSession();
            UserConfListAndForm();
            if (Session[_DOCTYPE_].ToString() == "LOC")
            {
                ViewData["PROVICES_SELECT"] = DB.HRProvices.ToList().OrderBy(w => w.Description);
            }
            else
            {
                ViewData["PROVICES_SELECT"] = DB.HRCountries.ToList().OrderBy(w => w.Description);
            }
            return null;
        }
        public ActionResult SelectMissType(string txtType)
        {
            ActionName = "Create";
            HRMissPlanObject BSM = new HRMissPlanObject();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (HRMissPlanObject)Session[Index_Sess_Obj + ActionName];
            }
            var listBranch = SYConstant.getBranchDataAccess();

            if (txtType != null)
            {
                if (txtType == "LOC")
                {
                    var ListBranch_ = SMS.HRBranches.ToList();
                    ViewData["PROVICES_SELECT"] = DB.HRProvices.ToList();
                }
                else
                {
                    ViewData["PROVICES_SELECT"] = DB.HRCountries.ToList();
                }
                var data = new
                {
                    MS = SYConstant.OK,
                    DATA = ViewData["PROVICES_SELECT"]
                };
                return Json(data, JsonRequestBehavior.DenyGet);
            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        private void DataSelector()
        {
            ViewData["MISSION_TYPE_SELECT"] = DB.ExCfWorkFlowItems.Where(w => w.ScreenID == SCREEN_ID).ToList();
            SYDataList objListMiss_Type = new SYDataList("TRAVEL_BY");
            ViewData["TRAVEL_BY_SELECT"] = objListMiss_Type.ListData;
            var objWf = new ExWorkFlowPreference();
            var location = "";

            if (Session[_LOCATION_] != null)
            {
                location = Session[_LOCATION_].ToString();
            }
            var docType = "";
            if (Session[_DOCTYPE_] != null)
            {
                docType = Session[_DOCTYPE_].ToString();
            }
            ViewData["WF_LIST"] = objWf.getApproverListByDocType(docType, location, SCREEN_ID);

            //objList = new SYDataList("MARITAL");
            //ViewData["MARITAL_SELECT"] = objList.ListData;
            //objList = new SYDataList("LANG_SKILLS");
            //ViewData["LANGSKILLS_SELECT"] = objList.ListData;
            //ViewData["BRANCHES_SELECT"] = DB.HRBranches.ToList().OrderBy(w => w.Description);
            //ViewData["COUNTRY_SELECT"] = DB.HRCountries.ToList().OrderBy(w => w.Description);
            //ViewData["NATION_SELECT"] = DB.HRNations.ToList().OrderBy(w => w.Description);
            //ViewData["RelationshipType_LIST"] = DB.HRRelationshipTypes.ToList();
            //ViewData["HREmpEduType_LIST"] = DB.HREduTypes.ToList();
            //var Processing = SYDocumentStatus.PROCESSING.ToString();
            //ViewData["VACANCY_SELECT"] = DB.RCMVacancies.Where(m => m.Status == Processing).ToList();
            //ViewData["LANG_SELECT"] = DB.RCLangs.ToList().OrderBy(w => w.Lang);
            //ViewData["POSITION_SELECT"] = DB.HRPositions.ToList().OrderBy(w => w.Description);
            ViewData["BRANCHES_SELECT"] = SYConstant.getBranchDataAccess();
            //ViewData["CHANNELRECEIVED_SELECT"] = DB.RCAdvertisers.ToList().OrderBy(w => w.Company);
            ViewData["MissType_SELECT"] = DB.HRMissTypes.ToList().OrderBy(w => w.Description);
            ViewData["PROVICES_SELECT"] = DB.HRProvices.ToList().OrderBy(w => w.Description);
            ViewData["STAFF_SELECT"] = DBV.HR_STAFF_VIEW.ToList();
        }
        private void LoadSession(string docType, string location)
        {
            Session[_DOCTYPE_] = docType;
            Session[_LOCATION_] = location;
        }
        #endregion 
    }
}
