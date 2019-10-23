-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Mohsin Hasan Khan
-- Create date: 05-03-2018
-- Description:	Get Card Linking Details
-- =============================================
CREATE PROCEDURE sp_CardLinkingDetail 
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Select
	 (ROW_NUMBER() OVER (ORDER BY r.ID)) as row
	,Creator = (select u.UserName from tbl_Users u inner join tbl_User_Log ul on ul.ActionUserStamp = u.ID where ul.EventName = 'i' AND ul.EntityID = r.ID)
	,Authorizer = (select u.UserName from tbl_Users u inner join tbl_User_Log ul on ul.ActionUserStamp = u.ID where ul.EventName = 'u' AND ul.EntityID = r.ID AND ul.ActionUserStamp NOT IN(select ul.ActionUserStamp from tbl_User_Log ul where ul.EventName = 'i' AND ul.EntityID = r.ID))
	,EntryDate= (select MAX(ul.ActionTimestamp) from tbl_User_Log ul where ul.EntityID = r.ID AND ul.EventName = 'i')
	,RequestId = r.ID
	,AccountNo = r.AccountNo 
	,CardType = ct.Title
	,CardNo = r.CardNo
	,CIFNo = r.CIFNo
	,BranchCode = Case when LEN(r.AccountNo) >= 13 THEN SUBSTRING(r.AccountNo,1,4) ELSE '0002' END
	,RegionTitle = (select Title from tbl_Region where Description = Case when LEN(r.AccountNo) >= 13 THEN SUBSTRING(r.AccountNo,1,4) ELSE '0002' END)
	,LinkingDelinkingAccount = r.LinkingDelinkingAccount
	,AuthorizationStatus = r.AuthorizationStatus
	INTO #Emp
	from tbl_Requests r OUTER APPLY [dbo].[StringSplit](r.LinkingDelinkingAccount,',')
	inner join tbl_Card_Types ct on ct.ID = r.CardTypeID
	inner join tbl_Customer_Accounts ca on ca.AccountNo = r.AccountNo
	inner join tbl_Customer_Cards cc on cc.CardNo = r.CardNo
	where r.IsActive = 1 AND r.RequestType = 'L' --AND ca.AccountNo in ((select * from [dbo].[StringSplit](r.LinkingDelinkingAccount,',')),r.AccountNo)
	

	DECLARE @Iterator INT
	DECLARE @RecordCount INT
	SET @Iterator = 1
	SET @RecordCount = (Select Count(*) from #Emp)

	WHILE (@Iterator <= @RecordCount)
	BEGIN
		UPDATE 
			#Emp
		SET
		LinkingDelinkingAccount = (select b.Value from (select (ROW_NUMBER() OVER (ORDER BY (SELECT NULL))) as row, * from [dbo].[StringSplit]((select LinkingDelinkingAccount from #Emp where row = @Iterator),',') a ) b
	where b.row = @Iterator)
		WHERE
			row = @Iterator
			--LinkingDelinkingAccount = (select top (1) * from [dbo].[StringSplit]((select top 1 LinkingDelinkingAccount from #Emp),',') EXCEPT select top (@Iterator - 1) * from [dbo].[StringSplit]((select top 1 LinkingDelinkingAccount from #Emp),','))
		Set @Iterator = @Iterator + 1
	END 

	Select * from #Emp 
	DROP TABLE #Emp
	END
GO

	