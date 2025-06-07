select * from SYRoleItems where screenId='RPTPR00009'
select * from SYMenuItem where screenId='RPTPR00009'
select * from SYActionTemplate where screenId='RPTPR00009'


insert SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values('7059','RPTPR00009','7059','Fringe Benefit','/Reporting/RPTFringeBenefitReport','N','OP','2','Distribution','4','0','1')

INSERT INTO SYActionTemplate values('RPTPR00009','Index','LIST_ACTION_NORMAL','Fringe Benefit')
INSERT INTO  SYRoleItems values('5','RPTPR00009','Index','LIST_ACTION_NORMAL','Fringe Benefit')
