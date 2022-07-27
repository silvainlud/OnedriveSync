using KeepassSync.Core.Business;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using File = System.IO.File;

namespace KeepassSync.Business.Graph;

public class OnedriveFileManagerService : IFileManagerService
{

	private GraphServiceClient _client;
	private ILogger<OnedriveFileManagerService> _logger;

	public OnedriveFileManagerService(GraphServiceClient client, ILogger<OnedriveFileManagerService> logger)
	{
		_client = client;
		_logger = logger;
	}

	public async Task Upload(string source, string? file)
	{
		using var fileStream = File.OpenRead(source);

		var uploadProps = new DriveItemUploadableProperties
		{
			AdditionalData = new Dictionary<string, object>
			{
				{ "@microsoft.graph.conflictBehavior", "replace" }
			}
		};

		var uploadSession = await _client.Me.Drive.Root
			.ItemWithPath(file)
			.CreateUploadSession(uploadProps)
			.Request()
			.PostAsync();

		// Max slice size must be a multiple of 320 KiB
		int maxSliceSize = 320 * 1024;
		var fileUploadTask =
			new LargeFileUploadTask<DriveItem>(uploadSession, fileStream, maxSliceSize);

		var totalLength = fileStream.Length;

		// Create a callback that is invoked after each slice is uploaded
		IProgress<long> progress = new Progress<long>(prog => { _logger.LogTrace("Uploaded from {SourceFile} to {TargetFile} : {Prog} bytes of {TotalLength} bytes", source, file, prog, totalLength); });

		try
		{
			// Upload the file
			var uploadResult = await fileUploadTask.UploadAsync(progress);

			if (uploadResult.UploadSucceeded)
			{
				_logger.LogInformation("Upload complete from {SourceFile} to {TargetFile}, item ID: {ItemResponseId}", source, file, uploadResult.ItemResponse.Id);
			}
			else
			{
				_logger.LogError("Upload failed from {SourceFile} to {TargetFile}", source, file);
			}

		}
		catch (ServiceException ex)
		{
			_logger.LogError("Error uploading from {SourceFile} to {TargetFile} : {Exception}", source, file, ex.ToString());
		}

	}

	public async Task Download(string? file, string destination)
	{
		DriveItem? driveItem;
		try
		{
			driveItem = await _client.Me.Drive.Root.ItemWithPath(file).Request().GetAsync();
		}
		catch (ServiceException e)
		{
			if (e.Error.Code == "itemNotFound")
			{
				driveItem = null;
			}
			else
			{
				Console.WriteLine(e);
				throw;
			}
		}

		if (driveItem == null)
		{
			_logger.LogWarning("Download failed from {SourceFile} to {TargetFile} : the file not exist on Onedrive", file, destination);
			return;
		}

		var content = await _client.Me.Drive.Items[driveItem.Id].Content.Request().GetAsync();
		if (content == null)
		{
			_logger.LogError("Download failed from {SourceFile} to {TargetFile} : the file has not content", file, destination);
			return;
		}

		await using FileStream destinationStream = System.IO.File.Create(destination);

		await content.CopyToAsync(destinationStream);
		_logger.LogInformation("Download complete from {SourceFile} to {TargetFile}", file, destination);
	}
}