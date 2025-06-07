CREATE TABLE HREmpJobDescription (
    TranNo INT IDENTITY(1,1) PRIMARY KEY, 
    LineItem INT NOT NULL,
    CompanyCode VARCHAR(20),
    EmpCode VARCHAR(15),
    JobResponsive NVARCHAR(MAX),
    JobDescription NVARCHAR(MAX),
    Attachment NVARCHAR(MAX),
    Document NVARCHAR(MAX),
    IsActive BIT
);
go
ALTER TABLE HREmpCode
ADD 
    BU VARCHAR(15),
    Branch VARCHAR(15);
go
ALTER TABLE HRStaffProfile
ADD 
    Currency NVARCHAR(20) not null DEFAULT 'USD',
	TeleGroupScan NVARCHAR(MAX),
	TeleGroupOT NVARCHAR(MAX),
	IsExceptScan bit not null default 0;
go
CREATE TABLE HRIdentityType (
    Code VARCHAR(10) PRIMARY KEY,
    Description VARCHAR(100),
    SecDescription VARCHAR(100)
);
go
ALTER TABLE HREmpIdentity
ADD 
    TranNo int,
    LineItem int,
    IsActive BIT,
	CompanyCode nvarchar(20),
	Document nvarchar(max);
go
CREATE TABLE PRPayPeriodItem (
    FiscalYear INT NOT NULL,
    PeriodID INT PRIMARY KEY,
    PeriodString VARCHAR(7),
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    IsActive BIT NOT NULL
);
ALTER TABLE HREmpFamily
ADD 
    Document nvarchar(max);
go
CREATE TABLE ExCFUploadMapping (
    ID INT IDENTITY(1,1) NOT NULL,
    ScreenID VARCHAR(10) NOT NULL,
    Version INT NOT NULL,
    TableName VARCHAR(50) NOT NULL,
    FieldName VARCHAR(70) NOT NULL,
    FiledIndex INT NOT NULL,
    AliasName VARCHAR(70),
    Caption VARCHAR(150),
    IsHeader BIT,
    IsGroup BIT,
    IsPrimary BIT,
    CONSTRAINT PK_ExCFUploadMapping PRIMARY KEY (
        ScreenID,
        Version,
        TableName,
        FieldName
    )
);
go
update SYMenuItem set NavigateUrl='/HRM/EmployeeInfo/HRStaffProfile' where ScreenId='HRE0000001'
update CFUploadPath set PathDirectory='~/Content/UPLOAD/' where PathCode='IMG_UPLOAD'
go
insert into CFUploadPath values('Attachment','Attachment','C:\inetpub\wwwroot\HumicaHHA\Content\UPLOAD')
go
alter table HREmpFamily 
alter column AttachFile nvarchar(max);

alter table HREmpEduc 
alter column AttachFile nvarchar(max);

alter table HREmpContract 
alter column ContractPath nvarchar(max);

alter table HREmpDisciplinary 
alter column AttachPath nvarchar(max);

alter table HREmpCertificate 
alter column AttachFile nvarchar(max);
go
insert into SYMessage values('File_Exit','EN','SY','E','File path do not exit',null)
go
INSERT INTO HREduType (Code, Description, SecDescription, Remark, LCK)
VALUES
    ('AD', N'បរិញ្ញាបត្ររង', N'Associate Degree', 5, null),
    ('BA', N'បរិញ្ញាបត្រ', N'Bachelor Degree', 4, null),
    ('ERA', N'សញ្ញាបត្រជាន់ខ្ពស់', N'Hauts Fonctionnaires', 4, null),
    ('HI', N'វិទ្យាល័យ', N'High School', 7,NULL),
    ('MA', N'បរិញ្ញាបត្រជាន់ខ្ពស់', N'Master Degree',2,NULL),
    ('PHD', N'បណ្ឌិត', N'Doctorate Degree', 1,NULL),
    ('SC', N'វគ្គបណ្ដុះបណ្ដាលខ្លីៗ និងវគ្គសិក្សា', N'Short Courses Training', 6,NULL),
    ('SE', N'អនុវិទ្យាល័យ', N'Secondary School', 8,NULL);
