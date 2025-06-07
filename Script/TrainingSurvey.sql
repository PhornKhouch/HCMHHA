insert SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values('7065','ESS0000025','7065','Training Survey','/SelfService/MyTeam/ESSMTTrainingSurvey','N','OP','33','Distribution','4','0','1')



INSERT INTO SYActionTemplate values('ESS0000025','Index','LIST_ACTION_NORMAL','')
INSERT INTO SYRoleItems values('5','ESS0000025','Index','LIST_ACTION_NORMAL','')

INSERT INTO SYActionTemplate values('ESS0000025','Edit','EDIT_ACTION_NORMAL','')
INSERT INTO SYRoleItems values('5','ESS0000025','Edit','EDIT_ACTION_NORMAL','')


insert SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values('8054','TR00000007','8054','Training Survey','/Training/Process/TRTrainingSurvey','N','OP','21','Distribution','4','0','1')

INSERT INTO SYActionTemplate values('TR00000007','Index','LIST_ACTION_NORMAL','')
INSERT INTO SYRoleItems values('5','TR00000007','Index','LIST_ACTION_NORMAL','')

INSERT INTO SYActionTemplate values('TR00000007','Edit','EDIT_ACTION_NORMAL','')
INSERT INTO SYRoleItems values('5','TR00000007','Edit','EDIT_ACTION_NORMAL','')

INSERT INTO SYActionTemplate values('TR00000007','Details','VIEW_ACTION_NORMAL','')
INSERT INTO SYRoleItems values('5','TR00000007','Details','VIEW_ACTION_NORMAL','')

INSERT INTO SYActionTemplate values('TR00000007','Delete','NONE','')
INSERT INTO SYRoleItems values('5','TR00000007','Delete','NONE','')

INSERT INTO SYActionTemplate values('TR00000007','Create','CREATE_ACTION_NORMAL','')
INSERT INTO SYRoleItems values('5','TR00000007','Create','CREATE_ACTION_NORMAL','')

INSERT INTO SYActionTemplate values('TR00000007','ExportTo','NONE','')
INSERT INTO SYRoleItems values('5','TR00000007','ExportTo','NONE','')

insert SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
values('8055','TRM000009','8055','Survey Setup','/Training/Setup/TRSurveySetup','N','OP','26','Distribution','4','0','1')



INSERT INTO SYActionTemplate values('TRM000009','Index','LIST_ACTION_NORMAL','')
INSERT INTO SYRoleItems values('5','TRM000009','Index','LIST_ACTION_NORMAL','')




-----NumberRank----


insert ExCfWorkFlowItem(ScreenID,DocType,Description,NumberRank,NumberRankItem,IsRequiredApproval)Values('TR00000007','TRS','Training Survey','TRS',4,1)


-------------------


