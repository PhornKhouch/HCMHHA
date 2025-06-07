ALTER TABLE ATLateEarlyDeduct
ADD IsTransfer bit
GO

update ATLateEarlyDeduct set IsTransfer=0

ALTER TABLE ATPolicy
ADD Late2 int

ALTER TABLE ATPolicy
ADD Early2 int
GO
update ATPolicy set Early2=0,Late2=0