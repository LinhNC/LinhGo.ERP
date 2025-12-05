# Multi-Tenant ERP System - Visual Structure

## 🏗️ System Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                         CLIENT LAYER                                 │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐  ┌────────────┐   │
│  │   Blazor   │  │   Mobile   │  │  REST API  │  │  External  │   │
│  │     UI     │  │    App     │  │   Clients  │  │    Apps    │   │
│  └────────────┘  └────────────┘  └────────────┘  └────────────┘   │
└────────────────────────────┬────────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────────┐
│                         API LAYER (LinhGo.ERP.Api)                   │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │  Controllers: Auth, Company, Customer, Product, Order, etc.    │ │
│  └────────────────────────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │  Middleware: Authentication, Tenant Resolution, Error Handling │ │
│  └────────────────────────────────────────────────────────────────┘ │
└────────────────────────────┬────────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────────┐
│                   APPLICATION LAYER (LinhGo.ERP.Application)         │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  Services: Business Logic, Workflows, Validations            │  │
│  │  - CustomerService, ProductService, OrderService, etc.        │  │
│  └───────────────────────────────────────────────────────────────┘  │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  DTOs: Data Transfer Objects, ViewModels, Requests/Responses  │  │
│  └───────────────────────────────────────────────────────────────┘  │
└────────────────────────────┬────────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────────┐
│                     DOMAIN LAYER (LinhGo.ERP.Domain)                 │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  Entities: Core business entities with relationships          │  │
│  │  - Company, User, Customer, Product, Order, etc.              │  │
│  └───────────────────────────────────────────────────────────────┘  │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  Interfaces: Repository contracts, Domain services            │  │
│  └───────────────────────────────────────────────────────────────┘  │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  Common: Base classes, enums, value objects                   │  │
│  └───────────────────────────────────────────────────────────────┘  │
└────────────────────────────┬────────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────────┐
│              INFRASTRUCTURE LAYER (LinhGo.ERP.Infrastructure)        │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  DbContext: Entity Framework Core database context           │  │
│  └───────────────────────────────────────────────────────────────┘  │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  Repositories: Data access implementations                    │  │
│  └───────────────────────────────────────────────────────────────┘  │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  Services: External services, caching, file storage           │  │
│  └───────────────────────────────────────────────────────────────┘  │
└────────────────────────────┬────────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────────┐
│                         DATABASE LAYER                               │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  SQL Server / PostgreSQL                                      │  │
│  │  - Tables with CompanyId for tenant isolation                │  │
│  │  - Indexes for performance                                    │  │
│  │  - Foreign keys for referential integrity                     │  │
│  └───────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

## 🔄 Request Flow

```
┌──────────┐
│  Client  │
└────┬─────┘
     │ 1. Request with JWT Token
     ▼
┌─────────────────────┐
│ Authentication      │ 2. Validate token
│ Middleware          │ 3. Extract user & company info
└────┬────────────────┘
     │ 4. Set TenantContext
     ▼
┌─────────────────────┐
│ Authorization       │ 5. Check permissions
│ Middleware          │
└────┬────────────────┘
     │ 6. Allowed
     ▼
┌─────────────────────┐
│ Controller          │ 7. Receive request
└────┬────────────────┘
     │ 8. Call service
     ▼
┌─────────────────────┐
│ Application Service │ 9. Business logic
└────┬────────────────┘
     │ 10. Call repository
     ▼
┌─────────────────────┐
│ Repository          │ 11. Query with CompanyId
└────┬────────────────┘
     │ 12. Execute query
     ▼
┌─────────────────────┐
│ Database            │ 13. Filter by CompanyId
└────┬────────────────┘
     │ 14. Return data
     ▼
┌──────────┐
│  Client  │ 15. JSON response
└──────────┘
```

## 📊 Data Flow Diagram

