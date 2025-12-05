# Database Configuration - Complete Summary

## ✅ Your ERP System is Now Using PostgreSQL

### What Was Changed

1. **Infrastructure Project** (`LinhGo.ERP.Infrastructure`)
   - ✅ Replaced `Microsoft.EntityFrameworkCore.SqlServer` with `Npgsql.EntityFrameworkCore.PostgreSQL`
   - ✅ Updated `DependencyInjection.cs` to use `.UseNpgsql()` instead of `.UseSqlServer()`
   - ✅ Package version: 9.0.0 (latest)

2. **Connection String** (Already correct in `appsettings.json`)
   ```json
   "DefaultConnection": "Host=localhost;Port=5432;Database=LinhGoERP;Username=postgres;Password=yourpassword"
   ```

3. **Build Status**
   - ✅ Solution builds successfully
   - ⚠️ Minor nullability warnings (not blocking)

## Why PostgreSQL Was Chosen

### Perfect for Your Multi-Company ERP System

| Feature | Why It Matters |
|---------|---------------|
| 🔒 **Row-Level Security** | Built-in isolation between companies |
| 💰 **ACID Compliance** | Critical for financial/inventory data |
| 🚀 **MVCC** | Better performance with multiple users |
| 📊 **Advanced Features** | JSON, arrays, full-text search |
| 💵 **Cost** | Completely free, no licensing |
| 📈 **Scalability** | Handles growth well |

## Next Steps

### 1. Install PostgreSQL

Choose one option:

#### Option A: Docker (Quickest)
```powershell
docker run --name linhgo-erp-postgres `
  -e POSTGRES_PASSWORD=yourpassword `
  -e POSTGRES_DB=LinhGoERP `
  -p 5432:5432 `
  -d postgres:16
```

#### Option B: Local Installation
- Download: https://www.postgresql.org/download/windows/
- Install with default settings
- Port: 5432
- Password: Set during installation

### 2. Update Connection String

Edit `appsettings.json` and `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=LinhGoERP;Username=postgres;Password=YOUR_ACTUAL_PASSWORD"
  }
}
```

### 3. Create Database & Run Migrations

```powershell
# Navigate to project root
cd E:\Projects\NET\LinhGo.ERP

# Create initial migration
dotnet ef migrations add InitialCreate `
  --project LinhGo.ERP.Infrastructure `
  --startup-project LinhGo.ERP.Api

# Apply migrations to database
dotnet ef database update `
  --project LinhGo.ERP.Infrastructure `
  --startup-project LinhGo.ERP.Api
```

### 4. Verify Database

```powershell
# Connect to PostgreSQL
psql -U postgres -h localhost -d LinhGoERP

# List tables
\dt

# Exit
\q
```

### 5. Run Your Application

```powershell
# API
cd LinhGo.ERP.Api
dotnet run

# Or Web
cd LinhGo.ERP.Web
dotnet run
```

## Database Management Tools

