# PostgreSQL Setup Guide for LinhGo ERP

## Why PostgreSQL?

Your ERP system is now configured to use **PostgreSQL** instead of MySQL or SQL Server. Here's why:

### Key Advantages for ERP Systems

✅ **Multi-Tenancy**: Better row-level security for multi-company data isolation  
✅ **Data Integrity**: Superior ACID compliance for financial/inventory data  
✅ **Performance**: Better concurrent access (MVCC - no read locks)  
✅ **Advanced Features**: JSON/JSONB, arrays, full-text search, materialized views  
✅ **Cost**: Completely free and open-source  
✅ **Scalability**: Handles large datasets efficiently  

## Installation

### Option 1: Local Installation (Windows)

1. **Download PostgreSQL**
   - Visit: https://www.postgresql.org/download/windows/
   - Download PostgreSQL 16.x installer
   - Run the installer

2. **Installation Settings**
   - Port: `5432` (default)
   - Superuser: `postgres`
   - Password: Set a strong password (you'll need this)
   - Locale: Default

3. **Verify Installation**
   ```powershell
   psql --version
   # Should show: psql (PostgreSQL) 16.x
   ```

### Option 2: Docker (Recommended for Development)

```powershell
# Pull PostgreSQL image
docker pull postgres:16

# Run PostgreSQL container
docker run --name linhgo-erp-postgres `
  -e POSTGRES_PASSWORD=yourpassword `
  -e POSTGRES_DB=LinhGoERP `
  -p 5432:5432 `
  -v linhgo-erp-data:/var/lib/postgresql/data `
  -d postgres:16

# Verify it's running
docker ps
```

### Option 3: Docker Compose (Best for Development)

Create `docker-compose.postgres.yml`:
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
      - linhgo-erp-data:/var/lib/postgresql/data
    restart: unless-stopped

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
  linhgo-erp-data:
```

Start:
```powershell
docker-compose -f docker-compose.postgres.yml up -d
```

## Configuration

### Connection String (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=LinhGoERP;Username=postgres;Password=yourpassword"
  }
}
```

### For Docker
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=LinhGoERP;Username=postgres;Password=yourpassword"
  }
}
```

### For Production
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-server.com;Port=5432;Database=LinhGoERP;Username=linhgo_user;Password=strong-password;SSL Mode=Require"
  }
}
```

## Database Setup

### 1. Create Database (if not exists)

```powershell
# Connect to PostgreSQL
psql -U postgres -h localhost

# Create database
CREATE DATABASE "LinhGoERP";

# Create user (for production)
CREATE USER linhgo_user WITH PASSWORD 'strong-password';
GRANT ALL PRIVILEGES ON DATABASE "LinhGoERP" TO linhgo_user;

# Exit
\q
```

### 2. Run Migrations

```powershell
# Navigate to project
cd E:\Projects\NET\LinhGo.ERP

# Create initial migration
dotnet ef migrations add InitialCreate --project LinhGo.ERP.Infrastructure --startup-project LinhGo.ERP.Api

# Apply migration to database
dotnet ef database update --project LinhGo.ERP.Infrastructure --startup-project LinhGo.ERP.Api
```

### 3. Verify Database

```powershell
# Connect
psql -U postgres -h localhost -d LinhGoERP

# List tables
\dt

# View table structure
\d "Companies"

# Exit
\q
```

## NuGet Packages (Already Configured)

Your project now includes:
- `Npgsql.EntityFrameworkCore.PostgreSQL` v9.0.0
- `Microsoft.EntityFrameworkCore` v9.0.0
- `Microsoft.EntityFrameworkCore.Design` v9.0.0
- `Microsoft.EntityFrameworkCore.Tools` v9.0.0

## Database Management Tools

### 1. pgAdmin (GUI - Recommended)
- Web-based: http://localhost:5050 (if using Docker)
- Desktop: Download from https://www.pgadmin.org/

### 2. DBeaver (Cross-platform)
- Download: https://dbeaver.io/

### 3. DataGrip (JetBrains - Paid)
- Professional tool with excellent features

### 4. psql (Command Line)
```powershell
psql -U postgres -h localhost -d LinhGoERP
```

## Common Commands

### psql Commands
```sql
-- List databases
\l

