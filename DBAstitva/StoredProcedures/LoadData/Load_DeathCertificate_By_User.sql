CREATE PROCEDURE [dbo].[Load_DeathCertificate_By_User]
	@UserId INT
AS
BEGIN
	SELECT 
		*
	FROM 
		DeathCertificate DC
	WHERE 
		DC.UserId = @UserId;
END