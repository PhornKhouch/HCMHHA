--create screen Report Att Training
Declare @SubID1 INT
select @SubID1=MenuId from SYMenuItem where ScreenId='RPTAT00001'
INSERT INTO SYMenuItem (
    MenuId, ScreenId, Parent, Text, NavigateUrl, Part, SegmentUri, InOrder, TitleSearch, TopIndex, ColSpan, IsActive
)
VALUES
    (@SubID1, 'RPTAT00007', @SubID1, 'Employee Check In-Out', '/Reporting/RPTEmpCheckInOut', 'N', 'OP', '15', 'Distribution', '4', '0', '1');

INSERT INTO SYActionTemplate values('RPTAT00007','Index','LIST_ACTION_REPORT','Emp CheckInOut') 
INSERT INTO SYRoleItems values(5,'RPTAT00007','Index','LIST_ACTION_REPORT','Emp CheckInOut') 

insert into SYData(SelectValue,SelectText,DropDownType,IsActive)
values('Staff','By Staff','CheckInOutSeparate_SELECT',1),
	  ('System','By System','CheckInOutSeparate_SELECT',1)