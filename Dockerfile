# Etapa 1: Build (usa el SDK para compilar)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia solo los archivos de proyecto para aprovechar cache
COPY *.csproj ./
RUN dotnet restore

# Copia el resto del código y publica
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Etapa 2: Runtime (imagen ligera con solo ASP.NET runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiamos el resultado del publish
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "test-api.dll"]
