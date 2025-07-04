USE [HumicaHHA]
GO
/****** Object:  StoredProcedure [dbo].[HR_RPT_EmpBonus]    Script Date: 5/24/2025 9:34:34 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROC [dbo].[HR_RPT_EmpBonus] (@Company nvarchar(200),@Branch Nvarchar(200),@Division Nvarchar(100),@Department Nvarchar(100) ,@Section Nvarchar(100) ,@Position Nvarchar(100) ,@Level Nvarchar(100),@InMonth as Date)
as
begin
		SELECT     HRStaffProfile.AllName,HRStaffProfile.OthAllName, HRStaffProfile.StartDate,HRStaffProfile.Title, HRStaffProfile.Sex, 
							 HRPosition.Description AS Position,HRPosition.SecDescription AS SecPosition, HRDepartment.Description AS Department,HRBranch.Description AS BranchName, 
							 -- HISGenBonus.FromDate, HISGenBonus.ToDate, HISGenBonus.WorkDay, HISGenBonus.RatePerDay, 
							  HISGenBonus.BonusCode, PR_RewardsType.Description, PR_RewardsType.OthDesc, PR_RewardsType.Tax, 
							  HISGenBonus.BonusAM, HISGenSalary.Branch, HISGenSalary.EmpType, 
							  HISGenSalary.Location, HISGenSalary.Division, HISGenSalary.DEPT, HISGenSalary.LINE, HISGenSalary.CATE, 
							  HISGenSalary.Sect, HISGenSalary.JobGrade, HISGenSalary.LevelCode, HRStaffProfile.EmpCode, HISGenBonus.INMonth, 
							  HISGenBonus.INYear, HRStaffProfile.TerminateStatus, HRSection.Description AS Section, HRStaffProfile.Salary, 
							  HISGenBonus.Remark
		FROM        HRBranch RIGHT OUTER JOIN
							  PR_RewardsType INNER JOIN
							  HISGenBonus ON PR_RewardsType.Code = HISGenBonus.BonusCode INNER JOIN
							  HISGenSalary ON HISGenBonus.INYear = HISGenSalary.INYear AND 
							  HISGenBonus.INMonth = HISGenSalary.INMonth AND HISGenBonus.EmpCode = HISGenSalary.EmpCode
							  INNER JOIN HRStaffProfile ON HISGenBonus.EMPCODE = HRStaffProfile.EMPCODE LEFT OUTER JOIN
							  HRSection ON HISGenSalary.Sect = HRSection.CODE ON HRBranch.Code = HISGenSalary.BRANCH LEFT OUTER JOIN
							  HRPosition ON HISGenSalary.POST =HRPosition.CODE LEFT OUTER JOIN
							  HRDepartment ON HISGenSalary.DEPT = HRDepartment.CODE 
							  LEFT Join HRCompany ON HRCompany.Company=HISGenSalary.CompanyCode
		Where HISGenBonus.INYear=YEAR(@InMonth) AND HISGenBonus.INMonth=MONTH(@InMonth) AND HRStaffProfile.IsHold!=1 and
		(@Company IS NULL OR @Company='' OR HISGenSalary.CompanyCode IN (SELECT Value FROM fn_Split(@Company, ','))) AND
		(@Branch IS NULL OR @Branch='' OR  HISGenSalary.Branch IN (SELECT Value FROM fn_Split(@Branch, ','))) AND 
		(@Division IS NULL OR @Division='' OR  HISGenSalary.Division=@Division) AND 
		(@Department IS NULL OR @Department='' OR  HISGenSalary.DEPT=@Department) AND
		(@Section IS NULL OR @Section='' OR  HISGenSalary.Sect=@Section) AND 
		(@Position IS NULL OR @Position='' OR  HISGenSalary.POST=@Position) AND
		(@Level IS NULL OR @Level='' OR HISGenSalary.LevelCode=@Level)
END

