FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY ChurchAdmin.sln .
COPY src/ChurchAdmin.Domain/ChurchAdmin.Domain.csproj src/ChurchAdmin.Domain/
COPY src/ChurchAdmin.Application/ChurchAdmin.Application.csproj src/ChurchAdmin.Application/
COPY src/ChurchAdmin.Infrastructure/ChurchAdmin.Infrastructure.csproj src/ChurchAdmin.Infrastructure/
COPY src/ChurchAdmin.Api/ChurchAdmin.Api.csproj src/ChurchAdmin.Api/

RUN dotnet restore src/ChurchAdmin.Api/ChurchAdmin.Api.csproj

COPY . .

RUN dotnet publish src/ChurchAdmin.Api/ChurchAdmin.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}

EXPOSE 8080

ENTRYPOINT ["dotnet", "ChurchAdmin.Api.dll"]
