using DevExpress.Web.Mvc;
using Humica.Core.DB;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.EF.Repo;
using Humica.Models.SY;
using Humica.Performance;
using HUMICA.Models.Report.Appraisal;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.SelfService.Performance
{
    public class ESSAppraiselFormController : Humica.EF.Controllers.MasterSaleController
    {
        private const string SCREEN_ID = "ESSA000001";
        private const string URL_SCREEN = "/SelfService/Performance/ESSAppraiselForm/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName;
        private string KeyName = "ApprID;Status";
        protected IClsAppraisel BSM;
        IUnitOfWork unitOfWork;
        public ESSAppraiselFormController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
            BSM = new ClsAppraisel();
            BSM.OnLoad();
            unitOfWork = new UnitOfWork<HumicaDBViewContext>(new HumicaDBViewContext());
        }
        #region "List"
        public ActionResult Index()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (IClsAppraisel)Session[Index_Sess_Obj + ActionName];
            }
            BSM.OnIndexLoading(true);
            BSM.OnIndexLoadingPending();
            BSM.ListHeader = BSM.ListHeader.Where(w => !BSM.ListAppraisaPending.Where(x => w.ApprID == x.ApprID).Any()).ToList();
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);
        }
        public ActionResult PartialList()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (IClsAppraisel)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView(SYListConfuration.ListDefaultView, BSM.ListHeader);
        }
        public ActionResult PendingList()
        {
            ActionName = "Index";
            UserSession();
            UserConfList(KeyName);
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (IClsAppraisel)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView("PendingList", BSM.ListAppraisaPending);
        }
        #endregion
        #region "Edit"
        public ActionResult Edit(string id)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = id;
            if (id != null)
            {
                var msg = BSM.OnEditESSLoading(id);
                if (msg != SYConstant.OK)
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
                }
                else
                {
                    Session[Index_Sess_Obj + ActionName] = BSM;
                    return View(BSM);
                }
            }
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        [HttpPost]

        public ActionResult Edit(string id, ClsAppraisel collection)
        {
            ActionName = "Create";
            UserSession();
            this.ScreendIDControl = SCREEN_ID;
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = id;
            if (id != null)
            {
                if (Session[Index_Sess_Obj + ActionName] != null)
                {
                    BSM = (IClsAppraisel)Session[Index_Sess_Obj + ActionName];
                }
                BSM.Header = collection.Header;
                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.Update(id, true);
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
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
                }
            }
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_INV", user.Lang);
            //return View(BSM);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

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
                if (BSM.Header != null)
                {
                    Session[Index_Sess_Obj + ActionName] = BSM;
                    return View(BSM);
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
            var SD = unitOfWork.Set<HREmpAppraisal>().FirstOrDefault(w => w.ApprID == id);
            if (SD != null)
            {
                try
                {
                    ViewData[Humica.EF.SYSConstant.PARAM_ID] = id;
                    var sa = new RptFromAppraisal();
                    var objRpt = unitOfWork.Set<CFReportObject>().Where(w => w.ScreenID == SCREEN_ID
                   && w.IsActive == true).ToList();
                    if (objRpt.Count > 0)
                    {
                        sa.LoadLayoutFromXml(ClsConstant.DEFAULT_REPORT_PATH + objRpt.First().ReportObject);
                    }
                    sa.Parameters["AppraisalID"].Value = SD.ApprID;
                    sa.Parameters["AppraisalID"].Visible = false;

                    Session[Index_Sess_Obj + ActionName] = sa;
                    Session[Index_Sess_Obj] = sa;
                    return PartialView("PrintForm", sa);
                }
                catch (Exception e)
                {
                    /*------------------SaveLog----------------------------------*/
                    SYEventLog log = new SYEventLog();
                    log.ScreenId = SCREEN_ID;
                    log.UserId = user.UserID.ToString();
                    //log.DocurmentAction = ;
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

            if (Session[Index_Sess_Obj] != null)
            {
                RptFromAppraisal reportModel = (RptFromAppraisal)Session[Index_Sess_Obj];
                return ReportViewerExtension.ExportTo(reportModel);
            }
            return null;
        }
        #endregion
        #region 'Action'
        public ActionResult Approve(string id)
        {
            UserSession();
            if (id != null)
            {
                BSM.ScreenId = SCREEN_ID;
                string fileName = Server.MapPath("~/Content/TEMPLATE/humica-e0886-firebase-adminsdk-95iz2-87c45a528b.json");
                string msg = BSM.ApproveDoc(id, fileName);
                if (msg == SYConstant.OK)
                {
                    var mess = SYMessages.getMessageObject("DOC_APPROVED", user.Lang);
                    mess.DocumentNumber = id;
                    mess.Description = mess.Description;
                    mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details?id=" + id;
                    Session[Index_Sess_Obj + ActionName] = null;
                    Session[SYConstant.MESSAGE_SUBMIT] = mess;
                    return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
                }
            }
            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_NE", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        #endregion
        [HttpPost]
        public ActionResult RatingChange(string Code, string Value, string Region, string Action)
        {
            this.ActionName = Action;
            UserSession();
            UserConfListAndForm();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (IClsAppraisel)Session[Index_Sess_Obj + ActionName];
                if (string.IsNullOrEmpty(Value)) Value = "0";
                int Rating = Convert.ToInt32(Value);
                decimal Score = 0;
                decimal Weighting = 0;
                var checkexist = BSM.ListScore.Where(w => w.Code == Code && w.Region == Region).ToList();
                var _Factor = BSM.ListFactor.FirstOrDefault(w => w.Code == Code && w.Region == Region);
                if (_Factor == null) Weighting = 0;
                //else Weighting = _Factor.Weighting;
                if (checkexist.Count == 0)
                {
                    var obj = new HREmpAppraisalItem();
                    obj.Code = Code;
                    obj.RatingID = Rating;
                    //obj.Weighting = _Factor.Weighting;
                    obj.Score = (Weighting * Rating) / 100.00M;
                    Score = (Weighting * Rating) / 100.00M;
                    BSM.ListScore.Add(obj);
                }
                else
                {
                    checkexist.First().Code = Code;
                    checkexist.First().RatingID = Rating;
                    if (Rating != 0)
                        checkexist.First().Score = Weighting / Rating;
                    else checkexist.First().Score = 0;
                    Score = (Weighting * Rating) / 100.00M;
                }
                var result = new
                {
                    MS = SYConstant.OK,
                    Scoring = Score
                };
                Session[Index_Sess_Obj + ActionName] = BSM;
                return Json(result, JsonRequestBehavior.DenyGet);
            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        [HttpPost]
        public ActionResult CommentValue(string Code, string Comment, string Region, string Action)
        {
            this.ActionName = Action;
            UserSession();
            UserConfListAndForm();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (IClsAppraisel)Session[Index_Sess_Obj + ActionName];

                var checkexist = BSM.ListScore.Where(w => w.Code == Code).ToList();
                if (checkexist.Count == 0)
                {
                    var obj = new HREmpAppraisalItem();
                    obj.Code = Code;
                    obj.Region = Region;
                    obj.Comment = Comment;
                    BSM.ListScore.Add(obj);
                }
                else
                {
                    checkexist.First().Code = Code;
                    checkexist.First().Comment = Comment;
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
        protected void DataSelector()
        {
            foreach (var data in BSM.OnDataSelectorLoading())
            {
                ViewData[data.Key] = data.Value;
            }
        }
    }
}