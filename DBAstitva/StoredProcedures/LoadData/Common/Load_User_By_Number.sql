CREATE PROCEDURE [dbo].[Load_User_By_Number]
	@Number VARCHAR(10)
AS
BEGIN
	SELECT * FROM [dbo].[User] WHERE [Number] = @Number
END