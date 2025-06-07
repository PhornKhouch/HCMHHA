ALTER TABLE HRKPIPlanHeader ADD ExpectedDate Date null,Dateline DATE;
GO
ALTER TABLE HRKPIPlanHeader drop column TotalScore
GO
ALTER TABLE HRKPIPlanHeader drop column TotalAchievement
go
ALTER TABLE HRKPIPlanHeader ADD Status Nvarchar(20);