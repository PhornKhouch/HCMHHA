Declare @ID INT
select @ID=Parent from SYMenuItem where ScreenId='RPTAP00001'

insert into SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values(@ID,'RPTAP00002',@ID,'Tracking','/Reporting/RPTKPITracking','N','OP',10,'Distribution','4','0','1')

INSERT INTO SYActionTemplate values('RPTAP00002','Index','LIST_ACTION_REPORT','Tracking')
INSERT INTO  SYRoleItems values('5','RPTAP00002','Index','LIST_ACTION_REPORT','Tracking')

