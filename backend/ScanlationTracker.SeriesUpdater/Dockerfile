FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY backend/Directory.Build.props .
COPY backend/ScanlationTracker.SeriesUpdater/ScanlationTracker.SeriesUpdater.csproj ScanlationTracker.SeriesUpdater/
COPY backend/ScanlationTracker.Core/ScanlationTracker.Core.csproj ScanlationTracker.Core/
COPY backend/ScanlationTracker.Infrastructure/ScanlationTracker.Infrastructure.csproj ScanlationTracker.Infrastructure/
RUN dotnet restore ScanlationTracker.SeriesUpdater/ScanlationTracker.SeriesUpdater.csproj

COPY backend/ScanlationTracker.SeriesUpdater ScanlationTracker.SeriesUpdater
COPY backend/ScanlationTracker.Core ScanlationTracker.Core
COPY backend/ScanlationTracker.Infrastructure ScanlationTracker.Infrastructure
RUN dotnet publish ScanlationTracker.SeriesUpdater \
    --no-restore \
    -c release \
    -o /app \
    /p:UseAppHost=false \
    /p:RunAnalyzers=false \
    /p:GenerateDocumentationFile=false

FROM base AS final
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "ScanlationTracker.SeriesUpdater.dll"]
