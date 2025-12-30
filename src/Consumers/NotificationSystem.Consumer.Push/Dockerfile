FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/Consumers/NotificationSystem.Consumer.Push/NotificationSystem.Consumer.Push.csproj", "NotificationSystem.Consumer.Push/"]
COPY ["src/NotificationSystem.Application/NotificationSystem.Application.csproj", "NotificationSystem.Application/"]
COPY ["src/NotificationSystem.Domain/NotificationSystem.Domain.csproj", "NotificationSystem.Domain/"]
COPY ["src/NotificationSystem.Infrastructure/NotificationSystem.Infrastructure.csproj", "NotificationSystem.Infrastructure/"]

RUN dotnet restore "NotificationSystem.Consumer.Push/NotificationSystem.Consumer.Push.csproj"

# Copy source code
COPY src/ .

# Build
WORKDIR "/src/Consumers/NotificationSystem.Consumer.Push"
RUN dotnet build "NotificationSystem.Consumer.Push.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NotificationSystem.Consumer.Push.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "NotificationSystem.Consumer.Push.dll"]
