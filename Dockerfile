
# Etapa 1: Compilar API con .NET 10
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore "src/Planilla.Api/Planilla.Api.csproj"

RUN dotnet publish "src/Planilla.Api/Planilla.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# Etapa 2: Ejecutar API
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8510
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8510

ENTRYPOINT ["dotnet", "Planilla.Api.dll"]
