EXEC sp_rename 'HRARPLeveMidPoint', 'HRAPPLevelMidPoint'
GO
EXEC sp_rename 'HRAPPLevelMidPoint.JobLeveMidPoint',  'JobLevelMidPoint', 'COLUMN'
GO
ALTER TABLE HRAPPLevelMidPoint ALTER COLUMN JobLevelMidPoint DECIMAl(18,2)
