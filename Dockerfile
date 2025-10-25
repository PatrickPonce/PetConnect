# --- Stage 1: Build ---
# Usamos la imagen del SDK de .NET 8 (ajusta la versión si usas otra)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar archivos de solución y proyecto primero para cachear las capas de dependencias
COPY *.sln .
COPY PetConnect/*.csproj ./PetConnect/
# (Si tienes más proyectos .csproj en otras carpetas, cópialos aquí también)

# Restaurar dependencias
RUN dotnet restore *.sln

# Copiar todo el resto del código fuente
COPY . .

# Mover al directorio del proyecto principal
WORKDIR /app/PetConnect

# Publicar la aplicación en modo Release
RUN dotnet publish -c Release -o /app/publish

# --- Stage 2: Final Image ---
# Usamos la imagen de ASP.NET Runtime (más ligera)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copiar los artefactos publicados desde la etapa de build
COPY --from=build /app/publish .

# Exponer el puerto que Render usará (Render prefiere 80 o 10100, pero configuraremos ASP.NET Core para usar el puerto que Render asigne)
# Exponemos 8080 como un estándar común que podemos configurar
EXPOSE 8080

# Establecer la variable de entorno para que ASP.NET Core escuche en el puerto correcto
# Render inyectará la variable PORT, ASPNETCORE_URLS tiene precedencia si ambas existen
# Usamos http://+:8080 para escuchar en todas las interfaces dentro del contenedor en el puerto 8080
ENV ASPNETCORE_URLS=http://+:8080

# Punto de entrada para ejecutar la aplicación
ENTRYPOINT ["dotnet", "PetConnect.dll"]