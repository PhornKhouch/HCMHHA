INSERT SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
       VALUES(7053,'INF0000013',7053,'Setting','/Config/Setup/HRSettings','N','OP',3,'Distribution',4,0,1)

INSERT INTO SYRoleItems VALUES('5','INF0000013','Index','LIST_ACTION_NORMAL','')
INSERT INTO SYRoleItems VALUES('5','INF0000013','Save','LIST_ACTION_NORMAL Alert','')
INSERT INTO SYActionTemplate VALUES('INF0000013','Index','LIST_ACTION_NORMAL','Setting')
INSERT INTO SYActionTemplate VALUES('INF0000013','Save','LIST_ACTION_NORMAL Alert','Setting')