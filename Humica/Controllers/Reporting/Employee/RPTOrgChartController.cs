using Humica.Core.DB;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic;
using Humica.Logic.MD;
using System.Linq;
using System.Web.Mvc;

namespace Humica.Controllers.Reporting
{
    public class RPTOrgChartController : Humica.EF.Controllers.MasterSaleController
    {
        private const string SCREEN_ID = "RPTOR00001";
        private const string URL_SCREEN = "/Reporting/RPTOrgChart/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string Index_Sess_Report = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName = "";
        private string KeyName = "EmpCode";
        HumicaDBContext DB = new HumicaDBContext();
        SMSystemEntity SMS = new SMSystemEntity();
        public RPTOrgChartController()
            : base()
        {
            this.ScreendIDControl = SCREEN_ID;
            this.ScreenUrl = URL_SCREEN;
        }
        public ActionResult Index()
        {
            ActionName = "Index";
            UserSession();
            UserConfListAndForm(this.KeyName);
            DataSelector();
            return View("Index");
        }
        [HttpPost]
        public ActionResult GetData(string Branch, string Division, string Department)
        {
            var URL = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');
            // Retrieve the data from your data source
            //NodeModel _nodes = new NodeModel();
            //var datava = _nodes.LoadDatas();
            MDOrgChart _nodes = new MDOrgChart();
            var datava = _nodes.LoadDatas(Branch, Division, Department, URL);
            var data = new
            {
                MS = "OK",
                DT = datava,
            };

            return Json(data, (JsonRequestBehavior)1);
        }
        private void DataSelector()
        {
            ViewData["DIVISION_SELECT"] = ClsFilter.LoadDivision();
            ViewData["DEPARTMENT_SELECT"] = ClsFilter.LoadDepartment();
            ViewData["COMPANY_SELECT"] = SYConstant.getCompanyDataAccess();
            //ViewData["POSITION_SELECT"] = ClsFilter.LoadPosition();
            ViewData["BRANCHES_SELECT"] = SYConstant.getBranchDataAccess();// DH.HRBranches.ToList();
        }
    }
}
