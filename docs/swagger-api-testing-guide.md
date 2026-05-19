# AutoFlow Swagger API Testing Guide

## 1. Environment Setup

Run the backend and seed test data:

```bash
cd /Users/krishna09/RiderProjects/root-auto/Backend/AutoFlow-Backend

dotnet build AutoFlow-Backend.slnx --no-restore -v minimal /nr:false /m:1

dotnet ef database update --project AutoFlow-Backend.Infrastructure --startup-project AutoFlow-Backend

ASPNETCORE_ENVIRONMENT=Development dotnet run --project AutoFlow-Backend -- --reset-and-seed-dev

dotnet run --project AutoFlow-Backend --urls http://localhost:5294
```

Swagger URL:

```text
http://localhost:5294/swagger/index.html
```

If port is already in use:

```bash
lsof -i :5294
kill -9 <PID>
```

## 2. Swagger Authorization Steps

1. Open Swagger UI.
2. Call `POST /api/auth/login` with role credentials.
3. Copy the returned JWT `token`.
4. Click `Authorize` (lock icon).
5. Paste `Bearer <token>`.
6. Click `Authorize` and close popup.
7. Run protected endpoints.

Notes:
- `401 Unauthorized`: token missing/expired/invalid.
- `403 Forbidden`: authenticated user does not have required role.

## 3. Seeded Test Accounts

Admin:
- `admin@autoflow.local / Admin@12345`

Staff:
- `staff@autoflow.local / Admin@12345`

Customer:
- `customer@autoflow.local / Admin@12345`

Generated Staff:
- `staff001@autoflow.local / Admin@12345`

Generated Customer:
- `customer001@autoflow.local / Admin@12345`

## 4. Admin API Testing

### 4.1 Admin Login
Endpoint and Method: `POST /api/auth/login`  
Required Role/Token: Public (no token)

Request JSON:

```json
{
  "email": "admin@autoflow.local",
  "password": "Admin@12345"
}
```

| Field | Value |
|---|---|
| Test No | 1 |
| Test Name | Admin Login |
| Test Type | API / Integration Test |
| Action | Send login request through Swagger using admin credentials. |
| Expected Output | API returns success response with JWT token, userId, email, fullName, and Admin role. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
If successful, copy JWT token and authorize Swagger with `Bearer <token>`.

### 4.2 Admin Change Password (Global Auth Endpoint)
Endpoint and Method: `POST /api/auth/change-password`  
Required Role/Token: Any authenticated user (Admin token)

Request JSON:

```json
{
  "currentPassword": "Admin@12345",
  "newPassword": "Admin@123456"
}
```

| Field | Value |
|---|---|
| Test No | 2 |
| Test Name | Admin Change Password (Auth) |
| Test Type | API / Integration Test |
| Action | Send change-password request with current and new password. |
| Expected Output | API returns success and password is updated. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
This updates seeded credentials. Revert if needed before other tests.

### 4.3 Admin Profile - Get
Endpoint and Method: `GET /api/admin/profile`  
Required Role/Token: Admin token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 3 |
| Test Name | Get Admin Profile |
| Test Type | API / Integration Test |
| Action | Fetch current admin profile using admin token. |
| Expected Output | API returns admin profile details for authenticated admin. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Expect HTTP 200 with admin profile object.

### 4.4 Admin Profile - Update
Endpoint and Method: `PUT /api/admin/profile`  
Required Role/Token: Admin token

Request JSON:

```json
{
  "fullName": "Admin Swagger Updated",
  "phone": "0499000001",
  "address": "Sydney Admin Office"
}
```

| Field | Value |
|---|---|
| Test No | 4 |
| Test Name | Update Admin Profile |
| Test Type | API / Integration Test |
| Action | Update admin profile fields through admin profile endpoint. |
| Expected Output | API returns updated admin profile data. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Confirm changes by calling `GET /api/admin/profile` again.

### 4.5 Dashboard Summary
Endpoint and Method: `GET /api/dashboard`  
Required Role/Token: Admin token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 5 |
| Test Name | View Dashboard Summary |
| Test Type | API / Integration Test |
| Action | Fetch admin dashboard summary. |
| Expected Output | Response includes totals and KPI fields like revenue, counts, lowStockParts, etc. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Use this to validate seeded data totals and dashboard API health.

### 4.6 Dashboard Activity Stream
Endpoint and Method: `GET /api/dashboard/activity-stream?limit=10`  
Required Role/Token: Admin token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 6 |
| Test Name | Dashboard Activity Stream |
| Test Type | API / Integration Test |
| Action | Fetch recent dashboard activity with limit query. |
| Expected Output | API returns list of activity events. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
If empty, seed/events may not yet generate activity records.

### 4.7 Dashboard Revenue Trend
Endpoint and Method: `GET /api/dashboard/revenue-trend?range=daily`  
Required Role/Token: Admin token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 7 |
| Test Name | Dashboard Revenue Trend |
| Test Type | API / Integration Test |
| Action | Fetch revenue trend points for daily range. |
| Expected Output | API returns trend points with label/date/revenue and optional salesCount. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Repeat with `range=weekly` and `range=monthly`.

### 4.8 Dashboard Fast Moving Inventory
Endpoint and Method: `GET /api/dashboard/fast-moving-inventory?limit=5`  
Required Role/Token: Admin token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 8 |
| Test Name | Dashboard Fast Moving Inventory |
| Test Type | API / Integration Test |
| Action | Fetch top moving inventory rows. |
| Expected Output | API returns list sorted by sold quantity/revenue logic. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Used by admin dashboard inventory insight widget.

### 4.9 Dashboard Priority Alerts
Endpoint and Method: `GET /api/dashboard/priority-alerts?limit=10`  
Required Role/Token: Admin token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 9 |
| Test Name | Dashboard Priority Alerts |
| Test Type | API / Integration Test |
| Action | Fetch dashboard priority alerts. |
| Expected Output | API returns alert items with severity and description. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
If empty, alert conditions may not be triggered in current data.

### 4.10 Staff List
Endpoint and Method: `GET /api/staff?page=1&pageSize=20`  
Required Role/Token: Admin token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 10 |
| Test Name | View Staff List |
| Test Type | API / Integration Test |
| Action | Fetch paged staff list. |
| Expected Output | API returns paged `StaffResponse` items. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Copy one staff `id` for by-id/update/deactivate tests.

