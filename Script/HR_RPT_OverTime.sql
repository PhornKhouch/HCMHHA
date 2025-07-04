USE [HumicaHHA]
GO
/****** Object:  StoredProcedure [dbo].[HR_RPT_OverTime]    Script Date: 5/24/2025 11:01:23 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROC [dbo].[HR_RPT_OverTime] (@Company nvarchar(200),@Branch Nvarchar(200),@Division Nvarchar(100),@Department Nvarchar(100) 
,@Section Nvarchar(100) ,@Position Nvarchar(100) ,@Level Nvarchar(100),@InMonth as Date)
as
begin
SELECT     HRStaffProfile.AllName,HRStaffProfile.OthAllName, HRStaffProfile.StartDate,HRStaffProfile.Title, HRStaffProfile.Sex, 
		HRPosition.Description AS Position,HRPosition.SecDescription AS SecPosition, HRDepartment.Description AS Department,HRBranch.Description AS BranchName, 
		OT.OTCode, OT.OTDesc,OT.OTDate,OT.OTHour,OT.OTRate,OT.Amount,HISGenSalary.Branch, HISGenSalary.EmpType, 
		HISGenSalary.Location, HISGenSalary.Division, HISGenSalary.DEPT, HISGenSalary.LINE, HISGenSalary.CATE, 
		HISGenSalary.Sect, HISGenSalary.JobGrade, HISGenSalary.LevelCode, HRStaffProfile.EmpCode, OT.INMonth, 
		OT.INYear, HRStaffProfile.TerminateStatus, HRSection.Description AS Section, HRStaffProfile.Salary
FROM  HISGenOTHour OT INNER JOIN
    HISGenSalary ON OT.INYear = HISGenSalary.INYear AND 
		OT.INMonth = HISGenSalary.INMonth AND OT.EmpCode = HISGenSalary.EmpCode
		INNER JOIN HRStaffProfile ON OT.EMPCODE = HRStaffProfile.EMPCODE 
		LEFT OUTER JOIN HRSection ON HISGenSalary.Sect = HRSection.CODE
		LEFT Join HRCompany ON HRCompany.Company=HRStaffProfile.CompanyCode
		LEFT JOIN HRBranch ON HRBranch.Code = HISGenSalary.BRANCH and HRBranch.CompanyCode=HRStaffProfile.CompanyCode
		LEFT OUTER JOIN HRPosition ON HISGenSalary.POST =HRPosition.CODE 
		LEFT OUTER JOIN HRDepartment ON HISGenSalary.DEPT = HRDepartment.CODE 
		Where OT.INYear=YEAR(@InMonth) AND OT.INMonth=MONTH(@InMonth) AND 
		(@Company IS NULL OR @Company='' OR  HISGenSalary.CompanyCode IN (SELECT Value FROM fn_Split(@Company, ','))) AND
		(@Branch IS NULL OR @Branch='' OR  HISGenSalary.Branch IN (SELECT Value FROM fn_Split(@Branch, ','))) AND 
		(@Division IS NULL OR @Division='' OR  HISGenSalary.Division=@Division) AND 
		(@Department IS NULL OR @Department='' OR  HISGenSalary.DEPT=@Department) AND
		(@Section IS NULL OR @Section='' OR  HISGenSalary.Sect=@Section) AND 
		(@Position IS NULL OR @Position='' OR  HISGenSalary.POST=@Position) AND
		(@Level IS NULL OR @Level='' OR HISGenSalary.LevelCode=@Level)
END

