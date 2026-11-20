# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore
COPY ["SurakshaSetu/SurakshaSetu.csproj", "SurakshaSetu/"]
RUN dotnet restore "SurakshaSetu/SurakshaSetu.csproj"

# Copy full source and publish
COPY . .
WORKDIR "/src/SurakshaSetu"
RUN dotnet publish "SurakshaSetu.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render exposes the port in the PORT environment variable (default 8080)
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SurakshaSetu.dll"]
