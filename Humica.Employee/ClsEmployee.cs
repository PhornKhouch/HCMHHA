﻿using Humica.Core;
using Humica.Core.CF;
using Humica.Core.DB;
using Humica.Core.FT;
using Humica.Core.SY;
using Humica.EF;
using Humica.EF.MD;
using Humica.EF.Models.SY;
using Humica.EF.Repo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Web;
using System.Web.Hosting;
using System.Xml.Linq;

namespace Humica.Employee
{
    public class ClsEmployee : IClsEmployee
    {
        protected IUnitOfWork unitOfWork;
        public string ScreenId { get; set; }
        public string MessageError { get; set; }
        public string Salary { get; set; }
        public bool IsEmpAuto { get; set; }
        public bool Fist_Last { get; set; }
        public bool Fist_Last_KH { get; set; }
        public SYUser User { get; set; }
        public SYUserBusiness BS { get; set; }
        public FTFilterEmployee Filter { get; set; }
        public HRStaffProfile Header { get; set; }
        public HREmpCareer HeaderCareer { get; set; }
        public List<HR_STAFF_VIEW> ListStaffView { get; set; }
        public List<HRStaffProfile> ListStaffProfile { get; set; }
        public List<HREmpIdentity> ListEmpIdentity { get; set; }
        public List<HREmpIdentity> ListEmpIdentityUP { get; set; }
        public List<HREmpJobDescription> ListEmpJobDescription { get; set; }
        public List<HREmpFamily> ListEmpFamily { get; set; }
		public List<HREmpEduc> ListEducation { get; set; }
		public List<HREmpContract> ListContract { get; set; }
		public List<HRDelayProbation> ListDelayProbation { get; set; }
		public List<HREmpDisciplinary> ListEmpDisciplinary { get; set; }
		public List<HREmpCertificate> ListEmpCertificate { get; set; }
		public List<HREmpPhischk> ListEmpMedical { get; set; }
		public List<MDUploadTemplate> ListTemplate { get; set; }

