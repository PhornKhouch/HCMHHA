


insert SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values('8075','HRF0000006','8075','KPI Tracking','/HRM/Appraisal/HRKPITracking','N','OP','7','Distribution','4','0','1')

INSERT INTO SYActionTemplate values('HRF0000006','Index','LIST_ACTION_NORMAL','KPI Tracking')
INSERT INTO SYRoleItems values('5','HRF0000006','Index','LIST_ACTION_NORMAL','')

INSERT INTO SYActionTemplate values('HRF0000006','Details','VIEW_ACTION_NORMAL','KPI Tracking')
INSERT INTO SYRoleItems values('5','HRF0000006','Details','VIEW_ACTION_NORMAL','')
INSERT INTO SYActionTemplate values('HRF0000006','Delete','NONE','KPI Tracking')
INSERT INTO SYRoleItems values('5','HRF0000006','Delete','NONE','')

INSERT INTO SYActionTemplate values('HRF0000006','Create','CREATE_ACTION_NORMAL','KPI Tracking')
INSERT INTO SYRoleItems values('5','HRF0000006','Create','CREATE_ACTION_NORMAL','')
INSERT INTO SYActionTemplate values('HRF0000006','Edit','EDIT_ACTION_NORMAL','KPI Tracking')
INSERT INTO SYRoleItems values('5','HRF0000006','Edit','EDIT_ACTION_NORMAL','')


insert SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values('6049','ESS0000020','6049','KPI Tracking','/HRM/Appraisal/ESSKPITracking','N','OP','21','Distribution','4','0','1')

INSERT INTO SYActionTemplate values('ESS0000020','Index','LIST_ACTION_NORMAL','Fringe Benefit')
INSERT INTO SYRoleItems values('5','ESS0000020','Index','LIST_ACTION_NORMAL','')

INSERT INTO SYActionTemplate values('ESS0000020','Edit','EDIT_ACTION_NORMAL','Fringe Benefit')
INSERT INTO SYRoleItems values('5','ESS0000020','Edit','EDIT_ACTION_NORMAL','')

INSERT INTO SYActionTemplate values('ESS0000020','Details','VIEW_ACTION_NORMAL','Fringe Benefit')
INSERT INTO SYRoleItems values('5','ESS0000020','Details','VIEW_ACTION_NORMAL','')

INSERT INTO SYActionTemplate values('ESS0000020','Delete','NONE','Fringe Benefit')
INSERT INTO SYRoleItems values('5','ESS0000020','Delete','NONE','')

INSERT INTO SYActionTemplate values('ESS0000020','Create','CREATE_ACTION_NORMAL','Fringe Benefit')
INSERT INTO SYRoleItems values('5','ESS0000020','Create','CREATE_ACTION_NORMAL','')



declare @ID int
select @ID=MenuId from SYMenuItem where ScreenId='ESS0000004'

insert SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values(@ID,'ESS0000023',@ID,'KPI Tracking','/SelfService/MyTeam/ESSMTKPITracking','N','OP',31,'Distribution','4','0','1')

INSERT INTO SYActionTemplate values('ESS0000023','Details','VIEW_ACTION_NORMAL','KPITracking')
INSERT INTO SYActionTemplate values('ESS0000023','Index','LIST_ACTION_NORMAL','KPITracking')
INSERT INTO SYActionTemplate values('ESS0000023','Delete','NONE','KPITracking')
INSERT INTO SYActionTemplate values('ESS0000023','Edit','EDIT_ACTION_NORMAL','KPITracking')
INSERT INTO SYActionTemplate values('ESS0000023','Create','CREATE_ACTION_NORMAL','KPITracking')

INSERT into SYRoleItems
select RoleId,'ESS0000023','Index','LIST_ACTION_NORMAL','' from SYRoleItems where ScreenId='ESS0000004'
group by RoleId
INSERT into SYRoleItems
select RoleId,'ESS0000023','Details','VIEW_ACTION_NORMAL','' from SYRoleItems where ScreenId='ESS0000004'
group by RoleId
INSERT into SYRoleItems
select RoleId,'ESS0000023','Delete','NONE','' from SYRoleItems where ScreenId='ESS0000004'
group by RoleId
INSERT into SYRoleItems
select RoleId,'ESS0000023','Edit','EDIT_ACTION_NORMAL','' from SYRoleItems where ScreenId='ESS0000004'
group by RoleId
INSERT into SYRoleItems
select RoleId,'ESS0000023','Create','CREATE_ACTION_NORMAL','' from SYRoleItems where ScreenId='ESS0000004'
group by RoleId
GO
declare @ID int
select @ID=ID from SYMenuItem where ScreenId='ESS0000023'

INsert into SYRoleMenuItem
select  RoleId,@ID,4 from SYRoleItems where ScreenId='ESS0000023'
GROUP BY RoleId
GO



CREATE TABLE [dbo].[HRKPITracking](
	[TranNo] [int] IDENTITY(1,1) NOT NULL,
	[EmpCode] [nvarchar](50) NOT NULL,
	[EmpName] [nvarchar](100) NOT NULL,
	[DocumentDate] [date] NOT NULL,
	[KPI] [nvarchar](50) NOT NULL,
	[KPIDescription] [nvarchar](max) NULL,
	[Amount] [decimal](18, 0) NOT NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
	[CreatedOn] [date] NOT NULL,
	[ChangedBy] [nvarchar](50) NULL,
	[ChangedOn] [date] NULL,
 CONSTRAINT [PK_HRKPITracking] PRIMARY KEY CLUSTERED 
(
	[TranNo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

