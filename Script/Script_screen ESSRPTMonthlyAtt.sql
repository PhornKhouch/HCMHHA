
Declare @ID int
select @ID=MenuId from SYMenuItem where ScreenId = 'RPTESS0001'
insert SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values(@ID,'RPTESS0002',@ID,'MonthlyAttendance','/Reporting/RPTESSMonthlyAttendance','N','OP','11','Distribution','4','0','1')

INSERT INTO SYActionTemplate values('RPTESS0002','Index','LIST_ACTION_REPORT','MonthlyAttendance')
INSERT INTO SYRoleItems values('5','RPTESS0002','Index','LIST_ACTION_REPORT','')
