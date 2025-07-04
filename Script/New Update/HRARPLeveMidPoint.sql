CREATE TABLE [dbo].[HRAPPLevelMidPoint](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[JobLevel] [nvarchar](10) NOT NULL,
	[JobLeveMidPoint] decimal(18,2) NULL,
 CONSTRAINT [PK_HRARPLevelMidPoint] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

