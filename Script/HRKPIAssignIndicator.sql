CREATE TABLE [dbo].[HRKPIAssignIndicator](
	[AssignCode] [nvarchar](50) NOT NULL,
	[Indicator] [nvarchar](20) NOT NULL,
	[IndicatorType] [nvarchar](250) NULL,
	[Weight] [decimal](18, 4) NOT NULL,
	[Acheivement] [decimal](18, 4) NULL,
 CONSTRAINT [PK_HRKPIAssignIndicator] PRIMARY KEY CLUSTERED 
(
	[AssignCode] ASC,
	[Indicator] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


