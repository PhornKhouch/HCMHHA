
insert into SYMenu (Text,Segment,Parent,Part,InOrder,IsActive)
values('Attendance','3','6048','N',1,1)

insert into SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values('16067','RPTESS0001',16067,'Attendance','/Reporting/RPTESSAttendance','N','OP',10,'Distribution','4','0','1')

INSERT INTO SYActionTemplate values('RPTESS0001','Index','LIST_ACTION_REPORT','Tracking')
INSERT INTO  SYRoleItems values('5','RPTESS0001','Index','LIST_ACTION_REPORT','Tracking')


