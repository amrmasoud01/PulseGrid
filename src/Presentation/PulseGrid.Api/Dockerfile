FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Core/PulseGrid.Domain/PulseGrid.Domain.csproj", "src/Core/PulseGrid.Domain/"]
COPY ["src/Core/PulseGrid.Application/PulseGrid.Application.csproj", "src/Core/PulseGrid.Application/"]
COPY ["src/Infrastructure/PulseGrid.Infrastructure/PulseGrid.Infrastructure.csproj", "src/Infrastructure/PulseGrid.Infrastructure/"]
COPY ["src/Infrastructure/PulseGrid.BackgroundJobs/PulseGrid.BackgroundJobs.csproj", "src/Infrastructure/PulseGrid.BackgroundJobs/"]
COPY ["src/Presentation/PulseGrid.Api/PulseGrid.Api.csproj", "src/Presentation/PulseGrid.Api/"]

RUN dotnet restore "src/Presentation/PulseGrid.Api/PulseGrid.Api.csproj"

COPY ["src/Core/PulseGrid.Domain/", "src/Core/PulseGrid.Domain/"]
COPY ["src/Core/PulseGrid.Application/", "src/Core/PulseGrid.Application/"]
COPY ["src/Infrastructure/PulseGrid.Infrastructure/", "src/Infrastructure/PulseGrid.Infrastructure/"]
COPY ["src/Infrastructure/PulseGrid.BackgroundJobs/", "src/Infrastructure/PulseGrid.BackgroundJobs/"]
COPY ["src/Presentation/PulseGrid.Api/", "src/Presentation/PulseGrid.Api/"]

WORKDIR "/src/src/Presentation/PulseGrid.Api"
RUN dotnet build "PulseGrid.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PulseGrid.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PulseGrid.Api.dll"]
