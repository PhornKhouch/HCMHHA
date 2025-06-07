using Humica.Core.DB;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic.PR;
using Humica.Models.SY;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.WH.OP
{
    public class PRRewardTypeController : Humica.EF.Controllers.MasterSaleController
    {
        private const string SCREEN_ID = "PRS0000004";
        private const string URL_SCREEN = "/PR/PRS/PRRewardType/";
        private string KeyName = "Code";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName = "";
        HumicaDBContext DB = new HumicaDBContext();
        public PRRewardTypeController()
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
            PRSRewardType BSM = new PRSRewardType();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                var obj = (PRSRewardType)Session[Index_Sess_Obj + ActionName];
            }
            BSM.ListHeader = DB.PR_RewardsType.ToList();
            Session[Index_Sess_Obj + ActionName] = BSM;

            return View(BSM);
        }
        public ActionResult PartialList()
        {
            ActionName = "Index";
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);
            PRSRewardType BSM = new PRSRewardType();
            BSM.ListHeader = new List<PR_RewardsType>();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (PRSRewardType)Session[Index_Sess_Obj + ActionName];
            }
            return PartialView(SYListConfuration.ListDefaultView, BSM.ListHeader);
        }
        #endregion
        #region "Create"
        public ActionResult Create()
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            PRSRewardType BSM = new PRSRewardType();
            UserConfListAndForm(this.KeyName);
            BSM.Header = new PR_RewardsType();
            BSM.Header.Amount = 0;
            BSM.Header.IsBIMonthly = false;
            BSM.Header.BIPercentageAm = 0;
            BSM.Header.ReCode = "ALLW";
            Session[Index_Sess_Obj + ActionName] = BSM;
            return View(BSM);

        }
        [HttpPost]
        public ActionResult Create(PRSRewardType MModel)
        {
            ActionName = "Create";
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);
            PRSRewardType BSM = new PRSRewardType();
            BSM = MModel;
            if (ModelState.IsValid)
            {
                BSM.ScreenId = SCREEN_ID;
                try
                {
                    string msg = BSM.CreateRawardType();
                    if (msg == SYConstant.OK)
                    {
                        SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                        mess.DocumentNumber = BSM.Header.Code;
                        mess.NavigationUrl = SYUrl.getBaseUrl() + URL_SCREEN + "Details/" + mess.DocumentNumber;

                        Session[Index_Sess_Obj + ActionName] = BSM;
                        Session[SYConstant.MESSAGE_SUBMIT] = mess;
                        return Redirect(SYUrl.getBaseUrl() + URL_SCREEN + "Create");
                    }
                    else
                    {
                        ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                        return View(BSM);
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                    return View(BSM);
                }
            }
            else
            {
                ViewData["EditError"] = SYMessages.getMessage("EE001", user.Lang);
                return View(BSM);
            }
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            return View(BSM);
        }
        #endregion
        #region "Edit"
        public ActionResult Edit(string id)
        {
            UserSession();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = id;
            DataSelector();
            if (id == "null") id = null;
            if (id != null)
            {
                PRSRewardType BSM = new PRSRewardType();
                var Header = DB.PR_RewardsType.ToList();
                BSM.Header = Header.FirstOrDefault(w => w.Code == id);
                if (BSM.Header != null)
                {
                    return View(BSM);
                }
            }

            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("RAWARDTYPE_NE", user.Lang);
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        [HttpPost]
        public ActionResult Edit(string id, PRSRewardType BSM)
        {
            UserSession();
            DataSelector();
            this.ScreendIDControl = SCREEN_ID;
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = id;
            if (id != null)
            {
                BSM.ScreenId = SCREEN_ID;
                string msg = BSM.EditRawardType(id);
                if (msg == SYConstant.OK)
                {
                    //MS001=Message load data has complate
                    SYMessages mess = SYMessages.getMessageObject("MS001", user.Lang);
                    mess.DocumentNumber = BSM.Header.Code;
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
            //EE001= Message load Error data not complate
            ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("EE001", user.Lang);
            return View(BSM);

        }
        #endregion
        #region "Detail"
        public ActionResult Details(string id)
        {
            UserSession();
            DataSelector();
            UserConfForm(SYActionBehavior.EDIT);
            ViewData[SYSConstant.PARAM_ID] = id;
            if (id == "null") id = null;
            if (id != null)
            {
                PRSRewardType BSM = new PRSRewardType();
                var Header = DB.PR_RewardsType.ToList();

                BSM.Header = Header.FirstOrDefault(w => w.Code == id);
                if (BSM.Header != null)
                {
                    return View(BSM);
                }
                else
                {
                    ViewData[SYConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("MS001", user.Lang);
                    return View(BSM);
                }
            }

            Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("RAWARDTYPE_NE", user.Lang);
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
                PRSRewardType GLA = new PRSRewardType();
                string msg = GLA.DeleteRawardType(id);
                if (msg == SYConstant.OK)
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("RAWARDTYPE_DEL", user.Lang);
                }
                else
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
                }
            }
            else
            {
                Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("RAWARDTYPE_NE", user.Lang);
            }
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);

        }

        #endregion
        private void DataSelector()
        {
            RawardType RT = new RawardType();
            List<RawardType> List = RT.ListRawardType();
            ViewData["RAWARD_SELECT"] = List;

            SYDataList syDataList = new SYDataList("TERM_CALCULATION");
            List<SYData> syData = new List<SYData> { new SYData() };
            object obj = new List<SYData>();
            obj = syDataList.ListData;
            syData.AddRange((List<SYData>)obj);

        }
    }


}
