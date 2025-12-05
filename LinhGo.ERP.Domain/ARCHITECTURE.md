# Multi-Tenant ERP System Architecture

## Overview
This ERP system is designed to handle multiple companies (tenants), where each company has its own isolated data including users, customers, orders, inventory, and more.

## Architecture Pattern

### Multi-Tenancy Strategy
The system uses a **Shared Database with Discriminator Column** approach:
- Single database shared across all companies
- Each tenant-specific entity includes a `CompanyId` field
- Data isolation is enforced at the application and repository level
- Better resource utilization and easier maintenance

### Key Components

#### 1. Base Classes

**BaseEntity**
- Base class for all domain entities
- Includes common audit fields (Created, Updated, Deleted)
- Provides soft delete functionality

**TenantEntity**
- Extends BaseEntity
- Implements ITenantEntity interface
- Automatically includes CompanyId for data isolation

#### 2. Domain Structure

```
Domain/
├── Common/
│   ├── BaseEntity.cs
│   ├── TenantEntity.cs
│   ├── ITenantEntity.cs
│   └── Interfaces/
│       ├── IRepository.cs
│       ├── ITenantRepository.cs
│       └── IUnitOfWork.cs
│
├── Companies/
│   ├── Entities/
│   │   ├── Company.cs (Tenant/Master data)
│   │   └── CompanySettings.cs
│   └── Interfaces/
│       └── ICompanyRepository.cs
│
├── Users/
│   ├── Entities/
│   │   ├── User.cs (Shared across companies)
│   │   ├── UserCompany.cs (Junction table)
│   │   └── UserPermission.cs
│   └── Interfaces/
│       └── IUserRepository.cs
│
├── Customers/
│   ├── Entities/
│   │   ├── Customer.cs (Tenant-specific)
│   │   ├── CustomerContact.cs
│   │   └── CustomerAddress.cs
│   └── Interfaces/
│       └── ICustomerRepository.cs
│
├── Inventory/
│   ├── Entities/
│   │   ├── Product.cs (Tenant-specific)
│   │   ├── ProductCategory.cs
│   │   ├── ProductVariant.cs
│   │   ├── Stock.cs
│   │   ├── Warehouse.cs
│   │   └── InventoryTransaction.cs
│   └── Interfaces/
│       ├── IProductRepository.cs
│       └── IStockRepository.cs
│
└── Orders/
    ├── Entities/
    │   ├── Order.cs (Tenant-specific)
    │   ├── OrderItem.cs
    │   ├── OrderPayment.cs
    │   ├── OrderShipment.cs
    │   └── OrderShipmentItem.cs
    └── Interfaces/
        └── IOrderRepository.cs
```

## Entity Relationships

### Company (Tenant)
- **One Company** has many:
  - Users (via UserCompany)
  - Customers
  - Products
  - Orders
  - Warehouses
  - Settings

### User System
- **User** can belong to multiple **Companies** (UserCompany junction table)
- Each **UserCompany** relationship has:
  - Role (Admin, Manager, User, Viewer)
  - Permissions (specific access rights)
  - IsDefaultCompany flag

### Customer Management
- **Customer** belongs to one **Company**
- **Customer** has many:
  - Contacts (additional contact persons)
  - Addresses (billing, shipping, etc.)
  - Orders

### Inventory Management
- **Product** belongs to one **Company**
- **Product** belongs to one **ProductCategory**
- **Product** has many:
  - Variants (size, color, etc.)
  - Stocks (per warehouse)
  - InventoryTransactions (audit trail)

- **Stock** represents inventory at a specific:
  - Product
  - Warehouse
  - Company

### Order Management
- **Order** belongs to one **Company** and one **Customer**
- **Order** has many:
  - OrderItems (line items)
  - OrderPayments (payment records)
  - OrderShipments (shipment records)

- **OrderShipment** has many:
  - OrderShipmentItems (which order items are being shipped)

## Data Isolation Strategies

### 1. Repository Level
```csharp
// ITenantRepository ensures CompanyId is always passed
public interface ITenantRepository<T> where T : TenantEntity
{
    Task<T?> GetByIdAsync(Guid companyId, Guid id, ...);
    Task<IEnumerable<T>> GetAllAsync(Guid companyId, ...);
}
```

