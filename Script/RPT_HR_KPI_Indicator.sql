ALTER Proc [dbo].[RPT_HR_KPI_Indicator] (@Company nvarchar(200),
    @Branch nvarchar(200),    @BusinessUnit nvarchar(200),    @Division nvarchar(100),
    @Department nvarchar(100),    @Office nvarchar(200),    @Section nvarchar(100),
    @Group nvarchar(200),    @Position nvarchar(100),    @Level nvarchar(100),
    @InYear as int)
as
BEGIN
WITH MonthlyData AS (
    SELECT   AH.HandleCode as EmpCode,AH.HandleName as EmployeeName,AH.Position,AH.Department,       
		HRKPIIndicator.Description Indicator,  i.ItemCode,i.Measure,i.KPI,Year(t.DocumentDate) AS InYear,MONTH(t.DocumentDate) AS [Month],
		T.Actual,i.Target
    FROM HRKPIAssignHeader AH 
	inner join HRStaffProfile M on M.EmpCode=AH.HandleCode
    Inner JOIN HRKPIAssignItem i ON AH.AssignCode = i.AssignCode
	inner join HRKPIIndicator on HRKPIIndicator.Code=i.Indicator
    Left JOIN HRKPITracking t ON AH.AssignCode = t.AssignCode AND i.ItemCode = t.KPI
	where AH.Status='RELEASED' and Year(AH.PeriodTo)=@InYear
			and t.Status='APPROVED'
			AND ( @Company IS NULL OR  @Company = '' OR M.CompanyCode IN (SELECT Value FROM fn_Split(@Company, ',')))
			AND ( @Branch IS NULL OR  @Branch = '' OR M.Branch IN (SELECT Value FROM fn_Split( @Branch, ',')))
			AND ( @Division IS NULL OR  @Division = '' OR M.Division IN (SELECT Code FROM HRDivision WHERE CODE IN (SELECT Value FROM fn_Split( @Division, ''))))
			AND ( @Department IS NULL OR  @Department = '' OR M.DEPT =  @Department)
			AND ( @Office IS NULL OR  @Office = '' OR M.Office =  @Office)
			AND ( @Section IS NULL OR  @Section = '' OR M.Sect =  @Section)
			AND ( @Group IS NULL OR  @Group = '' OR M.Groups =  @Group)
			AND ( @Position IS NULL OR  @Position = '' OR M.JobCode =  @Position)
			AND ( @Level IS NULL OR  @Level = '' OR M.LevelCode =  @Level)
)
SELECT     Department,EmpCode,EmployeeName,    Position,    Indicator,ItemCode,    Measure,
    KPI, InYear, Target,
	ISNULL([1], 0) AS Jan, ISNULL([2], 0) AS Feb,ISNULL([3], 0) AS Mar,
    ISNULL([4], 0) AS Apr,ISNULL([5], 0) AS May,ISNULL([6], 0) AS Jun,
	ISNULL([7], 0) AS Jul,ISNULL([8], 0) AS Aug,ISNULL([9], 0) AS Sep,
	ISNULL([10], 0) AS Oct,ISNULL([11], 0) AS Nov,ISNULL([12], 0) AS Dec
FROM 
    MonthlyData
PIVOT (
    sum(Actual) FOR [Month] IN ([1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12])
) AS PivotTable
ORDER BY 
    Department, EmployeeName, Indicator;
END
