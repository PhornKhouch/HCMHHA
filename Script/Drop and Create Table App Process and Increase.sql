IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HREmpAppProcessItem]') AND type in (N'U'))
DROP TABLE [dbo].[HREmpAppProcessItem]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HREmpAppProcess]') AND type in (N'U'))
DROP TABLE [dbo].[HREmpAppProcess]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HRAPPIncSalaryItem]') AND type in (N'U'))
DROP TABLE [dbo].[HRAPPIncSalaryItem]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HRAPPIncSalary]') AND type in (N'U'))
DROP TABLE [dbo].[HRAPPIncSalary]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HRAPPIncSalary](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[DocumentDate] [date] NOT NULL,
	[Requestor] [nvarchar](50) NOT NULL,
	[RequestorName] [nvarchar](250) NULL,
	[EffectiveDate] [date] NULL,
	[TotalEmployee] [int] NOT NULL,
	[TotalIncrease] [decimal](18, 2) NOT NULL,
	[CareerType] [nvarchar](100) NULL,
	[Remark] [nvarchar](max) NULL,
	[Status] [nvarchar](20) NOT NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ChangedBy] [nvarchar](50) NULL,
	[ChangedOn] [datetime] NULL,
 CONSTRAINT [PK_HRAPPIncSalary] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HRAPPIncSalaryItem]    Script Date: 06/27/24 9:22:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HRAPPIncSalaryItem](
	[ID] [int] NOT NULL,
	[EmpCode] [nvarchar](50) NOT NULL,
	[EmployeeName] [nvarchar](250) NULL,
	[Department] [nvarchar](250) NULL,
	[Position] [nvarchar](250) NULL,
	[Increase] [decimal](18, 2) NOT NULL,
	[NewSalary] [decimal](18, 2) NOT NULL,
	[DocumentRef] [nvarchar](20) NULL,
 CONSTRAINT [PK_HRAPPIncSalaryItem] PRIMARY KEY CLUSTERED 
(
	[ID] ASC,
	[EmpCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HREmpAppProcess]    Script Date: 06/27/24 9:22:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HREmpAppProcess](
	[TranNo] [int] IDENTITY(1,1) NOT NULL,
	[EmpCode] [nvarchar](20) NOT NULL,
	[EmployeeName] [nvarchar](250) NULL,
	[AppraisalType] [nvarchar](50) NOT NULL,
	[Department] [nvarchar](250) NULL,
	[Position] [nvarchar](250) NULL,
	[DocumentDate] [date] NOT NULL,
	[InYear] [int] NOT NULL,
	[PeriodFrom] [date] NOT NULL,
	[PeriodTo] [date] NOT NULL,
	[TotalScore] [decimal](18, 2) NOT NULL,
	[Rate] [decimal](18, 2) NOT NULL,
	[Attachment] [nvarchar](max) NULL,
	[DocumentRef] [nvarchar](50) NULL,
	[Grade] [nvarchar](20) NULL,
	[Status] [nvarchar](50) NOT NULL,
	[Comment] [nvarchar](max) NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ChangedBy] [nvarchar](50) NULL,
	[ChangedOn] [datetime] NULL,
 CONSTRAINT [PK_HREmpAppProcess] PRIMARY KEY CLUSTERED 
(
	[TranNo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HREmpAppProcessItem]    Script Date: 06/27/24 9:22:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HREmpAppProcessItem](
	[AppraisalProcessNo] [int] NOT NULL,
	[Category] [nvarchar](50) NOT NULL,
	[Code] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](250) NOT NULL,
	[Weight] [decimal](18, 2) NOT NULL,
	[MaxScore] [decimal](18, 2) NOT NULL,
	[Result] [decimal](18, 2) NOT NULL,
	[Inorder] [int] NOT NULL,
 CONSTRAINT [PK_HREmpAppProcessItem] PRIMARY KEY CLUSTERED 
(
	[AppraisalProcessNo] ASC,
	[Category] ASC,
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
