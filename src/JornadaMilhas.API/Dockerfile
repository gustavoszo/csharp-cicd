# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_CONFIGURATION=Release
EXPOSE 8080
EXPOSE 8081

WORKDIR /app/src

# Copia apenas os arquivos de projeto para otimizar cache
COPY ["JornadaMilhas.API/JornadaMilhas.API.csproj", "JornadaMilhas.API/"]
COPY ["JornadaMilhas.Dados/JornadaMilhas.Dados.csproj", "JornadaMilhas.Dados/"]
COPY ["JornadaMilhas.Dominio/JornadaMilhas.Dominio.csproj", "JornadaMilhas.Dominio/"]
COPY "JornadaMilhas-API.sln" .

# Restaura dependências
RUN dotnet restore "JornadaMilhas-API.sln"

# Copia o restante do código
COPY . .

# Compila e publica a aplicação
WORKDIR /app/src/JornadaMilhas.API
RUN dotnet publish "JornadaMilhas.API.csproj" -c $BUILD_CONFIGURATION -o /app/out /p:UseAppHost=false

# Imagem final
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "JornadaMilhas.API.dll"]
