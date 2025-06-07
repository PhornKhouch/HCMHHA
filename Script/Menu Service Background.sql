Declare @ID INT
select @ID=Parent from SYMenu where Text='User Management'
INSERT SYMenu (Text,Segment,Parent,Part,InOrder,IsActive) 
       VALUES('Service',3,@ID,'N',30,1)
GO
declare @ID int
select @ID=ID from SYMenu where Text='Service'
Insert into SYRoleMenuItem values(5,@ID,3)
GO
Declare @ID INT
select TOP 1 @ID=ID from SYMenu where Text = 'Service'

INSERT SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
       VALUES(@ID,'SYS0001',@ID,'Service Background','/Services/SYServiceBackground','N','OP',10,'Distribution',4,0,1)

INSERT INTO SYActionTemplate VALUES('SYS0001','Index','LIST_ACTION_NORMAL','Service Background')
INSERT INTO SYRoleItems VALUES(5,'SYS0001','Index','LIST_ACTION_NORMAL','')
INSERT INTO SYActionTemplate VALUES('SYS0001','Save','LIST_ACTION_NORMAL','Service Background')
INSERT INTO SYRoleItems VALUES(5,'SYS0001','Save','LIST_ACTION_NORMAL','')
GO
declare @ID int
select @ID=ID from SYMenuItem where Text='Service Background'
Insert into SYRoleMenuItem values(5,@ID,4)
--update SYActionTemplate set ActionName='TAB_Menu' where ScreenID= 'SYM000002' and ActionName='TAB_1'
--update SYActionTemplate set ActionName='TAB_MenuItem' where ScreenID= 'SYM000002' and ActionName='TAB_2'
--update SYRoleItems set ActionName='TAB_Menu' where ScreenID= 'SYM000002' and ActionName='TAB_1'
--update SYRoleItems set ActionName='TAB_MenuItem' where ScreenID= 'SYM000002' and ActionName='TAB_2'