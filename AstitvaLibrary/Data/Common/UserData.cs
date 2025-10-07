using AstitvaLibrary.DataAccess;
using AstitvaLibrary.Models.Common;

namespace AstitvaLibrary.Data.Common;

public static class UserData
{
	public static async Task InsertUser(UserModel user) =>
		await SqlDataAccess.SaveData(StoredProcedureNames.InsertUser, user);

	public static async Task<UserModel> LoadUserByNumber(string Number) =>
		(await SqlDataAccess.LoadData<UserModel, dynamic>(StoredProcedureNames.LoadUserByNumber, new { Number })).FirstOrDefault();
}
