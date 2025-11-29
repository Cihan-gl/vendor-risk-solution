# =========================
# 1) Runtime image
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# API portu 5207:8080 
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# =========================
# 2) Build image
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# csproj'ları kopyala 
COPY ["src/VendorRiskScoring.API/VendorRiskScoring.API.csproj", "src/VendorRiskScoring.API/"]
COPY ["src/VendorRiskScoring.Application/VendorRiskScoring.Application.csproj", "src/VendorRiskScoring.Application/"]
COPY ["src/VendorRiskScoring.Infrastructure/VendorRiskScoring.Infrastructure.csproj", "src/VendorRiskScoring.Infrastructure/"]
COPY ["src/VendorRiskScoring.Domain/VendorRiskScoring.Domain.csproj", "src/VendorRiskScoring.Domain/"]

RUN dotnet restore "src/VendorRiskScoring.API/VendorRiskScoring.API.csproj"

# tüm solution'ı kopyala
COPY . .

WORKDIR "/src/src/VendorRiskScoring.API"
RUN dotnet build "VendorRiskScoring.API.csproj" -c Release -o /app/build

# =========================
# 3) Publish
# =========================
FROM build AS publish
RUN dotnet publish "VendorRiskScoring.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# =========================
# 4) Final runtime image
# =========================
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "VendorRiskScoring.API.dll"]
