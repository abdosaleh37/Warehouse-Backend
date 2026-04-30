# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy solution and all project files
COPY Warehouse-Backend.sln ./
COPY Warehouse.Api/Warehouse.Api.csproj ./Warehouse.Api/
COPY Warehouse.DataAccess/Warehouse.DataAccess.csproj ./Warehouse.DataAccess/
COPY Warehouse.Entities/Warehouse.Entities.csproj ./Warehouse.Entities/

# Restore dependencies
RUN dotnet restore

# Copy everything and publish
COPY . .
RUN dotnet publish Warehouse.Api/Warehouse.Api.csproj -c Release -o /out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .

EXPOSE 10000
ENTRYPOINT ["dotnet", "Warehouse.Api.dll"]