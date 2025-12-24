# 1. Usar la imagen del SDK para construir la app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar todo y restaurar dependencias
COPY . .
RUN dotnet restore

# Publicar la app en modo Release
RUN dotnet publish -c Release -o out

# 2. Usar la imagen ligera de ASP.NET para correr la app
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Exponer el puerto que usa Render (automático)
ENV ASPNETCORE_URLS=http://+:8080

# Comando de inicio
ENTRYPOINT ["dotnet", "Financiera.Backend.dll"]