USE [HumicaHHA]
GO
/****** Object:  StoredProcedure [dbo].[HR_RPT_EmpAttendanceSeparate]    Script Date: 5/24/2025 8:58:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROC [dbo].[HR_RPT_EmpAttendanceSeparate] (@Company nvarchar(200),@Branch nvarchar(200),@Division nvarchar(100),@Department nvarchar(100),@Section nvarchar(100)
                          ,@Position nvarchar(100),@Level nvarchar(100),@CreatedBy nvarchar(200),@FromDate Date,@ToDate Date) AS
begin
SELECT ATT.TranNo,SP.EmpCode,SP.AllName,SP.OthAllName,ATT.TranDate,ATT.SHIFT,ATT.LeaveCode,ATT.LeaveDesc,
(CASE WHEN YEAR( ATT.Start1)=1900 THEN NULL ELSE ATT.Start1 END)Start1,ATT.Late1,
(CASE WHEN YEAR(ATT.End1)=1900 THEN NULL ELSE ATT.End1 END)End1,ATT.Early1
,ATT.WHOUR,ATT.WOT,ATT.NWH, SP.StartDate, SP.Probation, 
 SP.DateTerminate,
 CASE 
        WHEN @CreatedBy = 'System' THEN 'System'
        WHEN @CreatedBy = 'Staff' THEN 'Staff'
        WHEN ATT.CreateBy ='System' THEN 'System'
		WHEN ATT.CreateBy !='System' THEN 'Staff'
    END AS CreatedByType,
HRBranch.Description AS BRANCHDESC, HRDepartment.Description AS Department, HRPosition.Description AS Position,HRPosition.SecDescription AS SecPosition, HRDivision.Description AS DivisionDesc
FROM ATEmpSchedule ATT INNER JOIN HRStaffProfile SP  ON SP.EMPCODE = ATT.EMPCODE LEFT JOIN
	HRDivision ON SP.Division = HRDivision.CODE LEFT OUTER JOIN
	HRCompany ON HRCompany.Company=SP.CompanyCode LEFT OUTER JOIN
	HRBranch ON SP.BRANCH =HRBranch.Code and HRBranch.CompanyCode=SP.CompanyCode LEFT OUTER JOIN
	HRDepartment ON SP.DEPT = HRDepartment.CODE LEFT OUTER JOIN
	HRPosition ON SP.JOBCODE =HRPosition.CODE
	Where 
	    (TranDate Between @FromDate and @ToDate ) AND
		(@Position IS NULL OR @Position='' OR  SP.JobCode=@Position) AND
		(@Company IS NULL OR @Company='' OR  SP.CompanyCode IN (SELECT Value FROM fn_Split(@Company, ','))) AND
		(@Branch IS NULL OR @Branch='' OR  SP.Branch IN (SELECT Value FROM fn_Split(@Branch, ','))) AND 
		(@Division IS NULL OR @Division='' OR  SP.Division IN (SELECT Code FROM HRDivision WHERE CODE IN	(SELECT Value FROM fn_Split(@Division, ',')))) AND 
		(@Department IS NULL OR @Department='' OR  SP.DEPT=@Department) AND
		(@Section IS NULL OR @Section='' OR  SP.Sect=@Section) AND 
		(@Position IS NULL OR @Position='' OR  SP.JobCode=@Position) AND
		(@Level IS NULL OR @Level='' OR SP.LevelCode=@Level)
		AND (
            (@CreatedBy IS NULL OR @CreatedBy = '') 
            OR (@CreatedBy = 'System' AND ATT.CreateBy = 'System')
            OR (@CreatedBy = 'Staff' AND ATT.CreateBy != 'System')      
		)
END

