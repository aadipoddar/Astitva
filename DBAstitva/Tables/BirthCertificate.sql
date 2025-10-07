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
    [MunicipalityId] INT NOT NULL, 
    [Status] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [FK_BirthCertificate_ToMunicipality] FOREIGN KEY (MunicipalityId) REFERENCES [Municipality](Id)
)