### 4.11 Staff By ID
Endpoint and Method: `GET /api/staff/{id}`  
Required Role/Token: Admin token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 11 |
| Test Name | View Staff By ID |
| Test Type | API / Integration Test |
| Action | Fetch one staff record by ID from previous list test. |
| Expected Output | API returns matching staff record or 404 if invalid ID. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Use a valid GUID from `GET /api/staff`.

### 4.12 Create Staff
Endpoint and Method: `POST /api/staff`  
Required Role/Token: Admin token

Request JSON:

```json
{
  "staffCode": "STF-SWG-001",
  "fullName": "Swagger Staff User",
  "email": "swagger.staff001@autoflow.local",
  "password": "Admin@12345",
  "phone": "0499000100",
  "address": "Workshop Zone 1, Sydney",
  "role": "Staff"
}
```

| Field | Value |
|---|---|
| Test No | 12 |
| Test Name | Create Staff |
| Test Type | API / Integration Test |
| Action | Create a new staff account using CreateStaffRequest fields. |
| Expected Output | API returns created staff record. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Validate uniqueness of email before retrying failed creates.

### 4.13 Update Staff
Endpoint and Method: `PUT /api/staff/{id}`  
Required Role/Token: Admin token

Request JSON:

```json
{
  "fullName": "Swagger Staff User Updated",
  "email": "swagger.staff001@autoflow.local",
  "phone": "0499000101",
  "address": "Workshop Zone 2, Sydney",
  "position": "Technician"
}
```

| Field | Value |
|---|---|
| Test No | 13 |
| Test Name | Update Staff |
| Test Type | API / Integration Test |
| Action | Update existing staff details. |
| Expected Output | API returns updated staff record. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Confirm updates with `GET /api/staff/{id}`.

### 4.14 Deactivate Staff
Endpoint and Method: `DELETE /api/staff/{id}`  
Required Role/Token: Admin token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 14 |
| Test Name | Deactivate Staff |
| Test Type | API / Integration Test |
| Action | Deactivate selected staff record (soft delete). |
| Expected Output | API returns success with deactivation confirmation. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Deactivated staff should not authenticate for staff-only flows.

### 4.15 Vendor List
Endpoint and Method: `GET /api/vendors?page=1&pageSize=20`  
Required Role/Token: Admin token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 15 |
| Test Name | View Vendor List |
| Test Type | API / Integration Test |
| Action | Fetch paged vendor list. |
| Expected Output | API returns paged vendors. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Copy vendor `id` for create-part and purchase-invoice tests.

### 4.16 Vendor By ID
Endpoint and Method: `GET /api/vendors/{id}`  
Required Role/Token: Admin token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 16 |
| Test Name | View Vendor By ID |
| Test Type | API / Integration Test |
| Action | Fetch vendor by valid ID. |
| Expected Output | API returns vendor details or 404 for unknown ID. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Use `id` from vendor list endpoint.

### 4.17 Create Vendor
Endpoint and Method: `POST /api/vendors`  
Required Role/Token: Admin token

Request JSON:

```json
{
  "vendorName": "Swagger Auto Parts",
  "contactPerson": "Alex Vendor",
  "phone": "0499000200",
  "email": "vendor.swagger@autoflow.local",
  "address": "10 Parts Street, Sydney"
}
```

| Field | Value |
|---|---|
| Test No | 17 |
| Test Name | Create Vendor |
| Test Type | API / Integration Test |
| Action | Create new vendor via CreateVendorRequest DTO. |
| Expected Output | API returns created vendor data. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
If success, reuse created vendor in parts/purchase invoice tests.

### 4.18 Update Vendor
Endpoint and Method: `PUT /api/vendors/{id}`  
Required Role/Token: Admin token

Request JSON:

```json
{
  "vendorName": "Swagger Auto Parts Updated",
  "contactPerson": "Alex Vendor",
  "phone": "0499000201",
  "email": "vendor.swagger@autoflow.local",
  "address": "11 Parts Street, Sydney"
}
```

| Field | Value |
|---|---|
| Test No | 18 |
| Test Name | Update Vendor |
| Test Type | API / Integration Test |
| Action | Update vendor fields for a valid vendor ID. |
| Expected Output | API returns updated vendor details. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Confirm change with `GET /api/vendors/{id}`.

### 4.19 Vendor Search
Endpoint and Method: `GET /api/vendors/search?query=Swagger`  
Required Role/Token: Admin token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 19 |
| Test Name | Search Vendors |
| Test Type | API / Integration Test |
| Action | Search vendors by name/contact string. |
| Expected Output | API returns matching vendor list. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Useful for UI search verification and vendor lookup workflows.

### 4.20 Parts List
Endpoint and Method: `GET /api/parts?page=1&pageSize=20`  
Required Role/Token: Admin or Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 20 |
| Test Name | View Parts List |
| Test Type | API / Integration Test |
| Action | Fetch parts inventory list. |
| Expected Output | API returns paged parts list. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Copy part `id` for sale and purchase invoice item tests.

### 4.21 Low Stock Parts
Endpoint and Method: `GET /api/parts/low-stock`  
Required Role/Token: Admin or Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 21 |
| Test Name | View Low Stock Parts |
| Test Type | API / Integration Test |
| Action | Fetch parts below minimum stock threshold. |
| Expected Output | API returns low-stock part list. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Useful for alerts and reorder workflows.

### 4.22 Create Part
Endpoint and Method: `POST /api/parts`  
Required Role/Token: Admin token

Request JSON:

```json
{
  "partName": "Swagger Brake Pad",
  "partNumber": "SWG-BRK-001",
  "brand": "Brembo",
  "category": "Brake",
  "description": "Test brake pad from Swagger",
  "unitPrice": 80.0,
  "sellingPrice": 120.0,
  "stockQuantity": 25,
  "minimumStockLevel": 5,
  "vendorId": "PASTE_VENDOR_ID_HERE"
}
```

| Field | Value |
|---|---|
| Test No | 22 |
| Test Name | Create Part |
| Test Type | API / Integration Test |
| Action | Create inventory part using valid VendorId. |
| Expected Output | API returns created part record. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Use `GET /api/vendors` first to copy a valid `vendorId`.

