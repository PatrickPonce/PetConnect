# --- Stage 1: Build ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar archivos .sln y .csproj (asumiendo que ambos están en la raíz)
COPY *.sln .
COPY PetConnect.csproj .  # Copia el .csproj directamente a /app

# Restaurar dependencias (buscará el .csproj en el WORKDIR actual /app)
RUN dotnet restore

# Copiar todo el resto del código fuente (Controllers, Views, etc.)
COPY . .

# Publicar la aplicación especificando el archivo .csproj
# Ya estamos en /app, donde está el .csproj
RUN dotnet publish PetConnect.csproj -c Release -o /app/publish --no-restore

# --- Stage 2: Final Image ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "PetConnect.dll"]