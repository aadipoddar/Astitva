namespace AstitvaLibrary.DataAccess;

internal static class ConnectionStrings
{
	public static string Azure => $"Server=astitva.database.windows.net,1433;Initial Catalog={Secrets.DatabaseName};Persist Security Info=False;User ID={Secrets.DatabaseUserId};Password={Secrets.DatabasePassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;";
}
