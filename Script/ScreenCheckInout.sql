select * from SYMenuItem where ScreenId='RPTAT00001' Separate
select * from SYMenuItem where Parent='7061'
select count(*) from ATEmpSchedule where TranDate between '2025-01-01' and '2025-01-31' and CreateBy='system'
select * from SYData

insert into SYData(SelectValue,SelectText,DropDownType,IsActive)
values('Staff','By Staff','CheckInOutSeparate_SELECT',1),
	  ('System','By System','CheckInOutSeparate_SELECT',1)