CREATE TABLE [dbo].[HRKPIGrade](
	[TranNo] [int] IDENTITY(1,1) NOT NULL,
	[FromScore] [decimal](18, 2) NOT NULL,
	[ToScore] [decimal](18, 2) NOT NULL,
	[Grade] [nvarchar](100) NULL,
	[Description] [nvarchar](100) NULL,
 CONSTRAINT [PK_HRKPIGrade] PRIMARY KEY CLUSTERED 
(
	[TranNo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
alter Table HRKPIAssignHeader add Grade nvarchar(10)
GO


