CREATE TABLE [dbo].[BirthCertificate]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [FirstName] VARCHAR(50) NOT NULL, 
    [MiddleName] VARCHAR(50) NULL, 
    [LastName] VARCHAR(50) NULL, 
    [DateOfBirth] DATE NOT NULL, 
    [Sex] VARCHAR(10) NOT NULL, 
    [FatherName] VARCHAR(100) NULL, 
    [MotherName] VARCHAR(100) NULL, 
    [RegistrationNo] INT NULL, 
    [RegistrationDate] DATE NOT NULL, 
    [BirthPlace] VARCHAR(250) NULL,
    [Address] VARCHAR(250) NULL,
    [MunicipalityId] INT NOT NULL,
    [UserId] INT NOT NULL, 
    [Approved] BIT NOT NULL DEFAULT 0, 
    [Status] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [FK_BirthCertificate_ToMunicipality] FOREIGN KEY (MunicipalityId) REFERENCES [Municipality](Id)
)
