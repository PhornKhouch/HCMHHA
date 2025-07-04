USE [HumicaHHA]
GO
/****** Object:  StoredProcedure [dbo].[HR_RPT_EmpDeduction]    Script Date: 5/24/2025 9:50:20 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROC [dbo].[HR_RPT_EmpDeduction] (@Company nvarchar(200),@Branch Nvarchar(200),@Division Nvarchar(100),@Department Nvarchar(100) ,@Section Nvarchar(100) ,@Position Nvarchar(100) ,@Level Nvarchar(100),@InMonth as Date)
as
begin
		SELECT     HRStaffProfile.AllName,HRStaffProfile.OthAllName, HRStaffProfile.StartDate,HRStaffProfile.Title, HRStaffProfile.Sex, 
							  HRPosition.Description AS Position,HRPosition.SecDescription AS SecPosition, HRDepartment.Description AS Department,HRBranch.Description AS BranchName, 
							  HISGenDeduction.FromDate, HISGenDeduction.ToDate, HISGenDeduction.WorkDay, HISGenDeduction.RatePerDay, 
							  HISGenDeduction.DedCode, PR_RewardsType.Description, PR_RewardsType.OthDesc, PR_RewardsType.Tax, 
							  HISGenDeduction.DedAm, HISGenDeduction.DedAMPM, HISGenSalary.Branch, HISGenSalary.EmpType, 
							  HISGenSalary.Location, HISGenSalary.Division, HISGenSalary.DEPT, HISGenSalary.LINE, HISGenSalary.CATE, 
							  HISGenSalary.Sect, HISGenSalary.JobGrade, HISGenSalary.LevelCode, HRStaffProfile.EmpCode, HISGenDeduction.INMonth, 
							  HISGenDeduction.INYear, HRStaffProfile.TerminateStatus, HRSection.Description AS Section, HRStaffProfile.Salary, 
							  HISGenDeduction.Remark
		FROM        HRBranch RIGHT OUTER JOIN
							  PR_RewardsType INNER JOIN
							  HISGenDeduction ON PR_RewardsType.Code = HISGenDeduction.DedCode INNER JOIN
							  HISGenSalary ON HISGenDeduction.INYear = HISGenSalary.INYear AND 
							  HISGenDeduction.INMonth = HISGenSalary.INMonth AND HISGenDeduction.EmpCode = HISGenSalary.EmpCode
							  INNER JOIN HRStaffProfile ON HISGenDeduction.EMPCODE = HRStaffProfile.EMPCODE LEFT OUTER JOIN
							  HRSection ON HISGenSalary.Sect = HRSection.CODE ON HRBranch.Code = HISGenSalary.BRANCH LEFT OUTER JOIN
							  HRPosition ON HISGenSalary.POST =HRPosition.CODE LEFT OUTER JOIN
							  HRDepartment ON HISGenSalary.DEPT = HRDepartment.CODE LEFT OUTER JOIN
							  HRCompany ON HRCompany.Company=HRStaffProfile.CompanyCode
		Where HISGenDeduction.INYear=YEAR(@InMonth) AND HISGenDeduction.INMonth=MONTH(@InMonth) AND HRStaffProfile.IsHold!=1 and
		(@Company IS NULL OR @Company='' OR  HISGenSalary.CompanyCode IN (SELECT Value FROM fn_Split(@Company, ','))) AND
		(@Branch IS NULL OR @Branch='' OR  HISGenSalary.Branch IN (SELECT Value FROM fn_Split(@Branch, ','))) AND 
		(@Division IS NULL OR @Division='' OR  HISGenSalary.Division=@Division) AND 
		(@Department IS NULL OR @Department='' OR  HISGenSalary.DEPT=@Department) AND
		(@Section IS NULL OR @Section='' OR  HISGenSalary.Sect=@Section) AND 
		(@Position IS NULL OR @Position='' OR  HISGenSalary.POST=@Position) AND
		(@Level IS NULL OR @Level='' OR HISGenSalary.LevelCode=@Level)
END

