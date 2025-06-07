
Declare @ID int
select @ID=MenuId from SYMenuItem where ScreenId = 'RPTAP00002'

insert SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values(@ID,'RPTAP00005',@ID,'KPI Tracking Statistic','/Reporting/RPTKPITrackingStatistic','N','OP',40,'Distribution',4,0,1)

INSERT INTO SYActionTemplate values('RPTAP00005','Index','LIST_ACTION_REPORT','Tracking Statistic')
Go
insert into SYRoleItems
select 5,ScreenID,ActionName,ActionTemplateID,'' from SYActionTemplate where ScreenID='RPTAP00005'
GO
insert into SYRoleMenuItem 
select 5,ID,4 from SYMenuItem where ScreenID='RPTAP00005'
GO