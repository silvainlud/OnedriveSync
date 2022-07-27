using System.Runtime.InteropServices;
using DesktopNotifications;
using DesktopNotifications.FreeDesktop;
using KeepassSync.Business.Graph;
using KeepassSync.Core.Business;
using KeepassSync.Entity.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;

namespace KeepassSync.Business;

public static class RegisterBusinessService
{
	public static void AddBusiness(this IServiceCollection services, IConfiguration configuration, INotificationManager notificationManager)
	{

		var graphApiConfiguration = configuration.GetRequiredSection("Graph").Get<GraphApiConfiguration>();

		services.AddSingleton<INotificationManager>(notificationManager);

		services.AddSingleton(o => new GraphAuthenticationProvider(graphApiConfiguration.ClientId, graphApiConfiguration.Scopes.ToArray()));
		services.AddSingleton(o => new GraphServiceClient(o.GetRequiredService<GraphAuthenticationProvider>()));

		services.AddSingleton<IFileManagerService, OnedriveFileManagerService>();
		services.AddSingleton<IFileBackupService, FileBackupService>();

	}
}