        public void OnLoad()
        {
            unitOfWork = new UnitOfWork<HumicaDBViewContext>();
        }
        public ClsEmployee()
        {
            User = SYSession.getSessionUser();
            BS = SYSession.getSessionUserBS();
            OnLoad();
        }
        public void OnLoandindEmployee()
        {
            if (Filter == null)
            {
                Filter = new FTFilterEmployee();
                Filter.Status = SYDocumentStatus.A.ToString();
            }
            DateTime CurrentDate = DateTime.Now;
            ListStaffView = new List<HR_STAFF_VIEW>();

            List<HRCompany> _ListCompany = SYConstant.getCompanyDataAccess();
            List<HRBranch> _ListBranch = SYConstant.getBranchDataAccess();
            List<HRLevel> _ListLevel = SYConstant.getLevelDataAccess();
            var branchid = _ListBranch.Select(s => s.Code).ToList();
            var companyCodes = _ListCompany.Select(x => x.Company).ToList();
            var levelCodes = _ListLevel.Select(x => x.Code).ToList();
            ListStaffView = unitOfWork.Set<HR_STAFF_VIEW>()
                           .Where(w => companyCodes.Contains(w.CompanyCode)
                           && levelCodes.Contains(w.LevelCode)
                           && branchid.Contains(w.BranchID)
                           && (string.IsNullOrEmpty(Filter.Office) || w.Office == Filter.Office)
                           && (string.IsNullOrEmpty(Filter.Department) || w.DEPT == Filter.Department)
                           && (string.IsNullOrEmpty(Filter.BusinessUnit) || w.GroupDept == Filter.BusinessUnit)
                           && (string.IsNullOrEmpty(Filter.Team) || w.Groups == Filter.Team)
                           )
                           .ToList();
            if (Filter.StartDate != null)
            {
                if (Filter.Status == "A")
                    ListStaffView = ListStaffView.Where(w => (w.StatusCode == Filter.Status
                                    && w.StartDate.Value.Month == Filter.StartDate.Value.Month
                                    && w.StartDate.Value.Year == Filter.StartDate.Value.Year)
                                    || (w.StatusCode == "I" && w.DateTerminate > CurrentDate)).ToList();
                else if (Filter.Status == "I")
                    ListStaffView = ListStaffView.Where(w => w.StatusCode == Filter.Status
                                && w.DateTerminate.Value.AddDays(-1).Month == Filter.StartDate.Value.Month
                                && w.DateTerminate.Value.AddDays(-1).Year == Filter.StartDate.Value.Year
                                && w.DateTerminate <= CurrentDate).ToList();
                if (Filter.Status == "I/A")
                {
                    ListStaffView = ListStaffView.Where(w => (w.StatusCode == "A" || (w.StatusCode == "I" && w.DateTerminate > CurrentDate)) || (w.DateTerminate.Value.AddDays(-1).Month == Filter.StartDate.Value.Month
                                                   && w.DateTerminate.Value.AddDays(-1).Year == Filter.StartDate.Value.Year)).ToList();
                }
            }
            else
            {
                if (Filter.Status == "A")
                    ListStaffView = ListStaffView.Where(w => w.StatusCode == Filter.Status || (w.StatusCode == "I" && w.DateTerminate > CurrentDate)).ToList();
                if (Filter.Status == "I")
                    ListStaffView = ListStaffView.Where(w => w.StatusCode == Filter.Status && w.DateTerminate <= CurrentDate).ToList();
            }
            if (!string.IsNullOrEmpty(Filter.Branch))
            {
                var branchFilter = Filter.Branch.Split(',').Select(b => b.Trim()).Where(b => !string.IsNullOrEmpty(b)).ToList();
                ListStaffView = ListStaffView.Where(w => branchFilter.Contains(w.BranchID)).ToList();
            }

        }
        public void OnCreatingLoading(params object[] keys)
        {
            DateTime DateNow = DateTime.Now;
            Header = new HRStaffProfile();
            ListEmpIdentity = new List<HREmpIdentity>();
            ListEmpJobDescription = new List<HREmpJobDescription>();
            ListEmpFamily = new List<HREmpFamily>();
			ListEducation = new List<HREmpEduc>();
			ListContract = new List<HREmpContract>();
			ListDelayProbation = new List<HRDelayProbation>();
			ListEmpDisciplinary = new List<HREmpDisciplinary>();
			ListEmpCertificate = new List<HREmpCertificate>();
			ListEmpMedical = new List<HREmpPhischk>();
			Header.StartDate = DateNow;
            Header.Probation = DateNow;
            Header.DOB = DateNow;
            Header.LeaveConf = DateNow;
            Header.CareerDesc = "NEWJOIN";
            Header.BankName = "CC";
            Header.IsCalSalary = true;
            Header.IsResident = true;
            Header.IsOTApproval = false;
            Header.EmpType = "LOCAL";
            Header.IsHold = false;
            Header.Nation = "CAM";
            Header.IsAtten = false;
            Header.Sex = "M";
            Header.StaffType = "OS";
            Header.IsPayPartial = true;
            Header.TemLeave = "LB";
            Header.SalaryType = SYSalaryType.Monthly.ToString();
			Header.IsExceptScan = false;
			Header.ReSalary = DateTime.Now;

            Fist_Last = false;
            Fist_Last_KH = false;
            var FIRST_LASTNAME = SYSettings.getSetting("FIRST_LASTNAME");
            var FIRST_LASTNAME_KH = SYSettings.getSetting("FIRST_LASTNAME_KH");
            if (FIRST_LASTNAME != null)
            {
                if (FIRST_LASTNAME.SettinValue == "TRUE") Fist_Last = true;
            }
            if (FIRST_LASTNAME_KH != null)
            {
                if (FIRST_LASTNAME_KH.SettinValue == "TRUE") Fist_Last_KH = true;
            }
            var Is_EmpAuto = SYSettings.getSetting("EMPCODE").SettinValue;
            if (Is_EmpAuto == "YES") IsEmpAuto = true;
            if (User.IsSalary == true)
            {
                Salary = "0";
            }
            else
            {
                Salary = "#####";
            }
            var TemLeave = unitOfWork.Set<HRSetEntitleH>().FirstOrDefault();
            if (TemLeave != null)
            {
                Header.TemLeave = TemLeave.Code;
            }

        }
        public string CreatStaff()
        {
            OnLoad();
            try
            {
                if (Header.EmpType == null)
                    return "EMPTYPE_EN";
                if (Header.Branch == null)
                    return "BRANCH_EN";
                if (Header.DEPT == null)
                    return "DEPARTMENT_EN";
                if (Header.JobCode == null)
                    return "POSITION_EN";
                if (Header.PayParam == null)
                    return "PAYPARAM_EN";
                if (Header.TXPayType == null)
                    return "SALRY_PAID_EN";
                if (Header.LevelCode == null)
                    return "LEVEL_EN";
                if (Header.ProbationType == null)
                    return "PROBATION_TYPE_EN";
                if (Header.Nation == null)
                    return "Nation_EN";
                if (Header.SalaryType == null)
                {
                    return "SALARY_TYPE_EN";
                }
                if (IsEmpAuto == true)
                {
                    if (Header.EmpCode == "" || Header.EmpCode is null) return "EMPLOYEE_NE";
                }
                else
                {
                    var DocConf = unitOfWork.Set<HREmpCode>().ToList();
                    if (!string.IsNullOrEmpty(Header.CompanyCode))
                        DocConf = DocConf.Where(w => w.Company == Header.CompanyCode || string.IsNullOrEmpty(w.Company)).ToList();
                    if (!string.IsNullOrEmpty(Header.Branch))
                        DocConf = DocConf.Where(w => w.Branch == Header.Branch || string.IsNullOrEmpty(w.Branch)).ToList();
                    if (!string.IsNullOrEmpty(Header.GroupDept))
                        DocConf = DocConf.Where(w => w.BU == Header.GroupDept || string.IsNullOrEmpty(w.BU)).ToList();
                    if (!DocConf.Any())
                    {
                        return "NUMBER_RANK_NE";
                    }
                    var objNumber = new CFNumberRank(DocConf.FirstOrDefault().NumberRank, Header.CompanyCode, Header.StartDate.Value.Year, true);
                    if (objNumber.NextNumberRank == null)
                    {
                        return "NUMBER_RANK_NE";
                    }
                    Header.EmpCode = objNumber.NextNumberRank.Trim();
                }
                Header.EmpCode = Header.EmpCode.Trim().ToUpper();
                var EmpD = unitOfWork.Set<HRStaffProfile>().Where(w => w.EmpCode == Header.EmpCode).ToList();
                if (EmpD.Count() > 0)
                {
                    return "DUPL_KEY";
                }
                bool IsSalary = IsHideSalary(Header.LevelCode);
                if (IsSalary == true) Header.Salary = Convert.ToDecimal(Salary);
                else Header.Salary = 0;
                if (Header.FirstLine == Header.FirstLine2 && !string.IsNullOrEmpty(Header.FirstLine))
                {
                    return "FIRSTLINE_EN";
                }
                if (Header.SecondLine == Header.SecondLine2 && !string.IsNullOrEmpty(Header.SecondLine))
                {
                    return "SECONDLINE_EN";
                }
                if (Header.LeaveConf == null) Header.LeaveConf = Header.StartDate;
                Header.EffectDate = Header.StartDate;
                Header.Attachment = Header.Attachment;
                Header.GPSData = Header.GPSData;
                Header.AttachFile = Header.AttachFile;
                Header.CreatedOn = DateTime.Now.Date;
                Header.CreatedBy = User.UserName;
                Header.DateTerminate = new DateTime(1900, 1, 1);
                Header.Status = SYDocumentStatus.A.ToString();
                if (string.IsNullOrEmpty(Header.Currency)) Header.Currency = "USD";
                HeaderCareer = new HREmpCareer();
                SwapStaffCarrer(HeaderCareer, Header);
                ClsCopyFile clsCopyFile = new ClsCopyFile();
                int LinItem = 0;
                string Attachment = "";
                var listInden = unitOfWork.Set<HRIdentityType>().ToList();
                foreach (var item in ListEmpIdentity)
                {
                    Attachment = item.Attachment;
                    LinItem += 1;
                    var empIden = new HREmpIdentity();
                    SwapEmpIdentity(empIden, item, Header);
                    var DescInd = listInden.FirstOrDefault(w => w.Code == item.IdentityTye);
                    if (DescInd != null)
                    {
                        string DocumentType = DescInd.Description;
                        var strpath = clsCopyFile.CopyStructurePath(Header.CompanyCode, DocumentType, Header.EmpCode, Header.DEPT, Attachment);
                        if(strpath== "No_Path")
                        {
                            return "File_Exit";
                        }
                        else
                        {
                            empIden.Attachment = strpath;
                        }
                    }
                    empIden.LineItem = LinItem;
                    unitOfWork.Add(empIden);
                }
				#region Save Emp Family
				LinItem = 0;
                Attachment = "";
                foreach (var item in ListEmpFamily)
                {
                    Attachment = item.AttachFile;
                    LinItem += 1;
                    var emp = new HREmpFamily();
                    SwapEmpFamily(emp, item, Header);
                    if (!string.IsNullOrEmpty(Attachment))
                    {
                        var strpath = clsCopyFile.CopyStructurePath(Header.CompanyCode, "Family", Header.EmpCode, Header.DEPT, Attachment);
						if (strpath == "No_Path")
						{
							return "File_Exit";
						}
						emp.AttachFile = strpath;
					}
                    emp.AttachFile = item.AttachFile;
                    emp.CreateBy = item.CreateBy;
                    emp.CreateOn = DateTime.Now;
                    unitOfWork.Add(emp);
                }
				#endregion

				#region Save Emp Education
				Attachment = "";
				foreach (var item in ListEducation)
				{
					Attachment = item.AttachFile;
					var emp = new HREmpEduc();
					SwapEmpEducation(emp, item, Header);
					if (!string.IsNullOrEmpty(Attachment))
					{
						var strpath = clsCopyFile.CopyStructurePath(Header.CompanyCode, "Education", Header.EmpCode, Header.DEPT, Attachment);
						if (strpath == "No_Path")
						{
							return "File_Exit";
						}
                        else
                        {
							emp.AttachFile = strpath;
						}
					}
					emp.AttachFile = item.AttachFile;
					emp.CreatedBy = item.CreatedBy;
					emp.CreatedOn = DateTime.Now;
					unitOfWork.Add(emp);
				}
				#endregion

				#region Save Emp Contract
				Attachment = "";
				foreach (var item in ListContract)
				{
					Attachment = item.ContractPath;
					var emp = new HREmpContract();
					SwapEmpContract(emp, item, Header);
					if (!string.IsNullOrEmpty(Attachment))
					{
						var strpath = clsCopyFile.CopyStructurePath(Header.CompanyCode, "Contract", Header.EmpCode, Header.DEPT, Attachment);
						if (strpath == "No_Path")
						{
							return "File_Exit";
                        }
                        else
                        {
							emp.ContractPath = strpath;
						}
					}
					emp.CreateBy = item.CreateBy;
					emp.CreateOn = DateTime.Now;
					unitOfWork.Add(emp);
				}
				#endregion

				#region Save Emp Disciplinary
				Attachment = "";
				foreach (var item in ListEmpDisciplinary)
				{
					Attachment = item.AttachPath;
					var emp = new HREmpDisciplinary();
					SwapEmpDisciplinary(emp, item, Header);
					if (!string.IsNullOrEmpty(Attachment))
					{
						var strpath = clsCopyFile.CopyStructurePath(Header.CompanyCode, "Disciplinary", Header.EmpCode, Header.DEPT, Attachment);
						if (strpath == "No_Path")
						{
							return "File_Exit";
						}
                        else
                        {
							emp.AttachPath = strpath;
						}
					}
					emp.CreatedBy = item.CreatedBy;
					emp.CreatedOn = DateTime.Now;
					unitOfWork.Add(emp);
				}
				#endregion

				#region Save Emp Certificate
				Attachment = "";
				foreach (var item in ListEmpCertificate)
				{
					Attachment = item.AttachFile;
					var emp = new HREmpCertificate();
					SwapEmpCertificate(emp, item, Header);
					var getDes = unitOfWork.Set<HRCertificationType>().FirstOrDefault(w => w.Code == item.CertType);
					if (getDes != null)
					{
						emp.CertDesc = getDes.Description;
					}
					if (!string.IsNullOrEmpty(Attachment))
					{
						var strpath = clsCopyFile.CopyStructurePath(Header.CompanyCode, "Certificate", Header.EmpCode, Header.DEPT, Attachment);
						if (strpath == "No_Path")
						{
							return "File_Exit";
                        }
                        else
                        {
                            emp.AttachFile = strpath;
                        }
					}
					emp.CreatedBy = item.CreatedBy;
					emp.CreatedOn = DateTime.Now;
					unitOfWork.Add(emp);
				}
				#endregion

				#region Save Emp Medical
				foreach (var item in ListEmpMedical)
				{
					var emp = new HREmpPhischk();
					SwapEmpMedical(emp, item, Header);
					emp.Createdby = item.Createdby;
					emp.CreatedOn = DateTime.Now;
					unitOfWork.Add(emp);
				}
				#endregion

				LinItem = 0;
                foreach (var item in ListEmpJobDescription)
                {
                    Attachment = item.Attachment;
                    LinItem += 1;
                    var empJD = new HREmpJobDescription();
                    SwapEmpJD(empJD, item, Header);
                    if (!string.IsNullOrEmpty(Attachment))
                    {
                        var strpath = clsCopyFile.CopyStructurePath(Header.CompanyCode, "Job Description", Header.EmpCode, Header.DEPT, Attachment);
						if (strpath == "No_Path")
						{
							return "File_Exit";
						}
                        else
                        {
							empJD.Attachment = strpath;
						}
                    }
                    empJD.LineItem = LinItem;
                    unitOfWork.Add(empJD);
                }
                //Image
                string checkPhoto = CopyPathPhoto(Header, "Photo", Header.Images);
				if(checkPhoto== "No_Path")
                {
					return "File_Exit";
				}
                else
                {
                    Header.Images = checkPhoto;
                }
                //Signature
                string checkSign=CopyPathPhoto(Header, "Signature", Header.Signature);
				if (checkSign == "No_Path")
				{
					return "File_Exit";
                }
                else
                {
                    Header.Signature = checkSign;
				}
                unitOfWork.Add(HeaderCareer);
                unitOfWork.Add(Header);

                unitOfWork.Save();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, Header.EmpCode, SYActionBehavior.ADD.ToString(), e, true);
            }
            catch (DbUpdateException e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, Header.EmpCode, SYActionBehavior.ADD.ToString(), e, true);
            }
            catch (Exception e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, Header.EmpCode, SYActionBehavior.ADD.ToString(), e, true);
            }
        }
        public virtual void OnDetailLoading(params object[] keys)
        {
            string Empcode = (string)keys[0];
            ListEmpIdentity = new List<HREmpIdentity>();
            ListEmpDisciplinary = new List<HREmpDisciplinary>();
            ListEmpFamily = new List<HREmpFamily>();
			ListEducation = new List<HREmpEduc>();
			ListContract = new List<HREmpContract>();
			ListDelayProbation = new List<HRDelayProbation>();
			ListEmpCertificate = new List<HREmpCertificate>();
			ListEmpMedical = new List<HREmpPhischk>();
			Header = unitOfWork.Set<HRStaffProfile>().FirstOrDefault(x => x.EmpCode == Empcode);
			Fist_Last = false;
            Fist_Last_KH = false;
            var FIRST_LASTNAME = SYSettings.getSetting("FIRST_LASTNAME");
            var FIRST_LASTNAME_KH = SYSettings.getSetting("FIRST_LASTNAME_KH");
            if (FIRST_LASTNAME != null)
            {
                if (FIRST_LASTNAME.SettinValue == "TRUE") Fist_Last = true;
            }
            if (FIRST_LASTNAME_KH != null)
            {
                if (FIRST_LASTNAME_KH.SettinValue == "TRUE") Fist_Last_KH = true;
            }
            if (Header != null)
            {
                if (Header.SalaryFlag != true)
                {
                    Header.ReSalary = DateTime.Now;
                }
                ListEmpIdentity = unitOfWork.Repository<HREmpIdentity>().Queryable().Where(w => w.EmpCode == Empcode).ToList();
                ListEmpJobDescription = unitOfWork.Repository<HREmpJobDescription>().Queryable().Where(w => w.EmpCode == Empcode).ToList();
                ListEmpFamily = unitOfWork.Repository<HREmpFamily>().Queryable().Where(w => w.EmpCode == Empcode).ToList();
				ListEducation = unitOfWork.Repository<HREmpEduc>().Queryable().Where(w => w.EmpCode == Empcode).ToList();
				ListContract = unitOfWork.Repository<HREmpContract>().Queryable().Where(w => w.EmpCode == Empcode).ToList();
				ListDelayProbation = unitOfWork.Repository<HRDelayProbation>().Queryable().Where(w => w.EmpCode == Empcode).ToList();
				ListEmpDisciplinary = unitOfWork.Repository<HREmpDisciplinary>().Queryable().Where(w => w.EmpCode == Empcode).ToList();
				ListEmpCertificate = unitOfWork.Repository<HREmpCertificate>().Queryable().Where(w => w.EmpCode == Empcode).ToList();
				ListEmpMedical = unitOfWork.Repository<HREmpPhischk>().Queryable().Where(w => w.EmpCode == Empcode).ToList();

                var IsSalary = IsHideSalary(Header.LevelCode);
                if (IsSalary != true)
                {
                    Salary = "#####";
                }
                else
                {
                    Salary = Header.Salary.ToString();
                }
            }
        }
        public string UpdateStaff(string ID)
        {
            OnLoad();
            try
            {
                HRStaffProfile objMast = unitOfWork.Set<HRStaffProfile>().FirstOrDefault(w => w.EmpCode == ID);
                if (objMast == null)
                {
                    return "STAFF_NE";
                }
                if (Header.SalaryType == null)
                {
                    return "SALARY_TYPE_EN";
                }
                objMast.FirstName = Header.FirstName;
                objMast.LastName = Header.LastName;
                objMast.AllName = Header.AllName;
                //objMast.AllName = Header.FirstName + " " + Header.LastName;
                objMast.OthFirstName = Header.OthFirstName;
                objMast.OthLastName = Header.OthLastName;
                objMast.OthAllName = Header.OthAllName;
                // objMast.OthAllName = Header.OthFirstName + " " + Header.OthLastName;
                objMast.Sex = Header.Sex;
                objMast.Title = Header.Title;
                objMast.CardNo = Header.CardNo;
                objMast.Marital = Header.Marital;
                objMast.POB = Header.POB;
                objMast.Country = Header.Country;
                objMast.Nation = Header.Nation;
                objMast.State = Header.State;
                objMast.Phone1 = Header.Phone1;
                objMast.Phone2 = Header.Phone2;
                objMast.PhoneExt = Header.PhoneExt;
                objMast.Email = Header.Email;
                objMast.Email2 = Header.Email2;
                objMast.PayParam = Header.PayParam;
                objMast.TXPayType = Header.TXPayType;
                objMast.BankName = Header.BankName;
                objMast.BankAccName = Header.BankAccName;
                objMast.BankAcc = Header.BankAcc;
                objMast.Peraddress = Header.Peraddress;
                objMast.PerPhone = Header.PerPhone;
                objMast.PeraddressOth = Header.PeraddressOth;
                objMast.ChangedOn = DateTime.Now;
                objMast.IsOTApproval = Header.IsOTApproval;
                objMast.ChangedBy = User.UserName;
                //objMast.Images = Header.Images;
                //objMast.Signature = Header.Signature;
                objMast.PostFamily = Header.PostFamily;
                objMast.SOCSO = Header.SOCSO;
                objMast.ISNSSF = Header.ISNSSF;
                objMast.IsResident = Header.IsResident;
                objMast.HODCode = Header.HODCode;
                objMast.SalaryProb = Header.SalaryProb;
                objMast.IsHold = Header.IsHold;
                objMast.ROSTER = Header.ROSTER;
                objMast.ProbationType = Header.ProbationType;
                objMast.Probation = Header.Probation;
                objMast.ReSalary = Header.ReSalary;
                objMast.SalaryFlag = Header.SalaryFlag;
                objMast.DOB = Header.DOB;
                objMast.TeleGroup = Header.TeleGroup;
                objMast.TeleGroupOT = Header.TeleGroupOT;
                objMast.TeleGroupScan = Header.TeleGroupScan;
                objMast.TeleChartID = Header.TeleChartID;
				objMast.IsExceptScan = Header.IsExceptScan;
				if (Header.FirstLine == Header.FirstLine2 && !string.IsNullOrEmpty(Header.FirstLine))
                    return "FIRSTLINE_EN";
                if (Header.SecondLine == Header.SecondLine2 && !string.IsNullOrEmpty(Header.SecondLine))
                    return "SECONDLINE_EN";
                objMast.FirstLine = Header.FirstLine;
                objMast.SecondLine = Header.SecondLine;
                objMast.FirstLine2 = Header.FirstLine2;
                objMast.SecondLine2 = Header.SecondLine2;
                objMast.Supervisory = Header.Supervisory;
                objMast.BankFee = Header.BankFee;
                objMast.CheckSum = Header.CheckSum;
                objMast.BankBranch = Header.BankBranch;
                objMast.BankAccType = Header.BankAccType;
                objMast.Province = Header.Province;
                objMast.District = Header.District;
                objMast.Commune = Header.Commune;
                objMast.Village = Header.Village;
                objMast.TID = Header.TID;
                objMast.Costcent = Header.Costcent;
                objMast.SalaryType = Header.SalaryType;
                objMast.IsAtten = Header.IsAtten;
                objMast.StaffType = Header.StaffType;
                objMast.LeaveConf = Header.LeaveConf;
                if (Header.LeaveConf == null)
                    objMast.LeaveConf = Header.StartDate;
                objMast.HODCode = Header.HODCode;
                //objMast.NSSFContributionType = Header.NSSFContributionType;
                objMast.IsCalSalary = Header.IsCalSalary;
                objMast.TemLeave = Header.TemLeave;
                objMast.IsPayPartial = Header.IsPayPartial;
                objMast.ConAddress = Header.ConAddress;
                objMast.ConAddressOth = Header.ConAddressOth;
                objMast.PassPayslip = Header.PassPayslip;
                objMast.OTFirstLine = Header.OTFirstLine;
                objMast.OTSecondLine = Header.OTSecondLine;
                objMast.OTthirdLine = Header.OTthirdLine;
                objMast.IsMealAllowance = Header.IsMealAllowance;
                objMast.GPSData = Header.GPSData;
				//objMast.AttachFile = Header.AttachFile;
				objMast.Attachment = Header.Attachment;
                objMast.IsAutoAppLeave = Header.IsAutoAppLeave;
                objMast.IsAutoAppKPITraing = Header.IsAutoAppKPITraing;
                objMast.IsOnsite = Header.IsOnsite;
                objMast.APPTracking = Header.APPTracking;
                objMast.APPEvaluator = Header.APPEvaluator;
                objMast.APPAppraisal = Header.APPAppraisal;
                objMast.APPAppraisal2 = Header.APPAppraisal2;

				if (objMast.AttachFile != Header.AttachFile)
				{
					if (!string.IsNullOrEmpty(objMast.AttachFile))
					{
						var str = objMast.AttachFile.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
						foreach (var fileName in str)
						{
							DeleteFile(fileName);
						}
					}

					ClsCopyFile clsCopyFiles = new ClsCopyFile();
					var strpath = clsCopyFiles.CopyStructurePath(Header.CompanyCode, "Address", Header.EmpCode, Header.DEPT, Header.AttachFile);
					if (strpath == "No_Path")
					{
						return "File_Exit";
					}
					else
					{
						objMast.AttachFile = strpath;
					}
				}

				#region Save Emp HREmpIdentity

				var LisIdentityDelete = unitOfWork.Set<HREmpIdentity>().Where(w => w.EmpCode == Header.EmpCode).ToList();
				var ListEmpIdentityOld = LisIdentityDelete.Select(x => new HREmpIdentity
				{
					IdentityTye = x.IdentityTye,
					Attachment = x.Attachment
				}).ToList();
				var removedIdentity = ListEmpIdentityOld.Where(oldItem => !ListEmpIdentity.Any(newItem => newItem.IdentityTye == oldItem.IdentityTye)).ToList();
				foreach (var removed in removedIdentity)
				{
					if (!string.IsNullOrEmpty(removed.Attachment))
					{
						var files = removed.Attachment.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
						foreach (var file in files)
						{
							DeleteFile(file);
						}
					}
				}
				foreach (var item in LisIdentityDelete)
				{
					unitOfWork.Delete(item);
				}
				int LinItem = 0;
				string Attachment = "";
				ClsCopyFile clsCopyFile = new ClsCopyFile();
				var listInden = unitOfWork.Set<HRIdentityType>().ToList();
				foreach (var item in ListEmpIdentity)
				{
					Attachment = item.Attachment;
					LinItem += 1;
					var empIden = new HREmpIdentity();
					SwapEmpIdentity(empIden, item, Header);
					if (!string.IsNullOrEmpty(Attachment))
					{
						var objIdentMo = LisIdentityDelete.Where(w => w.IdentityTye == item.IdentityTye).ToList();
						if (!objIdentMo.Where(w => w.Attachment == item.Attachment).Any())
						{
							foreach (var existingFile in objIdentMo)
							{
								if (!string.IsNullOrEmpty(existingFile.Attachment))
								{
									var str1 = existingFile.Attachment.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
									var str2 = Attachment.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

									var filesToDelete = str1.Except(str2).ToList();
									foreach (var fileName in filesToDelete)
									{
										DeleteFile(fileName);
									}
								}
							}
							var DescInd = listInden.FirstOrDefault(w => w.Code == item.IdentityTye);
							if (DescInd != null)
							{
								string DocumentType = DescInd.Description;
								var strpath = clsCopyFile.CopyStructurePath(Header.CompanyCode, DocumentType, Header.EmpCode, Header.DEPT, Attachment);
								if (strpath == "No_Path")
								{
									return "File_Exit";
								}
                                else
                                {
                                    empIden.Attachment = strpath;
                                }
							}
						}
						else
						{
							empIden.Attachment = item.Attachment;
						}
					}
					else
					{
						var objIdentMo = LisIdentityDelete.Where(w => w.IdentityTye == item.IdentityTye).ToList();
						foreach (var existingFile in objIdentMo)
						{
							DeleteFile(existingFile.Attachment);
						}
					}
					empIden.LineItem = LinItem;
					unitOfWork.Add(empIden);
				}
				#endregion

				#region Save Emp JobDescription
				var LisJobDelete = unitOfWork.Set<HREmpJobDescription>().Where(w => w.EmpCode == Header.EmpCode).ToList();
				foreach (var item in LisJobDelete)
                {
                    unitOfWork.Delete(item);
				}
                LinItem = 0;
                foreach (var item in ListEmpJobDescription)
                {
                    Attachment = item.Attachment;
                    LinItem += 1;
                    var empJD = new HREmpJobDescription();
                    SwapEmpJD(empJD, item, Header);
                    if (!string.IsNullOrEmpty(Attachment))
                    {
                        var strpath = clsCopyFile.CopyStructurePath(Header.CompanyCode, "Job Description", Header.EmpCode, Header.DEPT, Attachment);
						if (strpath == "No_Path")
						{
							return "File_Exit";
                        }
                        else
                        {
                            empJD.Attachment = strpath;
                        }
                    }
                    empJD.LineItem = LinItem;
                    unitOfWork.Add(empJD);
                }
				if (Path.GetFileName(objMast.Images) != Path.GetFileName(Header.Images))
				{
                    string checkFile = CopyPathPhoto(Header, "Photo", Header.Images);
                    if(checkFile== "No_Path")
                    {
                        return "File_Exit";
                    }
                    else
                    {
						objMast.Images = checkFile;
					}
                }
				if (Path.GetFileName(objMast.Signature) != Path.GetFileName(Header.Signature))
				{
					string checkFile = CopyPathPhoto(Header, "Signature", Header.Signature);
					if (checkFile == "No_Path")
					{
						return "File_Exit";
					}
					else
					{
						objMast.Signature = checkFile;
					}
				}
				#endregion

				unitOfWork.Update(objMast);

				#region Save Emp Family

				var objEmpFamily = unitOfWork.Set<HREmpFamily>().Where(w => w.EmpCode == Header.EmpCode).ToList();
				var ListEmpFamilyOld = objEmpFamily.Select(x => new HREmpFamily
				{
					RelName = x.RelName,
					AttachFile = x.AttachFile
				}).ToList();
				var removedFamily = ListEmpFamilyOld.Where(oldItem => !ListEmpFamily.Any(newItem => newItem.RelName == oldItem.RelName)).ToList();
				foreach (var removed in removedFamily)
				{
					if (!string.IsNullOrEmpty(removed.AttachFile))
					{
						var files = removed.AttachFile.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
						foreach (var file in files)
						{
							DeleteFile(file);
						}
					}
				}
				foreach (var item in objEmpFamily)
                {
                    unitOfWork.Delete(item);
                }
                LinItem = 0;
                foreach (var item in ListEmpFamily)
                {
                    Attachment = item.AttachFile;
                    LinItem += 1;
                    var emp = new HREmpFamily();
                    SwapEmpFamily(emp, item, Header);
                    if (!string.IsNullOrEmpty(Attachment))
                    {
						var objIdentMo = objEmpFamily.Where(w => w.RelName == item.RelName).ToList();
						if (!objIdentMo.Where(w => w.AttachFile == item.AttachFile).Any())
                        {
							foreach (var existingFile in objIdentMo)
							{
								if (!string.IsNullOrEmpty(existingFile.AttachFile))
                                {
									var str1 = existingFile.AttachFile.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
									var str2 = Attachment.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

									var filesToDelete = str1.Except(str2).ToList();
									foreach (var fileName in filesToDelete)
									{
										DeleteFile(fileName);
									}
								}
							}
							var strpath = clsCopyFile.CopyStructurePath(Header.CompanyCode, "Family", Header.EmpCode, Header.DEPT, Attachment);
							if (strpath == "No_Path")
							{
								return "File_Exit";
                            }
                            else
                            {
                                emp.AttachFile = strpath;
                            }
						}
                        else
                        {
                            emp.AttachFile = item.AttachFile;
                        }
                    }
                    else
                    {
						var objIdentMo = objEmpFamily.Where(w => w.RelName == item.RelName).ToList();
						foreach (var existingFile in objIdentMo)
						{
							DeleteFile(existingFile.AttachFile);
						}
					}
                    emp.LineItem = LinItem;
                    emp.ChangedBy = User.UserName;
                    emp.ChangedOn = DateTime.Now;
                    unitOfWork.Add(emp);
                }
				#endregion

				#region Save Emp Education
				var objEmpEducation = unitOfWork.Set<HREmpEduc>().Where(w => w.EmpCode == Header.EmpCode).ToList();
				var ListEmpEduOld = objEmpEducation.Select(x => new HREmpEduc
				{
					EduType = x.EduType,
					AttachFile = x.AttachFile
				}).ToList();
				var removedEdu = ListEmpEduOld.Where(oldItem => !ListEducation.Any(newItem => newItem.EduType == oldItem.EduType)).ToList();
				foreach (var removed in removedEdu)
				{
					if (!string.IsNullOrEmpty(removed.AttachFile))
					{
						var files = removed.AttachFile.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
						foreach (var file in files)
						{
							DeleteFile(file);
						}
					}
				}
				foreach (var item in objEmpEducation)
				{
					unitOfWork.Delete(item);
				}
				foreach (var item in ListEducation)
				{
					Attachment = item.AttachFile;
					var emp = new HREmpEduc();
					SwapEmpEducation(emp, item, Header);
					if (!string.IsNullOrEmpty(Attachment))
					{
						var objIdentMo = objEmpEducation.Where(w => w.TranNo == item.TranNo).ToList();
						if (!objIdentMo.Where(w => w.AttachFile == item.AttachFile).Any())
						{
							foreach (var existingFile in objIdentMo)
							{
								if (!string.IsNullOrEmpty(existingFile.AttachFile))
								{
									var str1 = existingFile.AttachFile.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
									var str2 = Attachment.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

									var filesToDelete = str1.Except(str2).ToList();
									foreach (var fileName in filesToDelete)
									{
										DeleteFile(fileName);
									}
								}
							}
							var strpath = clsCopyFile.CopyStructurePath(Header.CompanyCode, "Education", Header.EmpCode, Header.DEPT, Attachment);
							if (strpath == "No_Path")
							{
								return "File_Exit";
							}
                            else
                            {
                                emp.AttachFile = strpath;
                            }
						}
						else
						{
							emp.AttachFile = item.AttachFile;
						}
					}
					emp.ChangedBy = User.UserName;
					emp.ChangedOn = DateTime.Now;
					unitOfWork.Add(emp);
				}
				#endregion

				#region Save Emp Contract
				var objEmpContract = unitOfWork.Set<HREmpContract>().Where(w => w.EmpCode == Header.EmpCode).ToList();
				var ListEmpContractOld = objEmpContract.Select(x => new HREmpContract
				{
					ConType = x.ConType,
					ContractPath = x.ContractPath
				}).ToList();
				var removedContract = ListEmpContractOld.Where(oldItem => !ListContract.Any(newItem => newItem.ConType == oldItem.ConType)).ToList();
				foreach (var removed in removedContract)
				{
					if (!string.IsNullOrEmpty(removed.ContractPath))
					{
						var files = removed.ContractPath.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
						foreach (var file in files)
						{
							DeleteFile(file);
						}
					}
				}
				foreach (var item in objEmpContract)
				{
					unitOfWork.Delete(item);
				}
				foreach (var item in ListContract)
				{
					Attachment = item.ContractPath;
					var emp = new HREmpContract();
					SwapEmpContract(emp, item, Header);
					if (!string.IsNullOrEmpty(Attachment))
					{
						var objContract = objEmpContract.Where(w => w.TranNo == item.TranNo).ToList();
						if (!objContract.Where(w => w.ContractPath == item.ContractPath).Any())
						{
							foreach (var existingFile in objContract)
							{
								if (!string.IsNullOrEmpty(existingFile.ContractPath))
								{
									var str1 = existingFile.ContractPath.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
									var str2 = Attachment.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

									var filesToDelete = str1.Except(str2).ToList();
									foreach (var fileName in filesToDelete)
									{
										DeleteFile(fileName);
									}
								}
							}
							var strpath = clsCopyFile.CopyStructurePath(Header.CompanyCode, "Contract", Header.EmpCode, Header.DEPT, Attachment);
							if (strpath == "No_Path")
							{
								return "File_Exit";
                            }
                            else
                            {
                                emp.ContractPath = strpath;
                            }
						}
						else
						{
							emp.ContractPath = item.ContractPath;
						}
					}
					emp.ChangedBy = User.UserName;
					emp.ChangedOn = DateTime.Now;
					unitOfWork.Add(emp);
				}
				#endregion

				#region Save Emp Disciplinary
				var objEmpDisciplinary = unitOfWork.Set<HREmpDisciplinary>().Where(w => w.EmpCode == Header.EmpCode).ToList();
				var ListEmpDisciplinaryOld = objEmpDisciplinary.Select(x => new HREmpDisciplinary
				{
					DiscType = x.DiscType,
					AttachPath = x.AttachPath
				}).ToList();
				var removedDisciplinary = ListEmpDisciplinaryOld.Where(oldItem => !ListEmpDisciplinary.Any(newItem => newItem.DiscType == oldItem.DiscType)).ToList();
				foreach (var removed in removedDisciplinary)
				{
					if (!string.IsNullOrEmpty(removed.AttachPath))
					{
						var files = removed.AttachPath.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
						foreach (var file in files)
						{
							DeleteFile(file);
						}
					}
				}
				foreach (var item in objEmpDisciplinary)
				{
					unitOfWork.Delete(item);
				}
				foreach (var item in ListEmpDisciplinary)
				{
					Attachment = item.AttachPath;
					var emp = new HREmpDisciplinary();
					SwapEmpDisciplinary(emp, item, Header);
					if (!string.IsNullOrEmpty(Attachment))
					{
						var objIdentMo = objEmpDisciplinary.Where(w => w.DiscType == item.DiscType).ToList();
						if (!objIdentMo.Where(w => w.AttachPath == item.AttachPath).Any())
						{
							foreach (var existingFile in objIdentMo)
							{
								if (!string.IsNullOrEmpty(existingFile.AttachPath))
								{
									var str1 = existingFile.AttachPath.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
									var str2 = Attachment.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

									var filesToDelete = str1.Except(str2).ToList();
									foreach (var fileName in filesToDelete)
									{
										DeleteFile(fileName);
									}
								}
							}
							var strpath = clsCopyFile.CopyStructurePath(Header.CompanyCode, "Disciplinary", Header.EmpCode, Header.DEPT, Attachment);
                            if (strpath == "No_Path")
                            {
                                return "File_Exit";
                            }
                            else
                            {
                                emp.AttachPath = strpath;
                            }
						}
						else
						{
							emp.AttachPath = item.AttachPath;
						}
					}
					else
					{
						var objIdentMo = objEmpDisciplinary.Where(w => w.DiscType == item.DiscType).ToList();
						foreach (var existingFile in objIdentMo)
						{
							DeleteFile(existingFile.AttachPath);
						}
					}
					emp.ChangedBy = User.UserName;
					emp.ChangedOn = DateTime.Now;
					unitOfWork.Add(emp);
				}
				#endregion

				#region Save Emp Certificate
				var objEmpCertificate = unitOfWork.Set<HREmpCertificate>().Where(w => w.EmpCode == Header.EmpCode).ToList();
				var ListEmpCertificateOld = objEmpCertificate.Select(x => new HREmpCertificate
				{
					CertType = x.CertType,
					AttachFile = x.AttachFile
				}).ToList();
				var removedCertificate = ListEmpCertificateOld.Where(oldItem => !ListEmpCertificate.Any(newItem => newItem.CertType == oldItem.CertType)).ToList();
				foreach (var removed in removedCertificate)
				{
					if (!string.IsNullOrEmpty(removed.AttachFile))
					{
						var files = removed.AttachFile.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
						foreach (var file in files)
						{
							DeleteFile(file);
						}
					}
				}
				foreach (var item in objEmpCertificate)
				{
					unitOfWork.Delete(item);
				}
				foreach (var item in ListEmpCertificate)
				{
					Attachment = item.AttachFile;
					var emp = new HREmpCertificate();
					var getDes = unitOfWork.Set<HRCertificationType>().FirstOrDefault(w => w.Code == item.CertType);
                    if (getDes != null)
                    {
						emp.CertDesc= getDes.Description;    
					}
					SwapEmpCertificate(emp, item, Header);
					if (!string.IsNullOrEmpty(Attachment))
					{
						var objIdentMo = objEmpCertificate.Where(w => w.CertType == item.CertType).ToList();
						if (!objIdentMo.Where(w => w.AttachFile == item.AttachFile).Any())
						{
							foreach (var existingFile in objIdentMo)
							{
								if (!string.IsNullOrEmpty(existingFile.AttachFile))
								{
									var str1 = existingFile.AttachFile.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
									var str2 = Attachment.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

									var filesToDelete = str1.Except(str2).ToList();
									foreach (var fileName in filesToDelete)
									{
										DeleteFile(fileName);
									}
								}
							}
							var strpath = clsCopyFile.CopyStructurePath(Header.CompanyCode, "Certificate", Header.EmpCode, Header.DEPT, Attachment);
							if (strpath == "No_Path")
							{
								return "File_Exit";
							}
                            else
                            {
                                emp.AttachFile = strpath;
                            }
						}
						else
						{
							emp.AttachFile = item.AttachFile;
						}
					}
					else
					{
						var objIdentMo = objEmpCertificate.Where(w => w.CertType == item.CertType).ToList();
						foreach (var existingFile in objIdentMo)
						{
							DeleteFile(existingFile.AttachFile);
						}
					}
					emp.ChangedBy = User.UserName;
					emp.ChangedOn = DateTime.Now;
					unitOfWork.Add(emp);
				}
				#endregion

				#region Save Emp Medical
				var objEmpMedical = unitOfWork.Set<HREmpPhischk>().Where(w => w.EmpCode == Header.EmpCode).ToList();
				foreach (var item in objEmpMedical)
				{
					unitOfWork.Delete(item);
				}
				foreach (var item in ListEmpMedical)
				{
					var emp = new HREmpPhischk();
					SwapEmpMedical(emp, item, Header);
					emp.ChangedBy = User.UserName;
					emp.ChangedOn = DateTime.Now;
					unitOfWork.Add(emp);
				}
				#endregion

				unitOfWork.Save();
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, ID, SYActionBehavior.ADD.ToString(), e, true);
            }
            catch (DbUpdateException e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, ID, SYActionBehavior.ADD.ToString(), e, true);
            }
            catch (Exception e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, ID, SYActionBehavior.ADD.ToString(), e, true);
            }
        }
        public string DeleteEmp(string EmpCode)
        {
            OnLoad();
            try
            {
                var objMatch = unitOfWork.Set<HRStaffProfile>().FirstOrDefault(w => w.EmpCode == EmpCode);
                if (objMatch == null)
                {
                    return "STAFF_NE";
                }
                var ListGen = unitOfWork.Set<HISGenSalaryD>().Where(w => w.EmpCode == EmpCode).ToList();
                if (ListGen.Count > 0)
                {
                    return "ISGEN_SALARY";
                }

                var ListEmpID = unitOfWork.Set<HREmpIdentity>().Where(w => w.EmpCode == EmpCode).ToList();
                foreach (var item in ListEmpID)
                {
                    unitOfWork.Delete(item);
                }
                var ListEmpJobDes = unitOfWork.Set<HREmpJobDescription>().Where(w => w.EmpCode == EmpCode).ToList();
                foreach (var item in ListEmpJobDes)
                {
                    unitOfWork.Delete(item);
                }
                var objMatchEmp = unitOfWork.Set<HREmpCareer>().FirstOrDefault(w => w.EmpCode == EmpCode);
                unitOfWork.Delete(objMatchEmp);
                unitOfWork.Delete(objMatch);
                unitOfWork.Save();
                ClsCopyFile copyFile = new ClsCopyFile();
                foreach (var item in ListEmpID)
                {
                    if (!string.IsNullOrEmpty(item.Attachment))
                    {
                        copyFile.DeleteFileStore(item.Attachment);
                    }
                }
                foreach (var item in ListEmpJobDes)
                {
                    if (!string.IsNullOrEmpty(item.Attachment))
                    {
                        copyFile.DeleteFileStore(item.Attachment);
                    }
                }
                if (!string.IsNullOrEmpty(objMatch.Images))
                {
                    copyFile.DeleteFileStore_URL(objMatch.Images);
                }
                if (!string.IsNullOrEmpty(objMatch.Signature))
                {
                    copyFile.DeleteFileStore_URL(objMatch.Signature);
                }
                return SYConstant.OK;
            }
            catch (Exception e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, EmpCode, SYActionBehavior.ADD.ToString(), e, true);
            }
        }
        public string OnGridModifyIdentity(HREmpIdentity MModel, string Action, bool IsEdit = false, bool IsAddNewFile = false)
        {
            int LineItem = 1;
            if (ListEmpIdentity.Count() > 0)
            {
                LineItem = ListEmpIdentity.Max(w => w.LineItem) + 1;
            }
            if (MModel.IssueDate.HasValue && MModel.ExpiryDate.HasValue)
            {
                if (MModel.ExpiryDate.Value < MModel.IssueDate.Value)
                {
                    return "IN_DATE_RANK";
                }
            }
            if (Action == "ADD")
            {
                if (string.IsNullOrEmpty(MModel.IDCardNo))
                {
                    return "INV_DOC";
                }
                MModel.LineItem = LineItem;
                ListEmpIdentity.Add(MModel);
            }
            else if (Action == "EDIT")
            {
                var objCheck = ListEmpIdentity.Where(w => w.LineItem == MModel.LineItem).FirstOrDefault();
                if (objCheck != null)
                {
                    objCheck.IDCardNo = MModel.IDCardNo;
                    objCheck.IssueDate = MModel.IssueDate;
                    objCheck.ExpiryDate = MModel.ExpiryDate;
					objCheck.IsActive = MModel.IsActive;
					string temppath = objCheck.Attachment;
					if (IsEdit)
					{
						if (!string.IsNullOrWhiteSpace(MModel.Attachment))
						{
							if (!string.IsNullOrWhiteSpace(objCheck.Attachment))
							{
								var existingFiles = objCheck.Attachment.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
								var newFiles = MModel.Attachment.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

								if (IsAddNewFile)
								{
									foreach (var file in newFiles)
									{
										if (!existingFiles.Contains(file, StringComparer.OrdinalIgnoreCase))
										{
											existingFiles.Add(file);
										}
									}
									objCheck.Attachment = string.Join(";", existingFiles);
								}
								else
								{
									existingFiles = newFiles;
									objCheck.Attachment = string.Join(";", existingFiles);
								}
							}
							else
							{
								objCheck.Attachment = MModel.Attachment;
							}
						}
						else
						{
							objCheck.Attachment = string.Empty;
						}
						//objCheck.AttachFile = MModel.AttachFile;
						if (objCheck.TranNo == 0)
						{
							DeleteFile(temppath, objCheck.Attachment);
						}
					}

				}
            }
            else if (Action == "DELETE")
            {
                var objCheck = ListEmpIdentity.Where(w => w.LineItem == MModel.LineItem).FirstOrDefault();
                if (objCheck != null)
                {
                    ListEmpIdentity.Remove(objCheck);
                    if (objCheck.TranNo == 0)
                    {
                        DeleteFile(objCheck.Attachment);
                    }
                    return SYConstant.OK;
                }
                else
                {
                    return "INV_DOC";
                }
            }
            return SYConstant.OK;
        }
        public string OnGridModifyJobDescription(HREmpJobDescription MModel, string Action, bool IsEdit = false)
        {
            int LineItem = 1;
            if (ListEmpJobDescription.Count() > 0)
            {
                LineItem = ListEmpJobDescription.Max(w => w.LineItem) + 1;
            }
            if (Action == "ADD")
            {
				if (string.IsNullOrEmpty(MModel.JobResponsive) && string.IsNullOrEmpty(MModel.JobDescription))
				{
					return "JOB_RESPONSIVE_AND_JOBDESCRIPTION_INVALID";
				}
				MModel.LineItem = LineItem;
                ListEmpJobDescription.Add(MModel);
            }
            else if (Action == "EDIT")
            {
				if (string.IsNullOrEmpty(MModel.JobResponsive) && string.IsNullOrEmpty(MModel.JobDescription))
				{
					return "JOB_RESPONSIVE_AND_JOBDESCRIPTION_INVALID";
				}
				var objCheck = ListEmpJobDescription.Where(w => w.TranNo == MModel.TranNo).FirstOrDefault();
                if (objCheck != null)
                {
                    objCheck.JobResponsive = MModel.JobResponsive;
                    objCheck.JobDescription = MModel.JobDescription;
                    objCheck.IsActive = MModel.IsActive;
                    string temppath = objCheck.Attachment;
                    if (IsEdit)
                    {
                        objCheck.Attachment = MModel.Attachment;
                        objCheck.Document = MModel.Document;
                        if (objCheck.TranNo == 0)
                        {
                            DeleteFile(temppath, objCheck.Attachment);
                        }
                    }

                }
            }
            else if (Action == "DELETE")
            {
                var objCheck = ListEmpJobDescription.Where(w => w.TranNo == MModel.TranNo).FirstOrDefault();
                if (objCheck != null)
                {
                    ListEmpJobDescription.Remove(objCheck);
                    if (objCheck.TranNo == 0)
                    {
                        DeleteFile(objCheck.Attachment);
                    }
                    return SYConstant.OK;
                }
                else
                {
                    return "INV_DOC";
                }
            }
            return SYConstant.OK;
        }

        public string OnGridModifyFamily(HREmpFamily MModel, string Action, bool IsEdit = false, bool IsAddNewFile=false)
        {
            if (Action == "ADD")
            {
                ListEmpFamily.Add(MModel);
            }
            else if (Action == "EDIT")
            {
                var objCheck = ListEmpFamily.Where(w => w.TranNo == MModel.TranNo).FirstOrDefault();
                if (objCheck != null)
                {
                    objCheck.RelCode = MModel.RelCode;
                    objCheck.RelName = MModel.RelName;
                    objCheck.Sex = MModel.Sex;
                    objCheck.TaxDeduc = MModel.TaxDeduc;
                    objCheck.Child = MModel.Child;
                    objCheck.Spouse = MModel.Spouse;
                    objCheck.IDCard = MModel.IDCard;
                    objCheck.Occupation = MModel.Occupation;
                    objCheck.InSchool = MModel.InSchool;
                    objCheck.PhoneNo = MModel.PhoneNo;
                    string temppath = objCheck.AttachFile;
                    if (IsEdit)
                    {
						if (!string.IsNullOrWhiteSpace(MModel.AttachFile))
						{
							if (!string.IsNullOrWhiteSpace(objCheck.AttachFile))
							{
								var existingFiles = objCheck.AttachFile.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
								var newFiles = MModel.AttachFile.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

								if (IsAddNewFile)
								{
									foreach (var file in newFiles)
									{
										if (!existingFiles.Contains(file, StringComparer.OrdinalIgnoreCase))
										{
											existingFiles.Add(file);
										}
									}
									objCheck.AttachFile = string.Join(";", existingFiles);
								}
								else
								{
									existingFiles = newFiles;
									objCheck.AttachFile = string.Join(";", existingFiles);
								}
							}
                            else
                            {
								objCheck.AttachFile = MModel.AttachFile;
							}
						}
                        else
                        {
							objCheck.AttachFile=string.Empty;
						}
                        //objCheck.AttachFile = MModel.AttachFile;
                        if (objCheck.TranNo == 0)
                        {
                            DeleteFile(temppath, objCheck.AttachFile);
                        }
                    }

                }
            }
            else if (Action == "DELETE")
            {
                var objCheck = ListEmpFamily.Where(w => w.TranNo == MModel.TranNo).FirstOrDefault();
                if (objCheck != null)
                {
                    ListEmpFamily.Remove(objCheck);
                    if (objCheck.TranNo == 0)
                    {
                        DeleteFile(objCheck.AttachFile);
                    }
                    return SYConstant.OK;
                }
                else
                {
                    return "INV_DOC";
                }
            }
            return SYConstant.OK;
        }
		public string OnGridModifyEducation(HREmpEduc MModel, string Action, bool IsEdit = false,bool IsAddNewFile=false)
		{
			if (Action == "ADD")
			{
				ListEducation.Add(MModel);
			}
			else if (Action == "EDIT")
			{
				var objCheck = ListEducation.Where(w => w.TranNo == MModel.TranNo).FirstOrDefault();
				if (objCheck != null)
				{
					objCheck.EduType = MModel.EduType;
					objCheck.StartDate = MModel.StartDate;
					objCheck.EndDate = MModel.EndDate;
					objCheck.EdcCenter = MModel.EdcCenter;
					objCheck.Major = MModel.Major;
					objCheck.Result = MModel.Result;
					objCheck.Remark = MModel.Remark;
					string temppath = objCheck.AttachFile;
					if (IsEdit)
					{
						if (!string.IsNullOrWhiteSpace(MModel.AttachFile))
						{
							if (!string.IsNullOrWhiteSpace(objCheck.AttachFile))
							{
								var existingFiles = objCheck.AttachFile.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
								var newFiles = MModel.AttachFile.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

								if (IsAddNewFile)
								{
									foreach (var file in newFiles)
									{
										if (!existingFiles.Contains(file, StringComparer.OrdinalIgnoreCase))
										{
											existingFiles.Add(file);
										}
									}
									objCheck.AttachFile = string.Join(";", existingFiles);
								}
								else
								{
									existingFiles = newFiles;
									objCheck.AttachFile = string.Join(";", existingFiles);
								}
							}
							else
							{
								objCheck.AttachFile = MModel.AttachFile;
							}
						}
						else
						{
							objCheck.AttachFile = string.Empty;
						}
						//objCheck.AttachFile = MModel.AttachFile;
						if (objCheck.TranNo == 0)
						{
							DeleteFile(temppath, objCheck.AttachFile);
						}
					}
				}
			}
			else if (Action == "DELETE")
			{
				var objCheck = ListEducation.Where(w => w.TranNo == MModel.TranNo).FirstOrDefault();
				if (objCheck != null)
				{
					ListEducation.Remove(objCheck);
					if (objCheck.TranNo == 0)
					{
						DeleteFile(objCheck.AttachFile);
					}
					return SYConstant.OK;
				}
				else
				{
					return "INV_DOC";
				}
			}
			return SYConstant.OK;
		}
		public string OnGridModifyContract(HREmpContract MModel, string Action, bool IsEdit = false, bool IsAddNewFile = false)
		{
			if (Action == "ADD")
			{
				ListContract.Add(MModel);
			}
			else if (Action == "EDIT")
			{
				var objCheck = ListContract.Where(w => w.TranNo == MModel.TranNo).FirstOrDefault();
				if (objCheck != null)
				{
					objCheck.ConType = MModel.ConType;
					objCheck.Conterm = MModel.Conterm;
					objCheck.FromDate = MModel.FromDate;
					objCheck.Description = MModel.Description;
					string temppath = objCheck.ContractPath;
					if (IsEdit)
					{
						if (!string.IsNullOrWhiteSpace(MModel.ContractPath))
						{
							if (!string.IsNullOrWhiteSpace(objCheck.ContractPath))
							{
								var existingFiles = objCheck.ContractPath.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
								var newFiles = MModel.ContractPath.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

								if (IsAddNewFile)
								{
									foreach (var file in newFiles)
									{
										if (!existingFiles.Contains(file, StringComparer.OrdinalIgnoreCase))
										{
											existingFiles.Add(file);
										}
									}
									objCheck.ContractPath = string.Join(";", existingFiles);
								}
								else
								{
									existingFiles = newFiles;
									objCheck.ContractPath = string.Join(";", existingFiles);
								}
							}
							else
							{
								objCheck.ContractPath = MModel.ContractPath;
							}
						}
						else
						{
							objCheck.ContractPath = string.Empty;
						}
						//objCheck.AttachFile = MModel.AttachFile;
						if (objCheck.TranNo == 0)
						{
							DeleteFile(temppath, objCheck.ContractPath);
						}
					}

				}
			}
			else if (Action == "DELETE")
			{
				var objCheck = ListContract.Where(w => w.TranNo == MModel.TranNo).FirstOrDefault();
				if (objCheck != null)
				{
					ListContract.Remove(objCheck);
					if (objCheck.TranNo == 0)
					{
						DeleteFile(objCheck.ContractPath);
					}
					return SYConstant.OK;
				}
				else
				{
					return "INV_DOC";
				}
			}
			return SYConstant.OK;
		}
		public string OnGridModifyDisciplinary(HREmpDisciplinary MModel, string Action, bool IsEdit = false, bool IsAddNewFile = false)
		{
			if (Action == "ADD")
			{
				ListEmpDisciplinary.Add(MModel);
			}
			else if (Action == "EDIT")
			{
				var objCheck = ListEmpDisciplinary.Where(w => w.TranNo == MModel.TranNo).FirstOrDefault();
				if (objCheck != null)
				{
					objCheck.DiscType = MModel.DiscType;
					objCheck.TranDate = MModel.TranDate;
					objCheck.DiscAction = MModel.DiscAction;
					objCheck.Remark = MModel.Remark;
					objCheck.Reference = MModel.Reference;
					objCheck.Consequence = MModel.Consequence;
					string temppath = objCheck.AttachPath;
					if (IsEdit)
					{
						if (!string.IsNullOrWhiteSpace(MModel.AttachPath))
						{
							if (!string.IsNullOrWhiteSpace(objCheck.AttachPath))
							{
								var existingFiles = objCheck.AttachPath.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
								var newFiles = MModel.AttachPath.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

								if (IsAddNewFile)
								{
									foreach (var file in newFiles)
									{
										if (!existingFiles.Contains(file, StringComparer.OrdinalIgnoreCase))
										{
											existingFiles.Add(file);
										}
									}
									objCheck.AttachPath = string.Join(";", existingFiles);
								}
								else
								{
									existingFiles = newFiles;
									objCheck.AttachPath = string.Join(";", existingFiles);
								}
							}
							else
							{
								objCheck.AttachPath = MModel.AttachPath;
							}
						}
						else
						{
							objCheck.AttachPath = string.Empty;
						}
						//objCheck.AttachFile = MModel.AttachFile;
						if (objCheck.TranNo == 0)
						{
							DeleteFile(temppath, objCheck.AttachPath);
						}
					}

				}
			}
			else if (Action == "DELETE")
			{
				var objCheck = ListEmpDisciplinary.Where(w => w.TranNo == MModel.TranNo).FirstOrDefault();
				if (objCheck != null)
				{
					ListEmpDisciplinary.Remove(objCheck);
					if (objCheck.TranNo == 0)
					{
						DeleteFile(objCheck.AttachPath);
					}
					return SYConstant.OK;
				}
				else
				{
					return "INV_DOC";
				}
			}
			return SYConstant.OK;
		}
		public string OnGridModifyCertificate(HREmpCertificate MModel, string Action, bool IsEdit = false,bool IsAddNewFile = false)
		{
			if (Action == "ADD")
			{
				ListEmpCertificate.Add(MModel);
			}
			else if (Action == "EDIT")
			{
				var objCheck = ListEmpCertificate.Where(w => w.TranNo == MModel.TranNo).FirstOrDefault();
				if (objCheck != null)
				{
					objCheck.CertType = MModel.CertType;
					objCheck.IssueDate = MModel.IssueDate;
					objCheck.Description = MModel.Description;
					objCheck.Remark = MModel.Remark;
					string temppath = objCheck.AttachFile;
					if (IsEdit)
					{
						if (!string.IsNullOrWhiteSpace(MModel.AttachFile))
						{
							if (!string.IsNullOrWhiteSpace(objCheck.AttachFile))
							{
								var existingFiles = objCheck.AttachFile.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
								var newFiles = MModel.AttachFile.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

								if (IsAddNewFile)
								{
									foreach (var file in newFiles)
									{
										if (!existingFiles.Contains(file, StringComparer.OrdinalIgnoreCase))
										{
											existingFiles.Add(file);
										}
									}
									objCheck.AttachFile = string.Join(";", existingFiles);
								}
								else
								{
									existingFiles = newFiles;
									objCheck.AttachFile = string.Join(";", existingFiles);
								}
							}
							else
							{
								objCheck.AttachFile = MModel.AttachFile;
							}
						}
						else
						{
							objCheck.AttachFile = string.Empty;
						}
						//objCheck.AttachFile = MModel.AttachFile;
						if (objCheck.TranNo == 0)
						{
							DeleteFile(temppath, objCheck.AttachFile);
						}
					}

				}
			}
			else if (Action == "DELETE")
			{
				var objCheck = ListEmpCertificate.Where(w => w.TranNo == MModel.TranNo).FirstOrDefault();
				if (objCheck != null)
				{
					ListEmpCertificate.Remove(objCheck);
					if (objCheck.TranNo == 0)
					{
						DeleteFile(objCheck.AttachFile);
					}
					return SYConstant.OK;
				}
				else
				{
					return "INV_DOC";
				}
			}
			return SYConstant.OK;
		}
		public string OnGridModifyMedical(HREmpPhischk MModel, string Action, bool IsEdit = false)
		{
			if (Action == "ADD")
			{
				ListEmpMedical.Add(MModel);
			}
			else if (Action == "EDIT")
			{
				var objCheck = ListEmpMedical.Where(w => w.TranNo == MModel.TranNo).FirstOrDefault();
				if (objCheck != null)
				{
					objCheck.CHKDate = MModel.CHKDate;
					objCheck.MedicalType = MModel.MedicalType;
					objCheck.HospCHK = MModel.HospCHK;
					objCheck.Result = MModel.Result;
					objCheck.Description = MModel.Description;
					objCheck.Remark = MModel.Remark;
				}
			}
			else if (Action == "DELETE")
			{
				var objCheck = ListEmpMedical.Where(w => w.TranNo == MModel.TranNo).FirstOrDefault();
				if (objCheck != null)
				{
					ListEmpMedical.Remove(objCheck);
					return SYConstant.OK;
				}
				else
				{
					return "INV_DOC";
				}
			}
			return SYConstant.OK;
		}
		public string CreateMulti()
        {
            OnLoad();
            if (ListStaffProfile == null)
            {
                ListStaffProfile = new List<HRStaffProfile>();
            }
            if (ListEmpIdentityUP == null)
            {
                ListEmpIdentityUP = new List<HREmpIdentity>();
            }
            var ListLevel = unitOfWork.Set<HRLevel>().ToList();
            var listBranch = unitOfWork.Set<HRBranch>().ToList();
            var ListProType = unitOfWork.Set<HRProbationType>().ToList();
            try
            {
                var FIRST_LASTNAME = SYSettings.getSetting("FIRST_LASTNAME");
                var FIRST_LASTNAME_KH = SYSettings.getSetting("FIRST_LASTNAME_KH");
                foreach (HRStaffProfile staff in ListStaffProfile)
                {
                    MessageError = staff.EmpCode;
                    Header = staff;
                    if (Header.LeaveConf == null) Header.LeaveConf = Header.StartDate;
                    if (string.IsNullOrEmpty(Header.Branch) || listBranch.Where(w => w.Code == Header.Branch).ToList().Count == 0)
                    {
                        return "BRANCH_EN";
                    }
                    if (string.IsNullOrEmpty(Header.LevelCode) || ListLevel.Where(w => w.Code == Header.LevelCode).ToList().Count == 0)
                    {
                        return "LEVEL_EN";
                    }
                    Header.AllName = Header.FirstName + " " + Header.LastName;
                    Header.OthAllName = Header.OthFirstName + " " + Header.OthLastName;
                    if (FIRST_LASTNAME != null)
                    {
                        if (FIRST_LASTNAME.SettinValue != "TRUE") Header.AllName = Header.LastName + " " + Header.FirstName;
                    }
                    if (FIRST_LASTNAME_KH != null)
                    {
                        if (FIRST_LASTNAME_KH.SettinValue != "TRUE") Header.OthAllName = Header.OthLastName + " " + Header.OthFirstName;
                    }
                    if (string.IsNullOrEmpty(Header.BankName))
                    {
                        Header.BankName = "CC";
                    }
                    if (string.IsNullOrEmpty(Header.Currency)) Header.Currency = "USD";
                    Header.PayParam = "DEFAULT";
                    Header.CareerDesc = "NEWJOIN";
                    Header.TemLeave = "LB";
                    Header.CreatedOn = DateTime.Now.Date;
                    Header.CreatedBy = User.UserName;
                    Header.DateTerminate = new DateTime(1900, 1, 1);
                    Header.Status = SYDocumentStatus.A.ToString();
                    if (string.IsNullOrEmpty(Header.SalaryType)) Header.SalaryType = "M";
                    if (string.IsNullOrEmpty(Header.TemLeave)) Header.TemLeave = "LB";
                    if (string.IsNullOrEmpty(Header.PayParam)) Header.PayParam = "DEFAULT";
                    if (string.IsNullOrEmpty(Header.CareerDesc)) Header.CareerDesc = "NEWJOIN";
                    var Pro = ListProType.FirstOrDefault(w => w.Code == Header.ProbationType);
                    if (Pro != null)
                    {
                        int Months = 0;
                        int day = 0;
                        if (Pro != null)
                        {
                            Months = Pro.InMonth;
                            day = Pro.Day;
                        }
                        DateTime Probation = Header.StartDate.Value.AddMonths(Months).AddDays(day);
                        Header.Probation = Probation;
                        Header.ProbationType = Header.ProbationType;
                    }
                    else
                    {
                        Header.Probation = Header.StartDate.Value.AddMonths(1).AddDays(1);
                        Header.ProbationType = "PRO01";
                    }
                    Header.EffectDate = Header.StartDate;
                    HeaderCareer = new HREmpCareer();
                    SwapStaffCarrer(HeaderCareer, Header);
                    var Iden = ListEmpIdentityUP.Where(w => w.EmpCode == Header.EmpCode).ToList();
                    int LinItem = 0;
                    foreach (var item in Iden)
                    {
                        item.IdentityTye = "IDCard";
                        LinItem += 1;
                        var empIden = new HREmpIdentity();
                        SwapEmpIdentity(empIden, item, Header);
                        empIden.LineItem = LinItem;
                        unitOfWork.Add(empIden);
                    }

                    unitOfWork.Add(HeaderCareer);
                    unitOfWork.Add(Header);
                    unitOfWork.Save();
                }
                return SYConstant.OK;
            }
            catch (DbEntityValidationException e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, Header.EmpCode, SYActionBehavior.ADD.ToString(), e, true);
            }
            catch (DbUpdateException e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, Header.EmpCode, SYActionBehavior.ADD.ToString(), e, true);
            }
            catch (Exception e)
            {
                return ClsEventLog.Save_EventLog(ScreenId, User.UserName, Header.EmpCode, SYActionBehavior.ADD.ToString(), e, true);
            }
        }

        #region Province
        public static IEnumerable<OBDesc> District()
        {
            HumicaDBContext DBB = new HumicaDBContext();
            List<OBDesc> _List = new List<OBDesc>();
            if (HttpContext.Current.Session["S_PROVINCE"] != null)
            {
                string ProvinceCode = HttpContext.Current.Session["S_PROVINCE"].ToString();
                _List = DBB.OBDescs.SqlQuery("SELECT distinct District as Code,DistrictDesc1 as Desc1 ,DistrictDesc2 as Desc2 FROM CFPostalCode WHERE Province=" + ProvinceCode).ToList();

            }
            return _List;
        }
        public static IEnumerable<OBDesc> Commune()
        {
            HumicaDBContext DBB = new HumicaDBContext();
            List<OBDesc> _List = new List<OBDesc>();
            if (HttpContext.Current.Session["DISTRICT"] != null)
            {
                string DistrictCode = HttpContext.Current.Session["DISTRICT"].ToString();
                _List = DBB.OBDescs.SqlQuery("SELECT distinct Commune as Code,CommuneDesc1 as Desc1 ,CommuneDesc2 as Desc2 FROM CFPostalCode WHERE District=" + DistrictCode).ToList();
            }
            return _List;
        }
        public static IEnumerable<OBDesc> Village()
        {
            HumicaDBContext DBB = new HumicaDBContext();
            List<OBDesc> _List = new List<OBDesc>();
            if (HttpContext.Current.Session["COMMUNE"] != null)
            {
                string CommuneCode = HttpContext.Current.Session["COMMUNE"].ToString();
                _List = DBB.OBDescs.SqlQuery("SELECT distinct Village as Code,VillageDesc1 as Desc1 ,VillageDesc2 as Desc2 FROM CFPostalCode WHERE Commune=" + CommuneCode).ToList();
            }
            return _List;
        }
        #endregion

        public string DeleteFile(string FilePath, string NewFilePath = ";")
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                return SYConstant.OK;
            }
            string root = HostingEnvironment.ApplicationPhysicalPath;
            string[] path = FilePath.Split(';');
            string[] newPaths = NewFilePath.Split(';');
            foreach (var pathItem in path)
            {
                if (!newPaths.Contains(pathItem))
                {
                    string fileNameDelete = root + pathItem;
                    fileNameDelete = fileNameDelete.Replace("~", "").Replace("/", "\\");
                    if (System.IO.File.Exists(fileNameDelete))
                    {
                        System.IO.File.Delete(fileNameDelete);
                    }
                }
            }
            return SYConstant.OK;
        }
        public DateTime GetProbationDate(DateTime StartDate, string ProType)
        {
            DateTime ProbationDate = DateTime.Now;
            var objProbation = unitOfWork.Set<HRProbationType>().FirstOrDefault(w => w.Code == ProType);
            int Months = 0;
            int day = 0;
            if (objProbation != null)
            {
                Months = objProbation.InMonth;
                day = objProbation.Day;
            }
            ProbationDate = StartDate.AddMonths(Months).AddDays(day);
            return ProbationDate;
        }
        public bool IsHideSalary(string Level)
        {
            var ListLevel = unitOfWork.Set<SYHRModifySalary>().Where(w => w.UserName == User.UserName && w.Level == Level).ToList();
            if (ListLevel.Count > 0)
            {
                return true;
            }
            return false;
        }
        public Dictionary<string, dynamic> OnDataPersonal(params object[] keys)
        {
            Dictionary<string, dynamic> keyValues = new Dictionary<string, dynamic>();
            keyValues.Add("GENDER_SELECT", new SYDataList(unitOfWork, "SEX").ListData);
            keyValues.Add("INITIAL_SELECT", new SYDataList(unitOfWork, "INITIAL").ListData);
            keyValues.Add("MARITAL_SELECT", new SYDataList(unitOfWork, "MARITAL").ListData);
            keyValues.Add("COUNTRY_SELECT", unitOfWork.Set<HRCountry>().ToList().OrderBy(w => w.Description));
            keyValues.Add("NATION_SELECT", unitOfWork.Set<HRNation>().ToList().OrderBy(w => w.Description));
            keyValues.Add("PROVICES_SELECT", unitOfWork.Set<HRProvice>().ToList().OrderBy(w => w.Description));
            keyValues.Add("COSTCENT_LIST", unitOfWork.Set<PRCostCenterGroup>().ToList());
            keyValues.Add("TELEGRAM_SELECT", unitOfWork.Set<TelegramBot>().ToList());

            return keyValues;
        }
        public Dictionary<string, dynamic> OnDataJobLoading(params object[] keys)
        {
            ClsFilterJob clsFilter = new ClsFilterJob();
            ClsFilterStaff clsfStaff = new ClsFilterStaff();
            Dictionary<string, dynamic> keyValues = new Dictionary<string, dynamic>();
            keyValues.Add("Company_SELECT", SYConstant.getCompanyDataAccess());
            keyValues.Add("BUSINESSUNIT_SELECT", clsFilter.LoadBusinessUnit());
            keyValues.Add("BRANCHES_SELECT", clsFilter.LoadBranch());
            keyValues.Add("DIVISION_SELECT", clsFilter.LoadDivision());
            keyValues.Add("DEPARTMENT_SELECT", clsFilter.LoadDepartment());
            keyValues.Add("OFFICE_SELECT", clsFilter.LoadOffice());
            keyValues.Add("SECTION_SELECT", clsFilter.LoadSection());
            keyValues.Add("GROUP_SELECT", clsFilter.LoadGroups());
            keyValues.Add("POSITION_SELECT", clsFilter.LoadPosition());
            keyValues.Add("Level_SELECT", unitOfWork.Set<HRLevel>().ToList());
            keyValues.Add("JOBGRADE_SELECT", unitOfWork.Set<HRJobGrade>().ToList());
            keyValues.Add("LOCATION_SELECT", clsFilter.LoadLocation());
            keyValues.Add("CATEGORY_SELECT", unitOfWork.Set<HRCategory>().ToList());

            return keyValues;
        }
        public Dictionary<string, dynamic> OnDataLoadingOther(params object[] keys)
        {
            ClsFilterJob clsFilter = new ClsFilterJob();
            ClsFilterStaff clsfStaff = new ClsFilterStaff();
            Dictionary<string, dynamic> keyValues = new Dictionary<string, dynamic>();
            List<HRBankAccount> ListBankAccount = unitOfWork.Set<HRBankAccount>().ToList();
            var ListBank = ListBankAccount.Select(w => w.Bank).ToList();
            ListBank.Add("CC");
            keyValues.Add("LINE_SELECT", unitOfWork.Set<HRLine>().ToList());
            keyValues.Add("STAFFTYPE_SELECT", unitOfWork.Set<HRWorkingType>().ToList());
            keyValues.Add("EMPTYPE_SELECT", unitOfWork.Set<HREmpType>().ToList().OrderBy(w => w.Description));
            //keyValues.Add("IdentityTye_SELECT", unitOfWork.Set<HRIdentityType>().ToList());
			keyValues.Add("IdentityTye_SELECT", new SYDataList(unitOfWork, "IdentityTye").ListData);
			keyValues.Add("STAFF_SELECT", clsfStaff.OnLoadStaff(true));
            keyValues.Add("TAXPAID_SELECT", new SYDataList(unitOfWork, "TAXPAID").ListData);
            keyValues.Add("SALARYTYPE_SELECT", new SYDataList(unitOfWork, "SALARYTYPE_SELECT").ListData);
            keyValues.Add("PERAMETER_SELECT", unitOfWork.Set<PRParameter>().ToList().OrderBy(w => w.Description));
            keyValues.Add("PROBATIONTYPE_SELECT", unitOfWork.Set<HRProbationType>().ToList());
            keyValues.Add("SetEntitle_SELECT", unitOfWork.Set<HRSetEntitleH>().ToList());
            keyValues.Add("ROSTER_SELECT", unitOfWork.Set<ATBatch>().ToList());
            keyValues.Add("SEX_LIST", unitOfWork.Set<SYData>().Where(w=>w.DropDownType=="SEX").ToList());
            keyValues.Add("BANK_SELECT", unitOfWork.Set<HRBank>().Where(w => w.IsActive == true
            && ListBank.Contains(w.Code)).ToList().OrderBy(w => w.Description));
            keyValues.Add("CURRENCY_SELECT", unitOfWork.Set<HRCurrency>().ToList());
            var Province = unitOfWork.Set<OBDesc>().SqlQuery("SELECT distinct Province as Code,ProvinceDesc1 as Desc1 ,ProvinceDesc2 as Desc2 FROM CFPostalCode");
            keyValues.Add("PROVINCE", Province.ToList());

            return keyValues;
        }
        public Dictionary<string, dynamic> OnDataFamily(params object[] keys)
        {
            Dictionary<string, dynamic> keyValues = new Dictionary<string, dynamic>();
            keyValues.Add("RelationshipType_LIST", unitOfWork.Set<HRRelationshipType>().ToList());
            keyValues.Add("SEX_LIST", new SYDataList(unitOfWork, "SEX").ListData);
            return keyValues;
        }
		public Dictionary<string, dynamic> OnDataEducation(params object[] keys)
		{
			Dictionary<string, dynamic> keyValues = new Dictionary<string, dynamic>();
			keyValues.Add("Education_LIST", unitOfWork.Set<HREduType>().ToList());
			return keyValues;
		}
		public Dictionary<string, dynamic> OnDataContract(params object[] keys)
		{
			Dictionary<string, dynamic> keyValues = new Dictionary<string, dynamic>();
			keyValues.Add("ContractType_LIST", unitOfWork.Set<HRContractType>().ToList());
			keyValues.Add("ContractConterm_LIST", new SYDataList(unitOfWork, "CONTRACT_TERM").ListData);
			return keyValues;
		}
		public Dictionary<string, dynamic> OnDataDisciplinary(params object[] keys)
		{
			Dictionary<string, dynamic> keyValues = new Dictionary<string, dynamic>();
			keyValues.Add("DISCIPLINAY_LIST", unitOfWork.Set<HRDisciplinType>().ToList());
			keyValues.Add("DISCIPLINACTION_SELECT", unitOfWork.Set<HRDisciplinAction>().ToList());
			return keyValues;
		}
		public Dictionary<string, dynamic> OnDataCertificate(params object[] keys)
		{
			Dictionary<string, dynamic> keyValues = new Dictionary<string, dynamic>();
			keyValues.Add("HREmpCertificate_LIST", unitOfWork.Set<HRCertificationType>().ToList());
			return keyValues;
		}
		public Dictionary<string, dynamic> OnDataMedical(params object[] keys)
		{
			Dictionary<string, dynamic> keyValues = new Dictionary<string, dynamic>();
			keyValues.Add("MEDICALTYPE_SELECT", unitOfWork.Set<HRMedicalType>().ToList());
			keyValues.Add("HOSP_SELECT", unitOfWork.Set<HRHospital>().ToList());
			keyValues.Add("PHCHKRESULT_SELECT", unitOfWork.Set<HRPHCKHResult>().ToList());
			return keyValues;
		}
		public Dictionary<string, dynamic> OnDataListLoading(params object[] keys)
        {
            Dictionary<string, dynamic> keyValues = new Dictionary<string, dynamic>();
            ClsFilterJob clsFilter = new ClsFilterJob();
            var objStatus = new SYDataList("STATUS_EMPLOYEE");
            keyValues.Add("STATUS_EMPLOYEE", objStatus.ListData);
            keyValues.Add("BRANCHES_SELECT", clsFilter.LoadBranch());
            keyValues.Add("DEPARTMENT_SELECT", clsFilter.LoadDepartment());
            keyValues.Add("BUSINESSUNIT_SELECT", clsFilter.LoadBusinessUnit());
            keyValues.Add("OFFICE_SELECT", clsFilter.LoadOffice());
            keyValues.Add("SECTION_SELECT", clsFilter.LoadSection());
            keyValues.Add("GROUP_SELECT", clsFilter.LoadGroups());

            return keyValues;
        }
        public string CopyPathPhoto(HRStaffProfile Staff,string DocType,string Image)
        {
            if (!string.IsNullOrEmpty(Image))
            {
                ClsCopyFile clsCopyFile = new ClsCopyFile();
                string savedFilePath;
                savedFilePath = clsCopyFile.CopyStructurePath(Staff.CompanyCode, DocType, Staff.EmpCode, Staff.DEPT, Image, true);

                return savedFilePath;
            }
            return null;
        }
		public void SwapStaffCarrer(HREmpCareer D,HRStaffProfile S)
        {
            D.EmpCode = S.EmpCode;
            D.CareerCode = S.CareerDesc;
            D.EmpType = S.EmpType;
            D.CompanyCode = S.CompanyCode;
            D.Branch = S.Branch;
            D.DEPT = S.DEPT;
            D.LOCT = S.LOCT;
            D.Division = S.Division;
            D.LINE = S.Line;
            D.SECT = S.SECT;
            D.CATE = S.CATE;
            D.LevelCode = S.LevelCode;
            D.JobCode = S.JobCode;
            D.JobGrade = S.JobGrade;
            D.JobDesc = S.POSTDESC;
            D.JobSpec = S.JOBSPEC;
            D.EstartSAL = S.Salary.ToString();
            D.EIncrease = S.Salary.ToString();
            D.ESalary = S.ESalary;
            D.SupCode = S.SubFunction;
            D.FromDate = S.StartDate;
            D.ToDate = Convert.ToDateTime("01/01/5000");
            D.EffectDate = S.EffectDate;
            D.ProDate = S.StartDate;
            D.Reason = "New Join";
            D.Remark = "Welcome to " + S.CompanyCode;
            D.Appby = "";
            D.AppDate = S.StartDate.Value.ToString("dd-MM-yyyy");
            D.VeriFyBy = "";
            D.VeriFYDate = S.StartDate.Value.ToString("dd-MM-yyyy");
            D.LCK = 1;
            D.OldSalary = S.Salary;
            D.Increase = 0;
            D.Functions = S.Functions;
            D.NewSalary = S.Salary;
            D.JobGrade = S.JobGrade;
            D.PersGrade = S.PersGrade;
            D.HomeFunction = S.HomeFunction;
            D.Functions = S.Functions;
            D.SubFunction = S.SubFunction;
            D.StaffType = S.StaffType;
            D.CreateBy = User.UserName;
            D.CreateOn = DateTime.Now;
            D.GroupDept = S.GroupDept;
            D.Office = S.Office;
            D.Groups = S.Groups;
        }
        public void SwapEmpIdentity(HREmpIdentity D, HREmpIdentity S,HRStaffProfile Staff)
        {
            D.CompanyCode = Staff.CompanyCode;
            D.EmpCode = Staff.EmpCode;
            D.IdentityTye = S.IdentityTye;
            D.IDCardNo = S.IDCardNo;
            D.IssueDate = S.IssueDate;
            D.ExpiryDate = S.ExpiryDate;
            D.Document = S.Document;
            D.IsActive = S.IsActive;
        }
        public void SwapEmpJD(HREmpJobDescription D, HREmpJobDescription S, HRStaffProfile Staff)
        {
            D.CompanyCode = Staff.CompanyCode;
            D.EmpCode = Staff.EmpCode;
            D.JobDescription = S.JobDescription;
            D.JobResponsive = S.JobResponsive;
            D.Document = S.Document;
            D.IsActive = S.IsActive;
        }

        public void SwapEmpFamily(HREmpFamily D, HREmpFamily S, HRStaffProfile Staff)
        {
            D.EmpCode = Staff.EmpCode;
            D.RelCode = S.RelCode;
            D.RelName = S.RelName;
            D.Sex = S.Sex;
            D.DateOfBirth = S.DateOfBirth;
            D.IDCard = S.IDCard;
            D.PhoneNo = S.PhoneNo;
            D.TaxDeduc = S.TaxDeduc;
            D.Occupation = S.Occupation;
            D.Child = S.Child;
            D.Spouse = S.Spouse;
            D.InSchool = S.InSchool;
            D.Document = S.Document;
        }
		public void SwapEmpEducation(HREmpEduc D, HREmpEduc S, HRStaffProfile Staff)
		{
			D.EmpCode = Staff.EmpCode;
			D.EduType = S.EduType;
			D.StartDate = S.StartDate;
			D.EndDate = S.EndDate;
			D.EdcCenter = S.EdcCenter;
			D.Major = S.Major;
			D.Result = S.Result;
			D.Remark = S.Remark;
			//D.AttachFile = S.AttachFile;
		}
		public void SwapEmpContract(HREmpContract D, HREmpContract S, HRStaffProfile Staff)
		{
			D.EmpCode = Staff.EmpCode;
			D.ConType = S.ConType;
			D.Conterm = S.Conterm;
			D.FromDate = S.FromDate;
			D.Description = S.Description;
			//D.ContractPath = S.ContractPath;
		}
		public void SwapEmpDisciplinary(HREmpDisciplinary D, HREmpDisciplinary S, HRStaffProfile Staff)
		{
			D.EmpCode = Staff.EmpCode;
			D.DiscType = S.DiscType;
			D.TranDate = S.TranDate;
			D.DiscAction = S.DiscAction;
			//D.AttachPath = S.AttachPath;
			D.Remark = S.Remark;
			D.Reference = S.Reference;
			D.Consequence = S.Consequence;
		}
		public void SwapEmpCertificate(HREmpCertificate D, HREmpCertificate S, HRStaffProfile Staff)
		{
			D.EmpCode = Staff.EmpCode;
			D.AllName = Staff.AllName;
			D.CertType = S.CertType;
			D.IssueDate = S.IssueDate;
			D.Description = S.Description;
			D.Remark = S.Remark;
		}
		public void SwapEmpMedical(HREmpPhischk D, HREmpPhischk S, HRStaffProfile Staff)
		{
			D.EmpCode = Staff.EmpCode;
			D.CHKDate = S.CHKDate;
			D.MedicalType = S.MedicalType;
			D.HospCHK = S.HospCHK;
			D.Result = S.Result;
			D.Description = S.Description;
			D.Remark = S.Remark;
		}
	}
}