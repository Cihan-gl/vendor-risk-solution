Vendor Risk Scoring
Modern, Dockerize edilmiş Vendor Risk Yönetimi Uygulaması
Bu proje, bir şirketin tedarikçi (vendor) risklerini yönetebilmesi için geliştirilmiş bir full-stack yazılım çözümüdür. Hem .NET Core Backend hem de React + TypeScript Frontend Docker üzerinde birlikte çalışır.
Aşağıdaki özellikleri barındırır:
✔ Vendor listeleme (pagination + arama)
✔ Vendor oluşturma / güncelleme / silme
✔ Risk hesaplama (AI-benzeri açıklama üretimi + madde madde breakdown)
✔ Latest Risk badge gösterimi
✔ Postgres + Redis entegrasyonu
✔ Temiz mimari (Domain – Application – Infrastructure – API)
✔ Docker Compose ile tek komutla tüm sistemi ayağa kaldırma


Projeyi Çalıştırma
Projeyi çalıştırmak için yalnızca tek bir komut yeterlidir.
1️⃣ Terminali aç ve backend klasörüne gir:
cd vendor-risk-solution/vendor-backend
2️⃣ Docker Compose build + up:
docker-compose build
docker-compose up
Bu işlem:
PostgreSQL’i başlatır → vendorrisk_db
Redis’i başlatır → vendorrisk_redis
.NET API’yı ayakta kaldırır → vendorrisk_api
React UI’yi build eder ve Nginx üzerinden sunar → vendorrisk_ui
🌍 Servis Adresleri
🔵 Vendor Risk API (Swagger UI)
http://localhost:5207/swagger
🟣 Vendor Dashboard UI
http://localhost:5173


Proje Mimarisi
vendor-risk-solution/
  vendor-backend/
    docker-compose.yml
    Dockerfile
    src/
      VendorRiskScoring.API/
      VendorRiskScoring.Application/
      VendorRiskScoring.Infrastructure/
      VendorRiskScoring.Domain/
  vendor-ui/
    Dockerfile
    nginx.conf
    package.json
    src/

Teknolojiler
Backend
.NET 8 / ASP.NET Core Web API
PostgreSQL 16
Redis 7
CQRS + MediatR
FluentValidation
Clean Architecture
EF Core (async repository)
Frontend
React 18
TypeScript
Vite
MantineUI
Axios
Nginx (deploy)
DevOps
Docker
docker-compose
Healthchecks
Multi-stage build
