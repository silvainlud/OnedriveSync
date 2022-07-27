namespace KeepassSync.Entity.Configuration;

public class GraphApiConfiguration
{
	public string ClientId { get; set; } = String.Empty;
	public List<string> Scopes { get; set; } = new List<string>();
}