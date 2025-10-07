using Astitva.Shared.Services;

using Plugin.Maui.Audio;

namespace Astitva.Services;

public class SoundService : ISoundService
{
	public async Task PlaySound(string soundFileName) =>
		AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync(soundFileName)).Play();
}
