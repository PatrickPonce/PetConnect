# --- Stage 1: Build ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia SOLO el archivo .sln
COPY PetConnect.sln .

# Copia SOLO el archivo .csproj al directorio actual (/app)
# Asegúrate que el nombre "PetConnect.csproj" sea EXACTO
COPY PetConnect.csproj ./ 

# Restaura las dependencias SOLO para ese proyecto
RUN dotnet restore PetConnect.csproj

# Copia TODO el resto del código fuente al directorio actual (/app)
COPY . .

# Publica la aplicación especificando el proyecto
# (ya estamos en /app donde está el .csproj)
RUN dotnet publish PetConnect.csproj -c Release -o /app/publish --no-restore 

# --- Stage 2: Final Image ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "PetConnect.dll"]