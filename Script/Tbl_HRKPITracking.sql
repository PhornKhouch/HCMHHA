

CREATE TABLE [dbo].[HRKPITracking](
	[TranNo] [int]IDENTITY(1,1) NOT NULL,
	[EMPCode] [nvarchar](50) NOT NULL,
	[EmpName] [nvarchar](100) NOT NULL,
	[DocumentDate] [date] NOT NULL,
	[KPI] [nvarchar](50) NOT NULL,
	[KPIDescription] [nvarchar](150) NULL,
	[Amount] [decimal](18, 0) NOT NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
	[CreatedOn] [date] NOT NULL,
	[ChangedBy] [nvarchar](50) NULL,
	[ChangedOn] [date] NULL,
	 CONSTRAINT [PK_HRKPITracking] PRIMARY KEY CLUSTERED 
(
	[TranNo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

) ON [PRIMARY]
GO


