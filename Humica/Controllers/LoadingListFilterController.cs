using DevExpress.Web;
using DevExpress.Web.Mvc;
using Humica.Core.DB;
using Humica.EF.Models.SY;
using Humica.EF.Repo;
using Humica.Logic;
using Humica.Models.SY;
using Humica.Performance;
using System.Web.Mvc;

namespace Humica.Controllers
{

    public class LoadingListFilterController : LoadingListController
    {

        private IUnitOfWork unitOfWork;
        public void OnLoad()
        {
            unitOfWork = new UnitOfWork<HumicaDBContext>(new HumicaDBContext());
        }
        public LoadingListFilterController()
            : base()
        {
            OnLoad();
        }
        public ActionResult KPIList(string Department)
        {
            UserSession();
            return GridViewExtension.GetComboBoxCallbackResult(cboProperties =>
            {
                cboProperties.CallbackRouteValues = new { Controller = "LoadingListFilter", Action = "KPIList" };
                cboProperties.ClientInstanceName = "SelectKPI_LIST";
                cboProperties.TextField = "Description";
                cboProperties.ValueField = "Code";

                cboProperties.TextFormatString = ClsConstant.TEXT_COMBOBOX_STRING;
                cboProperties.DisplayFormatString = ClsConstant.TEXT_COMBOBOX_STRING_SINGLE;
                cboProperties.Columns.Add("Code", Humica.EF.Models.SY.SYSettings.getLabel("Code"), 90);
                cboProperties.Columns.Add("Description", Humica.EF.Models.SY.SYSettings.getLabel("Description"), 300);
                cboProperties.ClientSideEvents.SelectedIndexChanged = "cboItemChange";

                cboProperties.IncrementalFilteringMode = IncrementalFilteringMode.Contains;

                cboProperties.BindList(CLsKPIAssign.GetKPIList());

            });

        }
        public ActionResult PartialEmployeeByHODSearch(string screenid)
        {
            UserSession();
            UserConfListAndForm();
            ClsFilter clsFilter = new ClsFilter();
            ViewData["STAFF_SELECT_HOD"] = clsFilter.OnLoadStaffByHOD();
            ViewData[SYConstant.SCREEN_ID] = screenid;
            return PartialView(SYListFilter.ListFilterViewEmpByHOD);
        }
    }
}
