Declare @ID int
Update SYMenuItem set InOrder=20 where ScreenId ='ESSE000001'
Update SYMenuItem set InOrder=30 where ScreenId ='ESSA000001'
select @ID=MenuId from SYMenuItem where ScreenId = 'ESSE000001'

insert SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values(@ID,'ESSS000001',@ID,'Configuration','/SelfService/Performance/ESSKPIConfiguration','N','OP',1,'Distribution','4','0','1')

INSERT INTO SYActionTemplate values('ESSS000001','Index','LIST_ACTION_NORMAL','Configuration')
Go
insert into SYRoleItems
select 5,ScreenID,ActionName,ActionTemplateID,'' from SYActionTemplate where ScreenID='ESSS000001'
GO
insert into SYRoleMenuItem 
select 5,ID,4 from SYMenuItem where ScreenID='ESSS000001'
GO