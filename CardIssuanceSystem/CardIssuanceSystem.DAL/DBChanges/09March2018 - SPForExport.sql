USE [vt_SoneriCIS]
GO
/****** Object:  StoredProcedure [dbo].[sp_ExportData]    Script Date: 03/09/2018 19:51:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Danish
-- Create date: 12 Feb 18
-- Description:	Retrieve Card Charges at the time of issuance/replacement/upgrade or at particular duration
-- =============================================
ALTER PROCEDURE [dbo].[sp_ExportData]
	-- Add the parameters for the stored procedure here
 @RequestType char(1),
 @CardTypeId int,@BranchCode varchar(500)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
select distinct [DateOfIssue]=getdate()
,[DateOfExpiry]=DATEADD (dd, -1, DATEADD(mm, DATEDIFF(mm, 0, dateadd(year,5,getdate())) + 1, 0))
,[BranchCode] = '0002'
,[AccountNo]= (case when LEN(r.AccountNo) <= 11 then '0002'+r.AccountNo else r.AccountNo end)
,[NameOnCard]=r.CardTitle
,a.[MainAddress]
,a.[MainLandline]
,a.[Mobile]
,[AssociatedAddress]=a.mainAddress
,[NameOfCustomer]=a.AccountTitle
,[DateOfBirth]=a.DateofBirth
,[MotherMaidenName]=a.MotherMaidenName
,[ResidentialPhoneNumber]=a.LandlineNo
,[MaxRetriesForPIN1]='03'
,[RetriesForPIN1]='03'
,[StatusForPIN1]='00'
,[MaxRetriesForPIN2]='03'
,[RetriesForPIN2]='03'
,[StatusForPIN2]='00'
,[MaxRetriesForPIN3]='03'
,[RetriesForPIN3]='03'
,[StatusForPIN3]='00'
,[PIN4IB]='00'
,[MaxRetriesForPIN4]='03'
,[RetriesForPIN4]='03'
,[StatusForPIN4]='00'
,[CompanyName]=a.Company
,[Address]=a.[Address]
,a.Mobile2
,[OfficeAddress1]=a.[Address2] 
,[OfficeAddress2]=a.[Address3]
,[MobileNumber]=a.mainMobile
,[CustomerNumber]=a.CIF
,[NIC]=a.CNIC
,[BillingAddress]=a.[Address]
,[MemberSince]=dateadd(year,1,getdate())
,[CustomerOf]='YYYNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN'
,[Nationality]=a.Nationality
,[PhoneOff]=a.PhoneOffice
,[Email]=a.email
,[Passport]=a.passportNo
,[BillingAddressFlag]=(case when a.MainAddress=a.[Address] then 'H' else 'O' end)
,[AccountStatus]='00'
,[Salutation] = r.Salutation
,[ID] = r.ID
,[LinkingDelinkingAccount] = r.LinkingDelinkingAccount
--,[DefaultAccount] = (case when @RequestType='L' OR @RequestType='D' then 'Y' when @RequestType='N' then 'O' else 'N' end)
,[DefaultAccount] = (case when @RequestType='L' OR @RequestType='D' then 'Y' when @RequestType='N' then 'O' else 'N' end)
,[Pan] = (case when @RequestType='N' or @RequestType='R' then
(select CardCode from tbl_Card_Types where ID = @CardTypeId)
when @RequestType='U' then
((select CardCode from tbl_Card_Types where ID = @CardTypeId)
+substring((select top 1 cc.cardno from tbl_customer_cards cc where cc.cardno=r.cardno),7,10))
else (select top 1 cc.cardno from tbl_customer_cards cc where cc.cardno=r.cardno)
 end)
 ,BankIMD=(select top 1 CardCode from tbl_Card_Types where ID = @CardTypeId)
 ,TipperFlag = (select top 1 [Description] from tbl_Card_Types ctt where ctt.ID = @CardTypeId)
 ,AccountType = (select top 1 [description] from tbl_Account_Types actt where actt.id=a.accounttypeid)
 ,Currency = isnull((select top 1 cr.Code from tbl_currency cr where cr.shortname=a.currency ),'586')
from tbl_Requests r,tbl_Customer_Accounts a

 where r.AccountNo=a.AccountNo 
 and r.IsActive=1  and r.IsExported=0 and r.CardTypeID = @CardTypeId
 and r.[AuthorizationStatus] ='A'
 and r.RequestType=@RequestType
 and (r.cardtypeid=@cardTypeId or @cardTypeId=-1)
-- and substring(r.AccountNo,1,4)=@BranchCode
END



