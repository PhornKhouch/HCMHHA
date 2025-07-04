USE [HumicaHHA]
GO
/****** Object:  StoredProcedure [dbo].[HR_PRT_OTPLAN]    Script Date: 5/24/2025 8:45:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROC [dbo].[HR_PRT_OTPLAN] (@Company nvarchar(200),@Branch Nvarchar(200),@Division Nvarchar(200),@Location nvarchar(100),@Department Nvarchar(200) ,
@Section Nvarchar(200) ,@Position Nvarchar(200),@Level Nvarchar(200),@FromDate as Date,@ToDate as date,@EmpCode NVARCHAR(max)) as
Begin 
	select OT.EmpCode,M.AllName,M.OthAllName,D.Description as Department,P.SecDescription AS SecPosition,P.Description AS Position,OTStartTime,OTEndTime,Hours,BreakTime,
	(select OTStart from ATEmpSchedule AT where AT.EmpCode=OT.EmpCode and at.TranDate=CONVERT(DATE,OT.OTStartTime)) OTStartAct,
	(select OTEnd from ATEmpSchedule AT where AT.EmpCode=OT.EmpCode and at.TranDate=CONVERT(DATE,OT.OTStartTime)) OTEndAct,
	(select OTApproval from ATEmpSchedule AT where AT.EmpCode=OT.EmpCode and at.TranDate=CONVERT(DATE,OT.OTStartTime)) OTApproval,
	(select Shift from ATEmpSchedule AT where AT.EmpCode=OT.EmpCode and at.TranDate=CONVERT(DATE,OT.OTStartTime)) Shift
	from HRRequestOT OT
	inner join HRStaffProfile M ON M.EmpCode=OT.EmpCode
	inner join HRDepartment D ON D.Code=M.DEPT
	inner join HRPosition P ON P.Code=M.JobCode
    LEFT Join HRCompany ON HRCompany.Company=M.CompanyCode
    INNER join HRBranch on M.Branch=HRBranch.Code and HRBranch.CompanyCode=M.CompanyCode
	where CONVERT(DATE,OTStartTime) between @FromDate and @ToDate and OT.Status='APPROVED'
	AND (@Company IS NULL OR @Company='' OR  M.CompanyCode IN (SELECT Value FROM fn_Split(@Company, ','))) 
	AND (@EmpCode IS NULL OR @EmpCode='' OR M.EmpCode IN (SELECT * FROM dbo.SplitString(@EmpCode, ','))) 
	AND (@Branch IS NULL OR @Branch='' OR  M.Branch IN (SELECT Value FROM fn_Split(@Branch, ',')))
	AND (@Location IS NULL OR @Location='' OR  M.LOCT IN (SELECT Code FROM HRLocation WHERE CODE IN	(SELECT Value FROM fn_Split(@Location, ','))))
	AND (@Division IS NULL OR @Division='' OR  M.Division IN (SELECT Code FROM HRDivision WHERE CODE IN	(SELECT Value FROM fn_Split(@Division, ','))))
	AND (@Department IS NULL OR @Department='' OR  M.DEPT=@Department) 
	AND	(@Section IS NULL OR @Section='' OR  M.Sect=@Section)  
	AND	(@Position IS NULL OR @Position='' OR  M.JobCode=@Position) 
	AND	(@Level IS NULL OR @Level='' OR M.LevelCode=@Level)
END
