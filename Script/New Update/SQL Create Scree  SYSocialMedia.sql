
insert SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values('7053','CNF0000008','7053','SocialMediaType','/Config/Setup/SYSocialMedia','N','OP','1','Distribution','4','0','1')

INSERT INTO SYActionTemplate values('CNF0000008','Index','LIST_ACTION_NORMAL','SocialMediaType')
INSERT INTO  SYRoleItems values('5','CNF0000008','Index','LIST_ACTION_NORMAL','SocialMediaType')