### Recommended: pgAdmin
- **Docker**: Included in docker-compose (http://localhost:5050)
- **Desktop**: Download from https://www.pgadmin.org/
- **Credentials**: admin@linhgo.com / admin (from docker-compose)

### Alternative: DBeaver
- Free, cross-platform
- Download: https://dbeaver.io/

## Connection String Formats

### Development (Local)
```
Host=localhost;Port=5432;Database=LinhGoERP;Username=postgres;Password=yourpassword
```

### Development (Docker)
```
Host=localhost;Port=5432;Database=LinhGoERP;Username=postgres;Password=yourpassword
```

### Production (with SSL)
```
Host=your-server.com;Port=5432;Database=LinhGoERP;Username=linhgo_user;Password=strong-password;SSL Mode=Require;Trust Server Certificate=true
```

### Azure Database for PostgreSQL
```
Host=yourserver.postgres.database.azure.com;Database=LinhGoERP;Username=linhgo_user@yourserver;Password=strong-password;SSL Mode=Require
```

### AWS RDS PostgreSQL
```
Host=yourinstance.region.rds.amazonaws.com;Port=5432;Database=LinhGoERP;Username=linhgo_user;Password=strong-password;SSL Mode=Require
```

## Docker Compose Configuration

Create `docker-compose.yml` in project root:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16
    container_name: linhgo-erp-postgres
    environment:
      POSTGRES_DB: LinhGoERP
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: yourpassword
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data
    restart: unless-stopped
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

  pgadmin:
    image: dpage/pgadmin4
    container_name: linhgo-erp-pgadmin
    environment:
      PGADMIN_DEFAULT_EMAIL: admin@linhgo.com
      PGADMIN_DEFAULT_PASSWORD: admin
    ports:
      - "5050:80"
    depends_on:
      - postgres
    restart: unless-stopped

volumes:
  postgres-data:
```

Start:
```powershell
docker-compose up -d
```

Stop:
```powershell
docker-compose down
```

## Backup & Restore

### Backup
```powershell
# Full backup
pg_dump -U postgres -h localhost -d LinhGoERP -F c -f backup_$(Get-Date -Format 'yyyyMMdd').dump

# SQL format
pg_dump -U postgres -h localhost -d LinhGoERP > backup_$(Get-Date -Format 'yyyyMMdd').sql
```

### Restore
```powershell
# From custom format
pg_restore -U postgres -h localhost -d LinhGoERP -c backup.dump

# From SQL
psql -U postgres -h localhost -d LinhGoERP < backup.sql
```

## Performance Tips

### 1. Indexes (Add after migrations)
```sql
-- Foreign key indexes
CREATE INDEX idx_orders_companyid ON "Orders"("CompanyId");
CREATE INDEX idx_orders_customerid ON "Orders"("CustomerId");
CREATE INDEX idx_orderitems_orderid ON "OrderItems"("OrderId");
CREATE INDEX idx_products_companyid ON "Products"("CompanyId");
```

### 2. Connection Pooling (Already enabled in Npgsql)
Default settings in connection string:
- Min Pool Size: 0
- Max Pool Size: 100

### 3. Query Optimization
```sql
-- Explain query plans
EXPLAIN ANALYZE SELECT * FROM "Orders" WHERE "CompanyId" = 'xxx';
```

## Monitoring

### Check Active Connections
```sql
SELECT count(*) FROM pg_stat_activity WHERE datname = 'LinhGoERP';
```

### Database Size
```sql
SELECT pg_size_pretty(pg_database_size('LinhGoERP'));
```

### Table Sizes
```sql
SELECT 
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size
FROM pg_tables
WHERE schemaname = 'public'
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;
```

## Troubleshooting

### Issue: Connection refused
```powershell
# Check if PostgreSQL is running
# Docker
docker ps | grep postgres

# Windows Service
Get-Service postgresql*
```

### Issue: Authentication failed
- Check username and password in connection string
- Verify `pg_hba.conf` allows connections

### Issue: Database doesn't exist
```powershell
# Create database
psql -U postgres -h localhost
CREATE DATABASE "LinhGoERP";
\q
```

### Issue: Port already in use
```powershell
# Check what's using port 5432
netstat -ano | findstr :5432

# Change port in connection string or stop conflicting service
```

## Documentation Files Created

1. ✅ **POSTGRESQL_SETUP_GUIDE.md** - Complete setup instructions
2. ✅ **POSTGRESQL_VS_MYSQL_COMPARISON.md** - Detailed comparison
3. ✅ **DATABASE_CONFIGURATION_SUMMARY.md** - This file

## Summary

Your LinhGo ERP system is now properly configured to use PostgreSQL:

✅ NuGet packages updated  
✅ EF Core configuration updated  
✅ Connection string correct  
✅ Build successful  
✅ Ready for migrations  

**Next Action**: Install PostgreSQL (Docker recommended) and run migrations!

## Quick Start Commands

```powershell
# 1. Start PostgreSQL (Docker)
docker run --name linhgo-erp-postgres -e POSTGRES_PASSWORD=yourpassword -e POSTGRES_DB=LinhGoERP -p 5432:5432 -d postgres:16

# 2. Update password in appsettings.json

# 3. Run migrations
cd E:\Projects\NET\LinhGo.ERP
dotnet ef database update --project LinhGo.ERP.Infrastructure --startup-project LinhGo.ERP.Api

# 4. Run application
cd LinhGo.ERP.Api
dotnet run
```

Your ERP is ready to go with PostgreSQL! 🐘✨

