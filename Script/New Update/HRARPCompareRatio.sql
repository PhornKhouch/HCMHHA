
CREATE TABLE [dbo].[HRARPCompareRatio](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FromRatio] [decimal](18, 2) NOT NULL,
	[ToRatio] [decimal](18, 2) NOT NULL,
	[Factor] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_HRARPCompareRatio] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HRARPLeveMidPoint]    Script Date: 5/6/2024 10:51:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO