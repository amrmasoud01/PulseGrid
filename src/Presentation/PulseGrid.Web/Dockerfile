FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Core/PulseGrid.Domain/PulseGrid.Domain.csproj", "src/Core/PulseGrid.Domain/"]
COPY ["src/Core/PulseGrid.Application/PulseGrid.Application.csproj", "src/Core/PulseGrid.Application/"]
COPY ["src/Presentation/PulseGrid.Web/PulseGrid.Web.csproj", "src/Presentation/PulseGrid.Web/"]

RUN dotnet restore "src/Presentation/PulseGrid.Web/PulseGrid.Web.csproj"

COPY ["src/Core/PulseGrid.Domain/", "src/Core/PulseGrid.Domain/"]
COPY ["src/Core/PulseGrid.Application/", "src/Core/PulseGrid.Application/"]
COPY ["src/Presentation/PulseGrid.Web/", "src/Presentation/PulseGrid.Web/"]

WORKDIR "/src/src/Presentation/PulseGrid.Web"
RUN dotnet build "PulseGrid.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PulseGrid.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PulseGrid.Web.dll"]
