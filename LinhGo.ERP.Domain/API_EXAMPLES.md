# API Examples - Multi-Tenant ERP System

## Authentication & Company Context

### 1. Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!"
}

Response:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "...",
  "user": {
    "id": "guid",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe"
  },
  "companies": [
    {
      "companyId": "company-guid-1",
      "companyName": "Company A",
      "role": "Admin",
      "isDefault": true
    },
    {
      "companyId": "company-guid-2",
      "companyName": "Company B",
      "role": "User",
      "isDefault": false
    }
  ]
}
```

### 2. Select Company Context
```http
POST /api/auth/select-company
Authorization: Bearer {token}
Content-Type: application/json

{
  "companyId": "company-guid-1"
}

Response:
{
  "token": "new-token-with-company-context...",
  "company": {
    "id": "company-guid-1",
    "name": "Company A",
    "code": "COMP-A",
    "currency": "USD"
  }
}
```

## Company Management (Admin Only)

### 3. Create Company
```http
POST /api/companies
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "name": "New Company Ltd",
  "code": "NEWCO",
  "taxId": "123456789",
  "email": "info@newcompany.com",
  "phone": "+1234567890",
  "addressLine1": "123 Main St",
  "city": "New York",
  "state": "NY",
  "postalCode": "10001",
  "country": "USA",
  "currency": "USD",
  "subscriptionPlan": "Premium"
}

Response: 201 Created
{
  "id": "new-company-guid",
  "name": "New Company Ltd",
  "code": "NEWCO",
  "isActive": true,
  "createdAt": "2025-12-05T10:00:00Z"
}
```

### 4. Get All Companies
```http
GET /api/companies
Authorization: Bearer {admin-token}

Response: 200 OK
[
  {
    "id": "guid-1",
    "name": "Company A",
    "code": "COMP-A",
    "isActive": true,
    "subscriptionPlan": "Premium"
  },
  ...
]
```

## Customer Management

### 5. Create Customer
```http
POST /api/customers
Authorization: Bearer {token}
X-Company-Id: {company-guid}
Content-Type: application/json

{
  "code": "CUST-001",
  "name": "ABC Corporation",
  "type": "Business",
  "email": "contact@abc.com",
  "phone": "+1234567890",
  "addressLine1": "456 Business Ave",
  "city": "Los Angeles",
  "state": "CA",
  "postalCode": "90001",
  "country": "USA",
  "creditLimit": 50000,
  "paymentTermDays": 30
}

Response: 201 Created
{
  "id": "customer-guid",
  "companyId": "company-guid",
  "code": "CUST-001",
  "name": "ABC Corporation",
  "type": "Business",
  "currentBalance": 0,
  "creditLimit": 50000,
  "isActive": true
}
```

### 6. Get All Customers
```http
GET /api/customers?page=1&pageSize=20
Authorization: Bearer {token}
X-Company-Id: {company-guid}

