# Entity Relationship Diagram

## Multi-Tenant ERP System - Database Schema

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         COMPANY (TENANT MASTER)                          │
└─────────────────────────────────────────────────────────────────────────┘

┌──────────────────┐
│    Company       │ (Master/Tenant Table)
├──────────────────┤
│ Id (PK)          │
│ Name             │
│ Code (Unique)    │
│ TaxId            │
│ Email, Phone     │
│ Address...       │
│ IsActive         │
│ Currency         │
│ SubscriptionPlan │
└──────────────────┘
        │
        ├─────────────────┬────────────────┬─────────────────┬──────────────────┐
        │                 │                │                 │                  │
        ▼                 ▼                ▼                 ▼                  ▼

┌──────────────────────────────────────────────────────────────────────────────┐
│                            USER MANAGEMENT                                    │
└──────────────────────────────────────────────────────────────────────────────┘

┌──────────────────┐       ┌──────────────────┐       ┌──────────────────┐
│      User        │◄──────┤   UserCompany    │──────►│     Company      │
├──────────────────┤  1:N  ├──────────────────┤  N:1  ├──────────────────┤
│ Id (PK)          │       │ Id (PK)          │       │ Id (PK)          │
│ Email (Unique)   │       │ UserId (FK)      │       │ ...              │
│ UserName         │       │ CompanyId (FK)   │       └──────────────────┘
│ PasswordHash     │       │ Role             │
│ FirstName        │       │ IsDefaultCompany │
│ LastName         │       │ IsActive         │
│ PhoneNumber      │       └──────────────────┘
│ IsActive         │               │
│ LastLoginAt      │               │ 1:N
└──────────────────┘               ▼
                           ┌──────────────────┐
                           │  UserPermission  │
                           ├──────────────────┤
                           │ Id (PK)          │
                           │ UserCompanyId(FK)│
                           │ PermissionKey    │
                           │ IsGranted        │
                           └──────────────────┘

┌──────────────────────────────────────────────────────────────────────────────┐
│                         CUSTOMER MANAGEMENT                                   │
└──────────────────────────────────────────────────────────────────────────────┘

┌──────────────────┐       ┌──────────────────┐       ┌──────────────────┐
│    Customer      │◄──────┤ CustomerContact  │       │ CustomerAddress  │
├──────────────────┤  1:N  ├──────────────────┤       ├──────────────────┤
│ Id (PK)          │       │ Id (PK)          │       │ Id (PK)          │
│ CompanyId (FK)   │       │ CustomerId (FK)  │       │ CustomerId (FK)  │
│ Code             │       │ CompanyId (FK)   │       │ CompanyId (FK)   │
│ Name             │       │ Name             │       │ AddressType      │
│ Type             │       │ Position         │       │ AddressLine1...  │
│ Email, Phone     │       │ Email, Phone     │       │ IsDefault        │
│ CreditLimit      │       │ IsPrimary        │       │ IsActive         │
│ CurrentBalance   │       └──────────────────┘       └──────────────────┘
│ Status           │                                           ▲
│ IsActive         │                                           │
└──────────────────┘                                           │ 1:N
        │                                                      │
        │                                                      │
        └──────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────────────┐
│                        INVENTORY MANAGEMENT                                   │
└──────────────────────────────────────────────────────────────────────────────┘

┌──────────────────┐       ┌──────────────────┐       ┌──────────────────┐
│ProductCategory   │◄──────┤     Product      │──────►│  ProductVariant  │
├──────────────────┤  1:N  ├──────────────────┤  1:N  ├──────────────────┤
│ Id (PK)          │       │ Id (PK)          │       │ Id (PK)          │
│ CompanyId (FK)   │       │ CompanyId (FK)   │       │ ProductId (FK)   │
│ ParentCategoryId │       │ CategoryId (FK)  │       │ CompanyId (FK)   │
│ Name, Code       │       │ Code (SKU)       │       │ Code             │
│ Level, Path      │       │ Name             │       │ Name             │
│ IsActive         │       │ Barcode          │       │ Size, Color      │
└──────────────────┘       │ CostPrice        │       │ PriceAdjustment  │
                           │ SellingPrice     │       │ Barcode          │
                           │ ReorderLevel     │       └──────────────────┘
                           │ TrackStock       │
                           │ IsActive         │
                           └──────────────────┘
                                   │
                                   │ 1:N
                                   ▼
┌──────────────────┐       ┌──────────────────┐       ┌──────────────────┐
│   Warehouse      │◄──────┤      Stock       │──────►│     Product      │
├──────────────────┤  1:N  ├──────────────────┤  N:1  ├──────────────────┤
│ Id (PK)          │       │ Id (PK)          │       │ Id (PK)          │
│ CompanyId (FK)   │       │ CompanyId (FK)   │       │ ...              │
│ Code, Name       │       │ ProductId (FK)   │       └──────────────────┘
│ Address...       │       │ WarehouseId (FK) │
│ IsDefault        │       │ VariantId (FK)   │
│ IsActive         │       │ QuantityOnHand   │
└──────────────────┘       │ QuantityReserved │
                           │ AverageCost      │
                           └──────────────────┘
                                   │
                                   │ Related To
                                   ▼
                           ┌──────────────────┐
                           │InventoryTransact │
                           ├──────────────────┤
                           │ Id (PK)          │
                           │ CompanyId (FK)   │
                           │ TransactionNo    │
                           │ ProductId (FK)   │
                           │ FromWarehouseId  │
                           │ ToWarehouseId    │
                           │ Quantity         │
                           │ Type             │
                           │ ReferenceType    │
                           │ ReferenceId      │
                           └──────────────────┘

