
CREATE TABLE [dbo].[HRAPPMatrixIncreaseSalary](
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
	[NewSalary] [decimal](18, 2) NOT NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[CreatedOn] [datetime] NULL,
	[ChangedBy] [nvarchar](50) NULL,
	[ChangedOn] [datetime] NULL,
 CONSTRAINT [PK_HRAPPMatrixIncreaseSalary] PRIMARY KEY CLUSTERED 
(
	[EmpCode] ASC,
	[InYear] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


