# Authentication & Authorization Implementation Guide

## Architecture Overview

```
┌─────────────────────────┐
│   LinhGo.ERP.Web        │
│   (Blazor Server)       │
│                         │
│  1. User Login          │
│  2. Receives JWT        │
│  3. Stores in Cookie    │
│     (HttpOnly)          │
└────────┬────────────────┘
         │ JWT Token
         ↓
┌─────────────────────────┐
│   LinhGo.ERP.Api        │
│                         │
│  1. Validates JWT       │
│  2. Checks Permissions  │
│  3. Returns Data        │
└─────────────────────────┘
```

## Implementation Steps

### Phase 1: API Authentication Setup
1. JWT Token generation in AccountController
2. JWT Bearer authentication middleware
3. Permission-based authorization policies

### Phase 2: Web Authentication Setup
1. Cookie authentication after login
2. HTTP client with JWT forwarding
3. Authorization components

### Phase 3: Permission System
1. Role-based access control (RBAC)
2. Resource-based permissions
3. Policy-based authorization

## Security Best Practices Applied

✅ HttpOnly cookies (prevent XSS)
✅ Secure flag for HTTPS
✅ SameSite cookie policy
✅ JWT with short expiration (15 min)
✅ Refresh token rotation
✅ CORS properly configured
✅ Authorization policies
✅ Claims-based identity

