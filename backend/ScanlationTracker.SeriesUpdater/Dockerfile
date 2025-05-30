FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

ADD https://github.com/uBlockOrigin/uBOL-home/releases/download/uBOLite_2025.518.1611/uBOLite_2025.518.1611.chromium.mv3.zip /uBOLite.zip
RUN apt update && apt install unzip && unzip /uBOLite.zip -d /uBOLite

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

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app

ARG BrowserPath=/browser
ENV Playwright__BrowserStoragePath=$BrowserPath/storage
ENV Playwright__AdBlockExtensionPath=$BrowserPath/uBOLite
COPY --from=build /uBOLite $Playwright__AdBlockExtensionPath

COPY --from=build /app/playwright.ps1 .
COPY --from=build /app/Microsoft.Playwright.dll .
COPY --from=build /app/.playwright .playwright/
ENV PLAYWRIGHT_BROWSERS_PATH=$BrowserPath/ms-playwright
RUN --mount=type=bind,from=build,source=/usr/share/powershell,target=/powershell \
    /powershell/pwsh playwright.ps1 install chromium --with-deps --no-shell \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app .

RUN chown -R $APP_UID $BrowserPath /app/.playwright
USER $APP_UID

ENTRYPOINT ["dotnet", "ScanlationTracker.SeriesUpdater.dll"]
