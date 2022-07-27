using KeepassSync.Business;
using KeepassSync.Core.Business;
using KeepassSync.Worker;

IHost host = Host.CreateDefaultBuilder(args)
	.UseSystemd()
	.ConfigureServices((hostContext, services) =>
	{

		IConfiguration configuration = hostContext.Configuration;

		services.AddBusiness(configuration);

		Dictionary<string, string> mappedTargetFiles = configuration.GetRequiredSection("TargetFile").Get<Dictionary<string, string>>();

		services.AddSingleton<IHostedService>(o => new Worker(
				o.GetRequiredService<ILogger<Worker>>(), o.GetRequiredService<IFileManagerService>()
				, o.GetRequiredService<IFileBackupService>(), mappedTargetFiles)
		);

	})
	.Build();

await host.RunAsync();