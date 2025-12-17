# Stock Inventory & Order Management System (SIOMS)

## Overview

Stock Inventory & Order Management System (SIOMS) is a comprehensive enterprise-grade inventory management solution built with **ASP.NET Core MVC**. The system provides complete control over inventory management, sales processing, purchase orders, supplier/customer management, and business analytics with a modern **cyber-neon themed interface**.

---

## Features

### 🔐 Authentication & Authorization

* Cookie-based authentication with role-based access control
* User registration and login with secure password handling
* Custom user management with role assignment
* Access denied handling with proper redirects

### 📦 Inventory Management

* Complete product catalog management with categories
* Real-time stock level tracking
* Automatic low stock alerts and notifications
* Stock movement history with audit trail
* Multi-location inventory tracking

### 💰 Sales & Order Processing

* Sales order creation with customer management
* Automated stock deduction on sales
* Customer relationship management
* Sales history and analytics
* Profit margin calculations

### 📋 Purchase & Supply Chain

* Purchase order management
* Supplier relationship tracking
* Automated stock updates on order receipt
* Order status tracking (Pending, Processing, Completed)
* Supplier performance analytics

### 📊 Business Intelligence

* Comprehensive dashboard with real-time KPIs
* Sales reports with date filtering
* Inventory valuation reports
* Profit & loss statements
* Stock movement analytics
* Top-selling products tracking

### ⚙️ System Features

* Responsive design with Bootstrap 5
* Modern cyber-neon UI theme
* Background services for automated tasks
* Daily stock reconciliation
* Export functionality for reports
* DataTables integration for enhanced tables

---

## Technology Stack

### Backend

* **Framework:** ASP.NET Core 6.0 MVC
* **Database:** SQL Server with Entity Framework Core
* **Authentication:** Cookie Authentication with custom roles
* **Architecture:** Repository Pattern, Service Layer, ViewModels

### Frontend

* **UI Framework:** Bootstrap 5.3.2
* **Icons:** FontAwesome 6.4.0
* **Charts:** Chart.js 3.9.1
* **Tables:** DataTables 1.13.6
* **Animations:** Animate.css 4.1.1
* **Styling:** Custom CSS with cyber-neon theme

### Development Tools

* **IDE:** Visual Studio 2022+
* **Package Manager:** NuGet
* **Version Control:** Git
* **Database Management:** SQL Server Management Studio

---

## Project Structure

```text
SIOMS/
├── Controllers/           # MVC Controllers
├── Data/                  # Database context and seeding
├── Models/                # Entity models
├── Repositories/          # Data access layer
├── Services/              # Business logic layer
├── ViewModels/            # Data transfer objects
├── Views/                 # Razor views
│   ├── Account/           # Authentication views
│   ├── Categories/        # Category management
│   ├── Customers/         # Customer management
│   ├── Home/              # Dashboard
│   ├── Products/          # Product management
│   ├── PurchaseOrders/    # Purchase order management
│   ├── Reports/           # Business reports
│   ├── SalesOrders/       # Sales order management
│   ├── Shared/            # Layout and partial views
│   ├── StockMovements/    # Stock tracking
│   └── Suppliers/         # Supplier management
├── wwwroot/               # Static files
│   ├── css/               # Stylesheets
│   └── js/                # JavaScript files
└── Program.cs             # Application entry point
```

---

## Installation & Setup

### Prerequisites

#### Development Environment

* Visual Studio 2022+ or Visual Studio Code
* .NET 6.0 SDK or later
* SQL Server 2019+ or SQL Server Express

#### Database Setup

* SQL Server instance running
* Appropriate permissions for database creation

### Installation Steps

#### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/sioms.git
cd sioms
```

#### 2. Configure Database Connection

Open **appsettings.json** and update the connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SIOMS;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

#### 3. Restore Dependencies

```bash
dotnet restore
```

#### 4. Database Migration

**Option 1: Entity Framework Core**

```bash
dotnet ef database update
```

**Option 2: SQL Script**

```sql
-- Execute update-database.sql in SQL Server Management Studio
```

#### 5. Seed Initial Data

The application automatically seeds:

* Default categories (Electronics, Clothing, etc.)
* Sample suppliers
* Initial products
* Demo user accounts

#### 6. Run the Application

```bash
dotnet run
```

Or run from Visual Studio using **F5 / Ctrl+F5**.

---

## Default Credentials

### Demo Accounts

**Administrator**

* Email: `admin@sioms.com`
* Password: `password123`

**Regular User**

* Email: `user@sioms.com`
* Password: `password123`

---

## Key Features Documentation

### Dashboard

* Total products, categories, suppliers, and customers
* Monthly sales tracking with interactive charts
* Low stock alerts with visual indicators
* Top-selling products with stock status
* Quick action buttons

### Product Management

* Complete CRUD operations
* Category and supplier assignment
* Stock level tracking
* Minimum stock level configuration
* Stock movement history
* Bulk operations support

### Sales Orders

* Customer sales management
* Automated stock deduction
* Profit calculation per sale
* Order status tracking
* Sales history and reporting

### Purchase Orders

* Supplier order management
* Automated stock updates
* Order tracking
* Supplier performance analysis
* Cost tracking

### Reports & Analytics

* Sales reports with date filters
* Inventory valuation & low stock reports
* Profit & loss statements
* Stock movement audit trail
* Export to CSV / Excel

---

## Background Services

* Daily stock reconciliation (2:00 AM)
* Low stock monitoring
* Performance optimization tasks

---

## Database Schema

### Core Tables

* Products
* Categories
* Suppliers
* Customers
* SalesOrders
* PurchaseOrders
* StockMovements
* LowStockAlerts
* Users

### Key Relationships

* Products → Categories, Suppliers
* SalesOrders → Customers, Products
* PurchaseOrders → Suppliers, Products
* StockMovements → Inventory changes
* LowStockAlerts → Automated triggers

---

## Configuration

### Environment Settings

* Development: `appsettings.Development.json`
* Production: `appsettings.Production.json`
* Environment variables supported

### Security Configuration

```csharp
options.LoginPath = "/Account/Login";
options.AccessDeniedPath = "/Account/AccessDenied";
options.ExpireTimeSpan = TimeSpan.FromDays(7);
options.SlidingExpiration = true;
```

### Logging

* Console logging (development)
* File logging (`sioms.log`) in production
* Event source & trace logging

---

## Customization

### UI Customization

```css
:root {
  --neon-blue: #00d9ff;
  --neon-purple: #bc13fe;
  --gradient-primary: linear-gradient(135deg, var(--neon-blue) 0%, var(--neon-purple) 100%);
}
```

* Edit `_Layout.cshtml` for layout changes
* Customize views in `Views/`

### Business Logic

* Extend services in `Services/`
* Add validation in `ViewModels/`
* Create new reports in `Views/Reports/`

### Database Extensions

```bash
dotnet ef migrations add NewFeature
dotnet ef database update
```

---

## Deployment

### Production Deployment

```bash
dotnet publish -c Release -o ./publish
```

* Configure IIS / Nginx / Apache
* Enable HTTPS
* Set environment variables

### Docker Deployment

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SIOMS.csproj", "."]
RUN dotnet restore "SIOMS.csproj"
COPY . .
RUN dotnet build "SIOMS.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SIOMS.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SIOMS.dll"]
```

---

## API Endpoints

### Authentication

* POST `/Account/Login`
* POST `/Account/Register`
* GET `/Account/Logout`
* GET `/Account/AccessDenied`

### Products

* GET `/Products`
* GET `/Products/{id}`
* POST `/Products/Create`
* PUT `/Products/Edit/{id}`
* DELETE `/Products/Delete/{id}`
* POST `/Products/UpdateStock/{id}`

### Sales Orders

* GET `/SalesOrders`
* POST `/SalesOrders/Create`
* GET `/SalesOrders/Details/{id}`
* POST `/SalesOrders/Complete/{id}`

### Reports

* GET `/Reports/Sales`
* GET `/Reports/Inventory`
* GET `/Reports/ProfitLoss`
* GET `/Reports/Export/{type}`

---

## Security Best Practices

### Implemented

* Cookie-based authentication
* Role-based authorization
* CSRF protection
* SQL injection prevention
* XSS protection
* HTTPS & HSTS

### Recommended

* Strong password policies
* Account lockout
* Two-factor authentication
* Audit logging
* Dependency updates

---

## Performance Optimization

* Database indexing
* Caching & compression
* Minified assets
* Lazy loading
* Background processing

---

## Monitoring & Metrics

* Application Insights
* Performance counters
* Health check endpoints

---

## Troubleshooting

### Common Issues

* Database connection errors
* Migration conflicts
* Authentication issues
* Performance bottlenecks

### Logging Locations

* Development: Console
* Production: `sioms.log`
* Windows Event Viewer

---

## Development Guidelines

* Use async/await
* Repository pattern
* ViewModels for views
* Proper error handling
* Unit & integration tests

---

## Version Control

* Feature branches
* Pull requests
* Semantic versioning
* CHANGELOG.md

---

## Support & Maintenance

### Maintenance

* Daily health checks
* Weekly performance review
* Monthly DB cleanup
* Quarterly security audit

### Backup Strategy

* Daily DB backups
* Off-site recovery plan

---

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests
4. Submit pull request

---

## License

**Proprietary Software** — All rights reserved.

---

## Contact

* **Email:** [support@sioms.com](mailto:support@sioms.com)
* **Documentation:** SIOMS Documentation Portal
* **Issue Tracker:** GitHub Issues

---

## Acknowledgments

* ASP.NET Core
* Bootstrap & FontAwesome
* Chart.js
* DataTables
