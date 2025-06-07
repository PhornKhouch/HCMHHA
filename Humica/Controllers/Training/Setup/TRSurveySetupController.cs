using System.Web.Mvc;
using Humica.EF;
using System.Data;
using Humica.EF.Models.SY;
using System.Linq;
using System;
using Humica.Training.Setup;
using Humica.Training;
using Humica.Core.DB;

namespace Humica.Controllers.Training.Setup
    {
        public class TRSurveySetupController : EF.Controllers.MasterSaleController
        {

            private const string SCREEN_ID = "TRM000009";
            private const string URL_SCREEN = "/Training/Setup/TRSurveySetup/";
            private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
            private string ActionName;
            private string KeyName = "code";

        Humica.Training.DB.HumicaDBContext DBD = new Humica.Training.DB.HumicaDBContext();
        HumicaDBContext DH= new HumicaDBContext();
        public TRSurveySetupController()
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
            TrainingSurveySetupObject BSM = new TrainingSurveySetupObject();
            BSM.ListSurveyFactor = DBD.TRSurveyFactors.ToList();
            BSM.ListSurveyRegion = DBD.TRSurveyRegions.ToList();
            BSM.ListSurveyRating = DBD.TRSurveyRatings.ToList();
            return View(BSM);
        }
        #region RCMInterveiwRegion

        public ActionResult GridItemApprRegion()
        {
            UserConf(ActionBehavior.EDIT);
            DataSelector();
            TrainingSurveySetupObject BSM = new TrainingSurveySetupObject();
            BSM.ListSurveyRegion = DBD.TRSurveyRegions.ToList();
            return PartialView("GridviewApprRegion", BSM.ListSurveyRegion);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateApprRegion(TRSurveyRegion MModel)
        {
            UserSession();
            DataSelector();
            TrainingSurveySetupObject BSM = new TrainingSurveySetupObject();
            if (ModelState.IsValid)
            {
                try
                {
                    MModel.Code = MModel.Code.ToUpper();
                    DBD.TRSurveyRegions.Add(MModel);
                    int row = DBD.SaveChanges();
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
            BSM.ListSurveyRegion = DBD.TRSurveyRegions.ToList();
            return PartialView("GridviewApprRegion", BSM.ListSurveyRegion);
        }
        //edit
        [HttpPost, ValidateInput(false)]
        public ActionResult EditApprRegion(TRSurveyRegion MModel)
        {
            UserSession();
            DataSelector();
            TrainingSurveySetupObject BSM = new TrainingSurveySetupObject();
            if (ModelState.IsValid)
            {
                try
                {
                    //DB = new HumicaDBContext();
                    DBD=new Humica.Training.DB.HumicaDBContext();
                    var objUpdate = DBD.TRSurveyRegions.FirstOrDefault(w => w.Code == MModel.Code);
                    objUpdate.Description = MModel.Description;
                    objUpdate.SecDescription = MModel.SecDescription;
                    objUpdate.InOrder = MModel.InOrder;
                    objUpdate.IsRating = MModel.IsRating;
                    objUpdate.IsComment = MModel.IsComment;
                    objUpdate.IsRating = MModel.IsRating;
                    objUpdate.Remark = MModel.Remark;
                    DBD.TRSurveyRegions.Attach(objUpdate);
                    DBD.Entry(objUpdate).State = System.Data.Entity.EntityState.Modified;
                    int row = DBD.SaveChanges();
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
            BSM.ListSurveyRegion = DBD.TRSurveyRegions.OrderBy(w => w.Code).ToList();
            return PartialView("GridviewApprRegion", BSM.ListSurveyRegion);
        }
        //delete
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteApprRegion(string Code)
        {
            UserSession();
            DataSelector();
            TrainingSurveySetupObject BSM = new TrainingSurveySetupObject();
            if (Code != null)
            {
                try
                {
                    var obj = DBD.TRSurveyRegions.FirstOrDefault(w => w.Code == Code);
                    if (obj != null)
                    {
                        DBD.TRSurveyRegions.Remove(obj);
                        int row = DBD.SaveChanges();
                    }
                    BSM.ListSurveyRegion = DBD.TRSurveyRegions.OrderBy(w => w.Code).ToList();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }

            return PartialView("GridviewApprRegion", BSM.ListSurveyRegion);
        }

        #endregion

        #region Appraisal Factor

        public ActionResult GridItemApprFactor()
        {
            UserConf(ActionBehavior.EDIT);
            DataSelector();
            TrainingSurveySetupObject BSM = new TrainingSurveySetupObject();
            BSM.ListSurveyFactor = DBD.TRSurveyFactors.ToList();
            return PartialView("GridviewApprFactor", BSM.ListSurveyFactor);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateApprFactor(TRSurveyFactor MModel)
        {
            UserSession();
            TrainingSurveySetupObject BSM = new TrainingSurveySetupObject();
            if (ModelState.IsValid)
            {
                try
                {
                    MModel.Code = MModel.Code.ToUpper();
                    DBD.TRSurveyFactors.Add(MModel);
                    int row = DBD.SaveChanges();
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
            BSM.ListSurveyFactor = DBD.TRSurveyFactors.ToList();
            DataSelector();
            return PartialView("GridviewApprFactor", BSM.ListSurveyFactor);
        }
        //edit
        [HttpPost, ValidateInput(false)]
        public ActionResult EditApprFactor(TRSurveyFactor MModel)
        {
            UserSession();
            TrainingSurveySetupObject BSM = new TrainingSurveySetupObject();
            if (ModelState.IsValid)
            {
                try
                {
                    var DBU = new HumicaDBContext();
                    var DBD = new Humica.Training.DB.HumicaDBContext();
                    var objUpdate = DBD.TRSurveyFactors.Find(MModel.Code);
                    DBD.TRSurveyFactors.Attach(MModel);
                    DBD.Entry(MModel).State = System.Data.Entity.EntityState.Modified;
                    int row = DBD.SaveChanges();
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
            BSM.ListSurveyFactor = DBD.TRSurveyFactors.OrderBy(w => w.Code).ToList();
            return PartialView("GridviewApprFactor", BSM.ListSurveyFactor);
        }
        // delete
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteApprFactor(string Code)
        {
            UserSession();
            TrainingSurveySetupObject BSM = new TrainingSurveySetupObject();
            if (Code != null)
            {
                try
                {
                    var obj = DBD.TRSurveyFactors.Find(Code);
                    if (obj != null)
                    {
                        DBD.TRSurveyFactors.Remove(obj);
                        int row = DBD.SaveChanges();
                    }
                    BSM.ListSurveyFactor = DBD.TRSurveyFactors.OrderBy(w => w.Code).ToList();
                    // }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            DataSelector();
            return PartialView("GridviewApprFactor", BSM.ListSurveyFactor);
        }

        #endregion

        #region Appraisal Rating

        public ActionResult GridItemApprRating()
        {
            UserConf(ActionBehavior.EDIT);
            DataSelector();
            TrainingSurveySetupObject BSM = new TrainingSurveySetupObject();
            BSM.ListSurveyRating = DBD.TRSurveyRatings.ToList();
            return PartialView("GridviewApprRating", BSM.ListSurveyRating);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateApprRating(TRSurveyRating MModel)
        {
            UserSession();
            TrainingSurveySetupObject BSM = new TrainingSurveySetupObject();
            try
            {
                DBD.TRSurveyRatings.Add(MModel);
                int row = DBD.SaveChanges();
            }
            catch (Exception e)
            {
                ViewData["EditError"] = e.Message;
            }
            BSM.ListSurveyRating = DBD.TRSurveyRatings.ToList();
            DataSelector();
            return PartialView("GridviewApprRating", BSM.ListSurveyRating);
        }
        //edit
        [HttpPost, ValidateInput(false)]
        public ActionResult EditApprRating(TRSurveyRating MModel)
        {
            UserSession();
            TrainingSurveySetupObject BSM = new TrainingSurveySetupObject();
            if (ModelState.IsValid)
            {
                try
                {
                    var DBU = new HumicaDBContext();
                    var DBD = new Humica.Training.DB.HumicaDBContext();
                    var objUpdate = DBD.TRSurveyRatings.Find(MModel.ID);
                    DBD.TRSurveyRatings.Attach(MModel);
                    DBD.Entry(MModel).State = System.Data.Entity.EntityState.Modified;
                    int row = DBD.SaveChanges();
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
            BSM.ListSurveyRating = DBD.TRSurveyRatings.ToList();
            return PartialView("GridviewApprRating", BSM.ListSurveyRating);
        }
        // delete
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteApprRating(int ID)
        {
            UserSession();
            TrainingSurveySetupObject BSM = new TrainingSurveySetupObject();
            if (ID != null)
            {
                try
                {
                    var obj = DBD.TRSurveyRatings.Find(ID);
                    if (obj != null)
                    {
                        DBD.TRSurveyRatings.Remove(obj);
                        int row = DBD.SaveChanges();
                    }
                    BSM.ListSurveyRating = DBD.TRSurveyRatings.ToList();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            DataSelector();
            return PartialView("GridviewApprRating", BSM.ListSurveyRating);
        }

        #endregion
        private void DataSelector()
        {
            ViewData["REGION_SELECT"] = DBD.TRSurveyRegions.ToList().OrderBy(w => w.Description);
            ViewData["EVALUATION_SELECT"] = DBD.TRSurveyRegions.ToList();
            ViewData["CATEGORY_SELECT"] = DBD.TRSurveyRegions.ToList();
        }
    }
}
