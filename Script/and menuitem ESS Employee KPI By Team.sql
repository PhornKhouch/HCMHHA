declare @ID int
select @ID=MenuId from SYMenuItem where ScreenId='ESSA000003'

insert SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values(@ID,'ESSA000007',@ID,'Employee KPI By Team','/SelfService/Performance/ESSEmployeeKPIByTeam','N','OP',45,'Distribution','4','0','1')

INSERT INTO SYActionTemplate values('ESSA000007','Details','VIEW_ACTION_NORMAL','Employee KPI By Team')
INSERT INTO SYActionTemplate values('ESSA000007','Index','LIST_ACTION_NORMAL','Employee KPI By Team')
INSERT INTO SYActionTemplate values('ESSA000007','Delete','NONE','Employee KPI By Team')
INSERT INTO SYActionTemplate values('ESSA000007','Edit','EDIT_ACTION_NORMAL','Employee KPI By Team')
INSERT INTO SYActionTemplate values('ESSA000007','Create','CREATE_ACTION_NORMAL','Employee KPI By Team')
INSERT INTO SYActionTemplate values('ESSA000007','Approve','NONE','Employee KPI By Team')
INSERT INTO SYActionTemplate values('ESSA000007','RequestForApproval','NONE','Employee KPI By Team')
GO
INSERT into SYRoleItems
select 5,ScreenId,ActionName,ActionTemplateID,'' from SYActionTemplate where ScreenId='ESSA000007'
GO
declare @ID int
select @ID=ID from SYMenuItem where ScreenId='ESSA000007'

INsert into SYRoleMenuItem
select  RoleId,@ID,4 from SYRoleItems where ScreenId='ESSA000007'
GROUP BY RoleId
GO