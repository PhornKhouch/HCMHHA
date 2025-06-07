ALTER Table HRAnnouncement ADD AnnounceType NVARCHAR(50)


CREATE TABLE [dbo].[HRAnnounceType](
  [Code] [varchar](10) NOT NULL,
  [Description] [nvarchar](100) NULL,
  [SecDescription] [nvarchar](100) NULL,
  [Remark] [nvarchar](500) NULL,
 CONSTRAINT [PK_HRAnnounceType] PRIMARY KEY CLUSTERED 
(
  [Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO