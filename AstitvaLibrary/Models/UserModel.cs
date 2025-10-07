namespace AstitvaLibrary.Models;

public class UserModel
{
	public int Id { get; set; }
	public string Number { get; set; }
	public string Name { get; set; }
	public bool Status { get; set; }
}

public record AuthenticationResult(
	bool IsAuthenticated,
	UserModel User = null,
	string ErrorMessage = null);