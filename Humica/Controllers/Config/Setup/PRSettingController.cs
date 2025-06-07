using Humica.Core.DB;
using Humica.EF;
using Humica.EF.Models.SY;
using Humica.Logic.MD;
using Humica.Logic.PR;
using Humica.Models.SY;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Humica.Controllers.Config.Setup
{
    public class PRSettingController : Humica.EF.Controllers.MasterSaleController
    {
        private const string SCREEN_ID = "INF0000008";
        private const string URL_SCREEN = "/Config/Setup/PRSetting/";
        private string ActionName;
        private string KeyName = "ID";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        HumicaDBContext DB = new HumicaDBContext();
        HumicaDBViewContext DBV = new HumicaDBViewContext();
        private string PARAM_INDEX = "ID;";

        public PRSettingController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
        }

        #region List
        public async Task<ActionResult> Index()
        {
            ActionName = "Index";
            UserSession();
            UserConfList(this.KeyName);
            await DataSelector();
            MDSetting BSM = new MDSetting();
            BSM.Header = new SYHRSetting();
            BSM.ListPensionFundSetting = await DB.PRPensionFundSettings.ToListAsync();
            BSM.isvisible = false;
            var obj = await DB.SYHRSettings.FirstOrDefaultAsync();
            if (obj != null)
            {
                BSM.Header = obj;
            }

            Session[Index_Sess_Obj + ActionName] = BSM;
            ViewData[ClsConstant.PARAM_INDEX] = PARAM_INDEX;
            return View(BSM);
        }
        [HttpPost]
        public async Task<ActionResult> Index(MDSetting collection)
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm();
            await DataSelector();
            MDSetting BSM = new MDSetting();
            BSM = (MDSetting)Session[Index_Sess_Obj + ActionName];
            collection.ListHRSetting = BSM.ListHRSetting;
            collection.ListPensionFundSetting = await DB.PRPensionFundSettings.ToListAsync();

            collection.ScreenId = SCREEN_ID;
            string msg = collection.UpdateSetting();

            if (msg != SYConstant.OK)
            {
                SYMessages mess_err = SYMessages.getMessageObject(msg, user.Lang);
                Session[Index_Sess_Obj + this.ActionName] = collection;
                UserConfForm(ActionBehavior.SAVEGRID);
                Session[SYConstant.MESSAGE_SUBMIT] = mess_err;
                return View(collection);

            }
            SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
            Session[Index_Sess_Obj + this.ActionName] = collection;
            UserConfForm(ActionBehavior.SAVEGRID);
            Session[SYConstant.MESSAGE_SUBMIT] = mess;
            return View(collection);
        }

        public ActionResult ShowHide(string value)
        {

            this.ActionName = "Index";
            UserSession();
            UserConfListAndForm();
            string h_ = "";
            MDSetting BSM = new MDSetting();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (MDSetting)Session[Index_Sess_Obj + ActionName];
                if (value != "LEAVE")
                {
                    h_ = "LEAVE";
                    BSM.hide = h_;
                    BSM.isvisible = true;
                    ViewData["Visiblie"] = false;
                }
                var data = new
                {
                    MS = SYConstant.OK,
                    Value_ = h_
                };
                Session[Index_Sess_Obj + ActionName] = BSM;
                return Json(data, JsonRequestBehavior.DenyGet);
            }
            var rs = new { MS = SYConstant.FAIL };
            return Json(rs, JsonRequestBehavior.DenyGet);
        }
        public async Task<ActionResult> PartialList()
        {
            ActionName = "Index";
            UserSession();
            UserConfList(this.KeyName);
            await DataSelector();
            MDSetting BSM = new MDSetting();
            BSM.Header = new SYHRSetting();
            BSM.ListHRSetting = new List<SYHRSetting>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (MDSetting)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView(SYListConfuration.ListDefaultView, BSM.ListHRSetting);
        }
        #endregion
        #region Pesion Fund Setting
        public ActionResult PensionFundSetting()
        {
            UserConf(ActionBehavior.EDIT);
            MDSetting BSM = new MDSetting();
            BSM.ListPensionFundSetting = DB.PRPensionFundSettings.ToList().OrderBy(w => w.TrainNo).ToList();
            return PartialView("PensionFundSetting", BSM.ListPensionFundSetting);
        }
        public ActionResult CreateItem(PRPensionFundSetting MModel)
        {
            UserSession();
            MDSetting BSM = new MDSetting();
            if (ModelState.IsValid)
            {
                try
                {
                    MModel.CreatedBy = user.UserName;
                    MModel.CreatedOn = DateTime.Today;
                    if (MModel.SeviceLenghtFrom > MModel.SeviceLenghtTo)
                    {
                        ViewData["EditError"] = "Service lenght from cannot greater than service lenght to.";
                    }
                    else if (IsOverlapAmount(MModel))
                        ViewData["EditError"] = "Overlap service lenght.";
                    else
                    {
                        DB.PRPensionFundSettings.Add(MModel);
                        int row = DB.SaveChanges();
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
            BSM.ListPensionFundSetting = DB.PRPensionFundSettings.ToList();
            return PartialView("PensionFundSetting", BSM.ListPensionFundSetting);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult ItemEdit(PRPensionFundSetting MModel)
        {
            UserSession();
            MDSetting BSM = new MDSetting();
            if (ModelState.IsValid)
            {
                try
                {
                    MModel.ChangedBy = user.UserName;
                    MModel.ChangedOn = DateTime.Today;
                    if (MModel.SeviceLenghtFrom >= MModel.SeviceLenghtTo)
                        ViewData["EditError"] = "Service lenght from cannot greater than service lenght to.";
                    else if (IsOverlapAmount(MModel))
                        ViewData["EditError"] = "Overlap service lenght.";
                    else
                    {
                        DB.PRPensionFundSettings.Attach(MModel);
                        DB.Entry(MModel).State = System.Data.Entity.EntityState.Modified;
                        int row = DB.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = SYMessages.getMessage("EE001", user.Lang);

            BSM.ListPensionFundSetting = DB.PRPensionFundSettings.OrderBy(w => w.TrainNo).ToList();
            return PartialView("PensionFundSetting", BSM.ListPensionFundSetting);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult ItemDelete(int TrainNo)
        {
            UserSession();
            MDSetting BSM = new MDSetting();
            if (TrainNo != 0)
            {
                try
                {
                    var obj = DB.PRPensionFundSettings.Find(TrainNo);
                    if (obj != null)
                    {
                        DB.PRPensionFundSettings.Remove(obj);
                        int row = DB.SaveChanges();
                    }

                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            BSM.ListPensionFundSetting = DB.PRPensionFundSettings.OrderBy(w => w.TrainNo).ToList();
            return PartialView("PensionFundSetting", BSM.ListPensionFundSetting);
        }
        private bool IsOverlapAmount(PRPensionFundSetting pfs)
        {
            var obj = DB.PRPensionFundSettings;
            int count = obj.Where(x => (x.SeviceLenghtFrom <= pfs.SeviceLenghtTo & x.SeviceLenghtTo >= pfs.SeviceLenghtFrom) &
            x.TrainNo != pfs.TrainNo).Count();
            return (count > 0) ? true : false;
        }
        #endregion
        private async Task DataSelector()
        {
            IEnumerable<PR_RewardsType> ListReward = await DB.PR_RewardsType.ToListAsync();
            ViewData["TELEGRAM_SELECT"] = await DB.TelegramBots.ToListAsync();
            string DEDType = "DED";
            ViewData["DED_SELECT"] = ListReward.Where(w => w.ReCode == DEDType).ToList();
            ViewData["ALLW_SELECT"] = ListReward.Where(w => w.ReCode == "ALLW").ToList();
            ViewData["SALARTYPE_SELECT"] = ClsSelaryType.LoadData();
            ViewData["LATE_DED_TYPE"] = new SYDataList("LATEDEDTYPE").ListData;
            ViewData["STAFF_SELECT"] = await DBV.HR_STAFF_VIEW.Where(w => w.StatusCode == "A").ToListAsync();
        }
    }

}