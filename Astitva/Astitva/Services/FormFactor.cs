using Astitva.Shared.Services;

namespace Astitva.Services;

public class FormFactor : IFormFactor
{
	public string GetFormFactor() =>
		DeviceInfo.Idiom.ToString();

	public string GetPlatform() =>
		DeviceInfo.Platform.ToString() + " - " + DeviceInfo.VersionString;
}