CREATE TABLE [dbo].[TRTrainingEmployee](
	[CalendarNo] [nvarchar](20) NOT NULL,
	[LineItem] [int] NOT NULL,
	[InYear] [int] NULL,
	[CourseID] [nvarchar](50) NULL,
	[GroupDescription] [nvarchar](200) NULL,
	[CourseName] [nvarchar](200) NULL,
	[CourseCategoryID] [nvarchar](15) NULL,
	[CourseCategory] [nvarchar](200) NULL,
	[TrainingType] [nvarchar](50) NULL,
	[RequestDate] [date] NULL,
	[EmpCode] [nvarchar](50) NULL,
	[EmpName] [nvarchar](250) NULL,
	[Department] [nvarchar](250) NULL,
	[Position] [nvarchar](250) NULL,
	[InviteTraining] [int] NULL,
	[InviteTrainingBy] [nvarchar](50) NULL,
	[InviteTrainingDate] [datetime] NULL,
	[ScoreTheory] [decimal](18, 2) NULL,
	[ScorePractice] [decimal](18, 2) NULL,
	[IsInvite] [bit] NULL,
	[IsAccept] [bit] NULL,
	[Status] [nvarchar](50) NULL,
	[ReStatus] [nvarchar](50) NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[ChangedOn] [datetime] NULL,
	[ChangedBy] [nvarchar](50) NULL,
	[IsAssign] [bit] NULL,
 CONSTRAINT [PK_TRTrainingEmployee_1] PRIMARY KEY CLUSTERED 
(
	[CalendarNo] ASC,
	[LineItem] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[TREmpSurRating](
  [SurveyID] [nvarchar](30) NOT NULL,
  [RatingID] [int] NOT NULL,
  [Region] [nvarchar](20) NOT NULL,
  [Description] [nvarchar](100) NOT NULL,
  [Rating] [int] NOT NULL,
 CONSTRAINT [PK_TREmpSurRating] PRIMARY KEY CLUSTERED 
(
  [SurveyID] ASC,
  [RatingID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


CREATE TABLE [dbo].[TRSurveyRating](
  [ID] [int] IDENTITY(1,1) NOT NULL,
  [Code] [nvarchar](50) NOT NULL,
  [Description] [nvarchar](100) NOT NULL,
  [Rating] [int] NOT NULL,
 CONSTRAINT [PK_TRSurveyRating] PRIMARY KEY CLUSTERED 
(
  [ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO



CREATE TABLE [dbo].[TRSurveyRegion](
  [Code] [nvarchar](50) NOT NULL,
  [Description] [nvarchar](max) NULL,
  [SecDescription] [nvarchar](500) NULL,
  [Remark] [nvarchar](500) NULL,
  [IsRating] [bit] NULL,
  [InOrder] [int] NULL,
  [IsComment] [bit] NULL,
 CONSTRAINT [PK_TRSurveyRegion] PRIMARY KEY CLUSTERED 
(
  [Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


CREATE TABLE [dbo].[TRSurveyFactor](
  [Code] [varchar](50) NOT NULL,
  [Description] [nvarchar](100) NULL,
  [SecDescription] [nvarchar](100) NULL,
  [Region] [nvarchar](50) NULL,
  [Remark] [nvarchar](500) NULL,
 CONSTRAINT [PK_TRSurveyFactor] PRIMARY KEY CLUSTERED 
(
  [Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO



CREATE TABLE [dbo].[TREmpSurveyScore](
  [ID] [int] IDENTITY(1,1) NOT NULL,
  [SurveyID] [nvarchar](20) NOT NULL,
  [Code] [nvarchar](10) NOT NULL,
  [Description] [nvarchar](100) NULL,
  [SecDescription] [nvarchar](100) NULL,
  [Region] [nvarchar](10) NULL,
  [Remark] [nvarchar](500) NULL,
  [Score] [int] NULL,
 CONSTRAINT [PK_TREmpSurveyScore_1] PRIMARY KEY CLUSTERED 
(
  [ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


CREATE TABLE [dbo].[TRTrainingSurvey](
  [SurveyID] [nvarchar](20) NOT NULL,
  [EmpCode] [nvarchar](15) NOT NULL,
  [EmpName] [nvarchar](30) NULL,
  [SurveyDate] [date] NULL,
  [SurFromDate] [date] NULL,
  [SurToDate] [date] NULL,
  [Positon] [nvarchar](200) NULL,
  [DateOfHire] [date] NULL,
  [Department] [nvarchar](200) NULL,
  [Result] [nvarchar](10) NULL,
  [TotalScore] [int] NOT NULL,
  [AssignedTo] [nvarchar](15) NOT NULL,
  [AssignedPositon] [nvarchar](200) NULL,
  [Strengths] [nvarchar](max) NULL,
  [Weakness] [nvarchar](max) NULL,
  [TrainingProgram] [nvarchar](max) NULL,
  [Comments] [nvarchar](max) NULL,
  [ActionPlan] [nvarchar](max) NULL,
  [CreatedBy] [nvarchar](30) NOT NULL,
  [CreatedOn] [datetime] NOT NULL,
  [ChangedBy] [nvarchar](30) NULL,
  [ChengedOn] [datetime] NULL,
 CONSTRAINT [PK_TRTrainingSurvey] PRIMARY KEY CLUSTERED 
(
  [SurveyID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO