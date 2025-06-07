
ALTER VIEW [dbo].[VIEW_ATInOut] as
SELECT ATInOut.ID,ATInOut.EmpCode,HRStaffProfile.AllName,ATInOut.CardNo,HRLocation.Description as Location,
HRStaffProfile.Division,HRDepartment.Description as Department,GroupDept,
HRPosition.DESCRIPTION AS Position, ATInOut.FullDate, HRStaffProfile.BRANCH,HRStaffProfile.DEPT,HRStaffProfile.LOCT,
HRStaffProfile.JOBCODE,ATInOut.STATUS,ATInOut.Remark,ATInOut.CreateBy,ATInOut.ChangedBy,Office,Groups
FROM ATInOut LEFT JOIN HRStaffProfile ON ATInOut.EMPCODE = HRStaffProfile.EMPCODE LEFT OUTER JOIN
HRDepartment ON HRStaffProfile.DEPT = HRDepartment.CODE LEFT OUTER JOIN
HRPosition ON HRStaffProfile.JOBCODE = HRPosition.CODE LEFT JOIN 
HRLocation on HRLocation.Code=HRStaffProfile.LOCT