```
                    ┌─────────────────┐
                    │     Company     │ (Tenant Master)
                    │  - Id           │
                    │  - Name         │
                    │  - Code         │
                    └────────┬────────┘
                             │
        ┌────────────────────┼────────────────────┐
        │                    │                    │
        ▼                    ▼                    ▼
┌───────────────┐    ┌───────────────┐    ┌───────────────┐
│  User-Company │    │   Customer    │    │   Product     │
│   Mapping     │    │   + Orders    │    │   + Stock     │
└───────────────┘    └───────────────┘    └───────────────┘
        │                    │                    │
        ▼                    │                    │
┌───────────────┐            │                    │
│     User      │            │                    │
│  (Shared)     │            │                    │
└───────────────┘            │                    │
                             ▼                    ▼
                      ┌─────────────────────────────┐
                      │          Order              │
                      │  - Customer (FK)            │
                      │  - Items (Products)         │
                      │  - Payments                 │
                      │  - Shipments                │
                      └─────────────────────────────┘
```

## 🗂️ Domain Module Structure

```
LinhGo.ERP.Domain/
│
├── Common/                          # Shared base classes
│   ├── BaseEntity.cs                # Base entity with audit fields
│   ├── TenantEntity.cs              # Base for tenant-specific entities
│   ├── ITenantEntity.cs             # Tenant interface
│   ├── ITenantContext.cs            # Tenant context service
│   └── Interfaces/
│       ├── IRepository.cs           # Generic repository
│       ├── ITenantRepository.cs     # Tenant-aware repository
│       └── IUnitOfWork.cs           # Transaction management
│
├── Companies/                       # Company/Tenant module
│   ├── Entities/
│   │   ├── Company.cs               # Tenant master
│   │   └── CompanySettings.cs       # Per-company settings
│   └── Interfaces/
│       └── ICompanyRepository.cs
│
├── Users/                           # User & Permission module
│   ├── Entities/
│   │   ├── User.cs                  # User entity
│   │   ├── UserCompany.cs           # User-Company mapping
│   │   └── UserPermission.cs        # Granular permissions
│   └── Interfaces/
│       └── IUserRepository.cs
│
├── Customers/                       # Customer Management
│   ├── Entities/
│   │   ├── Customer.cs              # Customer master
│   │   ├── CustomerContact.cs       # Contact persons
│   │   └── CustomerAddress.cs       # Multiple addresses
│   └── Interfaces/
│       └── ICustomerRepository.cs
│
├── Inventory/                       # Inventory Management
│   ├── Entities/
│   │   ├── Product.cs               # Product master
│   │   ├── ProductCategory.cs       # Categories
│   │   ├── ProductVariant.cs        # Size/color variants
│   │   ├── Stock.cs                 # Stock levels
│   │   ├── Warehouse.cs             # Warehouses
│   │   └── InventoryTransaction.cs  # Stock movements
│   └── Interfaces/
│       ├── IProductRepository.cs
│       └── IStockRepository.cs
│
└── Orders/                          # Order Management
    ├── Entities/
    │   ├── Order.cs                 # Sales order
    │   ├── OrderItem.cs             # Line items
    │   ├── OrderPayment.cs          # Payments
    │   ├── OrderShipment.cs         # Shipments
    │   └── OrderShipmentItem.cs     # Shipment details
    └── Interfaces/
        └── IOrderRepository.cs
```

## 🎯 Multi-Tenancy Pattern

```
┌─────────────────────────────────────────────────────────────┐
│                     Single Database                          │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐ │
│  │              Companies Table                           │ │
│  │  Id=1 | Name="Company A" | Code="COMP-A"              │ │
│  │  Id=2 | Name="Company B" | Code="COMP-B"              │ │
│  │  Id=3 | Name="Company C" | Code="COMP-C"              │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐ │
│  │              Customers Table (Tenant-Specific)         │ │
│  │  Id | CompanyId | Code  | Name                        │ │
│  │  1  |     1     | C001  | Customer A1                  │ │
│  │  2  |     1     | C002  | Customer A2                  │ │
│  │  3  |     2     | C001  | Customer B1  ← Same code,   │ │
│  │  4  |     2     | C002  | Customer B2     different   │ │
│  │  5  |     3     | C001  | Customer C1     company     │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐ │
│  │              Products Table (Tenant-Specific)          │ │
│  │  Id | CompanyId | Code  | Name     | Stock            │ │
│  │  1  |     1     | P001  | Product1 | 100              │ │
│  │  2  |     1     | P002  | Product2 | 50               │ │
│  │  3  |     2     | P001  | Product1 | 200              │ │
│  │  4  |     3     | P001  | Product1 | 75               │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐ │
│  │              Orders Table (Tenant-Specific)            │ │
│  │  Id | CompanyId | CustomerId | Total | Status          │ │
│  │  1  |     1     |     1      | 1000  | Completed       │ │
│  │  2  |     1     |     2      | 2000  | Processing      │ │
│  │  3  |     2     |     3      | 1500  | Completed       │ │
│  │  4  |     3     |     5      | 3000  | Shipped         │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│  Every query automatically filters by CompanyId:            │
│  SELECT * FROM Customers WHERE CompanyId = @CurrentCompanyId│
└─────────────────────────────────────────────────────────────┘
```

