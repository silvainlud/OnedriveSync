using KeepassSync.Core.Business;

namespace KeepassSync.Worker;

public class Worker : BackgroundService
{
	private readonly ILogger<Worker> _logger;
	private readonly List<FileSystemWatcher> _fileWatchers = new();
	private readonly IFileManagerService _fileManagerService;
	private readonly IFileBackupService _fileBackupService;

	private readonly Dictionary<string, string?> _mappedTargetFiles;

	static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

	public Worker(ILogger<Worker> logger, IFileManagerService fileManagerService, IFileBackupService fileBackupService, Dictionary<string, string?> mappedTargetFiles)
	{
		_logger = logger;
		_fileManagerService = fileManagerService;
		_fileBackupService = fileBackupService;
		_mappedTargetFiles = mappedTargetFiles;
	}

	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		return Task.CompletedTask;
	}

	public override async Task StartAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Service Starting");

		foreach (var (source, target) in _mappedTargetFiles)
		{
			await _fileBackupService.Backup(source, async x =>
			{
				await _fileManagerService.Upload(source, target);
				await _fileManagerService.Download(target, source);
			});


			if (!File.Exists(source))
			{
				_logger.LogWarning("Please make sure the File [{InputFile}] exists, then restart the service", source);
				continue;
			}

			_logger.LogInformation("Binding Events from File: {InputFile}", source);
			var watcher = new FileSystemWatcher(Path.GetDirectoryName(source) ?? String.Empty, Path.GetFileName(source))
			{
				NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName |
				               NotifyFilters.DirectoryName | NotifyFilters.Size
			};


			watcher.EnableRaisingEvents = true;
			watcher.Error += OnError;
			watcher.IncludeSubdirectories = false;

			watcher.Changed += Input_OnChanged;
			watcher.Deleted += Input_OnChanged;
			watcher.Renamed += Input_OnChanged;
			watcher.Created += Input_OnChanged;


			_fileWatchers.Add(watcher);
		}


		await base.StartAsync(cancellationToken);
	}

	private void OnError(object sender, ErrorEventArgs e)
	{
		_logger.LogError(e.GetException()?.StackTrace);
	}

	private async void Input_OnChanged(object source, FileSystemEventArgs e)
	{

		if (e.ChangeType == WatcherChangeTypes.Deleted || !File.Exists(e.FullPath))
		{
			return;
		}

		if (source is FileSystemWatcher watcher)
		{
			if (_mappedTargetFiles.TryGetValue(e.FullPath, out var destination))
			{

				_logger.LogInformation("Trigger File {File}", e.FullPath);
				await Semaphore.WaitAsync();
				try
				{
					watcher.EnableRaisingEvents = false;

					await _fileManagerService.Upload(e.FullPath, destination);
					Thread.Sleep(1000);
					await _fileManagerService.Download(destination, e.FullPath);

				}
				catch (Exception exception)
				{
					Console.WriteLine(exception);
				}
				finally
				{
					Semaphore.Release();
					watcher.EnableRaisingEvents = true;
				}

			}
		}
	}

	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		foreach (FileSystemWatcher fileWatcher in _fileWatchers)
		{
			_logger.LogInformation("Stopping Service for file {InputFile}", fileWatcher.Path + "/" + fileWatcher.Filter);
			fileWatcher.EnableRaisingEvents = false;
		}

		foreach (var (source, target) in _mappedTargetFiles)
		{
			if (File.Exists(source))
				await _fileBackupService.Backup(source);
		}


		await base.StopAsync(cancellationToken);
	}

	public override void Dispose()
	{
		_logger.LogInformation("Disposing Service");
		foreach (FileSystemWatcher watcher in _fileWatchers)
		{
			watcher.Dispose();
		}

		base.Dispose();
	}
}