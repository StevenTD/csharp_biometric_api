FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./

RUN dotnet restore

# Copy everything else and build
COPY . ./

RUN dotnet publish -c Debug -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime

WORKDIR /app

COPY --from=build /app/out ./

ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "csharp-crud-api.dll"]
