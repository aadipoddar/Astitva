CREATE VIEW [dbo].[DeathCertificate_Overview]
	AS
SELECT
	DC.Id,
	DC.FirstName,
	DC.MiddleName,
	DC.LastName,
	DC.DateOfDeath,
	DC.Sex,
	DC.FatherName,
	DC.MotherName,
	DC.HusbandName,
	DC.WifeName,
	DC.RegistrationNo,
	DC.RegistrationDate,
	DC.DeathPlace,
	DC.Address,
	DC.MunicipalityId,
	M.Name AS MunicipalityName,
	M.City AS MunicipalityCity,
	DC.UserId,
	DC.Approved,
	DC.Status
FROM
	DeathCertificate DC
INNER JOIN
	Municipality M ON DC.MunicipalityId = M.Id