-- Connect to database
\c LinhGoERP

-- List tables
\dt

-- Describe table
\d "Companies"

-- List all schemas
\dn

-- List all users
\du

-- Show current connection
\conninfo

-- Execute SQL file
\i path/to/file.sql

-- Exit
\q
```

### Backup & Restore
```powershell
# Backup database
pg_dump -U postgres -h localhost -d LinhGoERP -F c -f backup.dump

# Restore database
pg_restore -U postgres -h localhost -d LinhGoERP -c backup.dump

# Backup as SQL
pg_dump -U postgres -h localhost -d LinhGoERP > backup.sql

# Restore from SQL
psql -U postgres -h localhost -d LinhGoERP < backup.sql
```

## PostgreSQL-Specific Features for Your ERP

### 1. JSONB for Flexible Data
```csharp
// In your entities
public class Company
{
    // ...existing properties...
    public JsonDocument? Settings { get; set; }  // Flexible settings
}
```

### 2. Arrays
```csharp
public class User
{
    public string[] Tags { get; set; }  // User tags
}
```

### 3. Full-Text Search
```sql
-- Create full-text search index
CREATE INDEX idx_products_search ON "Products" 
USING GIN (to_tsvector('english', "Name" || ' ' || "Description"));

-- Search
SELECT * FROM "Products" 
WHERE to_tsvector('english', "Name" || ' ' || "Description") 
@@ to_tsquery('english', 'laptop');
```

### 4. Row-Level Security (Multi-Tenancy)
```sql
-- Enable RLS
ALTER TABLE "Orders" ENABLE ROW LEVEL SECURITY;

-- Create policy
CREATE POLICY company_isolation ON "Orders"
FOR ALL
USING ("CompanyId" = current_setting('app.current_company_id')::uuid);
```

## Performance Tuning

### Indexes
```sql
-- Create indexes for foreign keys
CREATE INDEX idx_orders_companyid ON "Orders"("CompanyId");
CREATE INDEX idx_orders_customerid ON "Orders"("CustomerId");
CREATE INDEX idx_orderitems_orderid ON "OrderItems"("OrderId");
```

### Analyze Tables
```sql
-- Update statistics
ANALYZE "Orders";
ANALYZE "Products";

-- Vacuum (clean up)
VACUUM ANALYZE;
```

## Monitoring

### Check Connections
```sql
SELECT * FROM pg_stat_activity WHERE datname = 'LinhGoERP';
```

### Check Table Sizes
```sql
SELECT 
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size
FROM pg_tables
WHERE schemaname = 'public'
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;
```

### Slow Query Log
Add to `postgresql.conf`:
```
log_min_duration_statement = 1000  # Log queries taking > 1 second
```

## Troubleshooting

### Connection Issues
```powershell
# Check if PostgreSQL is running
# Windows
Get-Service postgresql*

# Docker
docker ps | grep postgres
```

### Reset Password
```powershell
# Edit pg_hba.conf to allow trust authentication temporarily
# Restart PostgreSQL
# Connect and change password
psql -U postgres
ALTER USER postgres PASSWORD 'newpassword';
```

### Port Already in Use
```powershell
# Check what's using port 5432
netstat -ano | findstr :5432

# Stop conflicting service or change port
```

## Production Recommendations

1. **Use Connection Pooling** (PgBouncer)
2. **Enable SSL** connections
3. **Regular Backups** (automated with pg_dump)
4. **Monitoring** (pg_stat_statements)
5. **Separate Read Replicas** for reporting
6. **Use Managed PostgreSQL** (AWS RDS, Azure Database, Google Cloud SQL)

## Next Steps

1. ✅ PostgreSQL is now configured in your project
2. Run migrations to create database schema
3. Install pgAdmin for database management
4. Configure backup strategy
5. Set up monitoring

Your ERP system is now powered by PostgreSQL! 🐘

