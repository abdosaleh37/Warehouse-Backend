# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY Warehouse-Backend.slnx ./
COPY Warehouse.Api/Warehouse.Api.csproj ./Warehouse.Api/
COPY Warehouse.DataAccess/Warehouse.DataAccess.csproj ./Warehouse.DataAccess/
COPY Warehouse.Entities/Warehouse.Entities.csproj ./Warehouse.Entities/

RUN dotnet restore

COPY . .
RUN dotnet publish Warehouse.Api/Warehouse.Api.csproj -c Release -o /out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .

ENTRYPOINT ["dotnet", "Warehouse.Api.dll"]