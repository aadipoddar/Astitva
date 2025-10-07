using AstitvaLibrary.DataAccess;
using AstitvaLibrary.Models;

namespace AstitvaLibrary.Data;

public static class BirthCertificateData
{
	public static async Task InsertBirthCertificate(BirthCertificateModel birthCertificate) =>
		await SqlDataAccess.SaveData(StoredProcedureNames.InsertBirthCertificate, birthCertificate);

	public static async Task<BirthCertificateModel> LoadBirthCertificateByUser(int UserId) =>
		(await SqlDataAccess.LoadData<BirthCertificateModel, dynamic>(StoredProcedureNames.LoadBirthCertificateByUser, new { UserId })).FirstOrDefault();
}