### 4.23 Update Part
Endpoint and Method: `PUT /api/parts/{id}`  
Required Role/Token: Admin token

Request JSON:

```json
{
  "partName": "Swagger Brake Pad Updated",
  "partNumber": "SWG-BRK-001",
  "brand": "Brembo",
  "category": "Brake",
  "description": "Updated test brake pad",
  "unitPrice": 82.0,
  "sellingPrice": 125.0,
  "stockQuantity": 30,
  "minimumStockLevel": 5,
  "vendorId": "PASTE_VENDOR_ID_HERE"
}
```

| Field | Value |
|---|---|
| Test No | 23 |
| Test Name | Update Part |
| Test Type | API / Integration Test |
| Action | Update part by ID with new stock/pricing data. |
| Expected Output | API returns updated part record. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Verify response and re-fetch part by ID.

### 4.24 Customers List
Endpoint and Method: `GET /api/customers?page=1&pageSize=20`  
Required Role/Token: Admin or Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 24 |
| Test Name | View Customers List |
| Test Type | API / Integration Test |
| Action | Fetch customers with pagination. |
| Expected Output | API returns paged customers response. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Copy customer `id` and `applicationUserId` for linked tests.

### 4.25 Customer Detail
Endpoint and Method: `GET /api/customers/{id}`  
Required Role/Token: Admin or Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 25 |
| Test Name | View Customer By ID |
| Test Type | API / Integration Test |
| Action | Fetch one customer by ID. |
| Expected Output | API returns full customer detail. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Use valid ID from customer list endpoint.

### 4.26 Customer Purchases
Endpoint and Method: `GET /api/customers/{id}/purchases`  
Required Role/Token: Admin or Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 26 |
| Test Name | View Customer Purchases |
| Test Type | API / Integration Test |
| Action | Fetch purchase history for selected customer. |
| Expected Output | API returns list of customer sale records. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Useful for invoice and customer financial history checks.

### 4.27 Customer Services
Endpoint and Method: `GET /api/customers/{id}/services`  
Required Role/Token: Admin or Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 27 |
| Test Name | View Customer Services |
| Test Type | API / Integration Test |
| Action | Fetch appointment/service history for selected customer. |
| Expected Output | API returns list of appointments for customer. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Use same customer ID from list/detail tests.

### 4.28 Customer Vehicles
Endpoint and Method: `GET /api/customers/{id}/vehicles`  
Required Role/Token: Admin or Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 28 |
| Test Name | View Customer Vehicles |
| Test Type | API / Integration Test |
| Action | Fetch all vehicles registered under selected customer. |
| Expected Output | API returns vehicle list for customer. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
If no vehicles are linked, expect empty list with success response.

### 4.29 Create Customer
Endpoint and Method: `POST /api/customers`  
Required Role/Token: Admin or Staff token

Request JSON:

```json
{
  "fullName": "Swagger Customer User",
  "email": "swagger.customer001@autoflow.local",
  "phone": "0499000300",
  "address": "Customer Street, Sydney",
  "createLoginAccount": true
}
```

| Field | Value |
|---|---|
| Test No | 29 |
| Test Name | Create Customer |
| Test Type | API / Integration Test |
| Action | Create customer record with optional login account. |
| Expected Output | API returns created customer. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Use unique email to avoid conflict (409).

### 4.30 Update Customer
Endpoint and Method: `PUT /api/customers/{id}`  
Required Role/Token: Admin or Staff token

Request JSON:

```json
{
  "fullName": "Swagger Customer Updated",
  "email": "swagger.customer001@autoflow.local",
  "phone": "0499000301",
  "address": "Updated Customer Street, Sydney"
}
```

| Field | Value |
|---|---|
| Test No | 30 |
| Test Name | Update Customer |
| Test Type | API / Integration Test |
| Action | Update selected customer profile fields. |
| Expected Output | API returns updated customer object. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Re-run `GET /api/customers/{id}` to verify updates.

### 4.31 Vehicles List
Endpoint and Method: `GET /api/vehicles?page=1&pageSize=20`  
Required Role/Token: Admin or Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 31 |
| Test Name | View Vehicles List |
| Test Type | API / Integration Test |
| Action | Fetch global vehicles list (role-scoped). |
| Expected Output | API returns paged vehicle items. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Copy one vehicle ID for by-id/update/delete tests.

### 4.32 Create Vehicle (Admin on Behalf)
Endpoint and Method: `POST /api/vehicles`  
Required Role/Token: Admin or Staff token

Request JSON:

```json
{
  "vehicleNumber": "NSW-TEST-01",
  "brand": "Toyota",
  "model": "Corolla",
  "year": 2022,
  "mileage": 25000,
  "color": "White",
  "vin": "TESTVIN0000000001",
  "ownerUserId": "PASTE_OWNER_USER_ID_HERE"
}
```

| Field | Value |
|---|---|
| Test No | 32 |
| Test Name | Create Vehicle for Customer |
| Test Type | API / Integration Test |
| Action | Create vehicle linked to a customer using OwnerUserId. |
| Expected Output | API returns created vehicle details. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Use a valid `applicationUserId` from customer response for `ownerUserId`.

### 4.33 Appointments List
Endpoint and Method: `GET /api/appointments?page=1&pageSize=20`  
Required Role/Token: Admin, Staff, or Customer token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 33 |
| Test Name | View Appointments List |
| Test Type | API / Integration Test |
| Action | Fetch appointments list with paging. |
| Expected Output | API returns paged appointments according to role scope. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Copy appointment ID for detail/status/cancel tests.

### 4.34 Create Appointment (Admin)
Endpoint and Method: `POST /api/appointments`  
Required Role/Token: Admin or Staff token

Request JSON:

```json
{
  "customerId": "PASTE_CUSTOMER_ID_HERE",
  "vehicleId": "PASTE_VEHICLE_ID_HERE",
  "date": "2026-06-01",
  "time": "09:30:00",
  "status": "Pending"
}
```

| Field | Value |
|---|---|
| Test No | 34 |
| Test Name | Create Appointment |
| Test Type | API / Integration Test |
| Action | Create appointment for selected customer and vehicle. |
| Expected Output | API returns created appointment record. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Confirm creation using `GET /api/appointments/{id}`.

