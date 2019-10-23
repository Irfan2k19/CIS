IF NOT EXISTS (Select top 1 * from INFORMATION_SCHEMA.COLUMNS
where TABLE_NAME = 'vt_SoneriCIS.dbo.tbl_Customer_Cards' AND COLUMN_NAME = 'CIF')
BEGIN
	Alter table vt_SoneriCIS.dbo.tbl_Customer_Cards
	Add CIF varchar(30) null
END