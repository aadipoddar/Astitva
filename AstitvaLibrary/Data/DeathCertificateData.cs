using AstitvaLibrary.DataAccess;
using AstitvaLibrary.Models;
using AstitvaLibrary.Services;

namespace AstitvaLibrary.Data;

public static class DeathCertificateData
{
	public static async Task InsertDeathCertificate(Models.DeathCertificateModel deathCertificate)
	{
		// Save to database first
		await DataAccess.SqlDataAccess.SaveData(DataAccess.StoredProcedureNames.InsertDeathCertificate, deathCertificate);
		
		// Add to blockchain for immutable record
		try
		{
			await BlockchainService.AddDeathCertificateToBlockchainAsync(deathCertificate);
		}
		catch (Exception)
		{
			// Blockchain operation failed, but database save succeeded
			// Log this error in production
		}
	}

	public static async Task<Models.DeathCertificateModel> LoadDeathCertificateByUser(int UserId) =>
		(await DataAccess.SqlDataAccess.LoadData<Models.DeathCertificateModel, dynamic>(DataAccess.StoredProcedureNames.LoadDeathCertificateByUser, new { UserId })).FirstOrDefault();

	public static async Task<DeathCertificateOverviewModel> LoadDeathCertificateOverviewByUser(int UserId) =>
		(await SqlDataAccess.LoadData<DeathCertificateOverviewModel, dynamic>(StoredProcedureNames.LoadDeathCertificateOverviewByUser, new { UserId })).FirstOrDefault();
}