### 4.35 Update Appointment Status
Endpoint and Method: `PATCH /api/appointments/{id}/status`  
Required Role/Token: Admin or Staff token

Request JSON:

```json
{
  "status": "Confirmed"
}
```

| Field | Value |
|---|---|
| Test No | 35 |
| Test Name | Update Appointment Status |
| Test Type | API / Integration Test |
| Action | Update appointment status via status endpoint. |
| Expected Output | API returns updated appointment with new status. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Valid enum values: `Pending`, `Confirmed`, `InProgress`, `Completed`, `Cancelled`.

### 4.36 Sales List
Endpoint and Method: `GET /api/sales?page=1&pageSize=20`  
Required Role/Token: Admin or Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 36 |
| Test Name | View Sales List |
| Test Type | API / Integration Test |
| Action | Fetch sales list from sales endpoint. |
| Expected Output | API returns paged sales records. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Copy sale ID for detail/send-invoice/credit tests.

### 4.37 Send Invoice
Endpoint and Method: `POST /api/sales/{id}/send-invoice`  
Required Role/Token: Admin or Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 39 |
| Test Name | Send Sale Invoice |
| Test Type | API / Integration Test |
| Action | Send/resend invoice email for selected sale. |
| Expected Output | API returns send result including sent/failed metadata fields. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
If email settings are missing, API may return failure reason fields.

### 4.38 Credit Detail
Endpoint and Method: `GET /api/credits/{saleId}`  
Required Role/Token: Admin or Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 40 |
| Test Name | View Credit Detail |
| Test Type | API / Integration Test |
| Action | Fetch credit ledger details for one credit sale. |
| Expected Output | API returns totalCreditAmount, paidAmount, remainingAmount, status, paymentHistory. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

### 4.39 Send Credit Reminder
Endpoint and Method: `POST /api/credits/{saleId}/send-reminder`  
Required Role/Token: Admin or Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 43 |
| Test Name | Send Credit Reminder |
| Test Type | API / Integration Test |
| Action | Send credit reminder email for selected credit sale. |
| Expected Output | API returns send-reminder success/failure response with reminder metadata. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Use a valid credit sale ID; verify email settings/environment.

### 4.40 Purchase Invoice List
Endpoint and Method: `GET /api/purchase-invoices?page=1&pageSize=20`  
Required Role/Token: Admin token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 44 |
| Test Name | View Purchase Invoice List |
| Test Type | API / Integration Test |
| Action | Fetch paged purchase invoice list. |
| Expected Output | API returns paged purchase invoices with item summary data. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Copy invoice ID and vendor ID for related tests.

### 4.41 Create Purchase Invoice
Endpoint and Method: `POST /api/purchase-invoices`  
Required Role/Token: Admin token

Request JSON:

```json
{
  "vendorId": "PASTE_VENDOR_ID_HERE",
  "notes": "Swagger purchase invoice test",
  "items": [
    {
      "partId": "PASTE_PART_ID_HERE",
      "quantity": 5,
      "unitCost": 70.0
    }
  ]
}
```

| Field | Value |
|---|---|
| Test No | 45 |
| Test Name | Create Purchase Invoice |
| Test Type | API / Integration Test |
| Action | Create purchase invoice with vendor and part items. |
| Expected Output | API returns created purchase invoice and calculated totals. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Re-check part stock changes after successful purchase invoice creation.

### 4.42 Part Requests List
Endpoint and Method: `GET /api/part-requests?page=1&pageSize=20`  
Required Role/Token: Admin/Staff/Customer token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 46 |
| Test Name | View Part Requests |
| Test Type | API / Integration Test |
| Action | Fetch part requests list with role-aware scope. |
| Expected Output | API returns paged part requests. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Customers should only see their own requests.

#### Summary
For customer token, `customerId` can be omitted; backend resolves from auth user.

### 4.43 Update Part Request Status
Endpoint and Method: `PATCH /api/part-requests/{id}/status`  
Required Role/Token: Admin or Staff token

Request JSON:

```json
{
  "status": "done"
}
```

| Field | Value |
|---|---|
| Test No | 48 |
| Test Name | Update Part Request Status |
| Test Type | API / Integration Test |
| Action | Update status of existing part request. |
| Expected Output | API returns updated request status. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Supported public status values include `Pending`, `Done`, `Rejected`.

### 4.44 Reviews List
Endpoint and Method: `GET /api/reviews?page=1&pageSize=20`  
Required Role/Token: Customer/Staff/Admin token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 49 |
| Test Name | View Reviews |
| Test Type | API / Integration Test |
| Action | Fetch paged customer reviews. |
| Expected Output | API returns review records with rating/comment/customer data. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Used for admin/staff/customer review UIs.

### 4.45 Customer Reports - Top Spenders
Endpoint and Method: `GET /api/reports/customers/top-spenders`  
Required Role/Token: Admin or Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 51 |
| Test Name | Top Spenders Report |
| Test Type | API / Integration Test |
| Action | Fetch top spending customers report. |
| Expected Output | API returns list sorted by highest total spending. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Cross-check totals against seeded sales data if needed.

### 4.46 Customer Reports - Regular Customers
Endpoint and Method: `GET /api/reports/customers/regular`  
Required Role/Token: Admin or Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 52 |
| Test Name | Regular Customers Report |
| Test Type | API / Integration Test |
| Action | Fetch regular customers report. |
| Expected Output | API returns customers active in recent period. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Use for repeat-customer analytics validation.

### 4.47 Customer Reports - Pending Credit
Endpoint and Method: `GET /api/reports/customers/pending-credit`  
Required Role/Token: Admin or Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 53 |
| Test Name | Pending Credit Report |
| Test Type | API / Integration Test |
| Action | Fetch customers with pending/overdue credit balances. |
| Expected Output | API returns pending credit rows with overdue fields. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Use rows to validate credit-ledger/test data consistency.

### 4.48 Financial Report - Daily
Endpoint and Method: `GET /api/reports/financial/daily?date=2026-05-15`  
Required Role/Token: Admin token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 54 |
| Test Name | Financial Daily Report |
| Test Type | API / Integration Test |
| Action | Fetch daily financial report by date query. |
| Expected Output | API returns daily revenue/cost/profit breakdown. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Date is required; invalid date format returns 400.

