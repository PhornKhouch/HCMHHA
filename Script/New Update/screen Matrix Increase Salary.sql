Declare @ID INT
select @ID=MenuId from SYMenuItem where ScreenID = 'HRA0000013'
print @id--the same screen 

INSERT SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
       VALUES(@ID,'HRA0000012',@ID,'Matrix Increase Salary','/HRM/Appraisal/HRAPPMaIncSalary','N','OP',30,'Distribution',4,0,1)

INSERT INTO SYRoleItems VALUES('5','HRA0000012','Index','LIST_ACTION_NORMAL','')
INSERT INTO SYRoleItems VALUES('5','HRA0000012','Save','LIST_ACTION_NORMAL Alert','')
INSERT INTO SYActionTemplate VALUES('HRA0000012','Index','LIST_ACTION_NORMAL','Setting')
INSERT INTO SYActionTemplate VALUES('HRA0000012','Save','LIST_ACTION_NORMAL Alert','Setting')
GO
declare @ID int
select @ID=ID from SYMenuItem where ScreenId='HRA0000012' --New screen ID
Insert into SYRoleMenuItem
select RoleId,@ID,4 from SYRoleItems where ScreenId in ('HRA0000013') --Old screen ID
GROUP BY RoleId