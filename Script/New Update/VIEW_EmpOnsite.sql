
ALTER VIEW [dbo].[VIEW_EmpOnsite] as
SELECT 
	M.AllName,M.CompanyCode,GroupDept,M.Branch,M.Division,M.DEPT,M.JobCode,M.LOCT,SECT,M.Office,M.Groups,M.LevelCode,
	AT.*
FROM ATEmpActivity AT LEFT JOIN HRStaffProfile M ON AT.EmpCode = M.EmpCode
LEFT OUTER JOIN HRDepartment D ON M.DEPT = D.Code
LEFT OUTER JOIN HRPosition P ON M.JobCode = P.Code