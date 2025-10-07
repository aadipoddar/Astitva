CREATE PROCEDURE [dbo].[Load_BirthCertificateOverview_By_User]
	@UserId INT
AS
BEGIN
	SELECT 
		*
	FROM 
		BirthCertificate_Overview BC
	WHERE 
		BC.UserId = @UserId;
END