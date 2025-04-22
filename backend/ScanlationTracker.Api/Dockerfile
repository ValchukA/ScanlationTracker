FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY backend/Directory.Build.props .
COPY backend/ScanlationTracker.Api/ScanlationTracker.Api.csproj ScanlationTracker.Api/
RUN dotnet restore ScanlationTracker.Api/ScanlationTracker.Api.csproj

COPY backend/ScanlationTracker.Api ScanlationTracker.Api
RUN dotnet publish ScanlationTracker.Api \
    --no-restore \
    -c release \
    -o /app \
    /p:UseAppHost=false \
    /p:RunAnalyzers=false \
    /p:GenerateDocumentationFile=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "ScanlationTracker.Api.dll"]