### 2. Query Filter Level (EF Core)
```csharp
// Apply global query filters in DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Customer>().HasQueryFilter(c => c.CompanyId == CurrentCompanyId);
    modelBuilder.Entity<Product>().HasQueryFilter(p => p.CompanyId == CurrentCompanyId);
    // etc.
}
```

### 3. Application Level
- User authentication includes company context
- Middleware extracts and validates CompanyId
- All API requests include company context

## Security Considerations

### Access Control
1. **Authentication**: User login validates credentials
2. **Company Selection**: User selects which company to access
3. **Authorization**: Check UserCompany role and permissions
4. **Data Filtering**: All queries filtered by CompanyId

### Permission System
```
Format: "module.action"
Examples:
- inventory.view
- inventory.create
- inventory.edit
- sales.view
- sales.create
- customers.delete
```

## Key Features

### 1. Soft Delete
- All entities have IsDeleted, DeletedAt, DeletedBy
- Data is never physically deleted
- Queries automatically filter deleted records

### 2. Audit Trail
- CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
- Complete history of data changes
- Tracks who made changes and when

### 3. Stock Management
- Multi-warehouse support
- Real-time stock levels
- Quantity on hand, reserved, and available
- Complete transaction history

### 4. Order Fulfillment
- Order status tracking (Draft → Confirmed → Processing → Completed)
- Payment status tracking (Pending → Partial → Paid)
- Fulfillment status (Unfulfilled → Partial → Fulfilled → Shipped → Delivered)
- Multiple shipments per order

### 5. Flexible Customer Management
- Support for Individual, Business, and Government customers
- Multiple contacts per customer
- Multiple addresses (billing, shipping, etc.)
- Credit limit management

## Best Practices

### 1. Always Include CompanyId
```csharp
// Good
var customers = await _customerRepository.GetAllAsync(companyId);

// Bad
var customers = await _dbContext.Customers.ToListAsync();
```

### 2. Use Repository Pattern
```csharp
// Repositories handle data access and tenant filtering
public class CustomerRepository : ITenantRepository<Customer>
{
    // Implementation ensures CompanyId is always applied
}
```

### 3. Use Unit of Work for Transactions
```csharp
await _unitOfWork.BeginTransactionAsync();
try
{
    await _orderRepository.AddAsync(companyId, order);
    await _inventoryService.UpdateStockAsync(companyId, items);
    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

### 4. Denormalize for Performance
- Order stores customer name (snapshot at time of order)
- OrderItem stores product details (historical accuracy)
- Improves query performance and maintains history

## Scalability Considerations

### Database
- Index on CompanyId for all tenant tables
- Index on frequently queried fields (Code, Email, OrderNumber)
- Partition tables by CompanyId for very large datasets

### Caching
- Cache company settings
- Cache user permissions
- Cache product catalog per company

### Performance
- Use pagination for large result sets
- Implement lazy loading for navigation properties
- Use projection (Select) to fetch only needed fields

## Future Enhancements

1. **Purchasing Module**: Purchase orders, suppliers, receiving
2. **Accounting Module**: General ledger, accounts payable/receivable
3. **Manufacturing Module**: Bill of materials, work orders
4. **HR Module**: Employees, payroll, attendance
5. **Reporting Module**: Custom reports, dashboards, analytics
6. **API Integration**: Third-party integrations (payment, shipping, etc.)

## Migration Strategy

### Phase 1: Core Setup
- Company management
- User authentication & authorization
- Basic settings

### Phase 2: Customer & Inventory
- Customer management
- Product catalog
- Warehouse & stock management

### Phase 3: Sales & Orders
- Order management
- Payment processing
- Shipment tracking

### Phase 4: Advanced Features
- Reporting & analytics
- Advanced inventory (serialization, batch tracking)
- Multi-currency support

## Technology Stack Recommendations

### Backend
- **ASP.NET Core 10**: Web API
- **Entity Framework Core**: ORM
- **SQL Server/PostgreSQL**: Database
- **MediatR**: CQRS pattern
- **AutoMapper**: Object mapping
- **FluentValidation**: Input validation

### Authentication
- **ASP.NET Identity**: User management
- **JWT Tokens**: API authentication
- **Refresh Tokens**: Session management

### Frontend
- **Blazor**: UI framework (already in use)
- **MudBlazor/Radzen**: Component library
- **SignalR**: Real-time updates

### Infrastructure
- **Docker**: Containerization
- **Redis**: Caching
- **RabbitMQ/Azure Service Bus**: Message queue
- **Azure/AWS**: Cloud hosting

