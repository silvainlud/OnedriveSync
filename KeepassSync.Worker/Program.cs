using DesktopNotifications;
using DesktopNotifications.FreeDesktop;
using KeepassSync.Business;
using KeepassSync.Core.Business;
using KeepassSync.Worker;

var notificationManager = new FreeDesktopNotificationManager();
await notificationManager.Initialize();

IHost host = Host.CreateDefaultBuilder(args)
	.ConfigureServices((hostContext, services) =>
	{

		IConfiguration configuration = hostContext.Configuration;

		services.AddBusiness(configuration, notificationManager);

		Dictionary<string, string> mappedTargetFiles = configuration.GetRequiredSection("TargetFile").Get<Dictionary<string, string>>();

		services.AddSingleton<IHostedService>(o => new Worker(
				o.GetRequiredService<ILogger<Worker>>(), o.GetRequiredService<IFileManagerService>()
				, o.GetRequiredService<IFileBackupService>(), mappedTargetFiles,
				o.GetRequiredService<INotificationManager>()
			)
		);

	})
	.Build();

await host.RunAsync();