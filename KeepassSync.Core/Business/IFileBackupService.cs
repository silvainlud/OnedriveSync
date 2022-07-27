namespace KeepassSync.Core.Business;

public interface IFileBackupService
{
	public Task<bool> Backup(string inputFile);

	public Task<bool> Backup(string inputFile, Func<string, Task> callable);

	public void CleanUp(string inputFile);
}