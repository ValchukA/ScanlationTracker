FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY ScanlationTracker.SeriesUpdater/ScanlationTracker.SeriesUpdater.csproj ScanlationTracker.SeriesUpdater/
RUN dotnet restore ScanlationTracker.SeriesUpdater/ScanlationTracker.SeriesUpdater.csproj

COPY ScanlationTracker.SeriesUpdater ScanlationTracker.SeriesUpdater
RUN dotnet publish ScanlationTracker.SeriesUpdater -c release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "ScanlationTracker.SeriesUpdater.dll"]
