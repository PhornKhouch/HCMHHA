using Humica.Core.DB;
using Humica.Core.FT;
using Humica.EF.MD;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Hosting;

namespace Humica.Logic.MD
{
    public class MDOrgChart
    {
        public static HumicaDBViewContext DBV = new HumicaDBViewContext();
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public HR_OrgChart_View Header { get; set; }
        public string ScreenId { get; set; }
        public string CompanyCode { get; set; }
        public string MessageCode { get; set; }
        public bool IsInUse { get; set; }
        public bool IsEditable { get; set; }
        public List<HR_OrgChart_View> ListHeader { get; set; }
        public FTFilterEmployee Filter { get; set; }
        public List<NodeModel> ListOrg { get; set; }
        public MDOrgChart()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
        }
        public List<NodeModel> LoadDatas(string Branch,string Division, string Department,string URL)
        {
            DBV = new HumicaDBViewContext();
            ListOrg = new List<NodeModel>();
            List<HR_OrgChart_View> _list = DBV.HR_OrgChart_View.ToList();
            var ListStaff = DBV.HRStaffProfiles.Where(w => w.Status == "A").ToList();
            if(!string.IsNullOrEmpty(Branch))
            {
                ListStaff = ListStaff.Where(w => w.Branch == Branch).ToList();
            }
            if (!string.IsNullOrEmpty(Division))
            {
                ListStaff = ListStaff.Where(w => w.Division == Division).ToList();
            }
            if (!string.IsNullOrEmpty(Department))
            {
                ListStaff = ListStaff.Where(w => w.DEPT == Department).ToList();
            }
            _list = _list.Where(w => ListStaff.Where(x => x.EmpCode == w.EmpCode).Any()).ToList();
           string Imag= URL + "/Content/img/noimage.jpg";
            foreach (var item in _list)
            {
                string Image = Imag;
                if (!string.IsNullOrEmpty(item.Images))
                    Image = item.Images;
                //var obj = new NodeModel { id = item.EmpCode, name = item.Name, title = item.Designation,
                //    email = "", img = item.Images, pid = item.ReportingID,
                //Branch=item.Branch};
                var obj = new NodeModel
                {
                    id = item.EmpCode,
                    name = item.Name,
                    title = item.Designation,
                    email = "",
                    img = Image,
                    pid = item.ReportingID,
                    Branch = item.Branch
                };
                //if (item.EmpCode == "0051")
                //{
                //    obj.tags = "assistant";
                //    obj.pid = "0006";
                //}
                ListOrg.Add(obj);
            }
            return ListOrg;
        }
        public List<NodeModel> LoadDataCompany()
        {
            DBV = new HumicaDBViewContext();
            ListOrg = new List<NodeModel>();
            var _listView = new HumicaDBViewContext().HR_CompanyGroup_View.ToList();
            var _list = DBV.HRCompanyTrees.ToList();
            _list = _list.OrderBy(w => w.LevelGroup).ThenBy(w => w.Level).ToList();
            foreach (var item in _list)
            {
                string PID = item.ParentWorkGroupID;
                string tags = null;
                if (item.ParentWorkGroupID != null)
                    PID = _list.FirstOrDefault(w => w.CompanyMember == item.ParentWorkGroupID).ID.ToString();
                if (item.SubParent != null)
                    PID = _list.FirstOrDefault(w => w.CompanyMember == item.SubParent).ID.ToString();
                if (item.IsAssistant == true)
                {
                    tags = "assistant";
                }
                var obj = new NodeModel
                {
                    id = item.ID.ToString(),
                    name = item.WorkGroup,
                    title = item.CompanyMemberDesc,
                    pid = PID,
                    tags = tags

                };
                ListOrg.Add(obj);
            }
            return ListOrg;
        }
    }
    public class NodeModel
    {
        public string id { get; set; }
        public string pid { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        public string img { get; set; }
        public string email { get; set; }
        public string Branch { get; set; }
        public string Devision { get; set; }
        public string Department { get; set; }
        public string tags { get; set; }
        public List<NodeModel> ListChart { get; set; }
        public List<NodeModel> LoadDatas()
        {
            ListChart = new List<NodeModel>
            {
                new NodeModel { id = "1", name = "Jack Hill", title = "Chairman and CEO", email = "amber@domain.com", img = "https://cdn.balkan.app/shared/1.jpg" },
                new NodeModel { id = "2", pid = "1", name = "Lexie Cole", title = "QA Lead", email = "ava@domain.com", img = "https://cdn.balkan.app/shared/2.jpg" },
                new NodeModel { id = "3", pid = "1", name = "Janae Barrett", title = "Technical Director", img = "https://cdn.balkan.app/shared/3.jpg" },
                new NodeModel { id = "4", pid = "1", name = "Aaliyah Webb", title = "Manager", email = "jay@domain.com", img = "https://cdn.balkan.app/shared/4.jpg" },
                new NodeModel { id = "5", pid = "2", name = "Elliot Ross", title = "QA", img = "https://cdn.balkan.app/shared/5.jpg" },
                new NodeModel { id = "6", pid = "2", name = "Anahi Gordon", title = "QA", img = "https://cdn.balkan.app/shared/6.jpg" },
                new NodeModel { id = "7", pid = "2", name = "Knox Macias", title = "QA", img = "https://cdn.balkan.app/shared/7.jpg" },
                new NodeModel { id = "8", pid = "3", name = "Nash Ingram", title = ".NET Team Lead", email = "kohen@domain.com", img = "https://cdn.balkan.app/shared/8.jpg" },
                new NodeModel { id = "9", pid = "3", name = "Sage Barnett", title = "JS Team Lead", img = "https://cdn.balkan.app/shared/9.jpg" },
                new NodeModel { id = "10", pid = "8", name = "Alice Gray", title = "Programmer", img = "https://cdn.balkan.app/shared/10.jpg" },
                new NodeModel { id = "11", pid = "8", name = "Anne Ewing", title = "Programmer", img = "https://cdn.balkan.app/shared/11.jpg" },
                new NodeModel { id = "12", pid = "9", name = "Reuben Mcleod", title = "Programmer", img = "https://cdn.balkan.app/shared/12.jpg" },
                new NodeModel { id = "13", pid = "9", name = "Ariel Wiley", title = "Programmer", img = "https://cdn.balkan.app/shared/13.jpg" },
                new NodeModel { id = "14", pid = "4", name = "Lucas West", title = "Marketer", img = "https://cdn.balkan.app/shared/14.jpg" },
                new NodeModel { id = "15", pid = "4", name = "Adan Travis", title = "Designer", img = "https://cdn.balkan.app/shared/15.jpg" },
                new NodeModel { id = "16", pid = "4", name = "Alex Snider", title = "Sales Manager", img = "https://cdn.balkan.app/shared/16.jpg" }
            };
            return ListChart;
        }
    }
}