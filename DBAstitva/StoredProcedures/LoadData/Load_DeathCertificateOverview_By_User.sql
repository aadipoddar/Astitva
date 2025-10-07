CREATE PROCEDURE [dbo].[Load_DeathCertificateOverview_By_User]
	@UserId INT
AS
BEGIN
	SELECT 
		*
	FROM 
		DeathCertificate_Overview DC
	WHERE 
		DC.UserId = @UserId;
END