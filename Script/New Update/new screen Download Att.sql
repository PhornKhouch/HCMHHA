Declare @ID INT
select @ID=MenuId from SYMenuItem where ScreenID = 'ATM0000001'
print @id--the same screen 

INSERT SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
       VALUES(@ID,'ATM0000015',@ID,'Download Att','/Attendance/Attendance/HRDownloadAtt','N','OP',4,'Distribution',4,0,1)

INSERT INTO SYRoleItems VALUES('5','ATM0000015','Index','LIST_ACTION_NORMAL','')
INSERT INTO SYRoleItems VALUES('5','ATM0000015','Download','NONE','')
INSERT INTO SYActionTemplate VALUES('ATM0000015','Index','LIST_ACTION_NORMAL','Download att')
INSERT INTO SYActionTemplate VALUES('ATM0000015','Download','NONE','Download att')
GO
declare @ID int
select @ID=ID from SYMenuItem where ScreenId='ATM0000015' --New screen ID
Insert into SYRoleMenuItem
select RoleId,@ID,4 from SYRoleItems where ScreenId in ('ATM0000001') --Old screen ID
GROUP BY RoleId