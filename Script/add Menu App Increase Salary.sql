Declare @ID int
select @ID=MenuId from SYMenuItem where ScreenId LIKE 'HRA0000014'

insert SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values(@ID,'HRA0000017',@ID,'Increase Salary','/HRM/Appraisal/HRAppIncSalary','N','OP',70,'Distribution','4','0','1')

INSERT INTO SYActionTemplate values('HRA0000017','Index','LIST_ACTION_NORMAL','Increase Salary')
INSERT INTO SYActionTemplate values('HRA0000017','CreateMultiRef','CREATE_ACTION_NORMAL','Increase Salary')
INSERT INTO SYActionTemplate values('HRA0000017','Details','VIEW_ACTION_NORMAL','Increase Salary')
INSERT INTO SYActionTemplate values('HRA0000017','Edit','EDIT_ACTION_NORMAL','Increase Salary')
INSERT INTO SYActionTemplate values('HRA0000017','RequestForApproval','NONE','Increase Salary')
INSERT INTO SYActionTemplate values('HRA0000017','Approve','NONE','Increase Salary')
INSERT INTO SYActionTemplate values('HRA0000017','ReleaseDoc','NONE','Increase Salary')
Go
insert into SYRoleItems
select 5,ScreenID,ActionName,ActionTemplateID,'' from SYActionTemplate where ScreenID='HRA0000017'
GO
insert into SYRoleMenuItem 
select 5,ID,4 from SYMenuItem where ScreenID='HRA0000017'
GO