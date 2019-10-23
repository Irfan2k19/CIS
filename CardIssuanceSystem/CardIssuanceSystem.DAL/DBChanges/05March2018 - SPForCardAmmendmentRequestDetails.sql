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
-- Description:	Ammedment Report Data
-- =============================================
ALTER PROCEDURE sp_GetCardAmmendmentRequestDetails 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Select
	 Creator = (select u.UserName from tbl_Users u inner join tbl_User_Log ul on ul.ActionUserStamp = u.ID where ul.EventName = 'i' AND ul.EntityID = r.ID)
	,Authorizer = (select u.UserName from tbl_Users u inner join tbl_User_Log ul on ul.ActionUserStamp = u.ID where ul.EventName = 'u' AND ul.EntityID = r.ID AND ul.ActionUserStamp NOT IN(select ul.ActionUserStamp from tbl_User_Log ul where ul.EventName = 'i' AND ul.EntityID = r.ID))
	,ModifyDate= (select MAX(ul.ActionTimestamp) from tbl_User_Log ul where ul.EntityID = r.ID AND ul.EventName = 'u')
	,RequestNo = r.ID
	,AccountNo = r.AccountNo 
	,CardType = ct.Title
	,CardNo = r.CardNo
	,CardTitle = r.CardTitle
	,BranchCode = Case when LEN(r.AccountNo) >= 13 THEN SUBSTRING(r.AccountNo,1,4) ELSE '0002' END
	,BranchName = (select Title from tbl_Region where Description = Case when LEN(r.AccountNo) >= 13 THEN SUBSTRING(r.AccountNo,1,4) ELSE '0002' END)
	,AssociateAddress = ca.Address
	,Address1 = ca.Address
	,Address2=ca.Address2
	,Address3=ca.Address3
	,CardStatus=cc.CardStatusActive
	,Phone = ca.LandlineNo
	,CellNo = ca.MainMobile
	,EmailAddress = ca.Email
	,NIC = ca.CNIC
	,PassportNo = ca.PassportNo
	,DateOfBirth = ca.DateofBirth
	,MotherMaidenName = ca.MotherMaidenName
	,AuthorizationStatus = r.AuthorizationStatus
	from tbl_Requests r
	inner join tbl_Card_Types ct on ct.ID = r.CardTypeID
	inner join tbl_Customer_Accounts ca on ca.AccountNo = r.AccountNo
	inner join tbl_Customer_Cards cc on cc.CardNo = r.CardNo
	where r.IsActive = 1 AND r.RequestType = 'A' 
END
GO
