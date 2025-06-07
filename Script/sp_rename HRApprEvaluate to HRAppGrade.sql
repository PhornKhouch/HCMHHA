EXEC sp_rename 'HRApprEvaluate.FromRate', 'FromScore';
GO
EXEC sp_rename 'HRApprEvaluate.ToRate', 'ToScore';
GO
EXEC sp_rename 'HRApprEvaluate.Result', 'Grade';
GO
EXEC sp_rename 'HRApprEvaluate.Explanation', 'Rating';
GO
EXEC sp_rename 'HRApprEvaluate.SecDescription', 'Description';
GO
EXEC sp_rename 'HRApprEvaluate', 'HRAppGrade';
GO
ALTER TABLE HRAppGrade DROP CONSTRAINT PK_HRApprEvaluate;
GO
ALTER TABLE HRAppGrade ADD CONSTRAINT PK_HRAppGrade PRIMARY KEY CLUSTERED
	(
		TranNo ASC
	) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY  = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
Update HRAppGrade set Rating=0
GO
ALter Table HRAppGrade alter Column Rating Decimal(18,2) not null