### 4.49 Financial Report - Monthly
Endpoint and Method: `GET /api/reports/financial/monthly?year=2026&month=5`  
Required Role/Token: Admin token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 55 |
| Test Name | Financial Monthly Report |
| Test Type | API / Integration Test |
| Action | Fetch monthly financial summary and breakdown. |
| Expected Output | API returns monthly report for requested year-month. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Month must be 1-12.

### 4.50 Financial Report - Yearly
Endpoint and Method: `GET /api/reports/financial/yearly?year=2026`  
Required Role/Token: Admin token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 56 |
| Test Name | Financial Yearly Report |
| Test Type | API / Integration Test |
| Action | Fetch yearly report by year query. |
| Expected Output | API returns yearly summary with period breakdown. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Use this to verify annual totals against seeded transactions.

## 5. Staff API Testing

### 5.1 Staff Login
Endpoint and Method: `POST /api/auth/login`  
Required Role/Token: Public (no token)

Request JSON:

```json
{
  "email": "staff@autoflow.local",
  "password": "Admin@12345"
}
```

| Field | Value |
|---|---|
| Test No | 57 |
| Test Name | Staff Login |
| Test Type | API / Integration Test |
| Action | Authenticate as staff and retrieve JWT token. |
| Expected Output | API returns success with Staff role token. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Authorize Swagger with staff token for remaining staff tests.

### 5.2 View Customers
Endpoint and Method: `GET /api/customers?page=1&pageSize=20`  
Required Role/Token: Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 58 |
| Test Name | Staff View Customers |
| Test Type | API / Integration Test |
| Action | Fetch customer list as staff role. |
| Expected Output | API returns paged customer list (allowed). |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Staff has read access to customers endpoints.

### 5.3 View Vehicles
Endpoint and Method: `GET /api/vehicles?page=1&pageSize=20`  
Required Role/Token: Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 59 |
| Test Name | Staff View Vehicles |
| Test Type | API / Integration Test |
| Action | Fetch vehicle list as staff role. |
| Expected Output | API returns paged vehicle list. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Use one customer `applicationUserId` for add-vehicle-on-behalf flow.

### 5.4 Add Vehicle for Customer
Endpoint and Method: `POST /api/vehicles`  
Required Role/Token: Staff token

Request JSON:

```json
{
  "vehicleNumber": "STF-TEST-01",
  "brand": "Mazda",
  "model": "CX-5",
  "year": 2021,
  "mileage": 52000,
  "color": "Gray",
  "vin": "STFVIN0000000001",
  "ownerUserId": "PASTE_OWNER_USER_ID_HERE"
}
```

| Field | Value |
|---|---|
| Test No | 60 |
| Test Name | Staff Add Vehicle For Customer |
| Test Type | API / Integration Test |
| Action | Create vehicle as staff on behalf of customer. |
| Expected Output | API returns created vehicle linked to provided owner user ID. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Owner user ID should be customer ApplicationUserId.

### 5.5 View Appointments
Endpoint and Method: `GET /api/appointments?page=1&pageSize=20`  
Required Role/Token: Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 61 |
| Test Name | Staff View Appointments |
| Test Type | API / Integration Test |
| Action | Fetch appointments list as staff. |
| Expected Output | API returns appointments list. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Staff can view all appointments.

### 5.6 Book Appointment (Staff)
Endpoint and Method: `POST /api/appointments`  
Required Role/Token: Staff token

Request JSON:

```json
{
  "customerId": "PASTE_CUSTOMER_ID_HERE",
  "vehicleId": "PASTE_VEHICLE_ID_HERE",
  "date": "2026-06-01",
  "time": "09:30:00",
  "status": "Pending"
}
```

| Field | Value |
|---|---|
| Test No | 62 |
| Test Name | Staff Create Appointment |
| Test Type | API / Integration Test |
| Action | Create appointment for selected customer/vehicle. |
| Expected Output | API returns created appointment details. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Verify by fetching appointment by returned ID.

### 5.7 Update Appointment Status (Staff)
Endpoint and Method: `PATCH /api/appointments/{id}/status`  
Required Role/Token: Staff token

Request JSON:

```json
{
  "status": "Completed"
}
```

| Field | Value |
|---|---|
| Test No | 63 |
| Test Name | Staff Update Appointment Status |
| Test Type | API / Integration Test |
| Action | Update appointment status as staff. |
| Expected Output | API returns updated appointment object. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Enum values only; invalid string returns 400.

### 5.8 View Parts and Low Stock
Endpoint and Method: `GET /api/parts`, `GET /api/parts/low-stock`  
Required Role/Token: Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 64 |
| Test Name | Staff View Parts And Low Stock |
| Test Type | API / Integration Test |
| Action | Fetch parts list and low-stock list with staff token. |
| Expected Output | Both endpoints return success for staff role. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Staff has read access only for inventory in this controller.

### 5.9 View Part Requests
Endpoint and Method: `GET /api/part-requests?page=1&pageSize=20`  
Required Role/Token: Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 65 |
| Test Name | Staff View Part Requests |
| Test Type | API / Integration Test |
| Action | Fetch part requests list as staff. |
| Expected Output | API returns paged part requests list. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Use part request ID for status-update test.

### 5.10 Create Part Request (Staff)
Endpoint and Method: `POST /api/part-requests`  
Required Role/Token: Staff token

Request JSON:

```json
{
  "customerId": "PASTE_CUSTOMER_ID_HERE",
  "partName": "Staff Requested Part",
  "quantity": 1,
  "status": "Pending"
}
```

| Field | Value |
|---|---|
| Test No | 66 |
| Test Name | Staff Create Part Request |
| Test Type | API / Integration Test |
| Action | Create part request for a customer as staff. |
| Expected Output | API returns created request. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Validate with subsequent `GET /api/part-requests`.

### 5.11 Update Part Request Status (Staff)
Endpoint and Method: `PATCH /api/part-requests/{id}/status`  
Required Role/Token: Staff token

Request JSON:

```json
{
  "status": "Done"
}
```

| Field | Value |
|---|---|
| Test No | 67 |
| Test Name | Staff Update Part Request Status |
| Test Type | API / Integration Test |
| Action | Update request status with staff role. |
| Expected Output | API returns updated request status. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
`Done` maps to domain fulfilled status.

