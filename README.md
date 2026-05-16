# 🚗 AutoFlow — Auto Repair Shop Management System (Backend)

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-10.0-512BD4)](https://learn.microsoft.com/en-us/aspnet/core/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-EF_Core-336791?logo=postgresql)](https://www.postgresql.org/)
[![Architecture](https://img.shields.io/badge/Architecture-Clean-blueviolet)](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)

---

## 📖 Project Description

**AutoFlow** is a production-ready RESTful backend API for managing the full day-to-day operations of an auto repair shop. It provides a structured, role-based system covering customers, vehicles, parts inventory, sales, credit management, purchase invoices, staff, appointments, and rich financial reporting — all from a single unified API built on **Clean Architecture**.

### Problem it solves

Running an auto repair shop involves many moving parts: tracking vehicle service history, managing spare parts stock, recording sales and credit balances, scheduling appointments, and producing financial summaries. AutoFlow replaces fragmented spreadsheets and manual records with a clean, well-structured API that a frontend or mobile app can consume directly — with server-enforced business rules, automated email alerts, and a predictive maintenance engine built in.

---

## ✨ Features

- **Authentication & Authorization** — JWT-based login and registration with three roles: Admin, Staff, and Customer
- **Customer Self-Registration** — Customers register via a public endpoint; a linked `Customer` profile and Identity account are created atomically via a dedicated `RegistrationService`
- **Customer Management** — Full CRUD for customer profiles, service history, vehicle history, and purchase history
- **Vehicle Management** — Register and track vehicles per customer with make, model, year, and mileage
- **Parts & Inventory** — Manage spare parts with stock tracking, low-stock detection, and part search
- **Sales** — Create sales transactions with line items; supports Cash and Credit payment methods; email invoice to customer via HTML template
- **Credit Management** — Full credit lifecycle per sale: view credit details, record partial payments, update credit status, and send overdue payment reminder emails
- **Purchase Invoices** — Record vendor purchases with line items, status tracking, and vendor association
- **Vendor Management** — Full CRUD for suppliers with search
- **Appointments** — Create appointments linked to a customer and vehicle; update appointment status; cancel appointments
- **Staff Management** — Admin-level CRUD for staff profiles; Staff can also view and update their own profile via self-service endpoints
- **Admin Profile** — Admins can view and update their own profile and change their password
- **Failure Prediction** — Rule-based engine flagging vehicles likely needing battery, brake pad, coolant, timing belt, or transmission fluid service based on mileage and service history
- **Customer Self-Service** — Customers can view their own purchase history, service records, and update their own profile
- **Reviews** — Customers can submit service reviews; Admin and Staff can list all reviews
- **Part Requests** — Staff raise requests for new or additional stock parts; Admin can update request status
- **Dashboard** — Rich admin/staff dashboard with overall stats, a live activity stream, revenue trend (daily / weekly / monthly), fast-moving inventory, and priority alerts
- **Financial Reports** — Daily, monthly, and yearly revenue summaries (Admin only)
- **Customer Reports** — Top spenders, regular customers, and pending credit customers (Admin and Staff)
- **Email Notifications** — Automated alerts for low-stock parts and overdue credit via SMTP (MailKit); HTML invoice emails on sale creation
- **Unit of Work + Transactions** — Multi-step operations (e.g. credit payment recording) are wrapped in database transactions via `IUnitOfWork`
- **Pagination** — All list endpoints support `PagedRequest` / `PagedResponse` with `page`, `pageSize`, `totalCount`, and `totalPages`
- **Global Exception Handling** — All unhandled exceptions are caught by `GlobalExceptionMiddleware` and returned as a consistent `ApiResponse<T>` shape

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| **Framework** | ASP.NET Core 10 |
| **Language** | C# (.NET 10) |
| **Database** | PostgreSQL |
| **ORM** | Entity Framework Core 10 (Npgsql) |
| **Authentication** | ASP.NET Identity + JWT Bearer Tokens |
| **Validation** | FluentValidation |
| **Email** | MailKit (SMTP) |
| **API Docs** | Swagger / OpenAPI (Swashbuckle) |
| **Architecture** | Clean Architecture (Domain / Application / Infrastructure / API) |

---

## 🏗️ Architecture Overview

AutoFlow follows **Clean Architecture** across four projects, with a strict inward dependency rule:

```
┌─────────────────────────────────────────────────────┐
│               API Layer (AutoFlow-Backend)           │
│   Controllers · Middleware · Converters · Program   │
└────────────────────────┬────────────────────────────┘
                         │ depends on
┌────────────────────────▼────────────────────────────┐
│          Application Layer (.Application)            │
│   Services · Interfaces · DTOs · Validators         │
│   Mappers · Prediction Rules · Common utilities     │
└──────────────┬──────────────────────────────────────┘
               │ depends on                  ▲ implements
┌──────────────▼──────────────┐   ┌──────────────────────────────────┐
│   Domain Layer (.Domain)    │   │  Infrastructure Layer (.Infra)   │
│   Entities · Enums          │   │  Repositories · EF Core · Email  │
│   (no dependencies)         │   │  Identity · UnitOfWork · Seeder  │
└─────────────────────────────┘   └──────────────────────────────────┘
```

**Key rule:** Domain and Application layers never depend on Infrastructure. Infrastructure implements the interfaces defined in Application.

---

## 📁 Project Structure

```
AutoFlow-Backend/                           ← API layer
├── Controllers/                            # One controller per resource (21 controllers)
├── Middleware/
│   └── GlobalExceptionMiddleware.cs        # Catches all unhandled exceptions → ApiResponse
├── Converters/                             # Custom JSON converters (DateOnly, TimeOnly)
├── Extensions/
│   └── ApiResponseExtensions.cs           # IActionResult helpers for ApiResponse
└── Program.cs                             # App startup, DI registration, middleware pipeline

AutoFlow-Backend.Application/              ← Business logic layer
├── Common/
│   ├── ApiResponse.cs / ApiResponseFactory.cs  # Unified { success, message, data } response
│   ├── PagedRequest.cs / PagedResponse.cs      # Generic pagination models
│   ├── BusinessRulesSettings.cs               # Typed config (loyalty discount, regular customer threshold)
│   ├── FailurePredictionRules.cs              # Rule constants (mileage thresholds)
│   ├── PasswordGenerator.cs                   # Auto-generate secure passwords
│   └── StringNormalizer.cs                    # Email/string normalisation helpers
├── DTOs/                                  # Request + Response DTOs per resource
├── Interfaces/                            # Service contracts + Repository contracts
│   └── Repositories/                     # IRepositoryBase + specific repository interfaces
├── Mappers/                               # Entity ↔ DTO mapping (manual, no AutoMapper)
├── Models/                                # Internal read models for complex queries
├── Services/
│   ├── RegistrationService.cs            # Atomic customer self-registration (Identity + Customer row)
│   ├── CreditService.cs                  # Credit detail, payment recording, reminder emails
│   ├── DashboardService.cs               # Stats, activity stream, revenue trend, fast-moving inventory
│   ├── FailurePredictionService.cs       # Orchestrates prediction rules per vehicle
│   ├── PredictionRules/                  # BatteryRule, BrakePadRule, CoolantRule, TimingBeltRule, TransmissionFluidRule
│   ├── SaleService.cs / PartService.cs / CustomerService.cs ...  # One service per domain entity
│   └── ...
├── Validators/                            # FluentValidation validators per request DTO
└── DependencyInjection.cs                # Application-layer service registration

AutoFlow-Backend.Domain/                   ← Core domain (no dependencies)
├── Entities/                              # Appointment, CreditPayment, Customer, Part, PartRequest,
│                                          # PurchaseInvoice, PurchaseInvoiceItem, Review,
│                                          # Sale, SaleItem, Staff, Vehicle, Vendor
└── Enums/                                 # AppointmentStatus, CreditStatus, PartRequestStatus,
                                           # PaymentMethod, PurchaseInvoiceStatus, SaleStatus, StockStatus

AutoFlow-Backend.Infrastructure/           ← Data access & external services
├── Configuration/                         # Typed config: CompanySettings, EmailSettings, JwtSettings
├── Data/
│   ├── AppDbContext.cs                    # EF Core DbContext
│   ├── AppDbContextFactory.cs            # Design-time factory for migrations
│   ├── Configurations/                   # Fluent API entity config (one file per entity)
│   └── UnitOfWork.cs                     # IUnitOfWork: BeginTransactionAsync / Commit / Rollback
├── Identity/
│   └── IdentitySeeder.cs                 # Seeds default Admin user on startup
├── Migrations/                            # EF Core migration history (17 migrations)
├── Repositories/                          # Concrete repository implementations
│   ├── RepositoryBase.cs                 # Generic CRUD base
│   ├── ReportQueryRepository.cs          # Complex dashboard + report queries
│   └── ...                               # One repository per aggregate
├── Services/
│   ├── AuthService.cs                    # JWT token generation
│   ├── EmailService.cs                   # MailKit SMTP wrapper
│   ├── IdentityService.cs               # ASP.NET Identity abstraction
│   └── InvoiceTemplateBuilder.cs        # Builds HTML email invoice from SaleInvoiceDto
└── DependencyInjection.cs               # Infrastructure-layer service registration
```

---

## ⚙️ Installation Guide

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [PostgreSQL](https://www.postgresql.org/download/) (v13 or later)
- A terminal / command prompt
- *(Optional)* [Postman](https://www.postman.com/) or Swagger UI at `/swagger` for testing

---

### 1. Clone the repository

```bash
git clone https://github.com/your-username/AutoFlow-Backend.git
cd AutoFlow-Backend
```

---

### 2. Set up the database

Create a PostgreSQL database:

```sql
CREATE DATABASE "AutoFlow";
```

---

### 3. Configure environment variables

```bash
cp AutoFlow-Backend/appsettings.Example.json AutoFlow-Backend/appsettings.json
```

Open `appsettings.json` and fill in your values:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=AutoFlow;Username=YOUR_DB_USER;Password=YOUR_DB_PASSWORD"
  },
  "Jwt": {
    "Key": "YOUR_JWT_SECRET_KEY_MIN_32_CHARS",
    "Issuer": "AutoFlow-Backend",
    "Audience": "AutoFlow-Users"
  },
  "SeedAdmin": {
    "Enabled": true,
    "Email": "admin@autoflow.local",
    "Password": "Admin@12345",
    "FirstName": "System",
    "LastName": "Admin"
  },
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderName": "AutoFlow System",
    "Password": "your-app-password"
  }
}
```

> **💡 Gmail tip:** Generate an [App Password](https://myaccount.google.com/apppasswords) instead of your real Gmail password. Requires 2FA to be enabled.

> **⚠️ Never commit `appsettings.json`.** It is already in `.gitignore`. Only commit `appsettings.Example.json` with placeholder values.

---

### 4. Restore dependencies

```bash
dotnet restore
```

---

### 5. Apply database migrations

```bash
dotnet ef database update \
  --project AutoFlow-Backend.Infrastructure \
  --startup-project AutoFlow-Backend
```

This runs all 17 migrations and creates the full schema. On first run, `IdentitySeeder` also creates the default Admin account using your `SeedAdmin` config.

---

## ▶️ How to Run the Project Locally

```bash
dotnet run --project AutoFlow-Backend
```

The API starts at:

```
http://localhost:5000
https://localhost:5001
```

### Access Swagger UI

```
http://localhost:5000/swagger
```

> **💡 To authenticate in Swagger:** Call `POST /api/auth/login`, copy the `token` from the response, click **Authorize** at the top of Swagger, and enter `Bearer <your-token>`.

---

## 🔐 Environment Variables

| Key | Required | Description | Example |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | ✅ | PostgreSQL connection string | `Host=localhost;Port=5432;Database=AutoFlow;Username=postgres;Password=secret` |
| `Jwt:Key` | ✅ | JWT signing secret (min 32 chars) | `MySuperSecretKeyThatIsLongEnough!!` |
| `Jwt:Issuer` | ✅ | JWT issuer | `AutoFlow-Backend` |
| `Jwt:Audience` | ✅ | JWT audience | `AutoFlow-Users` |
| `SeedAdmin:Enabled` | No | Seed default admin on startup | `true` |
| `SeedAdmin:Email` | No | Default admin email | `admin@autoflow.local` |
| `SeedAdmin:Password` | No | Default admin password | `Admin@12345` |
| `SeedAdmin:FirstName` | No | Admin first name | `System` |
| `SeedAdmin:LastName` | No | Admin last name | `Admin` |
| `EmailSettings:Host` | No | SMTP server host | `smtp.gmail.com` |
| `EmailSettings:Port` | No | SMTP server port | `587` |
| `EmailSettings:SenderEmail` | No | Sender email address | `yourapp@gmail.com` |
| `EmailSettings:SenderName` | No | Sender display name | `AutoFlow System` |
| `EmailSettings:Password` | No | SMTP password or app password | `your-app-password` |

> **💡 Email is optional for local dev.** If SMTP credentials are missing, email-sending operations will fail gracefully and log the error — they won't crash the API.

---

## 📡 API Endpoints

All endpoints are prefixed with `/api`. Protected routes require `Authorization: Bearer <token>`.

---

### 🔑 Authentication — `/api/auth`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | Public | Self-register as a Customer |
| `POST` | `/api/auth/login` | Public | Login and receive a JWT token |
| `POST` | `/api/auth/change-password` | Any | Change own password |

---

### 👤 Customers — `/api/customers`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/customers` | Admin, Staff | Create a customer |
| `GET` | `/api/customers` | Admin, Staff | List all customers (paginated) |
| `GET` | `/api/customers/{id}` | Admin, Staff | Get customer by ID |
| `GET` | `/api/customers/search` | Admin, Staff | Search customers |
| `PUT` | `/api/customers/{id}` | Admin, Staff | Update a customer |
| `POST` | `/api/customers/{id}/vehicles` | Admin, Staff | Register a vehicle for a customer |
| `GET` | `/api/customers/{id}/vehicles` | Admin, Staff | List a customer's vehicles |
| `GET` | `/api/customers/{id}/purchases` | Admin, Staff | View a customer's purchase history |
| `GET` | `/api/customers/{id}/services` | Admin, Staff | View a customer's service history |

### 🧍 Customer Self-Service — `/api/customers/me`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/api/customers/me/profile` | Customer | View own profile |
| `PATCH` | `/api/customers/me/profile` | Customer | Update own profile |
| `GET` | `/api/customers/me/purchases` | Customer | View own purchase history |
| `GET` | `/api/customers/me/services` | Customer | View own service history |

---

### 🚙 Vehicles — `/api/vehicles`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/vehicles` | Admin, Staff | Create a vehicle |
| `GET` | `/api/vehicles` | Admin, Staff | List all vehicles |
| `GET` | `/api/vehicles/{id}` | Admin, Staff | Get vehicle by ID |
| `PUT` | `/api/vehicles/{id}` | Admin, Staff | Update a vehicle |
| `DELETE` | `/api/vehicles/{id}` | Admin, Staff | Delete a vehicle |

---

### 🔩 Parts & Inventory — `/api/parts`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/parts` | Admin | Create a part |
| `GET` | `/api/parts` | Admin, Staff | List all parts (paginated) |
| `GET` | `/api/parts/{id}` | Admin, Staff | Get part by ID |
| `GET` | `/api/parts/search` | Admin, Staff | Search parts by name or number |
| `GET` | `/api/parts/low-stock` | Admin, Staff | List parts below minimum stock level |
| `PUT` | `/api/parts/{id}` | Admin, Staff | Update a part |
| `DELETE` | `/api/parts/{id}` | Admin | Delete a part |

---

### 💰 Sales — `/api/sales`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/sales` | Staff | Create a sale (Cash or Credit) |
| `GET` | `/api/sales` | Admin, Staff | List all sales (paginated) |
| `GET` | `/api/sales/{id}` | Admin, Staff | Get sale by ID |
| `GET` | `/api/sales/customer/{customerId}` | Admin, Staff | Sales by customer |
| `POST` | `/api/sales/{id}/send-invoice` | Admin, Staff | Email HTML invoice to customer |

---

### 💳 Credits — `/api/credits`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/api/credits/{saleId}` | Admin, Staff | Get credit details for a sale (balance, payments, overdue days) |
| `POST` | `/api/credits/{saleId}/payments` | Admin, Staff | Record a partial or full credit payment |
| `PATCH` | `/api/credits/{saleId}/status` | Staff | Update credit status (Outstanding / Partial / Paid / Overdue / WrittenOff) |
| `POST` | `/api/credits/{saleId}/send-reminder` | Admin, Staff | Send overdue payment reminder email to customer |

---

### 🧾 Purchase Invoices — `/api/purchase-invoices`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/purchase-invoices` | Admin, Staff | Create a purchase invoice |
| `GET` | `/api/purchase-invoices` | Admin, Staff | List all purchase invoices |
| `GET` | `/api/purchase-invoices/{id}` | Admin, Staff | Get by ID |
| `GET` | `/api/purchase-invoices/vendor/{vendorId}` | Admin, Staff | Invoices by vendor |

---

### 🏭 Vendors — `/api/vendors`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/vendors` | Admin | Create a vendor |
| `GET` | `/api/vendors` | Admin, Staff | List all vendors |
| `GET` | `/api/vendors/{id}` | Admin, Staff | Get vendor by ID |
| `GET` | `/api/vendors/search` | Admin, Staff | Search vendors |
| `PUT` | `/api/vendors/{id}` | Admin | Update a vendor |
| `DELETE` | `/api/vendors/{id}` | Admin | Delete a vendor |

---

### 🗓️ Appointments — `/api/appointments`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/appointments` | Admin, Staff | Create an appointment |
| `GET` | `/api/appointments` | Admin, Staff | List appointments (paginated) |
| `GET` | `/api/appointments/{id}` | Admin, Staff | Get appointment by ID |
| `PATCH` | `/api/appointments/{id}/status` | Admin, Staff | Update appointment status |
| `PATCH` | `/api/appointments/{id}/cancel` | Admin, Staff | Cancel an appointment |

---

### 👨‍🔧 Staff — `/api/staff`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/staff` | Admin | Create a staff profile |
| `GET` | `/api/staff` | Admin | List all staff |
| `GET` | `/api/staff/{id}` | Admin | Get staff by ID |
| `PUT` | `/api/staff/{id}` | Admin | Update a staff profile |
| `DELETE` | `/api/staff/{id}` | Admin | Delete a staff profile |

### 🪪 Staff Self-Service — `/api/staff/me`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/api/staff/me/profile` | Staff, Admin | View own staff profile |
| `PATCH` | `/api/staff/me/profile` | Staff, Admin | Update own staff profile |

---

### 🛡️ Admin Profile — `/api/admin`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/api/admin/profile` | Admin | View own admin profile |
| `PUT` | `/api/admin/profile` | Admin | Update own admin profile |
| `POST` | `/api/admin/change-password` | Admin | Change own password |

---

### 🔮 Failure Predictions — `/api/predictions`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/api/predictions/{customerId}` | Admin, Staff | Run all prediction rules against a customer's vehicles |

---

### 📊 Dashboard — `/api/dashboard`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/api/dashboard` | Admin, Staff | Overall stats: revenue (today/monthly/yearly), counts, low-stock list, average review rating |
| `GET` | `/api/dashboard/activity-stream` | Admin, Staff | Recent activity feed (`?limit=N`) |
| `GET` | `/api/dashboard/revenue-trend` | Admin, Staff | Revenue trend (`?range=daily\|weekly\|monthly`) |
| `GET` | `/api/dashboard/fast-moving-inventory` | Admin, Staff | Top-selling parts by quantity (`?limit=N`) |
| `GET` | `/api/dashboard/priority-alerts` | Admin, Staff | Active alerts by severity (`?limit=N`) |

---

### 📈 Financial Reports — `/api/reports/financial`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/api/reports/financial/daily` | Admin | Daily revenue breakdown |
| `GET` | `/api/reports/financial/monthly` | Admin | Monthly revenue breakdown |
| `GET` | `/api/reports/financial/yearly` | Admin | Yearly revenue breakdown |

---

### 👥 Customer Reports — `/api/reports/customers`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/api/reports/customers/top-spenders` | Admin, Staff | Highest-spending customers |
| `GET` | `/api/reports/customers/regular` | Admin, Staff | Regular customers (minimum purchase count threshold) |
| `GET` | `/api/reports/customers/pending-credit` | Admin, Staff | Customers with outstanding credit balances |

---

### 🔔 Notifications — `/api/notifications`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/notifications/low-stock` | Admin | Send low-stock alert emails |
| `POST` | `/api/notifications/credit-overdue` | Admin | Send credit-overdue alert emails |

---

### ⭐ Reviews & Part Requests

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/reviews` | Customer | Submit a service review |
| `GET` | `/api/reviews` | Admin, Staff | List all reviews |
| `POST` | `/api/part-requests` | Admin, Staff | Raise a part stock request |
| `GET` | `/api/part-requests` | Admin, Staff | List all part requests |
| `PATCH` | `/api/part-requests/{id}/status` | Admin | Update a part request status |

---

## 🧩 How the System Works

### Architecture Flow

```
Client (Frontend / Postman / Swagger)
        │
        ▼
  [API Layer]          → Controller receives HTTP request, delegates to service
        │
        ▼
  [Application Layer]  → Service contains business logic; FluentValidation validates DTOs
        │                 UnitOfWork wraps multi-step operations in a DB transaction
        ▼
  [Infrastructure]     → Repository queries PostgreSQL via EF Core
        │                 EmailService sends emails via MailKit
        ▼
  [PostgreSQL Database]
```

### Authentication & Registration Flow

```
Customer self-registers via POST /api/auth/register
        │
        ▼
RegistrationService (Application)
  ├── Check email not already taken (via IIdentityService)
  ├── Create ASP.NET Identity user (ApplicationUser)
  ├── Assign "Customer" role
  ├── Create Customer domain entity linked to ApplicationUserId
  └── Return JWT token → customer is immediately logged in

Staff/Admin login via POST /api/auth/login
  └── AuthService validates credentials → jwt.sign({ userId, role }) → returns token
```

### Credit Management Flow

```
Sale created with PaymentMethod = Credit
        │
GET /api/credits/{saleId}
  └─ Returns total, paid, remaining, overdue days, payment history

POST /api/credits/{saleId}/payments   { amount, paymentMethod, note }
  └─ UnitOfWork.BeginTransactionAsync()
  └─ CreditPayment entity created
  └─ Sale.CreditStatus updated (Partial / Paid)
  └─ CommitAsync()

POST /api/credits/{saleId}/send-reminder
  └─ EmailService sends HTML reminder to customer's email
```

### Revenue Trend (Dashboard)

The `GetRevenueTrendAsync` method on `DashboardService` supports three `RevenueTrendRange` values:

- **Daily** — last 7 days, one point per day
- **Weekly** — last 8 ISO weeks, one point per week
- **Monthly** — last 12 months, one point per month

Each point contains `label`, `date`, `revenue`, and `salesCount`.

### Failure Prediction Engine

`FailurePredictionService` iterates all five prediction rules against every vehicle belonging to a customer. Each rule implements `IFailurePredictionRule` and is independently registered in DI. Rules evaluate mileage and last-service data against configurable thresholds in `FailurePredictionRules`:

| Rule | Trigger |
|---|---|
| `BatteryRule` | Vehicle age / mileage threshold |
| `BrakePadRule` | Mileage since last brake service |
| `CoolantRule` | Mileage since last coolant flush |
| `TimingBeltRule` | Mileage-based replacement interval |
| `TransmissionFluidRule` | Mileage since last fluid change |

### Global Error Handling

`GlobalExceptionMiddleware` wraps every request. Any unhandled exception returns a consistent JSON shape:

```json
{
  "success": false,
  "message": "An unexpected error occurred.",
  "data": null
}
```

The client never receives a raw stack trace.

---

## 📸 Screenshots / Demo

> Screenshots and a live demo link will be added once the frontend is connected.

| View | Description |
|---|---|
| `[Dashboard]` | Stats summary with revenue trend chart and priority alerts |
| `[Swagger UI]` | Full API documentation at `/swagger` |
| `[Sales Invoice Email]` | HTML invoice sent to customer on sale creation |
| `[Credit Detail]` | Balance, payment history, and overdue tracking |

---

## 🚀 Future Improvements

- [ ] Refresh token support for longer sessions
- [ ] Role-based pagination defaults per endpoint
- [ ] Unit and integration tests (xUnit + Testcontainers for PostgreSQL)
- [ ] Connect a React / Next.js frontend
- [ ] File upload for part images and invoice PDFs
- [ ] Improve prediction engine with ML-based scoring (ML.NET)
- [ ] Audit log table for Admin actions
- [ ] Docker + Docker Compose setup for one-command local dev

---

## 🤝 Contributing

Contributions are welcome!

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature-name`
3. Follow the existing Clean Architecture layer structure
4. Ensure the project builds: `dotnet build`
5. Commit: `git commit -m "feat: add your feature"`
6. Push and open a Pull Request — one feature per PR

---

## 📄 License

This project is licensed under the **MIT License** — see the [LICENSE](./LICENSE) file for details.

---
