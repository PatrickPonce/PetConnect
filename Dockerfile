FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["PetConnect.csproj", "."]
RUN dotnet restore "./PetConnect.csproj"

COPY . .
WORKDIR "/src/."
RUN dotnet build "PetConnect.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PetConnect.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "PetConnect.dll"]