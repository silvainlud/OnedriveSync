using KeepassSync.Core.Business;
using Microsoft.Extensions.Logging;

namespace KeepassSync.Business;

public class FileBackupService : IFileBackupService
{
	private ILogger<FileBackupService> _logger;

	private string ApplicationFolder
	{
		get => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.kepasssync/backup/";
	}

	public FileBackupService(ILogger<FileBackupService> logger)
	{
		_logger = logger;
	}

	public Task<bool> Backup(string inputFile)
	{
		if (!File.Exists(inputFile))
		{
			return Task.FromResult(false);
		}
		if (!Directory.Exists(ApplicationFolder))
		{
			Directory.CreateDirectory(ApplicationFolder);
		}

		string destinationPath = ApplicationFolder + DateTime.Now.ToString("dd-MM-yy_hh-mm-ss") + FormatFilename(inputFile);
		_logger.LogInformation("Backup {InputFile} : {DestinationFile}", inputFile, destinationPath);

		File.Copy(inputFile, destinationPath);

		CleanUp(inputFile);

		return Task.FromResult(true);

	}

	public async Task<bool> Backup(string inputFile, Func<string, Task> callable)
	{
		var state = await Backup(inputFile);

		await callable.Invoke(inputFile);

		return state;
	}

	public void CleanUp(string inputFile)
	{
		if (!File.Exists(inputFile))
		{
			return;
		}
		DateTime writeTimeUtc = File.GetLastWriteTimeUtc(inputFile);

		inputFile = FormatFilename(inputFile);

		Directory.GetFiles(ApplicationFolder)
			.Where(x => x.EndsWith(inputFile))
			.Where(x => File.GetLastWriteTimeUtc(x) < writeTimeUtc.AddMonths(-1))
			.ToList().ForEach(x =>
			{
				_logger.LogInformation("Clean Up {InputFile} : delete {File}", inputFile, x);
				File.Delete(x);
			});


	}

	private string FormatFilename(string filename)
	{
		return filename.Replace("/", "_").Replace("\\", "_");
	}
}