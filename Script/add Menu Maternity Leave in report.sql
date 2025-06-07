Declare @ID int
select @ID=MenuId from SYMenuItem where ScreenId = 'RPTLM00001'

insert SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values(@ID,'RPTLM00003',@ID,'Maternity Leave','/Reporting/RPTMaternityLeave','N','OP',30,'Distribution','4','0','1')

INSERT INTO SYActionTemplate values('RPTLM00003','Index','LIST_ACTION_REPORT','Maternity Leave')
Go
insert into SYRoleItems
select 5,ScreenID,ActionName,ActionTemplateID,'' from SYActionTemplate where ScreenID='RPTLM00003'
GO
insert into SYRoleMenuItem 
select 5,ID,4 from SYMenuItem where ScreenID='RPTLM00003'
GO