ALTER View [dbo].[NSSF_Latter]
as
SELECT HRCompany.SecDescription CompKHM,COM.CompAct CompAct,COM.DirName,HRCompany.Phone,COM.HDStreet,COM.HDCommune,COM.HDDistrict,COM.HDProvince,
  HRCompany.Email,COM.FAX,SUM(ISNULL(StaffHealth,0) + ISNUll(CompHealth,0)) Health ,SUM(isnull(SOSEC,0)) SOSEC ,
  SUM(ISNULL(StaffPensionFundAmountKH,0) + ISNUll(CompanyPensionFundAmountKH,0)) PensionFund,COUNT(M.Sex) Count_Sex,
  SUM((CASE WHEN M.Sex='Female' OR M.Sex='F' then 1 else 0 end )) Count_Female,HRCompany.NSSFNo,G.INYear,G.INMonth,G.Branch,
  G.FromDate,G.ToDate,COM.HDHouse,COM.DirPositon
FROM HISGenSalary G inner join HRStaffProfile M oN G.EmpCode=M.EmpCode
  INNER JOIN HRCompany on HRCompany.Company=M.CompanyCode
  INNER JOIN HRBranch ON HRBranch.Code=G.Branch and HRBranch.CompanyCode=G.CompanyCode
  CROSS JOIN SYHRCompany COM
WHERE M.ISNSSF=1 
GROUP BY HRCompany.SecDescription,COM.CompAct,COM.DirName,HRCompany.Phone,COM.HDStreet,COM.HDCommune,COM.HDDistrict,
  COM.HDProvince,HRCompany.Email,COM.FAX,G.FromDate,G.ToDate,HRCompany.NSSFNo,G.INYear,G.INMonth,G.Branch,COM.HDHouse,COM.DirPositon

GO