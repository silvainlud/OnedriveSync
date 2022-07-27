using System.Net.Http.Headers;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using File = System.IO.File;

namespace KeepassSync.Business.Graph;

public class GraphAuthenticationProvider : IAuthenticationProvider
{

	private const string Tenant = "consumers";
	private const string Instance = "https://login.microsoftonline.com/";

	private IPublicClientApplication _clientApp;
	private string[] _scopes;
	private IAccount? _userAccount = null;

	public GraphAuthenticationProvider(string appId, string[] scopes)
	{

		_clientApp = PublicClientApplicationBuilder.Create(appId)
			.WithAuthority($"{Instance}{Tenant}")
			.WithDefaultRedirectUri()
			.Build();
		_clientApp.UserTokenCache.SetBeforeAccess(BeforeAccessNotification);
		_clientApp.UserTokenCache.SetAfterAccess(AfterAccessNotification);
		_scopes = scopes;

	}

	private async Task<string> GetAccessToken()
	{


		AuthenticationResult result;
		// If there is no saved user account, the user must sign-in
		_userAccount = (await _clientApp.GetAccountsAsync()).FirstOrDefault();


		if (_userAccount == null)
		{
			try
			{
				// Invoke device code flow so user can sign-in with a browser
				result = await _clientApp.AcquireTokenInteractive(_scopes)
					.ExecuteAsync();


				_userAccount = result.Account;

			}
			catch (Exception exception)
			{
				Console.WriteLine($"Error getting access token: {exception.Message}");
				return String.Empty;
			}
		}
		else
		{

			// If there is an account, call AcquireTokenSilent
			// By doing this, MSAL will refresh the token automatically if
			// it is expired. Otherwise it returns the cached token.

			result = await _clientApp
				.AcquireTokenSilent(_scopes, _userAccount)
				.ExecuteAsync();
		}


		return result.AccessToken;

	}

	public async Task AuthenticateRequestAsync(HttpRequestMessage requestMessage)
	{
		requestMessage.Headers.Authorization =
			new AuthenticationHeaderValue("bearer", await GetAccessToken());
	}

	/// <summary>
	/// Path to the token cache
	/// </summary>
	private static readonly string CacheFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location + ".msalcache.bin3";

	private static readonly object FileLock = new object();


	private static void BeforeAccessNotification(TokenCacheNotificationArgs args)
	{
		lock (FileLock)
		{
			args.TokenCache.DeserializeMsalV3(File.Exists(CacheFilePath) ? File.ReadAllBytes(CacheFilePath) : null);
		}
	}

	private static void AfterAccessNotification(TokenCacheNotificationArgs args)
	{
		// if the access operation resulted in a cache update
		if (args.HasStateChanged)
		{
			lock (FileLock)
			{
				// reflect changesgs in the persistent store
				File.WriteAllBytes(CacheFilePath, args.TokenCache.SerializeMsalV3());
			}
		}
	}
}