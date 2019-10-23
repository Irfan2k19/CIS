Create Table tbl_Sector
(
ID int Not Null Primary Key Identity(1,1),
Code varchar(50),
[Description] varchar(500),
IsActive bit 
)

Create Table tbl_OperatingInstructions
(
ID int Not Null Primary Key Identity(1,1),
Code varchar(50),
[Description] varchar(500),
IsActive bit 
)

Create Table tbl_Posting_Restrictions
(
ID int Not Null Primary Key Identity(1,1),
Code varchar(50),
[Description] varchar(500),
IsActive bit 
)