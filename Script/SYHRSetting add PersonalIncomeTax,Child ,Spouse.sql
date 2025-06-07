Alter Table SYHRSetting add PersonalIncomeTax Decimal(18,2),Child Decimal(18,2),Spouse Decimal(18,2)
go
Update SYHRSetting set PersonalIncomeTax=0,Child=150000,Spouse=150000
GO
delete from SYMenuItem where ScreenId='HRM0000003'
delete from SYRoleItems where ScreenId='HRM0000003'
delete from SYActionTemplate where ScreenId='HRM0000003'
DROP TABLE HRExcept
GO
ALTER TABLE HREmpFamily DROP COLUMN ExceeptID
ALTER TABLE HREmpFamily ADD Child BIT NOT NULL DEFAULT 0
ALTER TABLE HREmpFamily ADD Spouse BIT NOT NULL DEFAULT 0