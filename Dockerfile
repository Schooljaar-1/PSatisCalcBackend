# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project file from the subfolder
COPY ["SCB.WebApi/SCB.WebApi.csproj", "SCB.WebApi/"]
RUN dotnet restore "SCB.WebApi/SCB.WebApi.csproj"

# Copy everything else and publish
COPY . .
WORKDIR "/src/SCB.WebApi"
RUN dotnet publish "SCB.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
# Ensure this matches your project's assembly name
ENTRYPOINT ["dotnet", "SCB.WebApi.dll"]