### 5.12 View Sales
Endpoint and Method: `GET /api/sales?page=1&pageSize=20`  
Required Role/Token: Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 68 |
| Test Name | Staff View Sales |
| Test Type | API / Integration Test |
| Action | Fetch sales list as staff. |
| Expected Output | API returns paged sales. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Copy sale ID for send-invoice/credit operations.

### 5.13 Create Sale (Staff)
Endpoint and Method: `POST /api/sales`  
Required Role/Token: Staff token

Request JSON:

```json
{
  "customerId": "PASTE_CUSTOMER_ID_HERE",
  "paymentMethod": "Cash",
  "notes": "Staff sale test",
  "items": [
    {
      "partId": "PASTE_PART_ID_HERE",
      "quantity": 1
    }
  ]
}
```

| Field | Value |
|---|---|
| Test No | 69 |
| Test Name | Staff Create Sale |
| Test Type | API / Integration Test |
| Action | Create sale with staff role and valid customer/part IDs. |
| Expected Output | API returns created sale; invoice generated. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Staff profile must be active, otherwise endpoint can return `403`.

### 5.14 Send Invoice (Staff)
Endpoint and Method: `POST /api/sales/{id}/send-invoice`  
Required Role/Token: Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 70 |
| Test Name | Staff Send Invoice |
| Test Type | API / Integration Test |
| Action | Send/resend invoice for selected sale. |
| Expected Output | API returns send result metadata. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Check send status fields in response (`invoiceSentAt` / failure details).

### 5.15 Credit Detail / Payment / Reminder (Staff)
Endpoint and Method: `GET /api/credits/{saleId}`, `POST /api/credits/{saleId}/payments`, `POST /api/credits/{saleId}/send-reminder`  
Required Role/Token: Staff token

Request JSON for payment:

```json
{
  "amount": 50.0,
  "paymentDate": "2026-06-01T12:00:00Z",
  "paymentMethod": "Cash",
  "note": "Staff credit payment test"
}
```

| Field | Value |
|---|---|
| Test No | 71 |
| Test Name | Staff Credit Operations |
| Test Type | API / Integration Test |
| Action | Fetch credit detail, record payment, and send reminder for credit sale. |
| Expected Output | APIs return success for allowed staff credit operations. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Use credit sale ID from sales list or created credit sale.

### 5.16 View Reviews (Staff)
Endpoint and Method: `GET /api/reviews`, `POST /api/reviews`  
Required Role/Token: Staff token

Request JSON for create:

```json
{
  "customerId": "PASTE_CUSTOMER_ID_HERE",
  "rating": 4,
  "comment": "Staff-created review test"
}
```

| Field | Value |
|---|---|
| Test No | 72 |
| Test Name | Staff Review Operations |
| Test Type | API / Integration Test |
| Action | Fetch reviews and create a review with staff token. |
| Expected Output | Both endpoints return success for staff role. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Rating must be 1-5.

### 5.17 View Customer Reports (Staff)
Endpoint and Method: `GET /api/reports/customers/top-spenders`, `GET /api/reports/customers/regular`, `GET /api/reports/customers/pending-credit`  
Required Role/Token: Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 73 |
| Test Name | Staff Customer Reports Access |
| Test Type | API / Integration Test |
| Action | Access all customer report endpoints as staff. |
| Expected Output | API returns success for all three report endpoints. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Staff is allowed for customer report endpoints.

### 5.18 Forbidden Test - Staff Access to Admin Dashboard
Endpoint and Method: `GET /api/dashboard`  
Required Role/Token: Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 74 |
| Test Name | Staff Forbidden Dashboard Access |
| Test Type | API / Integration Test |
| Action | Call admin-only dashboard endpoint with staff token. |
| Expected Output | API returns HTTP 403 Forbidden. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
This test should fail with HTTP 403 because the logged-in role does not have permission.

### 5.19 Forbidden Test - Staff Management Access
Endpoint and Method: `GET /api/staff`  
Required Role/Token: Staff token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 75 |
| Test Name | Staff Forbidden Staff Management Access |
| Test Type | API / Integration Test |
| Action | Attempt admin-only staff management list endpoint as staff. |
| Expected Output | API returns HTTP 403 Forbidden. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
This test should fail with HTTP 403 because the logged-in role does not have permission.

### 5.20 Forbidden Test - Vendor View
Endpoint and Method: `POST /api/vendors`, `PUT /api/vendors/{id}`  
Required Role/Token: Staff token

Request JSON:

```json
{
  "vendorName": "Staff Forbidden Vendor",
  "contactPerson": "Forbidden",
  "phone": "0499000999",
  "email": "forbidden@autoflow.local",
  "address": "No Access"
}
```

| Field | Value |
|---|---|
| Test No | 76 |
| Test Name | Staff Forbidden Vendor Mutations |
| Test Type | API / Integration Test |
| Action | Attempt vendor create/update with staff token. |
| Expected Output | API returns HTTP 403 Forbidden. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
This test should fail with HTTP 403 because the logged-in role does not have permission.

## 6. Customer API Testing

### 6.1 Customer Login
Endpoint and Method: `POST /api/auth/login`  
Required Role/Token: Public (no token)

Request JSON:

```json
{
  "email": "customer@autoflow.local",
  "password": "Admin@12345"
}
```

| Field | Value |
|---|---|
| Test No | 77 |
| Test Name | Customer Login |
| Test Type | API / Integration Test |
| Action | Authenticate with customer credentials. |
| Expected Output | API returns success with Customer role token. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Authorize Swagger with customer token for customer self-service tests.

### 6.2 View My Profile
Endpoint and Method: `GET /api/customers/me/profile`  
Required Role/Token: Customer token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 78 |
| Test Name | Customer View Own Profile |
| Test Type | API / Integration Test |
| Action | Fetch customer self profile endpoint. |
| Expected Output | API returns profile for authenticated customer only. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Customer profile endpoint is role-specific (`/api/customers/me/profile`).

### 6.3 Update My Profile
Endpoint and Method: `PATCH /api/customers/me/profile`  
Required Role/Token: Customer token

Request JSON:

