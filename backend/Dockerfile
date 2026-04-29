# Stage 1: build and publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first to leverage layer caching on restore
COPY Flowingly.sln ./
COPY Flowingly.Import.Api/Flowingly.Import.Api.csproj Flowingly.Import.Api/

RUN dotnet restore Flowingly.Import.Api/Flowingly.Import.Api.csproj

# Copy remaining source and publish
COPY Flowingly.Import.Api/ Flowingly.Import.Api/

RUN dotnet publish Flowingly.Import.Api/Flowingly.Import.Api.csproj \
    --configuration Release \
    --output /app/publish

# Stage 2: runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Flowingly.Import.Api.dll"]
