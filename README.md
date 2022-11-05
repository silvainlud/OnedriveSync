# OnedriveSync

Synchonization de fichier vers Onedrive.

## Fonctionnalités 

- Authentification au service de Onedrvice (cf. [Microsoft Graph](https://learn.microsoft.com/fr-fr/graph/overview))
- Détection des modification de fichiers inscrits dans le fichier de configuration
- Upload automatique lors de modifications d'un fichier inscrits dans la configuration du service, avec backup en cas d'échec

## Technologie

Ce projet implémente un [Service Worker](https://learn.microsoft.com/en-us/dotnet/core/extensions/workers) fournit par C# .Net Core 6. 
Il utilise éagelement [FileSystemWatcher](https://learn.microsoft.com/en-us/dotnet/api/system.io.filesystemwatcher?view=net-7.0) pour 
la détection de modification de fichiers, couplé à [Microsoft Graph](https://learn.microsoft.com/fr-fr/graph/overview) pour l'upload vers
Onedrive

Cette application utilise un décupage N-Tiers :
- `Entity` : les objets nescessaire pour le bon fonctionnement de l'application (ex : Modèle du fichier de configuration)
- `Core` : interface des services
- `Business` : la logique métier (ex : upload/download de fichiers depuis OneDrive ce qui déclenche un backup du fichier cible)
- `Worker` : service de synchronisation 


## Licence

Ce projet est sous la licence [GNU General Public License v3.0](LICENSE) - voir le fichier  [LICENSE](LICENSE) pour les détails.
