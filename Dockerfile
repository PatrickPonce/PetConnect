# --- Stage 1: Build ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar archivos .sln y TODOS los .csproj de la raíz
COPY *.sln .
COPY *.csproj . # Usamos comodín por si ayuda

# Restaurar dependencias (buscará los .csproj en /app)
RUN dotnet restore

# Copiar todo el resto del código fuente
COPY . .

# Publicar la aplicación especificando el archivo .csproj
# Asegúrate que el nombre "PetConnect.csproj" aquí sea EXACTO
RUN dotnet publish PetConnect.csproj -c Release -o /app/publish --no-restore 

# --- Stage 2: Final Image ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "PetConnect.dll"]