Response: 200 OK
{
  "items": [
    {
      "id": "customer-guid",
      "code": "CUST-001",
      "name": "ABC Corporation",
      "email": "contact@abc.com",
      "currentBalance": 1500.00,
      "isActive": true
    },
    ...
  ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20
}
```

### 7. Search Customers
```http
GET /api/customers/search?q=ABC
Authorization: Bearer {token}
X-Company-Id: {company-guid}

Response: 200 OK
[
  {
    "id": "customer-guid",
    "code": "CUST-001",
    "name": "ABC Corporation",
    "email": "contact@abc.com"
  }
]
```

## Product/Inventory Management

### 8. Create Product
```http
POST /api/products
Authorization: Bearer {token}
X-Company-Id: {company-guid}
Content-Type: application/json

{
  "code": "PROD-001",
  "name": "Laptop Computer",
  "description": "High-performance laptop",
  "categoryId": "category-guid",
  "barcode": "1234567890123",
  "costPrice": 800.00,
  "sellingPrice": 1200.00,
  "unit": "pcs",
  "reorderLevel": 10,
  "reorderQuantity": 50,
  "trackStock": true,
  "taxRate": 0.10
}

Response: 201 Created
{
  "id": "product-guid",
  "companyId": "company-guid",
  "code": "PROD-001",
  "name": "Laptop Computer",
  "sellingPrice": 1200.00,
  "isActive": true
}
```

### 9. Get Product with Stock Levels
```http
GET /api/products/{product-id}?includeStock=true
Authorization: Bearer {token}
X-Company-Id: {company-guid}

Response: 200 OK
{
  "id": "product-guid",
  "code": "PROD-001",
  "name": "Laptop Computer",
  "sellingPrice": 1200.00,
  "stocks": [
    {
      "warehouseId": "warehouse-guid-1",
      "warehouseName": "Main Warehouse",
      "quantityOnHand": 50,
      "quantityReserved": 5,
      "quantityAvailable": 45
    },
    {
      "warehouseId": "warehouse-guid-2",
      "warehouseName": "Secondary Warehouse",
      "quantityOnHand": 30,
      "quantityReserved": 0,
      "quantityAvailable": 30
    }
  ]
}
```

### 10. Update Stock Level
```http
POST /api/inventory/stock/adjust
Authorization: Bearer {token}
X-Company-Id: {company-guid}
Content-Type: application/json

{
  "productId": "product-guid",
  "warehouseId": "warehouse-guid",
  "quantity": 100,
  "type": "StockIn",
  "reason": "Purchase order received",
  "unitCost": 800.00,
  "referenceType": "PurchaseOrder",
  "referenceNumber": "PO-001"
}

Response: 200 OK
{
  "transactionId": "transaction-guid",
  "newQuantityOnHand": 150,
  "transactionDate": "2025-12-05T10:00:00Z"
}
```

### 11. Get Low Stock Products
```http
GET /api/inventory/low-stock
Authorization: Bearer {token}
X-Company-Id: {company-guid}

Response: 200 OK
[
  {
    "productId": "product-guid",
    "productCode": "PROD-001",
    "productName": "Laptop Computer",
    "currentStock": 8,
    "reorderLevel": 10,
    "reorderQuantity": 50
  },
  ...
]
```

## Order Management

### 12. Create Order
```http
POST /api/orders
Authorization: Bearer {token}
X-Company-Id: {company-guid}
Content-Type: application/json

{
  "customerId": "customer-guid",
  "orderDate": "2025-12-05",
  "requiredDate": "2025-12-10",
  "paymentMethod": "Bank Transfer",
  "paymentTerms": "Net 30",
  "shippingMethod": "Standard",
  "items": [
    {
      "productId": "product-guid-1",
      "quantity": 2,
      "unitPrice": 1200.00,
      "discountPercentage": 5,
      "warehouseId": "warehouse-guid"
    },
    {
      "productId": "product-guid-2",
      "quantity": 5,
      "unitPrice": 50.00,
      "discountPercentage": 0,
      "warehouseId": "warehouse-guid"
    }
  ],
  "shippingCost": 25.00,
  "notes": "Customer requested expedited processing"
}

Response: 201 Created
{
  "id": "order-guid",
  "orderNumber": "ORD-2025-0001",
  "companyId": "company-guid",
  "customerId": "customer-guid",
  "customerName": "ABC Corporation",
  "orderDate": "2025-12-05",
  "status": "Draft",
  "paymentStatus": "Pending",
  "fulfillmentStatus": "Unfulfilled",
  "subTotal": 2530.00,
  "discountAmount": 120.00,
  "taxAmount": 241.00,
  "shippingCost": 25.00,
  "totalAmount": 2676.00
}
```

### 13. Confirm Order
```http
POST /api/orders/{order-id}/confirm
Authorization: Bearer {token}
X-Company-Id: {company-guid}

Response: 200 OK
{
  "id": "order-guid",
  "orderNumber": "ORD-2025-0001",
  "status": "Confirmed",
  "confirmedAt": "2025-12-05T10:15:00Z"
}
```

### 14. Record Payment
```http
POST /api/orders/{order-id}/payments
Authorization: Bearer {token}
X-Company-Id: {company-guid}
Content-Type: application/json

{
  "amount": 1500.00,
  "paymentMethod": "Bank Transfer",
  "paymentReference": "TXN-123456",
  "notes": "Partial payment received"
}

Response: 201 Created
{
  "id": "payment-guid",
  "orderId": "order-guid",
  "paymentNumber": "PAY-2025-0001",
  "amount": 1500.00,
  "paymentDate": "2025-12-05T10:30:00Z",
  "orderTotalAmount": 2676.00,
  "orderPaidAmount": 1500.00,
  "orderBalanceAmount": 1176.00,
  "paymentStatus": "Partial"
}
```

### 15. Create Shipment
```http
POST /api/orders/{order-id}/shipments
Authorization: Bearer {token}
X-Company-Id: {company-guid}
Content-Type: application/json

{
  "shipmentDate": "2025-12-06",
  "carrier": "FedEx",
  "trackingNumber": "FEDEX-123456789",
  "shippingMethod": "Express",
  "items": [
    {
      "orderItemId": "order-item-guid-1",
      "quantity": 2
    },
    {
      "orderItemId": "order-item-guid-2",
      "quantity": 5
    }
  ]
}

Response: 201 Created
{
  "id": "shipment-guid",
  "shipmentNumber": "SHIP-2025-0001",
  "orderId": "order-guid",
  "shipmentDate": "2025-12-06",
  "carrier": "FedEx",
  "trackingNumber": "FEDEX-123456789",
  "status": "Shipped"
}
```

### 16. Get Order with Full Details
```http
GET /api/orders/{order-id}?include=items,payments,shipments
Authorization: Bearer {token}
X-Company-Id: {company-guid}

Response: 200 OK
{
  "id": "order-guid",
  "orderNumber": "ORD-2025-0001",
  "orderDate": "2025-12-05",
  "customer": {
    "id": "customer-guid",
    "code": "CUST-001",
    "name": "ABC Corporation"
  },
  "status": "Processing",
  "paymentStatus": "Partial",
  "fulfillmentStatus": "Partial",
  "totalAmount": 2676.00,
  "paidAmount": 1500.00,
  "balanceAmount": 1176.00,
  "items": [
    {
      "id": "item-guid-1",
      "productCode": "PROD-001",
      "productName": "Laptop Computer",
      "quantity": 2,
      "unitPrice": 1200.00,
      "discountAmount": 120.00,
      "lineTotal": 2280.00,
      "quantityShipped": 2,
      "fulfillmentStatus": "Fulfilled"
    }
  ],
  "payments": [
    {
      "id": "payment-guid",
      "paymentNumber": "PAY-2025-0001",
      "amount": 1500.00,
      "paymentDate": "2025-12-05",
      "paymentMethod": "Bank Transfer"
    }
  ],
  "shipments": [
    {
      "id": "shipment-guid",
      "shipmentNumber": "SHIP-2025-0001",
      "shipmentDate": "2025-12-06",
      "carrier": "FedEx",
      "trackingNumber": "FEDEX-123456789",
      "status": "Shipped"
    }
  ]
}
```

### 17. Get Orders by Customer
```http
GET /api/customers/{customer-id}/orders
Authorization: Bearer {token}
X-Company-Id: {company-guid}

Response: 200 OK
[
  {
    "id": "order-guid",
    "orderNumber": "ORD-2025-0001",
    "orderDate": "2025-12-05",
    "totalAmount": 2676.00,
    "status": "Processing"
  },
  ...
]
```

## Reports & Analytics

### 18. Sales Summary
```http
GET /api/reports/sales-summary?startDate=2025-12-01&endDate=2025-12-31
Authorization: Bearer {token}
X-Company-Id: {company-guid}

Response: 200 OK
{
  "period": {
    "startDate": "2025-12-01",
    "endDate": "2025-12-31"
  },
  "totalOrders": 150,
  "totalRevenue": 250000.00,
  "totalPaidAmount": 200000.00,
  "outstandingAmount": 50000.00,
  "averageOrderValue": 1666.67,
  "topProducts": [
    {
      "productId": "product-guid",
      "productName": "Laptop Computer",
      "quantitySold": 45,
      "revenue": 54000.00
    }
  ],
  "topCustomers": [
    {
      "customerId": "customer-guid",
      "customerName": "ABC Corporation",
      "orderCount": 8,
      "totalSpent": 25000.00
    }
  ]
}
```

### 19. Inventory Valuation
```http
GET /api/reports/inventory-valuation
Authorization: Bearer {token}
X-Company-Id: {company-guid}

Response: 200 OK
{
  "reportDate": "2025-12-05",
  "totalItems": 250,
  "totalQuantity": 5000,
  "totalValue": 400000.00,
  "byCategory": [
    {
      "categoryName": "Electronics",
      "itemCount": 50,
      "quantity": 1000,
      "value": 200000.00
    }
  ],
  "lowStockItems": 8
}
```

## Middleware Implementation

```csharp
// File: LinhGo.ERP.Api/Middleware/TenantMiddleware.cs

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        // Extract company ID from header, claim, or query string
        if (context.Request.Headers.TryGetValue("X-Company-Id", out var companyIdHeader))
        {
            if (Guid.TryParse(companyIdHeader, out var companyId))
            {
                tenantContext.SetCompanyId(companyId);
            }
        }
        else if (context.User.Identity?.IsAuthenticated == true)
        {
            var companyIdClaim = context.User.FindFirst("CompanyId")?.Value;
            if (!string.IsNullOrEmpty(companyIdClaim) && Guid.TryParse(companyIdClaim, out var companyId))
            {
                tenantContext.SetCompanyId(companyId);
            }
        }

        await _next(context);
    }
}
```

## Error Responses

```http
400 Bad Request
{
  "type": "ValidationError",
  "title": "Validation Failed",
  "errors": {
    "code": ["Customer code is required"],
    "email": ["Invalid email format"]
  }
}

401 Unauthorized
{
  "type": "AuthenticationError",
  "title": "Authentication Required",
  "detail": "You must be logged in to access this resource"
}

403 Forbidden
{
  "type": "AuthorizationError",
  "title": "Access Denied",
  "detail": "You don't have permission to perform this action"
}

404 Not Found
{
  "type": "NotFoundError",
  "title": "Resource Not Found",
  "detail": "The requested customer was not found"
}

409 Conflict
{
  "type": "ConflictError",
  "title": "Duplicate Resource",
  "detail": "A customer with code 'CUST-001' already exists"
}
```

