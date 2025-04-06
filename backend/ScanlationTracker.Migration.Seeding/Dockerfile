FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY backend/Directory.Build.props .
COPY backend/ScanlationTracker.Migration.Seeding/ScanlationTracker.Migration.Seeding.csproj ScanlationTracker.Migration.Seeding/
COPY backend/ScanlationTracker.Core/ScanlationTracker.Core.csproj ScanlationTracker.Core/
COPY backend/ScanlationTracker.Infrastructure/ScanlationTracker.Infrastructure.csproj ScanlationTracker.Infrastructure/
RUN dotnet restore ScanlationTracker.Migration.Seeding/ScanlationTracker.Migration.Seeding.csproj

COPY backend/ScanlationTracker.Migration.Seeding ScanlationTracker.Migration.Seeding
COPY backend/ScanlationTracker.Core ScanlationTracker.Core
COPY backend/ScanlationTracker.Infrastructure ScanlationTracker.Infrastructure
RUN dotnet publish ScanlationTracker.Migration.Seeding --no-restore -c release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "ScanlationTracker.Migration.Seeding.dll"]
