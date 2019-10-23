IF NOT EXISTS(Select top 1 * from INFORMATION_SCHEMA.COLUMNS
where TABLE_NAME = '[vt_SoneriCIS].dbo.tbl_Card_Charges' AND COLUMN_NAME = 'IsFED')

BEGIN
	ALTER TABLE [vt_SoneriCIS].dbo.tbl_Card_Charges
	ADD IsFED bit null
END


IF NOT EXISTS(Select top 1 * from INFORMATION_SCHEMA.COLUMNS
where TABLE_NAME = '[vt_SoneriCIS].dbo.tbl_Card_Charges' AND COLUMN_NAME = 'IsReplacement')

BEGIN
	ALTER TABLE [vt_SoneriCIS].dbo.tbl_Card_Charges
	ADD IsReplacement bit null
END