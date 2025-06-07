
Declare @ID int
select @ID=MenuId from SYMenuItem where ScreenId = 'RPTESS0001'
insert SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values(@ID,'RPTESS0003',@ID,'Miss Scan','/Reporting/RPTESSMissScan','N','OP','12','Distribution','4','0','1')

INSERT INTO SYActionTemplate values('RPTESS0003','Index','LIST_ACTION_REPORT','MissScan')
INSERT INTO SYRoleItems values('5','RPTESS0003','Index','LIST_ACTION_REPORT','')

