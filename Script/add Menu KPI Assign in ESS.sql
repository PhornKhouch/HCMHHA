Declare @ID int
select @ID=MenuId from SYMenuItem where ScreenId = 'ESSA000001'

insert SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values(@ID,'ESSA000002',@ID,'KPI Assign','/SelfService/Performance/ESSKPIAssign','N','OP',40,'Distribution','4','0','1')

INSERT INTO SYActionTemplate values('ESSA000002','Index','LIST_ACTION_NORMAL','KPI Assign')
INSERT INTO SYActionTemplate values('ESSA000002','Approve','NONE','KPI Assign')
INSERT INTO SYActionTemplate values('ESSA000002','Create','CREATE_ACTION_NORMAL','KPI Assign')
INSERT INTO SYActionTemplate values('ESSA000002','Details','VIEW_ACTION_NORMAL','KPI Assign')
INSERT INTO SYActionTemplate values('ESSA000002','Edit','EDIT_ACTION_NORMAL','KPI Assign')
INSERT INTO SYActionTemplate values('ESSA000002','Print','NONE','KPI Assign')
INSERT INTO SYActionTemplate values('ESSA000002','RequestForApproval','NONE','KPI Assign')
Go
insert into SYRoleItems
select 5,ScreenID,ActionName,ActionTemplateID,'' from SYActionTemplate where ScreenID='ESSA000002'
GO
insert into SYRoleMenuItem 
select 5,ID,4 from SYMenuItem where ScreenID='ESSA000002'
GO
