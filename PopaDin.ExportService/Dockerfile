# Etapa base (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
RUN apk add --no-cache icu-libs icu-data-full
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Etapa de build (SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY ./PopaDin.ExportService.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Etapa final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PopaDin.ExportService.dll"]
