
ALTER VIEW [dbo].[HR_STAFF_VIEW] AS
SELECT SP.CompanyCode,SP.EmpCode,SP.AllName,SP.OthAllName,SP.DOB,SP.Sex,SP.StartDate,SP.Probation,SP.Phone1,
SP.EmpType,SP.Division,SP.JobCode,SP.LevelCode,CardNo,SP.Title,
D.Description AS Department,SE.Description AS Section,P.Description AS Position,L.Description AS Level,ET.Description AS EmployeeType,SP.TerminateStatus,sp.PayParam,sp.Branch as BranchID
,SP.Peraddress,(Case when Year(SP.DateTerminate) = 1900 then null else sp.DateTerminate end) DateTerminate,
HRBranch.Description Branch,PRParameter.Description AS PayParameter,PostFamily,DEPT,HRDivision.Description DivisionDesc,
LO.Description as Location,SP.LOCT,HRNation.Description as Nationality,
SP.PeraddressOth,SP.BankBranch,HRBank.Description as BankName,SP.BankAccName,SP.BankAcc,SP.SOCSO as NSSF,
(CASE WHEN SP.TXPayType='C' THEN 'Net Pay - company paid tax' ELSE ( 
 Case WHEN SP.TXPayType='S' THEN 'Gross pay - employee paid tax' ELSE '' END) END) TXPayType,dbo.GET_Services_Lenght_String_EN(SP.StartDate,GETDATE()) ServicesLenght,
(CASE WHEN SP.Status='A' then 'Active' else 'Inactive' END) Status,Status AS StatusCode,
(SELECT top 1 EI.IDCardNo FROM HREmpIdentity EI WHERE EI.EmpCode=SP.EmpCode AND IdentityTye='IDCard') AS IDCard,
(SELECT top 1 EI.IssueDate FROM HREmpIdentity EI WHERE EI.EmpCode=SP.EmpCode AND IdentityTye='IDCard') AS IDCard_IssueDate,
(SELECT top 1 EI.ExpiryDate FROM HREmpIdentity EI WHERE EI.EmpCode=SP.EmpCode AND IdentityTye='IDCard') AS IDCard_ExpiryDate,
(SELECT top 1 EI.IDCardNo FROM HREmpIdentity EI WHERE EI.EmpCode=SP.EmpCode AND IdentityTye='PassportNo') AS PassportNo,
(CASE WHEN SP.Sex='M' then N'ប្រុស' else N'ស្រី' end) SexKH,DATEDIFF(year,DOB,GETDATE()) Age,
 (select top 1 ProvinceDesc2 from CFPostalCode where Province=SP.Province) Province,
(select top 1 DistrictDesc2 from CFPostalCode where District=SP.District) District,
(select top 1 CommuneDesc2 from CFPostalCode where Commune=SP.Commune) Commune,
(select top 1 VillageDesc2 from CFPostalCode where Village=SP.Village) Village,
(select Count(EmpCode) from HREmpDisciplinary ED where ED.EmpCode=SP.EmpCode) Warning,SP.IsHold,
HRBranch.SecDescription AS SecBranch,D.SecDescription AS SecDepartment,LO.OthDesc as SecLocation,
D.SecDescription AS SecDivision,P.SecDescription AS SecPostion,SP.Email,SP.TeleChartID,
SP.OpenDateShirt,IsIntegrate,
(CASE WHEN SP.Title='Mr' then N'លោក' when sp.Title='Madam' then N'លោកស្រី'
When sp.Title='Miss' then N'កញ្ញា' When sp.Title='Ms' then N'អ្នកស្រី' else N'អ្នកស្រី' end) TitleKH,IsPayPartial,
HODCode,FirstLine,SecondLine,FirstLine2,SecondLine2
FROM  HRStaffProfile SP 
LEFT JOIN HRLocation LO ON LO.Code=SP.LOCT
LEFT JOIN HRDepartment D ON SP.DEPT=D.Code
LEFT JOIN HRPosition P ON SP.JobCode=P.Code
LEFT JOIN HRLevel L ON SP.LevelCode=L.Code 
LEFT JOIN HREmpType ET ON SP.EmpType=ET.Code
LEFT JOIN HRSection SE ON SP.SECT=SE.Code
Left Join HRCompany ON HRCompany.Company=SP.CompanyCode
LEFT JOIN HRBranch ON HRBranch.Code=SP.Branch and HRBranch.CompanyCode=SP.CompanyCode
LEFT JOIN PRParameter ON PRParameter.Code=SP.PayParam
LEFT JOIN HRDivision ON HRDivision.Code=SP.Division
LEFT JOIN HRBank on HRBank.Code=SP.BankName
LEFT JOIN HRNation on HRNation.Code=SP.Nation
GO


