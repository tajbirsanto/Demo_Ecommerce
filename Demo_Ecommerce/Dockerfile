# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Demo_Ecommerce.csproj", "./"]
RUN dotnet restore "Demo_Ecommerce.csproj"
COPY . .
RUN dotnet build "Demo_Ecommerce.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Demo_Ecommerce.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create persistent data directory for SQLite
RUN mkdir -p /data

EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ConnectionStrings__DefaultConnection="Data Source=/data/demoecommerce.db"
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Demo_Ecommerce.dll"]
