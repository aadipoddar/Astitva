CREATE PROCEDURE [dbo].[Insert_DeathCertificate]
	@Id INT OUTPUT,
	@FirstName VARCHAR(50),
	@MiddleName VARCHAR(50) = NULL,
	@LastName VARCHAR(50) = NULL,
	@DateOfDeath DATE,
	@Sex VARCHAR(10),
	@FatherName VARCHAR(100) = NULL,
	@MotherName VARCHAR(100) = NULL,
	@HusbandName VARCHAR(100) = NULL,
	@WifeName VARCHAR(100) = NULL,
	@RegistrationNo INT = NULL,
	@RegistrationDate DATE,
	@DeathPlace VARCHAR(250) = NULL,
	@Address VARCHAR(250) = NULL,
	@MunicipalityId INT,
	@UserId INT,
	@Approved BIT = 0,
	@Status BIT = 1
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[DeathCertificate] (FirstName, MiddleName, LastName, DateOfDeath, Sex, FatherName, MotherName, HusbandName, WifeName, RegistrationNo, RegistrationDate, DeathPlace, Address, MunicipalityId, UserId, Approved, Status)
		VALUES (@FirstName, @MiddleName, @LastName, @DateOfDeath, @Sex, @FatherName, @MotherName, @HusbandName, @WifeName, @RegistrationNo, @RegistrationDate, @DeathPlace, @Address, @MunicipalityId, @UserId, @Approved, @Status);
		SET @Id = SCOPE_IDENTITY();
	END
	ELSE
	BEGIN
		UPDATE [dbo].[DeathCertificate]
		SET FirstName = @FirstName,
			MiddleName = @MiddleName,
			LastName = @LastName,
			DateOfDeath = @DateOfDeath,
			Sex = @Sex,
			FatherName = @FatherName,
			MotherName = @MotherName,
			HusbandName = @HusbandName,
			WifeName = @WifeName,
			RegistrationNo = @RegistrationNo,
			RegistrationDate = @RegistrationDate,
			DeathPlace = @DeathPlace,
			Address = @Address,
			MunicipalityId = @MunicipalityId,
			UserId = @UserId,
			Approved = @Approved,
			Status = @Status
		WHERE Id = @Id;
	END

	SELECT @Id AS Id;
END