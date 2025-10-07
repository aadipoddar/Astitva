namespace AstitvaLibrary.Data;

public static class DeathCertificateData
{
	public static async Task InsertDeathCertificate(Models.DeathCertificateModel deathCertificate) =>
		await DataAccess.SqlDataAccess.SaveData(DataAccess.StoredProcedureNames.InsertDeathCertificate, deathCertificate);

	public static async Task<Models.DeathCertificateModel> LoadDeathCertificateByUser(int UserId) =>
		(await DataAccess.SqlDataAccess.LoadData<Models.DeathCertificateModel, dynamic>(DataAccess.StoredProcedureNames.LoadDeathCertificateByUser, new { UserId })).FirstOrDefault();
}