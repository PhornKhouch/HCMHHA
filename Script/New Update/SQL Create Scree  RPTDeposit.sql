
insert SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values('7059','RPT0000019','7059','Deposit','/Reporting/RPTEmpDeposit','N','OP','10','Distribution','4','0','1')

INSERT INTO SYActionTemplate values('RPT0000019','Index','LIST_ACTION_REPORT','Deposit')
INSERT INTO  SYRoleItems values('5','RPT0000019','Index','LIST_ACTION_REPORT','Deposit')
