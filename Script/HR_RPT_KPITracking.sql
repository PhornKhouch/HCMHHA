ALTER PROC [dbo].[HR_RPT_KPITracking] (@Company nvarchar(200),@Branch nvarchar(200), @BusinessUnit nvarchar(200),@Division nvarchar(100),
    @Department nvarchar(100), @Office nvarchar(200), @Section nvarchar(100), @Group nvarchar(200),  @Position nvarchar(100),  @Level nvarchar(100),
    @InMonth as Date
) 
AS
BEGIN
    DECLARE @days NVARCHAR(MAX), @InYear NVARCHAR(4), @InMonthStr NVARCHAR(2);
    
    -- Extract year and month from @InMonth
    SET @InYear = Year(@InMonth);
    SET @InMonthStr =MOnth(@InMonth);

    -- Build the days dynamically for the current month
    SET @days = STUFF((SELECT ',[' + RIGHT('0' + CAST(DAY(T.d) AS VARCHAR(2)), 2) + ']'
                       FROM (SELECT DATEADD(DAY, n-1, DATEADD(MONTH, DATEDIFF(MONTH, 0, @InMonth), 0)) AS d
                             FROM (SELECT TOP 31 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
                                   FROM sys.objects) AS n) AS T
                       WHERE MONTH(T.d) = MONTH(@InMonth)
                       FOR XML PATH('')), 1, 1, '');

    -- Construct the dynamic SQL query
    EXEC('
    SELECT EmpCode, EmpName, Department, Position, KPIType,KPI, KPIDescription, TotalActual, ' + @days + '
    FROM (
        SELECT 
            M.EmpCode,EmpName,Department,Position,KPIType, KPI,
            KPIDescription, 
            (SELECT SUM(TA.Actual) FROM HRKPITracking TA where TA.EmpCode=M.EmpCode and TA.KPI=HRKPITracking.KPI and TA.AssignCode=HRKPITracking.AssignCode
			and  YEAR(TA.DocumentDate) = ' + @InYear + '  AND MONTH(TA.DocumentDate) = ' + @InMonthStr + ' ) TotalActual,
            RIGHT(''0'' + CAST(DAY(DocumentDate) AS VARCHAR(2)), 2) AS DocumentDay,
             SUM(Actual) Actual
        FROM 
            HRKPITracking 
            INNER JOIN HRStaffProfile M ON HRKPITracking.EmpCode = M.EmpCode
        WHERE HRKPITracking.Status=''APPROVED'' AND
            YEAR(DocumentDate) = ' + @InYear + ' 
            AND MONTH(DocumentDate) = ' + @InMonthStr + ' 
			AND (''' + @Company + ''' IS NULL OR ''' + @Company + ''' = '''' OR M.CompanyCode IN (SELECT Value FROM fn_Split(''' + @Company + ''', '','')))
			AND (''' + @Branch + ''' IS NULL OR ''' + @Branch + ''' = '''' OR M.Branch IN (SELECT Value FROM fn_Split(''' + @Branch + ''', '','')))
			AND (''' + @Division + ''' IS NULL OR ''' + @Division + ''' = '''' OR M.Division IN (SELECT Code FROM HRDivision WHERE CODE IN (SELECT Value FROM fn_Split(''' + @Division + ''', '',''))))
			AND (''' + @Department + ''' IS NULL OR ''' + @Department + ''' = '''' OR M.DEPT = ''' + @Department + ''')
			AND (''' + @Office + ''' IS NULL OR ''' + @Office + ''' = '''' OR M.Office = ''' + @Office + ''')
			AND (''' + @Section + ''' IS NULL OR ''' + @Section + ''' = '''' OR M.Sect = ''' + @Section + ''')
			AND (''' + @Group + ''' IS NULL OR ''' + @Group + ''' = '''' OR M.Groups = ''' + @Group + ''')
			AND (''' + @Position + ''' IS NULL OR ''' + @Position + ''' = '''' OR M.JobCode = ''' + @Position + ''')
			AND (''' + @Level + ''' IS NULL OR ''' + @Level + ''' = '''' OR M.LevelCode = ''' + @Level + ''')
			group by M.EmpCode,EmpName,Department, Position,KPIType, KPI,KPIDescription,DocumentDate,HRKPITracking.AssignCode
    ) AS SourceTable
    PIVOT (
        SUM(Actual) FOR DocumentDay IN (' + @days + ')
    ) AS PivotTable
    ORDER BY EmpCode;')
  
END
