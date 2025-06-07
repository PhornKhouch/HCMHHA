/****** Object:  Table [dbo].[HRAPPMatrixIncreaseSalary]    Script Date: 06/17/24 4:40:10 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HRAPPMatrixIncreaseSalary]') AND type in (N'U'))
DROP TABLE [dbo].[HRAPPMatrixIncreaseSalary]
GO
/****** Object:  Table [dbo].[HRAPPIncSalaryItem]    Script Date: 06/17/24 4:40:10 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HRAPPIncSalaryItem]') AND type in (N'U'))
DROP TABLE [dbo].[HRAPPIncSalaryItem]
GO
/****** Object:  Table [dbo].[HRAPPIncSalary]    Script Date: 06/17/24 4:40:10 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HRAPPIncSalary]') AND type in (N'U'))
DROP TABLE [dbo].[HRAPPIncSalary]
GO
/****** Object:  Table [dbo].[HRAPPIncSalary]    Script Date: 06/17/24 4:40:10 PM ******/
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
/****** Object:  Table [dbo].[HRAPPIncSalaryItem]    Script Date: 06/17/24 4:40:10 PM ******/
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
 CONSTRAINT [PK_HRAPPIncSalaryItem] PRIMARY KEY CLUSTERED 
(
	[ID] ASC,
	[EmpCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HRAPPMatrixIncreaseSalary]    Script Date: 06/17/24 4:40:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HRAPPMatrixIncreaseSalary](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[ScreenID] [nvarchar](50) NOT NULL,
	[DocumentRef] [nvarchar](50) NOT NULL,
	[EmpCode] [nvarchar](10) NOT NULL,
	[EmpName] [nvarchar](50) NULL,
	[InYear] [int] NOT NULL,
	[JobLevel] [nvarchar](10) NOT NULL,
	[JobLevelMidPoint] [decimal](18, 2) NOT NULL,
	[Salary] [decimal](18, 2) NOT NULL,
	[Rating] [int] NOT NULL,
	[CompaRatio] [decimal](18, 2) NOT NULL,
	[SalaryIncPers] [decimal](18, 2) NOT NULL,
	[SalaryIncAmount] [decimal](18, 2) NOT NULL,
	[Adding] [decimal](18, 2) NULL,
	[NewSalary] [decimal](18, 2) NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ChangedBy] [nvarchar](50) NULL,
	[ChangedOn] [datetime] NULL,
 CONSTRAINT [PK_HRAPPMatrixIncreaseSalary] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
