FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# Create a user with a specific UID/GID
RUN adduser --disabled-password --gecos '' appuser --uid 1000

# Set the user to the created user
USER appuser
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["QuickUnity/QuickUnity.csproj", "QuickUnity/"]
RUN dotnet restore "QuickUnity/QuickUnity.csproj"
COPY . .
WORKDIR "/src/QuickUnity"
RUN dotnet build "QuickUnity.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "QuickUnity.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Switch back to root to create directories and change ownership
USER root
RUN mkdir -p /app/wwwroot/Storage && chown -R appuser:appuser /app/wwwroot

# Switch back to appuser to run the application
USER appuser
ENTRYPOINT ["dotnet", "QuickUnity.dll"]
