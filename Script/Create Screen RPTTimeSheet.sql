Declare @ID INT
select @ID=ID from SYMenu where Text='Reports'

insert into SYMenu  values('Performance','','',NULL,'3',@ID,'N','5',NULL,NULL,NULL,NULL,NULL,NULL,NULL,'1')
GO
Declare @ID INT
select @ID=ID from SYMenu where Text='Performance' and Parent= (select ID from SYMenu where Text='Reports')

insert into SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values(@ID,'RPTAP00001',@ID,'Time Sheet','/Reporting/RPTTimeSheet','N','OP','1','Distribution','4','0','1')

INSERT INTO SYActionTemplate values('RPTAP00001','Index','LIST_ACTION_REPORT','TimeSheet')
INSERT INTO  SYRoleItems values('5','RPTAP00001','Index','LIST_ACTION_REPORT','TimeSheet')