go
INSERT INTO HRCertificationType (Code, Description, SecDescription)
VALUES
    ('CT02', N'លិខិតបញ្ជាក់ការងារមិនមានប្រាក់បៀរវត្សរ៍', 'Employment certificate without salary'),
    ('CT03', N'លិខិតសសើរការបំពេញការងារល្អ', 'Good Job'),
    ('CT04', N'បណ្ណសសើរបុគ្គលិកឆ្នើម', 'Outstanding Staff'),
    ('CT05', N'បុគ្គលិកឆ្នើម', 'Awards Staff (Best Agent)');
go
CREATE TABLE [dbo].[ExCfFileTemplate](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[ScreenId] [nvarchar](10) NOT NULL,
	[Version] [int] NOT NULL,
	[Description] [nvarchar](150) NULL,
	[FilePath] [nvarchar](500) NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_ExCfFileTemplate] PRIMARY KEY CLUSTERED 
(
	[ScreenId] ASC,
	[Version] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
insert into ExCfFileTemplate(ScreenId,Version,Description,FilePath,IsActive)
values('HRE0000001',1,'Staff Template',null,1)
go
INSERT INTO ExCFUploadMapping (ScreenID, Version, TableName, FieldName, FiledIndex, AliasName, Caption, IsHeader, IsGroup, IsPrimary) VALUES
('HRE0000001', 1, 'HRStaffProfile', 'CompanyCode', 0, NULL, 'Company Code', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'EmpCode', 1, NULL, 'EmpCode', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'FirstName', 2, NULL, 'First Name', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'LastName', 3, NULL, 'Last Name', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'OthFirstName', 4, NULL, 'First Name(KH)', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'OthLastName', 5, NULL, 'Last Name(KH)', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'Sex', 6, NULL, 'Sex', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'Title', 7, NULL, 'Title', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'Marital', 8, NULL, 'Marital', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'CardNo', 9, NULL, 'CardNo', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'DOB', 10, NULL, 'Date of Birth', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'POB', 11, NULL, 'Place of Birth', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'Country', 12, NULL, 'Country', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'Nation', 13, NULL, 'Nationality', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'Race', 14, NULL, 'Race', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'Religion', 15, NULL, 'Religion', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'State', 16, NULL, 'Province/State', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'Phone1', 19, NULL, 'Phone1', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'Phone2', 20, NULL, 'Phone2', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'PhoneExt', 21, NULL, 'Phone Extension', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'Email', 22, NULL, 'Company Email', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'Email2', 23, NULL, 'Personal Email', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'SOCSO', 24, NULL, 'NSSF No', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'Peraddress', 25, NULL, 'Peraddress', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'PeraddressOth', 26, NULL, 'Peraddress 2', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'EmpType', 27, NULL, 'Employee Type', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'Branch', 28, NULL, 'Branch', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'LOCT', 29, NULL, 'Location', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'Division', 30, NULL, 'Division', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'DEPT', 31, NULL, 'Department', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'Office', 32, NULL, 'Office', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'SECT', 33, NULL, 'Section', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'Groups', 34, NULL, 'Team', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'JobCode', 35, NULL, 'Position', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'HODCode', 36, NULL, 'Head Of Department', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'FirstLine', 37, NULL, 'First Line', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'FirstLine2', 38, NULL, 'First Line2', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'SecondLine', 39, NULL, 'Second Line', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'SecondLine2', 40, NULL, 'Second Line2', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'ROSTER', 41, NULL, 'Roster', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'StaffType', 42, NULL, 'StaffT ype', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'Salary', 43, NULL, 'Basic Salary', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'StartDate', 44, NULL, 'Start Date', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'BankName', 45, NULL, 'Bank Name', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'BankBranch', 46, NULL, 'Bank Branch', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'BankAccName', 47, NULL, 'Bank Account Name', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'BankAcc', 48, NULL, 'Bank Account No', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'LevelCode', 49, NULL, 'Level', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'LeaveConf', 50, NULL, 'Leave Confirm Date', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'TXPayType', 51, NULL, 'Tax Pay', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'JobGrade', 52, NULL, 'Job Grade', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'CATE', 53, NULL, 'Category', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'TeleGroup', 54, NULL, 'Telegram Group', NULL, NULL, NULL),
('HRE0000001', 1, 'HRStaffProfile', 'ProbationType', 55, NULL, 'Probation Type', NULL, NULL, NULL);