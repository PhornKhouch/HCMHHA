
Declare @ID int
select @ID=MenuId from SYMenuItem where ScreenId = 'RPTAT00002'

insert into SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values(@ID,'RPTAT00006',@ID,'Miss Scan','/Reporting/RPTMissScan','N','OP','1','Distribution','6','0','1')

INSERT INTO SYActionTemplate values('RPTAT00006','Index','LIST_ACTION_REPORT','MissScan')
INSERT INTO  SYRoleItems values('5','RPTAT00006','Index','LIST_ACTION_REPORT','')

select * from SYMenuItem where ScreenId='RPTAT00006'
select * from SYActionTemplate where ScreenId='RPTAT00006'
select * from SYMenuItem where ScreenId='RPTAT00006'