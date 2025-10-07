namespace AstitvaLibrary.DataAccess;

public static class TableNames
{
	public static string User => "User";
	public static string BirthCertificate => "BirthCertificate";
	public static string DeathCertificate => "DeathCertificate";
}

public static class StoredProcedureNames
{
	public static string LoadTableData => "Load_TableData";
	public static string LoadTableDataById => "Load_TableData_By_Id";
	public static string LoadTableDataByStatus => "Load_TableData_By_Status";
	public static string LoadUserByNumber => "Load_User_By_Number";
	public static string LoadBirthCertificateByUser => "Load_BirthCertificate_By_User";
	public static string LoadDeathCertificateByUser => "Load_DeathCertificate_By_User";

	public static string InsertUser => "Insert_User";
	public static string InsertBirthCertificate => "Insert_BirthCertificate";
	public static string InsertDeathCertificate => "Insert_DeathCertificate";
}

public static class ViewNames
{
}