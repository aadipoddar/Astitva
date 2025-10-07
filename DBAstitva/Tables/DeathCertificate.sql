CREATE TABLE [dbo].[DeathCertificate]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
	[FirstName] VARCHAR(50) NOT NULL, 
	[MiddleName] VARCHAR(50) NULL, 
	[LastName] VARCHAR(50) NULL, 
	[DateOfDeath] DATE NOT NULL, 
	[Sex] VARCHAR(10) NOT NULL, 
	[FatherName] VARCHAR(100) NULL, 
	[MotherName] VARCHAR(100) NULL, 
	[HusbandName] VARCHAR(100) NULL,
	[WifeName] VARCHAR(100) NULL,
	[RegistrationNo] INT NULL, 
	[RegistrationDate] DATE NOT NULL,
	[DeathPlace] VARCHAR(250) NULL,
	[Address] VARCHAR(250) NULL,
	[MunicipalityId] INT NOT NULL, 
	[UserId] INT NOT NULL, 
	[Approved] BIT NOT NULL DEFAULT 0, 
	[Status] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [FK_DeathCertificate_ToMunicipality] FOREIGN KEY (MunicipalityId) REFERENCES [Municipality](Id)
)
