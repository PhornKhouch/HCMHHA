alter table HREmpAppProcess drop column Increase,Adding
Go
EXEC sp_rename 'HREmpAppProcess.Remark', 'Comment';