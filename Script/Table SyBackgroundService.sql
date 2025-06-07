CREATE TABLE [dbo].[SyBackgroundService](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IsBirthday] [bit] NULL,
	[BDScheduleTime] [datetime] NULL,
	[BDNextRunTime] [int] NULL,
	[IsAttendance] [bit] NULL,
	[AttendanceType] [nvarchar](50) NULL,
	[AttScheduleTime] [datetime] NULL,
	[AttNextRunTime] [decimal](18, 2) NULL,
	[AttLastRunTime] [datetime] NULL,
	[IsAttWeekly] [bit] NULL,
	[WeeklyChatID] [nvarchar](100) NULL,
	[WeeklyScheduleTime] [datetime] NULL,
	[ScheduleType] [nvarchar](100) NULL,
	[IsAttMonthly] [bit] NULL,
	[MonthlyChatID] [nvarchar](100) NULL,
	[MonthlyScheduleTime] [datetime] NULL,
	[NextMonthlySchedule] [datetime] NULL,
 CONSTRAINT [PK_SyBackgroundService] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[SyServiceURL](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[URLType] [nvarchar](200) NULL,
	[URL] [nvarchar](max) NULL,
	[UserName] [nvarchar](max) NULL,
	[Password] [nvarchar](max) NULL,
 CONSTRAINT [PK_SyServiceURL] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
Insert into SyBackgroundService (IsBirthday,BDScheduleTime,BDNextRunTime,IsAttendance,AttendanceType,AttScheduleTime,AttNextRunTime)
values(1,'2024-06-13 8:00:00.000',24,1,'IP','2024-06-13 8:00:00.000',2)