```json
{
  "fullName": "Updated Customer Name",
  "phone": "0499111222",
  "address": "Updated Sydney Address"
}
```

| Field | Value |
|---|---|
| Test No | 79 |
| Test Name | Customer Update Own Profile |
| Test Type | API / Integration Test |
| Action | Update customer own profile fields. |
| Expected Output | API returns updated profile details. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Email is not changed via this patch DTO.

### 6.4 View My Vehicles
Endpoint and Method: `GET /api/vehicles?page=1&pageSize=20`  
Required Role/Token: Customer token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 80 |
| Test Name | Customer View Own Vehicles |
| Test Type | API / Integration Test |
| Action | Fetch vehicles using customer token. |
| Expected Output | API returns only vehicles owned by authenticated customer. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Role-based filtering happens in backend service.

### 6.5 Add My Vehicle
Endpoint and Method: `POST /api/vehicles`  
Required Role/Token: Customer token

Request JSON:

```json
{
  "vehicleNumber": "CUS-TEST-01",
  "brand": "Toyota",
  "model": "RAV4",
  "year": 2023,
  "mileage": 15000,
  "color": "Silver",
  "vin": "CUSTOMERVIN000001"
}
```

| Field | Value |
|---|---|
| Test No | 81 |
| Test Name | Customer Add Vehicle |
| Test Type | API / Integration Test |
| Action | Create a vehicle as authenticated customer. |
| Expected Output | API returns created vehicle linked to current customer. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Do not pass `ownerUserId` for customer self-create flow.

### 6.6 Update My Vehicle
Endpoint and Method: `PUT /api/vehicles/{id}`  
Required Role/Token: Customer token

Request JSON:

```json
{
  "vehicleNumber": "CUS-TEST-01",
  "brand": "Toyota",
  "model": "RAV4",
  "year": 2023,
  "mileage": 16000,
  "color": "Black",
  "vin": "CUSTOMERVIN000001"
}
```

| Field | Value |
|---|---|
| Test No | 82 |
| Test Name | Customer Update Own Vehicle |
| Test Type | API / Integration Test |
| Action | Update a vehicle owned by authenticated customer. |
| Expected Output | API returns updated vehicle. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
If vehicle belongs to another user, expect forbidden/not found behavior.

### 6.7 Delete My Vehicle
Endpoint and Method: `DELETE /api/vehicles/{id}`  
Required Role/Token: Customer token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 83 |
| Test Name | Customer Delete Own Vehicle |
| Test Type | API / Integration Test |
| Action | Delete a customer-owned vehicle. |
| Expected Output | API returns success for owner delete. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Only owner or staff/admin can delete vehicle.

### 6.8 View My Appointments
Endpoint and Method: `GET /api/appointments?page=1&pageSize=20`  
Required Role/Token: Customer token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 84 |
| Test Name | Customer View Own Appointments |
| Test Type | API / Integration Test |
| Action | Fetch appointments with customer token. |
| Expected Output | API returns only authenticated customer appointments. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Use returned appointment ID for cancel test.

### 6.9 Book Appointment (Customer)
Endpoint and Method: `POST /api/appointments`  
Required Role/Token: Customer token

Request JSON:

```json
{
  "vehicleId": "PASTE_MY_VEHICLE_ID_HERE",
  "date": "2026-06-01",
  "time": "09:30:00",
  "status": "Pending"
}
```

| Field | Value |
|---|---|
| Test No | 85 |
| Test Name | Customer Book Appointment |
| Test Type | API / Integration Test |
| Action | Create appointment as customer for own vehicle. |
| Expected Output | API returns created appointment linked to customer. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
`customerId` is optional for customer role; backend resolves from token.

### 6.10 Cancel Own Appointment
Endpoint and Method: `PATCH /api/appointments/{id}/cancel`  
Required Role/Token: Customer token

Request JSON:

```json
{
  "reason": "Unable to attend"
}
```

| Field | Value |
|---|---|
| Test No | 86 |
| Test Name | Customer Cancel Own Appointment |
| Test Type | API / Integration Test |
| Action | Cancel one appointment owned by authenticated customer. |
| Expected Output | API returns appointment with `Cancelled` status. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Customer cannot cancel another customer’s appointment.

### 6.11 View My Purchases
Endpoint and Method: `GET /api/customers/me/purchases`  
Required Role/Token: Customer token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 87 |
| Test Name | Customer View Purchases |
| Test Type | API / Integration Test |
| Action | Fetch purchase history from customer self-service endpoint. |
| Expected Output | API returns list of sales for authenticated customer. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
This endpoint replaces admin/staff-only customer purchase routes for customer role.

### 6.12 View My Services
Endpoint and Method: `GET /api/customers/me/services`  
Required Role/Token: Customer token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 88 |
| Test Name | Customer View Services |
| Test Type | API / Integration Test |
| Action | Fetch service history (appointments) for customer. |
| Expected Output | API returns service/appointment list for current customer. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Use this endpoint for customer service-history page validation.

### 6.13 View My Part Requests
Endpoint and Method: `GET /api/part-requests?page=1&pageSize=20`  
Required Role/Token: Customer token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 89 |
| Test Name | Customer View Part Requests |
| Test Type | API / Integration Test |
| Action | Fetch part requests with customer token. |
| Expected Output | API returns only customer-owned part requests. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Role filter should hide other customers’ requests.

### 6.14 Create Part Request (Customer)
Endpoint and Method: `POST /api/part-requests`  
Required Role/Token: Customer token

Request JSON:

```json
{
  "partName": "Customer Requested Brake Pad",
  "quantity": 1
}
```

| Field | Value |
|---|---|
| Test No | 90 |
| Test Name | Customer Create Part Request |
| Test Type | API / Integration Test |
| Action | Submit part request as customer. |
| Expected Output | API returns created request linked to current customer. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
`customerId` optional; backend resolves from customer token.

### 6.15 View Reviews
Endpoint and Method: `GET /api/reviews?page=1&pageSize=20`  
Required Role/Token: Customer token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 91 |
| Test Name | Customer View Reviews |
| Test Type | API / Integration Test |
| Action | Fetch review list as customer. |
| Expected Output | API returns reviews list (global visibility). |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Use this plus create-review to validate customer review flow.

