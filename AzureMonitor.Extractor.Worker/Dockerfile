#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["AzureMonitor.Extractor.Worker/AzureMonitor.Extractor.Worker.csproj", "AzureMonitor.Extractor.Worker/"]
RUN dotnet restore "AzureMonitor.Extractor.Worker/AzureMonitor.Extractor.Worker.csproj"
COPY . .
WORKDIR "/src/AzureMonitor.Extractor.Worker"
RUN dotnet build "AzureMonitor.Extractor.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AzureMonitor.Extractor.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "AzureMonitor.Extractor.Worker.dll"]