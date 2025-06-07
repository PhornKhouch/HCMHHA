Declare @ID int
select @ID=MenuId from SYMenuItem where ScreenId = 'RPTESS0002'

insert SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values(@ID,'RPTESS0004',@ID,'KPI Time Sheet','/Reporting/RPTESSKPITimeSheet','N','OP',30,'Distribution','4','0','1')
INSERT INTO SYActionTemplate values('RPTESS0004','Index','LIST_ACTION_REPORT','KPI Time Sheet')
INSERT INTO SYRoleItems values('5','RPTESS0004','Index','LIST_ACTION_REPORT','')
