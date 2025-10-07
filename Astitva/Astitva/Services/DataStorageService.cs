using Astitva.Shared.Services;

using AstitvaLibrary.Data;

namespace Astitva.Services;

public class DataStorageService : IDataStorageService
{
	public async Task SecureSaveAsync(string key, string value) =>
		await SecureStorage.Default.SetAsync(key, value);

	public async Task<string?> SecureGetAsync(string key) =>
		await SecureStorage.Default.GetAsync(key);

	public async Task SecureRemove(string key) =>
		SecureStorage.Default.Remove(key);

	public async Task SecureRemoveAll()
	{
		SecureStorage.Default.RemoveAll();

		await LocalRemove(StorageFileNames.UserDataFileName);
		await LocalRemove(StorageFileNames.BirthCertificateFileName);
		await LocalRemove(StorageFileNames.DeathCertificateFileName);
	}


	public async Task<bool> LocalExists(string key) =>
		File.Exists(Path.Combine(FileSystem.Current.AppDataDirectory, key));

	public async Task LocalSaveAsync(string key, string value) =>
		await File.WriteAllTextAsync(Path.Combine(FileSystem.Current.AppDataDirectory, key), value);

	public async Task<string?> LocalGetAsync(string key)
	{
		if (File.Exists(Path.Combine(FileSystem.Current.AppDataDirectory, key)))
			return await File.ReadAllTextAsync(Path.Combine(FileSystem.Current.AppDataDirectory, key));

		return null;
	}

	public async Task LocalRemove(string key) =>
		File.Delete(Path.Combine(FileSystem.Current.AppDataDirectory, key));
}
