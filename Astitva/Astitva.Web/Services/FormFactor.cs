using Astitva.Shared.Services;

namespace Astitva.Web.Services;

public class FormFactor : IFormFactor
{
	public string GetFormFactor() =>
		"Web";

	public string GetPlatform() =>
		Environment.OSVersion.ToString();
}
