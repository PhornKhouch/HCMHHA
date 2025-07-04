ALTER PROC [dbo].[HR_RPT_BankListFPay] (@Company nvarchar (100),@InMonths date,@BankName nvarchar(100),@Branch nvarchar(200),@Divison nvarchar(100)) as
begin

		SELECT G.EmpCode,G.BankAcc,M.AllName,M.OthAllName,M.Salary,G.NetWage,G.BankName,M.StartDate,G.TXPayType,INYear,
		INMonth,G.ToDate,'Salary for ' + FORMAT(G.ToDate,'MMM') as Transactions,G.WorkDay,G.ActWorkDay,
		HRDivision.Description Division,HRPosition.Description AS Position,M.Phone1,G.BankFee AS Commission,
		(SELECT SUM(HG.NetWage+HG.BankFee) FROM HISGenFirstPay HG INNER JOIN HRStaffProfile HM ON HG.EmpCode=HM.EmpCode WHERE HG.INYear=G.INYear 
		AND HG.INMonth=G.INMonth AND HG.BankName=G.BankName 
		AND (HG.Branch IN (SELECT Code FROM HRBranch WHERE CODE IN	(SELECT Value FROM fn_Split(@Branch, ','))))
		AND (@Divison IS NULL OR @Divison='' OR  HG.Division IN (SELECT Code FROM HRDivision WHERE CODE IN	(SELECT Value FROM fn_Split(@Divison, ','))))) Total,
		(G.NetWage+G.BankFee) AS GrandTotal,HRBranch.Description as BranchDesc,HRBankAccount.AccountNo AS Com_BankAcc,M.checkSum,HRCompany.Images AS Com_Image,
		HRBranch.Address as Com_Adress,HRBankAccount.AccountName as Com_AccName,G.BankBranch,M.BankAccType,
		dbo.fnNumberToWord((SELECT SUM(HG.NetWage + isnull(HG.BankFee,0)) FROM HISGenFirstPay HG INNER JOIN HRStaffProfile HM ON HG.EmpCode=HM.EmpCode WHERE HG.INYear=G.INYear
		AND HG.INMonth=G.INMonth AND HG.BankName=G.BankName and HM.IsHold!=1 
		AND (HG.Branch IN (SELECT Code FROM HRBranch WHERE CODE IN	(SELECT Value FROM fn_Split(@Branch, ','))))),'USD') W_Total,
		HRBankAccount.ReferenceCode as BranchCode,HRBankBranch.Address AS Bank_Address
		FROM HISGenFirstPay G INNER JOIN HRStaffProfile M ON G.EmpCode=M.EmpCode
		LEFT Join HRCompany ON HRCompany.Company=M.CompanyCode
		INNER join HRBranch on G.Branch=HRBranch.Code and HRBranch.CompanyCode=M.CompanyCode
		LEFT Join HRBankAccount ON HRBankAccount.Bank=G.BankName and HRBankAccount.Branch=G.Branch
		LEFT JOIN HRDivision ON G.Division=HRDivision.Code
		Left Join HRBankBranch ON HRBankBranch.Code=G.BankBranch
		LEFT JOIN HRPosition ON HRPosition.Code=G.POST 
		WHERE M.IsHold!=1 and
		--G.BankName='ABA'
	  (@Company IS NULL OR @Company='' OR  M.CompanyCode IN (SELECT Value FROM fn_Split(@Company, ','))) AND
	  (@BankName IS NULL OR @BankName='' OR  G.BankName=@BankName) AND G.INYear=YEAR(@InMonths) AND G.INMonth=MONTH(@InMonths) AND
	  (G.Branch IN (SELECT Code FROM HRBranch WHERE CODE IN	(SELECT Value FROM fn_Split(@Branch, ',')))) 
	  AND (@Divison IS NULL OR @Divison='' OR  G.Division IN (SELECT Code FROM HRDivision WHERE CODE IN	(SELECT Value FROM fn_Split(@Divison, ','))))

end
