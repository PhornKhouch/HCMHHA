USE [HumicaHHA]
GO
/****** Object:  StoredProcedure [dbo].[HR_RPT_FringBenifit]    Script Date: 5/24/2025 10:06:57 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROC [dbo].[HR_RPT_FringBenifit] (@Company nvarchar(200),@Branch Nvarchar(200),@Division Nvarchar(100),@Department Nvarchar(100) ,@Section Nvarchar(100) ,
                                   @Position Nvarchar(100) ,@Level Nvarchar(100),@InMonth as Date) AS
begin
SELECT G.EmpCode, EMP.AllName Employee,EMP.OthAllName EmployeeKH,EMP.Sex,
	EMP.StartDate,G.DEPT as Department,P.Description AS Position,P.SecDescription AS SecPosition,G.INYear,G.INMonth,G.Salary,G.ActWorkDay, G.FRINGAM,G.WorkDay,N.SecDescription Nationality,
	G.NetWage, G.AMFRINGTAX,HRCompany.Images Com_Image,HRBranch.Description BranchDesc,HRBranch.Address As Com_Address,
	 G.FromDate,G.ToDate, AlwBeforTax


	FROM  HISGenSalary G 
	INNER JOIN HRStaffProfile EMP ON EMP.EmpCode=G.EmpCode
	LEFT OUTER JOIN HRDepartment D ON D.Code=G.DEPT
	LEFT JOIN HRNation N  ON N.Code= EMP.Nation
	LEFT OUTER JOIN HRPosition P ON P.Code=G.POST
	LEFT Join HRCompany ON HRCompany.Company=G.CompanyCode
	LEFT OUTER JOIN HRBranch ON HRBranch.Code=G.Branch and HRBranch.CompanyCode=EMP.CompanyCode
	
	Where G.INYear=YEAR(@InMonth) AND G.INMonth=MONTH(@InMonth) AND EMP.IsHold!=1 and G.FRINGAM>0 AND
	(@Company IS NULL OR @Company='' OR  G.CompanyCode IN (SELECT Value FROM fn_Split(@Company, ','))) AND
	(G.Branch IN (SELECT Code FROM HRBranch WHERE CODE IN	(SELECT Value FROM fn_Split(@Branch, ',')))) AND 
	(@Division IS NULL OR @Division='' OR  G.Division=@Division) AND 
	(@Department IS NULL OR @Department='' OR  G.DEPT=@Department) AND
	(@Section IS NULL OR @Section='' OR  G.Sect=@Section) AND 
	(@Position IS NULL OR @Position='' OR  G.POST=@Position)

END

