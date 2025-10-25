# --- Stage 1: Build ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar archivo de solución primero
COPY *.sln .

# --- AJUSTE AQUÍ ---
# 1. Crear explícitamente el directorio del proyecto DENTRO de /app
RUN mkdir ./PetConnect
# 2. Copiar el archivo .csproj al directorio recién creado
COPY PetConnect/PetConnect.csproj ./PetConnect/
# (Si tuvieras más proyectos .csproj en otras carpetas, repite mkdir y COPY para cada uno)
# --- FIN DEL AJUSTE ---

# Restaurar dependencias usando el archivo .sln (encontrará los proyectos)
RUN dotnet restore *.sln

# Copiar todo el resto del código fuente
# El punto '.' significa copiar todo desde la raíz del contexto de build a /app
COPY . .

# Publicar la aplicación
# Cambiamos al directorio del proyecto para el comando publish
WORKDIR /app/PetConnect 
RUN dotnet publish -c Release -o /app/publish --no-restore

# --- Stage 2: Final Image ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "PetConnect.dll"]