## 🔐 Security & Access Control

```
User Login
    │
    ▼
┌─────────────────┐
│ Authenticate    │
│ (Email/Password)│
└────────┬────────┘
         │
         ▼
┌─────────────────────────────────┐
│ Get User's Company Memberships  │
│                                 │
│ Company A - Role: Admin         │
│ Company B - Role: User          │
│ Company C - Role: Manager       │
└────────┬────────────────────────┘
         │
         ▼
┌─────────────────┐
│ Select Company  │ ← User chooses which company to work with
└────────┬────────┘
         │
         ▼
┌──────────────────────────────┐
│ Generate JWT Token           │
│ Claims:                      │
│  - UserId                    │
│  - CompanyId (selected)      │
│  - Role (for this company)   │
│  - Permissions               │
└────────┬─────────────────────┘
         │
         ▼
┌─────────────────────────────────┐
│ All API Requests Include:       │
│ Authorization: Bearer {token}   │
│                                 │
│ Middleware extracts CompanyId   │
│ Sets TenantContext              │
│ Validates permissions           │
└─────────────────────────────────┘
```

## 📦 Module Dependencies

```
┌────────────┐
│   Web/API  │  ← Presentation Layer
└──────┬─────┘
       │ depends on
       ▼
┌────────────┐
│Application │  ← Business Logic
└──────┬─────┘
       │ depends on
       ▼
┌────────────┐
│   Domain   │  ← Core Entities & Interfaces
└──────┬─────┘
       ▲
       │ implemented by
       │
┌──────┴──────┐
│Infrastructure│  ← Data Access, External Services
└─────────────┘
```

## 🚀 Deployment Architecture

```
┌──────────────────────────────────────────────────────────┐
│                     Load Balancer                         │
└────────┬──────────────────────────────────────┬──────────┘
         │                                      │
    ┌────▼────┐                            ┌────▼────┐
    │ Web API │                            │ Web API │
    │Instance1│                            │Instance2│
    └────┬────┘                            └────┬────┘
         │                                      │
         └──────────┬───────────────────────────┘
                    │
    ┌───────────────▼──────────────┐
    │    Redis Cache (Optional)    │
    └───────────────┬──────────────┘
                    │
    ┌───────────────▼──────────────┐
    │      SQL Server Database     │
    │  - All tenant data           │
    │  - Indexed by CompanyId      │
    └──────────────────────────────┘
```

---

## 📋 Quick Reference

### Key Concepts
- **Tenant** = Company
- **Multi-Tenancy** = Multiple companies sharing same system
- **Tenant Isolation** = Each company's data is separate
- **CompanyId** = Foreign key in all tenant-specific tables

### Entity Types
- **Shared Entities**: User (can access multiple companies)
- **Tenant Entities**: Customer, Product, Order (belong to one company)
- **Master Entity**: Company (the tenant itself)

### Repository Patterns
- `IRepository<T>` - For shared entities (User, Company)
- `ITenantRepository<T>` - For tenant-specific entities (Customer, Product)

### Authentication Flow
1. User logs in
2. Gets list of companies they can access
3. Selects a company
4. Receives JWT with CompanyId
5. All requests filtered by CompanyId

This design ensures **complete data isolation** while maintaining **efficient resource usage**!

