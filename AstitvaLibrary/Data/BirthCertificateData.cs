using AstitvaLibrary.DataAccess;
using AstitvaLibrary.Models;
using AstitvaLibrary.Services;

namespace AstitvaLibrary.Data;

public static class BirthCertificateData
{
	public static async Task InsertBirthCertificate(BirthCertificateModel birthCertificate)
	{
		// Save to database first
		await SqlDataAccess.SaveData(StoredProcedureNames.InsertBirthCertificate, birthCertificate);
		
		// Add to blockchain for immutable record
		try
		{
			await BlockchainService.AddBirthCertificateToBlockchainAsync(birthCertificate);
		}
		catch (Exception)
		{
			// Blockchain operation failed, but database save succeeded
			// Log this error in production
		}
	}

	public static async Task<BirthCertificateModel> LoadBirthCertificateByUser(int UserId) =>
		(await SqlDataAccess.LoadData<BirthCertificateModel, dynamic>(StoredProcedureNames.LoadBirthCertificateByUser, new { UserId })).FirstOrDefault();

	public static async Task<BirthCertificateOverviewModel> LoadBirthCertificateOverviewByUser(int UserId) =>
		(await SqlDataAccess.LoadData<BirthCertificateOverviewModel, dynamic>(StoredProcedureNames.LoadBirthCertificateOverviewByUser, new { UserId })).FirstOrDefault();
}