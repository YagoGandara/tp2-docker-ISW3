# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./src/*.csproj .
RUN dotnet restore
COPY ./src/ .
RUN dotnet publish -c Release -o /app

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# defaults (se sobreescriben en run/compose)
ENV ASPNETCORE_ENVIRONMENT=Production
ENV CONNECTION_STRING="Server=mysql;Port=3306;Database=appdb;User ID=appuser;Password=apppass;"

ENTRYPOINT ["dotnet", "tp2-docker.dll"]
