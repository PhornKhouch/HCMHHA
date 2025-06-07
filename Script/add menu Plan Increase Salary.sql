
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
Declare @ID INT
select @ID=MenuId from SYMenuItem where ScreenID = 'HRA0000013'
print @id--the same screen 

INSERT SYMenuItem (MenuId,ScreenId,Parent,Text,NavigateUrl,Part,SegmentUri,InOrder,TitleSearch,TopIndex,ColSpan,IsActive) 
       VALUES(@ID,'HRA0000012',@ID,'Plan Increase Salary','/HRM/Appraisal/HRAPPPlanIncSalary','N','OP',30,'Distribution',4,0,1)

INSERT INTO SYRoleItems VALUES('5','HRA0000012','Index','LIST_ACTION_NORMAL','')
INSERT INTO SYRoleItems VALUES('5','HRA0000012','Save','LIST_ACTION_NORMAL Alert','')
INSERT INTO SYActionTemplate VALUES('HRA0000012','Index','LIST_ACTION_NORMAL','Setting')
INSERT INTO SYActionTemplate VALUES('HRA0000012','Save','LIST_ACTION_NORMAL Alert','Setting')
GO
declare @ID int
select @ID=ID from SYMenuItem where ScreenId='HRA0000012' --New screen ID
Insert into SYRoleMenuItem
select RoleId,@ID,4 from SYRoleItems where ScreenId in ('HRA0000013') --Old screen ID
GROUP BY RoleId

