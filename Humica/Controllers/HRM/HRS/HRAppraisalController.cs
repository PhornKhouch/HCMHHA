using Humica.Core.DB;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic.HRS;
using Humica.Performance;
using System;
using System.CodeDom;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.HRM.HRS
{
    public class HRAppraisalController : Humica.EF.Controllers.MasterSaleController

    {
        private const string SCREEN_ID = "INF0000005";
        private const string URL_SCREEN = "/HRM/HRS/HRAppraisal";
        private string ActionName;
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        HumicaDBContext DB = new HumicaDBContext();
        SMSystemEntity SM = new SMSystemEntity();
        IClsAppraisalMaster BSM;
        public HRAppraisalController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
            BSM = new ClsAppraisalMaster();
            BSM.OnLoad();
        }
        public ActionResult Index()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm();
            DataSelector();
            BSM.OnIndexLoading();
            return View(BSM);
        }

        #region Appraisal Types

        public ActionResult GridItemApprTypes()
        {
            UserConf(ActionBehavior.EDIT);

            MDAppraisal BSM = new MDAppraisal();
            BSM.ListApprType = DB.HRApprTypes.ToList();
            return PartialView("GridviewApprType", BSM.ListApprType);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateApprType(HRApprType MModel)
        {
            UserSession();
            MDAppraisal BSM = new MDAppraisal();
            if (ModelState.IsValid)
            {
                try
                {
                    MModel.Code = MModel.Code.ToUpper();
                    DB.HRApprTypes.Add(MModel);
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
            BSM.ListApprType = DB.HRApprTypes.ToList();
            return PartialView("GridviewApprType", BSM.ListApprType);
        }
        //edit
        [HttpPost, ValidateInput(false)]
        public ActionResult EditApprType(HRApprType MModel)
        {
            UserSession();
            MDAppraisal BSM = new MDAppraisal();
            if (ModelState.IsValid)
            {
                try
                {
                    var DBU = new HumicaDBContext();
                    var objUpdate = DBU.HRApprTypes.Find(MModel.Code);
                    DB.HRApprTypes.Attach(MModel);
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
            BSM.ListApprType = DB.HRApprTypes.OrderBy(w => w.Code).ToList();
            return PartialView("GridviewApprType", BSM.ListApprType);
        }
        //delete
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteApprType(string Code)
        {
            UserSession();
            MDAppraisal BSM = new MDAppraisal();
            if (Code != null)
            {
                try
                {
                    var obj = DB.HRApprTypes.Find(Code);
                    if (obj != null)
                    {
                        DB.HRApprTypes.Remove(obj);
                        int row = DB.SaveChanges();
                    }
                    BSM.ListApprType = DB.HRApprTypes.OrderBy(w => w.Code).ToList();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }

            return PartialView("GridviewApprType", BSM.ListApprType);
        }
        #endregion

        #region Appraisal Region

        public ActionResult GridItemApprRegion()
        {
            UserConf(ActionBehavior.EDIT);
            DataSelector();
            BSM.OnIndexLoadingRegion();
            return PartialView("GridviewApprRegion", BSM.ListApprRegion);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateApprRegion(HRApprRegion ModelObject)
        {
            UserSession();
            DataSelector();
            if (ModelState.IsValid)
            {
                try
                {
                    var msg = BSM.OnGridModifyRegion(ModelObject, SYActionBehavior.ADD.ToString());
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
            BSM.OnIndexLoadingRegion();
            return PartialView("GridviewApprRegion", BSM.ListApprRegion);
        }
        //edit
        [HttpPost, ValidateInput(false)]
        public ActionResult EditApprRegion(HRApprRegion ModelObject)
        {
            UserSession();
            DataSelector();
            if (ModelState.IsValid)
            {
                try
                {
                    var msg = BSM.OnGridModifyRegion(ModelObject, SYActionBehavior.EDIT.ToString());
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
            BSM.OnIndexLoadingRegion();
            return PartialView("GridviewApprRegion", BSM.ListApprRegion);
        }
        //delete
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteApprRegion(HRApprRegion ModelObject)
        {
            UserSession();
            DataSelector();
            try
            {
                BSM = (IClsAppraisalMaster)Session[Index_Sess_Obj + ActionName];
                var msg = BSM.OnGridModifyRegion(ModelObject, SYActionBehavior.DELETE.ToString());
                if (msg != SYConstant.OK)
                {
                    ViewData["EditError"] = SYMessages.getMessage(msg, user.Lang);
                }
            }
            catch (Exception e)
            {
                ViewData["EditError"] = e.Message;
            }
            BSM.OnIndexLoadingRegion();
            return PartialView("GridviewApprRegion", BSM.ListApprRegion);
        }

        #endregion

        #region Appraisal Factor

        public ActionResult GridItemApprFactor()
        {
            UserConf(ActionBehavior.EDIT);
            DataSelector();
            BSM.OnIndexLoadingFactor();
            return PartialView("GridviewApprFactor", BSM.ListApprFactor);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateApprFactor(HRApprFactor ModelObject)
        {
            UserSession();
            if (ModelState.IsValid)
            {
                try
                {
                    var msg = BSM.OnGridModifyFactor(ModelObject, SYActionBehavior.ADD.ToString());
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
            BSM.OnIndexLoadingFactor();
            DataSelector();
            return PartialView("GridviewApprFactor", BSM.ListApprFactor);
        }
        //edit
        [HttpPost, ValidateInput(false)]
        public ActionResult EditApprFactor(HRApprFactor ModelObject)
        {
            UserSession();
            if (ModelState.IsValid)
            {
                try
                {
                    var msg = BSM.OnGridModifyFactor(ModelObject, SYActionBehavior.EDIT.ToString());
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
            DataSelector();
            BSM.OnIndexLoadingFactor();
            return PartialView("GridviewApprFactor", BSM.ListApprFactor);
        }
        // delete
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteApprFactor(HRApprFactor ModelObject)
        {
            UserSession();
            try
            {
                BSM = (IClsAppraisalMaster)Session[Index_Sess_Obj + ActionName];
                var msg = BSM.OnGridModifyFactor(ModelObject, SYActionBehavior.DELETE.ToString());
                if (msg != SYConstant.OK)
                {
                    ViewData["EditError"] = SYMessages.getMessage(msg, user.Lang);
                }
            }
            catch (Exception e)
            {
                ViewData["EditError"] = e.Message;
            }
            DataSelector();
            BSM.OnIndexLoadingFactor();
            return PartialView("GridviewApprFactor", BSM.ListApprFactor);
        }

        #endregion

        #region Appraisal Rating

        public ActionResult GridItemApprRating()
        {
            UserConf(ActionBehavior.EDIT);
            DataSelector();
            MDAppraisal BSM = new MDAppraisal();
            BSM.ListApprRating = DB.HRApprRatings.ToList();
            return PartialView("GridviewApprRating", BSM.ListApprRating);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateApprRating(HRApprRating MModel)
        {
            UserSession();
            MDAppraisal BSM = new MDAppraisal();
            try
            {
                DB.HRApprRatings.Add(MModel);
                int row = DB.SaveChanges();
            }
            catch (Exception e)
            {
                ViewData["EditError"] = e.Message;
            }
            BSM.ListApprRating = DB.HRApprRatings.ToList();
            DataSelector();
            return PartialView("GridviewApprRating", BSM.ListApprRating);
        }
        //edit
        [HttpPost, ValidateInput(false)]
        public ActionResult EditApprRating(HRApprRating MModel)
        {
            UserSession();
            MDAppraisal BSM = new MDAppraisal();
            if (ModelState.IsValid)
            {
                try
                {
                    var DBU = new HumicaDBContext();
                    var objUpdate = DBU.HRApprRatings.Find(MModel.ID);
                    DB.HRApprRatings.Attach(MModel);
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
            BSM.ListApprRating = DB.HRApprRatings.ToList();
            return PartialView("GridviewApprRating", BSM.ListApprRating);
        }
        // delete
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteApprRating(int ID)
        {
            UserSession();
            MDAppraisal BSM = new MDAppraisal();
            if (ID != null)
            {
                try
                {
                    var obj = DB.HRApprRatings.Find(ID);
                    if (obj != null)
                    {
                        DB.HRApprRatings.Remove(obj);
                        int row = DB.SaveChanges();
                    }
                    BSM.ListApprRating = DB.HRApprRatings.ToList();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            DataSelector();
            return PartialView("GridviewApprRating", BSM.ListApprRating);
        }

        #endregion

        #region Appraisal Result

        public ActionResult GridItemResult()
        {
            UserConf(ActionBehavior.EDIT);
            DataSelector();
            MDAppraisal BSM = new MDAppraisal();
            BSM.ListApprResult = DB.HRAppGrades.OrderBy(w => w.Grade).ToList();
            return PartialView("GridviewResult", BSM.ListApprResult);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateResult(HRAppGrade MModel)
        {
            UserSession();
            MDAppraisal BSM = new MDAppraisal();
            if (ModelState.IsValid)
            {
                try
                {
                    DB.HRAppGrades.Add(MModel);
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
            BSM.ListApprResult = DB.HRAppGrades.OrderBy(w => w.Grade).ToList();
            return PartialView("GridviewResult", BSM.ListApprResult);
        }
        //edit
        [HttpPost, ValidateInput(false)]
        public ActionResult EditResult(HRAppGrade MModel)
        {
            UserSession();
            MDAppraisal BSM = new MDAppraisal();
            if (ModelState.IsValid)
            {
                try
                {
                    var DBU = new HumicaDBContext();
                    var objUpdate = DBU.HRAppGrades.Find(MModel.TranNo);
                    DB.HRAppGrades.Attach(MModel);
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
            BSM.ListApprResult = DB.HRAppGrades.OrderBy(w => w.Grade).ToList();
            return PartialView("GridviewResult", BSM.ListApprResult);
        }
        // delete
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteResult(int TranNo)
        {
            UserSession();
            MDAppraisal BSM = new MDAppraisal();
            if (TranNo != null)
            {
                try
                {

                    var obj = DB.HRAppGrades.Find(TranNo);
                    if (obj != null)
                    {
                        DB.HRAppGrades.Remove(obj);
                        int row = DB.SaveChanges();
                    }
                    BSM.ListApprResult = DB.HRAppGrades.OrderBy(w => w.Grade).ToList();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            return PartialView("GridviewResult", BSM.ListApprResult);
        }

        #endregion

        #region Self Assessment

        public ActionResult GridItemSelfAssass()
        {
            UserConf(ActionBehavior.EDIT);
            DataSelector();
            BSM.OnIndexLoadingSelfAssessment();
            return PartialView("GridItemSelfAssass", BSM.ListSelfAssessment);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateSelfAssass(HRApprSelfAssessment MModel)
        {
            UserSession();
            DataSelector();
            if (ModelState.IsValid)
            {
                var msg = BSM.OnGridModifySelfAssessment(MModel, SYActionBehavior.ADD.ToString());
                if (msg == SYConstant.OK)
                {
                    Session[Index_Sess_Obj + ActionName] = BSM;
                }
                else
                {
                    ViewData["EditError"] = SYMessages.getMessage(msg, user.Lang);
                }
            }
            else
            {
                ViewData["EditError"] = SYMessages.getMessage("EE001", user.Lang);
            }
            BSM.OnIndexLoadingSelfAssessment();
            return PartialView("GridItemSelfAssass", BSM.ListSelfAssessment);
        }
        //edit
        [HttpPost, ValidateInput(false)]
        public ActionResult EditSelfAssass(HRApprSelfAssessment MModel)
        {
            UserSession();
            DataSelector();
            if (ModelState.IsValid)
            {
                try
                {
                    var msg = BSM.OnGridModifySelfAssessment(MModel, SYActionBehavior.EDIT.ToString());
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
            BSM.OnIndexLoadingSelfAssessment();
            return PartialView("GridItemSelfAssass", BSM.ListSelfAssessment);
        }
        //delete
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteSelfAssass(HRApprSelfAssessment ModelObject)
        {
            UserSession();
            DataSelector();
            try
            {
                BSM = (IClsAppraisalMaster)Session[Index_Sess_Obj + ActionName];
                var msg = BSM.OnGridModifySelfAssessment(ModelObject, SYActionBehavior.DELETE.ToString());
                if (msg != SYConstant.OK)
                {
                    ViewData["EditError"] = SYMessages.getMessage(msg, user.Lang);
                }
            }
            catch (Exception e)
            {
                ViewData["EditError"] = e.Message;
            }
            BSM.OnIndexLoadingSelfAssessment();
            return PartialView("GridItemSelfAssass", BSM.ListSelfAssessment);
        }

        #endregion

        #region "Compare Ratio"
        public ActionResult GridComRatio()
        {
            UserSession();
            UserConf(ActionBehavior.EDIT);
            MDAppraisal BSM = new MDAppraisal();
            BSM.ListCompareRatio = DB.HRAppCompareRatios.ToList();
            return PartialView("GridCompareRatio", BSM.ListCompareRatio);
        }

        //create
        [HttpPost, ValidateInput(false)]
        public ActionResult CreateComRatio(HRAppCompareRatio MModel)
        {
            UserSession();
            UserConf(ActionBehavior.EDIT);
            MDAppraisal BSM = new MDAppraisal();
            if (ModelState.IsValid)
            {
                try
                {
                    var ListKPI = DB.HRAppCompareRatios.Where(w => (w.FromRatio >= MModel.FromRatio && w.FromRatio <= MModel.ToRatio) || (w.ToRatio >= MModel.FromRatio && w.ToRatio <= MModel.ToRatio)).ToList();
                    if (ListKPI.Count > 0)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("ISDUPLICATED");
                    }
                    else
                    {
                        DB.HRAppCompareRatios.Add(MModel);
                        int row = DB.SaveChanges();
                    }
                }
                catch (DbEntityValidationException e)
                {
                    /*------------------SaveLog----------------------------------*/
                    SYEventLog log = new SYEventLog();
                    log.ScreenId = SCREEN_ID;
                    log.UserId = user.UserName;
                    log.DocurmentAction = MModel.ID.ToString();
                    log.Action = SYActionBehavior.ADD.ToString();

                    SYEventLogObject.saveEventLog(log, e);
                    /*----------------------------------------------------------*/
                    ViewData["EditError"] = e.Message;
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

            BSM.ListCompareRatio = DB.HRAppCompareRatios.ToList();
            return PartialView("GridCompareRatio", BSM.ListCompareRatio);
        }
        //edit
        [HttpPost, ValidateInput(false)]
        public ActionResult EditComRatio(HRAppCompareRatio MModel)
        {
            UserSession();
            UserConf(ActionBehavior.EDIT);
            MDAppraisal BSM = new MDAppraisal();
            if (ModelState.IsValid)
            {
                try
                {
                    var objUpdate = DB.HRAppCompareRatios.FirstOrDefault(w => w.ID == MModel.ID);

                    if (objUpdate != null)
                    {
                        objUpdate.FromRatio = MModel.FromRatio;
                        objUpdate.ToRatio = MModel.ToRatio;
                        objUpdate.Factor = MModel.Factor;
                        DB.HRAppCompareRatios.Attach(objUpdate);
                        DB.Entry(objUpdate).State = System.Data.Entity.EntityState.Modified;
                        int row = DB.SaveChanges();
                    }
                    else
                    {
                        ViewData["EditError"] = SYMessages.getMessage("OBJ_NE");
                    }

                }
                catch (DbEntityValidationException e)
                {
                    /*------------------SaveLog----------------------------------*/
                    SYEventLog log = new SYEventLog();
                    log.ScreenId = SCREEN_ID;
                    log.UserId = user.UserName;
                    log.DocurmentAction = MModel.ID.ToString();
                    log.Action = SYActionBehavior.ADD.ToString();

                    SYEventLogObject.saveEventLog(log, e);
                    /*----------------------------------------------------------*/
                    ViewData["EditError"] = e.Message;
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


            BSM.ListCompareRatio = DB.HRAppCompareRatios.ToList();
            return PartialView("GridCompareRatio", BSM.ListCompareRatio);
        }
        //delete
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteComRatio(int ID)
        {
            UserSession();
            UserConf(ActionBehavior.EDIT);
            MDAppraisal BSM = new MDAppraisal();
            if (ID != 0)
            {
                try
                {
                    var obj = DB.HRAppCompareRatios.Where(w => w.ID == ID).ToList();
                    if (obj.Count > 0)
                    {
                        var objDel = obj.First();
                        DB.HRAppCompareRatios.Remove(objDel);
                        int row = DB.SaveChanges();
                    }

                }
                catch (DbEntityValidationException e)
                {
                    /*------------------SaveLog----------------------------------*/
                    SYEventLog log = new SYEventLog();
                    log.ScreenId = SCREEN_ID;
                    log.UserId = user.UserName;
                    log.DocurmentAction = ID.ToString();
                    log.Action = SYActionBehavior.ADD.ToString();

                    SYEventLogObject.saveEventLog(log, e);
                    /*----------------------------------------------------------*/
                    ViewData["EditError"] = e.Message;
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }

            BSM.ListCompareRatio = DB.HRAppCompareRatios.ToList();
            return PartialView("GridCompareRatio", BSM.ListCompareRatio);
        }
        #endregion

        #region "Performance Rate"
        public ActionResult GridPerfRate()
        {
            UserSession();
            UserConf(ActionBehavior.EDIT);
            MDAppraisal BSM = new MDAppraisal();
            BSM.ListPerformanceRate = DB.HRAppPerformanceRates.ToList();
            return PartialView("GridPerfRate", BSM.ListPerformanceRate);
        }

        //create
        [HttpPost, ValidateInput(false)]
        public ActionResult CreatePerfRate(HRAppPerformanceRate MModel)
        {
            UserSession();
            UserConf(ActionBehavior.EDIT);
            MDAppraisal BSM = new MDAppraisal();
            if (ModelState.IsValid)
            {
                try
                {
                    var ListKPI = DB.HRAppPerformanceRates.Where(w => (w.FromScore >= MModel.FromScore && w.FromScore <= MModel.ToScore)
                                    || (w.ToScore >= MModel.FromScore && w.ToScore <= MModel.ToScore)).ToList();
                    if (ListKPI.Count > 0)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("ISDUPLICATED");
                    }
                    else
                    {
                        DB.HRAppPerformanceRates.Add(MModel);
                        int row = DB.SaveChanges();
                    }
                }
                catch (DbEntityValidationException e)
                {
                    /*------------------SaveLog----------------------------------*/
                    SYEventLog log = new SYEventLog();
                    log.ScreenId = SCREEN_ID;
                    log.UserId = user.UserName;
                    log.DocurmentAction = MModel.ID.ToString();
                    log.Action = SYActionBehavior.ADD.ToString();

                    SYEventLogObject.saveEventLog(log, e);
                    /*----------------------------------------------------------*/
                    ViewData["EditError"] = e.Message;
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

            BSM.ListPerformanceRate = DB.HRAppPerformanceRates.ToList();
            return PartialView("GridPerfRate", BSM.ListPerformanceRate);
        }
        //edit
        [HttpPost, ValidateInput(false)]
        public ActionResult EditPerfRate(HRAppPerformanceRate MModel)
        {
            UserSession();
            UserConf(ActionBehavior.EDIT);
            MDAppraisal BSM = new MDAppraisal();
            if (ModelState.IsValid)
            {
                try
                {
                    var objUpdate = DB.HRAppPerformanceRates.FirstOrDefault(w => w.ID == MModel.ID);

                    if (objUpdate != null)
                    {
                        objUpdate.FromScore = MModel.FromScore;
                        objUpdate.ToScore = MModel.ToScore;
                        objUpdate.Rate = MModel.Rate;
                        objUpdate.Result = MModel.Result;
                        DB.HRAppPerformanceRates.Attach(objUpdate);
                        DB.Entry(objUpdate).State = System.Data.Entity.EntityState.Modified;
                        int row = DB.SaveChanges();
                    }
                    else
                    {
                        ViewData["EditError"] = SYMessages.getMessage("OBJ_NE");
                    }

                }
                catch (DbEntityValidationException e)
                {
                    /*------------------SaveLog----------------------------------*/
                    SYEventLog log = new SYEventLog();
                    log.ScreenId = SCREEN_ID;
                    log.UserId = user.UserName;
                    log.DocurmentAction = MModel.ID.ToString();
                    log.Action = SYActionBehavior.ADD.ToString();

                    SYEventLogObject.saveEventLog(log, e);
                    /*----------------------------------------------------------*/
                    ViewData["EditError"] = e.Message;
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


            BSM.ListPerformanceRate = DB.HRAppPerformanceRates.ToList();
            return PartialView("GridPerfRate", BSM.ListPerformanceRate);
        }
        //delete
        [HttpPost, ValidateInput(false)]
        public ActionResult DeletePerfRate(int ID)
        {
            UserSession();
            UserConf(ActionBehavior.EDIT);
            MDAppraisal BSM = new MDAppraisal();
            if (ID != 0)
            {
                try
                {
                    var obj = DB.HRAppPerformanceRates.Where(w => w.ID == ID).ToList();
                    if (obj.Count > 0)
                    {
                        var objDel = obj.First();
                        DB.HRAppPerformanceRates.Remove(objDel);
                        int row = DB.SaveChanges();
                    }

                }
                catch (DbEntityValidationException e)
                {
                    /*------------------SaveLog----------------------------------*/
                    SYEventLog log = new SYEventLog();
                    log.ScreenId = SCREEN_ID;
                    log.UserId = user.UserName;
                    log.DocurmentAction = ID.ToString();
                    log.Action = SYActionBehavior.ADD.ToString();

                    SYEventLogObject.saveEventLog(log, e);
                    /*----------------------------------------------------------*/
                    ViewData["EditError"] = e.Message;
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }

            BSM.ListPerformanceRate = DB.HRAppPerformanceRates.ToList();
            return PartialView("GridPerfRate", BSM.ListPerformanceRate);
        }
        #endregion

        #region "Leve MidPoint"
        public ActionResult GridMidPoint()
        {
            UserSession();
            DataSelector();
            UserConf(ActionBehavior.EDIT);
            MDAppraisal BSM = new MDAppraisal();
            BSM.ListLeveMidPoint = DB.HRAppLevelMidPoints.ToList();
            return PartialView("GridLevelMidPoint", BSM.ListLeveMidPoint);
        }

        //create
        [HttpPost, ValidateInput(false)]
        public ActionResult CreateMidPoint(HRAppLevelMidPoint MModel)
        {
            UserSession();
            UserConf(ActionBehavior.EDIT);
            MDAppraisal BSM = new MDAppraisal();
            if (ModelState.IsValid)
            {
                try
                {
                    //var ListKPI = DB.HRAppLevelMidPoints.Where(w => (w.FromRate >= MModel.FromRate && w.FromRate <= MModel.ToRate)
                    //                || (w.ToRate >= MModel.FromRate && w.ToRate <= MModel.ToRate)).ToList();
                    //if (ListKPI.Count > 0)
                    //{
                    //    ViewData["EditError"] = SYMessages.getMessage("ISDUPLICATED");
                    //}
                    //else
                    //{
                    //    DB.HRAppLevelMidPoints.Add(MModel);
                    //    int row = DB.SaveChanges();
                    //}
                    DB.HRAppLevelMidPoints.Add(MModel);
                    int row = DB.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    /*------------------SaveLog----------------------------------*/
                    SYEventLog log = new SYEventLog();
                    log.ScreenId = SCREEN_ID;
                    log.UserId = user.UserName;
                    log.DocurmentAction = MModel.ID.ToString();
                    log.Action = SYActionBehavior.ADD.ToString();

                    SYEventLogObject.saveEventLog(log, e);
                    /*----------------------------------------------------------*/
                    ViewData["EditError"] = e.Message;
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

            BSM.ListLeveMidPoint = DB.HRAppLevelMidPoints.ToList();
            return PartialView("GridLevelMidPoint", BSM.ListLeveMidPoint);
        }
        //edit
        [HttpPost, ValidateInput(false)]
        public ActionResult EditMidPoint(HRAppLevelMidPoint MModel)
        {
            UserSession();
            UserConf(ActionBehavior.EDIT);
            MDAppraisal BSM = new MDAppraisal();
            if (ModelState.IsValid)
            {
                try
                {
                    var objUpdate = DB.HRAppLevelMidPoints.FirstOrDefault(w => w.ID == MModel.ID);

                    if (objUpdate != null)
                    {
                        objUpdate.JobLevelMidPoint = MModel.JobLevelMidPoint;
                        DB.HRAppLevelMidPoints.Attach(objUpdate);
                        DB.Entry(objUpdate).State = System.Data.Entity.EntityState.Modified;
                        int row = DB.SaveChanges();
                    }
                    else
                    {
                        ViewData["EditError"] = SYMessages.getMessage("OBJ_NE");
                    }

                }
                catch (DbEntityValidationException e)
                {
                    /*------------------SaveLog----------------------------------*/
                    SYEventLog log = new SYEventLog();
                    log.ScreenId = SCREEN_ID;
                    log.UserId = user.UserName;
                    log.DocurmentAction = MModel.ID.ToString();
                    log.Action = SYActionBehavior.ADD.ToString();

                    SYEventLogObject.saveEventLog(log, e);
                    /*----------------------------------------------------------*/
                    ViewData["EditError"] = e.Message;
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


            BSM.ListLeveMidPoint = DB.HRAppLevelMidPoints.ToList();
            return PartialView("GridLevelMidPoint", BSM.ListLeveMidPoint);
        }
        //delete
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteMidPoint(int ID)
        {
            UserSession();
            UserConf(ActionBehavior.EDIT);
            MDAppraisal BSM = new MDAppraisal();
            if (ID != 0)
            {
                try
                {
                    var obj = DB.HRAppLevelMidPoints.Where(w => w.ID == ID).ToList();
                    if (obj.Count > 0)
                    {
                        var objDel = obj.First();
                        DB.HRAppLevelMidPoints.Remove(objDel);
                        int row = DB.SaveChanges();
                    }

                }
                catch (DbEntityValidationException e)
                {
                    /*------------------SaveLog----------------------------------*/
                    SYEventLog log = new SYEventLog();
                    log.ScreenId = SCREEN_ID;
                    log.UserId = user.UserName;
                    log.DocurmentAction = ID.ToString();
                    log.Action = SYActionBehavior.ADD.ToString();

                    SYEventLogObject.saveEventLog(log, e);
                    /*----------------------------------------------------------*/
                    ViewData["EditError"] = e.Message;
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }

            BSM.ListLeveMidPoint = DB.HRAppLevelMidPoints.ToList();
            return PartialView("GridLevelMidPoint", BSM.ListLeveMidPoint);
        }
        #endregion
        #region "Salary Budget"
        public ActionResult GridSalaryBudget()
        {
            UserSession();
            UserConf(ActionBehavior.EDIT);
            MDAppraisal BSM = new MDAppraisal();
            BSM.ListSalaryBudget = DB.HRAppSalaryBudgets.ToList();
            return PartialView("GridSalaryBudget", BSM.ListSalaryBudget);
        }

        //create
        [HttpPost, ValidateInput(false)]
        public ActionResult CreateSalaryBudget(HRAppSalaryBudget MModel)
        {
            UserSession();
            UserConf(ActionBehavior.EDIT);
            MDAppraisal BSM = new MDAppraisal();
            if (ModelState.IsValid)
            {
                try
                {
                    var ListKPI = DB.HRAppSalaryBudgets.Where(w => w.InYear == MModel.InYear).ToList();
                    if (ListKPI.Count > 0)
                    {
                        ViewData["EditError"] = SYMessages.getMessage("ISDUPLICATED");
                    }
                    else
                    {
                        DB.HRAppSalaryBudgets.Add(MModel);
                        int row = DB.SaveChanges();
                    }
                }
                catch (DbEntityValidationException e)
                {
                    /*------------------SaveLog----------------------------------*/
                    SYEventLog log = new SYEventLog();
                    log.ScreenId = SCREEN_ID;
                    log.UserId = user.UserName;
                    log.DocurmentAction = MModel.ID.ToString();
                    log.Action = SYActionBehavior.ADD.ToString();

                    SYEventLogObject.saveEventLog(log, e);
                    /*----------------------------------------------------------*/
                    ViewData["EditError"] = e.Message;
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

            BSM.ListSalaryBudget = DB.HRAppSalaryBudgets.ToList();
            return PartialView("GridSalaryBudget", BSM.ListSalaryBudget);
        }
        //edit
        [HttpPost, ValidateInput(false)]
        public ActionResult EditSalaryBudget(HRAppSalaryBudget MModel)
        {
            UserSession();
            UserConf(ActionBehavior.EDIT);
            MDAppraisal BSM = new MDAppraisal();
            if (ModelState.IsValid)
            {
                try
                {
                    var objUpdate = DB.HRAppSalaryBudgets.FirstOrDefault(w => w.ID == MModel.ID);

                    if (objUpdate != null)
                    {
                        objUpdate.Budget = MModel.Budget;
                        DB.HRAppSalaryBudgets.Attach(objUpdate);
                        DB.Entry(objUpdate).State = System.Data.Entity.EntityState.Modified;
                        int row = DB.SaveChanges();
                    }
                    else
                    {
                        ViewData["EditError"] = SYMessages.getMessage("OBJ_NE");
                    }

                }
                catch (DbEntityValidationException e)
                {
                    /*------------------SaveLog----------------------------------*/
                    SYEventLog log = new SYEventLog();
                    log.ScreenId = SCREEN_ID;
                    log.UserId = user.UserName;
                    log.DocurmentAction = MModel.ID.ToString();
                    log.Action = SYActionBehavior.ADD.ToString();

                    SYEventLogObject.saveEventLog(log, e);
                    /*----------------------------------------------------------*/
                    ViewData["EditError"] = e.Message;
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


            BSM.ListSalaryBudget = DB.HRAppSalaryBudgets.ToList();
            return PartialView("GridSalaryBudget", BSM.ListSalaryBudget);
        }
        //delete
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteSalaryBudget(int ID)
        {
            UserSession();
            UserConf(ActionBehavior.EDIT);
            MDAppraisal BSM = new MDAppraisal();
            if (ID != 0)
            {
                try
                {
                    var obj = DB.HRAppSalaryBudgets.Where(w => w.ID == ID).ToList();
                    if (obj.Count > 0)
                    {
                        var objDel = obj.First();
                        DB.HRAppSalaryBudgets.Remove(objDel);
                        int row = DB.SaveChanges();
                    }

                }
                catch (DbEntityValidationException e)
                {
                    /*------------------SaveLog----------------------------------*/
                    SYEventLog log = new SYEventLog();
                    log.ScreenId = SCREEN_ID;
                    log.UserId = user.UserName;
                    log.DocurmentAction = ID.ToString();
                    log.Action = SYActionBehavior.ADD.ToString();

                    SYEventLogObject.saveEventLog(log, e);
                    /*----------------------------------------------------------*/
                    ViewData["EditError"] = e.Message;
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }

            BSM.ListSalaryBudget = DB.HRAppSalaryBudgets.ToList();
            return PartialView("GridSalaryBudget", BSM.ListSalaryBudget);
        }
        #endregion
        protected void DataSelector()
        {
            foreach (var data in BSM.OnDataSelectorLoading())
            {
                ViewData[data.Key] = data.Value;
            }
        }
    }
}
