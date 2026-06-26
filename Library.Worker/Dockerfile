FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["Library.Worker/Library.Worker.csproj", "Library.Worker/"]
COPY ["Library.Application/Library.Application.csproj", "Library.Application/"]
COPY ["Library.Domain/Library.Domain.csproj", "Library.Domain/"]
COPY ["Library.Infrastructure/Library.Infrastructure.csproj", "Library.Infrastructure/"]
RUN dotnet restore "./Library.Worker/Library.Worker.csproj"
COPY . .
WORKDIR "/src/Library.Worker"
RUN dotnet build "./Library.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Library.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Library.Worker.dll"]
