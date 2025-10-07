CREATE VIEW [dbo].[BirthCertificate_Overview]
	AS
SELECT
	BC.Id,
	BC.FirstName,
	BC.MiddleName,
	BC.LastName,
	BC.DateOfBirth,
	BC.Sex,
	BC.FatherName,
	BC.MotherName,
	BC.RegistrationNo,
	BC.RegistrationDate,
	BC.BirthPlace,
	BC.Address,
	BC.MunicipalityId,
	M.Name AS MunicipalityName,
	M.City AS MunicipalityCity,
	BC.UserId,
	BC.Approved,
	BC.Status
FROM
	BirthCertificate BC

INNER JOIN
	Municipality M ON BC.MunicipalityId = M.Id