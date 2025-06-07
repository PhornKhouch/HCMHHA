Declare @ID INT
select @ID=Parent from SYMenuItem where ScreenId='RPTAP00001'

insert into SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values(@ID,'RPTAP00003',@ID,'KPIs by Indicator','/Reporting/RPTKPIbyIndicator','N','OP',5,'Distribution','4','0','1')

INSERT INTO SYActionTemplate values('RPTAP00003','Index','LIST_ACTION_REPORT','KPIs by Indicator')
INSERT INTO  SYRoleItems values('5','RPTAP00003','Index','LIST_ACTION_REPORT','KPIs by Indicator')

GO
Update SYMenuItem set InOrder=30 where ScreenId='RPTAP00001'
Update SYMenuItem set InOrder=20 where ScreenId='RPTAP00002'

GO