┌──────────────────────────────────────────────────────────────────────────────┐
│                          ORDER MANAGEMENT                                     │
└──────────────────────────────────────────────────────────────────────────────┘

┌──────────────────┐       ┌──────────────────┐       ┌──────────────────┐
│    Customer      │◄──────┤      Order       │──────►│   OrderItem      │
├──────────────────┤  1:N  ├──────────────────┤  1:N  ├──────────────────┤
│ Id (PK)          │       │ Id (PK)          │       │ Id (PK)          │
│ ...              │       │ CompanyId (FK)   │       │ OrderId (FK)     │
└──────────────────┘       │ CustomerId (FK)  │       │ CompanyId (FK)   │
                           │ OrderNumber      │       │ ProductId (FK)   │
                           │ OrderDate        │       │ VariantId (FK)   │
                           │ CustomerName     │       │ ProductCode      │
                           │ SubTotal         │       │ ProductName      │
                           │ TaxAmount        │       │ Quantity         │
                           │ ShippingCost     │       │ UnitPrice        │
                           │ TotalAmount      │       │ DiscountAmount   │
                           │ PaidAmount       │       │ TaxAmount        │
                           │ Status           │       │ LineTotal        │
                           │ PaymentStatus    │       │ QuantityShipped  │
                           │ FulfillmentStatus│       │ WarehouseId (FK) │
                           └──────────────────┘       └──────────────────┘
                                   │
                    ┌──────────────┼──────────────┐
                    │              │              │
                    ▼              ▼              ▼
         ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐
         │  OrderPayment    │ │  OrderShipment   │ │                  │
         ├──────────────────┤ ├──────────────────┤ │                  │
         │ Id (PK)          │ │ Id (PK)          │ │                  │
         │ OrderId (FK)     │ │ OrderId (FK)     │ │                  │
         │ CompanyId (FK)   │ │ CompanyId (FK)   │ │                  │
         │ PaymentNumber    │ │ ShipmentNumber   │ │                  │
         │ PaymentDate      │ │ ShipmentDate     │ │                  │
         │ Amount           │ │ Carrier          │ │                  │
         │ PaymentMethod    │ │ TrackingNumber   │ │                  │
         │ PaymentReference │ │ Status           │ │                  │
         │ Status           │ └──────────────────┘ │                  │
         └──────────────────┘         │            │                  │
                                      │ 1:N        │                  │
                                      ▼            │                  │
                              ┌──────────────────┐ │                  │
                              │OrderShipmentItem │ │                  │
                              ├──────────────────┤ │                  │
                              │ Id (PK)          │ │                  │
                              │ ShipmentId (FK)  │ │                  │
                              │ OrderItemId (FK) │ │                  │
                              │ CompanyId (FK)   │ │                  │
                              │ Quantity         │ │                  │
                              └──────────────────┘ │                  │
                                                   └──────────────────┘
```

## Key Relationships

### Tenant Isolation
- **CompanyId (FK)** appears in ALL tenant-specific tables
- Ensures complete data isolation between companies
- Global query filters automatically apply CompanyId

### User-Company Relationship
- **Many-to-Many**: Users can belong to multiple companies
- **UserCompany**: Junction table with role and permissions
- **UserPermission**: Granular permissions per user-company relationship

### Customer-Order Relationship
- **One-to-Many**: One customer can have many orders
- Orders denormalize customer info for historical accuracy

### Product-Inventory Relationship
- **Product**: Master product data
- **ProductVariant**: Product variations (size, color, etc.)
- **Stock**: Current inventory levels per warehouse
- **InventoryTransaction**: Complete audit trail of stock movements

### Order-Fulfillment Relationship
- **Order**: Main sales order
- **OrderItem**: Line items with product details (denormalized)
- **OrderPayment**: Payment records (can be multiple per order)
- **OrderShipment**: Shipment records (can be multiple per order)
- **OrderShipmentItem**: Links shipments to specific order items

## Indexes (Recommended)

```sql
-- Tenant filtering
CREATE INDEX IX_Customer_CompanyId ON Customer(CompanyId);
CREATE INDEX IX_Product_CompanyId ON Product(CompanyId);
CREATE INDEX IX_Order_CompanyId ON Order(CompanyId);
-- ... (all tenant tables)

-- Lookups
CREATE UNIQUE INDEX IX_Company_Code ON Company(Code);
CREATE UNIQUE INDEX IX_User_Email ON User(Email);
CREATE INDEX IX_Customer_Code ON Customer(CompanyId, Code);
CREATE INDEX IX_Product_Code ON Product(CompanyId, Code);
CREATE INDEX IX_Order_OrderNumber ON Order(CompanyId, OrderNumber);
CREATE INDEX IX_Order_CustomerId ON Order(CompanyId, CustomerId);

-- Status queries
CREATE INDEX IX_Order_Status ON Order(CompanyId, Status);
CREATE INDEX IX_Customer_Status ON Customer(CompanyId, Status);

-- Date range queries
CREATE INDEX IX_Order_OrderDate ON Order(CompanyId, OrderDate);
CREATE INDEX IX_InventoryTransaction_Date ON InventoryTransaction(CompanyId, TransactionDate);
```

## Foreign Key Constraints

All relationships use Foreign Key constraints to maintain referential integrity:
- CASCADE DELETE for dependent entities (e.g., OrderItem when Order is deleted)
- RESTRICT for referenced entities (e.g., cannot delete Product if it has Orders)
- Consider soft deletes instead of physical deletes for audit purposes

