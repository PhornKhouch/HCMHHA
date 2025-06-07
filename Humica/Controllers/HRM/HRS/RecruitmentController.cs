using Humica.Core.DB;
using Humica.EF.Models.SY;
using Humica.Logic;
using Humica.Logic.HRS;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.HRM.HRS
{
    public class RecruitmentController : Humica.EF.Controllers.MasterSaleController

    {
        private const string SCREEN_ID = "INF0000006";
        private const string URL_SCREEN = "/HRM/HRS/Recruitment";

        HumicaDBContext DB = new HumicaDBContext();
        public RecruitmentController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
        }
        public ActionResult Index()
        {
            UserSession();
            UserConfListAndForm();
            DataSelector();
            MDRecruitment BSM = new MDRecruitment();
            BSM.ListAdvType = DB.RCMAdvTypes.ToList();
            BSM.ListAds = DB.RCMSAdvertises.ToList();
            BSM.ListLanguage = DB.RCMSLangs.ToList();
            BSM.ListJD = DB.RCMSJobDescs.ToList();
            BSM.ListQuestionnaire = DB.RCMSQuestionnaires.ToList();
            BSM.ListSRefQues = DB.RCMSRefQuestions.ToList();
            return View(BSM);
        }
        #region 'Advertiser Type'
        public ActionResult GridItemAdvType()
        {
            UserConf(ActionBehavior.EDIT);

            MDRecruitment BSM = new MDRecruitment();
            BSM.ListAdvType = DB.RCMAdvTypes.ToList();
            return PartialView("GridItemAdvType", BSM.ListAdvType);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateAdvType(RCMAdvType MModel)
        {
            UserSession();
            MDRecruitment BSM = new MDRecruitment();
            if (ModelState.IsValid)
            {
                try
                {
                    if (MModel.Code == null)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("EECODE_EN", user.Lang);
                    }
                    else
                    {
                        var chkdup = DB.RCMAdvTypes.FirstOrDefault(w => w.Code == MModel.Code);
                        if (chkdup != null)
                        {
                            ViewData["EditError"] = SYMessages.getMessage("DUPL_KEY", user.Lang);
                        }
                        else
                        {
                            MModel.Code = MModel.Code.ToUpper();
                            DB.RCMAdvTypes.Add(MModel);
                            int row = DB.SaveChanges();
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
            BSM.ListAdvType = DB.RCMAdvTypes.ToList();
            return PartialView("GridItemAdvType", BSM.ListAdvType);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult EditAdvType(RCMAdvType MModel)
        {
            UserSession();
            MDRecruitment BSM = new MDRecruitment();
            if (ModelState.IsValid)
            {
                try
                {
                    DB = new HumicaDBContext();
                    var ObjMatch = DB.RCMAdvTypes.FirstOrDefault(w => w.Code == MModel.Code);
                    ObjMatch.Code = MModel.Code;
                    ObjMatch.Description = MModel.Description;
                    ObjMatch.SecondDescription = MModel.SecondDescription;
                    ObjMatch.Remark = MModel.Remark;

                    DB.RCMAdvTypes.Attach(ObjMatch);

                    DB.Entry(ObjMatch).Property(x => x.Code).IsModified = true;
                    DB.Entry(ObjMatch).Property(x => x.Description).IsModified = true;
                    DB.Entry(ObjMatch).Property(x => x.SecondDescription).IsModified = true;
                    DB.Entry(ObjMatch).Property(x => x.Remark).IsModified = true;

                    DB.SaveChanges();
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
            BSM.ListAdvType = DB.RCMAdvTypes.ToList();
            return PartialView("GridItemAdvType", BSM.ListAdvType);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteAdvType(string Code)
        {
            UserSession();
            MDRecruitment BSM = new MDRecruitment();
            if (Code != null)
            {
                try
                {
                    var obj = DB.RCMAdvTypes.Find(Code);
                    if (obj != null)
                    {
                        DB.RCMAdvTypes.Remove(obj);
                        int row = DB.SaveChanges();
                    }
                    BSM.ListAdvType = DB.RCMAdvTypes.ToList();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }

            return PartialView("GridItemAdvType", BSM.ListAdvType);
        }
        #endregion 
        #region 'Advertiser'
        public ActionResult GridItemAds()
        {
            DataSelector();
            UserConf(ActionBehavior.EDIT);

            MDRecruitment BSM = new MDRecruitment();
            BSM.ListAds = DB.RCMSAdvertises.ToList();
            return PartialView("GridItemAds", BSM.ListAds);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateAds(RCMSAdvertise MModel)
        {
            DataSelector();
            UserSession();
            MDRecruitment BSM = new MDRecruitment();
            if (ModelState.IsValid)
            {
                try
                {
                    if (MModel.Code == null)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("EECODE_EN", user.Lang);
                    }
                    else if (MModel.Company == null)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("EECOM_EN", user.Lang);
                    }
                    else if (MModel.AdsType == null)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("EEADS_EN", user.Lang);
                    }
                    MModel.Code = MModel.Code.ToUpper();
                    DB.RCMSAdvertises.Add(MModel);
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
            BSM.ListAds = DB.RCMSAdvertises.ToList();
            return PartialView("GridItemAds", BSM.ListAds);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult EditAds(RCMSAdvertise MModel)
        {
            DataSelector();
            UserSession();
            MDRecruitment BSM = new MDRecruitment();
            if (ModelState.IsValid)
            {
                try
                {
                    DB = new HumicaDBContext();
                    var ObjMatch = DB.RCMSAdvertises.FirstOrDefault(w => w.Code == MModel.Code && w.Company == MModel.Company);
                    ObjMatch.Address = MModel.Address;
                    ObjMatch.Contact = MModel.Contact;
                    ObjMatch.Email = MModel.Email;
                    ObjMatch.Phone1 = MModel.Phone1;
                    ObjMatch.Phone2 = MModel.Phone2;
                    ObjMatch.TokenCode = MModel.TokenCode;
                    ObjMatch.UserName = MModel.UserName;
                    ObjMatch.Password = MModel.Password;
                    ObjMatch.AppID = MModel.AppID;
                    ObjMatch.AppName = MModel.AppName;
                    ObjMatch.AdsType = MModel.AdsType;

                    DB.RCMSAdvertises.Attach(ObjMatch);

                    DB.Entry(ObjMatch).Property(x => x.Address).IsModified = true;
                    DB.Entry(ObjMatch).Property(x => x.Contact).IsModified = true;
                    DB.Entry(ObjMatch).Property(x => x.Email).IsModified = true;
                    DB.Entry(ObjMatch).Property(x => x.Phone1).IsModified = true;
                    DB.Entry(ObjMatch).Property(x => x.Phone2).IsModified = true;
                    DB.Entry(ObjMatch).Property(x => x.TokenCode).IsModified = true;
                    DB.Entry(ObjMatch).Property(x => x.UserName).IsModified = true;
                    DB.Entry(ObjMatch).Property(x => x.Password).IsModified = true;
                    DB.Entry(ObjMatch).Property(x => x.AppID).IsModified = true;
                    DB.Entry(ObjMatch).Property(x => x.AppName).IsModified = true;
                    DB.Entry(ObjMatch).Property(x => x.AdsType).IsModified = true;

                    DB.SaveChanges();
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
            BSM.ListAds = DB.RCMSAdvertises.OrderBy(w => w.Code).ToList();
            return PartialView("GridItemAds", BSM.ListAds);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteAds(string Code, string Company)
        {
            UserSession();
            MDRecruitment BSM = new MDRecruitment();
            if (Code != null)
            {
                try
                {
                    var obj = DB.RCMSAdvertises.Find(Code, Company);
                    if (obj != null)
                    {
                        DB.RCMSAdvertises.Remove(obj);
                        int row = DB.SaveChanges();
                    }
                    BSM.ListAds = DB.RCMSAdvertises.OrderBy(w => w.Code).ToList();

                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            return PartialView("GridItemAds", BSM.ListAds);
        }
        #endregion 'Advertiser'
        #region 'Job Responsibility'
        public ActionResult GridItemJD()
        {

            DataSelector();
            UserConf(ActionBehavior.EDIT);

            MDRecruitment BSM = new MDRecruitment();
            BSM.ListJD = DB.RCMSJobDescs.ToList();
            return PartialView("GridItemJD", BSM.ListJD);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateJD(RCMSJobDesc MModel)
        {
            DataSelector();
            UserSession();
            MDRecruitment BSM = new MDRecruitment();
            if (ModelState.IsValid)
            {
                try
                {
                    DB.RCMSJobDescs.Add(MModel);
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
            BSM.ListJD = DB.RCMSJobDescs.ToList();
            return PartialView("GridItemJD", BSM.ListJD);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult EditJD(RCMSJobDesc MModel)
        {
            DataSelector();
            UserSession();
            MDRecruitment BSM = new MDRecruitment();
            if (ModelState.IsValid)
            {
                try
                {
                    DB = new HumicaDBContext();
                    var ObjMatch = DB.RCMSJobDescs.FirstOrDefault(w => w.Code == MModel.Code);
                    ObjMatch.Description = MModel.Description;
                    ObjMatch.JobResponsibility = MModel.JobResponsibility;
                    ObjMatch.JobRequirement = MModel.JobRequirement;
                    ObjMatch.Position = MModel.Position;

                    DB.RCMSJobDescs.Attach(ObjMatch);
                    DB.Entry(ObjMatch).Property(x => x.Description).IsModified = true;
                    DB.Entry(ObjMatch).Property(x => x.JobResponsibility).IsModified = true;
                    DB.Entry(ObjMatch).Property(x => x.JobRequirement).IsModified = true;
                    DB.Entry(ObjMatch).Property(x => x.Position).IsModified = true;

                    DB.SaveChanges();
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
            BSM.ListJD = DB.RCMSJobDescs.OrderBy(w => w.Code).ToList();
            return PartialView("GridItemJD", BSM.ListJD);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteJD(string Code)
        {
            DataSelector();
            UserSession();
            MDRecruitment BSM = new MDRecruitment();
            if (Code != null)
            {
                try
                {
                    var obj = DB.RCMSJobDescs.Find(Code);
                    if (obj != null)
                    {
                        DB.RCMSJobDescs.Remove(obj);
                        int row = DB.SaveChanges();
                    }
                    BSM.ListJD = DB.RCMSJobDescs.OrderBy(w => w.Code).ToList();

                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            return PartialView("GridItemJD", BSM.ListJD);
        }
        #endregion 'Advertiser'
        #region 'Languages'
        public ActionResult GridItemLanguage()
        {
            UserConf(ActionBehavior.EDIT);

            MDRecruitment BSM = new MDRecruitment();
            BSM.ListLanguage = DB.RCMSLangs.ToList();
            return PartialView("GridItemLanguage", BSM.ListLanguage);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateLang(RCMSLang MModel)
        {
            UserSession();
            MDRecruitment BSM = new MDRecruitment();
            if (ModelState.IsValid)
            {
                try
                {
                    if (MModel.Code == null)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("EECODE_EN", user.Lang);
                    }
                    else
                    {
                        var chkdup = DB.RCMSLangs.FirstOrDefault(w => w.Code == MModel.Code);
                        if (chkdup != null)
                        {
                            ViewData["EditError"] = SYMessages.getMessage("DUPL_KEY", user.Lang);
                        }
                        else
                        {
                            MModel.Code = MModel.Code.ToUpper();
                            DB.RCMSLangs.Add(MModel);
                            int row = DB.SaveChanges();
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
            BSM.ListLanguage = DB.RCMSLangs.OrderBy(w => w.ID).ToList();
            return PartialView("GridItemLanguage", BSM.ListLanguage);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult EditLang(RCMSLang MModel)
        {
            UserSession();
            MDRecruitment BSM = new MDRecruitment();
            if (ModelState.IsValid)
            {
                try
                {
                    DB = new HumicaDBContext();
                    var ObjMatch = DB.RCMSLangs.FirstOrDefault(w => w.Code == MModel.Code);
                    ObjMatch.Code = MModel.Code;
                    ObjMatch.Description = MModel.Description;
                    ObjMatch.SecondDescription = MModel.SecondDescription;

                    DB.RCMSLangs.Attach(ObjMatch);

                    DB.Entry(ObjMatch).Property(x => x.Code).IsModified = true;
                    DB.Entry(ObjMatch).Property(x => x.Description).IsModified = true;
                    DB.Entry(ObjMatch).Property(x => x.SecondDescription).IsModified = true;

                    DB.SaveChanges();
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
            BSM.ListLanguage = DB.RCMSLangs.OrderBy(w => w.ID).ToList();
            return PartialView("GridItemLanguage", BSM.ListLanguage);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteLang(string Code)
        {
            UserSession();
            MDRecruitment BSM = new MDRecruitment();
            if (Code != null)
            {
                try
                {
                    var obj = DB.RCMSLangs.FirstOrDefault(w => w.Code == Code);
                    if (obj != null)
                    {
                        DB.RCMSLangs.Remove(obj);
                        int row = DB.SaveChanges();
                    }
                    BSM.ListLanguage = DB.RCMSLangs.OrderBy(w => w.ID).ToList();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }

            return PartialView("GridItemLanguage", BSM.ListLanguage);
        }
        #endregion 'Languages'
        #region 'Questionnaire'
        public ActionResult GridItemQuest()
        {
            DataSelector();
            UserConf(ActionBehavior.EDIT);

            MDRecruitment BSM = new MDRecruitment();
            BSM.ListQuestionnaire = DB.RCMSQuestionnaires.OrderBy(w => w.Step).OrderByDescending(w => w.TranNo).ToList();
            return PartialView("GridItemQuest", BSM.ListQuestionnaire);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult CreateQuest(RCMSQuestionnaire MModel)
        {
            DataSelector();
            UserSession();
            MDRecruitment BSM = new MDRecruitment();
            if (ModelState.IsValid)
            {
                try
                {
                    if (MModel.Position == null)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("POSITION_EN", user.Lang);
                    }
                    else
                    {
                        DB.RCMSQuestionnaires.Add(MModel);
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
            BSM.ListQuestionnaire = DB.RCMSQuestionnaires.OrderBy(w => w.Step).OrderByDescending(w => w.TranNo).ToList();
            return PartialView("GridItemQuest", BSM.ListQuestionnaire);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult EdiQuest(RCMSQuestionnaire MModel)
        {
            DataSelector();
            UserSession();
            MDRecruitment BSM = new MDRecruitment();
            if (ModelState.IsValid)
            {
                try
                {
                    DB = new HumicaDBContext();
                    var ObjMatch = DB.RCMSQuestionnaires.FirstOrDefault(w => w.TranNo == MModel.TranNo);
                    ObjMatch.Step = MModel.Step;
                    ObjMatch.Description = MModel.Description;
                    ObjMatch.Position = MModel.Position;

                    DB.RCMSQuestionnaires.Attach(ObjMatch);

                    DB.Entry(ObjMatch).Property(x => x.Step).IsModified = true;
                    DB.Entry(ObjMatch).Property(x => x.Description).IsModified = true;

                    DB.SaveChanges();
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
            BSM.ListQuestionnaire = DB.RCMSQuestionnaires.OrderBy(w => w.Step).OrderByDescending(w => w.TranNo).ToList();
            return PartialView("GridItemQuest", BSM.ListQuestionnaire);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteQuest(int TranNo)
        {
            DataSelector();
            MDRecruitment BSM = new MDRecruitment();

            try
            {
                var obj = DB.RCMSQuestionnaires.Find(TranNo);
                if (obj != null)
                {
                    DB.RCMSQuestionnaires.Remove(obj);
                    int row = DB.SaveChanges();
                }
                BSM.ListQuestionnaire = DB.RCMSQuestionnaires.OrderBy(w => w.Step).OrderByDescending(w => w.TranNo).ToList();
            }
            catch (Exception e)
            {
                ViewData["EditError"] = e.Message;
            }

            return PartialView("GridItemQuest", BSM.ListQuestionnaire);
        }
        #endregion 'Languages'
        #region 'Ref Questionnaire'
        public ActionResult GridItemRefQuest()
        {
            DataSelector();
            UserConf(ActionBehavior.EDIT);

            MDRecruitment BSM = new MDRecruitment();
            BSM.ListSRefQues = DB.RCMSRefQuestions.OrderBy(w => w.step).OrderByDescending(w => w.ID).ToList();
            return PartialView("GridItemRefQuest", BSM.ListSRefQues);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult CreateRefQuest(RCMSRefQuestion MModel)
        {
            DataSelector();
            UserSession();
            MDRecruitment BSM = new MDRecruitment();
            if (ModelState.IsValid)
            {
                try
                {
                    if (MModel.Description == null)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("DOC_NE", user.Lang);
                    }
                    else
                    {
                        DB.RCMSRefQuestions.Add(MModel);
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
            BSM.ListSRefQues = DB.RCMSRefQuestions.OrderBy(w => w.step).OrderByDescending(w => w.ID).ToList();
            return PartialView("GridItemRefQuest", BSM.ListSRefQues);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult EdiRefQuest(RCMSRefQuestion MModel)
        {
            DataSelector();
            UserSession();
            MDRecruitment BSM = new MDRecruitment();
            if (ModelState.IsValid)
            {
                try
                {
                    DB = new HumicaDBContext();
                    var ObjMatch = DB.RCMSRefQuestions.FirstOrDefault(w => w.ID == MModel.ID);
                    ObjMatch.step = MModel.step;
                    ObjMatch.Description = MModel.Description;
                    ObjMatch.SecDescription = MModel.SecDescription;

                    DB.RCMSRefQuestions.Attach(ObjMatch);

                    DB.Entry(ObjMatch).Property(x => x.step).IsModified = true;
                    DB.Entry(ObjMatch).Property(x => x.Description).IsModified = true;
                    DB.Entry(ObjMatch).Property(x => x.SecDescription).IsModified = true;

                    DB.SaveChanges();
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
            BSM.ListSRefQues = DB.RCMSRefQuestions.OrderBy(w => w.step).OrderByDescending(w => w.ID).ToList();
            return PartialView("GridItemRefQuest", BSM.ListSRefQues);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteRefQuest(int ID)
        {
            DataSelector();
            MDRecruitment BSM = new MDRecruitment();

            try
            {
                var obj = DB.RCMSRefQuestions.Find(ID);
                if (obj != null)
                {
                    DB.RCMSRefQuestions.Remove(obj);
                    int row = DB.SaveChanges();
                }
                BSM.ListSRefQues = DB.RCMSRefQuestions.OrderBy(w => w.step).OrderByDescending(w => w.ID).ToList();
            }
            catch (Exception e)
            {
                ViewData["EditError"] = e.Message;
            }

            return PartialView("GridItemRefQuest", BSM.ListSRefQues);
        }
        #endregion 'Languages'
        #region 'Private Code'
        private void DataSelector()
        {
            ViewData["ADS_SELECT"] = DB.RCMAdvTypes.ToList();
            ViewData["POSITION_SELECT"] = ClsFilter.LoadPosition();
        }
        #endregion
    }
}
