using Humica.Core.DB;
using Humica.Core.FT;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.Logic;
using Humica.Logic.Atts;
using Humica.Logic.PR;
using Humica.Models.SY;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Humica.Controllers.PR.PRM
{

    public class GenerateSalaryController : Humica.EF.Controllers.MasterSaleController
    {
        private static string Error = "";
        private const string SCREEN_ID = "PRM0000006";
        private const string URL_SCREEN = "/PR/PRM/GenerateSalary/";
        private string Index_Sess_Obj = SYSConstant.BUSINESS_OBJECT_MODEL + SCREEN_ID;
        private string ActionName = "";
        private string KeyName = "EmpCode";
        HumicaDBContext DB = new HumicaDBContext();
        SMSystemEntity SM = new SMSystemEntity();

        public GenerateSalaryController()
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

            PRGenerate_Salary BSM = new PRGenerate_Salary();
            BSM.ListEmployeeGen = new List<HR_View_EmpGenSalary>();
            BSM.Filter = new FTFilterEmployee();
            BSM.Filter.InMonth = DateTime.Now;
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                var obj = (PRGenerate_Salary)Session[Index_Sess_Obj + ActionName];
                BSM.Filter = obj.Filter;
            }
            Session[Index_Sess_Obj + ActionName] = BSM;

            return View(BSM);
        }
        [HttpPost]
        public ActionResult Index(PRGenerate_Salary BSM)
        {
            ActionName = "Index";
            UserSession();
            DataSelector();
            UserConfListAndForm(this.KeyName);
            BSM.ListEmployeeGen = BSM.LoadDataEmpGen(BSM.Filter, SYConstant.getBranchDataAccess());
            BSM.Progress = BSM.ListEmployeeGen.Count();
            Session[Index_Sess_Obj + ActionName] = BSM;

            return View(BSM);
        }

        #endregion
        public ActionResult Generate()
        {
            ActionName = "Index";
            UserSession();
            UserConfForm(SYActionBehavior.EDIT);
            DataSelector();

            var BSM = new PRGenerate_Salary();
            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (PRGenerate_Salary)Session[Index_Sess_Obj + ActionName];
            }
            var ListWaiting = SM.SYWaitingBackgrounds.Where(w => w.UserName == user.UserName && w.ScreenId == SCREEN_ID && w.IsFinish == false).ToList();
            if (ListWaiting.Count() == 0)
            {
                string id = BSM.EmpID;
                HostingEnvironment.QueueBackgroundWorkItem(cancellationToken => this.ReleaseDocWaiting(id, BSM.Filter, cancellationToken));
                int len = id.Split(';').Length;
                string actionDesc = SYActionObject.getTitleScreen(SCREEN_ID);

                InsertWaitingProcess(len, actionDesc, SYMessages.getMessage("PROCESSING"), id);

                Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("WAITING_PRO", user.Lang);
            }
            else
            {
                var ListWaitingItem = SM.SYWaitingBackgroundItems.Where(w => w.UserName == user.UserName && w.ScreenId == SCREEN_ID && w.IsFinish == false).ToList();
                if (ListWaiting.Count() > 0)
                {
                    string id = ListWaitingItem.First().DocumentReference;
                    HostingEnvironment.QueueBackgroundWorkItem(cancellationToken => this.ReleaseDocWaiting(id, BSM.Filter, cancellationToken));
                }
                //Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("WAITING_FOR_PRO", user.Lang);
            }
            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        public ActionResult Delete(DateTime INMonth)
        {
            UserSession();
            UserConfForm(SYActionBehavior.DELETE);
            DataSelector();
            PRGenerate_Salary GLA = new PRGenerate_Salary();
            string msg = GLA.Delete_GenerateAll(INMonth, SYConstant.getBranchDataAccess());
            if (msg == SYConstant.OK)
            {
                Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject("DOC_RM", user.Lang);
            }
            else
            {
                Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(msg, user.Lang);
            }

            return Redirect(SYUrl.getBaseUrl() + URL_SCREEN);
        }
        public ActionResult GridItems()
        {
            ActionName = "Index";
            UserSession();
            DataSelector();
            UserConfForm(ActionBehavior.ACC_REV);
            PRGenerate_Salary BSM = new PRGenerate_Salary();
            BSM.ScreenId = SCREEN_ID;

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (PRGenerate_Salary)Session[Index_Sess_Obj + ActionName];
            }
            Session[Index_Sess_Obj + ActionName] = BSM;
            Session[SYConstant.CURRENT_URL] = URL_SCREEN + "GridItems";
            return PartialView("GridItems", BSM.ListEmployeeGen);
        }
        [HttpPost]
        public string getEmpCode(string EmpCode, string Action)
        {
            this.ActionName = Action;
            UserSession();
            UserConfListAndForm();

            PRGenerate_Salary BSM = new PRGenerate_Salary();

            if (Session[Index_Sess_Obj + ActionName] != null)
            {
                BSM = (PRGenerate_Salary)Session[Index_Sess_Obj + ActionName];

                BSM.EmpID = EmpCode;

                Session[Index_Sess_Obj + ActionName] = BSM;
                return SYConstant.OK;
            }
            else
            {
                return SYMessages.getMessage("PLEASE_SEARCH_ALLOWANCE");
            }
        }
        #region Process
        public void ReleaseDocWaiting(string EmpID, FTFilterEmployee _filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                UserSession();
                PRGenerate_Salary param = new PRGenerate_Salary();
                param.Filter = _filter;
                string userName = user.UserName;
                param.User = user;
                param.BS = bs;
                Error = "";
                param.ScreenId = SCREEN_ID;
                if (PRGenerate_Salary.ListProgress == null)
                {
                    PRGenerate_Salary.ListProgress = new List<ListProgress>();
                }
                if (EmpID != null)
                {
                    if (PRGenerate_Salary.ListProgress.Where(w => w.UserName == user.UserName).ToList().Count() == 0)
                    {
                        PRGenerate_Salary.ListProgress.Add(new ListProgress { UserName = user.UserName, Progress = 0, Percen = 0 });
                    }
                    else
                    {
                        PRGenerate_Salary.ListProgress.Where(w => w.UserName == user.UserName).ToList().ForEach(x => x.Percen = 0);
                    }
                    string msg = param.GenerateSalarys(EmpID);
                    if (msg != SYConstant.OK)
                    {
                        Error = msg;
                        ProcessFail(SYMessages.getMessage(msg));
                    }
                    else
                    {
                        Error = "GENERATER_COMPLATED";
                        ProcessDone(SYMessages.getMessage("GENERATER_COMPLATED"));
                    }
                }
                else
                {
                    ProcessFail(SYMessages.getMessage("DOC_ERQ"));
                }
            }
            catch (Exception e)
            {
                Thread.Sleep(10000);
                ProcessFail();
                /*------------------SaveLog----------------------------------*/
                SYEventLog log = new SYEventLog();
                log.ScreenId = ScreendIDControl;
                log.UserId = user.UserID.ToString();
                log.DocurmentAction = "MASTER_QUOTA";
                log.Action = SYActionBehavior.EDIT.ToString();

                SYEventLogObject.saveEventLog(log, e, true);

                /*----------------------------------------------------------*/
            }
        }

        public ActionResult ShowProcess()
        {
            decimal P = 0;
            UserSession();
            if (PRGenerate_Salary.ListProgress != null)
            {
                P = PRGenerate_Salary.ListProgress.Where(w => w.UserName == user.UserName).Sum(x => x.Percen);
            }
            var ListWaiting = SM.SYWaitingBackgrounds.Where(w => w.UserName == user.UserName && w.ScreenId == SCREEN_ID && w.IsFinish == false).ToList();
            if (ListWaiting.Count() == 0)
            {
                if (Error.Length > 0)
                {
                    Session[SYSConstant.MESSAGE_SUBMIT] = SYMessages.getMessageObject(Error, user.Lang);
                }
                var result1 = new
                {
                    MS = SYConstant.FAIL,
                    Percen = P
                };
                return Json(result1, JsonRequestBehavior.AllowGet);
            }
            var result = new
            {
                MS = SYConstant.OK,
                Percen = P
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion
        private void DataSelector()
        {
            ViewData["DEPARTMENT_SELECT"] = ClsFilter.LoadDepartment();
            ViewData["BRANCHES_SELECT"] = SYConstant.getBranchDataAccess();
            ViewData["SECTION_SELECT"] = ClsFilter.LoadSection();
            ViewData["POSITION_SELECT"] = ClsFilter.LoadPosition();
            ViewData["DIVISION_SELECT"] = ClsFilter.LoadDivision();
            ViewData["CareerHistories_SELECT"] = DB.HRCareerHistories.ToList();
        }
    }
}
