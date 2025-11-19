FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["RegistryService/RegistryService.csproj", "RegistryService/"]
RUN dotnet restore "RegistryService/RegistryService.csproj"
COPY RegistryService/ RegistryService/
COPY version.txt RegistryService/
WORKDIR /src/RegistryService
RUN dotnet build "RegistryService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RegistryService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RegistryService.dll"]
