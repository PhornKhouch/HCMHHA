ALTER TABLE [dbo].[PRParameter] DROP CONSTRAINT [DF_PR_PARAMETER_WHOUR]
GO
alter Table PRParameter alter column WHOUR Decimal(18,2)