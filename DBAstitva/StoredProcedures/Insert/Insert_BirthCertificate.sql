CREATE PROCEDURE [dbo].[Insert_BirthCertificate]
	@Id INT OUTPUT,
	@FirstName VARCHAR(50),
	@MiddleName VARCHAR(50) = NULL,
	@LastName VARCHAR(50) = NULL,
	@DateOfBirth DATE,
	@Sex VARCHAR(10),
	@FatherName VARCHAR(100) = NULL,
	@MotherName VARCHAR(100) = NULL,
	@RegistrationNo INT = NULL,
	@RegistrationDate DATE,
	@BirthPlace VARCHAR(250) = NULL,
	@Address VARCHAR(250) = NULL,
	@MunicipalityId INT,
	@UserId INT,
	@Approved BIT = 0,
	@Status BIT = 1
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[BirthCertificate] (FirstName, MiddleName, LastName, DateOfBirth, Sex, FatherName, MotherName, RegistrationNo, RegistrationDate, BirthPlace, Address, MunicipalityId, UserId, Approved, Status)
		VALUES (@FirstName, @MiddleName, @LastName, @DateOfBirth, @Sex, @FatherName, @MotherName, @RegistrationNo, @RegistrationDate, @BirthPlace, @Address, @MunicipalityId, @UserId, @Approved, @Status);
		SET @Id = SCOPE_IDENTITY();
	END
	ELSE
	BEGIN
		UPDATE [dbo].[BirthCertificate]
		SET FirstName = @FirstName,
			MiddleName = @MiddleName,
			LastName = @LastName,
			DateOfBirth = @DateOfBirth,
			Sex = @Sex,
			FatherName = @FatherName,
			MotherName = @MotherName,
			RegistrationNo = @RegistrationNo,
			RegistrationDate = @RegistrationDate,
			BirthPlace = @BirthPlace,
			Address = @Address,
			MunicipalityId = @MunicipalityId,
			UserId = @UserId,
			Approved = @Approved,
			Status = @Status
		WHERE Id = @Id;
	END

	SELECT @Id AS Id;
END