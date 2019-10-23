If Not Exists (Select * from Information_Schema.Columns
			Where Table_Name = 'tbl_Currency')
Begin
	CREATE TABLE tbl_Currency
	(
		ID int Not Null Identity(1,1) Primary Key,
		FullName nvarchar(100) Null,
		ShortName nvarchar(50) Not Null,
		Code nvarchar(50) Not Null
	)
End


Alter table [dbo].[tbl_Customer_Accounts]
Add Currency nvarchar(50) NULL