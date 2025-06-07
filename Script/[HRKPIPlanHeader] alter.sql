IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HRKPIPlanHeader]') AND type in (N'U'))
DROP TABLE [dbo].[HRKPIPlanHeader]
GO
CREATE TABLE [dbo].[HRKPIPlanHeader](
	[Code] [nvarchar](30) NOT NULL,
	[Category] [nvarchar](250) NULL,
	[KPICategory] [nvarchar](30) NOT NULL,
	[CriteriaType] [nvarchar](50) NOT NULL,
	[KPIType] [nvarchar](50) NOT NULL,
	[CriteriaName] [nvarchar](150) NULL,
	[Description] [nvarchar](max) NULL,
	[DocumentDate] [date] NULL,
	[PlanerCode] [nvarchar](20) NULL,
	[PlanerName] [nvarchar](200) NULL,
	[PlanerPosition] [nvarchar](250) NULL,
	[PICCode] [nvarchar](20) NULL,
	[PICName] [nvarchar](200) NULL,
	[PICPosition] [nvarchar](250) NULL,
	[TotalAchievement] [decimal](18, 4) NULL,
	[TotalScore] [decimal](18, 4) NULL,
	[TotalWeight] [decimal](18, 4) NULL,
	[KPIAverage] [decimal](18, 4) NULL,
	[PeriodFrom] [date] NULL,
	[PeriodTo] [date] NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
	[CreatedOn] [date] NOT NULL,
	[ChangedBy] [nvarchar](50) NULL,
	[ChangedOn] [date] NULL,
 CONSTRAINT [PK_HRKPIPlanHeader] PRIMARY KEY CLUSTERED 
(
	[KPICategory] ASC,
	[CriteriaType] ASC,
	[KPIType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
Alter Table HRKPIEmpRating alter Column Remark nvarchar(250) 
GO
CREATE TABLE [dbo].[HRKPIAssignMember](
	[AssignCode] [nvarchar](50) NOT NULL,
	[EmpCode] [nvarchar](20) NOT NULL,
	[EmployeeName] [nvarchar](200) NULL,
	Department	nvarchar(250)	null,
	[Position] [nvarchar](250) NULL,
 CONSTRAINT [PK_HRKPIAssignMember] PRIMARY KEY CLUSTERED 
(
	[AssignCode] ASC,
	[EmpCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HRKPIAssignHeader]') AND type in (N'U'))
DROP TABLE [dbo].[HRKPIAssignHeader]
GO

/****** Object:  Table [dbo].[HRKPIAssignHeader]    Script Date: 06/01/24 12:02:50 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[HRKPIAssignHeader](
	[AssignCode] [nvarchar](50) NOT NULL,
	[AuthorizePerson] [nvarchar](50) NOT NULL,
	[KPICategory] [nvarchar](10) NOT NULL,
	[HandleName] [nvarchar](150) NULL,
	[Position] [nvarchar](50) NULL,
	[KPIBU] [nvarchar](50) NULL,
	[EmpCode] [nvarchar](50) NULL,
	[EmpName] [nvarchar](150) NULL,
	[KPIType] [nvarchar](50) NULL,
	[Department] [nvarchar](250) NULL,
	[ExpectedDate] [date] NULL,
	[CompletedDate] [date] NULL,
	[DocumentDate] [date] NULL,
	[ReviewBy] [nvarchar](50) NULL,
	[AcknowledgeBy] [nvarchar](50) NULL,
	[VerifyBy] [nvarchar](50) NULL,
	[ApprovedBy] [nvarchar](50) NULL,
	[Signature] [nvarchar](250) NULL,
	[TotalAcheivement] [decimal](18, 4) NULL,
	[KPIAverage] [decimal](18, 4) NULL,
	[TotalScore] [decimal](18, 4) NULL,
	[Description] [nvarchar](150) NULL,
	[TotalWeight] [decimal](18, 2) NULL,
	[KPICode] [nvarchar](20) NULL,
	[PICPosition] [nvarchar](250) NULL,
	[PeriodFrom] [date] NULL,
	[PeriodTo] [date] NULL,
	[Member] [int] NULL,
	[Status] [nvarchar](50) NULL,
	[TeamName] [nvarchar](150) NULL,
	[ScreenID] [nvarchar](50) NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[CreatedOn] [date] NULL,
	[ChangedBy] [nvarchar](50) NULL,
	[ChangedOn] [date] NULL,
 CONSTRAINT [PK_HRKPIAssignHeader] PRIMARY KEY CLUSTERED 
(
	[AssignCode] ASC,
	[AuthorizePerson] ASC,
	[KPICategory] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


