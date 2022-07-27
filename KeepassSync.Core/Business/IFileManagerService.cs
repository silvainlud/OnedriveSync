namespace KeepassSync.Core.Business;

public interface IFileManagerService
{
	/// <summary>
	/// Mettre en ligne un fichier
	/// </summary>
	/// <param name="source">Chemin vers le fichiers local</param>
	/// <param name="file">Chemin vers le fichier distant</param>
	/// <returns></returns>
	public Task Upload(string source, string? file);

	/// <summary>
	/// Télécharger un fichier
	/// </summary>
	/// <param name="file">Chemin vers le fichier en ligne à récupérer</param>
	/// <param name="destination">Emplacement local où le fichier sera copié</param>
	/// <returns></returns>
	public Task Download(string? file, string destination);
}