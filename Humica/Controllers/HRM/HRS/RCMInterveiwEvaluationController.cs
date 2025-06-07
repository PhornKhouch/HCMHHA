using Humica.Core.DB;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic.RCM;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.HRM.HRS
{
    public class RCMInterveiwEvaluationController : Humica.EF.Controllers.MasterSaleController

    {
        private const string SCREEN_ID = "INF0000012";
        private const string URL_SCREEN = "/HRM/HRS/RCMInterveiwEvaluation/";

        HumicaDBContext DB = new HumicaDBContext();
        SMSystemEntity SM = new SMSystemEntity();
        public RCMInterveiwEvaluationController()
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
            RCMInterveiwEvaluation BSM = new RCMInterveiwEvaluation();
            BSM.ListInterveiwFactor = DB.RCMInterveiwFactors.ToList();
            BSM.ListInterveiwRegion = DB.RCMInterveiwRegions.ToList();
            BSM.ListInterveiwRating = DB.RCMInterveiwRatings.ToList();
            return View(BSM);
        }
        #region RCMInterveiwRegion

        public ActionResult GridItemApprRegion()
        {
            UserConf(ActionBehavior.EDIT);
            DataSelector();
            RCMInterveiwEvaluation BSM = new RCMInterveiwEvaluation();
            BSM.ListInterveiwRegion = DB.RCMInterveiwRegions.ToList();
            return PartialView("GridviewApprRegion", BSM.ListInterveiwRegion);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateApprRegion(RCMInterveiwRegion MModel)
        {
            UserSession();
            DataSelector();
            RCMInterveiwEvaluation BSM = new RCMInterveiwEvaluation();
            if (ModelState.IsValid)
            {
                try
                {
                    MModel.Code = MModel.Code.ToUpper();
                    DB.RCMInterveiwRegions.Add(MModel);
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
            BSM.ListInterveiwRegion = DB.RCMInterveiwRegions.ToList();
            return PartialView("GridviewApprRegion", BSM.ListInterveiwRegion);
        }
        //edit
        [HttpPost, ValidateInput(false)]
        public ActionResult EditApprRegion(RCMInterveiwRegion MModel)
        {
            UserSession();
            DataSelector();
            RCMInterveiwEvaluation BSM = new RCMInterveiwEvaluation();
            if (ModelState.IsValid)
            {
                try
                {
                    DB = new HumicaDBContext();
                    var objUpdate = DB.RCMInterveiwRegions.FirstOrDefault(w => w.Code == MModel.Code);
                    objUpdate.Description = MModel.Description;
                    objUpdate.SecDescription = MModel.SecDescription;
                    objUpdate.InOrder = MModel.InOrder;
                    objUpdate.IsRating = MModel.IsRating;
                    objUpdate.IsComment = MModel.IsComment;
                    objUpdate.Rating = MModel.Rating;
                    objUpdate.Remark = MModel.Remark;
                    DB.RCMInterveiwRegions.Attach(objUpdate);
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
            BSM.ListInterveiwRegion = DB.RCMInterveiwRegions.OrderBy(w => w.Code).ToList();
            return PartialView("GridviewApprRegion", BSM.ListInterveiwRegion);
        }
        //delete
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteApprRegion(string Code)
        {
            UserSession();
            DataSelector();
            RCMInterveiwEvaluation BSM = new RCMInterveiwEvaluation();
            if (Code != null)
            {
                try
                {
                    var obj = DB.RCMInterveiwRegions.FirstOrDefault(w => w.Code == Code);
                    if (obj != null)
                    {
                        DB.RCMInterveiwRegions.Remove(obj);
                        int row = DB.SaveChanges();
                    }
                    BSM.ListInterveiwRegion = DB.RCMInterveiwRegions.OrderBy(w => w.Code).ToList();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }

            return PartialView("GridviewApprRegion", BSM.ListInterveiwRegion);
        }

        #endregion

        #region Appraisal Factor

        public ActionResult GridItemApprFactor()
        {
            UserConf(ActionBehavior.EDIT);
            DataSelector();
            RCMInterveiwEvaluation BSM = new RCMInterveiwEvaluation();
            BSM.ListInterveiwFactor = DB.RCMInterveiwFactors.ToList();
            return PartialView("GridviewApprFactor", BSM.ListInterveiwFactor);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateApprFactor(RCMInterveiwFactor MModel)
        {
            UserSession();
            RCMInterveiwEvaluation BSM = new RCMInterveiwEvaluation();
            if (ModelState.IsValid)
            {
                try
                {
                    MModel.Code = MModel.Code.ToUpper();
                    DB.RCMInterveiwFactors.Add(MModel);
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
            BSM.ListInterveiwFactor = DB.RCMInterveiwFactors.ToList();
            DataSelector();
            return PartialView("GridviewApprFactor", BSM.ListInterveiwFactor);
        }
        //edit
        [HttpPost, ValidateInput(false)]
        public ActionResult EditApprFactor(RCMInterveiwFactor MModel)
        {
            UserSession();
            RCMInterveiwEvaluation BSM = new RCMInterveiwEvaluation();
            if (ModelState.IsValid)
            {
                try
                {
                    var DBU = new HumicaDBContext();
                    var objUpdate = DBU.RCMInterveiwFactors.Find(MModel.Code);
                    DB.RCMInterveiwFactors.Attach(MModel);
                    DB.Entry(MModel).State = System.Data.Entity.EntityState.Modified;
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
            DataSelector();
            BSM.ListInterveiwFactor = DB.RCMInterveiwFactors.OrderBy(w => w.Code).ToList();
            return PartialView("GridviewApprFactor", BSM.ListInterveiwFactor);
        }
        // delete
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteApprFactor(string Code)
        {
            UserSession();
            RCMInterveiwEvaluation BSM = new RCMInterveiwEvaluation();
            if (Code != null)
            {
                try
                {
                    var obj = DB.RCMInterveiwFactors.Find(Code);
                    if (obj != null)
                    {
                        DB.RCMInterveiwFactors.Remove(obj);
                        int row = DB.SaveChanges();
                    }
                    BSM.ListInterveiwFactor = DB.RCMInterveiwFactors.OrderBy(w => w.Code).ToList();
                    // }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            DataSelector();
            return PartialView("GridviewApprFactor", BSM.ListInterveiwFactor);
        }

        #endregion

        #region Appraisal Rating

        public ActionResult GridItemApprRating()
        {
            UserConf(ActionBehavior.EDIT);
            DataSelector();
            RCMInterveiwEvaluation BSM = new RCMInterveiwEvaluation();
            BSM.ListInterveiwRating = DB.RCMInterveiwRatings.ToList();
            return PartialView("GridviewApprRating", BSM.ListInterveiwRating);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateApprRating(RCMInterveiwRating MModel)
        {
            UserSession();
            RCMInterveiwEvaluation BSM = new RCMInterveiwEvaluation();
            try
            {
                DB.RCMInterveiwRatings.Add(MModel);
                int row = DB.SaveChanges();
            }
            catch (Exception e)
            {
                ViewData["EditError"] = e.Message;
            }
            BSM.ListInterveiwRating = DB.RCMInterveiwRatings.ToList();
            DataSelector();
            return PartialView("GridviewApprRating", BSM.ListInterveiwRating);
        }
        //edit
        [HttpPost, ValidateInput(false)]
        public ActionResult EditApprRating(RCMInterveiwRating MModel)
        {
            UserSession();
            RCMInterveiwEvaluation BSM = new RCMInterveiwEvaluation();
            if (ModelState.IsValid)
            {
                try
                {
                    var DBU = new HumicaDBContext();
                    var objUpdate = DBU.RCMInterveiwRatings.Find(MModel.ID);
                    DB.RCMInterveiwRatings.Attach(MModel);
                    DB.Entry(MModel).State = System.Data.Entity.EntityState.Modified;
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
            DataSelector();
            BSM.ListInterveiwRating = DB.RCMInterveiwRatings.ToList();
            return PartialView("GridviewApprRating", BSM.ListInterveiwRating);
        }
        // delete
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteApprRating(int ID)
        {
            UserSession();
            RCMInterveiwEvaluation BSM = new RCMInterveiwEvaluation();
            if (ID != null)
            {
                try
                {
                    var obj = DB.RCMInterveiwRatings.Find(ID);
                    if (obj != null)
                    {
                        DB.RCMInterveiwRatings.Remove(obj);
                        int row = DB.SaveChanges();
                    }
                    BSM.ListInterveiwRating = DB.RCMInterveiwRatings.ToList();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            DataSelector();
            return PartialView("GridviewApprRating", BSM.ListInterveiwRating);
        }

        #endregion
        private void DataSelector()
        {
            ViewData["REGION_SELECT"] = DB.RCMInterveiwRegions.ToList().OrderBy(w => w.Description);
            ViewData["EVALUATION_SELECT"] = DB.RCMInterveiwRegions.ToList();
            ViewData["CATEGORY_SELECT"] = DB.RCMInterveiwRegions.ToList();
        }
    }
}
