FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["KeepassSyncService/KeepassSyncService.csproj", "KeepassSyncService/"]
RUN dotnet restore "KeepassSyncService/KeepassSyncService.csproj"
COPY . .
WORKDIR "/src/KeepassSyncService"
RUN dotnet build "KeepassSyncService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KeepassSyncService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KeepassSyncService.dll"]
