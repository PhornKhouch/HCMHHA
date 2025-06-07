using Humica.Core.DB;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic.Asset;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.Asset
{
    public class HRAssetTypeController : Humica.EF.Controllers.MasterSaleController

    {
        private const string SCREEN_ID = "AM00000003";
        private const string URL_SCREEN = "/Asset/HRAssetType";
        SMSystemEntity SMS = new SMSystemEntity();
        HumicaDBContext DB = new HumicaDBContext();
        public HRAssetTypeController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
        }
        #region 'List'
        public ActionResult Index()
        {
            UserSession();
            UserConfListAndForm();
            DataSelector();
            HRAssetObject BSM = new HRAssetObject();
            BSM.ListAssetClass = DB.HRAssetClasses.ToList();
            BSM.ListAssetType = DB.HRAssetTypes.ToList();
            return View(BSM);
        }
        #endregion
        #region 'GridItemAssetType'
        public ActionResult GridItemAssetType()
        {
            UserConf(ActionBehavior.EDIT);

            HRAssetObject BSM = new HRAssetObject();
            BSM.ListAssetType = DB.HRAssetTypes.ToList();
            return PartialView("GridItemAssetType", BSM.ListAssetType);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateAssType(HRAssetType MModel)
        {
            UserSession();
            HRAssetObject BSM = new HRAssetObject();
            if (ModelState.IsValid)
            {
                try
                {
                    MModel.AssetTypeCode = MModel.AssetTypeCode.ToUpper();
                    DB.HRAssetTypes.Add(MModel);
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
            BSM.ListAssetType = DB.HRAssetTypes.ToList();
            return PartialView("GridItemAssetType", BSM.ListAssetType);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult EditAssType(HRAssetType MModel)
        {
            UserSession();
            HRAssetObject BSM = new HRAssetObject();
            if (ModelState.IsValid)
            {
                try
                {
                    DB = new HumicaDBContext();
                    var ObjMatch = DB.HRAssetTypes.FirstOrDefault(w => w.AssetTypeCode == MModel.AssetTypeCode);
                    if (ObjMatch != null)
                    {
                        ObjMatch.Description = MModel.Description;

                        DB.HRAssetTypes.Attach(ObjMatch);
                        DB.Entry(ObjMatch).Property(x => x.Description).IsModified = true;
                        DB.SaveChanges();
                    }
                    else
                    {
                        ViewData["EditError"] = SYMessages.getMessage("EE001", user.Lang);
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
            BSM.ListAssetType = DB.HRAssetTypes.ToList();
            return PartialView("GridItemAssetType", BSM.ListAssetType);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteAssType(string AssetTypeCode)
        {
            UserSession();
            HRAssetObject BSM = new HRAssetObject();
            if (AssetTypeCode != null)
            {
                try
                {
                    var obj = DB.HRAssetTypes.Find(AssetTypeCode);
                    if (obj != null)
                    {
                        DB.HRAssetTypes.Remove(obj);
                        DB.SaveChanges();
                    }
                    BSM.ListAssetType = DB.HRAssetTypes.ToList();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            return PartialView("GridItemAssetType", BSM.ListAssetType);
        }
        #endregion

        #region 'GridItemAssetClass'
        public ActionResult GridItemAssetClass()
        {
            UserConf(ActionBehavior.EDIT);
            DataSelector();
            HRAssetObject BSM = new HRAssetObject();
            BSM.ListAssetClass = DB.HRAssetClasses.ToList();
            return PartialView("GridItemAssetClass", BSM.ListAssetClass);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateAssClass(HRAssetClass MModel)
        {
            UserSession();
            DataSelector();
            HRAssetObject BSM = new HRAssetObject();
            if (ModelState.IsValid)
            {
                try
                {
                    MModel.AssetClassCode = MModel.AssetClassCode.ToUpper();
                    DB.HRAssetClasses.Add(MModel);
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
            BSM.ListAssetClass = DB.HRAssetClasses.ToList();
            return PartialView("GridItemAssetClass", BSM.ListAssetClass);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult EditAssClass(HRAssetClass MModel)
        {
            UserSession();
            DataSelector();
            HRAssetObject BSM = new HRAssetObject();
            if (ModelState.IsValid)
            {
                try
                {
                    DB = new HumicaDBContext();
                    var ObjMatch = DB.HRAssetClasses.FirstOrDefault(w => w.AssetClassCode == MModel.AssetClassCode);
                    if (ObjMatch != null)
                    {
                        ObjMatch.NumberRank = MModel.NumberRank;
                        ObjMatch.Description = MModel.Description;
                        ObjMatch.AssetTypeCode = MModel.AssetTypeCode;
                        ObjMatch.Remark = MModel.Remark;
                        DB.HRAssetClasses.Attach(ObjMatch);
                        DB.Entry(ObjMatch).Property(x => x.Description).IsModified = true;
                        DB.Entry(ObjMatch).Property(x => x.AssetTypeCode).IsModified = true;
                        DB.Entry(ObjMatch).Property(x => x.NumberRank).IsModified = true;
                        DB.Entry(ObjMatch).Property(x => x.Remark).IsModified = true;
                        DB.SaveChanges();
                    }
                    else
                    {
                        ViewData["EditError"] = SYMessages.getMessage("EE001", user.Lang);
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
            BSM.ListAssetClass = DB.HRAssetClasses.ToList();
            return PartialView("GridItemAssetClass", BSM.ListAssetClass);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteAssClass(string AssetClassCode)
        {
            UserSession();
            HRAssetObject BSM = new HRAssetObject();
            DataSelector();
            if (AssetClassCode != null)
            {
                try
                {
                    var obj = DB.HRAssetClasses.Find(AssetClassCode);
                    if (obj != null)
                    {
                        DB.HRAssetClasses.Remove(obj);
                        DB.SaveChanges();
                    }
                    BSM.ListAssetClass = DB.HRAssetClasses.ToList();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            return PartialView("GridItemAssetClass", BSM.ListAssetClass);
        }
        #endregion

        #region 'Private Code'
        private void DataSelector()
        {
            ViewData["ASSETTYPE_SELECT"] = DB.HRAssetTypes.ToList();
            ViewData["NUMBERING_SELECT"] = SMS.BSNumberRanks.ToList();
        }
        #endregion
    }
}
