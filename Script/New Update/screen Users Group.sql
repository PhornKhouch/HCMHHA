
INSERT SYActionTemplate VALUES('SYU00002','Generate','NONE','Users Group')
INSERT SYActionTemplate VALUES('SYU00002','Index','LIST_ACTION_NORMAL','Users Group')

INSERT SYRoleItems VALUES(5,'SYU00002','Generate','NONE',null)
INSERT SYRoleItems VALUES(5,'SYU00002','Index','LIST_ACTION_NORMAL',null)
update SYMenuItem set IsActive=1 where ScreenId='SYU00002'


