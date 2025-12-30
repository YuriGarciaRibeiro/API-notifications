FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/NotificationSystem.Api/NotificationSystem.Api.csproj", "NotificationSystem.Api/"]
COPY ["src/NotificationSystem.Application/NotificationSystem.Application.csproj", "NotificationSystem.Application/"]
COPY ["src/NotificationSystem.Domain/NotificationSystem.Domain.csproj", "NotificationSystem.Domain/"]
COPY ["src/NotificationSystem.Infrastructure/NotificationSystem.Infrastructure.csproj", "NotificationSystem.Infrastructure/"]

RUN dotnet restore "NotificationSystem.Api/NotificationSystem.Api.csproj"

# Copy source code
COPY src/ .

# Build
WORKDIR "/src/NotificationSystem.Api"
RUN dotnet build "NotificationSystem.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NotificationSystem.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "NotificationSystem.Api.dll"]
