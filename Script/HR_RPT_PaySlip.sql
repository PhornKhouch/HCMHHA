USE [HumicaHHA]
GO
/****** Object:  StoredProcedure [dbo].[HR_RPT_PaySlip]    Script Date: 5/24/2025 10:19:38 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROC [dbo].[HR_RPT_PaySlip] (@Company nvarchar(200),@Branch Nvarchar(200),@Division Nvarchar(100),@Department Nvarchar(100) ,@Section Nvarchar(100) ,@Position Nvarchar(100) ,@Level Nvarchar(100),@InMonth as Date,@EmpCode NVARCHAR(max))
as
begin
		SELECT     HISPaySlip.TranLine, HISPaySlip.EmpCode, HISPaySlip.INYear, HISPaySlip.INMonth, HISPaySlip.EarnDesc, 
		HISPaySlip.EAmount, HISPaySlip.DeductDesc, HISPaySlip.DeductAmount, HISGenSalary.EmpType, 
		HISGenSalary.Branch, HISGenSalary.Location, HISGENSALARY.Division, HISGENSALARY.DEPT, 
		HISGenSalary.LINE, HISGenSalary.CATE, HISGenSalary.Sect, HISGenSalary.POST, HISGenSalary.LevelCode, 
		HISGenSalary.SEX, HISGenSalary.TAXNO, HISGenSalary.SOCSO, HISGenSalary.BankName, 
		HISGenSalary.BankBranch, HISGenSalary.BankAcc, HISGenSalary.GrossPay, HISGenSalary.NetWage, 
		HRDepartment.Description AS Department, HISGenSalary.EmpName, HRStaffProfile.AllName, HRStaffProfile.OthAllName,
		HRBranch.Description ASBranch_DESC, HRPosition.Description AS POSITION,HRPosition.SecDescription AS SecPOSITION,HISGenSalary.JobGrade,
		HISGenSalary.TAXAM,HRStaffProfile.TerminateStatus TERMSTATUS
	FROM HISPaySlip INNER JOIN
	HISGenSalary ON HISPaySlip.INYEAR = HISGenSalary.INYEAR AND 
	HISPaySlip.INMONTH = HISGenSalary.INMONTH AND HISPaySlip.EMPCODE = HISGenSalary.EmpCode LEFT OUTER JOIN
	HRDepartment ON HISGenSalary.DEPT = HRDepartment.CODE INNER JOIN
	HRStaffProfile ON HISGENSALARY.EMPCODE = HRStaffProfile.EmpCode LEFT OUTER JOIN
	HRCompany ON HRCompany.Company=HRStaffProfile.CompanyCode LEFT OUTER JOIN
	HRBranch ON HISGenSalary.Branch = HRBRANCH.Code and HRBranch.CompanyCode=HRStaffProfile.CompanyCode LEFT OUTER JOIN
	HRPosition ON HISGenSalary.POST = HRPosition.Code 
	Where 
	   HISGenSalary.INMonth=MONTH(@InMonth) AND HISGenSalary.INYear=year(@InMonth) AND
	  (@Company IS NULL OR @Company='' OR  HISGenSalary.CompanyCode IN (SELECT Value FROM fn_Split(@Company, ','))) AND
	  (@Branch IS NULL OR @Branch='' OR  HISGenSalary.Branch IN (SELECT * FROM dbo.SplitString(@Branch, ',')))  AND 
	  (@EmpCode IS NULL OR @EmpCode='' OR  HISGenSalary.EmpCode=@EmpCode) AND
	  (@Division IS NULL OR @Division='' OR  HISGenSalary.Division=@Division) AND 
	  (@Department IS NULL OR @Department='' OR  HISGenSalary.DEPT=@Department) AND
	  (@Section IS NULL OR @Section='' OR  HISGenSalary.Sect=@Section) AND 
	  (@Position IS NULL OR @Position='' OR  HISGenSalary.POST=@Position) AND
	  (@Level IS NULL OR @Level='' OR HISGenSalary.LevelCode=@Level)AND 
	  (@EmpCode IS NULL OR @EmpCode='' OR HISGenSalary.EmpCode IN (SELECT * FROM dbo.SplitString(@EmpCode, ','))) 
END

