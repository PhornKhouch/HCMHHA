INSERT INTO SYActionTemplate values('HRA0000013','ReleaseDoc','NONE','Appraisal Reveiw')
INSERT INTO SYRoleItems values(5,'HRA0000013','ReleaseDoc','NONE','Appraisal Reveiw')
GO
alter Table HRAPPMatrixIncreaseSalary add Adding decimal(18,2)
GO
Declare @ID int
select @ID=MenuId from SYMenuItem where ScreenId LIKE 'HRA0000013'

insert SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values(@ID,'HRA0000014',@ID,'Matrix Increase Salary','/HRM/Appraisal/HRAppMatrixIncSalary','N','OP',60,'Distribution','4','0','1')

INSERT INTO SYActionTemplate values('HRA0000014','Index','LIST_ACTION_NORMAL','Matrix Increase Salary')
INSERT INTO SYActionTemplate values('HRA0000014','Save','LIST_ACTION_NORMAL','Matrix Increase Salary')
Go
insert into SYRoleItems
select 5,ScreenID,ActionName,ActionTemplateID,'' from SYActionTemplate where ScreenID='HRA0000014'
GO
insert into SYRoleMenuItem 
select 5,ID,4 from SYMenuItem where ScreenID='HRA0000014'
GO