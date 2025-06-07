alter Table HRKPIAssignHeader Add AssignedBy Nvarchar(50)
go
EXEC sp_rename 'HRKPIAssignHeader.KPIBU', 'CriteriaType';
GO