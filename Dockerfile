# ---------- Etapa 1: build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restaura dependências primeiro (aproveita cache do Docker quando só o código muda)
COPY LabRobotica.csproj ./
RUN dotnet restore LabRobotica.csproj

COPY . ./
RUN dotnet publish LabRobotica.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- Etapa 2: runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Fuso horário (os registros usam DateTime.Now)
ENV TZ=America/Sao_Paulo \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080 \
    DataProtection__KeysPath=/app/keys \
    DOTNET_RUNNING_IN_CONTAINER=true

# Pasta das chaves de login (montada como volume), com dono = usuário não-root "app"
RUN mkdir -p /app/keys && chown -R app:app /app/keys

COPY --from=build --chown=app:app /app/publish ./

USER app
EXPOSE 8080

ENTRYPOINT ["dotnet", "LabRobotica.dll"]
