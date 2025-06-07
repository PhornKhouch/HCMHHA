EXEC sp_rename 'HRAppPerformanceRate.FromRate', 'FromScore';
GO
EXEC sp_rename 'HRAppPerformanceRate.ToRate', 'ToScore';
GO
ALter Table HRAppPerformanceRate add Rate decimal(18,2)
GO
Update HRAppPerformanceRate set Rate=0
GO
ALter Table HRAppPerformanceRate alter column Rate decimal(18,2) not null
GO
