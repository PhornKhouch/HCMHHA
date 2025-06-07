Declare @ID int
select @ID=MenuId from SYMenuItem where ScreenId = 'ESSA000001'

insert SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values(@ID,'ESSA000003',@ID,'KPI Plan','/SelfService/Performance/ESSKPIPlan','N','OP',35,'Distribution','4','0','1')

INSERT INTO SYActionTemplate values('ESSA000003','Index','LIST_ACTION_NORMAL','KPI Plan')
insert into SYActionTemplate values('ESSA000003','ReleaseDoc','NONE','KPI Plan')
INSERT INTO SYActionTemplate values('ESSA000003','Details','VIEW_ACTION_NORMAL','KPI Plan')
INSERT INTO SYActionTemplate values('ESSA000003','Edit','EDIT_ACTION_NORMAL','KPI Plan')
--INSERT INTO SYActionTemplate values('ESSA000002','Print','NONE','KPI Plan')
--INSERT INTO SYActionTemplate values('ESSA000002','RequestForApproval','NONE','KPI Plan')
Go
insert into SYRoleItems
select 5,ScreenID,ActionName,ActionTemplateID,'' from SYActionTemplate where ScreenID='ESSA000003'
GO
insert into SYRoleMenuItem 
select 5,ID,4 from SYMenuItem where ScreenID='ESSA000003'
GO
