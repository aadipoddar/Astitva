CREATE PROCEDURE [dbo].[Load_BirthCertificate_By_User]
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	SELECT 
		*
	FROM 
		BirthCertificate BC
	WHERE 
		BC.UserId = @UserId;
END