### 6.16 Create Review (Customer)
Endpoint and Method: `POST /api/reviews`  
Required Role/Token: Customer token

Request JSON:

```json
{
  "rating": 5,
  "comment": "Very good service from customer Swagger test"
}
```

| Field | Value |
|---|---|
| Test No | 92 |
| Test Name | Customer Create Review |
| Test Type | API / Integration Test |
| Action | Submit review with customer token. |
| Expected Output | API returns created review record. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Rating must be between 1 and 5.

### 6.17 View Predictions (Customer)
Endpoint and Method: `GET /api/predictions/{customerId}`  
Required Role/Token: Customer token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 93 |
| Test Name | Customer View Predictions |
| Test Type | API / Integration Test |
| Action | Fetch failure predictions for authenticated customer ID. |
| Expected Output | API returns prediction list if data exists; otherwise not-found/empty depending service result. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
Use your own customer ID from profile endpoint.

### 6.18 Forbidden Test - Customer Status Update
Endpoint and Method: `PATCH /api/appointments/{id}/status`  
Required Role/Token: Customer token

Request JSON:

```json
{
  "status": "Completed"
}
```

| Field | Value |
|---|---|
| Test No | 94 |
| Test Name | Customer Forbidden Appointment Status Update |
| Test Type | API / Integration Test |
| Action | Attempt staff/admin-only status update with customer token. |
| Expected Output | API returns HTTP 403 Forbidden. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
This test should fail with HTTP 403 because the logged-in role does not have permission.

### 6.19 Forbidden Test - Customer Sales Create
Endpoint and Method: `POST /api/sales`  
Required Role/Token: Customer token

Request JSON:

```json
{
  "customerId": "PASTE_CUSTOMER_ID_HERE",
  "paymentMethod": "Cash",
  "notes": "Forbidden test",
  "items": [
    {
      "partId": "PASTE_PART_ID_HERE",
      "quantity": 1
    }
  ]
}
```

| Field | Value |
|---|---|
| Test No | 95 |
| Test Name | Customer Forbidden Sales Create |
| Test Type | API / Integration Test |
| Action | Attempt to create sale with customer token. |
| Expected Output | API returns HTTP 403 Forbidden. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
This test should fail with HTTP 403 because the logged-in role does not have permission.

### 6.20 Forbidden Test - Customer Staff Management Access
Endpoint and Method: `GET /api/staff`  
Required Role/Token: Customer token

Request JSON: N/A

| Field | Value |
|---|---|
| Test No | 96 |
| Test Name | Customer Forbidden Staff Management Access |
| Test Type | API / Integration Test |
| Action | Attempt admin-only staff management endpoint with customer token. |
| Expected Output | API returns HTTP 403 Forbidden. |
| Actual Output | To be filled after testing. |
| Conclusion | Pass / Fail |

#### Summary
This test should fail with HTTP 403 because the logged-in role does not have permission.

## 7. Role Permission Matrix

| Feature | Admin | Staff | Customer |
|---|---|---|---|
| Dashboard (`/api/dashboard*`) | Yes | No | No |
| Staff Management (`/api/staff`) | Yes | No | No |
| Vendor Management (`/api/vendors`) | Yes | No | No |
| Parts Inventory (`/api/parts`) | All CRUD | Read-only | No direct |
| Customers (`/api/customers`) | Yes | Yes | No |
| Customer Self-Service (`/api/customers/me/*`) | No | No | Yes |
| Vehicles (`/api/vehicles`) | All | All | Own only |
| Appointments (`/api/appointments`) | All + status update | All + status update | Own list/create/cancel |
| Sales (`/api/sales`) | View + send invoice | Create + view + send invoice | No |
| Credits (`/api/credits/*`) | View + send reminder | View + payment + status + reminder | No |
| Part Requests (`/api/part-requests`) | All + status update | All + status update | Own create/view |
| Reviews (`/api/reviews`) | View/Create | View/Create | View/Create |
| Customer Reports (`/api/reports/customers/*`) | Yes | Yes | No |
| Financial Reports (`/api/reports/financial/*`) | Yes | No | No |
| Predictions (`/api/predictions/{customerId}`) | Yes | Yes | Own only |

## 8. Common Swagger Errors and Fixes

- **401 Unauthorized**
  - Cause: Missing/expired JWT or malformed `Bearer` token.
  - Fix: Re-login and re-authorize in Swagger.

- **403 Forbidden**
  - Cause: Token role does not match endpoint `[Authorize(Roles=...)]`.
  - Fix: Login with required role and retry.

- **400 Bad Request**
  - Cause: DTO validation failure or bad enum/date values.
  - Fix: Match JSON fields exactly to DTOs and use valid values.

- **Weak password error**
  - Cause: Password does not satisfy identity policy.
  - Fix: Use strong password with uppercase/lowercase/number/symbol.

- **Invalid GUID**
  - Cause: `{id}` path uses malformed or non-existing GUID.
  - Fix: Copy valid GUID from related list endpoint first.

- **Wrong date/time format**
  - Cause: Date/Time values not parseable for `DateOnly`/`TimeOnly`/`DateTime`.
  - Fix: Use `YYYY-MM-DD`, `HH:mm:ss`, and ISO UTC datetime where required.

- **Port already in use**
  - Cause: Existing process on port 5294.
  - Fix: `lsof -i :5294` then `kill -9 <PID>`.

## 9. Recommended Full Test Order

1. Run environment setup and seed.
2. Admin login and authorize token.
3. Core master data:
   - Vendors
   - Parts
   - Customers
   - Vehicles
4. Operational flows:
   - Appointments
   - Sales
   - Credit operations
   - Purchase invoices
   - Part requests
5. Reporting and dashboard tests.
6. Staff login and role-scoped functional + forbidden tests.
7. Customer login and self-service + forbidden tests.
8. Re-run key GET endpoints to verify created/updated records.

---

### Notes on Endpoints Not Available in Current Controllers

- `GET /api/customers/{id}/reviews` is **not present**.
- `PUT /api/purchase-invoices/{id}` update endpoint is **not present**.
- `GET /api/credits` list endpoint is **not present** (only `/api/credits/{saleId}` and actions).
- Admin-specific cancel route for appointments is **not present**; Admin/Staff should use status update endpoint.
