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
-- Create date: 19-03-2018
-- Description:	Sp To Insert User Log
-- =============================================
CREATE PROCEDURE sp_AddUserLog 
	-- Add the parameters for the stored procedure here
	@EventName char(1) = null,
	@EntityName varchar(100) = null,
	@EntityID bigint = null,
	@UserId int = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	insert into tbl_User_Log values (@EventName,@EntityName,
	case when (@EntityID IS NULL OR @EntityID = 0) then (SELECT IDENT_CURRENT(@EntityName)+1) else @EntityID end, GETDATE(), @UserId)
END
GO