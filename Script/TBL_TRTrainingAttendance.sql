CREATE TABLE [dbo].[TRTrainingAttendance](
  [TrainNo] [int] NOT NULL,
  [CalendarID] [int] NOT NULL,
  [LineItem] [int] NOT NULL,
  [EmpCode] [nvarchar](15) NOT NULL,
  [InDate] [date] NOT NULL,
  [IsAttend] [bit] NOT NULL,
  [Remark] [nvarchar](500) NULL,
  [CreatedOn] [datetime] NOT NULL,
  [CreatedBy] [nvarchar](15) NOT NULL,
  [ChangedOn] [datetime] NULL,
  [ChangedBy] [nvarchar](15) NULL,
 CONSTRAINT [PK_TRTrainingAttendance] PRIMARY KEY CLUSTERED 
(
  [TrainNo] ASC,
  [CalendarID] ASC,
  [LineItem] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO