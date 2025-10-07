CREATE PROCEDURE [dbo].[Insert_User]
	@Id INT,
	@Number VARCHAR(10),
	@Name VARCHAR(50) = NULL,
	@Status BIT
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[User] (Name, Number, Status)
		VALUES (@Name, @Number, @Status);
	END

	ELSE
	BEGIN
		UPDATE [dbo].[User]
		SET Name = @Name,
			Number = @Number,
			Status = @Status
		WHERE Id = @Id;
	END
END;